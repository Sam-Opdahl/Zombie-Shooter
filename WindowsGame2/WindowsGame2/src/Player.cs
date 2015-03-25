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
    public enum PlayerState {
        Walking,
        Shooting, Standing
    }

    class Player : Entity {
        private readonly string[] SPRITE_LOCATION = { "Graphics/shooterofzombonestorsos_final", "Graphics/shooterofzombonesrunnin_final" };
        private Rectangle spriteRect = new Rectangle(0, 0, 33, 63);
        private static readonly int LEG_SPRITE_WIDTH = 23;
        private static readonly int LEG_SPRITE_HEIGHT = 38;
        private static readonly int LEG_SPRITE_GAP = 10;
        private static readonly int MAX_LEG_FRAME = 8;

        private static readonly int MAX_SHOOT_FRAME = 3;
        private static readonly int MAX_SHOOT_FRAME_COUNT = 0;

        private static readonly int MAX_MONEY = 999999999;

        private static readonly float MAX_ATTACK_TIME = 0.25f;
        private static readonly float MAX_DAMAGE_TIME = 0.20f;
        private static readonly Color DAMAGE_COLOR = Color.Red * 0.75f;
        private static readonly Color NORMAL_COLOR = Color.White;

        private List<Texture2D> spriteList;
        private int currentFrame = 0;
        private int frameTimer = 0;
        private readonly int MAX_FRAME_TIME = 2;

        private readonly int START_X = 200;
        private readonly int START_Y = 200;

        private static Texture2D blankTexture;
        private Color drawColor = NORMAL_COLOR;

        private bool isShooting = false;
        private int shootFrame = 0;
        private int shootFrameCount = 0;

        private int moneyAmount = 0;
        
        public bool isBeingAttacked = false;
        private bool takingDamage = false;
        private int damageAmount = 1;
        private float attackTimer = 10f;
        private float damageTimer = 0f;
        private bool hurtSoundPlayed = false;

        public int totalDamageTaken = 0;

        public double bulletStartX() {
            return RelativeCenterX + (10 * Math.Cos(angle - 0.15));
        }

        public double bulletStartY() {
            return RelativeCenterY + (10 * Math.Sin(angle - 0.15));
        }

        public Player(World world) : base(world) {
            this.spriteList = new List<Texture2D>();

            this.location = new Vector2(START_X, START_Y);
            this.velocity = 200;
            this.angle = 0;
            this.collisionBoxSize = 28;
        }

        public void LoadContent(ContentManager content, GraphicsDevice device) {
            foreach (string location in SPRITE_LOCATION) {
                spriteList.Add(content.Load<Texture2D>(location));
            }
            
            blankTexture = new Texture2D(device, 1, 1);
            blankTexture.SetData(new Color[] { new Color(255, 255, 255, 180) });
            this.currentHealth = world.getPlayerAttribute(PlayerAttribute.MaxHealth);
        }

        public override void Update(GameTime gameTime, Input input) {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            //angle for looking toward the mouse position
            angle = Math.Atan2(input.mouseTransformPosition.Y - RelativeCenterY, input.mouseTransformPosition.X - RelativeCenterX);

            //update the player's x position, if a key is pressed.
            float dx = 0;
            if (input.kbs.IsKeyDown(Keys.A)) {
                dx -= velocity * elapsedTime;
            }
            if (input.kbs.IsKeyDown(Keys.D)) {
                dx += velocity * elapsedTime;
            }
            location.X += dx;
            //keep the player's x position within the bounds of the map.
            location.X = MathHelper.Clamp(location.X, spriteRect.Width / 2, 
                    world.Map.MapWidth * world.Map.TileWidth - (spriteRect.Width / 2));
            //handle side colision after player has moved.
            handleSideCollision(dx);

            //similar to above, except update for y position.
            float dy = 0;
            if (input.kbs.IsKeyDown(Keys.W)) {
                dy -= velocity * elapsedTime;
            }
            if (input.kbs.IsKeyDown(Keys.S)) {
                dy += velocity * elapsedTime;
            }
            location.Y += dy;
            location.Y = MathHelper.Clamp(location.Y, spriteRect.Width / 2,
                    world.Map.MapHeight * world.Map.TileHeight - (spriteRect.Width / 2));
            handleTopBottomCollision(dy);


            if (isBeingAttacked && !takingDamage) {
                attackTimer += elapsedTime;
                if (attackTimer > MAX_ATTACK_TIME) {
                    takingDamage = true;
                    attackTimer = 0;
                }
            } else if (!isBeingAttacked) {
                attackTimer = 10f;
            } 

            if (takingDamage) {
                damageTimer += elapsedTime;
                if (damageTimer <= MAX_DAMAGE_TIME) {
                    drawColor = DAMAGE_COLOR;
                    currentHealth -= damageAmount;
                    totalDamageTaken += damageAmount;
                    if (!hurtSoundPlayed) {
                        SoundManager.getInstance().playSound(Sound.PlayerHurt);
                        hurtSoundPlayed = true;
                    }
                } else {
                    takingDamage = false;
                    damageTimer = 0;
                    drawColor = NORMAL_COLOR;
                    hurtSoundPlayed = false;
                }
            }

            if (isShooting) {
                if (shootFrameCount++ > MAX_SHOOT_FRAME_COUNT) {
                    shootFrameCount = 0;
                    if (shootFrame++ >= MAX_SHOOT_FRAME) {
                        isShooting = false;
                        shootFrame = 0;
                    }
                }
            }

            //Handle animating the player's avatar
            if (frameTimer++ > MAX_FRAME_TIME) {
                frameTimer = 0;

                if (Math.Abs(dx) > 0 || Math.Abs(dy) > 0) {
                    if (currentFrame++ >= MAX_LEG_FRAME) {
                        currentFrame = 0;
                    }
                } else {
                    currentFrame = 0;
                }
            }
        }

        public void setNormalColor() {
            drawColor = NORMAL_COLOR;
        }

        public override void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Draw(spriteList[1],
                    location,
                    new Rectangle((LEG_SPRITE_WIDTH + LEG_SPRITE_GAP) * currentFrame, 0, LEG_SPRITE_WIDTH, LEG_SPRITE_HEIGHT),
                    drawColor,
                    (float)angle + (3 * MathHelper.Pi / 2),
                    getLegCenter(), 1.0f, SpriteEffects.None, 0);

            spriteBatch.Draw(spriteList[0], location, new Rectangle(spriteRect.X + (shootFrame * spriteRect.Width), spriteRect.Y, spriteRect.Width, spriteRect.Height),
                    drawColor, (float)angle + (3 * MathHelper.Pi / 2),
                    getCenterLocation(), 1.0f, SpriteEffects.None, 0);
        }

        public void shootBullet() {
            isShooting = true;
            shootFrameCount = 0;
        }

        private Vector2 getCenterLocation() {
            return new Vector2(spriteRect.Width / 2,
                    spriteRect.Width / 2);
        }

        private Vector2 getLegCenter() {
            return new Vector2(LEG_SPRITE_WIDTH / 2, LEG_SPRITE_HEIGHT / 2);
        }

        public int MoneyAmount {
            get {
                return moneyAmount;
            }
        }

        public void modifyMoney(int amount) {
            moneyAmount += amount;

            if (moneyAmount < 0) {
                moneyAmount = 0;
            } else if (moneyAmount > MAX_MONEY) {
                moneyAmount = MAX_MONEY;
            }
        }

    }
}
