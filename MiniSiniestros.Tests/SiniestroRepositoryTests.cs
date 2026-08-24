using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniSiniestros.Data;
using MiniSiniestros.Data.Repositories;
using MiniSiniestros.Entities;
using MiniSiniestros.Entities.Enums;
using Xunit;

namespace MiniSiniestros.Tests;

public class SiniestroRepositoryTests
{
    // Cada test arma su propia base en memoria, con nombre único
    // (Guid.NewGuid()) para que no se pisen datos entre tests que
    // corren en paralelo
    private static MiniSiniestrosDbContext CrearContextoConDatos()
    {
        var options = new DbContextOptionsBuilder<MiniSiniestrosDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new MiniSiniestrosDbContext(options);

        context.Siniestros.AddRange(
            new Siniestro { CuitEmpleador = "20111111111", CuilTrabajador = "27111111111", Estado = EstadoSiniestro.Recibido, FechaOcurrencia = new DateTime(2026, 8, 1), FechaAlta = new DateTime(2026, 8, 1) },
            new Siniestro { CuitEmpleador = "20222222222", CuilTrabajador = "27222222222", Estado = EstadoSiniestro.EnAnalisis, FechaOcurrencia = new DateTime(2026, 7, 15), FechaAlta = new DateTime(2026, 7, 16) },
            new Siniestro { CuitEmpleador = "20111111111", CuilTrabajador = "27333333333", Estado = EstadoSiniestro.Cerrado, FechaOcurrencia = new DateTime(2026, 6, 1), FechaAlta = new DateTime(2026, 6, 2) }
        );
        context.SaveChanges();

        return context;
    }

    [Fact]
    public async Task GetFiltradosAsync_FiltraPorEstado_DevuelveSoloLosQueCoinciden()
    {
        // Arrange
        using var context = CrearContextoConDatos();
        var repo = new SiniestroRepository(context);

        // Act
        var resultado = await repo.GetFiltradosAsync(
            estado: EstadoSiniestro.Recibido, desde: null, hasta: null,
            cuitEmpleador: null, cuilTrabajador: null,
            ordenarPor: null, page: 1, pageSize: 10);

        // Assert
        Assert.Single(resultado);
        Assert.Equal(EstadoSiniestro.Recibido, resultado[0].Estado);
    }

    [Fact]
    public async Task GetFiltradosAsync_FiltraPorCuitParcial_DevuelveCoincidencias()
    {
        using var context = CrearContextoConDatos();
        var repo = new SiniestroRepository(context);

        // Dos de los tres siniestros de prueba comparten el mismo CUIT
        var resultado = await repo.GetFiltradosAsync(
            estado: null, desde: null, hasta: null,
            cuitEmpleador: "20111111111", cuilTrabajador: null,
            ordenarPor: null, page: 1, pageSize: 10);

        Assert.Equal(2, resultado.Count);
    }

    [Fact]
    public async Task GetFiltradosAsync_SinFiltros_RespetaLaPaginacion()
    {
        using var context = CrearContextoConDatos();
        var repo = new SiniestroRepository(context);

        // Pedimos página 1 con tamaño 2, de un total de 3 registros
        var resultado = await repo.GetFiltradosAsync(
            estado: null, desde: null, hasta: null,
            cuitEmpleador: null, cuilTrabajador: null,
            ordenarPor: null, page: 1, pageSize: 2);

        Assert.Equal(2, resultado.Count); // solo 2, no los 3 totales
    }

    [Fact]
    public async Task GetFiltradosAsync_OrdenarPorEstado_OrdenaPorElValorDelEnum()
    {
        using var context = CrearContextoConDatos();
        var repo = new SiniestroRepository(context);

        var resultado = await repo.GetFiltradosAsync(
            estado: null, desde: null, hasta: null,
            cuitEmpleador: null, cuilTrabajador: null,
            ordenarPor: "estado", page: 1, pageSize: 10);

        // Recibido=0, EnAnalisis=1, Cerrado=4 - ordena por el valor
        // numérico subyacente del enum, no alfabéticamente
        Assert.Equal(EstadoSiniestro.Recibido, resultado[0].Estado);
    }

    [Fact]
    public async Task ContarFiltradosAsync_FiltraPorEstado_CuentaSoloLosQueCoinciden()
    {
        using var context = CrearContextoConDatos();
        var repo = new SiniestroRepository(context);

        var total = await repo.ContarFiltradosAsync(
            estado: EstadoSiniestro.Cerrado, desde: null, hasta: null,
            cuitEmpleador: null, cuilTrabajador: null);

        Assert.Equal(1, total);
    }
}