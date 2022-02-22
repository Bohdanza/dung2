using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Audio;

namespace dung
{
    public class Ghost : MapObject
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

        private int texturePhase, timeSinceLastAttack, attackSpeed, timeSinceLastUpdateTexture = 0, timeSinceLastDamage = 2;
        public override double Radius { get; protected set; }
        public override int HP { get; protected set; }
        protected double viewRadius { get; set; }
        public Rectangle influenceRect = new Rectangle(0, 0, 0, 0);
        private bool reject = false;

        /// <summary>
        /// item is item, 2 ints are the min and the max number
        /// </summary>
        public List<Tuple<Item, int, int>> Loot { get; protected set; }
        private List<Tuple<double, double>> path { get; set; } = new List<Tuple<double, double>>();
        private bool alreadyDropped = false;
        private List<SoundEffect> soundEffects;

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

            loadSounds(contentManager);
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

            loadSounds(contentManager);
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

            loadSounds(contentManager);
        }

        private void loadSounds(ContentManager contentManager)
        {
            soundEffects = new List<SoundEffect>();

            if (File.Exists("Content/" + Type.ToString() + "mob_id_sound.xnb"))
            {
                soundEffects.Add(contentManager.Load<SoundEffect>(Type.ToString() + "mob_id_sound"));
            }
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
            if (reject)
            {
                var prtrefer = gameWorld.AddObject(new Particle(contentManager, X, Y, 1, 10, 0));

                ((Particle)prtrefer).drawPlus = new Vector2(0, -(float)Textures[texturePhase].Height);
                ((Particle)prtrefer).drawMovement = new Vector2(0, -1.25f);
            }

            reject = false;

            string pdir = direction;

            timeSinceLastDamage++;
            timeSinceLastAttack++;

            double px = X;
            double py = Y;

            var rnd = new Random();

            int prc = 7;

            if (Action == "dm")
            {
                prc = 30;
            }

            if (Action == "di")
            {
                prc = 70;
            }

            if (rnd.Next(0, 100) <= prc)
            {
                var partref = gameWorld.AddObject(new Particle(contentManager, X + (rnd.NextDouble() - 0.5), Y + (rnd.NextDouble() - 0.5), 0, 120, 0));

                ((Particle)partref).drawMovement.Y = -1;
            }

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

                    gameWorld.referenceToHero.Attack(1, gameWorld);
                }
            }

            //to check if it has already dropped
            if (Action == "di" && texturePhase == Textures.Count - 1 && !alreadyDropped)
            {
                alreadyDropped = true;

                foreach (var currentItem in Loot)
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

            //playing sounds
            if ((rnd.Next(0, 10000) <= 0 || (Action == "at" && timeSinceLastDamage == 1)) && soundEffects.Count > 0)
            {
                double dist_to_hero = gameWorld.GetDist(X, Y, gameWorld.referenceToHero.X, gameWorld.referenceToHero.Y);

                if (dist_to_hero <= 25)
                {
                    var inst = soundEffects[0].CreateInstance();

                    inst.Volume = (float)(dist_to_hero / 25d) * gameWorld.soundsVolume;

                    inst.Play();
                }
            }

            if (degDirection > Math.PI)
            {
                direction = "w";
            }
            else
            {
                direction = "s";
            }

            timeSinceLastUpdateTexture++;

            if (pact != Action || pdir != direction)
            {
                timeSinceLastUpdateTexture = 0;

                updateTexture(contentManager, true);
            }
            else if(timeSinceLastUpdateTexture>=5)
            {
                timeSinceLastUpdateTexture = 0;

                updateTexture(contentManager, false);
            }

            pact = Action;
        }

        public override void Draw(SpriteBatch spriteBatch, int x, int y, GameWorld gameWorld)
        {
            spriteBatch.Draw(Textures[texturePhase], new Vector2(x - Textures[texturePhase].Width / 2, y - Textures[texturePhase].Height), Color.White);
        }

        public override void Attack(int strenght, GameWorld gameWorld)
        {
            if (this.influenceRect.Contains((float)gameWorld.referenceToHero.X, (float)gameWorld.referenceToHero.Y))
            {
                HP -= strenght;

                if (strenght > 0)
                {
                    timeSinceLastDamage = 0;
                    Action = "dm";
                }

                if (HP <= 0)
                {
                    timeSinceLastDamage = 0;

                    HP = 0;

                    Action = "di";
                }
            }
            else
            {
                reject = true;
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
        /// Now it's finished
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        /// <param name="gameWorld"></param>
        public void FindPath(int x, int y, int dx, int dy, GameWorld gameWorld)
        {
            x -= influenceRect.X;
            y -= influenceRect.Y;
            
            dx -= influenceRect.X;
            dy -= influenceRect.Y;

            List<List<int>> interpr = new List<List<int>>();

            for (int i = influenceRect.X; i < influenceRect.X + influenceRect.Width; i++)
            {
                List<int> tmplist = new List<int>();

                for (int j = influenceRect.Y; j < influenceRect.Y + influenceRect.Height; j++)
                {
                    if (gameWorld.blocks[i][j].passable)
                    {
                        tmplist.Add(-3);
                    }
                    else
                    {
                        tmplist.Add(-4);
                    }
                }

                interpr.Add(tmplist);
            }

            List<Tuple<int, int>> current, discovered;

            current = new List<Tuple<int, int>>();
            discovered = new List<Tuple<int, int>>();

            current.Add(new Tuple<int, int>(x, y));
            interpr[x][y] = 0;

            while (interpr[dx][dy] == -3 && current.Count > 0)
            {
                discovered = new List<Tuple<int, int>>();

                while(current.Count>0)
                {
                    int tmpx = current[0].Item1;
                    int tmpy = current[0].Item2;

                    current.RemoveAt(0);

                    if (tmpx > 0)
                    {
                        if (interpr[tmpx - 1][tmpy] != -4)
                        {
                            if (interpr[tmpx - 1][tmpy] == -3)
                            {
                                interpr[tmpx - 1][tmpy] = interpr[tmpx][tmpy] + 1;

                                discovered.Add(new Tuple<int, int>(tmpx - 1, tmpy));
                            }
                            else
                            {
                                interpr[tmpx - 1][tmpy] = Math.Min(interpr[tmpx][tmpy] + 1, interpr[tmpx - 1][tmpy]);
                            }
                        }
                    }
                    
                    if (tmpy > 0)
                    {
                        if (interpr[tmpx][tmpy-1] != -4)
                        {
                            if (interpr[tmpx][tmpy-1] == -3)
                            {
                                interpr[tmpx][tmpy - 1] = interpr[tmpx][tmpy] + 1;

                                discovered.Add(new Tuple<int, int>(tmpx, tmpy-1));
                            }
                            else
                            {
                                interpr[tmpx][tmpy - 1] = Math.Min(interpr[tmpx][tmpy] + 1, interpr[tmpx][tmpy - 1]);
                            }
                        }
                    }

                    if (tmpx < interpr.Count - 1)
                    {
                        if (interpr[tmpx + 1][tmpy] != -4)
                        {
                            if (interpr[tmpx + 1][tmpy] == -3)
                            {
                                interpr[tmpx + 1][tmpy] = interpr[tmpx][tmpy] + 1;

                                discovered.Add(new Tuple<int, int>(tmpx + 1, tmpy));
                            }
                            else
                            {
                                interpr[tmpx + 1][tmpy] = Math.Min(interpr[tmpx][tmpy] + 1, interpr[tmpx + 1][tmpy]);
                            }
                        }
                    }

                    if (tmpy < interpr[tmpx].Count - 1)
                    {
                        if (interpr[tmpx][tmpy + 1] != -4)
                        {
                            if (interpr[tmpx][tmpy + 1] == -3)
                            {
                                interpr[tmpx][tmpy + 1] = interpr[tmpx][tmpy] + 1;

                                discovered.Add(new Tuple<int, int>(tmpx, tmpy + 1));
                            }
                            else
                            {
                                interpr[tmpx][tmpy + 1] = Math.Min(interpr[tmpx][tmpy] + 1, interpr[tmpx][tmpy + 1]);
                            }
                        }
                    }
                }

                current = discovered;
            }

            path = new List<Tuple<double, double>>();

            int cx = dx, cy = dy;

            while (cx != x || cy != y)
            {
                path.Add(new Tuple<double, double>(cx + influenceRect.X, cy + influenceRect.Y));

                if (cx > 0 && interpr[cx - 1][cy] == interpr[cx][cy] - 1)
                {
                    cx--;
                }
                else if (cy > 0 && interpr[cx][cy - 1] == interpr[cx][cy] - 1)
                {
                    cy--;
                }
                else if (cx < interpr.Count - 1 && interpr[cx + 1][cy] == interpr[cx][cy] - 1)
                {
                    cx++;
                }
                else if (cy < interpr[cx].Count - 1 && interpr[cx][cy + 1] == interpr[cx][cy] - 1)
                {
                    cy++;
                }
            }

            path.Reverse();
        }
    }
}
