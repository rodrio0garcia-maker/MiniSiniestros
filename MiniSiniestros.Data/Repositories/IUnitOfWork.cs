using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace MiniSiniestros.Data.Repositories;

public interface IUnitOfWork
{
    ISiniestroRepository Siniestros { get; }
    IPrestadorRepository Prestadores { get; }
    IHistorialEstadoRepository HistorialEstados { get; }

    Task<int> SaveChangesAsync();
}