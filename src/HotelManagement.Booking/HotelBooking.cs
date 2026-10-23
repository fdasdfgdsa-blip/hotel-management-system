using HotelManagement.Contracts;

namespace HotelManagement.Booking;

public class HotelBooking
{
    public required Guid Id { get; init; }

    public required Guid GuestId { get; init; }

    public required Guid RoomId { get; init; }

    public DateOnly CheckInDate { get; init; }

    public DateOnly CheckOutDate { get; init; }

    public decimal TotalPrice { get; init; }

    public string? Comment { get; init; }

    public string Status { get; set; } = BookingStatuses.Created;

    public BookingDto ToDto()
    {
        return new BookingDto(Id, GuestId, RoomId, CheckInDate, CheckOutDate, TotalPrice, Status);
    }
}
