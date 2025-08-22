using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservationApp.Data;
using ReservationApp.Dtos;
using ReservationApp.Enums;
using ReservationApp.Models;
using System.Globalization;
using System.Security.Claims;
using System.Data;

namespace ReservationApp.Controllers;

[ApiController]
[Route("api/reservations")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ReservationsController(AppDbContext db) => _db = db;

    Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    bool IsAdmin => User.IsInRole(nameof(Role.Admin));

    [HttpPost]
    public async Task<IActionResult> Create(ReservationCreateDto dto)
    {
        var resource = await _db.Resources
            .Include(r => r.Venue)
            .FirstOrDefaultAsync(r => r.Id == dto.ResourceId);
        if (resource is null) return BadRequest("Resource not found");

        if (dto.EndTime <= dto.StartTime) return BadRequest("EndTime must be after StartTime");
        if (dto.StartTime < DateTime.UtcNow) return BadRequest("StartTime cannot be in the past");

        if (!TryParseWorkingHours(resource.Venue.WorkingHours, out var open, out var close))
            return BadRequest("Venue WorkingHours format is invalid. Expected HH:mm-HH:mm");

        var startLocal = dto.StartTime.ToLocalTime();
        var endLocal = dto.EndTime.ToLocalTime();
        var startT = TimeOnly.FromDateTime(startLocal);
        var endT = TimeOnly.FromDateTime(endLocal);

        if (startT < open || endT > close)
            return BadRequest($"Outside venue working hours ({resource.Venue.WorkingHours})");


        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {

            var overlap = await _db.Reservations.AnyAsync(r =>
                r.ResourceId == resource.Id &&
                r.Status == ReservationStatus.Confirmed &&
                dto.StartTime < r.EndTime &&
                dto.EndTime > r.StartTime
            );

            if (overlap)
            {
                await tx.RollbackAsync();
                return Conflict(new { message = "Overlapping reservation exists" });
            }

            var user = await _db.Users.AsNoTracking().FirstAsync(u => u.Id == CurrentUserId);

            var res = new Reservation
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                FullName = user.FullName,
                ResourceId = resource.Id,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Status = ReservationStatus.Confirmed
            };

            _db.Reservations.Add(res);
            await _db.SaveChangesAsync();

            await tx.CommitAsync();

            return StatusCode(201, new
            {
                res.Id,
                res.FullName,
                ResourceName = resource.Name,
                res.StartTime,
                res.EndTime,
                res.Status
            });
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }


        [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOne(Guid id)
    {
        var r = await _db.Reservations.Include(x => x.Resource).FirstOrDefaultAsync(x => x.Id == id);
        if (r is null) return NotFound();
        if (r.UserId != CurrentUserId && !IsAdmin) return Forbid();
        return Ok(new { r.Id, r.FullName, Resource = r.Resource.Name, r.StartTime, r.EndTime, r.Status });
    }

    [HttpGet]
    public Task<object[]> Mine() =>
        _db.Reservations
          .Where(r => r.UserId == CurrentUserId)
          .OrderByDescending(r => r.StartTime)
          .Select(r => new { r.Id, r.FullName, Resource = r.Resource.Name, r.StartTime, r.EndTime, r.Status })
          .Cast<object>()
          .ToArrayAsync();

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var r = await _db.Reservations.FirstOrDefaultAsync(x => x.Id == id);
        if (r is null) return NotFound();
        if (r.UserId != CurrentUserId && !IsAdmin) return Forbid();

        r.Status = ReservationStatus.Cancelled;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static bool TryParseWorkingHours(string input, out TimeOnly open, out TimeOnly close)
    {
        open = default; close = default;
        var parts = input.Split('-', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2) return false;

        var ok1 = TimeOnly.TryParseExact(parts[0], "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out open);
        var ok2 = TimeOnly.TryParseExact(parts[1], "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out close);
        return ok1 && ok2 && open < close;
    }
}
