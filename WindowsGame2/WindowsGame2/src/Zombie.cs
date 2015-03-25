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

    class Zombie : Entity {

        private static readonly int ZOMBIE_COLLISION_BOX_SIZE = 20;
        private static readonly string[] SPRITE_LOCATION = { "Graphics\\zombie1" };
        private static List<Texture2D> spriteList = new List<Texture2D>();
        private static readonly int SPRITE_WIDTH = 48;
        private static readonly int SPRITE_HEIGHT = 40;
        private static readonly int MAX_FRAME_TIME = 5;
        private static readonly int MAX_FRAME = 7;

        private static readonly int HEALTH_BAR_WIDTH = 20;
        private static readonly int HEALTH_BAR_HEIGHT = 5;
        private static readonly int HEALTH_BAR_Y_OFFSET = 8;

        private int currentFrame = 0;
        private int frameTimer = 0;

        private bool isActive = true;
        private bool displayingReward = false;
        private Path<Tile> path = null;
        private List<Tile> pathList = null;
        private Tile target;
        public bool requiresNewPath = false;
        public bool stuck = false;
        private Tile endOfMainPath;
        private Tile endOfTempPath;
        private int pathUpdates = 0;
        private float returnToPlayerTimer = 0f;

        private float rewardAlpha = 0;
        private Vector2 rewardPosition;
        private static readonly float REWARD_TRANSITION_SPEED = 0.05f;
        private static readonly int REWARD_MOVEMENT_SPEED = 2;

        private Ray playerDetectRay;
        private float distToPlayer;

        private float dx;
        private float dy;

        public Zombie(World world, Vector2 startLocation) : this(world, startLocation, 75, 50) { }

        public Zombie(World world, Vector2 startLocation, int velocity) : this(world, startLocation, velocity, 50) { }

        public Zombie(World world, Vector2 startLocation, int velocity, int health) : base(world) {
            this.currentHealth = health;
            this.maxHealth = health;

            location = startLocation;
            this.velocity = velocity;
            angle = 0;
            collisionBoxSize = ZOMBIE_COLLISION_BOX_SIZE;
        }

        public static void LoadContent(ContentManager content) {
            foreach (string location in SPRITE_LOCATION) {
                spriteList.Add(content.Load<Texture2D>(location));
            }
        }

        public void onCreate() {
            if (!hasPlayerLos()) {
                requiresNewPath = true;
            }

            angle = Math.Atan2(target.Center.Y - RelativeCenterY, target.Center.X - RelativeCenterX);
        }

        public override void Update(GameTime gameTime, Input input) {
            if (!isActive) {
                return;
            }

            if (displayingReward) {
                rewardAlpha += REWARD_TRANSITION_SPEED;

                if (rewardAlpha < 1.0f) {
                    rewardPosition.Y -= REWARD_MOVEMENT_SPEED;
                } else if (rewardAlpha >= 1.25f) {
                    this.isActive = false;
                }
                return;
            }

            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (hasPlayerLos()) {
                if (path == null) {
                    requiresNewPath = false;
                    angle = Math.Atan2(world.player.Location.Y - RelativeCenterY, world.player.Location.X - RelativeCenterX);
                } else {
                    returnToPlayerTimer += elapsedTime;
                    if (pathList.Count <= 0 || returnToPlayerTimer > 1f) {
                        angle = Math.Atan2(world.player.Location.Y - RelativeCenterY, world.player.Location.X - RelativeCenterX);
                        requiresNewPath = false;
                        resetPathData();
                    } else {
                        UpdatePath();
                        if (getTileLocation() == target) {
                            findNextTarget();
                        }
                        angle = Math.Atan2(target.Center.Y - RelativeCenterY, target.Center.X - RelativeCenterX);
                    }
                }
            } else {
                if (path == null) {
                    requiresNewPath = true;
                    angle = Math.Atan2(world.player.Location.Y - RelativeCenterY, world.player.Location.X - RelativeCenterX);
                } else {
                    returnToPlayerTimer = 0;
                    UpdatePath();
                    if (getTileLocation() == target) {
                        findNextTarget();
                    }
                    angle = Math.Atan2(target.Center.Y - RelativeCenterY, target.Center.X - RelativeCenterX);

                    if (pathUpdates > 2) {
                        requiresNewPath = true;
                    }
                }
            }

            if (distToPlayer > 20) {
                
                Tile t = getTileLocation();

                dx = (float)Math.Cos(angle) * velocity * elapsedTime;
                dy = (float)Math.Sin(angle) * velocity * elapsedTime;

                location.X += dx;
                handleSideCollision(dx);

                location.Y += dy;
                handleTopBottomCollision(dy);

                if (sideCollision(true, getIntersectingRows(), getUpperBoundTile(CollisionBox.Right) - 1) != null ||
                    location.X < 0 || location.Y < 0 || location.X > world.Map.MapPixelWidth || location.Y > world.Map.MapPixelHeight) {
                    stuck = true;
                    isActive = false;
                    return;
                }
            } else {
                world.zombieManager.playerAttacked = true;
            }

            if (frameTimer++ > MAX_FRAME_TIME) {
                frameTimer = 0;

                if (Math.Floor(Math.Abs(dx)) > 0 || Math.Floor(Math.Abs(dy)) > 0) {
                    if (currentFrame++ >= MAX_FRAME) {
                        currentFrame = 0;
                    }
                } else {
                    currentFrame = 0;
                }
            }
        }

        private void resetPathData() {
            path = null;
            endOfMainPath = null;
            endOfTempPath = null;
            returnToPlayerTimer = 0f;
            pathUpdates = 0;
        }

        public void UpdateCollisions(float dx, float dy) {
                location.X += dx;
                handleSideCollision(dx);
                location.Y += dy;
                handleTopBottomCollision(dy);
        }

        public void UpdatePath() {
            //tile that points to end of path (player's position)
            Tile end = world.player.getTileLocation();

            if (end != endOfTempPath) {
                //player moved to new tile, add the new position to the path.
                pathUpdates++;
                path = path.AddStep(end, 1);
                int previousLength = pathList.Count;
                pathList = path.Reverse().ToList();
                pathList.RemoveRange(0, pathList.Count - previousLength - 1);
                endOfTempPath = end;
            }
        }

        public void createNewPath() {
            Tile end = world.player.getTileLocation();
            if (end != endOfMainPath) {
                //A* estimation algorithm (currently using dijkstra)
                Func<Tile, double> estimate = n => 1;// Math.Abs(n.x - end.x) + Math.Abs(n.y - end.y);
                //Find a new path
                Tile t = getTileLocation();
                if (t == null) {
                    return;
                }
                path = Path<Tile>.FindPath(t, end, estimate);
                if (path != null) {
                    pathList = path.Reverse().ToList();
                    findNextTarget();
                    endOfMainPath = end;
                    endOfTempPath = null;
                    requiresNewPath = false;
                    pathUpdates = 0;
                }
            }
        }

        public void setNewPath(Path<Tile> path, Tile end) {
            if (end != endOfMainPath) {
                if (path != null) {
                    this.path = path;
                    this.pathList = path.Reverse().ToList();
                    this.endOfMainPath = end;
                    endOfTempPath = null;
                    findNextTarget();
                    requiresNewPath = false;
                    pathUpdates = 0;
                }
            }
        }


        //Check if the zombie has the player in its line of sight
        private bool hasPlayerLos() {

            distToPlayer = Vector2.Distance(location, world.player.Location);

            //Ray used for detecting obstacles between the zombie and player
            playerDetectRay = new Ray(Location3D, unitDirection(world.player.Location));

            Rectangle r = getDistanceTileRectangle(world.player);
            //max x/y length of the viewable tile rectangle
            int ly = world.Map.ViewableTiles.GetLength(0) - 1;
            int lx = world.Map.ViewableTiles.GetLength(1) - 1;

            for (int x = r.X; x < r.Width + r.X; x++) {
                int cx = x - world.camera.TileRectangle.X;
                if (cx < 0 || cx > lx)
                    continue;

                for (int y = r.Y; y < r.Height + r.Y; y++) {
                    int cy = y - world.camera.TileRectangle.Y;

                    if (cy < 0 || cy > ly)
                        continue;
                    if (world.Map.ViewableTiles[cy, cx] == null)
                        continue;

                    //check if ray intersects any foreground tiles (walls)
                    if (playerDetectRay.Intersects(world.Map.ViewableTiles[cy, cx].boundingBox) != null) {
                        //if ray intersects a tile, check if it blocking the way between the zombie and player, or tile is behind player.
                        if (Vector2.Distance(location, world.Map.ViewableTiles[cy, cx].vCenter) < distToPlayer) {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        //public void drawPath(SpriteBatch spriteBatch) {
        //    if (pathList != null && pathList.Count > 0) {
        //        Tile tile = pathList[0];
        //        spriteBatch.Draw(world.blankTexture,
        //                    new Rectangle(tile.x * (int)world.Map.MapData.tilewidth, tile.y * (int)world.Map.MapData.tileheight, (int)world.Map.MapData.tilewidth, (int)world.Map.MapData.tileheight),
        //                    Color.Red);

        //        int i = 0;
        //        foreach (Tile t in pathList) {
        //            if (i++ == 0) continue;
        //            spriteBatch.Draw(world.blankTexture,
        //                new Rectangle(t.x * (int)world.Map.MapData.tilewidth, t.y * (int)world.Map.MapData.tileheight, (int)world.Map.MapData.tilewidth, (int)world.Map.MapData.tileheight),
        //                Color.White);
        //        }
        //    }
        //}

        public override void Draw(SpriteBatch spriteBatch) {

            if (!isActive) {
                return;
            }

            if (displayingReward) {
                string s = "+$" + string.Format("{0:n0}", world.getPlayerAttribute(PlayerAttribute.ZombieAward));
                spriteBatch.DrawString(Main.getFont(Font.Motorwerk), s, rewardPosition + new Vector2(2), Color.Black * rewardAlpha);
                spriteBatch.DrawString(Main.getFont(Font.Motorwerk), s, rewardPosition, Color.White * rewardAlpha);
                return;
            }

            // ****** Debug Draw calls - ignore ******

            //if (path != null) {
            //    foreach (Tile t in path) {
            //        spriteBatch.Draw(world.blankTexture,
            //            new Rectangle(t.x * (int)world.Map.MapData.tilewidth, t.y * (int)world.Map.MapData.tileheight, (int)world.Map.MapData.tilewidth, (int)world.Map.MapData.tileheight),
            //            Color.White);
            //    }
            //}

            //Tile tile = target;
            //spriteBatch.Draw(world.blankTexture,
            //        new Rectangle(tile.x * (int)world.Map.MapData.tilewidth, tile.y * (int)world.Map.MapData.tileheight, (int)world.Map.MapData.tilewidth, (int)world.Map.MapData.tileheight),
            //        Color.Red);

            //foreach (Tile t in target.Neighbours) {
            //    spriteBatch.Draw(world.blankTexture,
            //        new Rectangle(t.x * (int)world.Map.MapData.tilewidth, t.y * (int)world.Map.MapData.tileheight, (int)world.Map.MapData.tilewidth, (int)world.Map.MapData.tileheight),
            //        Color.Blue);
            //}

            //spriteBatch.Draw(world.blankTexture, new Rectangle((int)playerDetectRay.Position.X, (int)playerDetectRay.Position.Y, 2, 1000),
            //        null,
            //        Color.White, (float)Math.Atan2((float)world.player.Location.Y - RelativeCenterY, (float)world.player.Location.X - RelativeCenterX) + (3 * MathHelper.Pi / 2),
            //        Vector2.Zero, SpriteEffects.None, 0);

            //foreach (Tile t in world.Map.ViewableTiles) {
            //    if (t == null) continue;
            //    spriteBatch.Draw(world.blankTexture,
            //        t.DestRect,
            //        Color.Blue);
            //}


            //foreach (Tile t in world.Map.ViewableTiles) {
            //    if (t == null) continue;
            //    spriteBatch.Draw(world.blankTexture,
            //        t.DestRect,
            //        Color.Yellow);
            //}

            //spriteBatch.Draw(world.blankTexture,
            //    new Rectangle(r.X * Tile.SIZE, r.Y * Tile.SIZE, r.Width * Tile.SIZE, r.Height * Tile.SIZE),
            //    Color.Red);

            //Rectangle r = getDistanceTileRectangle(world.player);
            //int ly = world.Map.ViewableTiles.GetLength(0) - 1;
            //int lx = world.Map.ViewableTiles.GetLength(1) - 1;
            //for (int x = r.X; x < r.Width + r.X ; x++) {
            //    for (int y = r.Y; y < r.Height + r.Y; y++) {
            //        int cy = y - world.camera.TileRectangle.Y;
            //        int cx = x - world.camera.TileRectangle.X;

            //        if (cy < 0 || cy > ly || cx < 0 || cx > lx)
            //            continue;
            //        if (world.Map.ViewableTiles[cy, cx] == null)
            //            continue;

            //        spriteBatch.Draw(world.blankTexture,
            //            world.Map.ViewableTiles[cy, cx].DestRect,
            //            Color.LightGreen);
            //    }
            //}

            spriteBatch.Draw(spriteList[0], location, 
                    new Rectangle(SPRITE_WIDTH * currentFrame, 0, SPRITE_WIDTH, SPRITE_HEIGHT),
                    Color.White, (float)angle + (3 * MathHelper.Pi / 2), getCenterLocation(), 1f, SpriteEffects.None, 0);

            if (currentHealth < maxHealth) {
                int remainingHealth = (int)Math.Round(HEALTH_BAR_WIDTH * (double)currentHealth / maxHealth);
                spriteBatch.Draw(world.blankTexture, 
                    new Rectangle((int)RelativeCenterX - (HEALTH_BAR_WIDTH / 2), (int)RelativeCenterY - HEALTH_BAR_Y_OFFSET, HEALTH_BAR_WIDTH, HEALTH_BAR_HEIGHT), 
                    Color.Red);
                spriteBatch.Draw(world.blankTexture, 
                    new Rectangle((int)RelativeCenterX - (HEALTH_BAR_WIDTH / 2), (int)RelativeCenterY - HEALTH_BAR_Y_OFFSET, remainingHealth, HEALTH_BAR_HEIGHT), 
                    Color.GreenYellow);
            }
        }

        public void onBulletHit(int damage) {
            currentHealth -= damage;
            if (currentHealth <= 0) {
                if (isActive && !displayingReward) {
                    displayingReward = true;
                    world.player.modifyMoney(world.getPlayerAttribute(PlayerAttribute.ZombieAward));
                    rewardPosition = this.location - new Vector2(10, 0);
                }
            }
        }


        private void findNextTarget() {
            if (path.Count() > 0) {
                target = pathList[0];
                pathList.RemoveAt(0);
            } else {
                target = world.player.getTileLocation();
            }
        }

        public Vector2 getCenterLocation() {
            return new Vector2(SPRITE_WIDTH / 2,
                    SPRITE_HEIGHT / 2);
        }

        protected int bulletCollisionBoxSize = 30;
        public Rectangle BulletCollisionBox {
            get {
                return new Rectangle((int)location.X - (bulletCollisionBoxSize / 2), (int)location.Y - (bulletCollisionBoxSize / 2),
                    bulletCollisionBoxSize, bulletCollisionBoxSize);
            }
        }

        public bool HasPath {
            get {
                return path != null;
            }
        }

        public float DistToPlayer {
            get {
                return distToPlayer;
            }
        }

        public Path<Tile> CurrentPath {
            get {
                return path;
            }
        }

        public Tile EndOfPath {
            get {
                return endOfMainPath;
            }
        }

        public bool IsActive {
            get {
                return isActive;
            }
        }

        public bool IsDisplayingReward {
            get {
                return displayingReward;
            }
        }
    }
}
