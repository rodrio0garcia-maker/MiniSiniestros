using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniSiniestros.Entities;

namespace MiniSiniestros.Data.Repositories;

public class PrestadorRepository : IPrestadorRepository
{
    private readonly MiniSiniestrosDbContext _context;

    public PrestadorRepository(MiniSiniestrosDbContext context)
    {
        _context = context;
    }

    public async Task<Prestador?> GetByIdAsync(int id)
    {
        // FindAsync porque busca primero en el cache y luego en la base de datos, es más eficiente que FirstOrDefaultAsync
        return await _context.Prestadores.FindAsync(id);
    }

    public async Task<List<Prestador>> GetAllAsync()
    {
        return await _context.Prestadores.ToListAsync();
    }
}
