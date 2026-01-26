using Hotel.Application.Dto;
using Hotel.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/guests")]
public class GuestsController(IGuestsService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GuestDto>>> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var (items, total) = await service.GetAsync(page, pageSize, ct);
        Response.Headers["X-Total-Count"] = total.ToString();
        return Ok(items);
    }
    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<GuestDto>> GetById(int id, CancellationToken ct)
        => Ok(await service.GetByIdAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<GuestDto>> Create([FromBody] CreateGuestDto dto, CancellationToken ct)
    {
        var created = await service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
    
    [HttpPut("{id:int}")]
    public async Task<ActionResult<GuestDto>> Update(int id, [FromBody] UpdateGuestDto dto, CancellationToken ct)
    => Ok(await service.UpdateAsync(id, dto, ct));
}