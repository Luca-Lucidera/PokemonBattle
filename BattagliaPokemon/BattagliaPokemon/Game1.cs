using Microsoft.Xna.Framework;
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
        private Pokemon mioPokemon;

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
            pokemonAvversario = new Pokemon();
            mioPokemon = new Pokemon();

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
            Console.WriteLine(String.Format("X:{0} Y:{1}", mouseState.X, mouseState.Y));
            //INIZIO DELLA GAME LOGIC

            if (gameLogic.Equals("gameStart"))//schermata iniziale
            {
                if (mouseState.LeftButton == ButtonState.Pressed) //controllo se il mouse ha premuto il tasto sinistro
                    if (!nome.Equals("")) //controllo che il nome non sia vuoto
                        if ((mousePosition.X >= 325 && mousePosition.X <= 325 + 150) && (mousePosition.Y >= 410 && mousePosition.Y <= 410 + 25)) //controllo se ha premuto il pulsante
                        {
                            gameLogic = "sceltaPokemon";
                            Thread.Sleep(100);
                        }
            }
            else if (gameLogic.Equals("sceltaPokemon")) //schermata scelta del pokemon
            {
                if (mouseState.LeftButton == ButtonState.Pressed)//controllo se il mouse ha premuto il tasto sinistro
                {
                    var tmp = allPokemon.CheckPokemonPremuto(mousePosition);//vado a vedere se il pokemon è stato premuto 
                    if (tmp != null &&  mieiPokemon.getNumeroPokemonScelti() < 6) //controllo che il risultato non sia nullo
                        mieiPokemon.addPokemon(tmp);//aggiungo il pokemon scelto alla classe mieiPokemon

                    //blocco per proseguire alla prossima schemrata
                    if (mouseState.LeftButton == ButtonState.Pressed)//controllo se è stato premuto il tasto sinistro
                        if (!mieiPokemon.showAll().Equals(""))//controllo che ci sia almeno un pokemon nella mia selezione, in base allo showAll
                            if ((mousePosition.X >= 250 && mousePosition.X <= 250 + 252) && (mousePosition.Y >= 350 && mousePosition.Y <= 350 + 91))//controllo che sia stato premuto nella posizione corretta
                            {
                                gameLogic = "ipSelection";
                                Thread.Sleep(100);
                            }
                }
            }
            else if (gameLogic.Equals("ipSelection")) //schermata della selezione ip
            {
                if (mouseState.LeftButton == ButtonState.Pressed) //controllo se è stato premuto il tato sinistro
                    if (!ipAvversario.Equals("")) //controllo che l'ip non sia vuoto
                    {
                        //da eliminare
                        strMioPokemon = mieiPokemon.getPokemonByPos(0).nome;

                        myPeer = new TcpClient(); //creo l'oggetto myPeer che rappresenta il mio client
                        myPeer.Connect(ipAvversario, 42069); //provo a connettermi tramite il metodo connect, dandogli come input l'ip del secondo peer e la porta di ascolto

                        string mioNomeDaMandare = "m;" + nome; //invio la m e il mio nome

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

                        sw.WriteLine(String.Format("s;{0}", strMioPokemon));
                        sw.Flush();

                        sw.Close();
                        sr.Close();
                        sr.Dispose();
                        gameLogic = "battle";
                        Thread.Sleep(100);
                    }

            }
            else if (gameLogic.Equals("battle"))
            {

                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    if (battleLogic.Equals(""))
                    {
                        if (mousePosition.X >= 182 && mousePosition.X <= 182 + 190 && mousePosition.Y >= 328 && mousePosition.Y <= 328 + 60)
                        {
                            battleLogic = "Zaino";
                        }
                        else if (mousePosition.X >= 423 && mousePosition.X <= 423 + 190 && mousePosition.Y >= 328 && mousePosition.Y <= 328 + 60)
                        {
                            battleLogic = "Attacca";
                        }
                        else if (mousePosition.X >= 182 && mousePosition.X <= 182 + 190 && mousePosition.Y >= 398 && mousePosition.Y <= 398 + 60)
                        {
                            battleLogic = "Fuga";
                        }
                        else if (mousePosition.X >= 423 && mousePosition.X <= 423 + 190 && mousePosition.Y >= 398 && mousePosition.Y <= 398 + 60)
                        {
                            battleLogic = "Cambia";
                        }
                    }
                    else
                    {
                        Thread.Sleep(100);
                        if (battleLogic.Equals("Attacca"))
                        {
                            if (mousePosition.X >= 21 && mousePosition.X <= 21 + 136 && mousePosition.Y >= 398 && mousePosition.Y <= 398 + 60) //bottone schermata attacca
                            {
                                battleLogic = "";
                            }
                            else
                            {
                                //da qui inserire la logica per gestire la fase a turni tra peer 1 e peer 2
                                Pokemon mioPokemonTMP = new Pokemon();
                                Pokemon[] tmp = allPokemon.getPokemon();
                                Mossa mossaScelta = new Mossa();
                                for (int i = 0; i < allPokemon.getPokemon().Length; i++)
                                {
                                    if (strMioPokemon.Equals(tmp[i].nome))
                                    {
                                        mioPokemon = tmp[i];
                                        break;
                                    }
                                }
                                if (mousePosition.X >= 182 && mousePosition.X <= 182 + 190 && mousePosition.Y >= 328 && mousePosition.Y <= 328 + 60)
                                    mossaScelta = mioPokemon.mosse[0];
                                else if (mousePosition.X >= 423 && mousePosition.X <= 423 + 190 && mousePosition.Y >= 328 && mousePosition.Y <= 328 + 60)
                                    mossaScelta = mioPokemon.mosse[1];
                                else if (mousePosition.X >= 182 && mousePosition.X <= 182 + 190 && mousePosition.Y >= 398 && mousePosition.Y <= 398 + 60)
                                    mossaScelta = mioPokemon.mosse[2];
                                else if (mousePosition.X >= 423 && mousePosition.X <= 423 + 190 && mousePosition.Y >= 398 && mousePosition.Y <= 398 + 60)
                                    mossaScelta = mioPokemon.mosse[3];

                                Console.WriteLine("Mossa scelta: " + mossaScelta.ToString());
                                /* DA QUI INSERIRE LA LOGICA TCP PER I TURNI */

                            }
                        }
                        else if (battleLogic.Equals("Zaino"))
                        {

                        }
                        else if (battleLogic.Equals("Cambia"))
                        {

                        }
                        else if (battleLogic.Equals("Fuga"))
                        {

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
            var mouseState = Mouse.GetState();
            string debug = String.Format("X:{0} Y:{1} GameLogic: {2} BattleLogic: {3}", mouseState.X, mouseState.Y, gameLogic, battleLogic);

            if (gameLogic.Equals("gameStart"))
            {
                _spriteBatch.Draw(personaggio, new Vector2(0, 0), Color.White);
                _spriteBatch.DrawString(generalFont, nome, new Vector2(255, 315), Color.Black);
            }
            else if (gameLogic.Equals("sceltaPokemon"))
            {
                Pokemon[] allpokemon = allPokemon.getPokemon();
                int space = 0;
                //int lastX = 0;
                for (int i = 0; i < allpokemon.Length; i++)
                {
                    _spriteBatch.Draw(allpokemon[i].front, new Vector2(allpokemon[i].posizione.X, allpokemon[i].posizione.Y), Color.White);
                    _spriteBatch.DrawString(generalFont, allpokemon[i].nome, new Vector2(allpokemon[i].posizione.X, allpokemon[i].posizione.Y + 90), Color.Black);
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
                if (battleLogic.Equals("Zaino"))
                {

                }
                else if (battleLogic.Equals("Attacca"))
                {
                    Texture2D rettMossa = new Texture2D(GraphicsDevice, 1, 1);
                    Texture2D rettGoBack = new Texture2D(GraphicsDevice, 1, 1);
                    rettMossa.SetData(new[] { Color.White });
                    rettGoBack.SetData(new[] { Color.White });
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
                            _spriteBatch.Draw(rettGoBack, new Rectangle(21, 398, 136, 60), Color.Red); //disegno il bottone per tornare alla schermata pricipale 
                            _spriteBatch.DrawString(generalFont, "torna indietro", new Vector2(25, 402), Color.Black);
                            int xAdder1 = 0;
                            int xAdder2 = 0;
                            //for per disegnare i rettangoli delle mosse
                            for (int j = 0; j < allpokemon[i].mosse.Length; j++)
                            {
                                if (j < 2) //new Rectangle(182 + xAdder1, 328, 190, 60)
                                {
                                    if (allpokemon[i].mosse[j].tipo == "Normale")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.Gray);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder1, 332), Color.Black);
                                    }
                                    else if (allpokemon[i].mosse[j].tipo == "Fuoco")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.Orange);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder1, 332), Color.Black);
                                    }
                                    else if (allpokemon[i].mosse[j].tipo == "Erba")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.Green);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder1, 332), Color.Black);
                                    }
                                    else if (allpokemon[i].mosse[j].tipo == "Psico")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.LightPink);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder1, 332), Color.Black);

                                    }
                                    else if (allpokemon[i].mosse[j].tipo == "Acqua")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.Blue);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder1, 332), Color.White);
                                    }
                                    else if (allpokemon[i].mosse[j].tipo == "Ghiaccio")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.DeepSkyBlue);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder1, 332), Color.Black);
                                    }
                                    else if (allpokemon[i].mosse[j].tipo == "Drago")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.Brown);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder1, 332), Color.Black);
                                    }
                                    else if (allpokemon[i].mosse[j].tipo == "Buio")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.Black);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder1, 332), Color.White);
                                    }
                                    else if (allpokemon[i].mosse[j].tipo == "Lotta")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.Brown);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder1, 332), Color.Black);
                                    }
                                    else if (allpokemon[i].mosse[j].tipo == "Volante")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.AliceBlue);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder1, 332), Color.Black);
                                    }
                                    else if (allpokemon[i].mosse[j].tipo == "Veleno")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.Purple);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder1, 332), Color.Black);
                                    }
                                    else if (allpokemon[i].mosse[j].tipo == "Terra")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.SandyBrown);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder1, 332), Color.Black);
                                    }
                                    else if (allpokemon[i].mosse[j].tipo == "Roccia")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.RosyBrown);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder1, 332), Color.Black);
                                    }
                                    else if (allpokemon[i].mosse[j].tipo == "Spettro")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.Purple);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder1, 332), Color.Black);
                                    }
                                    else if (allpokemon[i].mosse[j].tipo == "Acciaio")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.DarkGray);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder1, 332), Color.Black);
                                    }
                                    else if (allpokemon[i].mosse[j].tipo == "Coleottero")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.LightGreen);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder1, 332), Color.Black);
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
                                    else if (allpokemon[i].mosse[j].tipo == "Fuoco")
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
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder2, 398, 190, 60), Color.LightPink);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder2, 400), Color.Black);

                                    }
                                    else if (allpokemon[i].mosse[j].tipo == "Acqua")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder2, 398, 190, 60), Color.Blue);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder2, 400), Color.White);
                                    }
                                    else if (allpokemon[i].mosse[j].tipo == "Ghiaccio")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder2, 398, 190, 60), Color.DeepSkyBlue);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder2, 400), Color.Black);
                                    }
                                    else if (allpokemon[i].mosse[j].tipo == "Drago")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder2, 398, 190, 60), Color.Brown);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder2, 400), Color.Black);
                                    }
                                    else if (allpokemon[i].mosse[j].tipo == "Buio")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder2, 398, 190, 60), Color.Black);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder2, 400), Color.White);
                                    }
                                    else if (allpokemon[i].mosse[j].tipo == "Lotta")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder2, 398, 190, 60), Color.Brown);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder2, 400), Color.Black);
                                    }
                                    else if (allpokemon[i].mosse[j].tipo == "Volante")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder2, 398, 190, 60), Color.AliceBlue);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder2, 400), Color.Black);
                                    }
                                    else if (allpokemon[i].mosse[j].tipo == "Veleno")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder2, 398, 190, 60), Color.Purple);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder2, 400), Color.Black);
                                    }
                                    else if (allpokemon[i].mosse[j].tipo == "Terra")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder2, 398, 190, 60), Color.SandyBrown);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder2, 400), Color.Black);
                                    }
                                    else if (allpokemon[i].mosse[j].tipo == "Roccia")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder2, 398, 190, 60), Color.RosyBrown);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder2, 400), Color.Black);
                                    }
                                    else if (allpokemon[i].mosse[j].tipo == "Spettro")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder2, 398, 190, 60), Color.Purple);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder2, 400), Color.Black);
                                    }
                                    else if (allpokemon[i].mosse[j].tipo == "Acciaio")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder2, 398, 190, 60), Color.DarkGray);
                                        _spriteBatch.DrawString(generalFont, allpokemon[i].mosse[j].nome, new Vector2(185 + xAdder2, 400), Color.Black);
                                    }
                                    else if (allpokemon[i].mosse[j].tipo == "Coleottero")
                                    {
                                        _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder2, 398, 190, 60), Color.LightGreen);
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
            _spriteBatch.DrawString(generalFont, debug, new Vector2(0, _graphics.PreferredBackBufferHeight - 25), Color.Black);
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
                        //NEL PEER 2 
                        mioPokemon = mieiPokemon.getPokemonByPos(0);//da sostituire con l'xml
                        string xmlDaRitornare = "valore contenuto in mio pokemon";
                        strMioPokemon = mieiPokemon.getPokemonByPos(0).nome;
                        sw.WriteLine(strMioPokemon);
                        sw.Flush();
                    }
                    else if (strClientInput.StartsWith("s"))  //peer 1 riceve i pokemon del peer 2 e quindi il peer 1 invia il suo pokemon
                    {
                        strPokemonSceltoAvversario = strClientInput.Substring(2);
                        gameLogic = "battle";
                    }
                    else if (strClientInput.StartsWith("a"))
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