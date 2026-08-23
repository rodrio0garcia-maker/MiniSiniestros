using AutoMapper;
using MiniSiniestros.Data.Repositories;
using MiniSiniestros.Dto;
using MiniSiniestros.Entities;
using MiniSiniestros.Entities.Enums;
using MiniSiniestros.Services.Validaciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSiniestros.Services;

public class SiniestroService : ISiniestroService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SiniestroService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
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

        return _mapper.Map<SiniestroDto>(siniestro);
    }

    public async Task<SiniestroDto?> ObtenerPorIdAsync(int id)
    {
        var siniestro = await _unitOfWork.Siniestros.GetByIdAsync(id);
        return siniestro is null ? null : _mapper.Map<SiniestroDto>(siniestro);
    }

    public async Task<Siniestro?> ObtenerEntidadPorIdAsync(int id)
    {
        return await _unitOfWork.Siniestros.GetByIdAsync(id);
    }

    public async Task<List<SiniestroDto>> ListarAsync(EstadoSiniestro? estado, DateTime? desde, DateTime? hasta,
        string? cuitEmpleador, string? cuilTrabajador, string? ordenarPor, int page, int pageSize)
    {
        var siniestros = await _unitOfWork.Siniestros.GetFiltradosAsync(
            estado, desde, hasta, cuitEmpleador, cuilTrabajador, ordenarPor, page, pageSize);
        return _mapper.Map<List<SiniestroDto>>(siniestros);
    }  

    public async Task<int> ContarAsync(EstadoSiniestro? estado, DateTime? desde, DateTime? hasta,
        string? cuitEmpleador, string? cuilTrabajador)
    {
        return await _unitOfWork.Siniestros.ContarFiltradosAsync(
            estado, desde, hasta, cuitEmpleador, cuilTrabajador);
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
}