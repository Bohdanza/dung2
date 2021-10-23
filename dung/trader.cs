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
        public List<Tuple<Item, Item>> ItemsForChange { get; protected set; }

        public Trader(ContentManager contentManager, double x, double y, int type)
        {
            //given shit
            X = x;
            Y = y;

            Type = type;

            //reading shit from file
            using (StreamReader sr = new StreamReader("info/global/npc/traders/" + Type.ToString() + "/m.info"))
            {
                List<string> tmplist = sr.ReadToEnd().Split('\n').ToList();

                int n = Int32.Parse(tmplist[0]);
            }

            base.UpdateTexture(contentManager, true);
        }

        public Trader(ContentManager contentManager, double x, double y, Trader sample)
        {
            //given shit
            X = x;
            Y = y;

            Type = sample.Type;

            base.UpdateTexture(contentManager, true);
        }

        public override void Update(ContentManager contentManager, GameWorld gameWorld, int myIndex)
        {
            UpdateTexture(contentManager, false);
        }

        public override void Draw(SpriteBatch spriteBatch, int x, int y)
        {
            base.Draw(spriteBatch, x, y);
        }
    }
}