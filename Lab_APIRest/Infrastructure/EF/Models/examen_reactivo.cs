using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class examen_reactivo
{
    public int id_examen_reactivo { get; set; }

    public int? id_examen { get; set; }

    public int? id_reactivo { get; set; }

    public decimal? cantidad_usada { get; set; }

    public string? unidad { get; set; }

    public virtual examen? id_examenNavigation { get; set; }

    public virtual reactivo? id_reactivoNavigation { get; set; }
}
