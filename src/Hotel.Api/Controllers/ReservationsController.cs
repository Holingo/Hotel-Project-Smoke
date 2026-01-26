using Hotel.Application.Dto;
using Hotel.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Api.Controllers;

[ApiController]
[Route("api/reservations")]
public class ReservationsController(IReservationsService service) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReservationDto>> GetById(int id, CancellationToken ct)
    => Ok(await service.GetByIdAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<ReservationDto>> Create([FromBody] CreateReservationDto dto, CancellationToken ct)
    {
        var created = await service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await service.CancelAsync(id, ct);
        return NoContent();
    }
}