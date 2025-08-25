using System.ComponentModel.DataAnnotations;

namespace ReservationApp.Dtos
{
    public record ResourceCreateDto(Guid VenueId, string Name, int Capacity);
    public class ResourceUpdateDto
    {
        public Guid? VenueId { get; set; }                 
        [StringLength(100)]
        public string? Name { get; set; }                  
        [Range(1, 500)]
        public int? Capacity { get; set; }                 
    }

}
