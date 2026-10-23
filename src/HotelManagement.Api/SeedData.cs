using HotelManagement.Rooms;

namespace HotelManagement.Api;

public static class SeedData
{
    public static IReadOnlyList<Room> Rooms { get; } = new List<Room>
    {
        new()
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111101"),
            Number = "101",
            CategoryName = "Стандарт (одноместный)",
            Floor = 1,
            PricePerNight = 2500m,
            Capacity = 1
        },
        new()
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111102"),
            Number = "102",
            CategoryName = "Стандарт (двухместный)",
            Floor = 1,
            PricePerNight = 3500m,
            Capacity = 2
        },
        new()
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111201"),
            Number = "201",
            CategoryName = "Люкс",
            Floor = 2,
            PricePerNight = 7000m,
            Capacity = 2
        }
    };
}
