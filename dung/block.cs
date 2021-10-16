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
    public class Block
    {
        public virtual int x { get; private set; }
        public virtual int y { get; private set; }
        public virtual int type { get; private set; }
        public List<Texture2D> textures { get; private set; }
        private int texturePhase;
        public bool passable { get; private set; }

        /// <summary>
        /// With file reading
        /// </summary>
        /// <param name="type"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="contentManager"></param>
        public Block(int type, int x, int y, ContentManager contentManager)
        {
            this.type = type;

            this.x = x;
            this.y = y;

            using(StreamReader sr = new StreamReader("info/global/blocks/"+type.ToString()+"/main_info"))
            {
                List<string> stringlist = sr.ReadToEnd().Split('\n').ToList();

                passable = bool.Parse(stringlist[0]);
            }

            updateTexture(contentManager, true);
        }

        /// <summary>
        /// With sample
        /// </summary>
        /// <param name="type"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="contentManager"></param>
        /// <param name="sampleBlock"></param>
        public Block(int type, int x, int y, ContentManager contentManager, Block sampleBlock)
        {
            this.type = type;

            this.x = x;
            this.y = y;

            passable = sampleBlock.passable;

            updateTexture(contentManager, true);
        }

        public void updateTexture(ContentManager contentManager, bool reload)
        {
            if(reload)
            {
                textures = new List<Texture2D>();

                texturePhase = 0;

                //must be rewriten without file reading but i dont know how to do it
                while(File.Exists(@"Content/"+type.ToString()+"block"+texturePhase.ToString()+".xnb"))
                {
                    textures.Add(contentManager.Load<Texture2D>(type.ToString() + "block" + texturePhase.ToString()));

                    texturePhase++;
                }

                texturePhase = 0;
            }
            else
            {
                texturePhase++;

                texturePhase %= textures.Count;
            }
        }

        public virtual void update(ContentManager contentManager)
        {
            updateTexture(contentManager, false);
        }

        public virtual void draw(SpriteBatch spriteBatch, int x, int y)
        {
            spriteBatch.Draw(textures[texturePhase], new Vector2(x, y), Color.White);
        }
    }
}