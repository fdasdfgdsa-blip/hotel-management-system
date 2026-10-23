namespace HotelManagement.Contracts;

/// <summary>
/// Контракт резервирования номера (RoomReservationContract v1).
/// Инициатор — BookingModule, получатель — RoomModule.
/// </summary>
public interface IRoomReservationService
{
    Task<ReserveRoomResult> ReserveAsync(
        ReserveRoomRequest request,
        CancellationToken cancellationToken);
}
