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
    public abstract class Item : MapObject
    {
        public override double X { get; protected set; }
        public override double Y { get; protected set; }
        public override int Type { get; protected set; }
        public override double Radius { get; protected set; }
        public override List<Texture2D> Textures { get; protected set; }
        protected int texturesPhase;
        public virtual int weight { get; protected set; }

        protected virtual void updateTexture(ContentManager contentManager, bool reload)
        {
            if (reload)
            {
                texturesPhase = 0;

                Textures = new List<Texture2D>();

                while (File.Exists("Content/" + Type.ToString() + "item" + texturesPhase.ToString() + ".xnb"))
                {
                    Textures.Add(contentManager.Load<Texture2D>(Type.ToString() + "item" + texturesPhase.ToString()));

                    texturesPhase++;
                }

                texturesPhase = 0;
            }
            else
            {
                texturesPhase++;

                texturesPhase %= Textures.Count;
            }
        }

        public override void Update(ContentManager contentManager, GameWorld gameWorld, int myIndex)
        {
            updateTexture(contentManager, false);

            base.Update(contentManager, gameWorld, myIndex);
        }

        public override void Draw(SpriteBatch spriteBatch, int x, int y)
        {
            spriteBatch.Draw(Textures[texturesPhase], new Vector2(x - Textures[texturesPhase].Width / 2, y - Textures[texturesPhase].Height), Color.White);

            base.Draw(spriteBatch, x, y);
        }

        public override string GetTypeAsString()
        {
            return "Item";
        }
    }
}