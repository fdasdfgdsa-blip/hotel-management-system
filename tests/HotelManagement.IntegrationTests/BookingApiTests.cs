using System.Net;
using System.Net.Http.Json;
using HotelManagement.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HotelManagement.IntegrationTests;

/// <summary>
/// Проверка обработки запросов POST /api/bookings через HTTP.
/// </summary>
public class BookingApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string RoomId = "11111111-1111-1111-1111-111111111102";

    private readonly HttpClient _client;

    public BookingApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostBooking_DateWithTime_ReturnsBadRequestWithErrorCode()
    {
        string body = $$"""
            {
              "guestId": "22222222-2222-2222-2222-222222222222",
              "roomId": "{{RoomId}}",
              "checkInDate": "2026-12-10T00:00:00.000Z",
              "checkOutDate": "2026-12-12T00:00:00.000Z"
            }
            """;

        var response = await _client.PostAsync(
            "/api/bookings",
            new StringContent(body, System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal(BookingErrorCodes.InvalidRequest, error.ErrorCode);
    }

    [Fact]
    public async Task PostBooking_TextPlainContentType_ReturnsUnsupportedMediaType()
    {
        var response = await _client.PostAsync(
            "/api/bookings",
            new StringContent("{}", System.Text.Encoding.UTF8, "text/plain"));

        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
    }

    [Fact]
    public async Task PostBooking_SameRoomTwice_ReturnsCreatedThenConflict()
    {
        var request = new CreateBookingRequest(
            Guid.NewGuid(),
            Guid.Parse("11111111-1111-1111-1111-111111111101"),
            new DateOnly(2027, 1, 10),
            new DateOnly(2027, 1, 12),
            null);

        var first = await _client.PostAsJsonAsync("/api/bookings", request);
        var second = await _client.PostAsJsonAsync("/api/bookings", request);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.NotNull(first.Headers.Location);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);

        var error = await second.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal(RoomErrorCodes.RoomNotAvailable, error.ErrorCode);
    }

    [Fact]
    public async Task PostBooking_UnknownRoom_ReturnsNotFound()
    {
        var request = new CreateBookingRequest(
            Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2027, 1, 10), new DateOnly(2027, 1, 12), null);

        var response = await _client.PostAsJsonAsync("/api/bookings", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal(RoomErrorCodes.RoomNotFound, error.ErrorCode);
    }

    [Fact]
    public async Task GetBooking_UnknownId_ReturnsNotFoundWithErrorCode()
    {
        var response = await _client.GetAsync($"/api/bookings/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal(BookingErrorCodes.BookingNotFound, error.ErrorCode);
    }

    private sealed record ErrorResponse(string ErrorCode);
}
