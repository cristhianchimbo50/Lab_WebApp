using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_Contracts.Ajustes.Perfil
{
    public class PerfilResponseDto
    {
        public PerfilDto Perfil { get; set; } = new();
        public bool EsSoloLectura => true;
    }
}