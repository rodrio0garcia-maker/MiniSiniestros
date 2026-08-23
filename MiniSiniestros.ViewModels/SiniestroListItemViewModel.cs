using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniSiniestros.Entities.Enums;

namespace MiniSiniestros.ViewModels;

public class SiniestroListItemViewModel
{
    public int Id { get; set; }
    public string CuitEmpleador { get; set; } = string.Empty;
    public string CuilTrabajador { get; set; } = string.Empty;
    public EstadoSiniestro Estado { get; set; }
    public DateTime FechaOcurrencia { get; set; }
    public DateTime FechaAlta { get; set; }
}
