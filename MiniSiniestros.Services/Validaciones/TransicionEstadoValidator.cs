using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniSiniestros.Entities.Enums;

namespace MiniSiniestros.Services.Validaciones;

public static class TransicionEstadoValidator
{
    // Diccionario: para cada estado ACTUAL, qué estados NUEVOS están permitidos
    private static readonly Dictionary<EstadoSiniestro, EstadoSiniestro[]> TransicionesPermitidas = new()
    {
        [EstadoSiniestro.Recibido] = new[] { EstadoSiniestro.EnAnalisis },
        [EstadoSiniestro.EnAnalisis] = new[] { EstadoSiniestro.Aprobado, EstadoSiniestro.Rechazado },
        [EstadoSiniestro.Aprobado] = new[] { EstadoSiniestro.Cerrado },
        [EstadoSiniestro.Rechazado] = new[] { EstadoSiniestro.Cerrado },
        [EstadoSiniestro.Cerrado] = Array.Empty<EstadoSiniestro>() // estado final
    };

    public static bool EsTransicionValida(EstadoSiniestro estadoActual, EstadoSiniestro nuevoEstado)
    {
        return TransicionesPermitidas.TryGetValue(estadoActual, out var permitidos)
            && permitidos.Contains(nuevoEstado);
    }
}