using Test.Model.DTOs;

namespace Test.Service;

public interface IDbService
{

    Task<AppointmentHistoryDTO> GetAppointmentById(int id );
    
    
    Task<bool> AppointmentExists(int id);
    
    
    Task AddAppointmet(AddAppointmentDTO appointmentDto);


}