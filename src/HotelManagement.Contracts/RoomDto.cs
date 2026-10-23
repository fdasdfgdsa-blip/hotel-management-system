namespace HotelManagement.Contracts;

public record RoomDto(
    Guid Id,
    string Number,
    string CategoryName,
    int Floor,
    decimal PricePerNight,
    int Capacity,
    bool IsAvailable);
