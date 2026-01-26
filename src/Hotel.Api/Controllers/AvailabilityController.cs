using Hotel.Application.Dto;
using Hotel.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Api.Controllers;

[ApiController]
[Route("api/availability")]
public class AvailabilityController(IAvailabilityService service) : Controller
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RoomDto>>> Get(
        [FromQuery] DateOnly checkIn,
        [FromQuery] DateOnly checkOut,
        [FromQuery] int minCapacity = 0,
        [FromQuery] string? type = null,
        CancellationToken ct = default)
    {
        var rooms = await service.GetAvailableRoomsAsync(checkIn, checkOut, minCapacity, type, ct);
        return Ok(rooms);
    }
}