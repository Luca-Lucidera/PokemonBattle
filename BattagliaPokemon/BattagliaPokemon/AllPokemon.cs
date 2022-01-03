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

        const int Xadder = 65; //aggiungo uno spazio di 65 pixel da un immagine ad un altra (praticamente saranno attaccate le immagini) 
        const int Yadder = 65; //stesso discorso per l'asse Y
        public AllPokemon(Game1 g, GraphicsDeviceManager graphics)
        {
            //OGNI POKEMON È 65*65px
            string[] FilesFront = Directory.GetFiles("./Content/Pokemon/Front/"); //prendo il pathName di dove ho inserito il fronte dell'immagine del pokemon
            string[] FilesRetro = Directory.GetFiles("./Content/Pokemon/Retro/"); //prendo il pathName di dove ho inserito il retro dell'immagine del pokemon
            
            /*
             * Vado a vedere quanti pokemon ci sono (in base a quante immagini sono presenti nella cartella "Front"
             *⚠️ SE IL NUMERO DEI FILE NON SONO UGUALI CI SARÀ UN ERROER
             */
            pokemon = new Pokemon[FilesFront.Length]; 
            
            //vado a creare tutti gli oggetti pokemon necessari in base alla quantità di immagini
            for (int i = 0; i < pokemon.Length; i++)
                pokemon[i] = new Pokemon();

            //X e Y sono la posizione dell'immagine di fronte nella schemrata di scelta del pokemon
            int x = 0;
            int y = 0;

            Mossa m;

            //ciclo per caricare i pokemon dalle cartelle Front e Retro
            for (int i = 0; i < FilesFront.Length; i++)
            {
                //vado a prendere il nome del file senza l'estensione della cartella delle immagini frontali (es: BulbasaurFront.png)
                string tmp = Path.GetFileNameWithoutExtension(FilesFront[i]);
                //vado a prendere la posizione di dove si trovi la parola Front
                int pos = tmp.IndexOf("Front");
                pokemon[i].nome = tmp.Substring(0, pos); //per prendere il nome del pokemon devo prendere il nome del file ma senza la parola Front
                pokemon[i].front = g.Content.Load<Texture2D>("Pokemon/Front/"+Path.GetFileNameWithoutExtension(FilesFront[i])); //Vado a prendere l'immagine frontale del pokemon
                pokemon[i].retro = g.Content.Load<Texture2D>("Pokemon/Retro/"+Path.GetFileNameWithoutExtension(FilesRetro[i])); //Vado a prendere l'immagine dle retro del pokemon
                pokemon[i].vita = 200; //imposto il nome
                switch (pokemon[i].nome) //in base al nome vado a impostare le mosse (Nome, Tipo, Danno, UtilizziMassimi) info prese da https://wiki.pokemoncentral.it/Charizard/Mosse_apprese_in_prima_generazione
                {
                    case ("Charmander"):
                        m = new Mossa("Graffio", "Normale", 40, 35);
                        pokemon[i].aggiungiMossa(m);
                        m = new Mossa("Ruggito", "Normale", 0, 40);
                        pokemon[i].aggiungiMossa(m);
                        m = new Mossa("Braciere", "Fouco", 40, 25);
                        pokemon[i].aggiungiMossa(m);
                        m = new Mossa("Fulmisguardo", "Normale", 0, 30);
                        pokemon[i].aggiungiMossa(m);
                        break;
                    case ("Bulbasaur"):
                        m = new Mossa("Azione", "Normale", 35, 35);
                        pokemon[i].aggiungiMossa(m);
                        m = new Mossa("Ruggito", "Normale", 0, 40);
                        pokemon[i].aggiungiMossa(m);
                        m = new Mossa("Parassiseme", "Erba", 0, 10);
                        pokemon[i].aggiungiMossa(m);
                        m = new Mossa("Frustata", "Erba", 35, 10);
                        pokemon[i].aggiungiMossa(m);
                        break;
                    case ("poliwhirl"):
                        m = new Mossa("Bolla", "Acqua", 20, 30);
                        pokemon[i].aggiungiMossa(m);
                        m = new Mossa("Ipnosi", "Psico", 0, 20);
                        pokemon[i].aggiungiMossa(m);
                        m = new Mossa("Pistolacqua", "Acqua", 40, 25);
                        pokemon[i].aggiungiMossa(m);
                        m = new Mossa("Ipnosi", "Psico", 0, 20);
                        pokemon[i].aggiungiMossa(m);
                        break;
                }

                if(graphics.PreferredBackBufferWidth <= x) //vado a vedere se la l'asse X ha superato l'asse X allora scala in altezza
                {
                    y += Yadder;
                    x = 0;
                }
                pokemon[i].posizione = new Vector2(x, y); 
                x+= Xadder;
            }
        }
        public Pokemon[] getPokemon()
        {
            return pokemon;
        }
        public Pokemon CheckPokemonPremuto(Vector2 posizionePresa)
        {
            int lastX = 0;
            for (int i = 0; i < pokemon.Length; i++)
            {
                //                                         posizione inzia deve essere minore della sua posizione + la sua lunghezza + l'adder
                if((posizionePresa.X >= pokemon[i].posizione.X + lastX )  && (posizionePresa.X <= pokemon[i].posizione.X + Xadder + lastX))
                {
                    if (posizionePresa.Y >= pokemon[i].posizione.Y && posizionePresa.Y <= pokemon[i].posizione.Y + Yadder)
                        return pokemon[i];
                }
                lastX = Convert.ToInt32(pokemon[i].posizione.X) + 40;
            }
            return null;
        }
    }
}