using Microsoft.Xna.Framework.Input;
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

        public Pokemon cambiaPokemon(MouseState mouseState)
        {
            int xAdder = 0;
            for (int i = 0; i < pokemonScelti.Count; i++)
            {
                if (mouseState.X >= pokemonScelti.ElementAt(i).posizione.X &&
                    mouseState.X <= pokemonScelti.ElementAt(i).posizione.X + 65 &&
                    mouseState.Y >= pokemonScelti.ElementAt(i).posizione.Y &&
                    mouseState.Y <= pokemonScelti.ElementAt(i).posizione.Y + 65)
                    return pokemonScelti.ElementAt(i);
            }
            return null;
        }
    }
}
