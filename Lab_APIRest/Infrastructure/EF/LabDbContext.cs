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

    public virtual DbSet<convenio> Convenio { get; set; }

    public virtual DbSet<detalle_orden> DetalleOrden { get; set; }

    public virtual DbSet<detalle_pago> DetallePago { get; set; }

    public virtual DbSet<estado_pago> EstadoPago { get; set; }

    public virtual DbSet<detalle_resultado> DetalleResultado { get; set; }

    public virtual DbSet<examen> Examen { get; set; }

    public virtual DbSet<examen_composicion> ExamenComposicion { get; set; }

    public virtual DbSet<examen_reactivo> ExamenReactivo { get; set; }

    public virtual DbSet<estudio> Estudio { get; set; }

    public virtual DbSet<tipo_pago> TipoPago { get; set; }

    public virtual DbSet<grupo_examen> GrupoExamen { get; set; }

    public virtual DbSet<tipo_muestra> TipoMuestra { get; set; }

    public virtual DbSet<tipo_examen> TipoExamen { get; set; }

    public virtual DbSet<tecnica> Tecnica { get; set; }

    public virtual DbSet<tipo_registro> TipoRegistro { get; set; }

    public virtual DbSet<referencia_examen> ReferenciaExamen { get; set; }

    public virtual DbSet<genero> Genero { get; set; }

    public virtual DbSet<medico> Medico { get; set; }

    public virtual DbSet<movimiento_reactivo> MovimientoReactivo { get; set; }

    public virtual DbSet<orden> Orden { get; set; }

    public virtual DbSet<paciente> Paciente { get; set; }

    public virtual DbSet<pago> Pago { get; set; }

    public virtual DbSet<reactivo> Reactivo { get; set; }

    public virtual DbSet<resultado> Resultado { get; set; }

    public virtual DbSet<estado_orden> EstadoOrden { get; set; }

    public virtual DbSet<estado_resultado> EstadoResultado { get; set; }

    public virtual DbSet<tokens_usuarios> TokensUsuarios { get; set; }

    public virtual DbSet<usuario> Usuario { get; set; }

    public virtual DbSet<rol> Rol { get; set; }

    public virtual DbSet<persona> Persona { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<convenio>(entity =>
        {
            entity.HasKey(e => e.id_convenio).HasName("PK__convenio");

            entity.ToTable("convenio", tb => tb.HasTrigger("tr_convenio_fecha_fin"));

            entity.Property(e => e.id_convenio).HasColumnName("id_convenio");
            entity.Property(e => e.activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.fecha_convenio).HasColumnName("fecha_convenio");
            entity.Property(e => e.fecha_fin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");
            entity.Property(e => e.id_medico).HasColumnName("id_medico");
            entity.Property(e => e.monto_total)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("monto_total");
            entity.Property(e => e.porcentaje_comision)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("porcentaje_comision");

            entity.HasOne(d => d.medico_navigation).WithMany(p => p.convenio)
                .HasForeignKey(d => d.id_medico)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__convenio__id_medico");
        });

        modelBuilder.Entity<estudio>(entity =>
        {
            entity.HasKey(e => e.id_estudio).HasName("PK__estudio");

            entity.ToTable("estudio", tb => tb.HasTrigger("tr_estudio_fecha_fin"));

            entity.Property(e => e.id_estudio).HasColumnName("id_estudio");
            entity.Property(e => e.nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.fecha_creacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.fecha_actualizacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.fecha_fin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");
        });

        modelBuilder.Entity<tipo_pago>(entity =>
        {
            entity.HasKey(e => e.id_tipo_pago).HasName("PK__tipo_pago");

            entity.ToTable("tipo_pago", tb => tb.HasTrigger("tr_tipo_pago_fecha_fin"));

            entity.Property(e => e.id_tipo_pago).HasColumnName("id_tipo_pago");
            entity.Property(e => e.nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.descripcion)
                .HasMaxLength(255)
                .HasColumnName("descripcion");
            entity.Property(e => e.activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.fecha_creacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.fecha_actualizacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.fecha_fin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");
        });

        modelBuilder.Entity<grupo_examen>(entity =>
        {
            entity.HasKey(e => e.id_grupo_examen).HasName("PK__grupo_examen");

            entity.ToTable("grupo_examen", tb => tb.HasTrigger("tr_grupo_examen_fecha_fin"));

            entity.Property(e => e.id_grupo_examen).HasColumnName("id_grupo_examen");
            entity.Property(e => e.nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.fecha_creacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.fecha_actualizacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.fecha_fin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");
        });

        modelBuilder.Entity<tipo_muestra>(entity =>
        {
            entity.HasKey(e => e.id_tipo_muestra).HasName("PK__tipo_muestra");

            entity.ToTable("tipo_muestra", tb => tb.HasTrigger("tr_tipo_muestra_fecha_fin"));

            entity.Property(e => e.id_tipo_muestra).HasColumnName("id_tipo_muestra");
            entity.Property(e => e.nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.fecha_creacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.fecha_actualizacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.fecha_fin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");
        });

        modelBuilder.Entity<tipo_examen>(entity =>
        {
            entity.HasKey(e => e.id_tipo_examen).HasName("PK__tipo_examen");

            entity.ToTable("tipo_examen", tb => tb.HasTrigger("tr_tipo_examen_fecha_fin"));

            entity.Property(e => e.id_tipo_examen).HasColumnName("id_tipo_examen");
            entity.Property(e => e.nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.fecha_creacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.fecha_actualizacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.fecha_fin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");
        });

        modelBuilder.Entity<tecnica>(entity =>
        {
            entity.HasKey(e => e.id_tecnica).HasName("PK__tecnica");

            entity.ToTable("tecnica", tb => tb.HasTrigger("tr_tecnica_fecha_fin"));

            entity.Property(e => e.id_tecnica).HasColumnName("id_tecnica");
            entity.Property(e => e.nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.fecha_creacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.fecha_actualizacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.fecha_fin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");
        });

        modelBuilder.Entity<tipo_registro>(entity =>
        {
            entity.HasKey(e => e.id_tipo_registro).HasName("PK__tipo_registro");

            entity.ToTable("tipo_registro", tb => tb.HasTrigger("tr_tipo_registro_fecha_fin"));

            entity.Property(e => e.id_tipo_registro).HasColumnName("id_tipo_registro");
            entity.Property(e => e.nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
            entity.Property(e => e.activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.fecha_creacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.fecha_actualizacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.fecha_fin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");
        });

        modelBuilder.Entity<referencia_examen>(entity =>
        {
            entity.HasKey(e => e.id_referencia_examen).HasName("PK__referencia_examen");

            entity.ToTable("referencia_examen", tb => tb.HasTrigger("tr_referencia_examen_fecha_fin"));

            entity.Property(e => e.id_referencia_examen).HasColumnName("id_referencia_examen");
            entity.Property(e => e.id_examen).HasColumnName("id_examen");
            entity.Property(e => e.valor_min)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("valor_min");
            entity.Property(e => e.valor_max)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("valor_max");
            entity.Property(e => e.valor_texto)
                .HasMaxLength(255)
                .HasColumnName("valor_texto");
            entity.Property(e => e.unidad)
                .HasMaxLength(50)
                .HasColumnName("unidad");
            entity.Property(e => e.activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.fecha_creacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.fecha_actualizacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.fecha_fin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");

            entity.HasOne(d => d.examen_navigation).WithMany(p => p.referencia_examen)
                .HasForeignKey(d => d.id_examen)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_referencia_examen_examen");
        });

        modelBuilder.Entity<persona>(entity =>
        {
            entity.HasKey(e => e.id_persona).HasName("PK__persona");

            entity.ToTable("persona", tb => tb.HasTrigger("tr_persona_fecha_fin"));

            entity.HasIndex(e => e.cedula, "UQ_persona_cedula").IsUnique();

            entity.Property(e => e.id_persona).HasColumnName("id_persona");
            entity.Property(e => e.cedula)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("cedula");
            entity.Property(e => e.nombres)
                .HasMaxLength(150)
                .HasColumnName("nombres");
            entity.Property(e => e.apellidos)
                .HasMaxLength(150)
                .HasColumnName("apellidos");
            entity.Property(e => e.id_genero)
                .HasColumnName("id_genero");
            entity.Property(e => e.fecha_nacimiento)
                .HasColumnType("date")
                .HasColumnName("fecha_nacimiento");
            entity.Property(e => e.telefono)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("telefono");
            entity.Property(e => e.direccion)
                .HasMaxLength(150)
                .HasColumnName("direccion");
            entity.Property(e => e.activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.fecha_creacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.fecha_actualizacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.fecha_fin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");

            entity.HasOne(d => d.genero_navigation).WithMany(p => p.persona)
                .HasForeignKey(d => d.id_genero)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_persona_genero");
        });

        modelBuilder.Entity<estado_orden>(entity =>
        {
            entity.HasKey(e => e.id_estado_orden).HasName("PK__estado_orden");

            entity.ToTable("estado_orden");

            entity.Property(e => e.id_estado_orden).HasColumnName("id_estado_orden");
            entity.Property(e => e.activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.descripcion)
                .HasMaxLength(255)
                .HasColumnName("descripcion");
            entity.Property(e => e.fecha_actualizacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.fecha_creacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.fecha_fin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");
            entity.Property(e => e.nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<estado_pago>(entity =>
        {
            entity.HasKey(e => e.id_estado_pago).HasName("PK__estado_pago");

            entity.ToTable("estado_pago", tb => tb.HasTrigger("tr_estado_pago_fecha_fin"));

            entity.Property(e => e.id_estado_pago).HasColumnName("id_estado_pago");
            entity.Property(e => e.nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
            entity.Property(e => e.descripcion)
                .HasMaxLength(255)
                .HasColumnName("descripcion");
            entity.Property(e => e.activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.fecha_creacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.fecha_actualizacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.fecha_fin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");
        });

        modelBuilder.Entity<detalle_orden>(entity =>
        {
            entity.HasKey(e => new { e.id_orden, e.id_examen });

            entity.ToTable("detalle_orden");

            entity.Property(e => e.id_orden).HasColumnName("id_orden");
            entity.Property(e => e.id_examen).HasColumnName("id_examen");
            entity.Property(e => e.precio)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("precio");

            entity.HasOne(d => d.examen_navigation).WithMany(p => p.detalle_orden)
                .HasForeignKey(d => d.id_examen)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_detalle_orden_examen");

            entity.HasOne(d => d.orden_navigation).WithMany(p => p.detalle_orden)
                .HasForeignKey(d => d.id_orden)
                .HasConstraintName("FK_detalle_orden_orden");
        });

        modelBuilder.Entity<detalle_pago>(entity =>
        {
            entity.HasKey(e => e.id_detalle_pago).HasName("PK__detalle_pago");

            entity.ToTable("detalle_pago", tb => tb.HasTrigger("tr_detalle_pago_fecha_fin"));

            entity.HasIndex(e => e.id_pago, "IX_detalle_pago_id_pago");
            entity.HasIndex(e => e.id_tipo_pago, "IX_detalle_pago_id_tipo_pago");

            entity.Property(e => e.id_detalle_pago).HasColumnName("id_detalle_pago");
            entity.Property(e => e.activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.fecha_creacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.fecha_fin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");
            entity.Property(e => e.id_pago).HasColumnName("id_pago");
            entity.Property(e => e.id_tipo_pago).HasColumnName("id_tipo_pago");
            entity.Property(e => e.monto)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("monto");
            entity.Property(e => e.numero_comprobante)
                .HasMaxLength(100)
                .HasColumnName("numero_comprobante");

            entity.HasOne(d => d.pago_navigation).WithMany(p => p.detalle_pago)
                .HasForeignKey(d => d.id_pago)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__detalle_pago_id_pago");

            entity.HasOne(d => d.tipo_pago_navigation).WithMany(p => p.detalle_pago)
                .HasForeignKey(d => d.id_tipo_pago)
                .HasConstraintName("FK_detalle_pago_tipo_pago");
        });

        modelBuilder.Entity<detalle_resultado>(entity =>
        {
            entity.HasKey(e => new { e.id_resultado, e.id_examen });

            entity.ToTable("detalle_resultado");

            entity.Property(e => e.id_resultado).HasColumnName("id_resultado");
            entity.Property(e => e.id_examen).HasColumnName("id_examen");
            entity.Property(e => e.valor)
                .HasMaxLength(100)
                .HasColumnName("valor");

            entity.HasOne(d => d.examen_navigation).WithMany(p => p.detalle_resultado)
                .HasForeignKey(d => d.id_examen)
                .HasConstraintName("FK__detalle_resultado_examen");

            entity.HasOne(d => d.resultado_navigation).WithMany(p => p.detalle_resultado)
                .HasForeignKey(d => d.id_resultado)
                .HasConstraintName("FK__detalle_resultado_resultado");
        });

        modelBuilder.Entity<examen>(entity =>
        {
            entity.HasKey(e => e.id_examen).HasName("PK__examen");

            entity.ToTable("examen", tb => tb.HasTrigger("tr_examen_fecha_fin"));

            entity.Property(e => e.id_examen).HasColumnName("id_examen");
            entity.Property(e => e.activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.fecha_actualizacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.fecha_creacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.fecha_fin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");
            entity.Property(e => e.nombre_examen)
                .HasMaxLength(255)
                .HasColumnName("nombre_examen");
            entity.Property(e => e.titulo_examen)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("titulo_examen");
            entity.Property(e => e.precio)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("precio");
            entity.Property(e => e.tiempo_entrega_minutos).HasColumnName("tiempo_entrega_minutos");
            entity.Property(e => e.id_estudio).HasColumnName("id_estudio");
            entity.Property(e => e.id_grupo_examen).HasColumnName("id_grupo_examen");
            entity.Property(e => e.id_tipo_muestra).HasColumnName("id_tipo_muestra");
            entity.Property(e => e.id_tipo_examen).HasColumnName("id_tipo_examen");
            entity.Property(e => e.id_tecnica).HasColumnName("id_tecnica");
            entity.Property(e => e.id_tipo_registro).HasColumnName("id_tipo_registro");

            entity.HasOne(d => d.estudio_navigation).WithMany(p => p.examen)
                .HasForeignKey(d => d.id_estudio)
                .HasConstraintName("FK_examen_estudio");

            entity.HasOne(d => d.grupo_examen_navigation).WithMany(p => p.examen)
                .HasForeignKey(d => d.id_grupo_examen)
                .HasConstraintName("FK_examen_grupo_examen");

            entity.HasOne(d => d.tipo_muestra_navigation).WithMany(p => p.examen)
                .HasForeignKey(d => d.id_tipo_muestra)
                .HasConstraintName("FK_examen_tipo_muestra");

            entity.HasOne(d => d.tipo_examen_navigation).WithMany(p => p.examen)
                .HasForeignKey(d => d.id_tipo_examen)
                .HasConstraintName("FK_examen_tipo_examen");

            entity.HasOne(d => d.tecnica_navigation).WithMany(p => p.examen)
                .HasForeignKey(d => d.id_tecnica)
                .HasConstraintName("FK_examen_tecnica");

            entity.HasOne(d => d.tipo_registro_navigation).WithMany(p => p.examen)
                .HasForeignKey(d => d.id_tipo_registro)
                .HasConstraintName("FK_examen_tipo_registro");
        });

        modelBuilder.Entity<examen_composicion>(entity =>
        {
            entity.HasKey(e => new { e.id_examen_padre, e.id_examen_hijo }).HasName("PK__examen_composicion");

            entity.ToTable("examen_composicion", tb => tb.HasTrigger("tr_examen_composicion_fecha_fin"));

            entity.Property(e => e.id_examen_padre).HasColumnName("id_examen_padre");
            entity.Property(e => e.id_examen_hijo).HasColumnName("id_examen_hijo");
            entity.Property(e => e.activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.fecha_actualizacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.fecha_creacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.fecha_fin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");

            entity.HasOne(d => d.examen_hijo_navigation).WithMany(p => p.examen_composicion_hijo)
                .HasForeignKey(d => d.id_examen_hijo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_examen_composicion_hijo");

            entity.HasOne(d => d.examen_padre_navigation).WithMany(p => p.examen_composicion_padre)
                .HasForeignKey(d => d.id_examen_padre)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_examen_composicion_padre");
        });

        modelBuilder.Entity<examen_reactivo>(entity =>
        {
            entity.HasKey(e => new { e.id_examen, e.id_reactivo });

            entity.ToTable("examen_reactivo", tb => tb.HasTrigger("tr_examen_reactivo_fecha_fin"));

            entity.Property(e => e.id_examen).HasColumnName("id_examen");
            entity.Property(e => e.id_reactivo).HasColumnName("id_reactivo");
            entity.Property(e => e.activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.cantidad_usada)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("cantidad_usada");
            entity.Property(e => e.fecha_actualizacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.fecha_creacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.fecha_fin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");

            entity.HasOne(d => d.examen_navigation).WithMany(p => p.examen_reactivo)
                .HasForeignKey(d => d.id_examen)
                .HasConstraintName("FK_examen_reactivo_examen");

            entity.HasOne(d => d.reactivo_navigation).WithMany(p => p.examen_reactivo)
                .HasForeignKey(d => d.id_reactivo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_examen_reactivo_reactivo");
        });

        modelBuilder.Entity<genero>(entity =>
        {
            entity.HasKey(e => e.id_genero).HasName("PK__genero");

            entity.ToTable("genero");

            entity.HasIndex(e => e.nombre, "UQ_genero_nombre").IsUnique();

            entity.Property(e => e.id_genero).HasColumnName("id_genero");
            entity.Property(e => e.activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.descripcion)
                .HasMaxLength(100)
                .HasColumnName("descripcion");
            entity.Property(e => e.fecha_actualizacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.fecha_creacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<medico>(entity =>
        {
            entity.HasKey(e => e.id_medico).HasName("PK__medico");

            entity.ToTable("medico", tb => tb.HasTrigger("tr_medico_fecha_fin"));

            entity.HasIndex(e => e.correo, "UQ__medico").IsUnique();

            entity.Property(e => e.id_medico).HasColumnName("id_medico");
            entity.Property(e => e.activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.correo)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("correo");
            entity.Property(e => e.especialidad)
                .HasMaxLength(100)
                .HasColumnName("especialidad");
            entity.Property(e => e.fecha_actualizacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.fecha_creacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.fecha_fin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");
            entity.Property(e => e.nombre_medico)
                .HasMaxLength(100)
                .HasColumnName("nombre_medico");
            entity.Property(e => e.telefono)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("telefono");
        });

        modelBuilder.Entity<movimiento_reactivo>(entity =>
        {
            entity.HasKey(e => e.id_movimiento_reactivo).HasName("PK__movimiento_reactivo");

            entity.ToTable("movimiento_reactivo");

            entity.Property(e => e.id_movimiento_reactivo).HasColumnName("id_movimiento_reactivo");
            entity.Property(e => e.cantidad)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("cantidad");
            entity.Property(e => e.fecha_movimiento)
                .HasColumnType("datetime")
                .HasColumnName("fecha_movimiento");
            entity.Property(e => e.id_detalle_resultado).HasColumnName("id_detalle_resultado");
            entity.Property(e => e.id_examen).HasColumnName("id_examen");
            entity.Property(e => e.id_reactivo).HasColumnName("id_reactivo");
            entity.Property(e => e.id_resultado).HasColumnName("id_resultado");
            entity.Property(e => e.observacion)
                .HasMaxLength(255)
                .HasColumnName("observacion");
            entity.Property(e => e.tipo_movimiento)
                .HasMaxLength(50)
                .HasColumnName("tipo_movimiento");

            entity.HasOne(d => d.reactivo_navigation).WithMany(p => p.movimiento_reactivo)
                .HasForeignKey(d => d.id_reactivo)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__movimiento_reactivo_reactivo");

            entity.HasOne(d => d.detalle_resultado_navigation).WithMany(p => p.movimiento_reactivo)
                .HasForeignKey(d => new { d.id_resultado, d.id_examen })
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_movimiento_reactivo_detalle_resultado");
        });

        modelBuilder.Entity<orden>(entity =>
        {
            entity.HasKey(e => e.id_orden).HasName("PK__orden");

            entity.ToTable("orden", tb => tb.HasTrigger("tr_orden_fecha_fin"));

            entity.HasIndex(e => e.numero_orden, "UQ__orden").IsUnique();

            entity.HasIndex(e => e.id_estado_pago, "IX_orden_id_estado_pago");

            entity.Property(e => e.id_orden).HasColumnName("id_orden");
            entity.Property(e => e.activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.id_estado_orden)
                .HasDefaultValue(1)
                .HasColumnName("id_estado_orden");
            entity.Property(e => e.id_estado_pago).HasColumnName("id_estado_pago");
            entity.Property(e => e.fecha_fin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");
            entity.Property(e => e.fecha_completado)
                .HasColumnType("datetime")
                .HasColumnName("fecha_completado");
            entity.Property(e => e.fecha_orden).HasColumnName("fecha_orden");
            entity.Property(e => e.id_convenio).HasColumnName("id_convenio");
            entity.Property(e => e.id_medico).HasColumnName("id_medico");
            entity.Property(e => e.id_paciente).HasColumnName("id_paciente");
            entity.Property(e => e.numero_orden)
                .HasMaxLength(50)
                .HasColumnName("numero_orden");
            entity.Property(e => e.observacion)
                .HasMaxLength(255)
                .HasColumnName("observacion");
            entity.Property(e => e.saldo_pendiente)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("saldo_pendiente");
            entity.Property(e => e.total)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("total");

            entity.HasOne(d => d.convenio_navigation).WithMany(p => p.orden)
                .HasForeignKey(d => d.id_convenio)
                .HasConstraintName("FK_orden_convenio");

            entity.HasOne(d => d.medico_navigation).WithMany(p => p.orden)
                .HasForeignKey(d => d.id_medico)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__orden_medico");

            entity.HasOne(d => d.paciente_navigation).WithMany(p => p.orden)
                .HasForeignKey(d => d.id_paciente)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__orden_paciente");

            entity.HasOne(d => d.estado_orden_navigation).WithMany(p => p.orden)
                .HasForeignKey(d => d.id_estado_orden)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_orden_estado_orden");

            entity.HasOne(d => d.estado_pago_navigation).WithMany(p => p.orden)
                .HasForeignKey(d => d.id_estado_pago)
                .HasConstraintName("FK_orden_estado_pago");
        });

        modelBuilder.Entity<paciente>(entity =>
        {
            entity.HasKey(e => e.id_paciente).HasName("PK__paciente");

            entity.ToTable("paciente", tb => tb.HasTrigger("tr_paciente_fecha_fin"));

            entity.Property(e => e.id_paciente).HasColumnName("id_paciente");
            entity.Property(e => e.activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.fecha_actualizacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.fecha_creacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.fecha_fin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");
            entity.Property(e => e.id_persona).HasColumnName("id_persona");

            entity.HasOne(d => d.persona_navigation).WithMany(p => p.paciente)
                .HasForeignKey(d => d.id_persona)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_paciente_persona");
        });

        modelBuilder.Entity<pago>(entity =>
        {
            entity.HasKey(e => e.id_pago).HasName("PK__pago");

            entity.ToTable("pago", tb => tb.HasTrigger("tr_pago_fecha_fin"));

            entity.HasIndex(e => e.fecha_pago, "idx_pago_fecha");

            entity.Property(e => e.id_pago).HasColumnName("id_pago");
            entity.Property(e => e.activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.fecha_fin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");
            entity.Property(e => e.fecha_pago)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_pago");
            entity.Property(e => e.id_orden).HasColumnName("id_orden");
            entity.Property(e => e.monto_recibido)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("monto_recibido");
            entity.Property(e => e.monto_aplicado)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("monto_aplicado");
            entity.Property(e => e.monto_vuelto)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("monto_vuelto");
            entity.Property(e => e.observacion)
                .HasMaxLength(255)
                .HasColumnName("observacion");

            entity.HasOne(d => d.orden_navigation).WithMany(p => p.pago)
                .HasForeignKey(d => d.id_orden)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__pago_orden");
        });

        modelBuilder.Entity<reactivo>(entity =>
        {
            entity.HasKey(e => e.id_reactivo).HasName("PK__reactivo");

            entity.ToTable("reactivo", tb => tb.HasTrigger("tr_reactivo_fecha_fin"));

            entity.Property(e => e.id_reactivo).HasColumnName("id_reactivo");
            entity.Property(e => e.activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.cantidad_disponible)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("cantidad_disponible");
            entity.Property(e => e.fabricante)
                .HasMaxLength(100)
                .HasColumnName("fabricante");
            entity.Property(e => e.fecha_actualizacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.fecha_creacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.fecha_fin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");
            entity.Property(e => e.nombre_reactivo)
                .HasMaxLength(100)
                .HasColumnName("nombre_reactivo");
            entity.Property(e => e.unidad)
                .HasMaxLength(50)
                .HasColumnName("unidad");
        });

        modelBuilder.Entity<estado_resultado>(entity =>
        {
            entity.HasKey(e => e.id_estado_resultado).HasName("PK__estado_resultado");

            entity.ToTable("estado_resultado");

            entity.Property(e => e.id_estado_resultado).HasColumnName("id_estado_resultado");
            entity.Property(e => e.activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.descripcion)
                .HasMaxLength(255)
                .HasColumnName("descripcion");
            entity.Property(e => e.fecha_actualizacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.fecha_creacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.fecha_fin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");
            entity.Property(e => e.nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<resultado>(entity =>
        {
            entity.HasKey(e => e.id_resultado).HasName("PK__resultado");

            entity.ToTable("resultado", tb => tb.HasTrigger("tr_resultado_fecha_fin"));

            entity.Property(e => e.id_resultado).HasColumnName("id_resultado");
            entity.Property(e => e.activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.id_estado_resultado)
                .HasDefaultValue(1)
                .HasColumnName("id_estado_resultado");
            entity.Property(e => e.fecha_fin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");
            entity.Property(e => e.fecha_resultado)
                .HasColumnType("datetime")
                .HasColumnName("fecha_resultado");
            entity.Property(e => e.fecha_revision)
                .HasColumnType("datetime")
                .HasColumnName("fecha_revision");
            entity.Property(e => e.id_orden).HasColumnName("id_orden");
            entity.Property(e => e.id_revisor).HasColumnName("id_revisor");
            entity.Property(e => e.numero_resultado)
                .HasMaxLength(50)
                .HasColumnName("numero_resultado");
            entity.Property(e => e.observacion_revision)
                .HasColumnType("text")
                .HasColumnName("observacion_revision");
            entity.Property(e => e.observaciones)
                .HasMaxLength(255)
                .HasColumnName("observaciones");

            entity.HasOne(d => d.orden_navigation).WithMany(p => p.resultado)
                .HasForeignKey(d => d.id_orden)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_resultado_orden");

            entity.HasOne(d => d.revisor_navigation).WithMany(p => p.resultado)
                .HasForeignKey(d => d.id_revisor)
                .HasConstraintName("FK_resultado_revisor");

            entity.HasOne(d => d.estado_resultado_navigation).WithMany(p => p.resultado)
                .HasForeignKey(d => d.id_estado_resultado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_resultado_estado_resultado");
        });

        modelBuilder.Entity<tokens_usuarios>(entity =>
        {
            entity.HasKey(e => e.id_token).HasName("PK__tokens_usuarios");

            entity.ToTable("tokens_usuarios");

            entity.HasIndex(e => e.fecha_expiracion, "IX_recup_expiracion");

            entity.HasIndex(e => new { e.id_usuario, e.usado }, "IX_recup_usuario_usado");

            entity.HasIndex(e => e.token_hash, "UQ_recup_token_hash").IsUnique();

            entity.Property(e => e.id_token).HasColumnName("id_token");
            entity.Property(e => e.dispositivo_origen)
                .HasMaxLength(256)
                .HasColumnName("dispositivo_origen");
            entity.Property(e => e.fecha_expiracion)
                .HasPrecision(0)
                .HasColumnName("fecha_expiracion");
            entity.Property(e => e.fecha_solicitud)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("fecha_solicitud");
            entity.Property(e => e.id_usuario).HasColumnName("id_usuario");
            entity.Property(e => e.ip_solicitud)
                .HasMaxLength(45)
                .HasColumnName("ip_solicitud");
            entity.Property(e => e.tipo_token)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("tipo_token");
            entity.Property(e => e.token_hash)
                .HasMaxLength(32)
                .HasColumnName("token_hash");
            entity.Property(e => e.usado).HasColumnName("usado");
            entity.Property(e => e.usado_en)
                .HasPrecision(0)
                .HasColumnName("usado_en");

            entity.HasOne(d => d.usuario_navigation).WithMany(p => p.tokens_usuarios)
                .HasForeignKey(d => d.id_usuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tokens_usuario");
        });

        modelBuilder.Entity<usuario>(entity =>
        {
            entity.HasKey(e => e.id_usuario).HasName("PK__usuario");

            entity.ToTable("usuario", tb => tb.HasTrigger("tr_usuario_fecha_fin"));

            entity.HasIndex(e => e.correo, "UQ_usuario_correo").IsUnique();

            entity.Property(e => e.id_usuario).HasColumnName("id_usuario");
            entity.Property(e => e.activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.correo)
                .HasMaxLength(150)
                .HasColumnName("correo");
            entity.Property(e => e.password_hash)
                .HasMaxLength(255)
                .IsRequired(false)
                .HasColumnName("password_hash");
            entity.Property(e => e.fecha_actualizacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.fecha_creacion)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.fecha_fin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");
            entity.Property(e => e.id_rol).HasColumnName("id_rol");
            entity.Property(e => e.id_persona).HasColumnName("id_persona");
            entity.Property(e => e.ultimo_acceso).HasColumnName("ultimo_acceso");

            entity.HasOne(d => d.rol_navigation).WithMany(p => p.usuario)
                .HasForeignKey(d => d.id_rol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_usuario_rol");

            entity.HasOne(d => d.persona_navigation).WithMany(p => p.usuario)
                .HasForeignKey(d => d.id_persona)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_usuario_persona");
        });

        modelBuilder.Entity<rol>(entity =>
        {
            entity.HasKey(e => e.id_rol).HasName("PK__rol");

            entity.ToTable("rol");

            entity.HasIndex(e => e.nombre, "UQ_rol_nombre").IsUnique();

            entity.Property(e => e.id_rol).HasColumnName("id_rol");
            entity.Property(e => e.descripcion)
                .HasMaxLength(100)
                .HasColumnName("descripcion");
            entity.Property(e => e.fecha_actualizacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.fecha_creacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
