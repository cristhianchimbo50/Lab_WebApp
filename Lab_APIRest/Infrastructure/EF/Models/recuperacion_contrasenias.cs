using System;

namespace Lab_APIRest.Infrastructure.EF.Models
{
    public partial class recuperacion_contrasenias
    {
        public long id_recuperacion { get; set; }
        public int id_usuario { get; set; }
        public byte[] token_hash { get; set; } = null!;
        public DateTime fecha_solicitud { get; set; }
        public DateTime fecha_expiracion { get; set; }
        public bool usado { get; set; }
        public DateTime? usado_en { get; set; }
        public string? ip_solicitud { get; set; }
        public string? user_agent { get; set; }

        public virtual usuario Usuario { get; set; } = null!;
    }
}
