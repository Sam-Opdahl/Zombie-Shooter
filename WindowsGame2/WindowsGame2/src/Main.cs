using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace WindowsGame2
{
    public enum Font {
        Motorwerk = 0,
        Motorwerk2
    }

    public enum GameState {
        PlayGame,
        MainMenu
    }

    public class Main : Microsoft.Xna.Framework.Game
    {
        public static Texture2D blankTexture;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        ContentManager contentManager;

        private GameState gameState;
        private GameState transitionState;
        private int transitionAlpha;
        private bool isTransitioning = false;

        private Input input;
        private GameScreen gameScreen;
        public MainScreen mainScreen;

        private static List<SpriteFont> fontList = new List<SpriteFont>();
        private static string[] fontsToLoad = { 
            Font.Motorwerk.ToString(),
            Font.Motorwerk2.ToString()
        };
        

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            input = new Input();
            mainScreen = new MainScreen(this);
            gameState = GameState.MainMenu;
        }

        protected override void Initialize()
        {
            this.IsMouseVisible = true;
            Window.Title = "Zombies Must Die";

            base.Initialize();
        }


        protected override void LoadContent()
        {
            contentManager = this.Content;

            spriteBatch = new SpriteBatch(GraphicsDevice);

            foreach (string font in fontsToLoad) {
                fontList.Add(Content.Load<SpriteFont>("Fonts\\" + font));
            }

            //gameScreen.LoadContent(contentManager, graphics.GraphicsDevice);
            mainScreen.LoadContent(Content, graphics.GraphicsDevice);

            blankTexture = new Texture2D(graphics.GraphicsDevice, 1, 1);
            blankTexture.SetData(new Color[] { Color.White });

            SoundManager.getInstance().LoadContent(Content);
        }

        public void transitonToState(GameState state) {
            isTransitioning = true;
            transitionState = state;
            transitionAlpha = 0;
        }

        protected override void Update(GameTime gameTime)
        {
            if (!this.IsActive) {
                return;
            }

            input.Update();

            if (isTransitioning) {
                transitionAlpha += 3;
                if (transitionAlpha > 255) {
                    if (MediaPlayer.State == MediaState.Playing) {
                        MediaPlayer.Stop();
                    }

                    isTransitioning = false;
                    gameState = transitionState;
                    if (gameState == GameState.PlayGame) {
                        gameScreen = new GameScreen(this, mainScreen.SelectedMap);
                        gameScreen.LoadContent(contentManager, graphics.GraphicsDevice);
                        gameScreen.Update(gameTime, input);
                    }
                }
                return;
            }

            switch (gameState) {
                case GameState.MainMenu:
                    mainScreen.Update(gameTime, input);
                    break;
                case GameState.PlayGame:
                    gameScreen.Update(gameTime, input);
                    break;
                default:
                    break;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            switch (gameState) {
                case GameState.MainMenu:
                    mainScreen.Draw(spriteBatch);
                    break;
                case GameState.PlayGame:
                    gameScreen.Draw(spriteBatch);
                    break;
                default:
                    break;
            }

            if (isTransitioning) {
                int w = graphics.GraphicsDevice.Viewport.Width;
                int h = graphics.GraphicsDevice.Viewport.Height;
                spriteBatch.Begin();
                spriteBatch.Draw(blankTexture, new Rectangle(0, 0, w, h), new Color(0, 0, 0, transitionAlpha));
                spriteBatch.End();
            }

            base.Draw(gameTime);
        }

        public static SpriteFont getFont(Font font) {
            return fontList[(int)font];
        }
    }
}
