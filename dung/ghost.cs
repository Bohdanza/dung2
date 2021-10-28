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
        public Rectangle influenceRect = new Rectangle(0, 0, 0, 0);

        /// <summary>
        /// item is item, 2 ints are the min and the max number
        /// </summary>
        public List<Tuple<Item, int, int>> Loot { get; protected set; }

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

                //6=5+1
                int tmpn = Int32.Parse(tmplist[5]) * 4 + 6;
                int currentString;

                Loot = new List<Tuple<Item, int, int>>();

                for (currentString = 6; currentString < tmpn; currentString += 4)
                {
                    if (tmplist[currentString].Trim('\n').Trim('\r') == "coin")
                    {
                        Loot.Add(new Tuple<Item, int, int>(new Coin(contentManager, 0, 0, Int32.Parse(tmplist[currentString + 1])), Int32.Parse(tmplist[currentString + 2]), Int32.Parse(tmplist[currentString + 3])));
                    }
                }
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

            Loot = sampleGhost.Loot;

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

            Loot = samples[Type].Loot;

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
                if (influenceRect.Contains((float)gameWorld.referenceToHero.X, (float)gameWorld.referenceToHero.Y) && gameWorld.GetDist(X, Y, gameWorld.referenceToHero.X, gameWorld.referenceToHero.Y) <= viewRadius)
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

            //to check if it has already dropped
            if (Action == "di" && texturePhase == Textures.Count-1)
            {
                foreach(var currentItem in Loot)
                {
                    int c = rnd.Next(currentItem.Item2, currentItem.Item2);

                    if (currentItem.Item1.GetTypeAsString() == "Coin")
                    {
                        for (int i = 0; i < c; i++)
                        {
                            gameWorld.AddObject(new Coin(contentManager, this.X - 0.5 + rnd.NextDouble(), this.Y - 0.5 + rnd.NextDouble(), (Coin)currentItem.Item1));
                        }
                    }
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

        public override MapObject Clone(ContentManager contentManager)
        {
            return new Ghost(contentManager, Type, X, Y, WorkingX, WorkingY, this);
        }

        /// <summary>
        /// Fuck, i need to drink something harder than cola to write this. Anyway, me in future, please write it. Yes, you! And don't think that i'm talking about far future, i mean 28.10.2021
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        /// <param name="gameWorld"></param>
        public void FindPath(int x, int y, int dx, int dy, GameWorld gameWorld)
        {
            List<Tuple<int, Tuple<int, int>>> discovered = new List<Tuple<int, Tuple<int, int>>>();
            List<Tuple<int, Tuple<int, int>>> current = new List<Tuple<int, Tuple<int, int>>>();
            List<Tuple<int, Tuple<int, int>>> newPoints = new List<Tuple<int, Tuple<int, int>>>();

            current.Add(new Tuple<int, Tuple<int, int>>(0, new Tuple<int, int>(x, y)));

            bool finished = false;

            while (current.Count > 0 && !finished)
            {
                for (int i = 0; i < current.Count && !finished; i++)
                {
                    if (current[i].Item2.Item1 == dx && current[i].Item2.Item2 == dy)
                    {
                        finished = true;
                    }

                    if (current[i].Item2.Item1 > 0)
                    {
                        
                    }
                }
            }
        }
    }
}
