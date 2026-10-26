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
        var body = $$"""
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
        Assert.Equal(BookingErrorCodes.InvalidRequest, error!.ErrorCode);
    }

    private sealed record ErrorResponse(string ErrorCode);
}
