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
    public class Turret:MapObject
    {
        public override double X { get => base.X; protected set => base.X = value; }
        public override double Y { get => base.Y; protected set => base.Y = value; }
        public override int Type { get => base.Type; protected set => base.Type = value; }
        public override List<Texture2D> Textures { get => base.Textures; protected set => base.Textures = value; }
        private int texturePhase;
        public override double Radius { get => base.Radius; protected set => base.Radius = value; }
        public Gun gun { get; protected set; }

        public Turret(ContentManager contentManager, double x, double y, int type)
        {
            X = x;
            Y = y;

            Type = type;

            using (StreamReader sr = new StreamReader("info/global/turrets/" + Type.ToString() + "/m.info"))
            {
                List<string> tmplist = sr.ReadToEnd().Split('\n').ToList();

                gun = new Gun(contentManager, Int32.Parse(tmplist[0]), x, y);

                Radius = double.Parse(tmplist[1]);
            }

            updateTexture(contentManager, true);
        }

        public Turret(ContentManager contentManager, double x, double y, Turret sample)
        {
            Type = sample.Type;

            X = x;
            Y = y;

            gun = (Gun)sample.gun.Clone(contentManager);

            gun.ChangeCoords(X, Y);

            Radius = sample.Radius;

            updateTexture(contentManager, true);
        }

        private void updateTexture(ContentManager contentManager, bool reload)
        {
            if (reload)
            {
                texturePhase = 0;
                Textures = new List<Texture2D>();

                while (File.Exists("Content/" + Type.ToString() + "turret" + texturePhase.ToString() + ".xnb"))
                {
                    Textures.Add(contentManager.Load<Texture2D>(Type.ToString() + "turret" + texturePhase.ToString()));

                    texturePhase++;
                }
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

            updateTexture(contentManager, false);

            double tmpdir = Math.Atan2(Y - gameWorld.referenceToHero.Y, X - gameWorld.referenceToHero.X);

            tmpdir += 3f * (float)Math.PI;

            tmpdir %= (float)(Math.PI * 2);

            gun.ShootInDirection(gameWorld, contentManager, X, Y, tmpdir, Radius);

            gun.Update(contentManager, gameWorld, -1);
        }

        public override void Draw(SpriteBatch spriteBatch, int x, int y, GameWorld gameWorld)
        {
            spriteBatch.Draw(Textures[texturePhase], new Vector2(x - Textures[texturePhase].Width / 2, y - Textures[texturePhase].Height), Color.White);

            double tmpdir = Math.Atan2(Y - gameWorld.referenceToHero.Y, X - gameWorld.referenceToHero.X);

            tmpdir += 3f * (float)Math.PI;

            tmpdir %= (float)(Math.PI * 2);

            gun.Draw(spriteBatch, x, y - Textures[texturePhase].Height, tmpdir);
        }

        public override MapObject Clone(ContentManager contentManager)
        {
            return new Turret(contentManager, X, Y, this);
        }
    }
}
