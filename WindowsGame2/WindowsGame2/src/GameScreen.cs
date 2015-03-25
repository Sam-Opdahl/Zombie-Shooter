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

namespace WindowsGame2 {

    enum PlayGameState {
        Playing,
        Paused,
        GameOver,
        WaveOver,
        ShopState
    }

    class GameScreen {

        private static readonly int HEALTH_BAR_WIDTH = 95;
        private static readonly int HEALTH_BAR_HEIGHT = 18;
        private static readonly int HEALTH_BAR_X = 67;
        private static readonly int HEALTH_BAR_Y = 411;

        private static readonly int BULLET_WIDTH = 95;
        private static readonly int BULLET_HEIGHT = 18;
        private static readonly int BULLET_X = 67;
        private static readonly int BULLET_Y = 434;

        private static readonly int PORTRAIT_X = 10;
        private static readonly int PORTRAIT_Y = 408;
        private static readonly int PORTRAIT_SIZE = 48;

        private static readonly float WAVE_OVER_TIME_MAX = 3f;
        private static readonly float DISPLAY_WAVE_OVER_TIME_START = 0f;
        private static readonly float WAVE_OVER_ALPHA_START = 0f;
        private static readonly float WAVE_OVER_TRANSITION_SPEED = 0.01f;

        private static readonly String INTERFACE_LOCATION = "Graphics\\player_interface";
        private static readonly String INTERFACE2_LOCATION = "Graphics\\interfacemoney";
        private static readonly String PORTRAIT_LOCATION = "Graphics\\player_portrait";
        private static readonly String CROSSHAIR_LOCATION = "Graphics\\crosshair";

        private Texture2D playerInterface;
        private Texture2D moneyInterface;
        private Texture2D playerPortrait;
        private Texture2D crosshair;
        public PlayGameState state;
        private PlayGameState stateBeforePause;
        private GraphicsDevice graphicsDevice;

        private SpriteFont menuItemFont;
        private Rectangle menuRect;
        private Rectangle statsRect;

        private string[] menuItems = {
            "Main Menu",
            "Exit"
        };
        private List<Rectangle> menuItemRect = new List<Rectangle>();
        private List<Color> menuItemColors = new List<Color>();
        public ShopScreen shopScreen;

        public World world;
        private Main mainClass;

        private bool displayWaveOver = true;
        private bool transitionIn = true;
        private float displayWaveOverTime = DISPLAY_WAVE_OVER_TIME_START;
        private float waveOverAlpha = WAVE_OVER_ALPHA_START;
        private float waveOverRedTint = 1f;

        private Rectangle gameOverRect;
        private int gameOverAlpha = 120;

        private string sfx = "Sounds Effects";
        private Rectangle sfxRect;
        private string music = "music";
        private Rectangle musicRect;

        private Vector2 crosshairPosition;
        private bool usedCheats = false;

        public GameScreen(Main mainClass, int map) {
            world = new World(this, map);
            state = PlayGameState.Playing;
            this.mainClass = mainClass;
            shopScreen = new ShopScreen(this);

            world.Wave = 1;
        }

        public void LoadContent(ContentManager content, GraphicsDevice graphics) {
            graphicsDevice = graphics;
            world.LoadContent(content, graphics);
            shopScreen.LoadContent(graphics);

            playerInterface = content.Load<Texture2D>(INTERFACE_LOCATION);
            moneyInterface = content.Load<Texture2D>(INTERFACE2_LOCATION);
            playerPortrait = content.Load<Texture2D>(PORTRAIT_LOCATION);
            crosshair = content.Load<Texture2D>(CROSSHAIR_LOCATION);

            int w = 250;
            int x = 85;// (graphics.Viewport.Width - w) / 2;
            menuRect = new Rectangle(x, 50, w, 400);
            int w2 = 325;
            statsRect = new Rectangle((x) + 30 + w, 50, w2, 400);

            menuItemFont = Main.getFont(Font.Motorwerk);
            int startX = menuRect.X + 10;
            int startY = menuRect.Y + 60;
            int curY = 0;
            foreach (string item in menuItems) {
                Vector2 size = menuItemFont.MeasureString(item);
                menuItemRect.Add(new Rectangle(startX, startY + curY, (int)size.X, (int)size.Y));
                menuItemColors.Add(Color.White);
                curY += (int)size.Y;
            }

            gameOverRect = new Rectangle(0, 0, graphics.Viewport.Width, 0);

            Vector2 sv = menuItemFont.MeasureString(sfx);
            sfxRect = new Rectangle(650, 10, (int)sv.X, (int)sv.Y);
            Vector2 mv = menuItemFont.MeasureString(music);
            musicRect = new Rectangle(650, 30, (int)mv.X, (int)mv.Y);

            world.startNewWave();
        }

