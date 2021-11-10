using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace dung
{
    public class Slider
    {
        public int X;
        public int Y;

        public int Width { get; protected set; }
        public int Height { get; protected set; }

        public bool LockX = false, LockY = false;
        
        public int SliderX { get; protected set; }
        public int SliderY { get; protected set; }

        public Texture2D SliderTexture { get; protected set; }
        
        public Slider(int x, int y, int width, int height, int sliderX, int sliderY, Texture2D sliderTexture)
        {
            X = x;
            Y = y;

            Width = width;
            Height = height;

            SliderX = sliderX;
            SliderY = sliderY;

            SliderTexture = sliderTexture;
        }

        public void Update()
        {
            var mouseState = Mouse.GetState();

            if (mouseState.X >= X && mouseState.Y >= Y && mouseState.X < X + Width && mouseState.Y < Y + Height && mouseState.LeftButton == ButtonState.Pressed)
            {
                if(!LockX)
                {
                    SliderX = mouseState.X - X;
                }

                if (!LockY)
                {
                    SliderY = mouseState.Y - Y;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(SliderTexture, new Vector2(X + SliderX - SliderTexture.Width / 2, Y + SliderY - SliderTexture.Height / 2), Color.White);
        }

        public void ChangeCoords(int x, int y)
        {
            X = x;
            Y = y;
        }

        public void ChangeDimersions(int width, int height)
        {
            Width = width;
            Height = height;

            if (X >= width)
            {
                SliderX = width - 1;
            }

            if (SliderY >= height)
            {
                SliderX = height - 1;
            }
        }
    }
}