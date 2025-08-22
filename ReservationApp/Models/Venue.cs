namespace ReservationApp.Models
{
    public class Venue
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = default!;
        public string WorkingHours { get; set; } = null!;
        public ICollection<Resource> Resources { get; set; } = new List<Resource>();
    }
}
