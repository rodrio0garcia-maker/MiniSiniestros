using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniSiniestros.Entities;
using MiniSiniestros.Entities.Enums;

namespace MiniSiniestros.Data.Repositories;

public class SiniestroRepository : ISiniestroRepository
{
    private readonly MiniSiniestrosDbContext _context;

    public SiniestroRepository(MiniSiniestrosDbContext context)
    {
        _context = context;
    }

    public async Task<Siniestro?> GetByIdAsync(int id)
    {
        // Trae las relaciones cargadas junto con el siniestro
        // FirstOrDefaultAsync para poder utlizar el Include y traer las relaciones, ya que FindAsync no permite incluir relaciones.
        return await _context.Siniestros
            .Include(s => s.Prestadores)
            .Include(s => s.Historial)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<Siniestro>> GetAllAsync()
    {
        return await _context.Siniestros.ToListAsync();
    }

    public async Task AddAsync(Siniestro siniestro)
    {
        await _context.Siniestros.AddAsync(siniestro);
    }

    public void Update(Siniestro siniestro)
    {
        // NO es async: No toca la base hasta que se llama a SaveChangesAsync()
        _context.Siniestros.Update(siniestro);
    }

    public async Task<List<Siniestro>> GetFiltradosAsync(int? numeroSiniestro, EstadoSiniestro? estado, DateTime? desde, DateTime? hasta,
        string? cuitEmpleador, string? cuilTrabajador, string? ordenarPor, int page, int pageSize)
    {
        // AsQueryable permite construir la query de manera dinámica, agregando filtros según los parámetros que se pasen.
        // La consulta final se ejecuta cuando se llama a ToListAsync(), que es cuando EF Core genera el SQL y lo ejecuta en la base de datos.
        var query = _context.Siniestros.AsQueryable();

        if (numeroSiniestro.HasValue)
            query = query.Where(s => s.Id == numeroSiniestro.Value);

        if (estado.HasValue)
            query = query.Where(s => s.Estado == estado.Value);

        if (desde.HasValue)
            query = query.Where(s => s.FechaOcurrencia >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(s => s.FechaOcurrencia <= hasta.Value);

        if (!string.IsNullOrWhiteSpace(cuitEmpleador))
            query = query.Where(s => s.CuitEmpleador.Contains(cuitEmpleador));

        if (!string.IsNullOrWhiteSpace(cuilTrabajador))
            query = query.Where(s => s.CuilTrabajador.Contains(cuilTrabajador));

        // Por default ordena por FechaAlta descendente, si piden "estado" ordena por Estado en su lugar
        query = ordenarPor == "estado"
            ? query.OrderBy(s => s.Estado)
            : query.OrderByDescending(s => s.FechaAlta);

        // Skip y Take permiten paginar los resultados. Skip salta los registros de las páginas anteriores, y Take toma solo la cantidad de registros de la página actual.
        return await query
            .OrderByDescending(s => s.FechaAlta)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> ContarFiltradosAsync(int? numeroSiniestro, EstadoSiniestro? estado, DateTime? desde, DateTime? hasta,
        string? cuitEmpleador, string? cuilTrabajador)
    {
        // SIN Skip/Take - necesitamos el total real, no solo lo que entra en una página
        var query = _context.Siniestros.AsQueryable();

        if (numeroSiniestro.HasValue)
            query = query.Where(s => s.Id == numeroSiniestro.Value);

        if (estado.HasValue)
            query = query.Where(s => s.Estado == estado.Value);

        if (desde.HasValue)
            query = query.Where(s => s.FechaOcurrencia >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(s => s.FechaOcurrencia <= hasta.Value);

        if (!string.IsNullOrWhiteSpace(cuitEmpleador))
            query = query.Where(s => s.CuitEmpleador.Contains(cuitEmpleador));

        if (!string.IsNullOrWhiteSpace(cuilTrabajador))
            query = query.Where(s => s.CuilTrabajador.Contains(cuilTrabajador));

        return await query.CountAsync();
    }
}