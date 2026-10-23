using HotelManagement.Booking;
using HotelManagement.Contracts;
using Microsoft.Extensions.Logging.Abstractions;

namespace HotelManagement.UnitTests;

public class BookingServiceTests
{
    private static readonly Guid RoomId = Guid.NewGuid();

    [Fact]
    public async Task CreateAsync_ThreeNights_CalculatesTotalPriceByNights()
    {
        var store = new InMemoryBookingStore();
        var service = CreateService(store, ReserveRoomResult.Ok());
        var request = new CreateBookingRequest(
            Guid.NewGuid(), RoomId, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 4), null);

        var result = await service.CreateAsync(request, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(9000m, result.Booking!.TotalPrice);
        Assert.Equal(1, store.Count);
    }

    [Fact]
    public async Task CreateAsync_ReservationRejected_DoesNotSaveBooking()
    {
        var store = new InMemoryBookingStore();
        var service = CreateService(store, ReserveRoomResult.Fail(RoomErrorCodes.RoomNotAvailable));
        var request = new CreateBookingRequest(
            Guid.NewGuid(), RoomId, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 4), null);

        var result = await service.CreateAsync(request, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(RoomErrorCodes.RoomNotAvailable, result.ErrorCode);
        Assert.Equal(0, store.Count);
    }

    private static BookingService CreateService(InMemoryBookingStore store, ReserveRoomResult reserveResult)
    {
        return new BookingService(
            new StubRoomCatalog(),
            new StubRoomReservation(reserveResult),
            store,
            NullLogger<BookingService>.Instance);
    }

    private sealed class StubRoomCatalog : IRoomCatalogService
    {
        private readonly RoomDto _room = new(RoomId, "101", "Стандарт", 1, 3000m, 1, true);

        public Task<IReadOnlyList<RoomDto>> GetAllAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<RoomDto>>(new[] { _room });
        }

        public Task<RoomDto?> GetByIdAsync(Guid roomId, CancellationToken cancellationToken)
        {
            return Task.FromResult(roomId == RoomId ? _room : null);
        }
    }

    private sealed class StubRoomReservation : IRoomReservationService
    {
        private readonly ReserveRoomResult _result;

        public StubRoomReservation(ReserveRoomResult result)
        {
            _result = result;
        }

        public Task<ReserveRoomResult> ReserveAsync(
            ReserveRoomRequest request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_result);
        }
    }
}
