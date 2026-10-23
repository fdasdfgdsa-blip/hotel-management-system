using HotelManagement.Contracts;

namespace HotelManagement.Rooms;

public class RoomCatalogService : IRoomCatalogService
{
    private readonly InMemoryRoomStore _store;

    public RoomCatalogService(InMemoryRoomStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<RoomDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        IReadOnlyList<RoomDto> rooms = _store.GetAll()
            .OrderBy(room => room.Number)
            .Select(room => ToDto(room, today))
            .ToList();

        return Task.FromResult(rooms);
    }

    public Task<RoomDto?> GetByIdAsync(Guid roomId, CancellationToken cancellationToken)
    {
        var room = _store.Find(roomId);
        var today = DateOnly.FromDateTime(DateTime.Today);

        return Task.FromResult(room is null ? null : ToDto(room, today));
    }

    private RoomDto ToDto(Room room, DateOnly today)
    {
        return new RoomDto(
            room.Id,
            room.Number,
            room.CategoryName,
            room.Floor,
            room.PricePerNight,
            room.Capacity,
            _store.IsFree(room.Id, today));
    }
}
