using MiniSiniestros.Dto;
using MiniSiniestros.Entities;
using MiniSiniestros.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSiniestros.Services;

public interface ISiniestroService
{
    Task<SiniestroDto> CrearAsync(SiniestroCreateDto dto);
    Task<SiniestroDto?> ObtenerPorIdAsync(int id);
    Task<List<SiniestroDto>> ListarAsync( int? numeroSiniestro, EstadoSiniestro? estado, DateTime? desde, DateTime? hasta,
        string? cuitEmpleador, string? cuilTrabajador, string? ordenarPor, int page, int pageSize);
    Task<int> ContarAsync(int? numeroSiniestro, EstadoSiniestro? estado, DateTime? desde, DateTime? hasta,
        string? cuitEmpleador, string? cuilTrabajador);
    Task<bool> CambiarEstadoAsync(int id, EstadoSiniestro nuevoEstado);
    Task<bool> AsignarPrestadorAsync(int siniestroId, int prestadorId);
    Task<Siniestro?> ObtenerEntidadPorIdAsync(int id);
}
