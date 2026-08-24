using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniSiniestros.Entities.Enums;
using MiniSiniestros.Services.Validaciones;
using Xunit;

namespace MiniSiniestros.Tests;

public class TransicionEstadoValidatorTests
{
    [Theory]
    [InlineData(EstadoSiniestro.Recibido, EstadoSiniestro.EnAnalisis)]
    [InlineData(EstadoSiniestro.EnAnalisis, EstadoSiniestro.Aprobado)]
    [InlineData(EstadoSiniestro.EnAnalisis, EstadoSiniestro.Rechazado)]
    [InlineData(EstadoSiniestro.Aprobado, EstadoSiniestro.Cerrado)]
    [InlineData(EstadoSiniestro.Rechazado, EstadoSiniestro.Cerrado)]
    public void EsTransicionValida_TransicionesPermitidas_DevuelveTrue(
        EstadoSiniestro estadoActual, EstadoSiniestro nuevoEstado)
    {
        Assert.True(TransicionEstadoValidator.EsTransicionValida(estadoActual, nuevoEstado));
    }

    [Theory]
    [InlineData(EstadoSiniestro.Recibido, EstadoSiniestro.Cerrado)]     // salto de pasos intermedios
    [InlineData(EstadoSiniestro.Cerrado, EstadoSiniestro.Recibido)]      // desde un estado final
    [InlineData(EstadoSiniestro.Aprobado, EstadoSiniestro.Rechazado)]    // cruce entre ramas finales
    [InlineData(EstadoSiniestro.Recibido, EstadoSiniestro.Aprobado)]     // salto directo, se salta EnAnalisis
    public void EsTransicionValida_TransicionesNoPermitidas_DevuelveFalse(
        EstadoSiniestro estadoActual, EstadoSiniestro nuevoEstado)
    {
        Assert.False(TransicionEstadoValidator.EsTransicionValida(estadoActual, nuevoEstado));
    }
}