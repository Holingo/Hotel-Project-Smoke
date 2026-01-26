using Hotel.Application.Dto;
using Hotel.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/rooms")]
public class RoomsController(IRoomsService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RoomDto>>> Get(
        [FromQuery] int? minCapacity,
        [FromQuery] bool? onlyActive,
        [FromQuery] string? type,
        [FromQuery] string? sortBy,
        [FromQuery(Name = "sort")] string? sort,
        [FromQuery] string? sortDir,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var finalSortBy = sortBy ?? sort;

        var (items, total) = await service.GetAsync(
            minCapacity,
            onlyActive,
            type,
            finalSortBy,
            sortBy,
            page,
            pageSize,
            ct);
        
        Response.Headers["X-Total-Count"] = total.ToString();
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RoomDto>> GetById(int id, CancellationToken ct)
        => Ok(await service.GetByIdAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<RoomDto>> Create([FromBody] CreateRoomDto dto, CancellationToken ct)
    {
        var created = await service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
    
    [HttpPut("{id:int}")]
    public async Task<ActionResult<RoomDto>> Update(int id, [FromBody] UpdateRoomDto dto, CancellationToken ct)
        => Ok(await service.UpdateAsync(id, dto, ct));


    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await service.DeactivateAsync(id, ct);
        return NoContent();
    }
}