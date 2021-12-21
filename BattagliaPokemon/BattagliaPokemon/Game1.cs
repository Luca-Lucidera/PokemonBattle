using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

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
        private AllPokemon ap;
        private PokemonScelti ps;

        private string nome;
        private string gameLogic = "gameStart";
        private string ipAvversario = ""; 
        
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
            else if(gameLogic.Equals("sceltaPokemon"))
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
                if(mouseState.LeftButton == ButtonState.Pressed)
                    if(!ipAvversario.Equals(""))
                    {
                        //inserire qui la logica per fare la connessione con l'altro peer

                        //se il tipo accetta
                        gameLogic = "battle";
                        //se non accetta gameLogic = "ipSelection"
                    }
            }
            else if (gameLogic.Equals("battle"))
            {

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
    }
}
