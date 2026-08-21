using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniSiniestros.Entities;

namespace MiniSiniestros.Data.Repositories;

public class HistorialEstadoRepository : IHistorialEstadoRepository
{
    private readonly MiniSiniestrosDbContext _context;

    public HistorialEstadoRepository(MiniSiniestrosDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(HistorialEstado historial)
    {
        await _context.HistorialEstados.AddAsync(historial);
    }
}