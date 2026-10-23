using HotelManagement.Contracts;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Rooms;

public class RoomReservationService : IRoomReservationService
{
    private readonly InMemoryRoomStore _store;
    private readonly ILogger<RoomReservationService> _logger;

    public RoomReservationService(InMemoryRoomStore store, ILogger<RoomReservationService> logger)
    {
        _store = store;
        _logger = logger;
    }

    public Task<ReserveRoomResult> ReserveAsync(
        ReserveRoomRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogInformation(
            "Reserve room {RoomId} for booking {BookingId}, period {CheckInDate} - {CheckOutDate}",
            request.RoomId, request.BookingId, request.CheckInDate, request.CheckOutDate);

        if (request.CheckOutDate < request.CheckInDate)
        {
            return Task.FromResult(Reject(request, RoomErrorCodes.InvalidBookingPeriod));
        }

        if (_store.Find(request.RoomId) is null)
        {
            return Task.FromResult(Reject(request, RoomErrorCodes.RoomNotFound));
        }

        var reservation = new RoomReservation(
            request.RoomId,
            request.BookingId,
            request.CheckInDate,
            request.CheckOutDate);

        if (!_store.TryAddReservation(reservation))
        {
            return Task.FromResult(Reject(request, RoomErrorCodes.RoomNotAvailable));
        }

        _logger.LogInformation(
            "Room {RoomId} reserved for booking {BookingId}",
            request.RoomId, request.BookingId);

        return Task.FromResult(ReserveRoomResult.Ok());
    }

    private ReserveRoomResult Reject(ReserveRoomRequest request, string errorCode)
    {
        _logger.LogWarning(
            "Reservation rejected: {ErrorCode}, room {RoomId}, booking {BookingId}, period {CheckInDate} - {CheckOutDate}",
            errorCode, request.RoomId, request.BookingId, request.CheckInDate, request.CheckOutDate);

        return ReserveRoomResult.Fail(errorCode);
    }
}
