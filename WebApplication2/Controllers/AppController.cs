using Microsoft.AspNetCore.Mvc;
using WebApplication2.Models.DTOs;
using WebApplication2.Services;

namespace WebApplication2.Controllers;

[ApiController]
[Route("api/clients")]
public class AppController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly Service _service;

    public AppController(IConfiguration configuration)
    {
        _configuration = configuration;
        _service = new Service(configuration);
    }

    [HttpGet("{clientID}")]
    public async Task<ActionResult<ClientDTO>> GetClient(int clientID)
    {
        var client = await _service.GetClient(clientID);

        if (client == null)
        {
            return NotFound();
        }

        return Ok(client);
    }

    [HttpPost]

    public async Task<IActionResult> CreateClientAndRental([FromBody] ClientAndRentalDTO clientAndRentalDTO)
    {
        var (clientId, error) = await _service.CreateClientAndRental(clientAndRentalDTO);
        if (!string.IsNullOrEmpty(error))
        {
            return BadRequest(error);
        }

        return Ok(new {ClientId = clientId});
    }
}