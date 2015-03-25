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

    class World {

        public Player player;
        public Camera camera;
        private Map map;
        public BulletManager bulletManager;
        public ZombieManager zombieManager;
        public GameScreen gameScreen;

        public Texture2D blankTexture;
        private int wave;

        public World(GameScreen gameScreen, int mapToLoad) {
            player = new Player(this);
            map = new Map(this, ""+mapToLoad);
            bulletManager = new BulletManager(this);
            zombieManager = new ZombieManager(this);
            this.gameScreen = gameScreen;
        }

        public void LoadContent(ContentManager content, GraphicsDevice graphics) {
            bulletManager.LoadContent();
            
            blankTexture = new Texture2D(graphics, 1, 1);
            blankTexture.SetData(new Color[] { Color.White });

            camera = new Camera(graphics.Viewport, this);
            player.LoadContent(content, graphics);
            Tile.LoadContent(content);

            Zombie.LoadContent(content);
            startNewWave();
        }

        public void Update(GameTime gameTime, Input input) {
            player.Update(gameTime, input);
            camera.Update();
            map.Update();
            input.updateMouseTransform(camera.inverseTransform);
            bulletManager.Update(this, gameTime, input);

            zombieManager.Update(gameTime, input);
        }

        public void startNewWave() {
            zombieManager.MaxZombiesToSpawn = (int)Math.Ceiling(0.5 * Math.Pow(wave, 2)) + 5;
            zombieManager.MaxZombiesAtOnce = (int)Math.Ceiling(1.33 * wave);
            zombieManager.ZombiesSpawnedThisWave = 0;
        }

        public void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, camera.transform);

            map.DrawBackground(spriteBatch);

            spriteBatch.End();


            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null, camera.transform);

            bulletManager.Draw(spriteBatch);

            spriteBatch.End();


            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, camera.transform);

            map.DrawForeground(spriteBatch);

            zombieManager.Draw(spriteBatch);

            spriteBatch.End();


            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null, camera.transform);

            player.Draw(spriteBatch);

            spriteBatch.End();
        }

        public int getGunAttribute(GunAttribute upgrade) {
            return gameScreen.shopScreen.getGunAttribute(upgrade);
        }

        public int getPlayerAttribute(PlayerAttribute upgrade) {
            return gameScreen.shopScreen.getPlayerAttribute(upgrade);
        }

        public Map Map {
            get {
                return this.map;
            }
        }

        public int Wave {
            get {
                return wave;
            }
            set {
                wave = value;
            }
        }
    }
}
