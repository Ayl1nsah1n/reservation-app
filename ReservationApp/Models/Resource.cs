namespace ReservationApp.Models
{
    public class Resource
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid VenueId { get; set; }

        public string Name { get; set; } = default!;
        public int Capacity { get; set; }
        public Venue Venue { get; set; } = null!;
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    }
}
