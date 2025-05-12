namespace Test.Model.DTOs;

public class AddAppointmentDTO
{
    public int appoitmentid { get; set; }
    public int patientId { get; set; }
    public string pwz { get; set; }
    public List<ServiceDTO> services { get; set; }
}

public class ServiceDTO
{
    public string serviceName { get; set; }
    public decimal ServiceFee { get; set; }
}