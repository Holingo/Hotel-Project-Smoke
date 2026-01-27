using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers; // Potrzebne do AuthenticationHeaderValue
using FluentAssertions;
using Hotel.Application.Dto;
using Xunit;

namespace Hotel.Tests;

public class ReservationAndAvailabilityTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ReservationAndAvailabilityTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();

        // DODANO: Autoryzacja tokenem JWT dla tej klasy testowej
        var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbkBhZG1pbiIsImp0aSI6ImM4OTQzYWYyLTE0YjAtNDAyMC1iOTAzLWMwMzkzMTQ1NzkzYyIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwiZXhwIjoxNzY5NTQ2NDk4LCJpc3MiOiJIb3RlbEFwaSIsImF1ZCI6IkhvdGVsVXNlcnMifQ.hp2hHEoTG-nn9RqbulIWmMi55P5PuTy8j2h7PkPy8m4".Trim();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task Creating_overlapping_reservation_returns_409()
    {
        // Pokój 1 jest zajęty w seedzie od 2030-01-10 do 2030-01-12
        var dto = new
        {
            roomId = 1,
            guestId = 1,
            checkIn = "2030-01-11",
            checkOut = "2030-01-13",
            guestsCount = 2
        };

        var res = await _client.PostAsJsonAsync("/api/reservations", dto);

        res.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Invalid_dates_return_400()
    {
        var dto = new
        {
            roomId = 2,
            guestId = 1,
            checkIn = "2030-02-10",
            checkOut = "2030-02-10", // Data wyjazdu taka sama jak przyjazdu jest niepoprawna
            guestsCount = 1
        };

        var res = await _client.PostAsJsonAsync("/api/reservations", dto);

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Too_many_guests_for_capacity_returns_400()
    {
        // RoomId=1 ma capacity = 2 w seedzie
        var dto = new
        {
            roomId = 1,
            guestId = 1,
            checkIn = "2030-02-01",
            checkOut = "2030-02-03",
            guestsCount = 3 // Przekroczenie limitu osób
        };

        var res = await _client.PostAsJsonAsync("/api/reservations", dto);

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Total_price_is_calculated_correctly()
    {
        var dto = new
        {
            roomId = 2, // 320 za noc
            guestId = 1,
            checkIn = "2030-03-04", // Wtorek
            checkOut = "2030-03-07", // Piątek (3 noce: Wt, Śr, Czw)
            guestsCount = 2
        };

        var res = await _client.PostAsJsonAsync("/api/reservations", dto);
        res.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await res.Content.ReadFromJsonAsync<ReservationDto>();
        body.Should().NotBeNull();

        // Obliczenie: 3 noce * 320 = 960
        body!.TotalPrice.Should().Be(960m);
        body.Status.Should().Be("Active");
    }

    [Fact]
    public async Task Availability_does_not_return_booked_room()
    {
        // Sprawdzamy termin, w którym pokój 1 jest już zajęty (według Twoich logów z seedowania)
        var url = "/api/availability?checkIn=2030-01-10&checkOut=2030-01-12&minCapacity=1";
        var rooms = await _client.GetFromJsonAsync<List<RoomDto>>(url);

        rooms.Should().NotBeNull();
        rooms!.Any(r => r.Id == 1).Should().BeFalse("Room 1 is seeded as booked for this period");
    }
}