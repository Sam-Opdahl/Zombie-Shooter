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
    class BulletManager
    {
        private List<Bullet> bulletList;
        private int bulletTimer = 0;
        public int clipSize;
        private World world;
        public bool hasReloaded = false;
        public bool reloading = false;
        public float reloadTimer = 10f;

        public long bulletsFired = 0;
        public long bulletHits = 0;

        public BulletManager(World world)
        {
            bulletList = new List<Bullet>();
            this.world = world;
        }

        public void LoadContent() {
            clipSize = world.getGunAttribute(GunAttribute.ClipSize);
        }

        public void Update(World world, GameTime gameTime, Input input)
        {
            int bulletMaxTime = world.getGunAttribute(GunAttribute.FireRate);
            if ((bulletTimer++ >= bulletMaxTime )
                    && ((world.getGunAttribute(GunAttribute.Automatic) == 0) ? input.isMouseClicked() : input.isMouseDown())
                    && (clipSize > 0))
            {
                if (reloading) {
                    reloading = false;
                    reloadTimer = 0;
                }

                SoundManager.getInstance().playSound(Sound.Shooting);

                bulletTimer = 0;
                bulletList.Add(new Bullet(world, world.player.bulletStartX(), world.player.bulletStartY(), world.player.Angle, 20));
                world.player.shootBullet();
                clipSize--;
                bulletsFired++;
            }

            if (input.isKeyPressed(Keys.R)) {
                if (clipSize < world.getGunAttribute(GunAttribute.ClipSize) && !reloading) {
                    hasReloaded = true;
                    reloading = true;
                    SoundManager.getInstance().playSound(Sound.GunLoading);
                }
            }

            if (reloading) {
                reloadTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (clipSize < world.getGunAttribute(GunAttribute.ClipSize)) {
                    if (reloadTimer > ((float)world.getGunAttribute(GunAttribute.ReloadSpeed)) / 100) {
                        reloadTimer = 0f;
                        clipSize++;
                    }
                } else {
                    SoundManager.getInstance().playSound(Sound.GunLoading);
                    reloading = false;
                    reloadTimer = 0f;
                }
            }

            for (int i = bulletList.Count - 1; i >= 0; i--)
            {
                if (!bulletList[i].isActive)
                {
                    bulletList.RemoveAt(i);
                    continue;
                }
                bulletList[i].Update(gameTime);
            }

            foreach (Zombie zombie in world.zombieManager.ZombieList) {
                foreach (Bullet bullet in bulletList) {
                    if (bullet.isActive && zombie.IsActive && !zombie.IsDisplayingReward) {
                        if (zombie.BulletCollisionBox.Intersects(bullet.CollisionBox)) {
                            if (bullet.zombiesHit.Count == 0) {
                                bulletHits++;
                            }

                            if (!bullet.zombiesHit.Contains(zombie)) {
                                zombie.onBulletHit(world.getGunAttribute(GunAttribute.BulletDamage));
                                bullet.zombiesHit.Add(zombie);
                                if (bullet.zombiesHit.Count >= world.getGunAttribute(GunAttribute.StoppingPower)) {
                                    bullet.isActive = false;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Bullet b in bulletList)
            {
                b.Draw(spriteBatch);
            }
        }

        public float ShotAccuracy {
            get {
                return bulletsFired == 0 ? 0 : ((float)bulletHits / (float)bulletsFired) * 100f;
            }
        }
    }
}
