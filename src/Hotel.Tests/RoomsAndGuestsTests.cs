using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using FluentAssertions;
using Hotel.Application.Dto;
using Xunit;

namespace Hotel.Tests;

public class RoomsAndGuestsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RoomsAndGuestsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Rooms_list_supports_paging_and_total_count_header()
    {
        var res = await _client.GetAsync("/api/rooms?page=1&pageSize=2");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Headers.Should().ContainKey("X-Total-Count");

        var rooms = await res.Content.ReadFromJsonAsync<List<RoomDto>>();
        rooms.Should().NotBeNull();
        rooms!.Count.Should().BeLessThanOrEqualTo(2);
    }

    [Fact]
    public async Task Rooms_filter_by_type_works()
    {
        var rooms = await _client.GetFromJsonAsync<List<RoomDto>>("/api/rooms?type=Deluxe&page=1&pageSize=50");

        rooms.Should().NotBeNull();
        rooms!.All(r => r.Type == "Deluxe").Should().BeTrue();
    }

    [Fact]
    public async Task Guests_crud_create_then_get_by_id()
    {
        var create = new
        {
            firstName = "Test",
            lastName = "User",
            email = $"test.user.{Guid.NewGuid():N}@example.com",
            phone = "123123123",
            identityDocument = "ID123"
        };

        var post = await _client.PostAsJsonAsync("/api/guests", create);
        post.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await post.Content.ReadFromJsonAsync<GuestDto>();
        created.Should().NotBeNull();
        created!.Id.Should().BeGreaterThan(0);

        var fetched = await _client.GetFromJsonAsync<GuestDto>($"/api/guests/{created.Id}");
        fetched.Should().NotBeNull();
        fetched!.Email.Should().Be(create.email);
    }
}