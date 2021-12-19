using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;

namespace BattagliaPokemon
{
    class Pokemon
    {
        public string nome { set; get; }
        public Texture2D front { set; get; }
        public Texture2D retro { set; get; }
        public Vector2 posizione { set; get; }
        public Pokemon()
        {
            nome = "";
            front = null;
            retro = null;
            posizione = new Vector2(-1, -1);
        }
    }
}
