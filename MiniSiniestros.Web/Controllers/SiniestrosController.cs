using Microsoft.AspNetCore.Mvc;
using MiniSiniestros.Entities.Enums;
using MiniSiniestros.Services;
using MiniSiniestros.ViewModels;

namespace MiniSiniestros.Web.Controllers;

public class SiniestrosController : Controller
{
    private readonly ISiniestroService _siniestroService;

    public SiniestrosController(ISiniestroService siniestroService)
    {
        _siniestroService = siniestroService;
    }

    public async Task<IActionResult> Index(
    EstadoSiniestro? estado,
    DateTime? desde,
    DateTime? hasta,
    string? cuitEmpleador,
    string? cuilTrabajador,
    string? ordenarPor,
    int page = 1)
    {
        const int pageSize = 10;

        var siniestrosDto = await _siniestroService.ListarAsync(
            estado, desde, hasta, cuitEmpleador, cuilTrabajador, ordenarPor, page, pageSize);
        var total = await _siniestroService.ContarAsync(
            estado, desde, hasta, cuitEmpleador, cuilTrabajador);

        var viewModel = new SiniestroListaViewModel
        {
            Items = siniestrosDto.Select(s => new SiniestroListItemViewModel
            {
                Id = s.Id,
                CuitEmpleador = s.CuitEmpleador,
                CuilTrabajador = s.CuilTrabajador,
                Estado = s.Estado,
                FechaOcurrencia = s.FechaOcurrencia,
                FechaAlta = s.FechaAlta
            }).ToList(),
            Total = total,
            Page = page,
            PageSize = pageSize,
            Estado = estado,
            Desde = desde,
            Hasta = hasta,
            CuitEmpleador = cuitEmpleador,
            CuilTrabajador = cuilTrabajador,
            OrdenarPor = ordenarPor
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Detalle(int id)
    {
        var siniestro = await _siniestroService.ObtenerEntidadPorIdAsync(id);
        if (siniestro is null)
            return NotFound();

        var viewModel = new SiniestroDetalleViewModel
        {
            Id = siniestro.Id,
            CuitEmpleador = siniestro.CuitEmpleador,
            CuilTrabajador = siniestro.CuilTrabajador,
            Estado = siniestro.Estado,
            FechaOcurrencia = siniestro.FechaOcurrencia,
            FechaAlta = siniestro.FechaAlta,
            Prestadores = siniestro.Prestadores.Select(p => new PrestadorViewModel
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Especialidad = p.Especialidad
            }).ToList(),
            Historial = siniestro.Historial
                .OrderByDescending(h => h.FechaCambio)
                .Select(h => new HistorialEstadoViewModel
                {
                    EstadoAnterior = h.EstadoAnterior,
                    EstadoNuevo = h.EstadoNuevo,
                    FechaCambio = h.FechaCambio
                }).ToList()
        };

        return View(viewModel);
    }
}