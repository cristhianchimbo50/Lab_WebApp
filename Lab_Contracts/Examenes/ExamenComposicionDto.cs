namespace Lab_Contracts.Examenes
{
    public class ExamenComposicionDto
    {
        public int IdExamenPadre { get; set; }
        public int IdExamenHijo { get; set; }
        public string? NombreExamenPadre { get; set; }
        public string? NombreExamenHijo { get; set; }
    }
}
