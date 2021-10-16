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
    public class Bullet:MapObject
    {
        public override double X { get; protected set; }
        public override double Y { get; protected set; }

        public override double Radius { get; protected set; }
        public override int Type { get; protected set; }
        public override List<Texture2D> Textures { get; protected set; }
        public int damage { get; protected set; }
        public double degDirection { get; protected set; }
        public double speed { get; protected set; }
        private int texturesPhase;
        public override bool alive { get; protected set; }
        public int beatFromWalls { get; protected set; }
        public int maxBeatFromWalls { get; protected set; }

        /// <summary>
        /// With file reading
        /// </summary>
        /// <param name="contentManager"></param>
        /// <param name="type"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Bullet(ContentManager contentManager, int type, double x, double y, double directon)
        {
            beatFromWalls = 0;

            alive = true;

            //given shit
            X = x;
            Y = y;

            Type = type;

            degDirection = directon;

            //shit from files
            using (StreamReader sr = new StreamReader("info/global/bullets/" + Type.ToString() + "/m.info"))
            {
                List<string> tmplist = sr.ReadToEnd().Split('\n').ToList();
                
                damage = Int32.Parse(tmplist[0]);

                Radius = double.Parse(tmplist[1]);

                speed = double.Parse(tmplist[2]);

                maxBeatFromWalls = Int32.Parse(tmplist[3]);
            }

            UpdateTexture(contentManager, true);
        }

        public Bullet(ContentManager contentManager, int type, double x, double y, double directon, Bullet sampleBullet)
        {
            beatFromWalls = 0;

            alive = true;

            Type = type;

            X = x;
            Y = y;

            degDirection = directon;

            damage = sampleBullet.damage;
            Radius = sampleBullet.Radius;
            speed = sampleBullet.speed;

            maxBeatFromWalls = sampleBullet.maxBeatFromWalls;

            UpdateTexture(contentManager, true);
        }

        private void UpdateTexture(ContentManager contentManager, bool reload)
        {
            if (reload)
            {
                texturesPhase = 0;

                Textures = new List<Texture2D>();

                while(File.Exists("Content/"+Type.ToString()+"bullet"+texturesPhase.ToString()+".xnb"))
                {
                    Textures.Add(contentManager.Load<Texture2D>(Type.ToString() + "bullet" + texturesPhase.ToString()));

                    texturesPhase++;
                }

                texturesPhase = 0;
            }
            else
            {
                texturesPhase++;

                texturesPhase %= Textures.Count;
            }
        }

        public override void Update(ContentManager contentManager, GameWorld gameWorld, int myIndex)
        {
            double px = X, py = Y;

            X += Math.Cos(degDirection) * speed;
            Y += Math.Sin(degDirection) * speed;

            if ((int)X != (int)px || (int)Y != (int)py)
            {
                if (X < 0 || Y < 0 || X >= gameWorld.blocks.Count || Y >= gameWorld.blocks[(int)X].Count || !gameWorld.blocks[(int)X][(int)Y].passable)
                {
                    beatFromWalls++;

                    if (Math.Abs(py - Y) <= Math.Abs(px - X))
                    {
                        degDirection = Math.PI - degDirection;
                    }
                    else
                    {
                        degDirection = Math.PI * 2 - degDirection;
                    }

                    degDirection %= Math.PI;

                    X = px;
                    Y = py;

                    if (beatFromWalls > maxBeatFromWalls)
                    {
                        alive = false;
                    }
                }
            }
            else
            {
                MapObject closestObject = gameWorld.GetClosestObject(X, Y, myIndex, "Ghost");

                if (closestObject != null)
                {
                    if (gameWorld.GetDist(X, Y, closestObject.X, closestObject.Y) < Radius + closestObject.Radius)
                    {
                        alive = false;
                        
                        closestObject.Attack(damage);
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, int x, int y)
        {
            spriteBatch.Draw(Textures[texturesPhase], new Vector2(x - Textures[texturesPhase].Width / 2, y - Textures[texturesPhase].Height), Color.White);
        }

        public override string GetTypeAsString()
        {
            return "Bullet";
        }
    }
}