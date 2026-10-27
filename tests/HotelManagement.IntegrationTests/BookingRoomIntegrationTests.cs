using HotelManagement.Booking;
using HotelManagement.Contracts;
using HotelManagement.Rooms;
using Microsoft.Extensions.Logging.Abstractions;

namespace HotelManagement.IntegrationTests;

/// <summary>
/// Проверка взаимодействия BookingModule и RoomModule
/// через контракт IRoomReservationService.
/// </summary>
public class BookingRoomIntegrationTests
{
    private static readonly Guid RoomId = Guid.NewGuid();

    private readonly InMemoryBookingStore _bookingStore = new();
    private readonly BookingService _bookingService;

    public BookingRoomIntegrationTests()
    {
        var roomStore = new InMemoryRoomStore(new[]
        {
            new Room
            {
                Id = RoomId,
                Number = "201",
                CategoryName = "Люкс",
                Floor = 2,
                PricePerNight = 7000m,
                Capacity = 2
            }
        });

        _bookingService = new BookingService(
            new RoomCatalogService(roomStore),
            new RoomReservationService(roomStore, NullLogger<RoomReservationService>.Instance),
            _bookingStore,
            NullLogger<BookingService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_FreeRoom_CreatesBookingAndReservesRoom()
    {
        var request = CreateRequest(new DateOnly(2026, 12, 10), new DateOnly(2026, 12, 12));

        var result = await _bookingService.CreateAsync(request, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(14000m, result.Booking.TotalPrice);
        Assert.Equal(BookingStatuses.Created, result.Booking.Status);
        Assert.Equal(1, _bookingStore.Count);
    }

    [Fact]
    public async Task CreateAsync_RoomAlreadyBooked_ReturnsRoomNotAvailable()
    {
        await _bookingService.CreateAsync(
            CreateRequest(new DateOnly(2026, 12, 10), new DateOnly(2026, 12, 12)),
            CancellationToken.None);

        var result = await _bookingService.CreateAsync(
            CreateRequest(new DateOnly(2026, 12, 11), new DateOnly(2026, 12, 13)),
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(RoomErrorCodes.RoomNotAvailable, result.ErrorCode);
        Assert.Equal(1, _bookingStore.Count);
    }

    [Fact]
    public async Task CreateAsync_SameCheckInAndCheckOut_ReturnsInvalidBookingPeriod()
    {
        var request = CreateRequest(new DateOnly(2026, 12, 10), new DateOnly(2026, 12, 10));

        var result = await _bookingService.CreateAsync(request, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(RoomErrorCodes.InvalidBookingPeriod, result.ErrorCode);
        Assert.Equal(0, _bookingStore.Count);
    }

    private static CreateBookingRequest CreateRequest(DateOnly checkIn, DateOnly checkOut)
    {
        return new CreateBookingRequest(Guid.NewGuid(), RoomId, checkIn, checkOut, null);
    }
}
