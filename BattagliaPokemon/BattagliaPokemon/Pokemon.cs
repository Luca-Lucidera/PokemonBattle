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
        public Mossa[] mosse { set; get; }
        public Pokemon()
        {
            nome = "";
            front = null;
            retro = null;
            posizione = new Vector2(-1, -1);
            mosse = new Mossa[4];
            for (int i = 0; i < mosse.Length; i++)
            {
                mosse[i] = null;
            }
        }
        public void aggiungiMossa(Mossa m)
        {
            for (int i = 0; i < mosse.Length; i++)
            {
                if (mosse[i] == null)
                {
                    mosse[i] = m;
                    break;
                }
            }
        }
    }
}
