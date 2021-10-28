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
    public class Potion:Item
    {
        public override double X { get => base.X; protected set => base.X = value; }
        public override double Y { get => base.Y; protected set => base.Y = value; }
        public override int Type { get => base.Type; protected set => base.Type = value; }
        public override double Radius { get => base.Radius; protected set => base.Radius = value; }
        public int AddHp { get; protected set; }
        public override bool alive { get; protected set; } = true;

        public Potion(ContentManager contentManager, double x, double y, int type)
        {
            Type = type;

            X = x;
            Y = y;

            using (StreamReader sr = new StreamReader("info/global/items/potions/" + Type.ToString() + "/m.info"))
            {
                List<string> tmplist = sr.ReadToEnd().Split('\n').ToList();

                Radius = double.Parse(tmplist[0]);

                AddHp = Int32.Parse(tmplist[1]);
            }

            base.updateTexture(contentManager, true);

            base.RefreshName();
        }

        public Potion(ContentManager contentManager, double x, double y, Potion sample)
        {
            X = x;
            Y = y;

            Type = sample.Type;

            Radius = sample.Radius;
            AddHp = sample.AddHp;

            Name = sample.Name;

            base.updateTexture(contentManager, true);
        }

        public override void Update(ContentManager contentManager, GameWorld gameWorld, int myIndex)
        {
            if (gameWorld.GetDist(X, Y, gameWorld.referenceToHero.X, gameWorld.referenceToHero.Y) <= Radius + gameWorld.referenceToHero.Radius)
            {
                var ks = Keyboard.GetState();

                if(ks.IsKeyDown(Keys.Space))
                {
                    alive = false;

                    gameWorld.referenceToHero.Attack(-AddHp);
                }
            }

            base.updateTexture(contentManager, false);
        }

        public override void Draw(SpriteBatch spriteBatch, int x, int y)
        {
            spriteBatch.Draw(Textures[base.texturesPhase], new Vector2(x - Textures[base.texturesPhase].Width / 2, y - Textures[base.texturesPhase].Height), Color.White);
        }

        public override MapObject Clone(ContentManager contentManager)
        {
            return new Potion(contentManager, X, Y, this);
        }
    }
}