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
    class Upgrade {

        private static readonly string PURCHASE_MESSAGE = " Upgrade Purchased";

        private static readonly int CLICK_BOX_WIDTH = 35;
        private static readonly int CLICK_BOX_HEIGHT = 25;

        private List<Rectangle> clickRects;
        private Vector2 nameLocation;
        private Rectangle separator;
        private int scrollIndex = 0;

        private int purchaseMessageY = 0;
        private int purchaseMessageX = 0;
        private float purchseMessageAlpha = 0;
        private bool purchaseMade = false;
        private bool insufficientFunds = false;


        public Upgrade(string name, int[] values, int[] cost) {
            Name = name;
            CurrentUpgrade = 0;
            MaxUpgrade = cost.Length;

            UpgradeCost = new List<int>();
            foreach (int i in cost) {
                UpgradeCost.Add(i);
            }

            Values = new List<int>();
            foreach (int i in values) {
                Values.Add(i);
            }
        }

        public void init(int index, int x, int y, GraphicsDevice graphics) {
            clickRects = new List<Rectangle>();
            textSize = Main.getFont(Font.Motorwerk).MeasureString(Name);
            int ry = y + ShopScreen.INNER_SCREEN_PADDING + (index * ShopScreen.INNER_SCREEN_PADDING);
            nameLocation = new Vector2(x, ry);
            separator = new Rectangle(ShopScreen.OFFSET_X + ShopScreen.INNER_SCREEN_PADDING, 
                ry + (ShopScreen.INNER_SCREEN_PADDING / 2) + (int)textSize.Y, 
                graphics.Viewport.Width - (x * 2), 
                1);

            int rx = x + (int)textSize.X + 20;
            for (int i = 0; i < UpgradeCost.Count; i++) {
                clickRects.Add(new Rectangle(rx, ry, CLICK_BOX_WIDTH, CLICK_BOX_HEIGHT));
                rx += CLICK_BOX_WIDTH + 2;
            }
        }

        public void Update(Input input, Player player, bool isActive) {

            if (purchaseMade || insufficientFunds) {
                purchseMessageAlpha += 0.02f;
                if (purchseMessageAlpha < 1.0f) {
                    purchaseMessageX += 1;
                } else if (purchseMessageAlpha >= 2f) {
                    purchaseMade = false;
                    insufficientFunds = false;
                }
                return;
            }

            if (isActive) {
                Rectangle msRect = input.getMouseRect();
                int i = 0;
                bool scrollFound = false;
                foreach (Rectangle r in clickRects) {
                    if (r.Intersects(msRect)) {
                        if (i == CurrentUpgrade) {
                            if (!input.getPrevMouseRect().Intersects(r)) {
                                SoundManager.getInstance().playSound(Sound.MouseOver);
                            }

                            if (input.isMouseClicked()) {
                                if (player.MoneyAmount >= UpgradeCost[CurrentUpgrade]) {
                                    player.modifyMoney(-UpgradeCost[CurrentUpgrade]);
                                    CurrentUpgrade++;
                                    purchaseMade = true;
                                    SoundManager.getInstance().playSound(Sound.UpgradeSuccess);
                                } else {
                                    insufficientFunds = true;
                                    SoundManager.getInstance().playSound(Sound.UpgradeFail);
                                }

                                purchaseMessageX = 0;
                                purchaseMessageY = i;
                                purchseMessageAlpha = 0;
                            }
                        }
                        scrollIndex = i;
                        scrollFound = true;
                        break;

                    }
                    i++;
                }
                if (!scrollFound) {
                    scrollIndex = -1;
                }
            }
        }

        public void handleClose() {
            purchaseMade = false;
            insufficientFunds = false;
        }

        public void Draw(SpriteBatch spriteBatch) {
            spriteBatch.DrawString(Main.getFont(Font.Motorwerk), Name, nameLocation, Color.White);
            spriteBatch.Draw(Main.blankTexture, separator, Color.Black);

            int i = 0;
            int x = 0;
            int y = 0; ;
            SpriteFont f = Main.getFont(Font.Motorwerk);
            foreach (Rectangle r in clickRects) {
                Color c1;
                Color c2;
                if (i == scrollIndex && i == CurrentUpgrade) {
                    c1 = Color.Gray;
                    c2 = Color.Red;
                } else if (i == CurrentUpgrade) {
                    c1 = Color.LightGray;
                    c2 = Color.Blue;
                } else {
                    c1 = Color.Gray;
                    c2 = Color.DarkGray;
                }

                spriteBatch.Draw(Main.blankTexture, r, c1);
                string s = "" + (i+1);
                x = r.X + (r.Width / 2) - ((int)f.MeasureString(s).X / 2);
                y = r.Y + (r.Height / 2) - ((int)f.MeasureString(s).Y / 2);
                spriteBatch.DrawString(Main.getFont(Font.Motorwerk), s, new Vector2(x, y), c2);
                i++;
                x += r.Width + 15;
            }

            string cost = CurrentUpgrade == UpgradeCost.Count ? "Fully Upgraded!" : "Cost: $" + string.Format("{0:n0}", UpgradeCost[CurrentUpgrade]);
            spriteBatch.DrawString(f, cost, new Vector2(x, y), Color.White);

            if (purchaseMade) {
                x += (int)f.MeasureString(cost).X;
                spriteBatch.DrawString(f, PURCHASE_MESSAGE, new Vector2(x + purchaseMessageX - 15, y), Color.White * purchseMessageAlpha);
            } else if (insufficientFunds) {
                x += (int)f.MeasureString(cost).X;
                spriteBatch.DrawString(f, "Insufficient Funds", new Vector2(x + purchaseMessageX - 15, y), Color.Red * purchseMessageAlpha);
            }
            
        }

        public int getCurrentUpgradeValue() {
            return Values[CurrentUpgrade];
        }

        public string Name { get; private set; }
        public int CurrentUpgrade { get; private set; }
        public int MaxUpgrade { get; private set; }
        public List<int> UpgradeCost { get; private set; }
        public List<int> Values { get; private set; }
        public Vector2 textSize { get; private set; }
    }
}
