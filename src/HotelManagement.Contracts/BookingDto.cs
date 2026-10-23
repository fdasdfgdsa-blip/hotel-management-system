namespace HotelManagement.Contracts;

public record BookingDto(
    Guid Id,
    Guid GuestId,
    Guid RoomId,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    decimal TotalPrice,
    string Status);
