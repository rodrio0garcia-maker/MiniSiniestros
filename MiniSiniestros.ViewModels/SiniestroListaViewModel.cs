using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniSiniestros.Entities.Enums;

namespace MiniSiniestros.ViewModels;

public class SiniestroListaViewModel
{
    public List<SiniestroListItemViewModel> Items { get; set; } = new();

    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPaginas => (int)Math.Ceiling((double)Total / PageSize);
    public EstadoSiniestro? Estado { get; set; }
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public string? CuitEmpleador { get; set; }
    public string? CuilTrabajador { get; set; }
    public string? OrdenarPor { get; set; }
    public int? NumeroSiniestro { get; set; }
}