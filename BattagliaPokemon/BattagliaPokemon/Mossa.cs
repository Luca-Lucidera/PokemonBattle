using System;
using System.Collections.Generic;
using System.Text;

namespace BattagliaPokemon
{
    public class Mossa
    {
        public string nome { set; get; }
        public string tipo { set; get; }
        public int danni { set; get; }
        public int utilizzi { set; get; }

        public Mossa(string nome, string tipo, int danni, int utilizzi)
        {
            this.nome = nome;
            this.tipo = tipo;
            this.danni = danni;
            this.utilizzi = utilizzi;
        }
        public Mossa()
        {
            nome = "";
            tipo = "";
            danni = 0;
            utilizzi = 0;
        }

        public override string ToString()
        {
            return "Nome " + nome + " tipo: " + tipo + " danni: " + danni + " utilizziMax: " + utilizzi;
        }
    }
}
