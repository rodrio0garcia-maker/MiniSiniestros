using MiniSiniestros.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSiniestros.Entities
{
    public class HistorialEstado
    {
        public int Id { get; set; }
        public int SiniestroId { get; set; }
        public Siniestro Siniestro { get; set; } = null!;
        public EstadoSiniestro EstadoAnterior { get; set; }
        public EstadoSiniestro EstadoNuevo { get; set; }
        public DateTime FechaCambio { get; set; } = DateTime.UtcNow;
    }
}
