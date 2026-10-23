namespace HotelManagement.Rooms;

public record RoomReservation(
    Guid RoomId,
    Guid BookingId,
    DateOnly CheckInDate,
    DateOnly CheckOutDate)
{
    public bool Overlaps(DateOnly checkInDate, DateOnly checkOutDate)
    {
        return CheckInDate < checkOutDate && checkInDate < CheckOutDate;
    }
}
