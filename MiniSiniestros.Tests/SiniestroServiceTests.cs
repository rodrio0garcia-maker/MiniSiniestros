using MiniSiniestros.Data.Repositories;
using MiniSiniestros.Dto;
using MiniSiniestros.Entities;
using MiniSiniestros.Entities.Enums;
using MiniSiniestros.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MiniSiniestros.Tests;

public class SiniestroServiceTests
{
    [Fact]
    public async Task CambiarEstadoAsync_SiniestroNoExiste_DevuelveFalse()
    {
        // Arrange: armamos los mocks
        var mockSiniestroRepo = new Mock<ISiniestroRepository>();
        mockSiniestroRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Siniestro?)null); // simula que no se encontró nada

        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(u => u.Siniestros).Returns(mockSiniestroRepo.Object);

        var mapper = ArmarMapperReal();
        var service = new SiniestroService(mockUnitOfWork.Object, mapper);

        // Act
        var resultado = await service.CambiarEstadoAsync(999, EstadoSiniestro.EnAnalisis);

        // Assert
        Assert.False(resultado);

        // Verify: además de chequear el resultado, confirmamos que
        // NUNCA se llegó a intentar guardar nada, porque el siniestro
        // no existía
        mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CambiarEstadoAsync_TransicionValida_ActualizaEstadoYGuardaHistorial()
    {
        // Arrange
        var siniestroExistente = new Siniestro
        {
            Id = 1,
            Estado = EstadoSiniestro.Recibido,
            CuitEmpleador = "20304050607",
            CuilTrabajador = "27111222339",
            Prestadores = new List<Prestador>(),
            Historial = new List<HistorialEstado>()
        };

        var mockSiniestroRepo = new Mock<ISiniestroRepository>();
        mockSiniestroRepo
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(siniestroExistente);

        var mockHistorialRepo = new Mock<IHistorialEstadoRepository>();

        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(u => u.Siniestros).Returns(mockSiniestroRepo.Object);
        mockUnitOfWork.Setup(u => u.HistorialEstados).Returns(mockHistorialRepo.Object);

        var mapper = ArmarMapperReal();
        var service = new SiniestroService(mockUnitOfWork.Object, mapper);

        // Act: Recibido -> EnAnalisis es una transición válida
        var resultado = await service.CambiarEstadoAsync(1, EstadoSiniestro.EnAnalisis);

        // Assert
        Assert.True(resultado);
        Assert.Equal(EstadoSiniestro.EnAnalisis, siniestroExistente.Estado);

        // Verify: confirmamos que se llamó a Update sobre el siniestro
        mockSiniestroRepo.Verify(r => r.Update(siniestroExistente), Times.Once);

        // Verify: confirmamos que se agregó un registro de historial,
        // con los datos correctos (estado anterior y nuevo)
        mockHistorialRepo.Verify(h => h.AddAsync(It.Is<HistorialEstado>(
            historial => historial.EstadoAnterior == EstadoSiniestro.Recibido &&
                         historial.EstadoNuevo == EstadoSiniestro.EnAnalisis)),
            Times.Once);

        // Verify: se guardó todo junto, una sola vez
        mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CambiarEstadoAsync_TransicionInvalida_NoModificaNiGuardaNada()
    {
        // Arrange: un siniestro en estado Cerrado - no admite ninguna transición
        var siniestroCerrado = new Siniestro
        {
            Id = 1,
            Estado = EstadoSiniestro.Cerrado,
            CuitEmpleador = "20304050607",
            CuilTrabajador = "27111222339",
            Prestadores = new List<Prestador>(),
            Historial = new List<HistorialEstado>()
        };

        var mockSiniestroRepo = new Mock<ISiniestroRepository>();
        mockSiniestroRepo
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(siniestroCerrado);

        var mockHistorialRepo = new Mock<IHistorialEstadoRepository>();

        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(u => u.Siniestros).Returns(mockSiniestroRepo.Object);
        mockUnitOfWork.Setup(u => u.HistorialEstados).Returns(mockHistorialRepo.Object);

        var mapper = ArmarMapperReal();
        var service = new SiniestroService(mockUnitOfWork.Object, mapper);

        // Act: intentar mover un siniestro Cerrado a Recibido - inválido
        var resultado = await service.CambiarEstadoAsync(1, EstadoSiniestro.Recibido);

        // Assert
        Assert.False(resultado);

        // El estado del objeto en memoria NO debería haber cambiado
        Assert.Equal(EstadoSiniestro.Cerrado, siniestroCerrado.Estado);

        // Verify: como la transición es inválida, ninguno de estos 3
        // métodos debería haberse llamado nunca
        mockSiniestroRepo.Verify(r => r.Update(It.IsAny<Siniestro>()), Times.Never);
        mockHistorialRepo.Verify(h => h.AddAsync(It.IsAny<HistorialEstado>()), Times.Never);
        mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task AsignarPrestadorAsync_PrestadorNoAsignadoAun_LoAgregaYDevuelveTrue()
    {
        // Arrange
        var siniestro = new Siniestro
        {
            Id = 1,
            Estado = EstadoSiniestro.Recibido,
            CuitEmpleador = "20304050607",
            CuilTrabajador = "27111222339",
            Prestadores = new List<Prestador>(), // todavía sin prestadores
            Historial = new List<HistorialEstado>()
        };

        var prestador = new Prestador { Id = 5, Nombre = "Clínica San Martín", Especialidad = "Traumatología" };

        var mockSiniestroRepo = new Mock<ISiniestroRepository>();
        mockSiniestroRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(siniestro);

        var mockPrestadorRepo = new Mock<IPrestadorRepository>();
        mockPrestadorRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(prestador);

        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(u => u.Siniestros).Returns(mockSiniestroRepo.Object);
        mockUnitOfWork.Setup(u => u.Prestadores).Returns(mockPrestadorRepo.Object);

        var service = new SiniestroService(mockUnitOfWork.Object, ArmarMapperReal());

        // Act
        var resultado = await service.AsignarPrestadorAsync(1, 5);

        // Assert
        Assert.True(resultado);
        Assert.Contains(prestador, siniestro.Prestadores); // el prestador quedó agregado a la colección

        mockSiniestroRepo.Verify(r => r.Update(siniestro), Times.Once);
        mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AsignarPrestadorAsync_PrestadorYaAsignado_DevuelveFalseYNoDuplica()
    {
        // Arrange
        var prestador = new Prestador { Id = 5, Nombre = "Clínica San Martín", Especialidad = "Traumatología" };

        var siniestro = new Siniestro
        {
            Id = 1,
            Estado = EstadoSiniestro.Recibido,
            CuitEmpleador = "20304050607",
            CuilTrabajador = "27111222339",
            Prestadores = new List<Prestador> { prestador }, // YA tiene este prestador asignado
            Historial = new List<HistorialEstado>()
        };

        var mockSiniestroRepo = new Mock<ISiniestroRepository>();
        mockSiniestroRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(siniestro);

        var mockPrestadorRepo = new Mock<IPrestadorRepository>();
        mockPrestadorRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(prestador);

        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(u => u.Siniestros).Returns(mockSiniestroRepo.Object);
        mockUnitOfWork.Setup(u => u.Prestadores).Returns(mockPrestadorRepo.Object);

        var service = new SiniestroService(mockUnitOfWork.Object, ArmarMapperReal());

        // Act: intentamos asignar el MISMO prestador de nuevo
        var resultado = await service.AsignarPrestadorAsync(1, 5);

        // Assert
        Assert.False(resultado);
        Assert.Single(siniestro.Prestadores); // sigue habiendo solo 1, no se duplicó

        mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task AsignarPrestadorAsync_PrestadorNoExiste_DevuelveFalse()
    {
        var siniestro = new Siniestro
        {
            Id = 1,
            Estado = EstadoSiniestro.Recibido,
            Prestadores = new List<Prestador>(),
            Historial = new List<HistorialEstado>()
        };

        var mockSiniestroRepo = new Mock<ISiniestroRepository>();
        mockSiniestroRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(siniestro);

        var mockPrestadorRepo = new Mock<IPrestadorRepository>();
        mockPrestadorRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Prestador?)null);

        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(u => u.Siniestros).Returns(mockSiniestroRepo.Object);
        mockUnitOfWork.Setup(u => u.Prestadores).Returns(mockPrestadorRepo.Object);

        var service = new SiniestroService(mockUnitOfWork.Object, ArmarMapperReal());

        var resultado = await service.AsignarPrestadorAsync(1, 999);

        Assert.False(resultado);
        mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CrearAsync_SiempreArrancaEnEstadoRecibido()
    {
        // Arrange
        var dto = new SiniestroCreateDto
        {
            CuitEmpleador = "20304050607",
            CuilTrabajador = "27111222339",
            FechaOcurrencia = new DateTime(2026, 8, 1)
        };

        var mockSiniestroRepo = new Mock<ISiniestroRepository>();

        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(u => u.Siniestros).Returns(mockSiniestroRepo.Object);

        var service = new SiniestroService(mockUnitOfWork.Object, ArmarMapperReal());

        // Act
        var resultado = await service.CrearAsync(dto);

        // Assert
        Assert.Equal(EstadoSiniestro.Recibido, resultado.Estado);
        Assert.Equal(dto.CuitEmpleador, resultado.CuitEmpleador);
        Assert.Equal(dto.CuilTrabajador, resultado.CuilTrabajador);

        // Verify: se llamó a AddAsync con un Siniestro que tiene el estado correcto
        mockSiniestroRepo.Verify(r => r.AddAsync(
            It.Is<Siniestro>(s => s.Estado == EstadoSiniestro.Recibido)),
            Times.Once);

        mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    private static AutoMapper.IMapper ArmarMapperReal()
    {
        var config = new AutoMapper.MapperConfiguration(cfg =>
            cfg.AddProfile<MiniSiniestros.Services.Mapping.MappingProfile>());
        return config.CreateMapper();
    }
}