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
    public class Door:MapObject
    {
        public override double X { get => base.X; protected set => base.X = value; }
        public override double Y { get => base.Y; protected set => base.Y = value; }
        public override double Radius { get => base.Radius; protected set => base.Radius = value; }
        public override int Type { get => base.Type; protected set => base.Type = value; }
        public override string Action { get => base.Action; protected set => base.Action = value; }
        private Block closedBlock, openedBlock;
        public int TexturesPhase { get; protected set; }

        public Door(ContentManager contentManager, int type, int x, int y, GameWorld gameWorld)
        {
            Action = "clsd";

            X = x;
            Y = y;

            Type = type;

            closedBlock = new Block(5, (int)X, (int)Y, contentManager, gameWorld.sampleBlocks[5]);
            openedBlock = new Block(4, (int)X, (int)Y, contentManager, gameWorld.sampleBlocks[4]);

            updateTexture(contentManager, true);
        }

        private void updateTexture(ContentManager contentManager, bool reload)
        {
            if (reload)
            {
                Textures = new List<Texture2D>();

                TexturesPhase = 0;

                while (File.Exists("Content/" + Type.ToString() + "door_" + Action + "_" + TexturesPhase.ToString() + ".xnb"))
                {
                    Textures.Add(contentManager.Load<Texture2D>(Type.ToString() + "door_" + Action + "_" + TexturesPhase.ToString()));

                    TexturesPhase++;
                }

                TexturesPhase = 0;
            }
            else
            {
                TexturesPhase++;

                TexturesPhase %= Textures.Count;
            }
        }

        public override void Update(ContentManager contentManager, GameWorld gameWorld, int myIndex)
        {
            string pact = Action;

            if (TexturesPhase == Textures.Count - 1)
            {
                if (Action == "cls")
                {
                    Action = "clsd";
                }

                if (Action == "opn")
                {
                    Action = "opnd";
                }
            }

            double dst = gameWorld.GetDist(X, Y, gameWorld.referenceToHero.X, gameWorld.referenceToHero.Y);

            if (dst <= 2)
            {
                if (Action != "opnd" && Action != "opn")
                {
                    Action = "opn";

                    gameWorld.PlaceBlock(openedBlock, (int)X, (int)Y);
                }
            }
            else
            {
                if (Action != "clsd" && Action != "cls")
                {
                    Action = "cls";
                    
                    gameWorld.PlaceBlock(closedBlock, (int)X, (int)Y);
                }
            }

            if (pact == Action)
            {
                updateTexture(contentManager, false);
            }
            else
            {
                updateTexture(contentManager, true);
            }
        }

        public override void Draw(SpriteBatch spriteBatch, int x, int y, GameWorld gameWorld)
        {
            spriteBatch.Draw(Textures[TexturesPhase], new Vector2(x, y-Textures[TexturesPhase].Height+GameWorld.blockDrawY), Color.White);
        }

        public override string GetTypeAsString()
        {
            return "Door";
        }

        public override MapObject Clone(ContentManager contentManager)
        {
            throw new Exception("Door needs a GameWord object to be initialized, so U cant use this method. Try something like creating new door");
        }
    }
}