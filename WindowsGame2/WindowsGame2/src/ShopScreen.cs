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

    enum GunAttribute {
        FireRate = 0,
        ClipSize,
        ReloadSpeed,
        StoppingPower,
        Automatic,
        BulletDamage
    }

    enum PlayerAttribute {
        MaxHealth = 0,
        ZombieAward
    }

    class ShopScreen {

        private static readonly string TITLE = "Upgrade Your Experience!";
        private static readonly string[] TABS = {
            "Gun",
            "Player"
        };
        public static readonly int OFFSET_X = 25;
        public static readonly int OFFSET_Y = 70;
        private static readonly int INNER_TAB_PADDING = 10;
        private static readonly int TAB_HEIGHT = 25;

        public static readonly int INNER_SCREEN_PADDING = 15;

        private GraphicsDevice graphicsDevice;
        private GameScreen gameScreen;

        private List<Rectangle> tabRects;

        private int activeTab = 0;
        private int intersectedTab = -1;

        private Upgrade[] gunUpgrades = { 
            new Upgrade("Fire Rate", new int[] { 12, 8, 6, 4 }, new int[] { 1500, 5000, 15000 } ),
            new Upgrade("Clip Size", new int[] { 15, 18, 22, 28, 40 }, new int[] { 2000, 7000, 24000, 52000 } ),
            new Upgrade("Reload Speed", new int[] { 9, 6, 3, 1 }, new int[] { 2500, 10000, 25000 } ),
            new Upgrade("Stopping Power", new int[] { 1, 2, 3, 5 }, new int[] { 3750, 15500, 32000 } ),
            new Upgrade("Automatic Pistol", new int[] { 0, 1 }, new int[] { 150000 } ),
            new Upgrade("Bullet Damage", new int[] { 20, 30  }, new int[] { 45000 } ),
        };

        private Upgrade[] playerUpgrades = {
            new Upgrade("Max Health", new int[] { 100, 125, 150, 200, 300 }, new int[] { 1500, 5500, 18000, 45000 } ),
            new Upgrade("Zombie Payment", new int[] { 45, 75, 115, 155, 235 }, new int[] { 2500, 8500, 21500, 51000 } ),
        };

        public ShopScreen(GameScreen gameScreen) {
            this.gameScreen = gameScreen;
        }

        public void LoadContent(GraphicsDevice graphicsDevice) {
            this.graphicsDevice = graphicsDevice;

            tabRects = new List<Rectangle>();
            int x = OFFSET_X;
            SpriteFont f = Main.getFont(Font.Motorwerk2);
            foreach (string s in TABS) {
                int w = (int)f.MeasureString(s).X + (INNER_TAB_PADDING * 2);
                tabRects.Add(new Rectangle(x, OFFSET_Y, w, TAB_HEIGHT + (INNER_TAB_PADDING * 2)));
                x += w;
            }

            int y = OFFSET_Y + TAB_HEIGHT + (INNER_TAB_PADDING * 2);
            x = OFFSET_X + INNER_SCREEN_PADDING;
            for (int i = 0; i < gunUpgrades.Length; i++) {
                gunUpgrades[i].init(i, x, y, graphicsDevice);
                y += TAB_HEIGHT;
            }

            y = OFFSET_Y + TAB_HEIGHT + (INNER_TAB_PADDING * 2);
            x = OFFSET_X + INNER_SCREEN_PADDING;
            for (int i = 0; i < playerUpgrades.Length; i++) {
                playerUpgrades[i].init(i, x, y, graphicsDevice);
                y += TAB_HEIGHT;
            }
        }

        public void Update(GameTime gameTime, Input input) {
            Rectangle msRect = new Rectangle(input.ms.X, input.ms.Y, 1, 1);
            bool intersectFound = false;
            for (int i = 0; i < tabRects.Count; i++) {
                if (msRect.Intersects(tabRects[i])) {
                    if (!input.getPrevMouseRect().Intersects(tabRects[i])) {
                        SoundManager.getInstance().playSound(Sound.MouseOver);
                    }

                    if (input.isMouseClicked()) {
                        activeTab = i;
                    }
                    intersectedTab = i;
                    intersectFound = true;
                    break;
                }
            }
            if (!intersectFound) {
                intersectedTab = -1;
            }

            foreach (Upgrade upgrade in gunUpgrades) {
                upgrade.Update(input, gameScreen.world.player, 0 == activeTab);
            }
            foreach (Upgrade upgrade in playerUpgrades) {
                upgrade.Update(input, gameScreen.world.player, 1 == activeTab);
            }
        }

        public void Draw(SpriteBatch spriteBatch) {
            spriteBatch.DrawString(Main.getFont(Font.Motorwerk2), TITLE, new Vector2(28, 28), Color.Black);
            spriteBatch.DrawString(Main.getFont(Font.Motorwerk2), TITLE, new Vector2(25, 25), Color.White);

            SpriteFont font = Main.getFont(Font.Motorwerk2);
            string money = "$" + string.Format("{0:n0}", gameScreen.world.player.MoneyAmount);
            int x = graphicsDevice.Viewport.Width - OFFSET_X - (int)font.MeasureString(money).X;
            spriteBatch.DrawString(font, money, new Vector2(x, 28), Color.Black);
            spriteBatch.DrawString(font, money, new Vector2(x, 25), Color.White);

            for (int i = 0; i < TABS.Length; i++) {
                Color c;
                if (i == intersectedTab) {
                    c = Color.LightGray;
                } else if (i == activeTab) {
                    c = Color.DarkGray;
                } else {
                    c = Color.Gray;
                }
                spriteBatch.Draw(Main.blankTexture, tabRects[i], c);
                Vector2 v = new Vector2(tabRects[i].X + INNER_TAB_PADDING, tabRects[i].Y + INNER_TAB_PADDING);
                spriteBatch.DrawString(Main.getFont(Font.Motorwerk2), TABS[i], v, Color.White);
            }

            Rectangle mainRect = new Rectangle(OFFSET_X, tabRects[0].Bottom, graphicsDevice.Viewport.Width - (OFFSET_X * 2),
                graphicsDevice.Viewport.Height - tabRects[0].Bottom - TAB_HEIGHT);
            spriteBatch.Draw(Main.blankTexture, mainRect, Color.DarkGray);

            Upgrade[] upgrades = activeTab == 0 ? gunUpgrades : playerUpgrades;

            for (int i = 0; i < upgrades.Length; i++) {
                upgrades[i].Draw(spriteBatch);
            }
        }

        public void handleClose() {
            foreach (Upgrade upgrade in gunUpgrades) {
                upgrade.handleClose();
            }
            foreach (Upgrade upgrade in playerUpgrades) {
                upgrade.handleClose();
            }
        }

        public int getPlayerAttribute(PlayerAttribute name) {
            return playerUpgrades[(int)name].getCurrentUpgradeValue();
        }

        public int getGunAttribute(GunAttribute name) {
            return gunUpgrades[(int)name].getCurrentUpgradeValue();
        }
    }
}
