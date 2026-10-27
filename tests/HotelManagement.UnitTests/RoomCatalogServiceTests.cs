using HotelManagement.Rooms;

namespace HotelManagement.UnitTests;

public class RoomCatalogServiceTests
{
    private static readonly Guid RoomId = Guid.Parse("33333333-3333-3333-3333-333333333102");

    private readonly InMemoryRoomStore _store;
    private readonly RoomCatalogService _service;

    public RoomCatalogServiceTests()
    {
        _store = new InMemoryRoomStore(new[] { CreateRoom(RoomId, "102", floor: 1) });
        _service = new RoomCatalogService(_store);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownRoom_ReturnsNull()
    {
        var room = await _service.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(room);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingRoom_ReturnsPricePerNight()
    {
        var room = await _service.GetByIdAsync(RoomId, CancellationToken.None);

        Assert.NotNull(room);
        Assert.Equal(3500m, room.PricePerNight);
    }

    [Fact]
    public async Task GetByIdAsync_RoomWithoutReservations_IsAvailable()
    {
        var room = await _service.GetByIdAsync(RoomId, CancellationToken.None);

        Assert.NotNull(room);
        Assert.True(room.IsAvailable);
    }

    [Fact]
    public async Task GetByIdAsync_RoomReservedAroundToday_IsNotAvailable()
    {
        // Резерв захватывает соседние дни, чтобы смена даты во время теста не влияла на результат
        var today = DateOnly.FromDateTime(DateTime.Today);
        _store.TryAddReservation(new RoomReservation(RoomId, Guid.NewGuid(), today.AddDays(-1), today.AddDays(2)));

        var room = await _service.GetByIdAsync(RoomId, CancellationToken.None);

        Assert.NotNull(room);
        Assert.False(room.IsAvailable);
    }

    [Fact]
    public async Task GetAllAsync_RoomsOnDifferentFloors_OrderedByFloorThenNumber()
    {
        var store = new InMemoryRoomStore(new[]
        {
            CreateRoom(Guid.NewGuid(), "1001", floor: 10),
            CreateRoom(Guid.NewGuid(), "201", floor: 2),
            CreateRoom(Guid.NewGuid(), "101", floor: 1)
        });
        var service = new RoomCatalogService(store);

        var rooms = await service.GetAllAsync(CancellationToken.None);

        Assert.Equal(new[] { "101", "201", "1001" }, rooms.Select(room => room.Number));
    }

    private static Room CreateRoom(Guid id, string number, int floor)
    {
        return new Room
        {
            Id = id,
            Number = number,
            CategoryName = "Стандарт (двухместный)",
            Floor = floor,
            PricePerNight = 3500m,
            Capacity = 2
        };
    }
}
