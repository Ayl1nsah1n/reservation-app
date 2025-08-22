using ReservationApp.Enums;
using System.ComponentModel.DataAnnotations;

namespace ReservationApp.Models
{
    public class Reservation
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }
        [MaxLength(120)] public string FullName { get; set; } = default!;

        public Guid ResourceId { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public User User { get; set; } = null!;
        public Resource Resource { get; set; } = null!;

        public ReservationStatus Status { get; set; } = ReservationStatus.Confirmed;

    }
}
