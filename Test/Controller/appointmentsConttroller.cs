using Microsoft.AspNetCore.Mvc;
using Test.Model.DTOs;
using Test.Service;

namespace Test.Controller;

[Route("api/[controller]")]
[ApiController]
public class appointmentsController : ControllerBase
{
    private readonly IDbService _dbService;

    public appointmentsController(IDbService dbService)
    {
        _dbService = dbService;
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetAppointmentById(int id)
    {
        try
        {
            var res = await _dbService.GetAppointmentById(id);
            return Ok(res);
        }
        catch (ServiceException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // 200 OK x 

    // 201 Created x 

    // 400 Bad Request x 

    // 404 Not Found x 

    // 409 Conflict x 


    [HttpPost]
    public async Task<IActionResult> AddAppointment(AddAppointmentDTO appointmentDto)
    {
        if (!appointmentDto.services.Any())
        {
            return BadRequest();
        }

        try
        {
            await _dbService.AddAppointmet(appointmentDto);
        }
        catch (ServiceException ex)
        {
            return BadRequest(ex.Message);
        }

        return Created();
    }
}