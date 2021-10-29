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
    public class Coin:Item
    {
        public override double X { get => base.X; protected set => base.X = value; }
        public override double Y { get => base.Y; protected set => base.Y = value; }
        public override int Type { get => base.Type; protected set => base.Type = value; }
        public int value { get; protected set; }
        
        public Coin(ContentManager contentManager, double x, double y, int type)
        {
            Type = type;

            this.X = x;
            this.Y = y;

            using (StreamReader sr = new StreamReader("info/global/items/coins/" + Type.ToString() + "/m.info"))
            {
                List<string> tmplist = sr.ReadToEnd().Split('\n').ToList();

                value = Int32.Parse(tmplist[0]);
            }

            base.updateTexture(contentManager, true);
        }

        public Coin(ContentManager contentManager, double x, double y, Coin sample)
        {
            Type = sample.Type;

            this.X = x;
            this.Y = y;

            value = sample.value;

            base.updateTexture(contentManager, true);
        }

        public override void Update(ContentManager contentManager, GameWorld gameWorld, int myIndex)
        {
            base.Update(contentManager, gameWorld, myIndex);
        }

        public override void Draw(SpriteBatch spriteBatch, int x, int y, GameWorld gameWorld)
        {
            base.Draw(spriteBatch, x, y, gameWorld);
        }

        public override string GetTypeAsString()
        {
            return "Coin";
        }

        public override List<string> SaveList()
        {
            List<string> tmplist = new List<string>();

            tmplist.Add("Coin");

            tmplist.Add(X.ToString());
            tmplist.Add(Y.ToString());

            tmplist.Add(Type.ToString());

            return tmplist;
        }

        public override MapObject Clone(ContentManager contentManager)
        {
            return new Coin(contentManager, X, Y, this);
        }
    }
}