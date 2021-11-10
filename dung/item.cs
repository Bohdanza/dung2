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
        public virtual string Name { get; protected set; } = "";
        public virtual SpriteFont Font { get; protected set; } = null;

        protected virtual void updateTexture(ContentManager contentManager, bool reload)
        {
            if (reload)
            {
                if (Font == null)
                {
                    Font = contentManager.Load<SpriteFont>("item_font");
                }

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

        public override void Draw(SpriteBatch spriteBatch, int x, int y, GameWorld gameWorld)
        {
            spriteBatch.Draw(Textures[texturesPhase], new Vector2(x - Textures[texturesPhase].Width / 2, y - Textures[texturesPhase].Height), Color.White);

            if (gameWorld.GetDist(X, Y, gameWorld.referenceToHero.X, gameWorld.referenceToHero.Y) <= Radius + gameWorld.referenceToHero.Radius + 2)
            {
                spriteBatch.DrawString(Font, Name, new Vector2(x - Textures[texturesPhase].Width / 2 - Font.MeasureString(Name).X / 2, y - Textures[texturesPhase].Height - Font.MeasureString(Name).Y * 1.1f), Color.White);
            }

            base.Draw(spriteBatch, x, y, gameWorld);
        }

        public override string GetTypeAsString()
        {
            return "Item";
        }

        /// <summary>
        /// I know that looks like shit, but i'm too lazy to write one more method for almost EACH class
        /// </summary>
        public void RefreshName()
        {
            using (StreamReader sr = new StreamReader("info/global/items/names"))
            {
                var tmplist = sr.ReadToEnd().Split('\n').ToList();

                Name = tmplist[Type];
            }
        }
    }
}