namespace ReservationApp.Dtos
{
    public record ReservationCreateDto(Guid ResourceId, DateTime StartTime, DateTime EndTime);
}
