using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniSiniestros.Entities;
using MiniSiniestros.Entities.Enums;

namespace MiniSiniestros.Data.Repositories;

public interface ISiniestroRepository
{
    Task<Siniestro?> GetByIdAsync(int id);
    Task<List<Siniestro>> GetAllAsync();
    Task AddAsync(Siniestro siniestro);
    void Update(Siniestro siniestro);
    Task<List<Siniestro>> GetFiltradosAsync(int? numeroSiniestro, EstadoSiniestro? estado, DateTime? desde, DateTime? hasta,
        string? cuitEmpleador, string? cuilTrabajador, string? ordenarPor, int page, int pageSize);
    Task<int> ContarFiltradosAsync(int? numeroSiniestro, EstadoSiniestro? estado, DateTime? desde, DateTime? hasta,
        string? cuitEmpleador, string? cuilTrabajador);
}