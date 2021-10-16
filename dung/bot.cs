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
    public class Robot : MapObject
    {
        public override int Type { get; protected set; }

        public override double X { get; protected set; }
        public override double Y { get; protected set; }
        public override string Action { get; protected set; }
        public override List<Texture2D> Textures { get; protected set; }

        public int Direction { get; private set; }

        private int texturePhase;
        
        /// <summary>
        /// With file reading
        /// </summary>
        public Robot(int type, ContentManager contentManager, double x, double y)
        {
            Type = type;

            X = x;
            Y = y;

            Action = "id";
            Direction = 0;

            /*using (StreamReader sr = new StreamReader("info/global/objects/robots/" + type.ToString() + "/main_info"))
            {
                
            }*/

            UpdateTexture(contentManager, true);
        }

        public Robot(int type, ContentManager contentManager, double x, double y, Robot sampleRobot)
        {
            Type = type;

            X = x;
            Y = y;

            Action = "no";
            Direction = 0;

            UpdateTexture(contentManager, true);
        }

        private void UpdateTexture(ContentManager contentManager, bool reload)
        {
            if (reload)
            {
                Textures = new List<Texture2D>();

                Textures.Add(contentManager.Load<Texture2D>("tmpbot"));

                texturePhase = 0;

                //while(File.Exists("Content/"))
            }
            else
            {
                texturePhase++;

                texturePhase %= Textures.Count;
            }
        }

        public override void Update(ContentManager contentManager, GameWorld gameWorld, int myIndex)
        {
            double previousX = X, previousY = Y;

            Tuple<double, double> newCoords = addToCoords(X, Y, 0.1, Direction);

            X = newCoords.Item1;
            Y = newCoords.Item2;

            Tuple<int, int> leftcoords = getCoordsInDirection(Direction, (int)X, (int)Y, -1);

            if (leftcoords.Item1 < 0 || leftcoords.Item2 < 0 || leftcoords.Item1 >= gameWorld.blocks.Count || leftcoords.Item2 >= gameWorld.blocks[leftcoords.Item1].Count || !gameWorld.blocks[leftcoords.Item1][leftcoords.Item2].passable)
            {
                if ((int)X != (int)previousX || (int)Y != (int)previousY)
                {
                    if (X < 0 || Y < 0 || X >= gameWorld.blocks.Count || Y >= gameWorld.blocks[(int)X].Count || !gameWorld.blocks[(int)X][(int)Y].passable)
                    {
                        X = previousX;
                        Y = previousY;

                        Direction++;
                    }
                }
            }
            else
            {
                Direction--;

                newCoords = addToCoords(X, Y, 0.1, Direction);
                
                X = newCoords.Item1;
                Y = newCoords.Item2;
            }

            if (Direction < 0)
            {
                Direction = 4 - (Direction * -1 % 4);
            }
            else
            {
                Direction %= 4;
            }

            previousX = X;
            previousY = Y;

            UpdateTexture(contentManager, false);
        }

        public override void Draw(SpriteBatch spriteBatch, int x, int y)
        {
            spriteBatch.Draw(Textures[texturePhase], new Vector2(x-Textures[texturePhase].Width/2, y - Textures[texturePhase].Height), Color.White);
        }

        private Tuple<int, int> getCoordsInDirection(int direction, int x, int y, int addDirection)
        {
            int tmpdirection = direction;

            tmpdirection += addDirection;

            tmpdirection %= 4;

            if (tmpdirection == 0)
            {
                return new Tuple<int, int>(x, y - 1);
            }
            else if (tmpdirection == 1)
            {
                return new Tuple<int, int>(x + 1, y);
            }
            else if (tmpdirection == 2)
            {
                return new Tuple<int, int>(x, y + 1);
            }

            return new Tuple<int, int>(x - 1, y);
        }

        private Tuple<double, double> addToCoords(double x, double y, double speed, int direction)
        {
            if (direction == 0)
            {
                return new Tuple<double, double>(x, y - speed);
            }
            else if (direction == 1)
            {
                return new Tuple<double, double>(x + speed, y);
            }
            else if (direction == 2)
            {
                return new Tuple<double, double>(x, y + speed);
            }

            return new Tuple<double, double>(x - speed, y);
        }
    }
}