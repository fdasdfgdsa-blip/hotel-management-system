namespace HotelManagement.Rooms;

public class Room
{
    public required Guid Id { get; init; }

    public required string Number { get; init; }

    public required string CategoryName { get; init; }

    public int Floor { get; init; }

    public decimal PricePerNight { get; init; }

    public int Capacity { get; init; }
}
