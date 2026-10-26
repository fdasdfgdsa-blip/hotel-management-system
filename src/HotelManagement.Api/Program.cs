using HotelManagement.Api;
using HotelManagement.Booking;
using HotelManagement.Contracts;
using HotelManagement.Rooms;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(_ => new InMemoryRoomStore(SeedData.Rooms));
builder.Services.AddScoped<IRoomCatalogService, RoomCatalogService>();
builder.Services.AddScoped<IRoomReservationService, RoomReservationService>();
builder.Services.AddSingleton<InMemoryBookingStore>();
builder.Services.AddScoped<BookingService>();

// Ошибки чтения тела запроса обрабатываются одинаково во всех окружениях.
// Исключения журналируются в обработчике ниже, поэтому стандартная запись
// ExceptionHandlerMiddleware отключена в appsettings.json.
builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);

var app = builder.Build();

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    var logger = context.RequestServices
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("HotelManagement.Api");

    if (exception is BadHttpRequestException badRequest)
    {
        logger.LogWarning(
            "Bad request {Method} {Path}: {Reason}",
            context.Request.Method,
            context.Request.Path,
            badRequest.InnerException?.Message ?? badRequest.Message);

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new { errorCode = BookingErrorCodes.InvalidRequest });
        return;
    }

    logger.LogError(exception, "Unhandled error {Method} {Path}", context.Request.Method, context.Request.Path);

    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await context.Response.WriteAsJsonAsync(new { errorCode = "INTERNAL_ERROR" });
}));

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
