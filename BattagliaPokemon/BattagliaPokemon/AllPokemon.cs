using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;

namespace BattagliaPokemon
{
    class AllPokemon
    {
        private Pokemon[] pokemon;
        public AllPokemon(Game1 g, GraphicsDeviceManager graphics)
        {
            //OGNI POKEMON È 65*65px
            string[] FilesFront = Directory.GetFiles("./Content/Pokemon/Front/"); //prendo il pathName di dove ho inserito i pokemon
            string[] FilesRetro = Directory.GetFiles("./Content/Pokemon/Retro/"); //prendo il pathName di dove ho inserito i pokemon
            pokemon = new Pokemon[FilesFront.Length];
            for (int i = 0; i < pokemon.Length; i++)
            {
                pokemon[i] = new Pokemon();
            }
            int x = 0;
            int y = 0;
            for (int i = 0; i < FilesFront.Length; i++)
            {
                string tmp = Path.GetFileNameWithoutExtension(FilesFront[i]);
                int pos = tmp.IndexOf("Front");
                pokemon[i].nome = tmp.Substring(0, pos);
                pokemon[i].front = g.Content.Load<Texture2D>("Pokemon/Front/"+Path.GetFileNameWithoutExtension(FilesFront[i])); //li prendo uno a uno
                pokemon[i].retro = g.Content.Load<Texture2D>("Pokemon/Retro/"+Path.GetFileNameWithoutExtension(FilesRetro[i])); //li prendo uno a uno
                if(graphics.PreferredBackBufferWidth <= x)
                {
                    y += 65;
                    x = 0;
                }
                pokemon[i].posizione = new Vector2(x, y);
                x+=65;
            }
        }
        public Pokemon[] getPokemon()
        {
            return pokemon;
        }
    }
}
