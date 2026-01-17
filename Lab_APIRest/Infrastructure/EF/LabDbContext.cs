using System;
using System.Collections.Generic;
using Lab_APIRest.Infrastructure.EF.Models;
using Microsoft.EntityFrameworkCore;

namespace Lab_APIRest.Infrastructure.EF;

public partial class LabDbContext : DbContext
{
    public LabDbContext()
    {
    }

    public LabDbContext(DbContextOptions<LabDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Convenio> Convenio { get; set; }

    public virtual DbSet<DetalleOrden> DetalleOrden { get; set; }

    public virtual DbSet<DetallePago> DetallePago { get; set; }

    public virtual DbSet<DetalleResultado> DetalleResultado { get; set; }

    public virtual DbSet<Examen> Examen { get; set; }

    public virtual DbSet<ExamenComposicion> ExamenComposicion { get; set; }

    public virtual DbSet<ExamenReactivo> ExamenReactivo { get; set; }

    public virtual DbSet<Genero> Genero { get; set; }

    public virtual DbSet<Medico> Medico { get; set; }

    public virtual DbSet<MovimientoReactivo> MovimientoReactivo { get; set; }

    public virtual DbSet<Orden> Orden { get; set; }

    public virtual DbSet<Paciente> Paciente { get; set; }

    public virtual DbSet<Pago> Pago { get; set; }

    public virtual DbSet<Reactivo> Reactivo { get; set; }

    public virtual DbSet<Resultado> Resultado { get; set; }

    public virtual DbSet<TokensUsuarios> TokensUsuarios { get; set; }

    public virtual DbSet<Usuario> Usuario { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=BD_Lab_p;User Id=sa;Password=randomcch1203;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Convenio>(entity =>
        {
            entity.HasKey(e => e.IdConvenio).HasName("PK__convenio");

            entity.ToTable("convenio", tb => tb.HasTrigger("tr_convenio_fecha_fin"));

            entity.Property(e => e.IdConvenio).HasColumnName("id_convenio");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.FechaConvenio).HasColumnName("fecha_convenio");
            entity.Property(e => e.FechaFin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");
            entity.Property(e => e.IdMedico).HasColumnName("id_medico");
            entity.Property(e => e.MontoTotal)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("monto_total");
            entity.Property(e => e.PorcentajeComision)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("porcentaje_comision");

            entity.HasOne(d => d.IdMedicoNavigation).WithMany(p => p.Convenio)
                .HasForeignKey(d => d.IdMedico)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__convenio__id_medico");
        });

        modelBuilder.Entity<DetalleOrden>(entity =>
        {
            entity.HasKey(e => new { e.IdOrden, e.IdExamen });

            entity.ToTable("detalle_orden");

            entity.Property(e => e.IdOrden).HasColumnName("id_orden");
            entity.Property(e => e.IdExamen).HasColumnName("id_examen");
            entity.Property(e => e.Precio)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("precio");

            entity.HasOne(d => d.IdExamenNavigation).WithMany(p => p.DetalleOrden)
                .HasForeignKey(d => d.IdExamen)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_detalle_orden_examen");

            entity.HasOne(d => d.IdOrdenNavigation).WithMany(p => p.DetalleOrden)
                .HasForeignKey(d => d.IdOrden)
                .HasConstraintName("FK_detalle_orden_orden");
        });

        modelBuilder.Entity<DetallePago>(entity =>
        {
            entity.HasKey(e => e.IdDetallePago).HasName("PK__detalle_pago");

            entity.ToTable("detalle_pago", tb => tb.HasTrigger("tr_detalle_pago_fecha_fin"));

            entity.Property(e => e.IdDetallePago).HasColumnName("id_detalle_pago");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaFin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");
            entity.Property(e => e.IdPago).HasColumnName("id_pago");
            entity.Property(e => e.Monto)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("monto");
            entity.Property(e => e.TipoPago)
                .HasMaxLength(50)
                .HasColumnName("tipo_pago");

            entity.HasOne(d => d.IdPagoNavigation).WithMany(p => p.DetallePago)
                .HasForeignKey(d => d.IdPago)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__detalle_pago_id_pago");
        });

        modelBuilder.Entity<DetalleResultado>(entity =>
        {
            entity.HasKey(e => new { e.IdResultado, e.IdExamen });

            entity.ToTable("detalle_resultado");

            entity.Property(e => e.IdResultado).HasColumnName("id_resultado");
            entity.Property(e => e.IdExamen).HasColumnName("id_examen");
            entity.Property(e => e.Valor)
                .HasMaxLength(100)
                .HasColumnName("valor");

            entity.HasOne(d => d.IdExamenNavigation).WithMany(p => p.DetalleResultado)
                .HasForeignKey(d => d.IdExamen)
                .HasConstraintName("FK__detalle_resultado_examen");

            entity.HasOne(d => d.IdResultadoNavigation).WithMany(p => p.DetalleResultado)
                .HasForeignKey(d => d.IdResultado)
                .HasConstraintName("FK__detalle_resultado_resultado");
        });

        modelBuilder.Entity<Examen>(entity =>
        {
            entity.HasKey(e => e.IdExamen).HasName("PK__examen");

            entity.ToTable("examen", tb => tb.HasTrigger("tr_examen_fecha_fin"));

            entity.Property(e => e.IdExamen).HasColumnName("id_examen");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Estudio)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("estudio");
            entity.Property(e => e.FechaActualizacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaFin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");
            entity.Property(e => e.NombreExamen)
                .HasMaxLength(255)
                .HasColumnName("nombre_examen");
            entity.Property(e => e.Precio)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("precio");
            entity.Property(e => e.Tecnica)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("tecnica");
            entity.Property(e => e.TiempoEntrega)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("tiempo_entrega");
            entity.Property(e => e.TipoExamen)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("tipo_examen");
            entity.Property(e => e.TipoMuestra)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("tipo_muestra");
            entity.Property(e => e.TituloExamen)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("titulo_examen");
            entity.Property(e => e.Unidad)
                .HasMaxLength(50)
                .HasColumnName("unidad");
            entity.Property(e => e.ValorReferencia)
                .HasMaxLength(100)
                .HasColumnName("valor_referencia");
        });

        modelBuilder.Entity<ExamenComposicion>(entity =>
        {
            entity.HasKey(e => new { e.IdExamenPadre, e.IdExamenHijo }).HasName("PK__examen_composicion");

            entity.ToTable("examen_composicion", tb => tb.HasTrigger("tr_examen_composicion_fecha_fin"));

            entity.Property(e => e.IdExamenPadre).HasColumnName("id_examen_padre");
            entity.Property(e => e.IdExamenHijo).HasColumnName("id_examen_hijo");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.FechaActualizacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaFin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");

            entity.HasOne(d => d.IdExamenHijoNavigation).WithMany(p => p.ExamenComposicionIdExamenHijoNavigation)
                .HasForeignKey(d => d.IdExamenHijo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_examen_composicion_hijo");

            entity.HasOne(d => d.IdExamenPadreNavigation).WithMany(p => p.ExamenComposicionIdExamenPadreNavigation)
                .HasForeignKey(d => d.IdExamenPadre)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_examen_composicion_padre");
        });

        modelBuilder.Entity<ExamenReactivo>(entity =>
        {
            entity.HasKey(e => new { e.IdExamen, e.IdReactivo });

            entity.ToTable("examen_reactivo", tb => tb.HasTrigger("tr_examen_reactivo_fecha_fin"));

            entity.Property(e => e.IdExamen).HasColumnName("id_examen");
            entity.Property(e => e.IdReactivo).HasColumnName("id_reactivo");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.CantidadUsada)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("cantidad_usada");
            entity.Property(e => e.FechaActualizacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaFin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");

            entity.HasOne(d => d.IdExamenNavigation).WithMany(p => p.ExamenReactivo)
                .HasForeignKey(d => d.IdExamen)
                .HasConstraintName("FK_examen_reactivo_examen");

            entity.HasOne(d => d.IdReactivoNavigation).WithMany(p => p.ExamenReactivo)
                .HasForeignKey(d => d.IdReactivo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_examen_reactivo_reactivo");
        });

        modelBuilder.Entity<Genero>(entity =>
        {
            entity.HasKey(e => e.IdGenero).HasName("PK__genero");

            entity.ToTable("genero");

            entity.HasIndex(e => e.Nombre, "UQ_genero_nombre").IsUnique();

            entity.Property(e => e.IdGenero).HasColumnName("id_genero");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaActualizacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Medico>(entity =>
        {
            entity.HasKey(e => e.IdMedico).HasName("PK__medico");

            entity.ToTable("medico", tb => tb.HasTrigger("tr_medico_fecha_fin"));

            entity.HasIndex(e => e.Correo, "UQ__medico").IsUnique();

            entity.Property(e => e.IdMedico).HasColumnName("id_medico");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("correo");
            entity.Property(e => e.Especialidad)
                .HasMaxLength(100)
                .HasColumnName("especialidad");
            entity.Property(e => e.FechaActualizacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaFin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");
            entity.Property(e => e.NombreMedico)
                .HasMaxLength(100)
                .HasColumnName("nombre_medico");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("telefono");
        });

        modelBuilder.Entity<MovimientoReactivo>(entity =>
        {
            entity.HasKey(e => e.IdMovimientoReactivo).HasName("PK__movimiento_reactivo");

            entity.ToTable("movimiento_reactivo");

            entity.Property(e => e.IdMovimientoReactivo).HasColumnName("id_movimiento_reactivo");
            entity.Property(e => e.Cantidad)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("cantidad");
            entity.Property(e => e.FechaMovimiento)
                .HasColumnType("datetime")
                .HasColumnName("fecha_movimiento");
            entity.Property(e => e.IdDetalleResultado).HasColumnName("id_detalle_resultado");
            entity.Property(e => e.IdExamen).HasColumnName("id_examen");
            entity.Property(e => e.IdReactivo).HasColumnName("id_reactivo");
            entity.Property(e => e.IdResultado).HasColumnName("id_resultado");
            entity.Property(e => e.Observacion)
                .HasMaxLength(255)
                .HasColumnName("observacion");
            entity.Property(e => e.TipoMovimiento)
                .HasMaxLength(50)
                .HasColumnName("tipo_movimiento");

            entity.HasOne(d => d.IdReactivoNavigation).WithMany(p => p.MovimientoReactivo)
                .HasForeignKey(d => d.IdReactivo)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__movimiento_reactivo_reactivo");

            entity.HasOne(d => d.DetalleResultado).WithMany(p => p.MovimientoReactivo)
                .HasForeignKey(d => new { d.IdResultado, d.IdExamen })
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_movimiento_reactivo_detalle_resultado");
        });

        modelBuilder.Entity<Orden>(entity =>
        {
            entity.HasKey(e => e.IdOrden).HasName("PK__orden");

            entity.ToTable("orden", tb => tb.HasTrigger("tr_orden_fecha_fin"));

            entity.HasIndex(e => e.NumeroOrden, "UQ__orden").IsUnique();

            entity.HasIndex(e => e.EstadoPago, "idx_orden_estado_pago");

            entity.Property(e => e.IdOrden).HasColumnName("id_orden");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.EstadoPago)
                .HasMaxLength(20)
                .HasColumnName("estado_pago");
            entity.Property(e => e.EstadoOrden)
                .HasMaxLength(30)
                .HasDefaultValue("EN_PROCESO")
                .HasColumnName("estado_orden");
            entity.Property(e => e.FechaFin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");
            entity.Property(e => e.FechaFinalizacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_finalizacion");
            entity.Property(e => e.FechaOrden).HasColumnName("fecha_orden");
            entity.Property(e => e.IdConvenio).HasColumnName("id_convenio");
            entity.Property(e => e.IdMedico).HasColumnName("id_medico");
            entity.Property(e => e.IdPaciente).HasColumnName("id_paciente");
            entity.Property(e => e.NumeroOrden)
                .HasMaxLength(50)
                .HasColumnName("numero_orden");
            entity.Property(e => e.Observacion)
                .HasMaxLength(255)
                .HasColumnName("observacion");
            entity.Property(e => e.ResultadosHabilitados)
                .HasDefaultValue(false)
                .HasColumnName("resultados_habilitados");
            entity.Property(e => e.SaldoPendiente)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("saldo_pendiente");
            entity.Property(e => e.Total)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("total");
            entity.Property(e => e.TotalPagado)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("total_pagado");

            entity.HasOne(d => d.IdConvenioNavigation).WithMany(p => p.Orden)
                .HasForeignKey(d => d.IdConvenio)
                .HasConstraintName("FK_orden_convenio");

            entity.HasOne(d => d.IdMedicoNavigation).WithMany(p => p.Orden)
                .HasForeignKey(d => d.IdMedico)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__orden_medico");

            entity.HasOne(d => d.IdPacienteNavigation).WithMany(p => p.Orden)
                .HasForeignKey(d => d.IdPaciente)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__orden_paciente");
        });

        modelBuilder.Entity<Paciente>(entity =>
        {
            entity.HasKey(e => e.IdPaciente).HasName("PK__paciente");

            entity.ToTable("paciente", tb => tb.HasTrigger("tr_paciente_fecha_fin"));

            entity.HasIndex(e => e.CedulaPaciente, "UQ__paciente").IsUnique();

            entity.HasIndex(e => e.CedulaPaciente, "UQ_paciente_cedula_paciente").IsUnique();

            entity.HasIndex(e => e.NombrePaciente, "idx_paciente_nombre");

            entity.Property(e => e.IdPaciente).HasColumnName("id_paciente");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.CedulaPaciente)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("cedula_paciente");
            entity.Property(e => e.CorreoElectronicoPaciente)
                .HasMaxLength(150)
                .HasColumnName("correo_electronico_paciente");
            entity.Property(e => e.DireccionPaciente)
                .HasMaxLength(150)
                .HasColumnName("direccion_paciente");
            entity.Property(e => e.FechaActualizacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaFin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");
            entity.Property(e => e.FechaNacPaciente).HasColumnName("fecha_nac_paciente");
            entity.Property(e => e.IdGenero).HasColumnName("id_genero");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.NombrePaciente)
                .HasMaxLength(100)
                .HasColumnName("nombre_paciente");
            entity.Property(e => e.TelefonoPaciente)
                .HasMaxLength(20)
                .HasColumnName("telefono_paciente");

            entity.HasOne(d => d.IdGeneroNavigation).WithMany(p => p.Paciente)
                .HasForeignKey(d => d.IdGenero)
                .HasConstraintName("FK_paciente_genero");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Paciente)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK_paciente_usuario");
        });

        modelBuilder.Entity<Pago>(entity =>
        {
            entity.HasKey(e => e.IdPago).HasName("PK__pago");

            entity.ToTable("pago", tb => tb.HasTrigger("tr_pago_fecha_fin"));

            entity.HasIndex(e => e.FechaPago, "idx_pago_fecha");

            entity.Property(e => e.IdPago).HasColumnName("id_pago");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.FechaFin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");
            entity.Property(e => e.FechaPago)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_pago");
            entity.Property(e => e.IdOrden).HasColumnName("id_orden");
            entity.Property(e => e.MontoPagado)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("monto_pagado");
            entity.Property(e => e.Observacion)
                .HasMaxLength(255)
                .HasColumnName("observacion");

            entity.HasOne(d => d.IdOrdenNavigation).WithMany(p => p.Pago)
                .HasForeignKey(d => d.IdOrden)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__pago_orden");
        });

        modelBuilder.Entity<Reactivo>(entity =>
        {
            entity.HasKey(e => e.IdReactivo).HasName("PK__reactivo");

            entity.ToTable("reactivo", tb => tb.HasTrigger("tr_reactivo_fecha_fin"));

            entity.Property(e => e.IdReactivo).HasColumnName("id_reactivo");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.CantidadDisponible)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("cantidad_disponible");
            entity.Property(e => e.Fabricante)
                .HasMaxLength(100)
                .HasColumnName("fabricante");
            entity.Property(e => e.FechaActualizacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaFin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");
            entity.Property(e => e.NombreReactivo)
                .HasMaxLength(100)
                .HasColumnName("nombre_reactivo");
            entity.Property(e => e.Unidad)
                .HasMaxLength(50)
                .HasColumnName("unidad");
        });

        modelBuilder.Entity<Resultado>(entity =>
        {
            entity.HasKey(e => e.IdResultado).HasName("PK__resultado");

            entity.ToTable("resultado", tb => tb.HasTrigger("tr_resultado_fecha_fin"));

            entity.Property(e => e.IdResultado).HasColumnName("id_resultado");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.EstadoResultado)
                .HasMaxLength(30)
                .HasColumnName("estado_resultado");
            entity.Property(e => e.FechaFin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");
            entity.Property(e => e.FechaResultado)
                .HasColumnType("datetime")
                .HasColumnName("fecha_resultado");
            entity.Property(e => e.FechaRevision)
                .HasColumnType("datetime")
                .HasColumnName("fecha_revision");
            entity.Property(e => e.IdOrden).HasColumnName("id_orden");
            entity.Property(e => e.IdRevisor).HasColumnName("id_revisor");
            entity.Property(e => e.NumeroResultado)
                .HasMaxLength(50)
                .HasColumnName("numero_resultado");
            entity.Property(e => e.ObservacionRevision)
                .HasColumnType("text")
                .HasColumnName("observacion_revision");
            entity.Property(e => e.Observaciones)
                .HasMaxLength(255)
                .HasColumnName("observaciones");

            entity.HasOne(d => d.IdOrdenNavigation).WithMany(p => p.Resultado)
                .HasForeignKey(d => d.IdOrden)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_resultado_orden");

            entity.HasOne(d => d.IdRevisorNavigation).WithMany(p => p.Resultado)
                .HasForeignKey(d => d.IdRevisor)
                .HasConstraintName("FK_resultado_revisor");
        });

        modelBuilder.Entity<TokensUsuarios>(entity =>
        {
            entity.HasKey(e => e.IdToken).HasName("PK__tokens_usuarios");

            entity.ToTable("tokens_usuarios");

            entity.HasIndex(e => e.FechaExpiracion, "IX_recup_expiracion");

            entity.HasIndex(e => new { e.IdUsuario, e.Usado }, "IX_recup_usuario_usado");

            entity.HasIndex(e => e.TokenHash, "UQ_recup_token_hash").IsUnique();

            entity.Property(e => e.IdToken).HasColumnName("id_token");
            entity.Property(e => e.DispositivoOrigen)
                .HasMaxLength(256)
                .HasColumnName("dispositivo_origen");
            entity.Property(e => e.FechaExpiracion)
                .HasPrecision(0)
                .HasColumnName("fecha_expiracion");
            entity.Property(e => e.FechaSolicitud)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("fecha_solicitud");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.IpSolicitud)
                .HasMaxLength(45)
                .HasColumnName("ip_solicitud");
            entity.Property(e => e.TipoToken)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("tipo_token");
            entity.Property(e => e.TokenHash)
                .HasMaxLength(32)
                .HasColumnName("token_hash");
            entity.Property(e => e.Usado).HasColumnName("usado");
            entity.Property(e => e.UsadoEn)
                .HasPrecision(0)
                .HasColumnName("usado_en");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.TokensUsuarios)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tokens_usuario");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__usuario");

            entity.ToTable("usuario", tb => tb.HasTrigger("tr_usuario_fecha_fin"));

            entity.HasIndex(e => e.CorreoUsuario, "UQ__usuario").IsUnique();

            entity.HasIndex(e => e.CorreoUsuario, "UQ_usuario_correo_usuario")
                .IsUnique()
                .HasFilter("([correo_usuario] IS NOT NULL)");

            entity.HasIndex(e => e.CorreoUsuario, "idx_usuario_correo");

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.ClaveUsuario)
                .HasMaxLength(255)
                .HasColumnName("clave_usuario");
            entity.Property(e => e.CorreoUsuario)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("correo_usuario");
            entity.Property(e => e.FechaActualizacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaFin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");
            entity.Property(e => e.Nombre)
                .HasMaxLength(255)
                .HasColumnName("nombre");
            entity.Property(e => e.Rol)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("rol");
            entity.Property(e => e.UltimoAcceso).HasColumnName("ultimo_acceso");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
