using System.Diagnostics.CodeAnalysis;

namespace HotelManagement.Contracts;

public record CreateBookingResult(
    BookingDto? Booking,
    string? ErrorCode)
{
    [MemberNotNullWhen(true, nameof(Booking))]
    public bool Success => Booking is not null;

    public static CreateBookingResult Ok(BookingDto booking) => new(booking, null);

    public static CreateBookingResult Fail(string errorCode) => new(null, errorCode);
}
