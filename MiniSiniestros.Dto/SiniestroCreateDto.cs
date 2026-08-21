using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSiniestros.Dto;

public class SiniestroCreateDto
{
    public string CuitEmpleador { get; set; } = string.Empty;
    public string CuilTrabajador { get; set; } = string.Empty;
    public DateTime FechaOcurrencia { get; set; }
}
