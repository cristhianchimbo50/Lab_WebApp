using System.Collections.Generic;

namespace Lab_Contracts.Ordenes
{
    public class OrdenCompletaDto
    {
        public OrdenDto Orden { get; set; } = new();
        public List<int> IdsExamenes { get; set; } = new();
    }


}

