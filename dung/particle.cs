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
        private int TexturePhase, lifetime, timeSinceBorn;
        public override int Type { get => base.Type; protected set => base.Type = value; }
        public double degDirection { get; protected set; }
        public Vector2 drawMovement;
        public Vector2 drawPlus;

        public Particle(ContentManager contentManager, double x, double y, int type, int lifetime, double degDirection)
        {
            drawPlus = new Vector2(0, 0);
            drawMovement = new Vector2(0, 0);

            this.degDirection = degDirection;

            Type = type;

            X = x;
            Y = y;

            alive = true;

            this.lifetime = lifetime;

            timeSinceBorn = 0;

            updateTexture(contentManager, true);
        }

        private void updateTexture(ContentManager contentManager, bool reload)
        {
            if (reload)
            {
                Textures = new List<Texture2D>();

                TexturePhase = 0;

                while (File.Exists("Content/" + Type.ToString() + "particle" + TexturePhase.ToString() + ".xnb"))
                {
                    Textures.Add(contentManager.Load<Texture2D>(Type.ToString() + "particle" + TexturePhase.ToString()));

                    TexturePhase++;
                }

                TexturePhase = 0;
            }
            else
            {
                TexturePhase++;

                TexturePhase %= Textures.Count;
            }
        }

        public override void Update(ContentManager contentManager, GameWorld gameWorld, int myIndex)
        {
            updateTexture(contentManager, false);

            drawPlus = new Vector2(drawPlus.X + drawMovement.X, drawPlus.Y + drawMovement.Y);

            timeSinceBorn++;

            if(timeSinceBorn>lifetime)
            {
                alive = false;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, int x, int y, GameWorld gameWorld)
        {
            spriteBatch.Draw(Textures[TexturePhase], new Vector2(x - Textures[TexturePhase].Width / 2 + drawPlus.X, y - Textures[TexturePhase].Height / 2 + drawPlus.Y), Color.White);

            //spriteBatch.Draw(Textures[TexturePhase], new Vector2(x - Textures[TexturePhase].Width / 2, y - Textures[TexturePhase].Height / 2), new Rectangle(0, 0, Textures[TexturePhase].Width, Textures[TexturePhase].Height), Color.White, (float)degDirection, new Vector2(0, 0), 1f, SpriteEffects.None, 0);
        }

        public override MapObject Clone(ContentManager contentManager)
        {
            return new Particle(contentManager, X, Y, Type, lifetime, degDirection);
        }
    }
}