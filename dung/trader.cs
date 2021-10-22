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
        public override string Action { get => base.Action; protected set => base.Action = value; }
        public override List<string> phrases { get => base.phrases; protected set => base.phrases = value; }
        public List<Tuple<Item, Item>> ItemsForChange { get; protected set; }

        public Trader(ContentManager contentManager, double x, double y, int type)
        {
            
        }

        public Trader(ContentManager contentManager, double x, double y, Trader sample)
        {
            
        }
    }
}