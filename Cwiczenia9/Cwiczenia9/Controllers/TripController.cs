using Cwiczenia9.Data;
using Cwiczenia9.DTOs;
using Cwiczenia9.Models;
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
    public async Task<IActionResult> GetTrips(int pageNum = 1, int pageSize = 10)
    {
        var trips = await _context.Trips.Select(e=>new
        {
            Name = e.Name,
            Description = e.Description,
            DateFrom = e.DateFrom,
            DateTo = e.DateTo,
            MaxPeople = e.MaxPeople,
            Countries = e.IdCountries.Select(c=> new
            {
                Name = c.Name
            }),
            Clients = e.ClientTrips.Select(cl=>new
            {
                FirstName = cl.IdClientNavigation.FirstName,
                LastName = cl.IdClientNavigation.LastName
            })
        }).Skip((pageNum-1)*pageSize).Take(pageSize).ToListAsync();
        
        var response = new
        {
            pageNum = pageNum,
            pageSize = pageSize,
            allPages = 20,
            trips = trips
        };
        return Ok(response);
    }
    
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DelateClient(int id)
    {
        var doesHaveTrip = _context.ClientTrips.Any(e => e.IdClient == id);
        if (doesHaveTrip)
        {
            return BadRequest();
        }
        else
        {
            var client = _context.Clients.FirstOrDefault(c=>c.IdClient==id);
            _context.Clients.Remove(client);
            _context.SaveChanges();
            return Ok();
        }
    }

    [HttpPost("\"/api/trips/{idTrip:int}/clients\"")]
    public async Task<IActionResult> AddClientToTrip(int idTrip, [FromBody] AddClientToTrip clientDto)
    {
        var client = _context.Clients.FirstOrDefault(e=>e.Pesel==clientDto.Pesel);
        if (client == null)
        {
            var newClient = new Client
            {
                FirstName = clientDto.FirstName,
                LastName = clientDto.LastName,
                Email = clientDto.Email,
                Telephone = clientDto.Telephone,
                Pesel = clientDto.Pesel
            };
            _context.Clients.Add(newClient);
            _context.SaveChanges();
            client = newClient;
        }

        // var existingAssignment = _context.Client_Trip.FirstOrDefault(ct => ct.IdClient == existingClient.IdClient && ct.IdTrip == idTrip);
        // if (existingAssignment != null)
        // {
        //     return BadRequest("Client is already assigned to this trip.");
        // }
        
        var doesClientHaveTrip =
            _context.ClientTrips.FirstOrDefault(ct => ct.IdClientNavigation.Pesel == clientDto.Pesel && ct.IdTrip==idTrip);
        
        if (doesClientHaveTrip != null)
        {
            return BadRequest();
        }
        
        var trip = _context.Trips.FirstOrDefault(t => t.IdTrip == idTrip && t.DateFrom>DateTime.Now);
        if (trip == null)
        {
            return BadRequest();
        }

        var clientToTrip = new ClientTrip
        {
            IdClient = client.IdClient,
            IdTrip = idTrip,
            RegisteredAt = DateTime.Now,
        };
        _context.ClientTrips.Add(clientToTrip);
        _context.SaveChanges();
        
        return Created();
    }

}