        public void Update(GameTime gameTime, Input input) {

            switch (state) {
                case PlayGameState.Playing:
                    if (mainClass.IsMouseVisible) {
                        mainClass.IsMouseVisible = false;
                    }

                    UpdateCrosshair(input);

                    if (MediaPlayer.State == MediaState.Stopped) {
                        SoundManager.getInstance().playMusic(Music.GameMusic);
                    } else if (MediaPlayer.State == MediaState.Paused) {
                        MediaPlayer.Resume();
                    }

                    if (input.isKeyPressed(Keys.Escape)) {
                        state = PlayGameState.Paused;
                        stateBeforePause = PlayGameState.Playing;
                    }
                    world.Update(gameTime, input);

                    if (world.player.CurrentHealth <= 0) {
                        world.player.setNormalColor();
                        state = PlayGameState.GameOver;
                        SoundManager.getInstance().playMusic(Music.DeathSong);
                        break;
                    }
                    if (world.zombieManager.isWaveOver) {
                        SoundManager.getInstance().playSound(Sound.WaveOver);
                        state = PlayGameState.WaveOver;
                        displayWaveOver = true;
                        transitionIn = true;
                        displayWaveOverTime = DISPLAY_WAVE_OVER_TIME_START;
                        waveOverAlpha = WAVE_OVER_ALPHA_START;
                        waveOverRedTint = 1;
                        break;
                    }
                    break;
                case PlayGameState.WaveOver:
                    if (mainClass.IsMouseVisible) {
                        mainClass.IsMouseVisible = false;
                    }

                    UpdateCrosshair(input);

                    if (MediaPlayer.State == MediaState.Playing) {
                        MediaPlayer.Stop();
                    }

                    int playerMaxHealth = shopScreen.getPlayerAttribute(PlayerAttribute.MaxHealth);
                    if (world.player.CurrentHealth < playerMaxHealth) {
                        world.player.CurrentHealth++;
                    }

                    //CHEATS! :O
                    if (input.isKeyDown(Keys.LeftControl) && input.isKeyDown(Keys.M) && 
                        input.isKeyDown(Keys.L) && input.isKeyPressed(Keys.G)) {
                            world.player.modifyMoney(1000000);
                            SoundManager.getInstance().playSound(Sound.UpgradeSuccess);
                            usedCheats = true;
                    }

                    if (input.isKeyPressed(Keys.Escape)) {
                        state = PlayGameState.Paused;
                        stateBeforePause = PlayGameState.WaveOver;
                        break;
                    }
                    if (input.isKeyPressed(Keys.Tab)) {
                        state = PlayGameState.ShopState;
                        break;
                    }
                    world.Update(gameTime, input);
                    waveOverUpdate(gameTime);
                    break;
                case PlayGameState.ShopState:
                    if (!mainClass.IsMouseVisible) {
                        mainClass.IsMouseVisible = true;
                    }

                    if (input.isKeyPressed(Keys.Escape) || input.isKeyPressed(Keys.Tab)) {
                        state = PlayGameState.WaveOver;
                        shopScreen.handleClose();
                        break;
                    }
                    shopScreen.Update(gameTime, input);

                    break;
                case PlayGameState.Paused:
                    if (!mainClass.IsMouseVisible) {
                        mainClass.IsMouseVisible = true;
                    }

                    if (MediaPlayer.State == MediaState.Playing) {
                        MediaPlayer.Pause();
                    }

                    SoundManager instance = SoundManager.getInstance();
                    if (input.getMouseRect().Intersects(sfxRect) && input.isMouseClicked()) {
                        instance.soundFxEnabled = !instance.soundFxEnabled;
                    }
                    if (input.getMouseRect().Intersects(musicRect) && input.isMouseClicked()) {
                        instance.musicEnabled = !instance.musicEnabled;
                        if (instance.musicEnabled) {
                            instance.playMusic(Music.MainMenu);
                        } else {
                            MediaPlayer.Stop();
                        }
                    }

                    if (input.isKeyPressed(Keys.Escape) || input.isKeyPressed(Keys.W) || input.isKeyPressed(Keys.A) 
                        || input.isKeyPressed(Keys.S) || input.isKeyPressed(Keys.D)) {
                        state = stateBeforePause;
                    }
                    break;
                case PlayGameState.GameOver:
                    if (!mainClass.IsMouseVisible) {
                        mainClass.IsMouseVisible = true;
                    }

                    if (gameOverRect.Height < graphicsDevice.Viewport.Height) {
                        gameOverRect.Height+=2;
                        break;
                    }

                    if (gameOverAlpha < 255) {
                        gameOverAlpha++;
                        break;
                    }

                    Rectangle msRect = new Rectangle(input.ms.X, input.ms.Y, 1, 1);
                    for (int i = 0; i < menuItems.Length; i++) {
                        if (msRect.Intersects(menuItemRect[i])) {
                            menuItemColors[i] = Color.Red;

                            if (!input.getPrevMouseRect().Intersects(menuItemRect[i])) {
                                SoundManager.getInstance().playSound(Sound.MouseOver);
                            }

                            if (input.isMouseClicked()) {
                                switch (i) {
                                    case 0:
                                        mainClass.mainScreen.state = MainScreenState.MainScreen;
                                        mainClass.transitonToState(GameState.MainMenu);
                                        break;
                                    case 1:
                                        mainClass.Exit();
                                        break;
                                }
                            }
                        } else {
                            menuItemColors[i] = Color.White;
                        }
                    }
                    break;
            }
        }

