using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniSiniestros.Data.Repositories;
using MiniSiniestros.Dto;
using MiniSiniestros.Entities;
using MiniSiniestros.Entities.Enums;
using MiniSiniestros.Services.Validaciones;

namespace MiniSiniestros.Services;

public class SiniestroService : ISiniestroService
{
    private readonly IUnitOfWork _unitOfWork;

    public SiniestroService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SiniestroDto> CrearAsync(SiniestroCreateDto dto)
    {
        var siniestro = new Siniestro
        {
            CuitEmpleador = dto.CuitEmpleador,
            CuilTrabajador = dto.CuilTrabajador,
            FechaOcurrencia = dto.FechaOcurrencia,
            Estado = EstadoSiniestro.Recibido // todo siniestro nuevo arranca con este estado
        };

        await _unitOfWork.Siniestros.AddAsync(siniestro);
        await _unitOfWork.SaveChangesAsync();

        return MapearADto(siniestro);
    }

    public async Task<SiniestroDto?> ObtenerPorIdAsync(int id)
    {
        var siniestro = await _unitOfWork.Siniestros.GetByIdAsync(id);
        return siniestro is null ? null : MapearADto(siniestro);
    }

    public async Task<List<SiniestroDto>> ListarAsync(
        EstadoSiniestro? estado, DateTime? desde, DateTime? hasta, int page, int pageSize)
    {
        var siniestros = await _unitOfWork.Siniestros.GetFiltradosAsync(estado, desde, hasta, page, pageSize);
        return siniestros.Select(MapearADto).ToList();
    }

    public async Task<int> ContarAsync(EstadoSiniestro? estado, DateTime? desde, DateTime? hasta)
    {
        return await _unitOfWork.Siniestros.ContarFiltradosAsync(estado, desde, hasta);
    }

    public async Task<bool> CambiarEstadoAsync(int id, EstadoSiniestro nuevoEstado)
    {
        var siniestro = await _unitOfWork.Siniestros.GetByIdAsync(id);
        if (siniestro is null)
            return false;

        if (!TransicionEstadoValidator.EsTransicionValida(siniestro.Estado, nuevoEstado))
            return false;

        var estadoAnterior = siniestro.Estado; // lo guardamos para el historial

        siniestro.Estado = nuevoEstado;
        _unitOfWork.Siniestros.Update(siniestro);

        var historial = new HistorialEstado
        {
            SiniestroId = siniestro.Id,
            EstadoAnterior = estadoAnterior,
            EstadoNuevo = nuevoEstado
        };
        await _unitOfWork.HistorialEstados.AddAsync(historial);

        // Un solo SaveChangesAsync para los DOS cambios (el Update del
        // siniestro y el AddAsync del historial) - se guardan juntos,
        // como una unidad, gracias a que ambos repositorios comparten
        // el mismo DbContext por dentro del UnitOfWork
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> AsignarPrestadorAsync(int siniestroId, int prestadorId)
    {
        var siniestro = await _unitOfWork.Siniestros.GetByIdAsync(siniestroId);
        if (siniestro is null)
            return false;

        var prestador = await _unitOfWork.Prestadores.GetByIdAsync(prestadorId);
        if (prestador is null)
            return false;

        // Evitamos asignar el mismo prestador dos veces al mismo siniestro
        if (siniestro.Prestadores.Any(p => p.Id == prestadorId))
            return false;

        siniestro.Prestadores.Add(prestador);
        _unitOfWork.Siniestros.Update(siniestro);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private static SiniestroDto MapearADto(Siniestro siniestro)
    {
        return new SiniestroDto
        {
            Id = siniestro.Id,
            CuitEmpleador = siniestro.CuitEmpleador,
            CuilTrabajador = siniestro.CuilTrabajador,
            Estado = siniestro.Estado,
            FechaOcurrencia = siniestro.FechaOcurrencia,
            FechaAlta = siniestro.FechaAlta,
            Prestadores = siniestro.Prestadores.Select(p => new PrestadorDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Especialidad = p.Especialidad
            }).ToList()
        };
    }
}