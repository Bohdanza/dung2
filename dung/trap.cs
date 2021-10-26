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
    //IT'S A TRAP
    /*                      __...------------._
                         ,-'                   `-.
                      ,-'                         `.
                    ,'                            ,-`.
                   ;                              `-' `.
                  ;                                 .-. \
                 ;                           .-.    `-'  \
                ;                            `-'          \
               ;                                          `.
               ;                                           :
              ;                                            |
             ;                                             ;
            ;                            ___              ;
           ;                        ,-;-','.`.__          |
       _..;                      ,-' ;`,'.`,'.--`.        |
      ///;           ,-'   `. ,-'   ;` ;`,','_.--=:      /
     |'':          ,'        :     ;` ;,;,,-'_.-._`.   ,'
     '  :         ;_.-.      `.    :' ;;;'.ee.    \|  /
      \.'    _..-'/8o. `.     :    :! ' ':8888)   || /
       ||`-''    \\88o\ :     :    :! :  :`""'    ;;/
       ||         \"88o\;     `.    \ `. `.      ;,'
       /)   ___    `."'/(--.._ `.    `.`.  `-..-' ;--.
       \(.="""""==.. `'-'     `.|      `-`-..__.-' `. `.
        |          `"==.__      )                    )  ;
        |   ||           `"=== '                   .'  .'
        /\,,||||  | |           \                .'   .'
        | |||'|' |'|'           \|             .'   _.' \
        | |\' |  |           || ||           .'    .'    \
        ' | \ ' |'  .   ``-- `| ||         .'    .'       \
          '  |  ' |  .    ``-.._ |  ;    .'    .'          `.
       _.--,;`.       .  --  ...._,'   .'    .'              `.__
     ,'  ,';   `.     .   --..__..--'.'    .'                __/_\
   ,'   ; ;     |    .   --..__.._.'     .'                ,'     `.
  /    ; :     ;     .    -.. _.'     _.'                 /         `
 /     :  `-._ |    .    _.--'     _.'                   |
/       `.    `--....--''       _.'                      |
          `._ _..-'                         |
             `-..____...-''                              |
                                                         |
                               mGk                       |
        */

    public class Trap:MapObject
    {
        public override double X { get => base.X; protected set => base.X = value; }
        public override double Y { get => base.Y; protected set => base.Y = value; }
        public override int Type { get => base.Type; protected set => base.Type = value; }
        public override string Action { get => base.Action; protected set => base.Action = value; }
        public override bool alive { get => base.alive; protected set => base.alive = value; }
        public override double Radius { get => base.Radius; protected set => base.Radius = value; }
        public override List<Texture2D> Textures { get => base.Textures; protected set => base.Textures = value; }
        public int TexturePhase { get; protected set; }
        public int Damage { get; protected set; }

        public Trap(ContentManager contentManager, double x, double y, int type)
        {
            alive = true;

            Action = "id";

            Type = type;

            X = x;
            Y = y;

            using(StreamReader sr=new StreamReader("info/global/traps/"+Type.ToString()+"/m.info"))
            {
                List<string> tmplist = sr.ReadToEnd().Split('\n').ToList();

                Damage = Int32.Parse(tmplist[0]);

                Radius = double.Parse(tmplist[1]);
            }

            updateTexture(contentManager, true);
        }

        public Trap(ContentManager contentManager, double x, double y, Trap sample)
        {
            alive = true;

            Action = "id";

            Type = sample.Type;

            X = x;
            Y = y;

            Damage = sample.Damage;
            Radius = sample.Radius;

            updateTexture(contentManager, true);
        }

        private void updateTexture(ContentManager contentManager, bool reload)
        {
            if(reload)
            {
                Textures = new List<Texture2D>();

                TexturePhase = 0;

                while (File.Exists("Content/" + Type.ToString() + "trap_" + Action + "_" + TexturePhase.ToString() + ".xnb"))
                {
                    Textures.Add(contentManager.Load<Texture2D>(Type.ToString() + "trap_" + Action + "_" + TexturePhase.ToString()));

                    TexturePhase++;
                }

                TexturePhase = 0;
            }
            else
            {
                if (Action == "at" && TexturePhase == Textures.Count - 1)
                {
                    Action = "id";

                    alive = false;
                }

                TexturePhase++;

                TexturePhase %= Textures.Count;
            }
        }

        public override void Update(ContentManager contentManager, GameWorld gameWorld, int myIndex)
        {
            string pact = Action;

            double dist = gameWorld.GetDist(X, Y, gameWorld.referenceToHero.X, gameWorld.referenceToHero.Y);

            if (dist <= Radius + gameWorld.referenceToHero.Radius && Action != "at")
            {
                Action = "at";

                gameWorld.referenceToHero.Attack(Damage);
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

        public override void Draw(SpriteBatch spriteBatch, int x, int y)
        {
            spriteBatch.Draw(Textures[TexturePhase], new Vector2(x - Textures[TexturePhase].Width / 2, y - Textures[TexturePhase].Height), Color.White);
        }

        public override MapObject Clone(ContentManager contentManager)
        {
            return new Trap(contentManager, X, Y, this);
        }
    }
}