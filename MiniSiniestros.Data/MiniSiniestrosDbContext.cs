using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniSiniestros.Entities;

namespace MiniSiniestros.Data;

public class MiniSiniestrosDbContext : DbContext
{
    public MiniSiniestrosDbContext(DbContextOptions<MiniSiniestrosDbContext> options)
        : base(options)
    {
    }

    public DbSet<Siniestro> Siniestros => Set<Siniestro>();
    public DbSet<Prestador> Prestadores => Set<Prestador>();
    public DbSet<HistorialEstado> HistorialEstados => Set<HistorialEstado>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // CuitEmpleador y CuilTrabajador: longitud fija de 11 caracteres
        // (CUIT/CUIL argentino), obligatorios
        modelBuilder.Entity<Siniestro>(entity =>
        {
            entity.Property(s => s.CuitEmpleador)
                .HasMaxLength(11)
                .IsRequired();

            entity.Property(s => s.CuilTrabajador)
                .HasMaxLength(11)
                .IsRequired();

            // El enum EstadoSiniestro se guarda como string en la base
            entity.Property(s => s.Estado)
                .HasConversion<string>()
                .HasMaxLength(20);
        });

        modelBuilder.Entity<HistorialEstado>(entity =>
        {
            entity.Property(h => h.EstadoAnterior).HasConversion<string>().HasMaxLength(20);
            entity.Property(h => h.EstadoNuevo).HasConversion<string>().HasMaxLength(20);
        });

        // Relación N:N implícita entre Siniestro y Prestador 
        modelBuilder.Entity<Siniestro>()
            .HasMany(s => s.Prestadores)
            .WithMany(p => p.Siniestros)
            .UsingEntity(j => j.ToTable("SiniestroPrestador"));
    }
}
