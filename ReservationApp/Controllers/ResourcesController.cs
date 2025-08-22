using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservationApp.Data;
using ReservationApp.Dtos;

namespace ReservationApp.Controllers;

[ApiController]
[Route("api/resources")]
public class ResourcesController : ControllerBase
{
    private readonly AppDbContext _db;
    public ResourcesController(AppDbContext db) => _db = db;

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(ResourceCreateDto dto)
    {
        var venueExists = await _db.Venues.AnyAsync(v => v.Id == dto.VenueId);
        if (!venueExists) return BadRequest("Venue not found");

        var entity = new Models.Resource
        {
            Id = Guid.NewGuid(),
            VenueId = dto.VenueId,
            Name = dto.Name,
            Capacity = dto.Capacity
        };

        _db.Resources.Add(entity);
        await _db.SaveChangesAsync();
        return StatusCode(201, new { entity.Id, entity.Name, entity.Capacity });
    }
}
