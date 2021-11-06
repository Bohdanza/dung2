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
    public class Particle:MapObject
    {
        public override double X { get => base.X; protected set => base.X = value; }
        public override double Y { get => base.Y; protected set => base.Y = value; }
        public override bool alive { get => base.alive; protected set => base.alive = value; }
        public override List<Texture2D> Textures { get => base.Textures; protected set => base.Textures = value; }
        private int texturePhase, lifetime, timeSinceBorn;

        public Particle(ContentManager contentManager, double X, double Y)
        {

        }

        public override MapObject Clone(ContentManager contentManager)
        {
            throw new NotImplementedException();
        }
    }
}