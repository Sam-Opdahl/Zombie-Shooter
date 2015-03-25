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

    public enum MainScreenState {
        MainScreen,
        MapSelect
    }

    public class MainScreen {

        private static readonly int MAP_COUNT = 1;
        private static readonly string[] MAP_DESCRIPTIONS = {
            "Abandoned Forest Town"
        };

        private SpriteFont menuItemFont;
        private Texture2D mainMenuBackground;

        private Rectangle menuRect = new Rectangle(15, 15, 250, 400);
        private Rectangle mapSelectRect;

        private List<Rectangle> mapSelectors = new List<Rectangle>();
        private int mapHoverIndex = -1;
        private int mapWordSpacing = 10;
        private int mapSelectorSize = 60;
        private Rectangle mapSelectBackRect;
        private string mapSelectBack = "<- back";
        private bool backHovered = false;
        private int selectedMap;
        
        private string[] menuItems = {
            "Play Game",
            "Exit"
        };

        private Main mainClass;
        private List<Rectangle> menuItemRect = new List<Rectangle>();
        private List<Color> menuItemColors = new List<Color>();

        private string sfx = "Sounds Effects";
        private Rectangle sfxRect;
        private string music = "music";
        private Rectangle musicRect;

        public MainScreenState state = MainScreenState.MainScreen;

        private List<MainScreenZombie> zombieList = new List<MainScreenZombie>();
        private int zombieCount = 8;

        public MainScreen(Main main) {
            mainClass = main;
        }

        public void LoadContent(ContentManager content, GraphicsDevice graphics) {
            mainMenuBackground = content.Load<Texture2D>("Graphics\\main_screen");
            menuItemFont = Main.getFont(Font.Motorwerk);

            int spacing = 50;
            int offset = 20;
            mapSelectRect = new Rectangle(spacing, spacing + offset, graphics.Viewport.Width - (spacing * 2), graphics.Viewport.Height - (spacing * 2));

            int sx = mapSelectRect.X + 20;
            int sy = mapSelectRect.Y + 20;
            for (int i = 0; i < MAP_COUNT; i++) {
                int w = mapSelectorSize + (int)menuItemFont.MeasureString(MAP_DESCRIPTIONS[i]).X + mapWordSpacing;
                mapSelectors.Add(new Rectangle(sx, sy, w, mapSelectorSize));
                sy += mapSelectRect.Y + 5;
            }
            mapSelectBackRect = new Rectangle(sx, 410, (int)menuItemFont.MeasureString(mapSelectBack).X, (int)menuItemFont.MeasureString(mapSelectBack).Y);

            int startX = menuRect.X + 10;
            int startY = menuRect.Y + 60;
            int curY = 0;
            foreach (string item in menuItems) {
                Vector2 size = menuItemFont.MeasureString(item);
                menuItemRect.Add(new Rectangle(startX, startY + curY, (int)size.X, (int)size.Y));
                menuItemColors.Add(Color.White);
                curY += (int)size.Y;
            }

            MainScreenZombie.LoadContent(content, graphics);
            for (int i = 0; i < zombieCount; i++) {
                zombieList.Add(new MainScreenZombie());
            }


            Vector2 sv = menuItemFont.MeasureString(sfx);
            sfxRect = new Rectangle(650, 10, (int)sv.X, (int)sv.Y);
            Vector2 mv = menuItemFont.MeasureString(music);
            musicRect = new Rectangle(650, 30, (int)mv.X, (int)mv.Y);
        }

        public void Update(GameTime gameTime, Input input) {

            if (MediaPlayer.State == MediaState.Stopped || MediaPlayer.State == MediaState.Paused) {
                SoundManager.getInstance().playMusic(Music.MainMenu);
            }

            Rectangle msRect = new Rectangle(input.ms.X, input.ms.Y, 1, 1);
            switch (state) {
                case MainScreenState.MainScreen:
                    for (int i = 0; i < menuItems.Length; i++) {
                        if (msRect.Intersects(menuItemRect[i])) {
                            menuItemColors[i] = Color.Red;
                            if (!input.getPrevMouseRect().Intersects(menuItemRect[i])) {
                                SoundManager.getInstance().playSound(Sound.MouseOver);
                            }
                            if (input.isMouseClicked()) {
                                switch (i) {
                                    case 0:
                                        //mainClass.transitonToState(GameState.PlayGame);
                                        state = MainScreenState.MapSelect;
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
                case MainScreenState.MapSelect:
                    bool hoverFound = false;
                    for (int i = 0; i < MAP_COUNT; i++) {
                        if (input.getMouseRect().Intersects(mapSelectors[i])) {
                            if (!input.getPrevMouseRect().Intersects(mapSelectors[i])) {
                                SoundManager.getInstance().playSound(Sound.MouseOver);
                            }
                            if (input.isMouseClicked()) {
                                mainClass.transitonToState(GameState.PlayGame);
                                selectedMap = i + 1;
                            }

                            mapHoverIndex = i;
                            hoverFound = true;
                            break;
                        }
                    }
                    if (!hoverFound) {
                        mapHoverIndex = -1;
                    }

                    backHovered = input.getMouseRect().Intersects(mapSelectBackRect);
                    if (backHovered) {
                        if (!input.getPrevMouseRect().Intersects(mapSelectBackRect)) {
                            SoundManager.getInstance().playSound(Sound.MouseOver);
                        }
                        if (input.isMouseClicked()) {
                            state = MainScreenState.MainScreen;
                        }
                    }

                    break;
            }

            foreach (MainScreenZombie zombie in zombieList) {
                zombie.Update(gameTime);
            }

            SoundManager instance = SoundManager.getInstance();
            if (msRect.Intersects(sfxRect) && input.isMouseClicked()) {
                instance.soundFxEnabled = !instance.soundFxEnabled;
            }
            if (msRect.Intersects(musicRect) && input.isMouseClicked()) {
                instance.musicEnabled = !instance.musicEnabled;
                if (instance.musicEnabled) {
                    instance.playMusic(Music.MainMenu);
                } else {
                    MediaPlayer.Stop();
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Begin();

            spriteBatch.Draw(mainMenuBackground, Vector2.Zero, Color.White);
            foreach (MainScreenZombie zombie in zombieList) {
                zombie.Draw(spriteBatch);
            }

            switch (state) { 
                case MainScreenState.MainScreen:
                    spriteBatch.Draw(Main.blankTexture, new Rectangle(menuRect.X + 3, menuRect.Y + 3, menuRect.Width, menuRect.Height), Color.Black);
                    spriteBatch.Draw(Main.blankTexture, menuRect, Color.Gray);

                    spriteBatch.DrawString(Main.getFont(Font.Motorwerk2), "zombies must die", new Vector2(44, 20), Color.White);

                    for (int i = 0; i < menuItems.Length; i++) {
                        Rectangle r = menuItemRect[i];
                        spriteBatch.DrawString(menuItemFont, menuItems[i], new Vector2(r.X, r.Y), menuItemColors[i]);
                    }
                    break;
                case MainScreenState.MapSelect:
                    spriteBatch.Draw(Main.blankTexture, new Rectangle(mapSelectRect.X + 3, mapSelectRect.Y + 3, mapSelectRect.Width, mapSelectRect.Height), Color.Black);
                    spriteBatch.Draw(Main.blankTexture, mapSelectRect, Color.Gray);
                    spriteBatch.DrawString(Main.getFont(Font.Motorwerk2), "Map Select", new Vector2(mapSelectRect.X+3, 38), Color.Black);
                    spriteBatch.DrawString(Main.getFont(Font.Motorwerk2), "Map Select", new Vector2(mapSelectRect.X, 35), Color.White);

                    SpriteFont font = Main.getFont(Font.Motorwerk2);
                    for (int i = 0; i < MAP_COUNT; i++) {
                        Rectangle r2 = mapSelectors[i];
                        spriteBatch.Draw(Main.blankTexture, new Rectangle(r2.X, r2.Y, mapSelectorSize, mapSelectorSize), i == mapHoverIndex ? Color.Black : Color.White);
                        string s = ""+(i+1);
                        int x = r2.X + (mapSelectorSize / 2) - ((int)font.MeasureString(s).X / 2);
                        int y = r2.Y + (mapSelectorSize / 2) - ((int)font.MeasureString(s).Y / 2);
                        spriteBatch.DrawString(font, s, new Vector2(x, y), i == mapHoverIndex ? Color.White : Color.Black);

                        spriteBatch.DrawString(menuItemFont, MAP_DESCRIPTIONS[i], new Vector2(r2.X + mapSelectorSize + mapWordSpacing, y), i == mapHoverIndex ? Color.Red : Color.White);
                    }

                    spriteBatch.DrawString(menuItemFont, mapSelectBack, new Vector2(mapSelectBackRect.X, mapSelectBackRect.Y), backHovered ? Color.Red : Color.White);
                    break;
            }

            SoundManager instance = SoundManager.getInstance();
            spriteBatch.DrawString(menuItemFont, sfx, new Vector2(sfxRect.X+3, sfxRect.Y+3), Color.Black);
            spriteBatch.DrawString(menuItemFont, sfx, new Vector2(sfxRect.X, sfxRect.Y), instance.soundFxEnabled ? Color.White : Color.Red);
            spriteBatch.DrawString(menuItemFont, music, new Vector2(musicRect.X+3, musicRect.Y+3), Color.Black);
            spriteBatch.DrawString(menuItemFont, music, new Vector2(musicRect.X, musicRect.Y), instance.musicEnabled ? Color.White : Color.Red);

            spriteBatch.End();
        }

        public int SelectedMap {
            get {
                return selectedMap;
            }
        }
    }
}
