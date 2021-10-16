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
    public class Ghost:MapObject
    {
        public override double X { get; protected set; }
        public override double Y { get; protected set; }
        public double WorkingX { get; protected set; }
        public double WorkingY { get; protected set; }
        public override List<Texture2D> Textures { get; protected set; }
        public override int Type { get; protected set; }
        public override string Action { get; protected set; }
        private string direction;
        private double degDirection, speed;
        private string pact = "id";

        private int texturePhase, timeSinceLastAttack, attackSpeed;
        public override double Radius { get; protected set; }
        public override int HP { get; protected set; }
        protected double viewRadius { get; set; }

        /// <summary>
        /// with file reading, hp to max
        /// </summary>
        /// <param name="contentManager"></param>
        /// <param name="type"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Ghost(ContentManager contentManager, int type, double x, double y, double workingX, double workingY)
        {
            //standart shit
            Action = "id";

            direction = "w";

            degDirection = 0;

            timeSinceLastAttack = 0;

            //given shit
            X = x;
            Y = y;

            WorkingX = workingX;
            WorkingY = workingY;

            Type = type;
            
            //shit in files
            using (StreamReader sr = new StreamReader("info/global/mobs/" + Type.ToString() + "/m.info"))
            {
                List<string> tmplist = sr.ReadToEnd().Split('\n').ToList();

                speed = double.Parse(tmplist[0].Trim('\r'));

                HP = Int32.Parse(tmplist[1].Trim('\r'));

                Radius = double.Parse(tmplist[2].Trim('\r'));

                attackSpeed = Int32.Parse(tmplist[3].Trim('\r'));

                viewRadius = double.Parse(tmplist[4].Trim('\r'));
            }
            
            updateTexture(contentManager, true);
        }

        /// <summary>
        /// with sample including hp
        /// </summary>
        /// <param name="contentManager"></param>
        /// <param name="type"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="sampleGhost"></param>
        public Ghost(ContentManager contentManager, int type, double x, double y, double workingX, double workingY, Ghost sampleGhost)
        {
            //standart shit
            Action = "id";

            direction = "w";

            degDirection = 0;

            timeSinceLastAttack = 0;

            //given shit
            X = x;
            Y = y;

            WorkingX = workingX;
            WorkingY = workingY;

            Type = type;

            speed = sampleGhost.speed;

            HP = sampleGhost.HP;

            Radius = sampleGhost.Radius;

            attackSpeed = sampleGhost.attackSpeed;

            viewRadius = sampleGhost.viewRadius;

            updateTexture(contentManager, true);
        }

        public Ghost(ContentManager contentManager, List<string> strList, int beginning, List<Ghost> samples)
        {
            //standart shit
            Action = "id";

            direction = "w";

            degDirection = 0;

            timeSinceLastAttack = 0;

            //given shit
            Type = Int32.Parse(strList[beginning]);

            X = double.Parse(strList[beginning + 1]);
            Y = double.Parse(strList[beginning + 2]);

            HP = Int32.Parse(strList[beginning + 3]);

            WorkingX = double.Parse(strList[beginning + 4]);
            WorkingY = double.Parse(strList[beginning + 5]);

            speed = samples[Type].speed;

            Radius = samples[Type].Radius;

            attackSpeed = samples[Type].attackSpeed;

            viewRadius = samples[Type].viewRadius;

            updateTexture(contentManager, true);
        }

        private void updateTexture(ContentManager contentManager, bool reload)
        {
            if (reload)
            {
                Textures = new List<Texture2D>();

                texturePhase = 0;

                while (File.Exists("Content/" + Type.ToString() + "mob_" + Action.ToString() + "_" + direction.ToString() + texturePhase.ToString() + ".xnb"))
                {
                    Textures.Add(contentManager.Load<Texture2D>(Type.ToString() + "mob_" + Action.ToString() + "_" + direction.ToString() + texturePhase.ToString()));

                    texturePhase++;
                }

                texturePhase = 0;
            }
            else
            {
                texturePhase++;

                if ((Action == "at" || Action == "dm") && texturePhase == Textures.Count)
                {
                    Action = "id";

                    //1-level recursion
                    updateTexture(contentManager, true);
                }

                if (Action == "di" && texturePhase == Textures.Count)
                {
                    alive = false;
                }

                texturePhase %= Textures.Count;
            }
        }

        public override void Update(ContentManager contentManager, GameWorld gameWorld, int myIndex)
        {
            string pdir = direction;

            timeSinceLastAttack++;

            double px = X;
            double py = Y;

            var rnd = new Random();

            if (Action != "at" && Action != "di" && Action != "dm")
            {
                if (gameWorld.GetDist(X, Y, gameWorld.referenceToHero.X, gameWorld.referenceToHero.Y) <= viewRadius)
                {
                    double x1 = X - gameWorld.referenceToHero.X, y1 = Y - gameWorld.referenceToHero.Y;

                    degDirection = Math.Atan2(y1, x1);

                    degDirection += (float)Math.PI;

                    degDirection %= (float)(Math.PI * 2);

                    Action = "wa";
                }
                else if (gameWorld.GetDist(X, Y, WorkingX, WorkingY) >= speed)
                {
                    double x1 = X - WorkingX, y1 = Y - WorkingY;

                    degDirection = Math.Atan2(y1, x1);

                    degDirection += (float)Math.PI;

                    degDirection %= (float)(Math.PI * 2);

                    Action = "wa";
                }
                else
                {
                    Action = "id";
                }
            }

            if (Action == "wa")
            {
                X += Math.Cos(degDirection) * speed;

                if (X < 0)
                {
                    X = 0;
                }

                if (X >= gameWorld.blocks.Count)
                {
                    X = gameWorld.blocks.Count - 1;
                }

                if (!gameWorld.blocks[(int)Math.Floor(X)][(int)Math.Floor(Y)].passable)
                {
                    X = px;
                }

                Y += Math.Sin(degDirection) * speed;

                if (Y < 0)
                {
                    Y = 0;
                }

                if (Y >= gameWorld.blocks[(int)Math.Floor(X)].Count)
                {
                    Y = gameWorld.blocks[(int)Math.Floor(X)].Count - 1;
                }

                if (!gameWorld.blocks[(int)Math.Floor(X)][(int)Math.Floor(Y)].passable)
                {
                    Y = py;
                }
            }

            double tmpdist = gameWorld.GetDist(X, Y, gameWorld.referenceToHero.X, gameWorld.referenceToHero.Y);

            if (Action != "di" && Action != "dm")
            {
                if (timeSinceLastAttack >= attackSpeed && tmpdist <= Radius + gameWorld.referenceToHero.Radius)
                {
                    Action = "at";

                    timeSinceLastAttack = 0;

                    gameWorld.referenceToHero.Attack(1);
                }
            }

            if (degDirection * 57.2957795 >= 0 && degDirection * 57.2957795 <= 180)
            {
                direction = "s";
            }
            else
            {
                direction = "w";
            }

            if (pact == Action && pdir == direction)
            {
                updateTexture(contentManager, false);
            }
            else
            {
                updateTexture(contentManager, true);
            }

            pact = Action;
        }

        public override void Draw(SpriteBatch spriteBatch, int x, int y)
        {
            spriteBatch.Draw(Textures[texturePhase], new Vector2(x - Textures[texturePhase].Width / 2, y - Textures[texturePhase].Height), Color.White);
        }

        public override void Attack(int strenght)
        {
            HP -= strenght;

            if (strenght > 0)
            {
                Action = "dm";
            }

            if (HP <= 0)
            {
                HP = 0;

                Action = "di";
            }
        }

        public override string GetTypeAsString()
        {
            return "Ghost";
        }

        public override List<string> SaveList()
        {
            List<string> tmplist = base.SaveList();

            tmplist.Add(HP.ToString());

            tmplist.Add(WorkingX.ToString());
            tmplist.Add(WorkingY.ToString());

            return tmplist;
        }
    }
}
