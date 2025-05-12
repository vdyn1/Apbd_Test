namespace Test.Model.DTOs;

public class AppointmentHistoryDTO
{
    public DateTime date { get; set; }
    public PatientDTO patient { get; set; }
    public DoctorDTO doctor { get; set; }
    public List<AppointmentServicesDTO> appoitmnetServices { get; set; } = [];
}

public class PatientDTO
{
    public string firstName { get; set; }
    public string lastName { get; set; }
    public DateTime dateOfBirth { get; set; }
}

public class DoctorDTO
{
    public int doctorId { get; set; }
    public string pwz { get; set; }
}

public class AppointmentServicesDTO
{
    public string name { get; set; }
    public decimal servicefee { get; set; }
}