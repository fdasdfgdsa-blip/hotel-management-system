namespace HotelManagement.Contracts;

public record ReserveRoomRequest(
    Guid RoomId,
    Guid BookingId,
    DateOnly CheckInDate,
    DateOnly CheckOutDate);
