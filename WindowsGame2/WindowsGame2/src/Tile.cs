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
    public enum TileLayer
    {
        foreground,
        background,
        background2
    }

    public enum TileType
    {
        Empty = 0,
        Block = 1
    }

    class Tile : IHasNeighbours<Tile>
    {
        public static List<Texture2D> tileSheet = new List<Texture2D>();

        private static readonly string[] mapSheetNames = { 
            "Graphics\\Other",
            "Graphics\\tileset2",
            "Graphics\\small_tree_trans",
            "Graphics\\marble",
            "Graphics\\grass",
            "Graphics\\rustville_tileset",
            "Graphics\\rustville_tileset 1",
            "Graphics\\building1",
            "Graphics\\street",
            "Graphics\\house2",
            "Graphics\\dusty grass",
            "Graphics\\small_tree",
            "Graphics\\house3"
        };
        public static readonly int SIZE = 32;

        private int tileSheetId;

        private Rectangle sourceRect;
        private Rectangle destRect;

        public int x;
        public int y;

        public Tile(string layer, TileType type, int destX, int destY, int? tileSheetId, int? sourceX, int? sourceY)
        {
            Layer = layer;
            Type = type;
            destRect = new Rectangle(destX, destY, SIZE, SIZE);

            if (Type != TileType.Empty)
            {
                this.tileSheetId = (int)tileSheetId;
                this.sourceRect = new Rectangle((int)sourceX, (int)sourceY, SIZE, SIZE);
            }
        }

        public static void LoadContent(ContentManager content)
        {
            foreach (string map in mapSheetNames)
            {
                tileSheet.Add(content.Load<Texture2D>(map));
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Type == TileType.Empty)
            {
                return;
            }

            spriteBatch.Draw(tileSheet[tileSheetId], destRect, sourceRect, Color.White);
        }


        // -- Accessors/Mutators --
        public int TileSheetId { get; set; }
        public string Layer { get; private set; }
        public TileType Type { get; private set; }
        public int SourceX { get; set; }
        public int SourceY { get; set; }
        public int DestX { get; set; }
        public int DestY { get; set; }

        public bool isActive {
            get {
                return Type != TileType.Empty;
            }
        }

        public Rectangle CollisionBox {
            get {
                return destRect;
            }
        }

        public BoundingBox boundingBox {
            get {
                return new BoundingBox(new Vector3(destRect.Left, DestRect.Top, 0),
                    new Vector3(destRect.Right, destRect.Bottom, 0));
            }
        }

        public Rectangle DestRect {
            get {
                return destRect;
            }
        }

        public Point Center {
            get {
                return new Point(x * SIZE + (SIZE / 2),
                        y * SIZE + (SIZE / 2));
            }
        }

        public Vector2 vCenter {
            get {
                return new Vector2(x * SIZE + (SIZE / 2),
                        y * SIZE + (SIZE / 2));
            }
        }

        public IEnumerable<Tile> Neighbors { get; set; }
        public List<int> NeighborWeight { get; set; }
    }
}
