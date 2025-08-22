using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservationApp.Data;
using ReservationApp.Dtos;

namespace ReservationApp.Controllers;

[ApiController]
[Route("api/venues")]
public class VenuesController : ControllerBase
{
    private readonly AppDbContext _db;
    public VenuesController(AppDbContext db) => _db = db;

    // GET /api/venues
    [HttpGet]
    public Task<List<object>> GetVenues() =>
        _db.Venues.Select(v => new { v.Id, v.Name, v.WorkingHours }).Cast<object>().ToListAsync();

    // GET /api/venues/{id}/resources
    [HttpGet("{id:guid}/resources")]
    public async Task<IActionResult> GetResources(Guid id)
    {
        var exists = await _db.Venues.AnyAsync(v => v.Id == id);
        if (!exists) return NotFound();

        var list = await _db.Resources
            .Where(r => r.VenueId == id)
            .Select(r => new { r.Id, r.Name, r.Capacity })
            .ToListAsync();

        return Ok(list);
    }
}
