using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattagliaPokemon
{
    class PokemonScelti
    {
        private List<Pokemon> pokemonScelti;
        public PokemonScelti()
        {
            pokemonScelti = new List<Pokemon>();
        }
        public void addPokemon(Pokemon pokemon)
        {
            if (!pokemonScelti.Contains(pokemon))
                pokemonScelti.Add(pokemon);
        }
        public string showAll()
        {
            string s = string.Empty;
            for (int i = 0; i < pokemonScelti.Count; i++)
                s += pokemonScelti.ElementAt(i).nome + " ";
            return s;
        }

        public Pokemon getPokemonByPos(int index)
        {
            return pokemonScelti.ElementAt(index);
        }
        public int getNumeroPokemonScelti()
        {
            return pokemonScelti.Count;
        }
    }
}
