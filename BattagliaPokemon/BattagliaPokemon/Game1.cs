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

        private SpriteFont generalFont;
        private Texture2D personaggio; // 96 x 104 px
        private Texture2D confirmButton; // 252 x 91 px
        private Texture2D test;
        private Texture2D BattleTexture;


        private AllPokemon ap;
        private PokemonScelti ps;
        private Pokemon pokemonAvversario;

        private string nome;
        private string gameLogic = "gameStart";
        private string ipAvversario = "";

        string pokemonSceltoPeer1;
        string pokeomSceltoPeer2;

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
            ap = new AllPokemon(this, _graphics);
            ps = new PokemonScelti();
            generalFont = Content.Load<SpriteFont>("Font");
            personaggio = Content.Load<Texture2D>("sceltaNomeD1");
            confirmButton = Content.Load<Texture2D>("confButton");
            test = Content.Load<Texture2D>("ipselect");
            BattleTexture = Content.Load<Texture2D>("Desktop - Battaglia");

            pokemonSceltoPeer1 = "";
            pokeomSceltoPeer2 = "";
            nome = "";
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            var mouseState = Mouse.GetState();
            Vector2 mousePosition = new Vector2(mouseState.X, mouseState.Y);

            if (gameLogic.Equals("gameStart"))
            {
                if (mouseState.LeftButton == ButtonState.Pressed)
                    if (!nome.Equals(""))
                        if ((mousePosition.X >= 325 && mousePosition.X <= 325 + 150) && (mousePosition.Y >= 410 && mousePosition.Y <= 410 + 25))
                            gameLogic = "sceltaPokemon";
            }
            else if (gameLogic.Equals("sceltaPokemon"))
            {
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    var tmp = ap.CheckPokemonPremuto(mousePosition);
                    if (tmp != null)
                        ps.addPokemon(tmp);
                    if (mouseState.LeftButton == ButtonState.Pressed)
                        if (!ps.showAll().Equals(""))
                            if ((mousePosition.X >= 250 && mousePosition.X <= 250 + 252) && (mousePosition.Y >= 350 && mousePosition.Y <= 350 + 91))
                                gameLogic = "ipSelection";
                }
            }
            else if (gameLogic.Equals("ipSelection"))
            {
                if (mouseState.LeftButton == ButtonState.Pressed)
                    if (!ipAvversario.Equals(""))
                    {
                        myPeer = new TcpClient();
                        myPeer.Connect(ipAvversario, 4269);
                        //da mandare il primo pokemon scelto (peer 1)
                        string daMandare = "m";
                        StreamWriter sw = new StreamWriter(myPeer.GetStream());
                        StreamReader sr = new StreamReader(myPeer.GetStream());
                        sw.WriteLine(daMandare);
                        sw.Flush();
                        pokeomSceltoPeer2 = sr.ReadLine();
                        gameLogic = "battle";
                        //se non accetta gameLogic = "ipSelection"
                        sr.Close();
                        sw.Close();
                        
                    }
            }
            else if (gameLogic.Equals("battle"))
            {
                /*
                if (done == false)
                {
                    myPeer = new TcpClient();
                    myPeer.Connect(ipAvversario, 4269);
                    StreamWriter sw = new StreamWriter(myPeer.GetStream());
                    StreamReader sr = new StreamReader(myPeer.GetStream());
                    sw.WriteLine("s");
                    sw.Flush();
                    string s = sr.ReadLine();
                    Console.WriteLine(s);
                    sr.Close();
                    done = true;
                }
                */
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
                Pokemon[] allpokemon = ap.getPokemon();
                int space = 0;
                int lastX = 0;
                for (int i = 0; i < allpokemon.Length; i++)
                {
                    _spriteBatch.Draw(allpokemon[i].front, new Vector2(allpokemon[i].posizione.X + space, allpokemon[i].posizione.Y), Color.White);
                    _spriteBatch.DrawString(generalFont, allpokemon[i].nome, new Vector2(allpokemon[i].posizione.X + space, allpokemon[i].posizione.Y + 90), Color.Black);
                    space += 40;
                    lastX = Convert.ToInt32(allpokemon[i].posizione.X);
                }
                _spriteBatch.DrawString(generalFont, "hai selezionato: " + ps.showAll(), new Vector2(0, 200), Color.Black);
                _spriteBatch.Draw(confirmButton, new Vector2(250, 350), Color.White);
            }
            else if (gameLogic.Equals("ipSelection"))
            {
                _spriteBatch.Draw(test, new Vector2(0, 0), Color.White);
                _spriteBatch.DrawString(generalFont, ipAvversario, new Vector2(380, 175), Color.Black);
            }
            else if (gameLogic.Equals("battle"))
            {
                Pokemon[] allpokemon = ap.getPokemon();
                _spriteBatch.Draw(BattleTexture, new Vector2(0, 0), Color.White);
                for (int i = 0; i < allpokemon.Length; i++)
                {
                    if(pokeomSceltoPeer2 == allpokemon[i].nome)
                    {
                        _spriteBatch.Draw(allpokemon[i].front, new Vector2(500, 20), Color.White);
                        _spriteBatch.DrawString(generalFont,allpokemon[i].nome, new Vector2(400, 20), Color.Black);
                        _spriteBatch.DrawString(generalFont, "Vita: "+Convert.ToString(allpokemon[i].vita), new Vector2(400, 40), Color.Black);
                    }
                    if (pokemonSceltoPeer1 == allpokemon[i].nome)
                    {
                        _spriteBatch.Draw(allpokemon[i].retro, new Vector2(200, 200), Color.White);
                        _spriteBatch.DrawString(generalFont, allpokemon[i].nome, new Vector2(270, 200), Color.Black);
                        _spriteBatch.DrawString(generalFont, "Vita: " + Convert.ToString(allpokemon[i].vita), new Vector2(270, 220), Color.Black);
                    }
                }
            }
            _spriteBatch.End();
            base.Draw(gameTime);
        }
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
            TcpListener listener = new TcpListener(IPAddress.Any, 4269);
            listener.Start();
            while (true)
            {
                secondPeer = listener.AcceptTcpClient();
            
                StreamWriter sw = new StreamWriter(secondPeer.GetStream());
                StreamReader sr = new StreamReader(secondPeer.GetStream());
                //deve ritornare il primo pokemon scelto (peer 2)

                String strClientInput = sr.ReadLine();
                if (strClientInput == "m") //peer 1 contatta peer 2 e il peer 2 gli manda il suo pokemon
                {
                    gameLogic = "battle";
                    string nomeAvversario = "da prendere dall'xml ricevuto";//da sostituire con l'xml
                    pokeomSceltoPeer2 = ps.getPokemonByPos(0);//da sostituire con l'xml
                    sw.WriteLine(pokeomSceltoPeer2);
                    sw.Flush();
                }
                else if(strClientInput == "s")  //peer 1 riceve i pokemon del peer 2 e quindi il peer 1 invia il suo pokemon
                {
                    pokemonSceltoPeer1 = ps.getPokemonByPos(0);//da sostituire con l'xml
                    sw.WriteLine(pokemonSceltoPeer1);
                    sw.Flush();
                }
                else if (strClientInput == "a")
                {
                    string mioPokemon = "r;Charmander;150;1";
                    sw.WriteLine(mioPokemon);
                    sw.Flush();
                }
                sw.Close();
                sr.Close();
            }
        }
    }
}