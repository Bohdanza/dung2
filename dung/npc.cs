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
    public abstract class Npc:MapObject
    {
        public override int Type { get => base.Type; protected set => base.Type = value; }
        public override double X { get => base.X; protected set => base.X = value; }
        public override double Y { get => base.Y; protected set => base.Y = value; }
        public override bool alive { get => base.alive; protected set => base.alive = value; }
        public override List<Texture2D> Textures { get => base.Textures; protected set => base.Textures = value; }
        public override double Radius { get => base.Radius; protected set => base.Radius = value; }
        public override string Action { get => base.Action; protected set => base.Action = value; }
        protected int texturePhase { get; set; }

        protected virtual void UpdateTexture(ContentManager contentManager, bool reload)
        {
            if (reload)
            {
                Textures = new List<Texture2D>();

                texturePhase = 0;

                while (File.Exists("Content/" + Type.ToString() + "npc_" + Action + "_" + texturePhase.ToString() + ".xnb"))
                {
                    Textures.Add(contentManager.Load<Texture2D>(Type.ToString() + "npc_" + Action + "_" + texturePhase.ToString()));

                    texturePhase++;
                }
                
                texturePhase = 0;
            }
            else
            {
                texturePhase++;

                texturePhase %= Textures.Count;
            }
        }

        public override void Update(ContentManager contentManager, GameWorld gameWorld, int myIndex)
        { 
            base.Update(contentManager, gameWorld, myIndex);
        }

        public override void Draw(SpriteBatch spriteBatch, int x, int y, GameWorld gameWorld)
        {
            spriteBatch.Draw(Textures[texturePhase], new Vector2(x - Textures[texturePhase].Width / 2, y - Textures[texturePhase].Height), Color.White);
        }

        public override string GetTypeAsString()
        {
            return "Npc";
        }
    }
}