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
    class MainScreenZombie {

        private static Viewport viewport;
        private static Random rand = new Random();
        private static Texture2D TEXTURE;
        private static readonly string TEXTURE_LOCATION = "Graphics\\zombie1";
        private static readonly int SPEED = 50;
        private static readonly float WAIT_TIME = 2;

        private static readonly int SPRITE_WIDTH = 48;
        private static readonly int SPRITE_HEIGHT = 40;
        private static readonly int MAX_FRAME_TIME = 5;
        private static readonly int MAX_FRAME = 7;

        private int currentFrame = 0;
        private int frameTimer = 0;

        private Vector2 location;
        private Vector2 destination;
        private double angle;

        private float waitCounter = 0;

        public MainScreenZombie() {
            this.location = findNewCoordinate();
            this.destination = findNewCoordinate();
        }

        private Vector2 findNewCoordinate() {
            return new Vector2(rand.Next(15, viewport.Width - 15), rand.Next(15, viewport.Height - 15));
        }

        public static void LoadContent(ContentManager content, GraphicsDevice graphics) {
            TEXTURE = content.Load<Texture2D>(TEXTURE_LOCATION);
            viewport = graphics.Viewport;
        }

        public void Update(GameTime gameTime) {

            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            //float dx;
            //if (destination.X > location.X) {
            //    dx = SPEED;
            //} else if (destination.X < location.X) {
            //    dx = -SPEED;
            //} else {
            //    dx = 0;
            //}
            //location.X += dx;

            //float dy;
            //if (destination.Y > location.Y) {
            //    dy = SPEED;
            //} else if (destination.Y < location.Y) {
            //    dy = -SPEED;
            //} else {
            //    dy = 0;
           // }

            //location.Y += dy;
            float dx = 0;
            float dy = 0;

            if (Vector2.Distance(location, destination) < 2) {
                waitCounter += elapsedTime;
                if (waitCounter > WAIT_TIME) {
                    waitCounter = 0;
                    destination = findNewCoordinate();
                }
            } else {
                angle = Math.Atan2(destination.Y - location.Y, destination.X - location.X);

                dx = (float)Math.Cos(angle) * SPEED * elapsedTime;
                dy = (float)Math.Sin(angle) * SPEED * elapsedTime;
                location.X += dx;
                location.Y += dy;
            }

            if (frameTimer++ > MAX_FRAME_TIME) {
                frameTimer = 0;

                if (Math.Abs(dx) > 0 || Math.Abs(dy) > 0) {
                    if (currentFrame++ >= MAX_FRAME) {
                        currentFrame = 0;
                    }
                } else {
                    currentFrame = 0;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Draw(TEXTURE, location,
                    new Rectangle(SPRITE_WIDTH * currentFrame, 0, SPRITE_WIDTH, SPRITE_HEIGHT),
                    Color.White, (float)angle + (3 * MathHelper.Pi / 2), getCenterLocation(), 1f, SpriteEffects.None, 0);
        }

        private Vector2 getCenterLocation() {
            return new Vector2(SPRITE_WIDTH / 2,
                    SPRITE_HEIGHT / 2);
        }
    }
}
