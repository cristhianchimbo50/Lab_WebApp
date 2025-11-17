using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class Paciente
{
    public int IdPaciente { get; set; }

    public string CedulaPaciente { get; set; } = null!;

    public string NombrePaciente { get; set; } = null!;

    public DateOnly FechaNacPaciente { get; set; }

    public string? DireccionPaciente { get; set; }

    public string? CorreoElectronicoPaciente { get; set; }

    public string? TelefonoPaciente { get; set; }

    public int? IdUsuario { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaFin { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public bool Activo { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }

    public virtual ICollection<Orden> Orden { get; set; } = new List<Orden>();
}
