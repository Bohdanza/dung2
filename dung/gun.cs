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
    public class Gun : Item
    {
        public override double X { get; protected set; }
        public override double Y { get; protected set; }
        public override int Type { get; protected set; }
        public List<Tuple<Bullet, double>> bulletsShooting { get; protected set; }
        public List<int> FireSpeed { get; protected set; }
        public int currentFirePause { get; protected set; }
        public int TimeSinceLastShoot { get; protected set; }
        public int rarity { get; protected set; }

        /// <summary>
        /// With file reading
        /// </summary>
        /// <param name="contentManager"></param>
        /// <param name="type"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Gun(ContentManager contentManager, int type, double x, double y)
        {
            TimeSinceLastShoot = 0;

            Type = type;

            X = x;
            Y = y;

            bulletsShooting = new List<Tuple<Bullet, double>>();

            using(StreamReader sr = new StreamReader("info/global/items/guns/"+Type.ToString()+"/m.info"))
            {
                FireSpeed = new List<int>();

                List<string> tmplist = sr.ReadToEnd().Split('\n').ToList();

                int tmpn = Int32.Parse(tmplist[0])+1, currentString = 1;

                for(currentString=1; currentString<tmpn; currentString++)
                {
                    FireSpeed.Add(Int32.Parse(tmplist[currentString]));
                }
                
                currentString = tmpn+1;
                tmpn = Int32.Parse(tmplist[tmpn]) * 2 + tmpn;

                for (int i = currentString; i < tmpn; i += 2)
                {
                    bulletsShooting.Add(new Tuple<Bullet, double>(new Bullet(contentManager, Int32.Parse(tmplist[i]), 0, 0, 0), double.Parse(tmplist[i+1])));

                    currentString += 2;
                }

                rarity = Int32.Parse(tmplist[currentString]);
            }

            base.updateTexture(contentManager, true);

            base.RefreshName();
        }

        /// <summary>
        /// With sample
        /// </summary>
        /// <param name="contentManager"></param>
        /// <param name="type"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="sampleGun"></param>
        public Gun(ContentManager contentManager, int type, double x, double y, Gun sampleGun)
        {
            TimeSinceLastShoot = 0;

            Type = type;

            X = x;
            Y = y;

            FireSpeed = sampleGun.FireSpeed;

            bulletsShooting = sampleGun.bulletsShooting;

            rarity = sampleGun.rarity;

            Name = sampleGun.Name;

            base.updateTexture(contentManager, true);
        }

        public Gun(ContentManager contentManager, List<string> strList, int beginning, List<Gun> sampleGuns)
        {
            Type = Int32.Parse(strList[beginning]);

            X = Int32.Parse(strList[beginning + 1]);
            Y = Int32.Parse(strList[beginning + 2]);

            FireSpeed = sampleGuns[Type].FireSpeed;

            bulletsShooting = sampleGuns[Type].bulletsShooting;

            rarity = sampleGuns[Type].rarity;

            Name = sampleGuns[Type].Name;

            base.updateTexture(contentManager, true);
        }

        public override void Update(ContentManager contentManager, GameWorld gameWorld, int myIndex)
        {
            if (TimeSinceLastShoot < FireSpeed[currentFirePause])
            {
                TimeSinceLastShoot++;
            }

            base.Update(contentManager, gameWorld, myIndex);
        }

        public override void Draw(SpriteBatch spriteBatch, int x, int y, GameWorld gameWorld)
        {
            base.Draw(spriteBatch, x, y, gameWorld);
        }

        public void Draw(SpriteBatch spriteBatch, int x, int y, double rotation)
        {
            if ((rotation + Math.PI * 0.5) % (2 * Math.PI) < Math.PI)
            {
                spriteBatch.Draw(Textures[base.texturesPhase], new Vector2(x, y - Textures[base.texturesPhase].Height / 2), new Rectangle(0, 0, Textures[base.texturesPhase].Width, Textures[base.texturesPhase].Height), Color.White, (float)rotation, new Vector2(Textures[texturesPhase].Width / 2, Textures[texturesPhase].Height / 2), 1f, SpriteEffects.None, 0);
            }
            else
            {
                spriteBatch.Draw(Textures[base.texturesPhase],
                new Vector2(x, y - Textures[base.texturesPhase].Height / 2),
                new Rectangle(0, 0, Textures[base.texturesPhase].Width, Textures[base.texturesPhase].Height),
                Color.White, (float)(rotation + Math.PI),
                new Vector2(Textures[texturesPhase].Width / 2, Textures[texturesPhase].Height / 2), new Vector2(1f, 1f),
                SpriteEffects.FlipHorizontally, 0);
            }
        }

        public void ShootInDirection(GameWorld gameWorld, ContentManager contentManager, double x, double y, double direction, double radius)
        {
            if (TimeSinceLastShoot >= FireSpeed[currentFirePause])
            {
                TimeSinceLastShoot = 0;

                for (int i = 0; i < bulletsShooting.Count; i++)
                {
                    double tmpbx = x + Math.Cos(direction + bulletsShooting[i].Item2) * (radius + bulletsShooting[i].Item1.Radius);
                    double tmpby = y + Math.Sin(direction + bulletsShooting[i].Item2) * (radius + bulletsShooting[i].Item1.Radius);

                    gameWorld.AddObject(new Bullet(contentManager, bulletsShooting[i].Item1.Type, tmpbx, tmpby, direction+bulletsShooting[i].Item2, bulletsShooting[i].Item1));
                }

                currentFirePause++;

                currentFirePause %= FireSpeed.Count;
            }
        }
        
        public void ShootInDirection(GameWorld gameWorld, ContentManager contentManager, double x, double y, double direction, double radius, List<string> targets)
        {
            if (TimeSinceLastShoot >= FireSpeed[currentFirePause])
            {
                TimeSinceLastShoot = 0;

                for (int i = 0; i < bulletsShooting.Count; i++)
                {
                    double tmpbx = x + Math.Cos(direction + bulletsShooting[i].Item2) * (radius + bulletsShooting[i].Item1.Radius);
                    double tmpby = y + Math.Sin(direction + bulletsShooting[i].Item2) * (radius + bulletsShooting[i].Item1.Radius);

                    var reference = gameWorld.AddObject(new Bullet(contentManager, bulletsShooting[i].Item1.Type, tmpbx, tmpby, direction + bulletsShooting[i].Item2, bulletsShooting[i].Item1));

                    ((Bullet)reference).targets = targets;
                }

                currentFirePause++;

                currentFirePause %= FireSpeed.Count;
            }
        }


        public override string GetTypeAsString()
        {
            return "Gun";
        }

        public override List<string> SaveList()
        {
            List<string> tmplist = base.SaveList();

            return tmplist;
        }

        public override MapObject Clone(ContentManager contentManager)
        {
            return new Gun(contentManager, this.Type, this.X, this.Y, this);
        }
    }
}