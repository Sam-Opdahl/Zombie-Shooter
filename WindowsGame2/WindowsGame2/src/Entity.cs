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
    class Entity {
        protected Vector2 location;
        protected int velocity;
        protected double angle;
        protected World world;

        protected int currentHealth;
        protected int maxHealth;


        public Entity(World world) {
            this.world = world;
        }

        public virtual void Update(GameTime gameTime, Input input) {
            
        }

        public virtual void Draw(SpriteBatch spriteBatch) {

        }


        protected void handleSideCollision(float dx) {
            if (dx == 0) {
                return;
            }

            Tile tile = sideCollision(true, getIntersectingRows(), getUpperBoundTile(CollisionBox.Right) - 1);
            if (tile != null) {
                location.X = tile.CollisionBox.Left - (float)Math.Ceiling(((double)CollisionBox.Width / 2));
            }

            tile = sideCollision(true, getIntersectingRows(), getLowBoundTile(CollisionBox.Left));
            if (tile != null) {
                location.X = tile.CollisionBox.Right + (CollisionBox.Width / 2);
            }
        }

        protected void handleTopBottomCollision(float dy) {
            if (dy == 0) {
                return;
            }

            Tile tile = sideCollision(false, getIntersectingColumns(), getLowBoundTile(CollisionBox.Bottom));
            if (tile != null) {
                location.Y = tile.CollisionBox.Top - (float)Math.Ceiling((double)CollisionBox.Height / 2);
            }

            tile = sideCollision(false, getIntersectingColumns(), getUpperBoundTile(CollisionBox.Top) - 1);
            if (tile != null) {
                location.Y = tile.CollisionBox.Bottom + (CollisionBox.Height / 2);
            }
        }

        protected Tile sideCollision(bool side, List<int> tilesToCheck, int position) {
            Map map = world.Map;
            for (int i = 0; i < tilesToCheck.Count; i++) {
                if (tilesToCheck[i] >= map.MapHeight || tilesToCheck[i] < 0 ||
                    position >= map.MapWidth || position < 0) {
                    continue;
                }

                Tile tile;
                if (side) {
                    tile = map.ForegroundTiles[tilesToCheck[i], position];
                } else {
                    tile = map.ForegroundTiles[position, tilesToCheck[i]];
                }
                if (tile.isActive) {
                    if (CollisionBox.Intersects(tile.CollisionBox)) {
                        return tile;
                    }
                }
            }

            return null;
        }

        protected List<int> getIntersectingColumns() {
            return getIntersectList(CollisionBox.Left, CollisionBox.Right);
        }

        protected List<int> getIntersectingRows() {
            return getIntersectList(CollisionBox.Top, CollisionBox.Bottom);
        }

        protected int getLowBoundTile(int bound) {
            return (int)Math.Floor((double)bound / Tile.SIZE);
        }

        protected int getUpperBoundTile(int bound) {
            return (int)Math.Ceiling((double)bound / Tile.SIZE);
        }

        protected List<int> getIntersectList(int start, int end) {
            start = getLowBoundTile(start);
            end = getUpperBoundTile(end);

            List<int> list = new List<int>();
            for (int i = 0; i < end - start; i++) {
                list.Add(start + i);
            }
            return list;
        }

        public Vector3 unitDirection(Vector2 otherPoint) {
            double angle = Math.Atan2(otherPoint.Y - RelativeCenterY, otherPoint.X - RelativeCenterX);
            return new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), 0);
        }

        public float distanceToTile(Tile t) {
            return Vector2.Distance(location, t.vCenter);
        }

        // -- Accessors/Mutators -- 

        public Vector2 Location {
            get {
                return location;
            }
            set {
                location = value;
            }
        }

        public Vector3 Location3D {
            get {
                return new Vector3(location, 0);
            }
        }

        public float RelativeCenterX {
            get {
                return location.X;
            }
        }

        public float RelativeCenterY {
            get {
                return location.Y;
            }
        }

        public double Angle {
            get {
                return angle;
            }
        }

        public int CurrentHealth {
            get {
                return currentHealth;
            }
            set {
                currentHealth = value; 
            }
        }

        public int MaxHealth {
            get {
                return maxHealth;
            }
        }

        protected int collisionBoxSize = 30;
        public Rectangle CollisionBox {
            get {
                return new Rectangle((int)location.X - (collisionBoxSize / 2), (int)location.Y - (collisionBoxSize / 2),
                    collisionBoxSize, collisionBoxSize);
            }
        }

        public BoundingBox boundingBox {
            get {
                return new BoundingBox(new Vector3(location.X - (collisionBoxSize / 2), location.Y - (collisionBoxSize / 2), 0),
                    new Vector3(location.X + (collisionBoxSize / 2), location.Y + (collisionBoxSize / 2), 0));
            }
        }

        public Rectangle getDistanceTileRectangle(Entity other) {
            double minX = Math.Min(this.location.X, other.Location.X);
            double minY = Math.Min(this.location.Y, other.Location.Y);
            int x = (int)Math.Round(minX / Tile.SIZE) - 1;
            int y = (int)Math.Round(minY / Tile.SIZE) - 1;
            int w = (int)Math.Round((Math.Max(this.location.X, other.Location.X) - minX) / Tile.SIZE) + 2;
            int h = (int)Math.Round((Math.Max(this.location.Y, other.Location.Y) - minY) / Tile.SIZE) + 2;

            return new Rectangle(Math.Max(0, x), Math.Max(0, y), w, h);
        }

        public Tile getTileLocation() {
            int x = (int)Math.Floor(location.X / world.Map.TileWidth);
            int y = (int)Math.Floor(location.Y / world.Map.TileHeight);

            if (x >= world.Map.MapWidth || y >= world.Map.MapHeight || x < 0 || y < 0) {
                return null;
            } else {
                return world.Map.ForegroundTiles[y, x];
            }
        }

        public Rectangle getSurroundingTileRectangle(int size) {
            int x = (int)(Math.Floor(location.X / world.Map.TileWidth) - size) * world.Map.TileWidth;
            int y = (int)(Math.Floor(location.Y / world.Map.TileHeight) - size) * world.Map.TileHeight;
            int w = (2 * size + 1) * world.Map.TileWidth;
            int h = (2 * size + 1) * world.Map.TileHeight;

            return new Rectangle(x, y, w, h);
        }
    }
}
