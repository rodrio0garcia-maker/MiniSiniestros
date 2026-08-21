using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniSiniestros.Entities;

namespace MiniSiniestros.Data.Repositories;

public interface IPrestadorRepository
{
    Task<Prestador?> GetByIdAsync(int id);
    Task<List<Prestador>> GetAllAsync();
}
