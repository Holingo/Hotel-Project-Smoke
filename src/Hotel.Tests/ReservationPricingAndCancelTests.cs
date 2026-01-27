using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers; // <-- DODANO
using FluentAssertions;
using Hotel.Application.Dto;
using Xunit;

namespace Hotel.Tests;

public class ReservationPricingAndCancelTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ReservationPricingAndCancelTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();

        // DODANO: Konfiguracja autoryzacji dla każdego testu w tej klasie
        var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbkBhZG1pbiIsImp0aSI6ImM4OTQzYWYyLTE0YjAtNDAyMC1iOTAzLWMwMzkzMTQ1NzkzYyIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwiZXhwIjoxNzY5NTQ2NDk4LCJpc3MiOiJIb3RlbEFwaSIsImF1ZCI6IkhvdGVsVXNlcnMifQ.hp2hHEoTG-nn9RqbulIWmMi55P5PuTy8j2h7PkPy8m4".Trim();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task Weekend_surcharge_is_applied_to_friday_and_saturday_nights()
    {
        // RoomId=2 => 320 per night
        // 2030-03-01 is Friday; stay 2 nights: Fri + Sat => 10% surcharge on both nights by default
        var dto = new
        {
            roomId = 2,
            guestId = 1,
            checkIn = "2030-03-01",
            checkOut = "2030-03-03", // 2 nights
            guestsCount = 2
        };

        var res = await _client.PostAsJsonAsync("/api/reservations", dto);
        res.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await res.Content.ReadFromJsonAsync<ReservationDto>();
        body.Should().NotBeNull();

        // Obliczenie: 2 noce * 320 * 1.10 (weekend surcharge) = 704.00
        body!.TotalPrice.Should().Be(704m);
    }

    [Fact]
    public async Task Cancel_is_idempotent_and_returns_no_content()
    {
        // Create a future reservation
        var create = new
        {
            roomId = 3,
            guestId = 1,
            checkIn = "2030-04-10",
            checkOut = "2030-04-12",
            guestsCount = 2
        };

        var post = await _client.PostAsJsonAsync("/api/reservations", create);
        post.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await post.Content.ReadFromJsonAsync<ReservationDto>();
        created.Should().NotBeNull();

        // Cancel once
        var del1 = await _client.DeleteAsync($"/api/reservations/{created!.Id}");
        del1.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Cancel again (idempotent - ponowne anulowanie nie powinno rzucać błędem)
        var del2 = await _client.DeleteAsync($"/api/reservations/{created.Id}");
        del2.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Fetch should show status = Canceled
        var get = await _client.GetFromJsonAsync<ReservationDto>($"/api/reservations/{created.Id}");
        get.Should().NotBeNull();
        get!.Status.Should().Be("Canceled");
    }
}