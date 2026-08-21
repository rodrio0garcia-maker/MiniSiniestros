using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace MiniSiniestros.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly MiniSiniestrosDbContext _context;

    public ISiniestroRepository Siniestros { get; }
    public IPrestadorRepository Prestadores { get; }
    public IHistorialEstadoRepository HistorialEstados { get; }

    public UnitOfWork(MiniSiniestrosDbContext context)
    {
        _context = context;

        // Los 3 repositorios se crean acá, y los 3 comparten la MISMA
        // instancia de _context - esto es lo que hace que
        // SaveChangesAsync() los persista a todos juntos
        Siniestros = new SiniestroRepository(context);
        Prestadores = new PrestadorRepository(context);
        HistorialEstados = new HistorialEstadoRepository(context);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}