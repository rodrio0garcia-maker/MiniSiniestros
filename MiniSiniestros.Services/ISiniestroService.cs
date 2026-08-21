using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniSiniestros.Dto;
using MiniSiniestros.Entities.Enums;

namespace MiniSiniestros.Services;

public interface ISiniestroService
{
    Task<SiniestroDto> CrearAsync(SiniestroCreateDto dto);
    Task<SiniestroDto?> ObtenerPorIdAsync(int id);
    Task<List<SiniestroDto>> ListarAsync(EstadoSiniestro? estado, DateTime? desde, DateTime? hasta, int page, int pageSize);
    Task<int> ContarAsync(EstadoSiniestro? estado, DateTime? desde, DateTime? hasta);
    Task<bool> CambiarEstadoAsync(int id, EstadoSiniestro nuevoEstado);
    Task<bool> AsignarPrestadorAsync(int siniestroId, int prestadorId);
}
