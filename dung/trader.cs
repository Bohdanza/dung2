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
        public List<Tuple<int, Tuple<Item, int>>> ItemsForChange { get; protected set; }
        private int timeSinceLastShow { get; set; } = 0;
        public int currentOffer { get; protected set; } = 0;
        private SpriteFont hpFont;

        public Trader(ContentManager contentManager, double x, double y, int type)
        {
            hpFont = contentManager.Load<SpriteFont>("hpfont");

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

                    if (classType1 == "Coin")
                    {
                        item1 = new Coin(contentManager, 0, 0, type1);
                    }
                    else if (classType1 == "Gun")
                    {
                        item1 = new Gun(contentManager, type1, 0, 0);
                    }

                    ItemsForChange.Add(new Tuple<int, Tuple<Item, int>>(count2, new Tuple<Item, int>(item1, count1)));
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

            ItemsForChange = sample.ItemsForChange;

            base.UpdateTexture(contentManager, true);
        }

        public override void Update(ContentManager contentManager, GameWorld gameWorld, int myIndex)
        {
            timeSinceLastShow++;

            var ks = Keyboard.GetState();

            if (timeSinceLastShow >= 40)
            {
                if (gameWorld.GetDist(gameWorld.referenceToHero.X, gameWorld.referenceToHero.Y, X, Y) <= this.Radius + gameWorld.referenceToHero.Radius + 1)
                {
                    if(ks.IsKeyDown(Keys.Space))
                    {
                        currentOffer++;
                    }
                }
            }
            
            UpdateTexture(contentManager, false);
        }

        public override void Draw(SpriteBatch spriteBatch, int x, int y)
        {
            base.Draw(spriteBatch, x, y);

            spriteBatch.DrawString(hpFont, currentOffer.ToString(), new Vector2(x - Textures[texturePhase].Width / 2, y - Textures[texturePhase].Height * 1.1f), Color.White);
        }
    }
}