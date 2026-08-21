using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniSiniestros.Entities.Enums;

namespace MiniSiniestros.Dto;

public class CambiarEstadoDto
{
    public EstadoSiniestro NuevoEstado { get; set; }
}
