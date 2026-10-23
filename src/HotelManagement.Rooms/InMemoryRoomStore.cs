namespace HotelManagement.Rooms;

/// <summary>
/// Хранилище номеров и резервов в памяти приложения.
/// </summary>
public class InMemoryRoomStore
{
    private readonly Dictionary<Guid, Room> _rooms;
    private readonly List<RoomReservation> _reservations = new();
    private readonly object _sync = new();

    public InMemoryRoomStore(IEnumerable<Room> rooms)
    {
        _rooms = rooms.ToDictionary(room => room.Id);
    }

    public IReadOnlyList<Room> GetAll()
    {
        return _rooms.Values.ToList();
    }

    public Room? Find(Guid roomId)
    {
        return _rooms.GetValueOrDefault(roomId);
    }

    public bool IsFree(Guid roomId, DateOnly date)
    {
        lock (_sync)
        {
            return !_reservations.Any(r =>
                r.RoomId == roomId && r.CheckInDate <= date && date < r.CheckOutDate);
        }
    }

    public bool TryAddReservation(RoomReservation reservation)
    {
        lock (_sync)
        {
            var isBusy = _reservations.Any(r =>
                r.RoomId == reservation.RoomId &&
                r.Overlaps(reservation.CheckInDate, reservation.CheckOutDate));

            if (isBusy)
            {
                return false;
            }

            _reservations.Add(reservation);
            return true;
        }
    }
}
