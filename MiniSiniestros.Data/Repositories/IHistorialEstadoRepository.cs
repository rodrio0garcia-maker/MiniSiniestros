using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniSiniestros.Entities;

namespace MiniSiniestros.Data.Repositories;

public interface IHistorialEstadoRepository
{
    Task AddAsync(HistorialEstado historial);
}