        public void UpdateCrosshair(Input input) {
            crosshairPosition = new Vector2(input.ms.X - (crosshair.Width / 2), input.ms.Y - (crosshair.Height / 2));
        }

        public void waveOverUpdate(GameTime gameTime) {
            if (displayWaveOver) {
                if (transitionIn) {
                    if (waveOverAlpha >= 1f) {
                        displayWaveOverTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                        if (displayWaveOverTime > WAVE_OVER_TIME_MAX) {
                            transitionIn = false;
                        }
                    } else {
                        waveOverAlpha += WAVE_OVER_TRANSITION_SPEED;
                    }
                } else {
                    if (waveOverAlpha <= 0f) {
                        displayWaveOver = false;
                        transitionIn = true;
                    } else {
                        waveOverAlpha -= WAVE_OVER_TRANSITION_SPEED;
                    }
                }
            } else {
                if (transitionIn) {
                    if (waveOverAlpha >= 1f) {
                        waveOverRedTint -= WAVE_OVER_TRANSITION_SPEED;
                        if (waveOverRedTint <= 0) {
                            transitionIn = false;
                        }
                    } else {
                        waveOverAlpha += WAVE_OVER_TRANSITION_SPEED;
                    }
                } else {
                    if (waveOverAlpha <= 0) {
                        world.Wave++;
                        world.startNewWave();
                        state = PlayGameState.Playing;
                        SoundManager.getInstance().playSound(Sound.NewWave);
                    } else {
                        waveOverAlpha -= WAVE_OVER_TRANSITION_SPEED;
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch) {

            world.Draw(spriteBatch);

            spriteBatch.Begin();

            spriteBatch.DrawString(Main.getFont(Font.Motorwerk), "Wave: " + world.Wave, new Vector2(14, 14), Color.Black);
            spriteBatch.DrawString(Main.getFont(Font.Motorwerk), "Wave: " + world.Wave, new Vector2(10, 10), Color.White);

            //string zombiesLeft = "Zombies Left: " + world.zombieManager.ZombiesLeft;
            //spriteBatch.DrawString(Main.getFont(Font.Motorwerk), zombiesLeft, new Vector2(14, 29), Color.Black);
            //spriteBatch.DrawString(Main.getFont(Font.Motorwerk), zombiesLeft, new Vector2(10, 25), Color.White);

            int maxHealth = shopScreen.getPlayerAttribute(PlayerAttribute.MaxHealth);
            int curHealth = world.player.CurrentHealth;
            float healthPercent = (float)curHealth / maxHealth;
            int damageStage = Math.Min((int)Math.Floor((float)(4 * (maxHealth - curHealth)) / maxHealth), 3);

            spriteBatch.Draw(playerInterface, new Vector2(0, 395), Color.White);
            int moneyX = graphicsDevice.Viewport.Width - moneyInterface.Width;
            spriteBatch.Draw(moneyInterface, new Vector2(moneyX, 395), Color.White);
            spriteBatch.Draw(playerPortrait, new Vector2(PORTRAIT_X, PORTRAIT_Y), new Rectangle(damageStage * PORTRAIT_SIZE, 0, PORTRAIT_SIZE, PORTRAIT_SIZE), Color.White);
            spriteBatch.Draw(world.blankTexture, new Rectangle(HEALTH_BAR_X, HEALTH_BAR_Y, HEALTH_BAR_WIDTH, HEALTH_BAR_HEIGHT), Color.Red);
            spriteBatch.Draw(world.blankTexture, new Rectangle(HEALTH_BAR_X, HEALTH_BAR_Y, (int)Math.Round(HEALTH_BAR_WIDTH * healthPercent), HEALTH_BAR_HEIGHT), Color.Green);
            string money = "$" + string.Format("{0:n0}", world.player.MoneyAmount);
            spriteBatch.DrawString(Main.getFont(Font.Motorwerk2), money, new Vector2(moneyX + 13, 418), Color.Black);
            spriteBatch.DrawString(Main.getFont(Font.Motorwerk2), money, new Vector2(moneyX + 10, 415), Color.White);

            int maxClipSize = shopScreen.getGunAttribute(GunAttribute.ClipSize);
            int curClipSize = world.bulletManager.clipSize;
            float clipPercent = (float)curClipSize / maxClipSize;
            spriteBatch.Draw(world.blankTexture, new Rectangle(BULLET_X, BULLET_Y, (int)Math.Round(BULLET_WIDTH * clipPercent), BULLET_HEIGHT), Color.Blue);
            string clipSize = curClipSize + "/" + maxClipSize;
            int bx = BULLET_X + (BULLET_WIDTH / 2) - (int)(Main.getFont(Font.Motorwerk).MeasureString(clipSize).X / 2);
            spriteBatch.DrawString(Main.getFont(Font.Motorwerk), clipSize, new Vector2(bx, BULLET_Y-2), curClipSize == 0 ? Color.Red : (curClipSize < Math.Round((double)maxClipSize / 2) ? Color.Yellow : Color.White));

            if (world.bulletManager.reloading) {
                spriteBatch.DrawString(Main.getFont(Font.Motorwerk), "Reloading...", new Vector2(173, BULLET_Y+1), Color.Black);
                spriteBatch.DrawString(Main.getFont(Font.Motorwerk), "Reloading...", new Vector2(170, BULLET_Y - 2), Color.White);
            }

            if (world.bulletManager.clipSize == 0 && !world.bulletManager.hasReloaded) {
                spriteBatch.DrawString(Main.getFont(Font.Motorwerk), "Press R to reload", new Vector2(173, BULLET_Y + 1), Color.Black);
                spriteBatch.DrawString(Main.getFont(Font.Motorwerk), "Press R to reload", new Vector2(170, BULLET_Y - 2), Color.White);
            }

            switch (state) {                
                case PlayGameState.Paused:
                    SpriteFont font = Main.getFont(Font.Motorwerk2);
                    string s = "PAUSED";
                    int x = (graphicsDevice.Viewport.Width - (int)font.MeasureString(s).X) / 2;
                    spriteBatch.Draw(Main.blankTexture, new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height),
                        new Color(0, 0, 0, 180));
                    spriteBatch.DrawString(font, s, new Vector2(x, 100), Color.White);

                    SoundManager instance = SoundManager.getInstance();
                    spriteBatch.DrawString(menuItemFont, sfx, new Vector2(sfxRect.X+3, sfxRect.Y+3), Color.Black);
                    spriteBatch.DrawString(menuItemFont, sfx, new Vector2(sfxRect.X, sfxRect.Y), instance.soundFxEnabled ? Color.White : Color.Red);
                    spriteBatch.DrawString(menuItemFont, music, new Vector2(musicRect.X+3, musicRect.Y+3), Color.Black);
                    spriteBatch.DrawString(menuItemFont, music, new Vector2(musicRect.X, musicRect.Y), instance.musicEnabled ? Color.White : Color.Red);
                    break;
                case PlayGameState.Playing:
                    spriteBatch.Draw(crosshair, crosshairPosition, Color.White);
                    break;
                case PlayGameState.GameOver:
                    spriteBatch.Draw(Main.blankTexture, gameOverRect, new Color(180, 0, 0, gameOverAlpha));

                    if (gameOverRect.Height >= graphicsDevice.Viewport.Height && gameOverAlpha >= 255) {
                        spriteBatch.Draw(Main.blankTexture, new Rectangle(menuRect.X + 3, menuRect.Y + 3, menuRect.Width, menuRect.Height), Color.Black);
                        spriteBatch.Draw(Main.blankTexture, menuRect, Color.Gray);

                        spriteBatch.Draw(Main.blankTexture, new Rectangle(statsRect.X + 3, statsRect.Y + 3, statsRect.Width, statsRect.Height), Color.Black);
                        spriteBatch.Draw(Main.blankTexture, statsRect, Color.Gray);

                        for (int i = 0; i < menuItems.Length; i++) {
                            Rectangle r = menuItemRect[i];
                            spriteBatch.DrawString(menuItemFont, menuItems[i], new Vector2(r.X, r.Y), menuItemColors[i]);
                        }

                        SpriteFont font2 = Main.getFont(Font.Motorwerk2);
                        string s2 = "Game Over!";
                        int x2 = (menuRect.Width - (int)font2.MeasureString(s2).X) / 2 + menuRect.X;
                        string stats = "stats";
                        int statX = (statsRect.Width - (int)font2.MeasureString(stats).X) / 2 + statsRect.X;

                        spriteBatch.DrawString(font2, s2, new Vector2(x2, 60), Color.White);
                        spriteBatch.DrawString(font2, stats, new Vector2(statX, 60), Color.White);

                        DrawStats(spriteBatch);
                    }
                    break;
                case PlayGameState.WaveOver:
                    SpriteFont font3 = Main.getFont(Font.Motorwerk2);
                    string s3 = displayWaveOver ? "Wave " + world.Wave + " complete!" : "Starting Wave " + (world.Wave + 1) + " ...";
                    Color color = displayWaveOver ? Color.White * waveOverAlpha
                        : new Color(1, waveOverRedTint, waveOverRedTint) * waveOverAlpha;
                    Color shadowColor = Color.Black * waveOverAlpha;

                    int x3 = (graphicsDevice.Viewport.Width - (int)font3.MeasureString(s3).X) / 2;
                    spriteBatch.DrawString(font3, s3, new Vector2(x3 + 3, 103), shadowColor);
                    spriteBatch.DrawString(font3, s3, new Vector2(x3, 100), color);

                    if (world.Wave == 1) {
                        string s4 = "Press tab to view and purchase upgrades";
                        int x4 = (graphicsDevice.Viewport.Width - (int)font3.MeasureString(s4).X) / 2;
                        spriteBatch.DrawString(font3, s4, new Vector2(x4 + 3, 303), Color.Black);
                        spriteBatch.DrawString(font3, s4, new Vector2(x4, 300), Color.White);
                    }

                    spriteBatch.Draw(crosshair, crosshairPosition, Color.White);
                    break;
                case PlayGameState.ShopState:
                    spriteBatch.Draw(Main.blankTexture, new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height),
                        new Color(0, 0, 0, 180));
                    shopScreen.Draw(spriteBatch);
                    break;
            }

            spriteBatch.End();
        }

        private void DrawStats(SpriteBatch spriteBatch) {
            int sx = statsRect.X + 10;
            int sy = statsRect.Y + 60;

            SpriteFont font = Main.getFont(Font.Motorwerk);
            int spacing = (int)font.MeasureString("blah").Y;
            spriteBatch.DrawString(font, "Final Wave: " + world.Wave, new Vector2(sx, sy), Color.White);
            sy += spacing;
            spriteBatch.DrawString(font, "Remaining Money: $" + string.Format("{0:n0}", world.player.MoneyAmount), new Vector2(sx, sy), Color.White);
            sy += spacing;
            spriteBatch.DrawString(font, "Zombies Killed: " + string.Format("{0:n0}", world.zombieManager.zombiesKilled), new Vector2(sx, sy), Color.White);
            sy += spacing;
            spriteBatch.DrawString(font, "Accuracy: " + Math.Round(world.bulletManager.ShotAccuracy, 2, MidpointRounding.ToEven) + "%", new Vector2(sx, sy), Color.White);
            sy += spacing;
            spriteBatch.DrawString(font, "Damage Taken: " + world.player.totalDamageTaken, new Vector2(sx, sy), Color.White);
            if (usedCheats) {
                sy += spacing;
                spriteBatch.DrawString(font, "Cheats Used: YES", new Vector2(sx, sy), Color.White);
            }
        }

    }
}
