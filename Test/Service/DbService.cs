using System.Data.Common;
using Microsoft.Data.SqlClient;
using Test.Model.DTOs;

namespace Test.Service;

public class DbService : IDbService
{
    private readonly string _connectionString;


    public DbService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default") ?? string.Empty;
    }


    public async Task<bool> AppointmentExists(int id)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"select 1 from Appointment where appointment_id =@appoitment_id";
        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@appoitment_id", id);
        var result = await cmd.ExecuteScalarAsync();

        return result != null;
    }

    public async Task<AppointmentHistoryDTO> GetAppointmentById(int id)
    {
        if (!await AppointmentExists(id))
        {
            throw new ServiceException("Appointment not found", 404);
        }

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"select a.date,
       p.first_name,
       p.last_name,
       p.date_of_birth,
       d.doctor_id,
       d.PWZ,
       s.name,
       a_s.service_fee
from Appointment a
         join Patient p on a.patient_id = p.patient_id
         join Doctor d on a.doctor_id = d.doctor_id
         join Appointment_Service a_s on a.appointment_id = a_s.appointment_id
         join Service s on a_s.service_id = s.service_id
where a.appointment_id = @appoitment_id";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@appoitment_id", id));
        using var reader = await command.ExecuteReaderAsync();

        AppointmentHistoryDTO appointmentDTO = null;
        while (await reader.ReadAsync())
        {
            if (appointmentDTO == null)
            {
                appointmentDTO = new AppointmentHistoryDTO()
                {
                    date = reader.GetDateTime(0),
                    patient = new PatientDTO()
                    {
                        firstName = reader.GetString(1),
                        lastName = reader.GetString(2),
                        dateOfBirth = reader.GetDateTime(3),
                    },
                    doctor = new DoctorDTO()
                    {
                        doctorId = reader.GetInt32(4),
                        pwz = reader.GetString(5),
                    },
                    appoitmnetServices = new List<AppointmentServicesDTO>()
                };
            }

            appointmentDTO.appoitmnetServices.Add(new AppointmentServicesDTO()
            {
                name = reader.GetString(6),
                servicefee = reader.GetDecimal(7),
            });
        }

        return appointmentDTO;
    }


    public async Task AddAppointmet(AddAppointmentDTO appointmentDto)
    {
       
        if (await AppointmentExists(appointmentDto.appoitmentid))
        {
            throw new ServiceException("Appointment  already exists", 409);
        }

        await using var connection = new SqlConnection(_connectionString);
        await using SqlCommand command = connection.CreateCommand();
        command.Connection = connection;
        await connection.OpenAsync();

        DbTransaction transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;


        try
        {
            
            command.CommandText = @"select 1 from Appointment where appointment_id = @appointment_id";
            command.Parameters.AddWithValue("@appointment_id", appointmentDto.appoitmentid);
            if (await command.ExecuteScalarAsync() is  null)
            {
                throw new ServiceException("appointment  exists", 404);
                
            }
            
            command.Parameters.Clear();
            command.CommandText = @"select 1 from Patient where patient_id = @patient_id";
            command.Parameters.AddWithValue("@patient_id", appointmentDto.patientId);
            if (await command.ExecuteScalarAsync() is null)
            {
                throw new ServiceException("Patient does not exists", 404);
            }


            command.Parameters.Clear();

            command.CommandText = @"select doctor_id from Doctor where PWZ = @pws";
            command.Parameters.AddWithValue("@pws", appointmentDto.pwz);
            var dID = await command.ExecuteScalarAsync();
            if (dID is null)
            {
                throw new ServiceException("Doctor does not exists", 404);
            }

            command.Parameters.Clear();
            command.CommandText = @"insert into Appointment (appointment_id, patient_id, doctor_id, date) 
                                    values (@id, @pid, @did, @date)";
            command.Parameters.AddWithValue("@id", appointmentDto.appoitmentid);
            command.Parameters.AddWithValue("@pid", appointmentDto.patientId);
            command.Parameters.AddWithValue("@did", dID);
            command.Parameters.AddWithValue("@date", DateTime.Now);
            await command.ExecuteNonQueryAsync();

            foreach (var s in appointmentDto.services)
            {
                command.Parameters.Clear();
                command.CommandText = @"select service_id from Service where name = @service_id";
                command.Parameters.AddWithValue("@service_id", s.serviceName);
                var sId = await command.ExecuteScalarAsync();
                if (sId is null)
                {
                    throw new ServiceException("Service does not exist", 404);
                }


                command.Parameters.Clear();
                command.CommandText = @"insert into  Appointment_Service(appointment_id, service_id, service_fee) 
                                        values (@aid, @sid, @fee)";
                command.Parameters.AddWithValue("@aid", appointmentDto.appoitmentid);
                command.Parameters.AddWithValue("@sid", sId);
                command.Parameters.AddWithValue("@fee", s.ServiceFee);

                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch (ServiceException ex)
        {
            await transaction.RollbackAsync();
            throw ex;
        }
    }
}