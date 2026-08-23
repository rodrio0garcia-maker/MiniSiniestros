using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniSiniestros.Entities.Enums;

namespace MiniSiniestros.ViewModels;

public class SiniestroDetalleViewModel
{
    public int Id { get; set; }
    public string CuitEmpleador { get; set; } = string.Empty;
    public string CuilTrabajador { get; set; } = string.Empty;
    public EstadoSiniestro Estado { get; set; }
    public DateTime FechaOcurrencia { get; set; }
    public DateTime FechaAlta { get; set; }

    public List<PrestadorViewModel> Prestadores { get; set; } = new();
    public List<HistorialEstadoViewModel> Historial { get; set; } = new();
}

public class PrestadorViewModel
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Especialidad { get; set; } = string.Empty;
}

public class HistorialEstadoViewModel
{
    public EstadoSiniestro EstadoAnterior { get; set; }
    public EstadoSiniestro EstadoNuevo { get; set; }
    public DateTime FechaCambio { get; set; }
}