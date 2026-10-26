using HotelManagement.Api;
using HotelManagement.Booking;
using HotelManagement.Contracts;
using HotelManagement.Rooms;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(_ => new InMemoryRoomStore(SeedData.Rooms));
builder.Services.AddScoped<IRoomCatalogService, RoomCatalogService>();
builder.Services.AddScoped<IRoomReservationService, RoomReservationService>();
builder.Services.AddSingleton<InMemoryBookingStore>();
builder.Services.AddScoped<BookingService>();

var app = builder.Build();

app.MapGet("/api/rooms", async (IRoomCatalogService catalog, CancellationToken cancellationToken) =>
    Results.Ok(await catalog.GetAllAsync(cancellationToken)));

app.MapPost("/api/bookings", async (
    CreateBookingRequest request,
    BookingService bookings,
    CancellationToken cancellationToken) =>
{
    var result = await bookings.CreateAsync(request, cancellationToken);
    if (result.Success)
    {
        return Results.Created($"/api/bookings/{result.Booking!.Id}", result.Booking);
    }

    var error = new { errorCode = result.ErrorCode };

    return result.ErrorCode switch
    {
        RoomErrorCodes.RoomNotFound => Results.NotFound(error),
        RoomErrorCodes.RoomNotAvailable => Results.Conflict(error),
        _ => Results.BadRequest(error)
    };
});

app.MapGet("/api/bookings/{id:guid}", async (
    Guid id,
    BookingService bookings,
    CancellationToken cancellationToken) =>
{
    var booking = await bookings.GetByIdAsync(id, cancellationToken);
    return booking is null ? Results.NotFound() : Results.Ok(booking);
});

app.Run();

// Нужен для WebApplicationFactory<Program> в интеграционных тестах
public partial class Program
{
}
