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
    class Camera
    {
        private World world;
        protected Viewport viewport;
        private Rectangle tileRectangle;
        private Rectangle previousTileRectangle;

        public Camera(Viewport viewport, World world)
        {
            zoom = 1.0f;
            this.viewport = viewport;
            this.world = world;
        }

        public void Update()
        {
            previousTileRectangle = tileRectangle;
            transform = Matrix.CreateTranslation(position.X, position.Y, 0);
            inverseTransform = Matrix.Invert(transform);

            //Center the camera on the player, but keep the camera within the bounds of the map.
            position.X = Math.Min(0, (float)-world.player.RelativeCenterX + (viewport.Width / 2));
            position.Y = Math.Min(0, (float)-world.player.RelativeCenterY + (viewport.Height / 2));

            position.X = Math.Max(position.X, -((float)world.Map.TileWidth * (float)world.Map.MapWidth - viewport.Width));
            position.Y = Math.Max(position.Y, -((float)world.Map.TileHeight * (float)world.Map.MapHeight - viewport.Height));

            //Find the camera's bounds in terms on tiles.
            tileRectangle = new Rectangle(Math.Max(Math.Abs((int)Math.Floor((double)position.X / Tile.SIZE)) - 2, 0), 
                    Math.Max(Math.Abs((int)Math.Floor(position.Y / Tile.SIZE)) - 2, 0),
                    (int)Math.Ceiling((double)viewport.Width / Tile.SIZE) + 3, 
                    (int)Math.Ceiling((double)viewport.Height / Tile.SIZE) + 3);
        }

        public Rectangle TileRectangle {
            get {
                return tileRectangle;
            }
        }

        public float zoom { get; set; }
        public Matrix transform { get; set; }
        public Matrix inverseTransform { get; set; }
        public Rectangle CollisionBox {
            get {
                return new Rectangle(-(int)position.X - 16, -(int)position.Y - 16, viewport.Width + 32, viewport.Height + 32);
            }
        }
        public Vector2 position = Vector2.Zero;
        public Vector2 centerPosition {
            get {
                return new Vector2(position.X - (viewport.Width / 2),
                        position.Y - (viewport.Height / 2));
            }
        }
    }
}
