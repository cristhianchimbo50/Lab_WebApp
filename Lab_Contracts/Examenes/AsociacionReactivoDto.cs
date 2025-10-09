namespace Lab_Contracts.Examenes
{
    public class AsociacionReactivoDto
    {
        public int IdExamenReactivo { get; set; }
        public int IdExamen { get; set; }
        public string? NombreExamen { get; set; }
        public int IdReactivo { get; set; }
        public string? NombreReactivo { get; set; }
        public decimal CantidadUsada { get; set; }
        public string? Unidad { get; set; }
    }
}
