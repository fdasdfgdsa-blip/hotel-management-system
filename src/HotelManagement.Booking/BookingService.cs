using HotelManagement.Contracts;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Booking;

public class BookingService
{
    private readonly IRoomCatalogService _roomCatalog;
    private readonly IRoomReservationService _roomReservation;
    private readonly InMemoryBookingStore _store;
    private readonly ILogger<BookingService> _logger;

    public BookingService(
        IRoomCatalogService roomCatalog,
        IRoomReservationService roomReservation,
        InMemoryBookingStore store,
        ILogger<BookingService> logger)
    {
        _roomCatalog = roomCatalog;
        _roomReservation = roomReservation;
        _store = store;
        _logger = logger;
    }

    public async Task<CreateBookingResult> CreateAsync(
        CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        var bookingId = Guid.NewGuid();

        _logger.LogInformation(
            "Create booking {BookingId}: guest {GuestId}, room {RoomId}, period {CheckInDate} - {CheckOutDate}",
            bookingId, request.GuestId, request.RoomId, request.CheckInDate, request.CheckOutDate);

        string? missingField = FindMissingField(request);
        if (missingField is not null)
        {
            _logger.LogWarning(
                "Booking {BookingId} not created: required field {Field} is empty",
                bookingId, missingField);

            return CreateBookingResult.Fail(BookingErrorCodes.InvalidRequest);
        }

        var room = await _roomCatalog.GetByIdAsync(request.RoomId, cancellationToken);
        if (room is null)
        {
            _logger.LogWarning(
                "Booking {BookingId} not created: room {RoomId} not found",
                bookingId, request.RoomId);

            return CreateBookingResult.Fail(RoomErrorCodes.RoomNotFound);
        }

        var reserveRequest = new ReserveRoomRequest(
            request.RoomId,
            bookingId,
            request.CheckInDate,
            request.CheckOutDate);

        var reserveResult = await _roomReservation.ReserveAsync(reserveRequest, cancellationToken);
        if (!reserveResult.Success)
        {
            string errorCode = reserveResult.ErrorCode
                ?? throw new InvalidOperationException("Reservation failed without error code");

            _logger.LogWarning(
                "Booking {BookingId} not created: {ErrorCode}",
                bookingId, errorCode);

            return CreateBookingResult.Fail(errorCode);
        }

        int nights = request.CheckOutDate.DayNumber - request.CheckInDate.DayNumber;

        var booking = new HotelBooking
        {
            Id = bookingId,
            GuestId = request.GuestId,
            RoomId = request.RoomId,
            CheckInDate = request.CheckInDate,
            CheckOutDate = request.CheckOutDate,
            TotalPrice = nights * room.PricePerNight,
            Comment = request.Comment
        };

        _store.Add(booking);

        _logger.LogInformation(
            "Booking {BookingId} created: {Nights} nights, total price {TotalPrice}",
            bookingId, nights, booking.TotalPrice);

        return CreateBookingResult.Ok(booking.ToDto());
    }

    public Task<BookingDto?> GetByIdAsync(Guid bookingId, CancellationToken cancellationToken)
    {
        return Task.FromResult(_store.Find(bookingId)?.ToDto());
    }

    private static string? FindMissingField(CreateBookingRequest request)
    {
        if (request.GuestId == Guid.Empty)
        {
            return nameof(request.GuestId);
        }

        if (request.RoomId == Guid.Empty)
        {
            return nameof(request.RoomId);
        }

        if (request.CheckInDate == default)
        {
            return nameof(request.CheckInDate);
        }

        if (request.CheckOutDate == default)
        {
            return nameof(request.CheckOutDate);
        }

        return null;
    }
}
