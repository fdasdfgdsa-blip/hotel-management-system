namespace HotelManagement.Contracts;

public record ReserveRoomResult(
    bool Success,
    string? ErrorCode)
{
    public static ReserveRoomResult Ok() => new(true, null);

    public static ReserveRoomResult Fail(string errorCode) => new(false, errorCode);
}
