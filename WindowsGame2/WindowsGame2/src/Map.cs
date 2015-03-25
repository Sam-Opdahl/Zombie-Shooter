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
using System.IO;
using Newtonsoft.Json;

namespace WindowsGame2
{
    class Map
    {
        private string mapId;
        private Tile[,] foregroundTiles;
        private Tile[,] backgroundTiles;
        private Tile[,] background2Tiles;
        private dynamic mapData;
        private Tile[,] viewableTiles;
        private World world;

        private int tileHeight;
        private int tileWidth;
        private int mapHeight;
        private int mapWidth;

        public Map(World world, string mapId)
        {
            this.mapId = mapId;
            this.world = world;
            LoadMap();
        }

        public void Update() {
            Rectangle r = world.camera.TileRectangle;
            viewableTiles = new Tile[r.Height, r.Width];
            for (int x = r.X; x < r.Width + r.X; x++) {
                for (int y = r.Y; y < r.Height + r.Y; y++) {
                    if (x > world.Map.MapWidth - 1 || y > world.Map.MapHeight - 1)
                        continue;

                    Tile t = foregroundTiles[y, x];
                    if (t.Type == TileType.Block) {
                        viewableTiles[y - r.Y, x - r.X] = t;
                    } else {
                        viewableTiles[y - r.Y, x - r.X] = null;
                    }
                }
            }
        }

        public void DrawForeground(SpriteBatch spriteBatch)
        {
            foreach (Tile t in viewableTiles) {
                if (t != null) {
                    t.Draw(spriteBatch);
                }
            }
        }

        public void DrawBackground(SpriteBatch spriteBatch) {
            Rectangle r = world.camera.TileRectangle;

            for (int x = r.X; x < r.Width + r.X; x++) {
                for (int y = r.Y; y < r.Height + r.Y; y++) {
                    if (x > world.Map.MapWidth - 1 || y > world.Map.MapHeight - 1)
                        continue;

                    if (backgroundTiles[y, x] != null) {
                        backgroundTiles[y, x].Draw(spriteBatch);
                    }
                }
            }

            for (int x = r.X; x < r.Width + r.X; x++) {
                for (int y = r.Y; y < r.Height + r.Y; y++) {
                    if (x > world.Map.MapWidth - 1 || y > world.Map.MapHeight - 1)
                        continue;

                    if (background2Tiles[y, x] != null) {
                        background2Tiles[y, x].Draw(spriteBatch);
                    }
                }
            }
        }

