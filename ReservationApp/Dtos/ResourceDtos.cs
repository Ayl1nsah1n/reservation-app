namespace ReservationApp.Dtos
{
    public record ResourceCreateDto(Guid VenueId, string Name, int Capacity);
}
