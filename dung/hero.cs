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
    public class Hero : MapObject
    {
        public override double X { get; protected set; }
        public override double Y { get; protected set; }
        public override string Action { get; protected set; } = "id";
        public string Direction { get; protected set; } = "s";
        public override int Type { get; protected set; }
        public override List<Texture2D> Textures { get; protected set; }
        const double speed = 0.05;
        private int texturesPhase;
        public override double Radius { get; protected set; }
        public override int HP { get; protected set; }

        private double pickUpRadius=3;

        /// <summary>
        /// used to avoid hp texture type each time. Try to cut it out, then you'll understand
        /// </summary>
        private List<int> HpTextures;

        private Texture2D reloadTexture;

        private List<Texture2D> hpHeartTextures;
        private SpriteFont hpFont;
        public Gun GunInHand;
        public override List<Effect> Effects { get; protected set; }

        private int timeSinceLastAction = 0;
        public List<Coin> coins { get; protected set; }
        public int CoinsSum { get; set; } = 0;
        private Texture2D damageTexture;
        private int timeSinceLastDamage = 100, timeSinceLastUpdateTexture = 0;

        public Hero(ContentManager contentManager, double x, double y)
        {
            X = x;
            Y = y;

            Radius = 0.35;

            HP = 15;

            hpHeartTextures = new List<Texture2D>();            
            
            for (int i = 0; i < 5; i++)
            {
                hpHeartTextures.Add(contentManager.Load<Texture2D>(i.ToString() + "hpheart"));
            }

            hpFont = contentManager.Load<SpriteFont>("hpfont");

            reloadTexture = contentManager.Load<Texture2D>("reloadfull");

            damageTexture = contentManager.Load<Texture2D>("damage");

            GunInHand = new Gun(contentManager, 1, 0, 0);

            coins = new List<Coin>();

            Effects = new List<Effect>();

            //Effects.Add(new Effect(contentManager, 0, 1000, this));

            HpTextures = new List<int>();

            stabilizeHpList();

            UpdateTextures(contentManager, true);
        }

        public Hero(ContentManager contentManager, List<string> strList, int beginning, List<Gun> sampleGuns, List<Coin> sampleCoins)
        {
            Type = Int32.Parse(strList[beginning]);

            X = double.Parse(strList[beginning + 1]);
            Y = double.Parse(strList[beginning + 2]);

            HP = Int32.Parse(strList[beginning + 3]);

            GunInHand = new Gun(contentManager, strList, beginning + 4, sampleGuns);
            
            Radius = 0.35;

            hpHeartTextures = new List<Texture2D>();

            for (int i = 0; i < 5; i++)
            {
                hpHeartTextures.Add(contentManager.Load<Texture2D>("hpheart" + i.ToString()));
            }

            hpFont = contentManager.Load<SpriteFont>("hpfont");

            damageTexture = contentManager.Load<Texture2D>("damage");

            coins = new List<Coin>();

            HpTextures = new List<int>();

            stabilizeHpList();

            UpdateTextures(contentManager, true);
        }

        private void UpdateTextures(ContentManager contentManager, bool reload)
        {
            if (reload)
            {
                Textures = new List<Texture2D>();

                texturesPhase = 0;

                try
                {
                    while (true)
                    {
                        Textures.Add(contentManager.Load<Texture2D>(Type.ToString() + "hero_" + Action.ToString() + "_" + "s" + "_" + texturesPhase.ToString()));

                        texturesPhase++;
                    }
                }
                catch
                {

                }

                texturesPhase = 0;
            }
            else
            {
                texturesPhase++;

                texturesPhase %= Textures.Count;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, int x, int y, GameWorld gameWorld)
        {
            //dont touch ANYTHING
            //Can U hear me?
            //U don't want to see what would happen, trust me
            var mouseState = Mouse.GetState();

            double tmpdir = Math.Atan2(540 - (int)(Textures[texturesPhase].Height * 0.04) - mouseState.Y, 960 - mouseState.X);

            tmpdir += 3f * (float)Math.PI;

            tmpdir %= (float)(Math.PI * 2);

            /*if (Direction == "w")
            {
                GunInHand.Draw(spriteBatch, x, y - (int)(Textures[texturesPhase].Height * 0.04), tmpdir);
                spriteBatch.Draw(Textures[texturesPhase], new Vector2(x - Textures[texturesPhase].Width / 2, y - Textures[texturesPhase].Height), Color.White);
            }
            else
            {
                spriteBatch.Draw(Textures[texturesPhase], new Vector2(x - Textures[texturesPhase].Width / 2, y - Textures[texturesPhase].Height), Color.White);
                GunInHand.Draw(spriteBatch, x, y - (int)(Textures[texturesPhase].Height * 0.04), tmpdir);
            }*/
            
            if (GunInHand.TimeSinceLastShoot < GunInHand.FireSpeed[GunInHand.currentFirePause])
            {
                spriteBatch.Draw(reloadTexture, new Vector2(x - reloadTexture.Width / 2, (int)(y - Textures[texturesPhase].Height - reloadTexture.Height * 1.2)), Color.White);

                spriteBatch.Draw(reloadTexture,
                new Vector2(x - reloadTexture.Width / 2, (int)(y - Textures[texturesPhase].Height - reloadTexture.Height * 1.2)),
                new Rectangle(0, 0, (int)(reloadTexture.Width * (double)GunInHand.TimeSinceLastShoot / GunInHand.FireSpeed[GunInHand.currentFirePause]), reloadTexture.Height), Color.White);
            }

            if (Direction == "s")
            {
                spriteBatch.Draw(Textures[texturesPhase], new Vector2(x - Textures[texturesPhase].Width / 2, y - Textures[texturesPhase].Height), Color.White);
            }
            else
            {
                spriteBatch.Draw(Textures[texturesPhase], new Vector2(x - Textures[texturesPhase].Width / 2, y - Textures[texturesPhase].Height), new Rectangle(0, 0, Textures[texturesPhase].Width, Textures[texturesPhase].Height), Color.White, 0f, new Vector2(0, 0), 1f, SpriteEffects.FlipHorizontally, 0);
            }
        }

        public void DrawInterface(SpriteBatch spriteBatch)
        {
            int cx = 5;

            for (int i = 0; i < HpTextures.Count; i++)
            {
                spriteBatch.Draw(hpHeartTextures[HpTextures[i]], new Vector2(cx, 35), Color.White);

                cx += (int)(hpHeartTextures[HpTextures[i]].Width * 1.1);
            }

            spriteBatch.DrawString(hpFont, HP.ToString(), new Vector2(15, (int)(35 + hpHeartTextures[0].Height * 1.3)), Color.White);
            spriteBatch.DrawString(hpFont, CoinsSum.ToString(), new Vector2(1900 - hpFont.MeasureString(CoinsSum.ToString()).X, 15), Color.White);

            if (timeSinceLastDamage <= 6)
            {
                spriteBatch.Draw(damageTexture, new Vector2(0, 0), new Color(255, 255, 255, 255 - timeSinceLastDamage * 42));
            }
        }

        public override void Update(ContentManager contentManager, GameWorld gameWorld, int myIndex)
        {
            var rnd = new Random();

            if (timeSinceLastDamage <= 10)
            {
                timeSinceLastDamage++;
            }

            timeSinceLastAction++;

            double px = X;
            double py = Y;

            string pact = Action;
            string pdir = Direction;

            var keyboardState = Keyboard.GetState();

            //speed is going to change randomly
            double spdMin = 0.9;
            double spd = (spdMin + rnd.NextDouble() * (1-spdMin)) * speed;

            if (keyboardState.IsKeyDown(Keys.W))
            {
                Y -= spd;
            }

            if (keyboardState.IsKeyDown(Keys.A))
            {
                X -= spd;

                Direction = "s";
            }

            if (keyboardState.IsKeyDown(Keys.S))
            {
                Y += spd;
            }

            if (keyboardState.IsKeyDown(Keys.D))
            {
                X += spd;

                Direction = "w";
            }

            if (timeSinceLastAction >= 100 && keyboardState.IsKeyDown(Keys.Space))
            {
                timeSinceLastAction = 0;

                MapObject closestGun = gameWorld.GetClosestObject(X, Y, myIndex, "Gun");

                if (closestGun != null)
                {
                    if (gameWorld.GetDist(X, Y, closestGun.X, closestGun.Y) <= this.Radius + closestGun.Radius + 0.5)
                    {
                        GunInHand.ChangeCoords(closestGun.X, closestGun.Y);

                        gameWorld.AddObject(GunInHand);

                        GunInHand = (Gun)closestGun.Clone(contentManager);

                        closestGun.Kill();
                    }
                }
            }

            MapObject closestCoin = gameWorld.GetClosestObject(X, Y, myIndex, "Coin");

            if (closestCoin != null && closestCoin.GetTypeAsString() == "Coin")
            {
                if (gameWorld.GetDist(X, Y, closestCoin.X, closestCoin.Y) < pickUpRadius)
                {
                    double xTo = X - closestCoin.X;
                    double yTo = Y - closestCoin.Y;

                    float dir = (float)Math.Atan2(yTo, xTo);

                    closestCoin.Move(dir, speed * 3);

                    if (gameWorld.GetDist(X, Y, closestCoin.X, closestCoin.Y) < speed * 3)
                    {
                        coins.Add((Coin)closestCoin);

                        CoinsSum += ((Coin)closestCoin).value;

                        closestCoin.Kill();
                    }
                }
            }

            GunInHand.Update(contentManager, gameWorld, myIndex);

            var mouseState = Mouse.GetState();

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                double tmpdir = Math.Atan2(540 - (int)(Textures[texturesPhase].Height * 0.07) - mouseState.Y, 960 - mouseState.X);

                tmpdir += (float)Math.PI;

                tmpdir %= (float)(Math.PI * 2);

                GunInHand.ShootInDirection(gameWorld, contentManager, X, Y - ((double)Textures[texturesPhase].Height * 0.04 / GameWorld.blockDrawY), tmpdir, Radius * 1.2);
            }

            if ((int)X != (int)px || (int)Y != (int)py)
            {
                if (X < 0 || X >= gameWorld.blocks.Count || Y < 0 || Y >= gameWorld.blocks[(int)X].Count || !gameWorld.blocks[(int)X][(int)Y].passable)
                {
                    X = px;
                    Y = py;
                }
            }

            //USED TO FIX STICKING BUG
            if (X > px)
            {
                int xTex2 = (int)(X + Radius);

                if (xTex2 >= gameWorld.blocks.Count)
                {
                    xTex2 = gameWorld.blocks.Count - 1;
                }

                if (!gameWorld.blocks[xTex2][(int)Y].passable)
                {
                    X = px;
                }
            }

            if (X < px)
            {
                int xTex1 = (int)(X - Radius);

                if (xTex1 < 0)
                {
                    xTex1 = 0;
                }

                if (!gameWorld.blocks[xTex1][(int)Y].passable)
                {
                    X = px;
                }
            }

            base.UpdateEffects(contentManager, gameWorld);

            //000
            if (X != px || Y != py)
            {
                Action = "wa";
            }
            else
            {
                Action = "id";
            }

            timeSinceLastUpdateTexture++;

            if (Action != pact || Direction != pdir)
            {
                timeSinceLastUpdateTexture = 0;

                UpdateTextures(contentManager, true);
            }
            else if (timeSinceLastUpdateTexture >= 5)
            {
                timeSinceLastUpdateTexture = 0;

                UpdateTextures(contentManager, false);
            }
        }

        public override void Attack(int strenght, GameWorld gameWorld)
        {
            if (strenght > 0)
            {
                timeSinceLastDamage = 0;

                var rnd = new Random();

                double tmpx = 0.5 + rnd.NextDouble(), tmpy = 0.5 + rnd.NextDouble();

                if ((int)X == (int)(X + tmpx) && (int)Y == (int)(Y + tmpx))
                {
                    this.Move(tmpx, tmpy);
                }
            }

            HP -= strenght;

            if (HP <= 0)
            {
                HP = 0;

                alive = false;
            }

            stabilizeHpList();
        }

        private void stabilizeHpList()
        {
            int hptex = HP / 5;

            if (HP % 5 != 0)
            {
                hptex++;
            }

            while (HpTextures.Count > hptex)
            {
                HpTextures.RemoveAt(HpTextures.Count - 1);
            }

            var rnd = new Random();

            while (HpTextures.Count < hptex)
            {
                HpTextures.Add(rnd.Next(0, hpHeartTextures.Count));
            }
        }

        public override string GetTypeAsString()
        {
            return "Hero";
        }

        public override List<string> SaveList()
        {
            List<string> tmplist = base.SaveList();

            tmplist.Add(HP.ToString());

            List<string> tmpgunlist = GunInHand.SaveList();
                 
            foreach(var currentString in tmpgunlist)
            {
                tmplist.Add(currentString);
            }
            
            return tmplist;
        }

        /// <summary>
        /// DONT USE IT! HERO CANT BE CLONED!
        /// </summary>
        /// <param name="contentManager"></param>
        /// <returns></returns>
        public override MapObject Clone(ContentManager contentManager)
        {
            throw new Exception("Hero cant be cloned! You didnt read the description, did you?");
        }
    }
}