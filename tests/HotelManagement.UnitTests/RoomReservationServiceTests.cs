using HotelManagement.Contracts;
using HotelManagement.Rooms;
using Microsoft.Extensions.Logging.Abstractions;

namespace HotelManagement.UnitTests;

public class RoomReservationServiceTests
{
    private static readonly Guid RoomId = Guid.NewGuid();

    private readonly RoomReservationService _service;

    public RoomReservationServiceTests()
    {
        var store = new InMemoryRoomStore(new[]
        {
            new Room
            {
                Id = RoomId,
                Number = "101",
                CategoryName = "Стандарт (одноместный)",
                Floor = 1,
                PricePerNight = 2500m,
                Capacity = 1
            }
        });

        _service = new RoomReservationService(store, NullLogger<RoomReservationService>.Instance);
    }

    [Fact]
    public async Task ReserveAsync_FreeRoom_ReturnsSuccess()
    {
        var request = CreateRequest(RoomId, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 5));

        var result = await _service.ReserveAsync(request, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Null(result.ErrorCode);
    }

    [Fact]
    public async Task ReserveAsync_OverlappingPeriod_ReturnsRoomNotAvailable()
    {
        await _service.ReserveAsync(
            CreateRequest(RoomId, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 5)),
            CancellationToken.None);

        var result = await _service.ReserveAsync(
            CreateRequest(RoomId, new DateOnly(2026, 11, 3), new DateOnly(2026, 11, 7)),
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(RoomErrorCodes.RoomNotAvailable, result.ErrorCode);
    }

    [Fact]
    public async Task ReserveAsync_CheckOutBeforeCheckIn_ReturnsInvalidBookingPeriod()
    {
        var request = CreateRequest(RoomId, new DateOnly(2026, 11, 5), new DateOnly(2026, 11, 1));

        var result = await _service.ReserveAsync(request, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(RoomErrorCodes.InvalidBookingPeriod, result.ErrorCode);
    }

    [Fact]
    public async Task ReserveAsync_UnknownRoom_ReturnsRoomNotFound()
    {
        var request = CreateRequest(Guid.NewGuid(), new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 5));

        var result = await _service.ReserveAsync(request, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(RoomErrorCodes.RoomNotFound, result.ErrorCode);
    }

    private static ReserveRoomRequest CreateRequest(Guid roomId, DateOnly checkIn, DateOnly checkOut)
    {
        return new ReserveRoomRequest(roomId, Guid.NewGuid(), checkIn, checkOut);
    }
}
