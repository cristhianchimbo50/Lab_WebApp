using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_Contracts.Ordenes
{
    public class OrdenRegistroDto
    {
        public OrdenDto Orden { get; set; }
        public List<int> IdsExamenes { get; set; }
        public PagoDto Pago { get; set; }
        public List<DetallePagoDto> DetallesPago { get; set; }
    }
}

