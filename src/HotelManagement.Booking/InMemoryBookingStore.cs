using System.Collections.Concurrent;

namespace HotelManagement.Booking;

/// <summary>
/// Хранилище бронирований в памяти приложения.
/// </summary>
public class InMemoryBookingStore
{
    private readonly ConcurrentDictionary<Guid, HotelBooking> _bookings = new();

    public int Count => _bookings.Count;

    public void Add(HotelBooking booking)
    {
        _bookings[booking.Id] = booking;
    }

    public HotelBooking? Find(Guid bookingId)
    {
        return _bookings.GetValueOrDefault(bookingId);
    }
}
