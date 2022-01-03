﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Threading;
using System.Net.Sockets;
using System.IO;
using System.Net;


namespace BattagliaPokemon
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        //font per disegnare
        private SpriteFont generalFont;
        
        //texture da disegnare
        private Texture2D personaggio; // 96 x 104 px
        private Texture2D confirmButton; // 252 x 91 px
        private Texture2D ipSelection;
        private Texture2D BattleTexture;

        //classi riguardo i Pokemon
        private AllPokemon allPokemon; //classe contenente tutti i pokemon del gioco
        private PokemonScelti mieiPokemon; //classe contente i pokemon che scieglierò nella schermata di scelta dei pokemon (gameLogic = confButton)
        private Pokemon pokemonAvversario;

        private string nome;
        private string gameLogic = "gameStart";
        private string battleLogic = "";
        private string ipAvversario = "";

        //variabili temporanee da sostituire con classi
        string strMioPokemon;
        string strPokemonSceltoAvversario;

        //connessione
        TcpClient myPeer;
        TcpClient secondPeer;
        Thread receivingThread;

        bool done = false; //da eliminare

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.TextInput += TextInputHandler;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            ThreadStart start = new ThreadStart(riceviDati);
            receivingThread = new Thread(start);
            receivingThread.IsBackground = true;
            receivingThread.SetApartmentState(ApartmentState.STA);
            receivingThread.Start();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            //vado a caricare le texture
            generalFont = Content.Load<SpriteFont>("Font");
            personaggio = Content.Load<Texture2D>("sceltaNomeD1");
            confirmButton = Content.Load<Texture2D>("confButton");
            ipSelection = Content.Load<Texture2D>("ipselect");
            BattleTexture = Content.Load<Texture2D>("Desktop - Battaglia");

            //creo oggetto mio pokemon
            mieiPokemon = new PokemonScelti();
            
            /*
             * per inserire delle texture nella classe all pokeon devo per forza passargli l'oggetto Game1 (this)
             * _graphics serve per andare a vedere la grandezza dello schermo, e quindi calcolare dove posizionare i pokemon
             */
            allPokemon = new AllPokemon(this, _graphics);

            //nome del mio allenatore
            nome = "";
            
            //stringhe temporanee da eliminare con delle classi
            strMioPokemon = "";
            strPokemonSceltoAvversario = "";
            
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var mouseState = Mouse.GetState(); //vado a prendere lo stato del mouse
            Vector2 mousePosition = new Vector2(mouseState.X, mouseState.Y); //vado a prendere la posizione del mouse

            //INIZIO DELLA GAME LOGIC
            
            if (gameLogic.Equals("gameStart"))//schermata iniziale
            {
                if (mouseState.LeftButton == ButtonState.Pressed) //controllo se il mouse ha premuto il tasto sinistro
                    if (!nome.Equals("")) //controllo che il nome non sia vuoto
                        if ((mousePosition.X >= 325 && mousePosition.X <= 325 + 150) && (mousePosition.Y >= 410 && mousePosition.Y <= 410 + 25)) //controllo se ha premuto il pulsante
                            gameLogic = "sceltaPokemon";
            }  
            else if (gameLogic.Equals("sceltaPokemon")) //schermata scelta del pokemon
            {
                if (mouseState.LeftButton == ButtonState.Pressed)//controllo se il mouse ha premuto il tasto sinistro
                {
                    var tmp = allPokemon.CheckPokemonPremuto(mousePosition);//vado a vedere se il pokemon è stato premuto 
                    if (tmp != null) //controllo che il risultato non sia nullo
                        mieiPokemon.addPokemon(tmp);//aggiungo il pokemon scelto alla classe mieiPokemon
                    
                    //blocco per proseguire alla prossima schemrata
                    if (mouseState.LeftButton == ButtonState.Pressed)//controllo se è stato premuto il tasto sinistro
                        if (!mieiPokemon.showAll().Equals(""))//controllo che ci sia almeno un pokemon nella mia selezione, in base allo showAll
                            if ((mousePosition.X >= 250 && mousePosition.X <= 250 + 252) && (mousePosition.Y >= 350 && mousePosition.Y <= 350 + 91))//controllo che sia stato premuto nella posizione corretta
                                gameLogic = "ipSelection";
                }
            }
            else if (gameLogic.Equals("ipSelection")) //schermata della selezione ip
            {
                if (mouseState.LeftButton == ButtonState.Pressed) //controllo se è stato premuto il tato sinistro
                    if (!ipAvversario.Equals("")) //controllo che l'ip non sia vuoto
                    {
                        myPeer = new TcpClient(); //creo l'oggetto myPeer che rappresenta il mio client
                        myPeer.Connect(ipAvversario, 42069); //provo a connettermi tramite il metodo connect, dandogli come input l'ip del secondo peer e la porta di ascolto
                        
                        string mioNomeDaMandare = "m;"+nome; //invio la m e il mio nome
                        
                        //prendo lo stream in lettura e scrittura del mio peer
                        StreamWriter sw = new StreamWriter(myPeer.GetStream()); 
                        StreamReader sr = new StreamReader(myPeer.GetStream());
                        
                        //invio il mio nome con il relativo codice al secondo peer
                        sw.WriteLine(mioNomeDaMandare);
                        sw.Flush(); //svuoto il buffer

                        //TODO: ogni stringa di invio e ricezione deve essere trasformata in XML e quindi in classe
                        //da qui io aspetto la risposta del secondo peer, io mi aspetto di ricevere il suo pokemon scelto per primo.
                        strPokemonSceltoAvversario = sr.ReadLine();
                        
                        //dopo aver ricevuto i pokemon dal secondo peer io gli invio il mio primo pokemon
                        sw.WriteLine(String.Format("s;{0}", mieiPokemon.getPokemonByPos(0)));
                        sw.Flush();
                        strMioPokemon = sr.ReadLine();

                        sr.Close();
                        sw.Close();
                        gameLogic = "battle";
                    }
            }
            else if (gameLogic.Equals("battle"))
            {
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    if(battleLogic == "")
                    {
                        if (mousePosition.X >= 182 && mousePosition.X <= 182 + 190 && mouseState.Y >= 328 && mouseState.Y <= 328 + 60)
                        {
                            battleLogic = "Zaino";
                        }
                        else if (mousePosition.X >= 423 && mousePosition.X <= 423 + 190 && mouseState.Y >= 328 && mouseState.Y <= 328 + 60)
                        {
                            battleLogic = "Attacca";
                        }
                        else if (mousePosition.X >= 182 && mousePosition.X <= 182 + 190 && mouseState.Y >= 398 && mouseState.Y <= 398 + 60)
                        {
                            battleLogic = "Fuga";
                        }
                        else if (mousePosition.X >= 423 && mousePosition.X <= 423 + 190 && mouseState.Y >= 398 && mouseState.Y <= 398 + 60)
                        {
                            battleLogic = "Cambia";
                        }
                    }
                    
                }
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            _spriteBatch.Begin();
            if (gameLogic.Equals("gameStart"))
            {
                _spriteBatch.Draw(personaggio, new Vector2(0, 0), Color.White);
                _spriteBatch.DrawString(generalFont, nome, new Vector2(255, 315), Color.Black);
            }
            else if (gameLogic.Equals("sceltaPokemon"))
            {
                Pokemon[] allpokemon = allPokemon.getPokemon();
                int space = 0;
                int lastX = 0;
                for (int i = 0; i < allpokemon.Length; i++)
                {
                    _spriteBatch.Draw(allpokemon[i].front, new Vector2(allpokemon[i].posizione.X + space, allpokemon[i].posizione.Y), Color.White);
                    _spriteBatch.DrawString(generalFont, allpokemon[i].nome, new Vector2(allpokemon[i].posizione.X + space, allpokemon[i].posizione.Y + 90), Color.Black);
                    space += 40;
                    lastX = Convert.ToInt32(allpokemon[i].posizione.X);
                }
                _spriteBatch.DrawString(generalFont, "hai selezionato: " + mieiPokemon.showAll(), new Vector2(0, 200), Color.Black);
                _spriteBatch.Draw(confirmButton, new Vector2(250, 350), Color.White);
            }
            else if (gameLogic.Equals("ipSelection"))
            {
                _spriteBatch.Draw(ipSelection, new Vector2(0, 0), Color.White);
                _spriteBatch.DrawString(generalFont, ipAvversario, new Vector2(380, 175), Color.Black);
            }
            else if (gameLogic.Equals("battle"))
            {
                if (battleLogic.Equals("Zaino")){

                }
                else if (battleLogic.Equals("Attacca"))
                {
                    Texture2D rettMossa = new Texture2D(GraphicsDevice, 1, 1);
                    rettMossa.SetData(new[] { Color.White });
                    Pokemon[] allpokemon = allPokemon.getPokemon();
                    for (int i = 0; i < allpokemon.Length; i++)
                    {
                        if (strPokemonSceltoAvversario == allpokemon[i].nome)
                        {
                            _spriteBatch.Draw(allpokemon[i].front, new Vector2(500, 20), Color.White);
                            _spriteBatch.DrawString(generalFont, allpokemon[i].nome, new Vector2(400, 20), Color.Black);
                            _spriteBatch.DrawString(generalFont, "Vita: " + Convert.ToString(allpokemon[i].vita), new Vector2(400, 40), Color.Black);
                        }
                        if (strMioPokemon == allpokemon[i].nome)
                        {
                            _spriteBatch.Draw(allpokemon[i].retro, new Vector2(200, 200), Color.White);
                            _spriteBatch.DrawString(generalFont, allpokemon[i].nome, new Vector2(270, 200), Color.Black);
                            _spriteBatch.DrawString(generalFont, "Vita: " + Convert.ToString(allpokemon[i].vita), new Vector2(270, 220), Color.Black);

                            int xAdder1 = 0;
                            int xAdder2 = 0;
                            for (int j = 0; j < allpokemon[i].mosse.Length; j++)
                            {
                                if(j < 2)
                                {
                                    if (allpokemon[i].mosse[j].tipo == "Normale")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.Gray);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder1,330),Color.Black);
                                    }
                                    else if (allpokemon[i].mosse[j].tipo == "Fouco")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.Orange);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder1, 330), Color.Black);
                                    }
                                    else if (allpokemon[i].mosse[j].tipo == "Erba")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.Green);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder1, 330), Color.Black);
                                    }
                                    else if (allpokemon[i].mosse[j].tipo == "Psico")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.Purple);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder1, 330), Color.Black);

                                    }
                                    else if (allpokemon[i].mosse[j].tipo == "Acqua")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.Blue);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder1, 330), Color.Black);
                                    }
                                    xAdder1 = 241;
                                }
                                else
                                {
                                    if (allpokemon[i].mosse[j].tipo == "Normale")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder2, 398, 190, 60), Color.Gray);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder2, 400), Color.Black);
                                    }
                                    else if (allpokemon[i].mosse[j].tipo == "Fouco")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder2, 398, 190, 60), Color.Orange);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder2, 400), Color.Black);
                                    }
                                    else if (allpokemon[i].mosse[j].tipo == "Erba")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder2, 398, 190, 60), Color.Green);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder2, 400), Color.Black);
                                    }
                                    else if (allpokemon[i].mosse[j].tipo == "Psico")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder2, 398, 190, 60), Color.Purple);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder2, 400), Color.Black);

                                    }
                                    else if (allpokemon[i].mosse[j].tipo == "Acqua")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder2, 398, 190, 60), Color.Blue);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder2, 400), Color.Black);
                                    }
                                    xAdder2 = 241;
                                }
                            }
                        }
                    }
                    
                    
                }
                else if (battleLogic.Equals("Fuga"))
                {

                }
                else if (battleLogic.Equals("Cambia"))
                {

                }
                else //esegue se non c'è una battleLogic (eseguita almeno una volta 
                {
                    Pokemon[] allpokemon = allPokemon.getPokemon();
                    _spriteBatch.Draw(BattleTexture, new Vector2(0, 0), Color.White);
                    for (int i = 0; i < allpokemon.Length; i++)
                    {
                        if (strPokemonSceltoAvversario == allpokemon[i].nome)
                        {
                            _spriteBatch.Draw(allpokemon[i].front, new Vector2(500, 20), Color.White);
                            _spriteBatch.DrawString(generalFont, allpokemon[i].nome, new Vector2(400, 20), Color.Black);
                            _spriteBatch.DrawString(generalFont, "Vita: " + Convert.ToString(allpokemon[i].vita), new Vector2(400, 40), Color.Black);
                        }
                        if (strMioPokemon == allpokemon[i].nome)
                        {
                            _spriteBatch.Draw(allpokemon[i].retro, new Vector2(200, 200), Color.White);
                            _spriteBatch.DrawString(generalFont, allpokemon[i].nome, new Vector2(270, 200), Color.Black);
                            _spriteBatch.DrawString(generalFont, "Vita: " + Convert.ToString(allpokemon[i].vita), new Vector2(270, 220), Color.Black);
                        }
                    }
                }
                
            }
            _spriteBatch.End();
            base.Draw(gameTime);
        }
        
        //funzione per gestire l'input da tastiera
        private void TextInputHandler(object sender, TextInputEventArgs args)
        {
            var character = args.Character;
            if (gameLogic.Equals("gameStart"))
            {
                if (character.Equals('\b'))
                {
                    if (nome != "")
                    {
                        nome = nome.Substring(0, nome.Length - 1);
                    }
                }
                else
                {
                    nome += character;
                }
            }
            else if (gameLogic.Equals("ipSelection"))
            {
                if (character.Equals('\b'))
                {
                    if (ipAvversario != "")
                    {
                        ipAvversario = ipAvversario.Substring(0, ipAvversario.Length - 1);
                    }
                }
                else
                {
                    ipAvversario += character;
                }
            }

        }

        //funzione eseguita dal thread per ricevere i dati dal 1° peer al 2° peer
        private void riceviDati()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 42069);
            listener.Start();
            StreamWriter sw;
            StreamReader sr;
            secondPeer = listener.AcceptTcpClient();
            while (true)
            {
                sw = new StreamWriter(secondPeer.GetStream());
                sr = new StreamReader(secondPeer.GetStream());

                String strClientInput = sr.ReadLine();
                if (strClientInput != null)
                {
                    if (strClientInput.StartsWith("m")) //peer 1 contatta peer 2 e il peer 2 gli manda il suo pokemon
                    {
                        strMioPokemon = mieiPokemon.getPokemonByPos(0);//da sostituire con l'xml
                        sw.WriteLine(strMioPokemon);
                        sw.Flush();
                        gameLogic = "battle";
                    }
                    else if (strClientInput.StartsWith("s"))  //peer 1 riceve i pokemon del peer 2 e quindi il peer 1 invia il suo pokemon
                    {
                        strPokemonSceltoAvversario = strClientInput.Substring(2);//da sostituire con l'xml
                        sw.WriteLine(strPokemonSceltoAvversario);
                        sw.Flush();
                    }
                    else if (strClientInput == "a")
                    {
                        string mioPokemon = "r;Charmander;150;1";
                        sw.WriteLine(mioPokemon);
                        sw.Flush();
                    }
                }


            }
            sw.Close();
            sr.Close();
        }
    }
}