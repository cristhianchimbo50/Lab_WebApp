namespace Lab_APIRest.Infrastructure.EF.Models
{
    public class examen_composicion
    {
        public int id_examen_padre { get; set; }
        public int id_examen_hijo { get; set; }

        public examen? id_examen_padreNavigation { get; set; }
        public examen? id_examen_hijoNavigation { get; set; }
    }
}
