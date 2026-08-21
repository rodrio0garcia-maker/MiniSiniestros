using Microsoft.AspNetCore.Mvc;
using MiniSiniestros.Dto;
using MiniSiniestros.Entities.Enums;
using MiniSiniestros.Services;

namespace MiniSiniestros.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SiniestrosController : ControllerBase
{
    private readonly ISiniestroService _siniestroService;

    public SiniestrosController(ISiniestroService siniestroService)
    {
        _siniestroService = siniestroService;
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] SiniestroCreateDto dto)
    {
        var creado = await _siniestroService.CrearAsync(dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var siniestro = await _siniestroService.ObtenerPorIdAsync(id);
        if (siniestro is null)
            return NotFound();

        return Ok(siniestro);
    }

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] EstadoSiniestro? estado,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var items = await _siniestroService.ListarAsync(estado, desde, hasta, page, pageSize);
        var total = await _siniestroService.ContarAsync(estado, desde, hasta);

        return Ok(new
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        });
    }

    [HttpPatch("{id}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoDto dto)
    {
        var resultado = await _siniestroService.CambiarEstadoAsync(id, dto.NuevoEstado);
        if (!resultado)
            return BadRequest(new { Mensaje = "Transición de estado inválida, o el siniestro no existe." });

        return NoContent();
    }

    [HttpPost("{id}/prestadores")]
    public async Task<IActionResult> AsignarPrestador(int id, [FromBody] int prestadorId)
    {
        var resultado = await _siniestroService.AsignarPrestadorAsync(id, prestadorId);
        if (!resultado)
            return BadRequest(new { Mensaje = "No se pudo asignar el prestador (no existe, o ya estaba asignado)." });

        return NoContent();
    }
}