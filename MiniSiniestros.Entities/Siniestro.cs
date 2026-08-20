using MiniSiniestros.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSiniestros.Entities
{
    public class Siniestro
    {
        public int Id { get; set; }
        public string CuitEmpleador { get; set; } = string.Empty;
        public string CuilTrabajador { get; set; } = string.Empty;
        public EstadoSiniestro Estado { get; set; } = EstadoSiniestro.Recibido;
        public DateTime FechaOcurrencia { get; set; }
        public DateTime FechaAlta { get; set; } = DateTime.UtcNow;
        public ICollection<Prestador> Prestadores { get; set; } = new List<Prestador>();
        public ICollection<HistorialEstado> Historial { get; set; } = new List<HistorialEstado>();
    }
}
