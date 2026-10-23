namespace HotelManagement.Contracts;

public record CreateBookingRequest(
    Guid GuestId,
    Guid RoomId,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    string? Comment);
