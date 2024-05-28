using Cwiczenia9.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cwiczenia9.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripController:ControllerBase
{
    private readonly ApbdContext _context;

    public TripController(ApbdContext context)
    {
        _context = context;
    }
    
    
    [HttpGet]
    public async Task<IActionResult> GetTrips()
    {
        var trips = await _context.Trips.Select(e=>new
        {
            Name = e.Name,
            Countries = e.IdCountries.Select(c=> new
            {
                Name = c.Name
            })
        }).Skip(0).Take(1).ToListAsync();
        
        return Ok(trips);
    }
    
}