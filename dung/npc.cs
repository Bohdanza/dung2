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
    public class Npc:MapObject
    {
        public override double X { get => base.X; protected set => base.X = value; }
        public override double Y { get => base.Y; protected set => base.Y = value; }
        public override bool alive { get => base.alive; protected set => base.alive = value; }
        public override List<Texture2D> Textures { get => base.Textures; protected set => base.Textures = value; }
        public List<string> phrases { get; protected set; }

        public Npc()
        {
            
        }

        public override void Update(ContentManager contentManager, GameWorld gameWorld, int myIndex)
        {
            base.Update(contentManager, gameWorld, myIndex);
        }

        public override void Draw(SpriteBatch spriteBatch, int x, int y)
        {
            
        }

        public override string GetTypeAsString()
        {
            return "Npc";
        }
    }
}