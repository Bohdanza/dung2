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
        public int timeSinceCreation { get; protected set; }

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

                added = new List<Tuple<string, int, int>>();
                particles = new List<Particle>();

                int tmpn = Int32.Parse(tmplist[0]), currentString = 0;

                for (currentString = 1; currentString <= tmpn * 3; currentString += 3)
                {
                    added.Add(new Tuple<string, int, int>(tmplist[currentString], Int32.Parse(tmplist[currentString + 1]), Int32.Parse(tmplist[currentString + 2])));
                }

                tmpn = Int32.Parse(tmplist[currentString]) * 2 + currentString + 1;
                parcticleSummonProbability = Int32.Parse(tmplist[currentString]);

                for (currentString+=2; currentString <= tmpn; currentString += 2)
                {
                    particles.Add(new Particle(contentManager, 0, 0, Int32.Parse(tmplist[currentString]), 0, 0));
                }
            }
        }

        public void Update(ContentManager contentManager, GameWorld gameWorld)
        {
            var rnd = new Random();
            
            foreach(var currentParticle in particles)
            {
                int tmp = rnd.Next(0, 100);

                if (tmp <= parcticleSummonProbability)
                {
                    var refer = gameWorld.AddObject(new Particle(contentManager, myObject.X - 0.5 + rnd.NextDouble(), myObject.Y - 0.5 + rnd.NextDouble(), currentParticle.Type, 100, 0));
                    
                    ((Particle)refer).drawMovement = new Vector2(0, -1);
                }
            }
        }
    }
}