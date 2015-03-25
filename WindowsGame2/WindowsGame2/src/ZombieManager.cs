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
    class ZombieManager {

        //determines the length in between the zombie's path finds
        //higher value = better performance, but it will take longer for hordes to find the paths around obastacles
        //lower value = less performance, but more zombies will find their paths in a shorter time
        private static readonly float PATH_FINDING_INTERVAL = 1f;
        private static readonly float SLOW_INTERVAL = 2f;
        private static Random rand = new Random();

        private List<Zombie> zombieList;
        private List<Zombie> zombiePathsToUpdate;
        private World world;
        private float elapsedTime;

        private Rectangle prevCameraTileRect;
        private List<Tile> availableSpawnPoints;
        public int zombiesKilled = 0;

        public bool playerAttacked = false;

        public ZombieManager(World world) {
            this.world = world;
            zombieList = new List<Zombie>();
            zombiePathsToUpdate = new List<Zombie>();
            availableSpawnPoints = new List<Tile>();
        }

        public void Update(GameTime gameTime, Input input) {
            elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            updateSpawnPoints();
            spawnZombies();

            playerAttacked = false;
            for (int i = zombieList.Count - 1; i >= 0; i--) {
                Zombie zombie = zombieList[i];
                if (!zombie.IsActive) {
                    zombieList.RemoveAt(i);
                    if (zombiePathsToUpdate.Contains(zombie)) {
                        zombiePathsToUpdate.Remove(zombie);
                    }
                    zombiesKilled++;
                    continue;
                }

                zombie.Update(gameTime, input);

                if (zombie.stuck) {
                    zombieList.Remove(zombie);
                    ZombiesSpawnedThisWave--;
                    continue;
                }

                if (zombie.requiresNewPath) {
                    //prevent a zombie from being added to the update queue multiple times
                    if (!zombiePathsToUpdate.Contains(zombie)) {
                        zombiePathsToUpdate.Add(zombie);
                    }
                } else {
                    //zombie no longer needs a path. if it is in the queue, remove it.
                    if (zombiePathsToUpdate.Contains(zombie)) {
                        zombiePathsToUpdate.Remove(zombie);
                    }
                }
            }

            world.player.isBeingAttacked = playerAttacked;

            //If zombie is far away from player, remove from list and respawn
            //this way zombies will not get too far away which can can be computationally expensive
            for (int i = zombieList.Count - 1; i >= 0; i--) {
                if (zombieList[i].DistToPlayer > 700) {
                    Zombie z = zombieList[i];
                    zombieList.Remove(z);
                    if (zombiePathsToUpdate.Contains(z)) {
                        zombiePathsToUpdate.Remove(z);
                    }
                    ZombiesSpawnedThisWave--;
                }
            }

            //create a sort of "horde" effect with the zombies.
            //this will prevent them all from piling on top of each other and will push them away.
            Zombie[] zombies = zombieList.ToArray();
            for (int i = zombieList.Count - 1; i >= 0; i--) {
                //main zombie to push others away from
                Zombie z = zombies[i];
                float dx = 0;
                float dy = 0;
                for (int j = Math.Max(0, i - 1); j >= 0; j--) {
                    //zombie being pushed away
                    Zombie z2 = zombies[j];
                    if (Vector2.Distance(z.Location, z2.Location) <= 20) {
                        //don't want to push itself away from itself
                        if (z == z2) continue;
                        //push the zombie away based on the current angle between the two zombies.
                        double angle = Math.Atan2(z.Location.Y - z2.Location.Y, z.Location.X - z2.Location.X);
                        dx += (float)Math.Cos(angle) * 0.75f;
                        dy += (float)Math.Sin(angle) * 0.75f;
                    }
                }
                //if a zombie did get pushed, check for collision against the map.
                if (dx != 0 || dy != 0)
                    z.UpdateCollisions(dx, dy);
            }




            //update zombie paths if enough time has passed. if the game is running slow, the interval will be longer.
            if (elapsedTime > (gameTime.IsRunningSlowly ? SLOW_INTERVAL : PATH_FINDING_INTERVAL)) {
                elapsedTime = 0f;
                //zombies that don't yet have paths take precedence over zombies that already have paths
                //zombiePathsToUpdate.OrderBy(x => !x.HasPath);
                    //.ThenBy(x => x.DistToPlayer);
                zombiePathsToUpdate.Sort((a, b) => a.HasPath.CompareTo(b.HasPath));

                if (zombiePathsToUpdate.Count > 0) {
                    //Update the first zombie's path in the queue
                    Zombie zombie = zombiePathsToUpdate[0];
                    zombiePathsToUpdate.Remove(zombie);
                    zombie.createNewPath();

                    //Check for any zombies which are near the previous zombie.
                    //if they are close, set their path along with the leading zombie.
                    //this cuts back on full path updates for all zombies and saves a LOT of resources.
                    Rectangle r = zombie.getSurroundingTileRectangle(1);
                    for (int i = zombiePathsToUpdate.Count - 1; i >= 0; i--) {
                        Zombie z2 = zombiePathsToUpdate[i];
                        if (r.Intersects(z2.CollisionBox)) {
                            z2.setNewPath(zombie.CurrentPath, zombie.EndOfPath);
                            zombiePathsToUpdate.Remove(z2);
                        }
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch) {
            foreach (Zombie zombie in zombieList) {
                if (zombie.CollisionBox.Intersects(world.camera.CollisionBox)) {
                    zombie.Draw(spriteBatch);
                }
            }
        }

        public void reset() {
            zombieList.Clear();
            zombiePathsToUpdate.Clear();
        }

        public void spawnZombies() {
            if (ZombiesSpawnedThisWave < MaxZombiesToSpawn) {
                if (zombieList.Count < MaxZombiesAtOnce) {
                    Vector2 pos = availableSpawnPoints[rand.Next(0, availableSpawnPoints.Count)].vCenter;
                    zombieList.Add(new Zombie(world, pos, rand.Next(100, 151), 50));
                    ZombiesSpawnedThisWave++;
                }
            }
        }

        //Finds available positions on the map around the viewable area to spawn zombies.
        public void updateSpawnPoints() {
            //If the camera has not moved, there is no need to update spawns.
            if (world.camera.TileRectangle != prevCameraTileRect) {
                prevCameraTileRect = world.camera.TileRectangle;
                availableSpawnPoints.Clear();
                   
                //get a one tile rectangle larger than the camer'as viewable area,
                //the one tile edge around the camera is where the zombies will spawn
                Rectangle r = new Rectangle(world.camera.TileRectangle.X - 1,
                        world.camera.TileRectangle.Y - 1,
                        world.camera.TileRectangle.Width + 2,
                        world.camera.TileRectangle.Height + 2);

                for (int x = r.X; x < r.Width + r.X; x++) {
                    for (int y = r.Y; y < r.Height + r.Y; y++) {
                        //make sure the tile is within the bounds of the map
                        if (x == r.X || x == r.X + r.Width - 1 ||
                            y == r.Y || y == r.Y + r.Height - 1) {

                            //only get the 'border' tiles around the camera area
                            if (y >= 0 && y < world.Map.MapHeight &&
                                x >= 0 && x < world.Map.MapWidth) {

                                if (world.Map.ForegroundTiles[y, x].Type == TileType.Empty) {
                                    availableSpawnPoints.Add(world.Map.ForegroundTiles[y, x]);
                                }
                            }
                        }
                    }
                }
            }
        }

        public List<Zombie> ZombieList {
            get {
                return zombieList;
            }
        }

        //Get the number of zombies left in the current wave
        public int ZombiesLeft {
            get {
                return MaxZombiesToSpawn + zombieList.Count - ZombiesSpawnedThisWave;
            }
        }

        public int MaxZombiesToSpawn { get; set; }
        public int MaxZombiesAtOnce { get; set; }
        public int ZombiesSpawnedThisWave { get; set; }

        public bool isWaveOver {
            get {
                return zombieList.Count == 0 && ZombiesSpawnedThisWave >= MaxZombiesToSpawn;
            }
        }
    }
}
