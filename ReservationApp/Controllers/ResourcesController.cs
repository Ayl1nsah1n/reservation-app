using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservationApp.Data;
using ReservationApp.Dtos;
using ReservationApp.Enums;

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

    [Authorize(Roles = "Admin")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var r = await _db.Resources
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new { x.Id, x.VenueId, x.Name, x.Capacity })
            .FirstOrDefaultAsync();

        return r is null ? NotFound() : Ok(r);
    }
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] ResourceUpdateDto dto)
    {
        var res = await _db.Resources.FirstOrDefaultAsync(x => x.Id == id);
        if (res is null) return NotFound();

        // Venue değişiyorsa, var mı kontrol et
        if (dto.VenueId.HasValue)
        {
            var venueExists = await _db.Venues.AnyAsync(v => v.Id == dto.VenueId.Value);
            if (!venueExists) return BadRequest("Venue not found.");
            res.VenueId = dto.VenueId.Value;
        }

        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            // Aynı şubede aynı isimden kaçın (opsiyonel ama faydalı)
            var duplicate = await _db.Resources.AnyAsync(x =>
                x.Id != id &&
                x.VenueId == (dto.VenueId ?? res.VenueId) &&
                x.Name == dto.Name);
            if (duplicate) return Conflict("A resource with this name already exists under the selected venue.");
            res.Name = dto.Name.Trim();
        }

        if (dto.Capacity.HasValue)
        {
            if (dto.Capacity.Value < 1) return BadRequest("Capacity must be at least 1.");
            res.Capacity = dto.Capacity.Value;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var res = await _db.Resources.FirstOrDefaultAsync(x => x.Id == id);
        if (res is null) return NotFound();

        // Gelecekteki veya devam eden onaylı rezervasyonlar var mı?
        var hasActiveReservations = await _db.Reservations.AnyAsync(r =>
            r.ResourceId == id &&
            r.Status == ReservationStatus.Confirmed &&
            r.EndTime > DateTime.UtcNow);

        if (hasActiveReservations)
            return Conflict("This resource has active reservations and cannot be deleted.");

        _db.Resources.Remove(res);
        await _db.SaveChangesAsync();
        return NoContent();
    }

}
