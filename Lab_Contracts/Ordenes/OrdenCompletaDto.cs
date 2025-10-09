using System.ComponentModel.DataAnnotations;

namespace Lab_Contracts.Ordenes
{
    public class OrdenCompletaDto
    {
        public OrdenDto Orden { get; set; }
        public List<int> IdsExamenes { get; set; }
    }


}

