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
    public class Input
    {
        public KeyboardState kbs = new KeyboardState();
        public KeyboardState prevkbs;
        public MouseState ms = new MouseState();
        public MouseState prevms;
        public MouseState mscentered;

        public Vector2 mouseTransformPosition;

        public void Update()
        {
            prevkbs = kbs;
            prevms = ms;

            kbs = Keyboard.GetState();
            ms = Mouse.GetState();
        }

        public Rectangle getMouseRect() {
            return new Rectangle(ms.X, ms.Y, 1, 1);
        }

        public Rectangle getPrevMouseRect() {
            return new Rectangle(prevms.X, prevms.Y, 1, 1);
        }

        public void updateMouseTransform(Matrix transform)
        {
            Vector2 mousePos = new Vector2(ms.X, ms.Y);
            mouseTransformPosition = Vector2.Transform(mousePos, transform);
        }

        public bool isMouseDown()
        {
            return ms.LeftButton == ButtonState.Pressed && prevms.LeftButton == ButtonState.Pressed;
        }

        public bool isMouseClicked() {
            return ms.LeftButton == ButtonState.Pressed && prevms.LeftButton != ButtonState.Pressed;
        }

        public bool isKeyPressed(Keys key) {
            return kbs.IsKeyDown(key) && !prevkbs.IsKeyDown(key);
        }

        public bool isKeyDown(Keys key) {
            return kbs.IsKeyDown(key);
        }
    }
}
