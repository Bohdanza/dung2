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
    public class Boss : MapObject
    {
        public override double X { get => base.X; protected set => base.X = value; }
        public override double Y { get => base.Y; protected set => base.Y = value; }
        public override int Type { get => base.Type; protected set => base.Type = value; }
        public override int HP { get => base.HP; protected set => base.HP = value; }
        public override List<Texture2D> Textures { get => base.Textures; protected set => base.Textures = value; }
        private int texturePhase, timeSinceLastUpdateTexture = 0;
        public override double Radius { get => base.Radius; protected set => base.Radius = value; }
        public override string Action { get => base.Action; protected set => base.Action = value; }

        public List<Gun> guns { get; protected set; }
        public int timeSinceLastAttack = 1000;
        private Texture2D HPtex;
        private string pact = "id";

        public Boss(ContentManager contentManager, int type, double x, double y)
        {
            Action = "id";

            X = x;
            Y = y;

            if(type==0)
            {
                guns = new List<Gun>();

                guns.Add(new Gun(contentManager, 14, 0, 0));
                HP = 150;

                Radius = 0.75;
            }

            updateTexture(contentManager, true);
        }

        private void updateTexture(ContentManager contentManager, bool reload)
        {
            if (reload)
            {
                texturePhase = 0;

                Textures = new List<Texture2D>();

                while (File.Exists("Content/" + Type.ToString() + "boss_" + Action + "_s" + texturePhase.ToString() + ".xnb"))
                {
                    Textures.Add(contentManager.Load<Texture2D>(Type.ToString() + "boss_" + Action + "_s" + texturePhase.ToString()));

                    texturePhase++;
                }

                texturePhase = 0;
            }
            else
            {
                texturePhase++;

                if ((Action == "at" || Action == "dm") && texturePhase == Textures.Count)
                {
                    Action = "id";

                    //1-level recursion
                    updateTexture(contentManager, true);
                }

                if (Action == "di" && texturePhase == Textures.Count)
                {
                    alive = false;
                }

                texturePhase %= Textures.Count;
            }
        }

        public override void Update(ContentManager contentManager, GameWorld gameWorld, int myIndex)
        {
            foreach (var currentGun in guns)
            {
                currentGun.Update(contentManager, gameWorld, -1);
            }

            if (Type == 0)
            {
                if(gameWorld.GetDist(X, Y, gameWorld.referenceToHero.X, gameWorld.referenceToHero.Y)<=8)
                {
                    double tmpdir = Math.Atan2(Y - gameWorld.referenceToHero.Y, X - gameWorld.referenceToHero.X);

                    tmpdir += 3f * (float)Math.PI;

                    tmpdir %= (float)(Math.PI * 2);
                    
                    var listFromStrings = new List<string>();
                    listFromStrings.Add("Hero");
                  
                    guns[0].ShootInDirection(gameWorld, contentManager, X, Y, tmpdir, Radius, listFromStrings);
                }
            }
            
            timeSinceLastUpdateTexture++;

            if (pact != Action)
            {
                timeSinceLastUpdateTexture = 0;

                updateTexture(contentManager, true);
            }
            else if (timeSinceLastUpdateTexture >= 12)
            {
                timeSinceLastUpdateTexture = 0;

                updateTexture(contentManager, false);
            }

            pact = Action;
        }

        public override void Draw(SpriteBatch spriteBatch, int x, int y, GameWorld gameWorld)
        {
            spriteBatch.Draw(Textures[texturePhase], new Vector2(x - Textures[texturePhase].Width / 2, y - Textures[texturePhase].Height), Color.White);
        }
        
        public override void Attack(int strenght)
        {
            HP -= strenght;

            if (strenght > 0)
            {
                Action = "dm";
            }

            if (HP <= 0)
            {
                HP = 0;

                Action = "di";
            }
        }

        public override string GetTypeAsString()
        {
            return "Boss";
        }

        public override MapObject Clone(ContentManager contentManager)
        {
            throw new NotImplementedException();
        }
    }
}