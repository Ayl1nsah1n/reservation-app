using ReservationApp.Enums;
using System.ComponentModel.DataAnnotations;

namespace ReservationApp.Models;


public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(120)]
    public string FullName { get; set; } = default!;

    [MaxLength(120)]
    public string Email { get; set; } = default!;

    public string PasswordHash { get; set; } = default!;

    public Role Role { get; set; } = Role.User;

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
