using System;
using System.Collections.Generic;
using System.Text;

namespace BattagliaPokemon
{
    class Mossa
    {
        string nome { set; get; }
        string tipo { set; get; }
        int danni { set; get; }
        int utilizzi { set; get; }

        public Mossa(string nome, string tipo, int danni, int utilizzi)
        {
            this.nome = nome;
            this.tipo = tipo;
            this.danni = danni;
            this.utilizzi = utilizzi;
        }
    }
}
