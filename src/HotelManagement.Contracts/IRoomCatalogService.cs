namespace HotelManagement.Contracts;

/// <summary>
/// Получение данных о номерах из RoomModule.
/// </summary>
public interface IRoomCatalogService
{
    Task<IReadOnlyList<RoomDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<RoomDto?> GetByIdAsync(Guid roomId, CancellationToken cancellationToken);
}
