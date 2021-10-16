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
    public abstract class MapObject
    {
        public virtual double X { get; protected set; }
        public virtual double Y { get; protected set; }

        public virtual int Type { get; protected set; }

        public virtual string Action { get; protected set; }
        public virtual List<Texture2D> Textures { get; protected set; }
        protected virtual bool isInfected { get; set; } = false;
        public virtual double Radius { get; protected set; }
        public virtual int HP { get; protected set; }
        public virtual bool alive { get; protected set; } = true;

        public virtual void Update(ContentManager contentManager, GameWorld gameWorld, int myIndex)
        {
            
        }

        public virtual void Draw(SpriteBatch spriteBatch, int x, int y)
        {

        }

        public virtual void Infect()
        {

        }

        public virtual void Attack(int strenght)
        {

        }

        public virtual string GetTypeAsString()
        {
            return "";
        }

        public virtual void ChangeCoords(double x, double y)
        {
            X = x;
            Y = y;
        }
        
        public virtual List<string> SaveList()
        {
            List<string> tmplist = new List<string>();

            tmplist.Add(Type.ToString());

            tmplist.Add(X.ToString());
            tmplist.Add(X.ToString());

            return tmplist;
        }
    }
}