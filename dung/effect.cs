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
    public class Effect
    {
        public MapObject myObject { get; protected set; }
        public int Type { get; protected set; }
        public int Lifetime { get; protected set; }

        //string is the property name, first int - size, second int - probability
        public List<Tuple<string, int, int>> added { get; protected set; }
        public List<Particle> particles { get; protected set; }
        private int parcticleSummonProbability { get; set; }

        public Effect(ContentManager contentManager, int type, int lifetime, MapObject myObject)
        {
            Lifetime = lifetime;

            Type = type;

            this.myObject = myObject;

            using (StreamReader sr = new StreamReader("info/global/" + "effects/" + Type.ToString() + "/m.info"))
            {
                List<string> tmplist = sr.ReadToEnd().Split('\n').ToList();

                int tmpn = Int32.Parse(tmplist[0]), currentString = 0;

                for (currentString = 1; currentString <= tmpn * 3; currentString += 3)
                {
                    added.Add(new Tuple<string, int, int>(tmplist[currentString], Int32.Parse(tmplist[currentString + 1]), Int32.Parse(tmplist[currentString + 2])));
                }
            }
        }

        public void Update(ContentManager contentManager, GameWorld gameWorld)
        {
            
        }
    }
}