        public void LoadMap() {
            string jsonContent;

            using (StreamReader r = new StreamReader(string.Format("Content/Maps/{0}.json", mapId))) {
                jsonContent = r.ReadToEnd();
            }

            mapData = JsonConvert.DeserializeObject(jsonContent);

            tileWidth = (int)mapData.tilewidth;
            tileHeight = (int)mapData.tileheight;
            mapWidth = (int)mapData.width;
            mapHeight = (int)mapData.height;

            foregroundTiles = new Tile[mapHeight, mapWidth];
            backgroundTiles = new Tile[mapHeight, mapWidth];
            background2Tiles = new Tile[mapHeight, mapWidth];

            foreach (var layer in mapData.layers) {
                if (layer.type == "tilelayer") {
                    for (int i = 0; i < layer.data.Count; i++) {
                        int destX = (i % mapWidth);
                        int destY = (int)Math.Floor((double)i / mapWidth);

                        if (layer.data[i] == 0) {
                            if (layer.name.ToString() == TileLayer.foreground.ToString()) {
                                addTileToMap(destX, destY,
                                    new Tile(layer.name.ToString(), TileType.Empty, destX * tileWidth, destY * tileWidth, null, null, null));
                            }
                            continue;
                        }

                        int sourceSetId = getTileSource((int)layer.data[i]);
                        var sourceSet = mapData.tilesets[sourceSetId];
                        double rowLength = sourceSet.imagewidth / tileWidth;
                        int tileSheetLocation = layer.data[i] - sourceSet.firstgid;
                        int sourceX = (int)(tileSheetLocation % rowLength);
                        int sourceY = (int)Math.Floor((tileSheetLocation) / rowLength);
                        if ((int)sourceSet.spacing > 0) {
                            sourceX += (sourceX * (int)sourceSet.spacing) * tileWidth;
                            sourceY += (sourceY * (int)sourceSet.spacing) * tileWidth;
                        } else {
                            sourceX *= tileWidth;
                            sourceY *= tileWidth;
                        }

                        addTileToMap(destX, destY,
                            new Tile(layer.name.ToString(), TileType.Block, destX * tileWidth, destY * tileWidth, sourceSetId, sourceX, sourceY));
                    }
                }
            }

            int[,] offset = {
                {-1, -1},
                {0, -1},
                {1, -1},
                {1, 0},
                {1, 1},
                {0, 1},
                {-1, 1},
                {-1, 0}
            };

            for (int x = 0; x < foregroundTiles.GetLength(0); x++) {
                for (int y = 0; y < foregroundTiles.GetLength(1); y++) {
                    List<Tile> neighbors = new List<Tile>();
                    List<int> nWeight = new List<int>();

                    if (foregroundTiles[x, y].Type == TileType.Empty) {
                        for (int i = 0; i < offset.GetLength(0); i++) {
                            int ox = x + offset[i, 0];
                            int oy = y + offset[i, 1];

                            if (ox < 0 ||
                                oy < 0 ||
                                ox >= mapHeight ||
                                oy >= mapWidth) {
                                continue;
                            }

                            if (foregroundTiles[ox, oy].Type == TileType.Empty) {
                                if (i == 0) {
                                    if (foregroundTiles[ox + 1, oy].Type == TileType.Block ||
                                        foregroundTiles[ox, oy + 1].Type == TileType.Block) {
                                        continue;
                                    }
                                }

                                if (i == 2) {
                                    if (foregroundTiles[ox - 1, oy].Type == TileType.Block ||
                                        foregroundTiles[ox, oy + 1].Type == TileType.Block) {
                                        continue;
                                    }
                                }

                                if (i == 4) {
                                    if (foregroundTiles[ox - 1, oy].Type == TileType.Block ||
                                        foregroundTiles[ox, oy - 1].Type == TileType.Block) {
                                        continue;
                                    }
                                }

                                if (i == 6) {
                                    if (foregroundTiles[ox + 1, oy].Type == TileType.Block ||
                                        foregroundTiles[ox, oy - 1].Type == TileType.Block) {
                                        continue;
                                    }
                                }


                                neighbors.Add(foregroundTiles[ox, oy]);
                                nWeight.Add(i % 2 == 0 ? 14 : 10);
                            }
                        }
                    }

                    foregroundTiles[x, y].Neighbors = neighbors;
                    foregroundTiles[x, y].NeighborWeight = nWeight;
                }
            }
        }

        private void addTileToMap(int x, int y, Tile tile) {
            if (tile.Layer.ToString() == TileLayer.foreground.ToString()) {
                tile.x = x;
                tile.y = y;
                foregroundTiles[y, x] = tile;
            } else if (tile.Layer.ToString() == TileLayer.background.ToString()) {
                backgroundTiles[y, x] = tile;
            } else if (tile.Layer.ToString() == TileLayer.background2.ToString()) {
                background2Tiles[y, x] = tile;
            }
        }

        private int getTileSource(int data) {
            for (int i = mapData.tilesets.Count - 1; i >= 0; i--) {
                if (data >= (int)mapData.tilesets[i].firstgid) {
                    return i;
                }
            }

            return -1;
        }




        //public static Rectangle EntityNeighborRectangle(Entity entity, Map map, )

        public Tile[,] ViewableTiles {
            get {
                return viewableTiles;
            }
        }

        public dynamic MapData {
            get {
                return this.mapData;
            }
        }

        public Tile[,] ForegroundTiles {
            get {
                return this.foregroundTiles;
            }
        }

        public int MapWidth {
            get {
                return mapWidth;
            }
        }

        public int MapHeight {
            get {
                return mapHeight;
            }
        }

        public int TileWidth {
            get {
                return tileWidth;
            }
        }
        public int TileHeight {
            get {
                return tileHeight;
            }
        }

        public int MapPixelWidth {
            get {
                return mapWidth * tileWidth;
            }
        }

        public int MapPixelHeight {
            get {
                return mapHeight * tileHeight;
            }
        }
    }
}
