using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;

namespace BattagliaPokemon
{
    public class Pokemon
    {
        public string nome { set; get; }
        public string tipo { set; get; }
        public Texture2D front { set; get; }
        public Texture2D retro { set; get; }
        public Vector2 posizione { set; get; }
        public Mossa[] mosse { set; get; }
        public int vita { set; get; }
        public Pokemon()
        {
            tipo = "";
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
        public Pokemon(string nome, Texture2D front, Texture2D retro, Vector2 posizione, Mossa[] mosse, int vita, string tipo)
        {
            this.nome = nome;
            this.front = front;
            this.retro = retro;
            this.posizione = posizione;
            this.mosse = mosse;
            this.vita = vita;
            this.tipo = tipo;
        }
        public Pokemon(string nome, string tipo, int vita, Game g)
        {
            string[] FilesFront = Directory.GetFiles("./Content/Pokemon/Front/"); //prendo il pathName di dove ho inserito il fronte dell'immagine del pokemon
            string[] FilesRetro = Directory.GetFiles("./Content/Pokemon/Retro/"); //prendo il pathName di dove ho inserito il retro dell'immagine del pokemon

            string tmp = Path.GetFileNameWithoutExtension("./Content/Pokemon/Front/");
            int pos = tmp.IndexOf("Front");
            tmp.Substring(0, pos); //per prendere il nome del pokemon devo prendere il nome del file ma senza la parola Front
            front = g.Content.Load<Texture2D>("Pokemon/Front/" + Path.GetFileNameWithoutExtension(String.Format("./Content/Pokemon/Front/{0}Front.png",nome))); //Vado a prendere l'immagine frontale del pokemon
            retro = g.Content.Load<Texture2D>("Pokemon/Retro/" + Path.GetFileNameWithoutExtension(String.Format("./Content/Pokemon/Retro/{0}Retro.png",nome))); //Vado a prendere l'immagine dle retro del pokemon
            this.nome = nome;
            this.tipo = tipo;
            this.vita = vita;
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



        public void aggiungiMosse(Mossa[] m)
        {
            mosse = m;
        }

        public string ToXML()
        {
            return String.Format("<root>" + "<comando>s</comando>" + "<pokemon>" + "<nome>{0}</nome>" + "<vita>{1}</vita>" + "<tipo>{2}</tipo>" + "<status></status>" + "</pokemon>" + "</root>", nome, vita, tipo);
        }
    }
}
