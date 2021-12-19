using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BattagliaPokemon
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont generalFont;
        private Texture2D personaggio; // 96 x 104 px
        private Texture2D confirmButton; // 252 x 91 px
        private AllPokemon ap;
        private PokemonScelti ps;
        private string nome;
        private string gameLogic = "gameStart";
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
            personaggio = Content.Load<Texture2D>("pg");
            confirmButton = Content.Load<Texture2D>("confButton");

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
                        if ((mousePosition.X >= 250 && mousePosition.X <= 250 + 252) && (mousePosition.Y >= 350 && mousePosition.Y <= 350 + 91))
                            gameLogic = "sceltaPokemon";

            }
            else
            {
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    var tmp = ap.CheckPokemonPremuto(mousePosition);
                    if (tmp != null)
                        ps.addPokemon(tmp);
                    if (mouseState.LeftButton == ButtonState.Pressed)
                        if (!ps.showAll().Equals(""))
                            if ((mousePosition.X >= 250 && mousePosition.X <= 250 + 252) && (mousePosition.Y >= 350 && mousePosition.Y <= 350 + 91))
                                gameLogic = "Battle";

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
                _spriteBatch.DrawString(generalFont, nome, new Vector2(250, 200), Color.Black);
                _spriteBatch.Draw(personaggio, new Vector2(250, 50), Color.White);
                _spriteBatch.Draw(confirmButton, new Vector2(250, 350), Color.White);
            }
            else if (gameLogic.Equals("sceltaPokemon"))
            {
                Pokemon[] allpokemon = ap.getPokemon();
                for (int i = 0; i < allpokemon.Length; i++)
                {
                    _spriteBatch.Draw(allpokemon[i].front, allpokemon[i].posizione, Color.White);
                }
                _spriteBatch.DrawString(generalFont, ps.showAll(), new Vector2(0, 100), Color.Black);
                _spriteBatch.Draw(confirmButton, new Vector2(250, 350), Color.White);
            }
            _spriteBatch.End();
            base.Draw(gameTime);
        }
        private void TextInputHandler(object sender, TextInputEventArgs args)
        {
            var character = args.Character;
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
    }
}
