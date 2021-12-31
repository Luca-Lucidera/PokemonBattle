using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattagliaPokemon
{
    class PokemonScelti
    {
        private List<Pokemon> ps;
        public PokemonScelti()
        {
            ps = new List<Pokemon>();
        }
        public void addPokemon(Pokemon pokemon)
        {
            if (!ps.Contains(pokemon))
                ps.Add(pokemon);
        }
        public string showAll()
        {
            string s = string.Empty;
            for (int i = 0; i < ps.Count; i++)
                s += ps.ElementAt(i).nome + " ";
            return s;
        }

        public string getPokemonByPos(int index)
        {
            return ps.ElementAt(index).nome;
        }
    }
}
