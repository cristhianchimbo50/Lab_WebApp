using System;
using System.Collections.Generic;
using Lab_APIRest.Infrastructure.EF.Models;
using Microsoft.EntityFrameworkCore;

namespace Lab_APIRest.Infrastructure.EF;

public partial class LabDbContext : DbContext
{
    public LabDbContext(DbContextOptions<LabDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<convenio> convenios { get; set; }

    public virtual DbSet<detalle_convenio> detalle_convenios { get; set; }

    public virtual DbSet<detalle_orden> detalle_ordens { get; set; }

    public virtual DbSet<detalle_pago> detalle_pagos { get; set; }

    public virtual DbSet<detalle_resultado> detalle_resultados { get; set; }

    public virtual DbSet<examen> examen { get; set; }
    public virtual DbSet<examen_composicion> examen_composicion { get; set; }

    public virtual DbSet<examen_reactivo> examen_reactivos { get; set; }

    public virtual DbSet<medico> medicos { get; set; }

    public virtual DbSet<movimiento_reactivo> movimiento_reactivos { get; set; }

    public virtual DbSet<orden> ordens { get; set; }

    public virtual DbSet<paciente> pacientes { get; set; }

    public virtual DbSet<pago> pagos { get; set; }

    public virtual DbSet<reactivo> reactivos { get; set; }

    public virtual DbSet<resultado> resultados { get; set; }

    public virtual DbSet<usuario> usuarios { get; set; }

    public virtual DbSet<usuario_token_activacion> usuario_token_activacions { get; set; }

    public virtual DbSet<v_paciente> v_pacientes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<convenio>(entity =>
        {
            entity.HasKey(e => e.id_convenio).HasName("PK__convenio__177BD43EA106A132");

            entity.ToTable("convenio");

            entity.Property(e => e.anulado).HasDefaultValue(false);
            entity.Property(e => e.monto_total).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.porcentaje_comision).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.id_medicoNavigation).WithMany(p => p.convenios)
                .HasForeignKey(d => d.id_medico)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__convenio__id_med__52593CB8");
        });

        modelBuilder.Entity<detalle_convenio>(entity =>
        {
            entity.HasKey(e => e.id_detalle_convenio).HasName("PK__detalle___8AD8083EE71E99F4");

            entity.ToTable("detalle_convenio");

            entity.Property(e => e.subtotal).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.id_convenioNavigation).WithMany(p => p.detalle_convenios)
                .HasForeignKey(d => d.id_convenio)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__detalle_c__id_co__5EBF139D");

            entity.HasOne(d => d.id_ordenNavigation).WithMany(p => p.detalle_convenios)
                .HasForeignKey(d => d.id_orden)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_detalle_convenio_orden");
        });

        modelBuilder.Entity<detalle_orden>(entity =>
        {
            entity.HasKey(e => e.id_detalle_orden).HasName("PK__detalle___D2FC3FD781609604");

            entity.ToTable("detalle_orden");

            entity.Property(e => e.precio).HasColumnType("decimal(10, 2)");

            entity.Property(e => e.anulado).HasDefaultValue(false);

            entity.HasOne(d => d.id_examenNavigation).WithMany(p => p.detalle_ordens)
                .HasForeignKey(d => d.id_examen)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__detalle_o__id_ex__0D7A0286");

            entity.HasOne(d => d.id_ordenNavigation).WithMany(p => p.detalle_ordens)
                .HasForeignKey(d => d.id_orden)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__detalle_o__id_or__0C85DE4D");

            entity.HasOne(d => d.id_resultadoNavigation).WithMany(p => p.detalle_ordens)
                .HasForeignKey(d => d.id_resultado)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__detalle_o__id_re__0E6E26BF");
        });

        modelBuilder.Entity<detalle_pago>(entity =>
        {
            entity.HasKey(e => e.id_detalle_pago).HasName("PK__detalle___55C3EFACDE063256");

            entity.ToTable("detalle_pago");

            entity.Property(e => e.monto).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.tipo_pago).HasMaxLength(50);

            entity.HasOne(d => d.id_pagoNavigation).WithMany(p => p.detalle_pagos)
                .HasForeignKey(d => d.id_pago)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__detalle_p__id_pa__00200768");
        });

        modelBuilder.Entity<detalle_resultado>(entity =>
        {
            entity.HasKey(e => e.id_detalle_resultado).HasName("PK__detalle___E4307FE10B18D938");

            entity.ToTable("detalle_resultado");

            entity.HasIndex(e => e.id_examen, "idx_detalle_resultado_examen");

            entity.Property(e => e.anulado).HasDefaultValue(false);
            entity.Property(e => e.observacion).HasMaxLength(255);
            entity.Property(e => e.unidad).HasMaxLength(50);
            entity.Property(e => e.valor).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.valor_referencia).HasMaxLength(100);

            entity.HasOne(d => d.id_examenNavigation).WithMany(p => p.detalle_resultados)
                .HasForeignKey(d => d.id_examen)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__detalle_r__id_ex__1CBC4616");

            entity.HasOne(d => d.id_resultadoNavigation).WithMany(p => p.detalle_resultados)
                .HasForeignKey(d => d.id_resultado)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__detalle_r__id_re__1BC821DD");
        });

        modelBuilder.Entity<examen>(entity =>
        {
            entity.HasKey(e => e.id_examen).HasName("PK__examen__D16A231D8ED13AFD");

            entity.Property(e => e.anulado).HasDefaultValue(false);
            entity.Property(e => e.estudio)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.nombre_examen).HasMaxLength(255);
            entity.Property(e => e.precio).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.tecnica)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.tiempo_entrega)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.tipo_examen)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.tipo_muestra)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.titulo_examen)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.unidad).HasMaxLength(50);
            entity.Property(e => e.valor_referencia).HasMaxLength(100);
        });

        modelBuilder.Entity<examen_composicion>(entity =>
        {
            entity.HasKey(e => new { e.id_examen_padre, e.id_examen_hijo });
            entity.ToTable("examen_composicion");

            entity.HasOne(d => d.id_examen_padreNavigation)
                .WithMany(p => p.id_examen_hijos)
                .HasForeignKey(d => d.id_examen_padre)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.id_examen_hijoNavigation)
                .WithMany(p => p.id_examen_padres)
                .HasForeignKey(d => d.id_examen_hijo)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });


        modelBuilder.Entity<examen_reactivo>(entity =>
        {
            entity.HasKey(e => e.id_examen_reactivo).HasName("PK__examen_r__ECE10F430B823FC7");

            entity.ToTable("examen_reactivo");

            entity.Property(e => e.cantidad_usada).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.unidad).HasMaxLength(50);

            entity.HasOne(d => d.id_examenNavigation).WithMany(p => p.examen_reactivos)
                .HasForeignKey(d => d.id_examen)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__examen_re__id_ex__07C12930");

            entity.HasOne(d => d.id_reactivoNavigation).WithMany(p => p.examen_reactivos)
                .HasForeignKey(d => d.id_reactivo)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__examen_re__id_re__08B54D69");
        });

        modelBuilder.Entity<medico>(entity =>
        {
            entity.HasKey(e => e.id_medico).HasName("PK__medico__E038EB43167721F0");

            entity.ToTable("medico");

            entity.HasIndex(e => e.correo, "UQ__medico__2A586E0B86B58ECE").IsUnique();

            entity.Property(e => e.anulado).HasDefaultValue(false);
            entity.Property(e => e.correo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.especialidad).HasMaxLength(100);
            entity.Property(e => e.nombre_medico).HasMaxLength(100);
            entity.Property(e => e.telefono)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<movimiento_reactivo>(entity =>
        {
            entity.HasKey(e => e.id_movimiento_reactivo).HasName("PK__movimien__6CA263B2FDAE2E0C");

            entity.ToTable("movimiento_reactivo");

            entity.Property(e => e.cantidad).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.fecha_movimiento).HasColumnType("datetime");
            entity.Property(e => e.observacion).HasMaxLength(255);
            entity.Property(e => e.tipo_movimiento).HasMaxLength(50);

            entity.HasOne(d => d.id_ordenNavigation).WithMany(p => p.movimiento_reactivos)
                .HasForeignKey(d => d.id_orden)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__movimient__id_or__04E4BC85");

            entity.HasOne(d => d.id_reactivoNavigation).WithMany(p => p.movimiento_reactivos)
                .HasForeignKey(d => d.id_reactivo)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__movimient__id_re__03F0984C");
        });

        modelBuilder.Entity<orden>(entity =>
        {
            entity.HasKey(e => e.id_orden).HasName("PK__orden__DD5B8F3315A23942");

            entity.ToTable("orden");

            entity.HasIndex(e => e.numero_orden, "UQ__orden__3706711555B8ACCE").IsUnique();

            entity.HasIndex(e => e.estado_pago, "idx_orden_estado_pago");

            entity.Property(e => e.anulado).HasDefaultValue(false);
            entity.Property(e => e.estado_pago).HasMaxLength(20);
            entity.Property(e => e.liquidado_convenio).HasDefaultValue(false);
            entity.Property(e => e.numero_orden).HasMaxLength(50);
            entity.Property(e => e.observacion).HasMaxLength(255);
            entity.Property(e => e.saldo_pendiente)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.total).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.total_pagado)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.id_medicoNavigation).WithMany(p => p.ordens)
                .HasForeignKey(d => d.id_medico)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__orden__id_medico__4E88ABD4");

            entity.HasOne(d => d.id_pacienteNavigation).WithMany(p => p.ordens)
                .HasForeignKey(d => d.id_paciente)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__orden__id_pacien__48CFD27E");
        });

        modelBuilder.Entity<paciente>(entity =>
        {
            entity.HasKey(e => e.id_paciente).HasName("PK__paciente__2C2C72BB0EBF26E8");

            entity.ToTable("paciente");

            entity.HasIndex(e => e.cedula_paciente, "UQ__paciente__4DE187B18A0DA1EB").IsUnique();

            entity.HasIndex(e => e.cedula_paciente, "UQ_paciente_cedula_paciente").IsUnique();

            entity.HasIndex(e => e.nombre_paciente, "idx_paciente_nombre");

            entity.Property(e => e.anulado).HasDefaultValue(false);
            entity.Property(e => e.cedula_paciente)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.correo_electronico_paciente)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.direccion_paciente).HasMaxLength(150);
            entity.Property(e => e.fecha_registro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.nombre_paciente).HasMaxLength(100);
            entity.Property(e => e.telefono_paciente)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.id_usuarioNavigation).WithMany(p => p.pacientes)
                .HasForeignKey(d => d.id_usuario)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__paciente__id_usu__44FF419A");
        });

        modelBuilder.Entity<pago>(entity =>
        {
            entity.HasKey(e => e.id_pago).HasName("PK__pago__0941B074D5DF9BAD");

            entity.ToTable("pago");

            entity.HasIndex(e => e.fecha_pago, "idx_pago_fecha");

            entity.Property(e => e.anulado).HasDefaultValue(false);
            entity.Property(e => e.fecha_pago)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.monto_pagado).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.observacion).HasMaxLength(255);

            entity.HasOne(d => d.id_ordenNavigation).WithMany(p => p.pagos)
                .HasForeignKey(d => d.id_orden)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__pago__id_orden__619B8048");
        });

        modelBuilder.Entity<reactivo>(entity =>
        {
            entity.HasKey(e => e.id_reactivo).HasName("PK__reactivo__EC691887291B11E4");

            entity.ToTable("reactivo");

            entity.Property(e => e.anulado).HasDefaultValue(false);
            entity.Property(e => e.cantidad_disponible)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.fabricante).HasMaxLength(100);
            entity.Property(e => e.nombre_reactivo).HasMaxLength(100);
            entity.Property(e => e.unidad).HasMaxLength(50);
        });

        modelBuilder.Entity<resultado>(entity =>
        {
            entity.HasKey(e => e.id_resultado).HasName("PK__resultad__33A42B30A58D4867");

            entity.ToTable("resultado");

            entity.Property(e => e.anulado).HasDefaultValue(false);
            entity.Property(e => e.fecha_resultado).HasColumnType("datetime");
            entity.Property(e => e.numero_resultado).HasMaxLength(50);
            entity.Property(e => e.observaciones).HasMaxLength(255);

            entity.HasOne(d => d.id_ordenNavigation).WithMany(p => p.resultados)
                .HasForeignKey(d => d.id_orden)
                .HasConstraintName("FK__resultado__id_or__7C4F7684");

            entity.HasOne(d => d.id_pacienteNavigation).WithMany(p => p.resultados)
                .HasForeignKey(d => d.id_paciente)
                .HasConstraintName("FK__resultado__id_pa__7A672E12");
        });

        modelBuilder.Entity<usuario>(entity =>
        {
            entity.HasKey(e => e.id_usuario).HasName("PK__usuario__4E3E04AD8F3B8033");

            entity.ToTable("usuario");

            entity.HasIndex(e => e.correo_usuario, "UQ__usuario__CD54AB1C29B4F762").IsUnique();

            entity.HasIndex(e => e.correo_usuario, "UQ_usuario_correo_usuario")
                .IsUnique()
                .HasFilter("([correo_usuario] IS NOT NULL)");

            entity.HasIndex(e => e.correo_usuario, "idx_usuario_correo");

            entity.Property(e => e.clave_usuario)
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.correo_usuario)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.es_contraseña_temporal).HasDefaultValue(true);
            entity.Property(e => e.estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("ACTIVO");
            entity.Property(e => e.estado_registro).HasDefaultValue(false);
            entity.Property(e => e.nombre).HasMaxLength(255);
            entity.Property(e => e.rol)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<usuario_token_activacion>(entity =>
        {
            entity.HasKey(e => e.id_token).HasName("PK__usuario___3C2FA9C416545FD3");

            entity.ToTable("usuario_token_activacion");

            entity.HasIndex(e => new { e.id_usuario, e.usado, e.expira_en }, "IX_TokenUsuario_Activo");

            entity.Property(e => e.emitido_en).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.token_hash)
                .HasMaxLength(64)
                .IsUnicode(false)
                .IsFixedLength();

            entity.HasOne(d => d.id_usuarioNavigation).WithMany(p => p.usuario_token_activacions)
                .HasForeignKey(d => d.id_usuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TokenUsuario");
        });

        modelBuilder.Entity<v_paciente>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_paciente");

            entity.Property(e => e.cedula_paciente)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.correo_electronico_paciente)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.direccion_paciente).HasMaxLength(150);
            entity.Property(e => e.fecha_registro).HasColumnType("datetime");
            entity.Property(e => e.id_paciente).ValueGeneratedOnAdd();
            entity.Property(e => e.nombre_paciente).HasMaxLength(100);
            entity.Property(e => e.telefono_paciente)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}