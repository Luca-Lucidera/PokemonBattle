using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Threading;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.Xml;
using NAudio;
using NAudio.Wave;

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

        //XML
        private XmlDocument xmlDoc;
        private XmlNode nodoPokemon;

        private int c = 0;
        private string nome;
        private string nomeAvversario;
        private string gameLogic = "gameStart";
        private string battleLogic = "";
        private string ipAvversario = "";
        private string messaggioTurno = "";

        private bool mioTurno = false;
        private bool done1 = false;
        private bool done2 = false;
        private int posAnimazioneAvversario = 800;
        private int posAnimazioneIo = 0;

        //variabili temporanee da sostituire con classi
        private string strPokemonSceltoAvversario;
        private bool eseguito = false;
        private WaveOutEvent outputDevice = new WaveOutEvent();
        private AudioFileReader audioFile;

        //connessione
        private TcpClient myPeer;
        private TcpClient secondPeer;
        private StreamReader srFP;
        private StreamWriter swFP;
        private Thread receivingThread;


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
            xmlDoc = new XmlDocument();
            myPeer = new TcpClient();

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
                    if (tmp != null && mieiPokemon.getNumeroPokemonScelti() < 6) //controllo che il risultato non sia nullo
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
                        mioPokemon = mieiPokemon.getPokemonByPos(0);

                        //myPeer = new TcpClient(); //creo l'oggetto myPeer che rappresenta il mio client
                        myPeer.Connect(ipAvversario, 42069); //provo a connettermi tramite il metodo connect, dandogli come input l'ip del secondo peer e la porta di ascolto
                        swFP = new StreamWriter(myPeer.GetStream());
                        srFP = new StreamReader(myPeer.GetStream());

                        string mioNomeDaMandare = String.Format("<root>" +
                            "<comando>m</comando>" +
                            "<nome>{0}</nome>" +
                            "</root>", nome); //invio la m e il mio nome



                        //invio il mio nome con il relativo codice al secondo peer
                        swFP.WriteLine(mioNomeDaMandare);
                        swFP.Flush(); //svuoto il buffer

                        //TODO: ogni stringa di invio e ricezione deve essere trasformata in XML e quindi in classe
                        //da qui io aspetto la risposta del secondo peer, io mi aspetto di ricevere il suo pokemon scelto per primo.
                        nomeAvversario = srFP.ReadLine();
                        strPokemonSceltoAvversario = srFP.ReadLine();


                        xmlDoc.LoadXml(strPokemonSceltoAvversario);
                        nodoPokemon = xmlDoc.DocumentElement.ChildNodes[1];
                        pokemonAvversario = new Pokemon(nodoPokemon.ChildNodes[0].InnerText, nodoPokemon.ChildNodes[2].InnerText, Convert.ToInt32(nodoPokemon.ChildNodes[1].InnerText), this);


                        //dopo aver ricevuto i pokemon dal secondo peer io gli invio il mio primo pokemon

                        swFP.WriteLine(mieiPokemon.getPokemonByPos(0).ToXML());
                        swFP.Flush();

                        gameLogic = "battle";
                        mioTurno = true;
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
                        Thread.Sleep(33);
                        if (mioTurno)
                            eseguiTurno();
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
                _spriteBatch.DrawString(generalFont, nome, new Vector2(260, 315), Color.Black);
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

                    //disegna il pokemon avversario + il nome + vita
                    _spriteBatch.Draw(pokemonAvversario.front, new Vector2(500, 20), Color.White);
                    _spriteBatch.DrawString(generalFont, pokemonAvversario.nome, new Vector2(400, 20), Color.Black);
                    _spriteBatch.DrawString(generalFont, "Vita: " + Convert.ToString(pokemonAvversario.vita), new Vector2(400, 40), Color.Black);

                    //disegna il mio pokemon + il nome + vita
                    _spriteBatch.Draw(mioPokemon.retro, new Vector2(200, 200), Color.White);
                    _spriteBatch.DrawString(generalFont, mioPokemon.nome, new Vector2(270, 200), Color.Black);
                    _spriteBatch.DrawString(generalFont, "Vita: " + Convert.ToString(mioPokemon.vita), new Vector2(270, 220), Color.Black);
                    _spriteBatch.Draw(rettGoBack, new Rectangle(21, 398, 136, 60), Color.Red);
                    _spriteBatch.DrawString(generalFont, "torna indietro", new Vector2(25, 402), Color.Black);

                    int xAdder1 = 0;
                    int xAdder2 = 0;
                    //for per disegnare i rettangoli delle mosse
                    for (int j = 0; j < mioPokemon.mosse.Length; j++)
                    {
                        if (j < 2) //new Rectangle(182 + xAdder1, 328, 190, 60)
                        {
                            if (mioPokemon.mosse[j].tipo == "Normale")
                            {
                                _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.Gray);
                                _spriteBatch.DrawString(generalFont, mioPokemon.mosse[j].nome + "\n Danni: " + mioPokemon.mosse[j].danni, new Vector2(185 + xAdder1, 332), Color.Black);
                            }
                            else if (mioPokemon.mosse[j].tipo == "Fuoco")
                            {
                                _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.Orange);
                                _spriteBatch.DrawString(generalFont, mioPokemon.mosse[j].nome + "\n Danni: " + mioPokemon.mosse[j].danni, new Vector2(185 + xAdder1, 332), Color.Black);
                            }
                            else if (mioPokemon.mosse[j].tipo == "Erba")
                            {
                                _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.Green);
                                _spriteBatch.DrawString(generalFont, mioPokemon.mosse[j].nome + "\n Danni: " + mioPokemon.mosse[j].danni, new Vector2(185 + xAdder1, 332), Color.Black);
                            }
                            else if (mioPokemon.mosse[j].tipo == "Psico")
                            {
                                _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.LightPink);
                                _spriteBatch.DrawString(generalFont, mioPokemon.mosse[j].nome + "\n Danni: " + mioPokemon.mosse[j].danni, new Vector2(185 + xAdder1, 332), Color.Black);

                            }
                            else if (mioPokemon.mosse[j].tipo == "Acqua")
                            {
                                _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.Blue);
                                _spriteBatch.DrawString(generalFont, mioPokemon.mosse[j].nome + "\n Danni: " + mioPokemon.mosse[j].danni, new Vector2(185 + xAdder1, 332), Color.White);
                            }
                            else if (mioPokemon.mosse[j].tipo == "Ghiaccio")
                            {
                                _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.DeepSkyBlue);
                                _spriteBatch.DrawString(generalFont, mioPokemon.mosse[j].nome + "\n Danni: " + mioPokemon.mosse[j].danni, new Vector2(185 + xAdder1, 332), Color.Black);
                            }
                            else if (mioPokemon.mosse[j].tipo == "Drago")
                            {
                                _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.Brown);
                                _spriteBatch.DrawString(generalFont, mioPokemon.mosse[j].nome + "\n Danni: " + mioPokemon.mosse[j].danni, new Vector2(185 + xAdder1, 332), Color.Black);
                            }
                            else if (mioPokemon.mosse[j].tipo == "Buio")
                            {
                                _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.Black);
                                _spriteBatch.DrawString(generalFont, mioPokemon.mosse[j].nome + "\n Danni: " + mioPokemon.mosse[j].danni, new Vector2(185 + xAdder1, 332), Color.White);
                            }
                            else if (mioPokemon.mosse[j].tipo == "Lotta")
                            {
                                _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.Brown);
                                _spriteBatch.DrawString(generalFont, mioPokemon.mosse[j].nome + "\n Danni: " + mioPokemon.mosse[j].danni, new Vector2(185 + xAdder1, 332), Color.Black);
                            }
                            else if (mioPokemon.mosse[j].tipo == "Volante")
                            {
                                _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.AliceBlue);
                                _spriteBatch.DrawString(generalFont, mioPokemon.mosse[j].nome + "\n Danni: " + mioPokemon.mosse[j].danni, new Vector2(185 + xAdder1, 332), Color.Black);
                            }
                            else if (mioPokemon.mosse[j].tipo == "Veleno")
                            {
                                _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.Purple);
                                _spriteBatch.DrawString(generalFont, mioPokemon.mosse[j].nome + "\n Danni: " + mioPokemon.mosse[j].danni, new Vector2(185 + xAdder1, 332), Color.Black);
                            }
                            else if (mioPokemon.mosse[j].tipo == "Terra")
                            {
                                _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.SandyBrown);
                                _spriteBatch.DrawString(generalFont, mioPokemon.mosse[j].nome + "\n Danni: " + mioPokemon.mosse[j].danni, new Vector2(185 + xAdder1, 332), Color.Black);
                            }
                            else if (mioPokemon.mosse[j].tipo == "Roccia")
                            {
                                _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.RosyBrown);
                                _spriteBatch.DrawString(generalFont, mioPokemon.mosse[j].nome + "\n Danni: " + mioPokemon.mosse[j].danni, new Vector2(185 + xAdder1, 332), Color.Black);
                            }
                            else if (mioPokemon.mosse[j].tipo == "Spettro")
                            {
                                _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.Purple);
                                _spriteBatch.DrawString(generalFont, mioPokemon.mosse[j].nome + "\n Danni: " + mioPokemon.mosse[j].danni, new Vector2(185 + xAdder1, 332), Color.Black);
                            }
                            else if (mioPokemon.mosse[j].tipo == "Acciaio")
                            {
                                _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.DarkGray);
                                _spriteBatch.DrawString(generalFont, mioPokemon.mosse[j].nome + "\n Danni: " + mioPokemon.mosse[j].danni, new Vector2(185 + xAdder1, 332), Color.Black);
                            }
                            else if (mioPokemon.mosse[j].tipo == "Coleottero")
                            {
                                _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.LightGreen);
                                _spriteBatch.DrawString(generalFont, mioPokemon.mosse[j].nome + "\n Danni: " + mioPokemon.mosse[j].danni, new Vector2(185 + xAdder1, 332), Color.Black);
                            }
                            else if (mioPokemon.mosse[j].tipo == "Elettro")
                            {
                                string nome = mioPokemon.mosse[j].nome;
                                _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder1, 328, 190, 60), Color.Yellow);
                                _spriteBatch.DrawString(generalFont, mioPokemon.mosse[j].nome + "\n Danni: " + mioPokemon.mosse[j].danni, new Vector2(185 + xAdder1, 332), Color.Black);
                            }
                            xAdder1 = 241;
                        }
                        else
                        {
                            if (mioPokemon.mosse[j].tipo == "Normale")
                            {
                                _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder2, 398, 190, 60), Color.Gray);
                                _spriteBatch.DrawString(generalFont, mioPokemon.mosse[j].nome + "\n Danni: " + mioPokemon.mosse[j].danni, new Vector2(185 + xAdder2, 400), Color.Black);
                            }
                            else if (mioPokemon.mosse[j].tipo == "Fuoco")
                            {
                                _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder2, 398, 190, 60), Color.Orange);
                                _spriteBatch.DrawString(generalFont, mioPokemon.mosse[j].nome + "\n Danni: " + mioPokemon.mosse[j].danni, new Vector2(185 + xAdder2, 400), Color.Black);
                            }
                            else if (mioPokemon.mosse[j].tipo == "Erba")
                            {
                                _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder2, 398, 190, 60), Color.Green);
                                _spriteBatch.DrawString(generalFont, mioPokemon.mosse[j].nome + "\n Danni: " + mioPokemon.mosse[j].danni, new Vector2(185 + xAdder2, 400), Color.Black);
                            }
                            else if (mioPokemon.mosse[j].tipo == "Psico")
                            {
                                _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder2, 398, 190, 60), Color.LightPink);
                                _spriteBatch.DrawString(generalFont, mioPokemon.mosse[j].nome + "\n Danni: " + mioPokemon.mosse[j].danni, new Vector2(185 + xAdder2, 400), Color.Black);

                            }
                            else if (mioPokemon.mosse[j].tipo == "Acqua")
                            {
                                _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder2, 398, 190, 60), Color.Blue);
                                _spriteBatch.DrawString(generalFont, mioPokemon.mosse[j].nome + "\n Danni: " + mioPokemon.mosse[j].danni, new Vector2(185 + xAdder2, 400), Color.White);
                            }
                            else if (mioPokemon.mosse[j].tipo == "Ghiaccio")
                            {
                                _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder2, 398, 190, 60), Color.DeepSkyBlue);
                                _spriteBatch.DrawString(generalFont, mioPokemon.mosse[j].nome + "\n Danni: " + mioPokemon.mosse[j].danni, new Vector2(185 + xAdder2, 400), Color.Black);
                            }
                            else if (mioPokemon.mosse[j].tipo == "Drago")
                            {
                                _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder2, 398, 190, 60), Color.Brown);
                                _spriteBatch.DrawString(generalFont, mioPokemon.mosse[j].nome + "\n Danni: " + mioPokemon.mosse[j].danni, new Vector2(185 + xAdder2, 400), Color.Black);
                            }
                            else if (mioPokemon.mosse[j].tipo == "Buio")
                            {
                                _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder2, 398, 190, 60), Color.Black);
                                _spriteBatch.DrawString(generalFont, mioPokemon.mosse[j].nome + "\n Danni: " + mioPokemon.mosse[j].danni, new Vector2(185 + xAdder2, 400), Color.White);
                            }
                            else if (mioPokemon.mosse[j].tipo == "Lotta")
                            {
                                _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder2, 398, 190, 60), Color.Brown);
                                _spriteBatch.DrawString(generalFont, mioPokemon.mosse[j].nome + "\n Danni: " + mioPokemon.mosse[j].danni, new Vector2(185 + xAdder2, 400), Color.Black);
                            }
                            else if (mioPokemon.mosse[j].tipo == "Volante")
                            {
                                _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder2, 398, 190, 60), Color.AliceBlue);
                                _spriteBatch.DrawString(generalFont, mioPokemon.mosse[j].nome + "\n Danni: " + mioPokemon.mosse[j].danni, new Vector2(185 + xAdder2, 400), Color.Black);
                            }
                            else if (mioPokemon.mosse[j].tipo == "Veleno")
                            {
                                _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder2, 398, 190, 60), Color.Purple);
                                _spriteBatch.DrawString(generalFont, mioPokemon.mosse[j].nome + "\n Danni: " + mioPokemon.mosse[j].danni, new Vector2(185 + xAdder2, 400), Color.Black);
                            }
                            else if (mioPokemon.mosse[j].tipo == "Terra")
                            {
                                _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder2, 398, 190, 60), Color.SandyBrown);
                                _spriteBatch.DrawString(generalFont, mioPokemon.mosse[j].nome + "\n Danni: " + mioPokemon.mosse[j].danni, new Vector2(185 + xAdder2, 400), Color.Black);
                            }
                            else if (mioPokemon.mosse[j].tipo == "Roccia")
                            {
                                _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder2, 398, 190, 60), Color.RosyBrown);
                                _spriteBatch.DrawString(generalFont, mioPokemon.mosse[j].nome + "\n Danni: " + mioPokemon.mosse[j].danni, new Vector2(185 + xAdder2, 400), Color.Black);
                            }
                            else if (mioPokemon.mosse[j].tipo == "Spettro")
                            {
                                _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder2, 398, 190, 60), Color.Purple);
                                _spriteBatch.DrawString(generalFont, mioPokemon.mosse[j].nome + "\n Danni: " + mioPokemon.mosse[j].danni, new Vector2(185 + xAdder2, 400), Color.Black);
                            }
                            else if (mioPokemon.mosse[j].tipo == "Acciaio")
                            {
                                _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder2, 398, 190, 60), Color.DarkGray);
                                _spriteBatch.DrawString(generalFont, mioPokemon.mosse[j].nome + "\n Danni: " + mioPokemon.mosse[j].danni, new Vector2(185 + xAdder2, 400), Color.Black);
                            }
                            else if (mioPokemon.mosse[j].tipo == "Coleottero")
                            {
                                _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder2, 398, 190, 60), Color.LightGreen);
                                _spriteBatch.DrawString(generalFont, mioPokemon.mosse[j].nome + "\n Danni: " + mioPokemon.mosse[j].danni, new Vector2(185 + xAdder2, 400), Color.Black);
                            }
                            else if (mioPokemon.mosse[j].tipo == "Elettro")
                            {
                                _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder2, 398, 190, 60), Color.Yellow);
                                _spriteBatch.DrawString(generalFont, mioPokemon.mosse[j].nome + "\n Danni: " + mioPokemon.mosse[j].danni, new Vector2(185 + xAdder2, 400), Color.Black);
                            }
                            else if (mioPokemon.mosse[j].tipo == "Drago")
                            {
                                _spriteBatch.Draw(rettMossa, new Rectangle(182 + xAdder2, 398, 190, 60), Color.BlueViolet);
                                _spriteBatch.DrawString(generalFont, mioPokemon.mosse[j].nome + "\n Danni: " + mioPokemon.mosse[j].danni, new Vector2(185 + xAdder2, 400), Color.Black);
                            }
                            xAdder2 = 241;
                        }
                    }
                }
                else if (battleLogic.Equals("Cambia"))
                {
                    int tmpX = 0;
                    int posMioPokemon = -1;
                    for (int i = 0; i < mieiPokemon.getNumeroPokemonScelti(); i++)
                    {
                        Pokemon tmp = mieiPokemon.getPokemonByPos(i);
                        if (tmp != mioPokemon && tmp.vita > 0)
                        {
                            _spriteBatch.Draw(tmp.front,
                                          new Vector2(tmpX, 0),
                                          Color.White);
                            tmp.posizione = new Vector2(tmpX, 0); //vado a cambiare la posizione che ha il pokemon 
                            _spriteBatch.DrawString(generalFont, tmp.nome, new Vector2(tmpX, 90), Color.Black);
                            tmpX += 105;
                        }
                        else
                        {
                            posMioPokemon = i;
                        }
                    }
                    mieiPokemon.getPokemonByPos(posMioPokemon).posizione = new Vector2(-200, 0);//metto il pokemon che è in uso attualmente come posizione fouri dalla schermata di gioco => non premibile
                }
                else //esegue se non c'è una battleLogic (eseguita almeno una volta 
                {
                    //Disegna la grafica generale
                    _spriteBatch.Draw(BattleTexture, new Vector2(0, 0), Color.White);
                    //disegna il pokemon avversario + il nome + vita


                    //doAnimation("Mio", mioPokemon);

                    if (done1 == false || done2 == false)
                    {



                        if (posAnimazioneAvversario > 500)
                        {
                            _spriteBatch.Draw(pokemonAvversario.front, new Vector2(posAnimazioneAvversario, 20), Color.White);
                            posAnimazioneAvversario -= 2;
                        }
                        else
                        {
                            done1 = true;
                        }
                        if (posAnimazioneIo < 300)
                        {
                            _spriteBatch.Draw(mioPokemon.retro, new Vector2(posAnimazioneIo, 200), Color.White);
                            posAnimazioneIo += 2;

                        }
                        else
                        {
                            done2 = true;
                        }
                        if (!eseguito)
                        {
                            audioEntrataPokemon();
                        }
                    }
                    else
                    {
                        _spriteBatch.Draw(pokemonAvversario.front, new Vector2(500, 20), Color.White);
                        _spriteBatch.DrawString(generalFont, pokemonAvversario.nome, new Vector2(400, 20), Color.Black);
                        _spriteBatch.DrawString(generalFont, "Vita: " + Convert.ToString(pokemonAvversario.vita), new Vector2(400, 40), Color.Black);

                        _spriteBatch.Draw(mioPokemon.retro, new Vector2(200, 200), Color.White);
                        _spriteBatch.DrawString(generalFont, mioPokemon.nome, new Vector2(270, 200), Color.Black);
                        _spriteBatch.DrawString(generalFont, "Vita: " + Convert.ToString(mioPokemon.vita), new Vector2(270, 220), Color.Black);
                    }


                }
                if (mioTurno)
                {
                    _spriteBatch.DrawString(generalFont, "tuo turno", new Vector2(700, 0), Color.Black);
                }
                else
                {
                    _spriteBatch.DrawString(generalFont, "turno avversario", new Vector2(650, 0), Color.Black);

                }
                _spriteBatch.DrawString(generalFont, messaggioTurno, new Vector2(650, 200), Color.Black);
            }
            _spriteBatch.DrawString(generalFont, debug, new Vector2(0, _graphics.PreferredBackBufferHeight - 25), Color.Black);
            _spriteBatch.End();
            base.Draw(gameTime);
        }
        private void riceviDati()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 42069);
            listener.Start();
            StreamWriter sw;
            StreamReader sr;

            secondPeer = listener.AcceptTcpClient();


            Socket s = secondPeer.Client;
            try
            {
                myPeer.Connect(((IPEndPoint)s.RemoteEndPoint).Address, 42069);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }


            while (true)
            {
                try
                {
                    sw = new StreamWriter(secondPeer.GetStream());
                    sr = new StreamReader(secondPeer.GetStream());
                    string strClientInput = sr.ReadLine();

                    if (strClientInput != null)
                    {
                        xmlDoc.LoadXml(strClientInput);

                        if (xmlDoc.GetElementsByTagName("comando")[0].InnerText == "m")
                        {
                            sw.WriteLine(String.Format("<root>" +
                            "<comando>m</comando>" +
                            "<nome>{0}</nome>" +
                            "</root>", nome));
                            sw.Flush();
                            sw.WriteLine(mieiPokemon.getPokemonByPos(0).ToXML());
                            sw.Flush();
                            mioPokemon = mieiPokemon.getPokemonByPos(0);
                        }
                        else if (xmlDoc.GetElementsByTagName("comando")[0].InnerText == "s")
                        {
                            nodoPokemon = xmlDoc.DocumentElement.ChildNodes[1];
                            pokemonAvversario = new Pokemon(nodoPokemon.ChildNodes[0].InnerText, nodoPokemon.ChildNodes[2].InnerText, Convert.ToInt32(nodoPokemon.ChildNodes[1].InnerText), this);
                            gameLogic = "battle";
                        }
                        else if (xmlDoc.GetElementsByTagName("comando")[0].InnerText == "a")
                        {
                            string mossaDaMandare = "";
                            if ((mioPokemon.tipo == "Fuoco" && (xmlDoc.GetElementsByTagName("tipoMossa")[0].InnerText == "Terra" || xmlDoc.GetElementsByTagName("tipoMossa")[0].InnerText == "Roccia" || xmlDoc.GetElementsByTagName("tipoMossa")[0].InnerText == "Acqua")) ||
                               (mioPokemon.tipo == "Acqua" && (xmlDoc.GetElementsByTagName("tipoMossa")[0].InnerText == "Erba" || xmlDoc.GetElementsByTagName("tipoMossa")[0].InnerText == "Elettro")) ||
                               (mioPokemon.tipo == "Erba" && (xmlDoc.GetElementsByTagName("tipoMossa")[0].InnerText == "Volante" || xmlDoc.GetElementsByTagName("tipoMossa")[0].InnerText == "Veleno" || xmlDoc.GetElementsByTagName("tipoMossa")[0].InnerText == "Coleottero" || xmlDoc.GetElementsByTagName("tipoMossa")[0].InnerText == "Fuoco" || xmlDoc.GetElementsByTagName("tipoMossa")[0].InnerText == "Ghiaccio")) ||
                               (mioPokemon.tipo == "Elettro" && (xmlDoc.GetElementsByTagName("tipoMossa")[0].InnerText == "Terra")) ||
                               (mioPokemon.tipo == "Terra" && (xmlDoc.GetElementsByTagName("tipoMossa")[0].InnerText == "Acqua" || xmlDoc.GetElementsByTagName("tipoMossa")[0].InnerText == "Erba" || xmlDoc.GetElementsByTagName("tipoMossa")[0].InnerText == "Ghiaccio")) ||
                               (mioPokemon.tipo == "Volante" && (xmlDoc.GetElementsByTagName("tipoMossa")[0].InnerText == "Elettro" || xmlDoc.GetElementsByTagName("tipoMossa")[0].InnerText == "Roccia" || xmlDoc.GetElementsByTagName("tipoMossa")[0].InnerText == "Ghiaccio")) ||
                               (mioPokemon.tipo == "Veleno" && (xmlDoc.GetElementsByTagName("tipoMossa")[0].InnerText == "Terra" || xmlDoc.GetElementsByTagName("tipoMossa")[0].InnerText == "Pisco")) ||
                               (mioPokemon.tipo == "Normale" && (xmlDoc.GetElementsByTagName("tipoMossa")[0].InnerText == "Lotta" || xmlDoc.GetElementsByTagName("tipoMossa")[0].InnerText == "Psico" || xmlDoc.GetElementsByTagName("tipoMossa")[0].InnerText == "Folletto")))
                            {
                                mioPokemon.vita = mioPokemon.vita - (Convert.ToInt32(xmlDoc.GetElementsByTagName("danni")[0].InnerText) * 2);
                                if (mioPokemon.vita <= 0)
                                {
                                    battleLogic = "Cambia";
                                    mioTurno = false;
                                    mossaDaMandare = String.Format("<root>" +
                                "<comando>r</comando>" +
                                "<vitaRimanente>{0}</vitaRimanente>" +
                                "<moltiplicatore>{1}</moltiplicatore>" +
                                "</root>", mioPokemon.vita, 2);
                                }
                                else
                                {
                                    mossaDaMandare = String.Format("<root>" +
                                "<comando>r</comando>" +
                                "<vitaRimanente>{0}</vitaRimanente>" +
                                "<moltiplicatore>{1}</moltiplicatore>" +
                                "</root>", mioPokemon.vita, 2);
                                    mioTurno = true;
                                }

                            }
                            else
                            {
                                mioPokemon.vita = mioPokemon.vita - Convert.ToInt32(xmlDoc.GetElementsByTagName("danni")[0].InnerText);
                                if (mioPokemon.vita <= 0)
                                {
                                    battleLogic = "Cambia";
                                    mioTurno = false;
                                    mossaDaMandare = String.Format("<root>" +
                                "<comando>r</comando>" +
                                "<vitaRimanente>{0}</vitaRimanente>" +
                                "<moltiplicatore>{1}</moltiplicatore>" +
                                "</root>", mioPokemon.vita, 2);
                                }
                                else
                                {
                                    mossaDaMandare = String.Format("<root>" +
                                "<comando>r</comando>" +
                                "<vitaRimanente>{0}</vitaRimanente>" +
                                "<moltiplicatore>{1}</moltiplicatore>" +
                                "</root>", mioPokemon.vita, 2);
                                    mioTurno = true;
                                }



                            }
                            sw.WriteLine(mossaDaMandare);
                            sw.Flush();
                        }
                        else if (xmlDoc.GetElementsByTagName("comando")[0].InnerText == "i")
                        {
                            string oggetto = sr.ReadLine();
                            xmlDoc.LoadXml(oggetto);
                            pokemonAvversario.vita = Convert.ToInt32(xmlDoc.GetElementsByTagName("vitaAttuale")[0].InnerText);
                            mioTurno = true;
                        }
                        else if (xmlDoc.GetElementsByTagName("comando")[0].InnerText == "f")
                        {
                            Exit();
                        }
                        else if (xmlDoc.GetElementsByTagName("comando")[0].InnerText == "c")
                        {
                            string tmp = strClientInput;
                            pokemonAvversario = allPokemon.getPokemonByName(xmlDoc.GetElementsByTagName("nome")[0].InnerText);
                            pokemonAvversario.vita = Convert.ToInt32(xmlDoc.GetElementsByTagName("vita")[0].InnerText);
                            mioTurno = true;
                        }
                        else if (xmlDoc.GetElementsByTagName("comando")[0].InnerText == "l")
                        {
                            c++;
                            battleLogic = "Cambia";
                            mioTurno = true;
                            if (mieiPokemon.getNumeroPokemonScelti() == c)
                            {
                                Exit();
                            }

                        }
                        else if (xmlDoc.GetElementsByTagName("comando")[0].InnerText == "e")
                        {
                            sw.WriteLine(" ");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
            sw.Close();
            sr.Close();

        } //Cosa esegue il secondo peer
        private void eseguiTurno() //se è il turno del peer, viene eseguito
        {
            var mouseState = Mouse.GetState(); //vado a prendere lo stato del mouse
            Vector2 mousePosition = new Vector2(mouseState.X, mouseState.Y); //vado a prendere la posizione del mouse
            if (battleLogic.Equals("Attacca"))
            {
                if (mousePosition.X >= 21 && mousePosition.X <= 21 + 136 && mousePosition.Y >= 398 && mousePosition.Y <= 398 + 60) //bottone schermata attacca
                {
                    battleLogic = "";
                }
                else
                {
                    Mossa mossaScelta = new Mossa();
                    if (mousePosition.X >= 182 && mousePosition.X <= 182 + 190 && mousePosition.Y >= 328 && mousePosition.Y <= 328 + 60)
                        mossaScelta = mioPokemon.mosse[0];
                    else if (mousePosition.X >= 423 && mousePosition.X <= 423 + 190 && mousePosition.Y >= 328 && mousePosition.Y <= 328 + 60)
                        mossaScelta = mioPokemon.mosse[1];
                    else if (mousePosition.X >= 182 && mousePosition.X <= 182 + 190 && mousePosition.Y >= 398 && mousePosition.Y <= 398 + 60)
                        mossaScelta = mioPokemon.mosse[2];
                    else if (mousePosition.X >= 423 && mousePosition.X <= 423 + 190 && mousePosition.Y >= 398 && mousePosition.Y <= 398 + 60)
                        mossaScelta = mioPokemon.mosse[3];




                    string mossaDaMandare = String.Format("<root>" +
                    "<comando>a</comando>" +
                    "<nomeMossa>{0}</nomeMossa>" +
                    "<tipoMossa>{1}</tipoMossa>" +
                    "<danni>{2}</danni>" +
                    "</root>", mossaScelta.nome, mossaScelta.tipo, mossaScelta.danni);

                    swFP = new StreamWriter(myPeer.GetStream());
                    srFP = new StreamReader(myPeer.GetStream());
                    swFP.WriteLine(mossaDaMandare);
                    swFP.Flush();

                    strPokemonSceltoAvversario = srFP.ReadLine();

                    xmlDoc.LoadXml(strPokemonSceltoAvversario);

                    pokemonAvversario.vita = Convert.ToInt32(xmlDoc.GetElementsByTagName("vitaRimanente")[0].InnerText);

                    if (Convert.ToInt32(xmlDoc.GetElementsByTagName("moltiplicatore")[0].InnerText) == 0)
                    {
                        messaggioTurno = "danno normale";
                        if (Convert.ToInt32(xmlDoc.GetElementsByTagName("vitaRimanente")[0].InnerText) <= 0)
                        {
                            string sconfittoDaMandare = String.Format("<root>" +
                       "<comando>l</comando>" +
                       "<pokemon>{0}</pokemon>" +
                       "</root>", pokemonAvversario.nome);
                            swFP.WriteLine(sconfittoDaMandare);
                            swFP.Flush();

                        }
                        mioTurno = false;

                    }
                    else if (Convert.ToInt32(xmlDoc.GetElementsByTagName("moltiplicatore")[0].InnerText) == 1)
                    {
                        messaggioTurno = "danno ridotto";
                        if (Convert.ToInt32(xmlDoc.GetElementsByTagName("vitaRimanente")[0].InnerText) <= 0)
                        {
                            string sconfittoDaMandare = String.Format("<root>" +
                       "<comando>l</comando>" +
                       "<pokemon>{0}</pokemon>" +
                       "</root>", pokemonAvversario.nome);
                            swFP.WriteLine(sconfittoDaMandare);
                            swFP.Flush();

                        }
                        mioTurno = false;

                    }
                    else if (Convert.ToInt32(xmlDoc.GetElementsByTagName("moltiplicatore")[0].InnerText) == 2)
                    {
                        messaggioTurno = "danno superefficace";
                        if (Convert.ToInt32(xmlDoc.GetElementsByTagName("vitaRimanente")[0].InnerText) <= 0)
                        {
                            string sconfittoDaMandare = String.Format("<root>" +
                       "<comando>l</comando>" +
                       "<pokemon>{0}</pokemon>" +
                       "</root>", pokemonAvversario.nome);
                            swFP.WriteLine(sconfittoDaMandare);
                            swFP.Flush();

                        }
                        mioTurno = false;
                    }
                    else if (Convert.ToInt32(xmlDoc.GetElementsByTagName("moltiplicatore")[0].InnerText) == 3)
                    {
                        messaggioTurno = "danno critico";
                        if (Convert.ToInt32(xmlDoc.GetElementsByTagName("vitaRimanente")[0].InnerText) <= 0)
                        {
                            string sconfittoDaMandare = String.Format("<root>" +
                       "<comando>l</comando>" +
                       "<pokemon>{0}</pokemon>" +
                       "</root>", pokemonAvversario.nome);
                            swFP.WriteLine(sconfittoDaMandare);
                            swFP.Flush();

                        }
                        mioTurno = false;
                    }
                    else if (Convert.ToInt32(xmlDoc.GetElementsByTagName("moltiplicatore")[0].InnerText) == 4)
                    {
                        messaggioTurno = "no danno";
                        if (Convert.ToInt32(xmlDoc.GetElementsByTagName("vitaRimanente")[0].InnerText) <= 0)
                        {
                            string sconfittoDaMandare = String.Format("<root>" +
                       "<comando>l</comando>" +
                       "<pokemon>{0}</pokemon>" +
                       "</root>", pokemonAvversario.nome);
                            swFP.WriteLine(sconfittoDaMandare);
                            swFP.Flush();

                        }
                        mioTurno = false;
                    }


                }
            }
            else if (battleLogic.Equals("Zaino"))
            {
                swFP = new StreamWriter(myPeer.GetStream());
                srFP = new StreamReader(myPeer.GetStream());


                string oggetto = "pozione";
                string oggettoDaMandare = String.Format("<root>" +
                    "<comando>i</comando>" +
                    "<oggetto>{0}</oggetto>" +
                    "<pokemon>{1}</pokemon>" +
                    "<vitaAttuale>{2}</vitaAttuale>" +
                    "</root>", oggetto, mioPokemon.nome, mioPokemon.vita);

                swFP.WriteLine(oggettoDaMandare);
                swFP.Flush();

            }
            else if (battleLogic.Equals("Cambia"))
            {
                swFP = new StreamWriter(myPeer.GetStream());
                srFP = new StreamReader(myPeer.GetStream());

                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    var pokemonSelezionato = mieiPokemon.cambiaPokemon(mouseState);
                    if (pokemonSelezionato != null)
                    {
                        mioPokemon = pokemonSelezionato;
                        battleLogic = "";
                        /*DA QUI INSERIRE LA CONNESSIONE TCP DOVE INVIARE LE INFORMAZIONI AL SECONDO PEER*/

                        string pokemonDaMandare = String.Format("<root>" +
                                        "<comando>c</comando>" +
                                        "<pokemon>" +
                                            "<nome>{0}</nome>" +
                                            "<vita>{1}</vita>" +
                                            "<tipo>{2}</tipo>" +
                                            "</pokemon>" +
                                      "</root>", mioPokemon.nome, mioPokemon.vita, mioPokemon.tipo);

                        swFP.WriteLine(pokemonDaMandare);
                        swFP.Flush();
                        mioTurno = false;
                    }
                }
            }
            else if (battleLogic.Equals("Fuga"))
            {
                swFP = new StreamWriter(myPeer.GetStream());
                srFP = new StreamReader(myPeer.GetStream());
                swFP.WriteLine("<root>" +
                    "<comando>f<comando>" +
                    "</root>");
                swFP.Flush();
                base.Exit();
            }
        }
        private void audioEntrataPokemon()
        {
            audioFile = new AudioFileReader("POKEMON BATTLE START SOUND EFFECT.mp3");
            outputDevice.Init(audioFile);
            outputDevice.Play();
            eseguito = true;
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

    }
}