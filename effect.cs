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
        public int Type { get; protected; }

        //string is the property name, first int - size, second int - probability
        public List<Tuple<string, int, int>> added { get; protected set; }

        public Effect()
        {
            
        }
    }
}