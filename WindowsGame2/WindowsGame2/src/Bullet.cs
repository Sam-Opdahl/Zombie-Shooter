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
    class Bullet
    {
        private static readonly int DEFAULT_VELOCITY = 800;


        private double x;
        private double y;
        private double angle;

        private int velocity = DEFAULT_VELOCITY;

        private int width = 12;
        private int height = 3;
        private double rotatedWidth;
        private double rotatedHeight;
        private World world;

        public List<Zombie> zombiesHit = new List<Zombie>();

        public Bullet(World world, double x, double y, double angle, int damage) {
            this.world = world;
            this.x = x;
            this.y = y;
            this.angle = angle;
            this.isActive = true;
            rotatedWidth = this.height * Math.Abs(Math.Sin(this.angle)) + this.width * Math.Abs(Math.Cos(this.angle));
            rotatedHeight = this.height * Math.Abs(Math.Cos(this.angle)) + this.width * Math.Abs(Math.Sin(this.angle));
        }

        public void Update(GameTime gameTime) {
            if (!isActive) {
                return;
            }

            if (!CollisionBox.Intersects(world.camera.CollisionBox)) {
                isActive = false;
                return;
            }

            var col = this.getIntersectingColumns();
            var row = this.getIntersectingRows();
            for (var i = 0; i < col.Count; i++) {
                if (col[i] < 0 || col[i] >= (int)this.world.Map.MapHeight) {
                    continue;
                }

                for (var j = 0; j < row.Count; j++) {
                    if (row[j] < 0 || row[j] >= (int)this.world.Map.MapWidth) {
                        continue;
                    }
                    if (world.Map.ForegroundTiles[row[j], col[i]].Type != TileType.Empty) {
                        this.isActive = false;
                        return;
                    }
                }
            }

            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            x += (float)deltaX() * velocity * elapsedTime;
            y += (float)deltaY() * velocity * elapsedTime;
        }

        public void Draw(SpriteBatch spriteBatch) {
            if (!this.isActive) {
                return;
            }

            spriteBatch.Draw(world.blankTexture, 
                new Rectangle((int)x, (int)y, width, height), 
                null, 
                Color.White, 
                (float)angle, 
                Vector2.Zero, 
                SpriteEffects.None, 
                0);
        }

        private List<int> getIntersectingColumns() {
            return getIntersectList(CollisionBox.Left, CollisionBox.Right);
        }

        private List<int> getIntersectingRows() {
            return getIntersectList(CollisionBox.Top, CollisionBox.Bottom);
        }

        private int getLowBoundTile(int bound) {
            return (int)Math.Floor((double)bound / Tile.SIZE);
        }

        private int getUpperBoundTile(int bound) {
            return (int)Math.Ceiling((double)bound / Tile.SIZE);
        }

        private List<int> getIntersectList(int start, int end) {
            start = getLowBoundTile(start);
            end = getUpperBoundTile(end);

            List<int> list = new List<int>();
            for (int i = 0; i < end - start; i++) {
                list.Add(start + i);
            }
            return list;
        }


        // Accessors/Mutators
        public Rectangle CollisionBox {
            get {
                return new Rectangle((int)(x - (rotatedWidth / 2)), (int)(y - (rotatedHeight / 2)), (int)rotatedWidth, (int)rotatedHeight);
            }
        }

        public bool isActive { get; set; }

        private double centerX() {
            return x + (width / 2);
        }

        private double centerY() {
            return y + (height / 2);
        }

        private double deltaX() {
            return Math.Cos(angle);
        }

        private double deltaY() {
            return Math.Sin(angle);
        }
    }
}
