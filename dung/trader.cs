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
    public class Trader : Npc
    {
        public override double X { get => base.X; protected set => base.X = value; }
        public override double Y { get => base.Y; protected set => base.Y = value; }
        public override string Action { get; protected set; } = "id";
        public List<Tuple<int, Tuple<Item, int>>> GlobalItemsForChange { get; protected set; }
        public List<Tuple<int, Tuple<Item, int>>> ItemsForChange { get; protected set; }
        private int timeSinceLastShow { get; set; } = 0;
        public int currentOffer { get; protected set; } = 0;
        private SpriteFont hpFont;
        private int itemsCount { get; set; } = 0;

        public Trader(ContentManager contentManager, double x, double y, int type)
        {
            var rnd = new Random();

            hpFont = contentManager.Load<SpriteFont>("hpfont");

            GlobalItemsForChange = new List<Tuple<int, Tuple<Item, int>>>();
            ItemsForChange = new List<Tuple<int, Tuple<Item, int>>>();
         
            //given shit
            X = x;
            Y = y;

            Type = type;

            //reading shit from file
            using (StreamReader sr = new StreamReader("info/global/npc/traders/" + Type.ToString() + "/m.info"))
            {
                List<string> tmplist = sr.ReadToEnd().Split('\n').ToList();

                int n = Int32.Parse(tmplist[0]);

                int currentStr;

                for (currentStr = 1; currentStr <= n * 4; currentStr += 4)
                {
                    string classType1 = tmplist[currentStr];
                    int type1 = Int32.Parse(tmplist[currentStr + 1]);
                    
                    int count1 = Int32.Parse(tmplist[currentStr + 2]);
                    int count2 = Int32.Parse(tmplist[currentStr + 3]);

                    Item item1 = null;

                    if (classType1.Trim('\n').Trim('\r') == "Coin")
                    {
                        item1 = new Coin(contentManager, 0, 0, type1);
                    }
                    else if (classType1.Trim('\n').Trim('\r') == "Gun")
                    {
                        item1 = new Gun(contentManager, type1, 0, 0);
                    }
                    else if (classType1.Trim('\n').Trim('\r') == "Potion")
                    {
                        item1 = new Potion(contentManager, 0, 0, type1);
                    }

                    GlobalItemsForChange.Add(new Tuple<int, Tuple<Item, int>>(count2, new Tuple<Item, int>(item1, count1)));
                }

                currentStr = n * 4 + 1;

                int tmpn = Int32.Parse(tmplist[currentStr]);
                tmpn = Math.Min(tmpn, GlobalItemsForChange.Count);

                itemsCount = tmpn;

                List<Tuple<int, Tuple<Item, int>>> tmpGlobalItems = new List<Tuple<int, Tuple<Item, int>>>(GlobalItemsForChange);

                for (int i = 0; i < tmpn; i++)
                {
                    int q = rnd.Next(0, tmpGlobalItems.Count);

                    ItemsForChange.Add(tmpGlobalItems[q]);

                    tmpGlobalItems.RemoveAt(q);
                }
            }

            base.UpdateTexture(contentManager, true);
        }

        public Trader(ContentManager contentManager, double x, double y, Trader sample)
        {
            hpFont = sample.hpFont;

            //given shit
            X = x;
            Y = y;

            Type = sample.Type;

            ItemsForChange = new List<Tuple<int, Tuple<Item, int>>>();

            GlobalItemsForChange = sample.GlobalItemsForChange;

            var rnd = new Random();

            int tmpn = sample.itemsCount;

            List<Tuple<int, Tuple<Item, int>>> tmpGlobalItems = new List<Tuple<int, Tuple<Item, int>>>(GlobalItemsForChange);

            for (int i = 0; i < tmpn; i++)
            {
                int q = rnd.Next(0, tmpGlobalItems.Count);

                ItemsForChange.Add(tmpGlobalItems[q]);

                tmpGlobalItems.RemoveAt(q);
            }

            base.UpdateTexture(contentManager, true);
        }

        public override void Update(ContentManager contentManager, GameWorld gameWorld, int myIndex)
        {
            timeSinceLastShow++;

            var rnd = new Random();
            var ks = Keyboard.GetState();

            if (timeSinceLastShow >= 10)
            {
                timeSinceLastShow = 0;

                if (gameWorld.GetDist(gameWorld.referenceToHero.X, gameWorld.referenceToHero.Y, X, Y) <= this.Radius + gameWorld.referenceToHero.Radius + 1)
                {
                    if(ks.IsKeyDown(Keys.Space))
                    {
                        currentOffer++;

                        currentOffer %= ItemsForChange.Count;
                    }
                    else if(ks.IsKeyDown(Keys.Enter))
                    {
                        if (((Hero)gameWorld.referenceToHero).CoinsSum >= ItemsForChange[currentOffer].Item1)
                        {
                            ((Hero)gameWorld.referenceToHero).CoinsSum -= ItemsForChange[currentOffer].Item1;

                            var reference = gameWorld.AddObject(ItemsForChange[currentOffer].Item2.Item1.Clone(contentManager));

                            reference.ChangeCoords(X, Y + 1.5);
                        }
                    }
                }
            }
            
            UpdateTexture(contentManager, false);
        }

        public override void Draw(SpriteBatch spriteBatch, int x, int y)
        {
            base.Draw(spriteBatch, x, y);

            spriteBatch.DrawString(hpFont, ItemsForChange[currentOffer].Item1.ToString(), new Vector2(x - Textures[texturePhase].Width / 2, y - Textures[texturePhase].Height - 25), Color.White);

            if (ItemsForChange[currentOffer].Item2.Item2 > 1)
            {
                spriteBatch.DrawString(hpFont, ItemsForChange[currentOffer].Item2.ToString(), new Vector2(x + Textures[texturePhase].Width / 2 - ItemsForChange[currentOffer].Item2.Item1.Textures[0].Width * 0.55f, y - Textures[texturePhase].Height * 1.1f), Color.White);
            }

            ItemsForChange[currentOffer].Item2.Item1.Draw(spriteBatch, x + Textures[texturePhase].Width / 2, (int)(y - Textures[texturePhase].Height * 1.1 + ItemsForChange[currentOffer].Item2.Item1.Textures[0].Height * 0.5f));
        }

        public override MapObject Clone(ContentManager contentManager)
        {
            return new Trader(contentManager, X, Y, this);
        }
    }
}