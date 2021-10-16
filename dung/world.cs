using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Audio;

namespace dung
{
    public class GameWorld
    {
        public List<List<Block>> blocks;
        public const int blockDrawY = 64, BlockWidth = 64;
        private Texture2D darknessEffect, backgroundTexture;
        private List<MapObject> mapObjects;
        public MapObject referenceToHero { get; private set; }
        public List<Block> sampleBlocks { get; private set; } = new List<Block>();
        public List<Ghost> sampleGhosts { get; private set; } = new List<Ghost>();
        public List<Gun> sampleGuns { get; private set; } = new List<Gun>();
        private SoundEffect backgroundSong;
        private Texture2D cursor;

        /// <summary>
        /// new world
        /// </summary>
        /// <param name="contentManager"></param>
        public GameWorld(ContentManager contentManager)
        {
            var rnd = new Random();

            cursor = contentManager.Load<Texture2D>("cursor");

            backgroundSong = contentManager.Load<SoundEffect>("background_music0");

            darknessEffect = contentManager.Load<Texture2D>("darkness");
            backgroundTexture = contentManager.Load<Texture2D>("background1");

            mapObjects = new List<MapObject>();

            for (int i = 0; i < 4; i++)
            {
                sampleBlocks.Add(new Block(i, 0, 0, contentManager));
            }
            
            for (int i = 0; i < 2; i++)
            {
                sampleGhosts.Add(new Ghost(contentManager, i, 0, 0, 0, 0));
            }

            for (int i = 0; i < 8; i++)
            {
                sampleGuns.Add(new Gun(contentManager, i, 0, 0));
            }

            //generating main dungeon
            DungeonSynthesizer ds = new DungeonSynthesizer(contentManager, 480, 480);

            ds.AlternativeGenerate(30, 5, 12);
            ds.PlaceWalls();

            List<List<int>> tmplist = ds.GetList();

            blocks = new List<List<Block>>();

            for (int i = 0; i < tmplist.Count; i++)
            {
                List<Block> tmpblock = new List<Block>();

                for (int j = 0; j < tmplist[i].Count; j++)
                {
                    if (tmplist[i][j] == 1)
                    {
                        int vr = rnd.Next(0, 10);
                        
                        if(vr==0)
                        {
                            tmplist[i][j] = 3;
                        }
                    }

                    tmpblock.Add(new Block(tmplist[i][j], i, j, contentManager, sampleBlocks[tmplist[i][j]]));
                }

                blocks.Add(tmpblock);
            }

            //generating mobs, loot etc.
            List<int> specialRooms = new List<int>();

            while(specialRooms.Count<2)
            {
                int tmpi = rnd.Next(0, ds.rooms.Count);

                if (!specialRooms.Contains(tmpi) && ds.roomsRarity[tmpi] == 0)
                {
                    specialRooms.Add(tmpi);
                }
            }

            AddObject(new Hero(contentManager, ds.rooms[specialRooms[0]].Item1, ds.rooms[specialRooms[0]].Item2));

            referenceToHero = mapObjects[mapObjects.Count - 1];

            List<List<int>> fightingRooms = new List<List<int>>();

            for (int i = 0; i < ds.rooms.Count; i++)
            {
                if(!specialRooms.Contains(i))
                {
                    insertRoomObtaclesAt(contentManager, ds.rooms[i].Item1 - 7, ds.rooms[i].Item2 - 7, 12, 12, "", 7, 5, 12);

                    int roomDif = ds.roomsRarity[i]; 

                    if (roomDif == 0)
                    {
                        insertMobs(contentManager, ds.rooms[i].Item1, ds.rooms[i].Item2, 12, 12, rnd.Next(3, 5), 0);
                    }
                    else if (roomDif == 1)
                    {
                        insertMobs(contentManager, ds.rooms[i].Item1, ds.rooms[i].Item2, 12, 12, rnd.Next(5, 8), 0);
                        insertMobs(contentManager, ds.rooms[i].Item1, ds.rooms[i].Item2, 12, 12, rnd.Next(2, 5), 1);
                    }
                    else if(roomDif == 2)
                    {
                        insertMobs(contentManager, ds.rooms[i].Item1, ds.rooms[i].Item2, 12, 12, rnd.Next(8, 12), 0);
                        insertMobs(contentManager, ds.rooms[i].Item1, ds.rooms[i].Item2, 12, 12, rnd.Next(5, 8), 1);
                    }
                    else if(roomDif == 3)
                    {
                        insertMobs(contentManager, ds.rooms[i].Item1, ds.rooms[i].Item2, 12, 12, rnd.Next(11, 16), 0);
                        insertMobs(contentManager, ds.rooms[i].Item1, ds.rooms[i].Item2, 12, 12, rnd.Next(5, 10), 1);
                        //insertMobs(contentManager, ds.rooms[i].Item1, ds.rooms[i].Item2, 12, 12, rnd.Next(1, 3), 2);
                    }
                    else
                    {
                        roomDif = 4;

                        insertMobs(contentManager, ds.rooms[i].Item1, ds.rooms[i].Item2, 12, 12, rnd.Next(12, 21), 0);
                        insertMobs(contentManager, ds.rooms[i].Item1, ds.rooms[i].Item2, 12, 12, rnd.Next(7, 14), 1);
                        //insertMobs(contentManager, ds.rooms[i].Item1, ds.rooms[i].Item2, 12, 12, rnd.Next(2, 5), 2);
                    }

                    int tmptype = 0;

                    List<int> tmprar = new List<int>();

                    for (int tmprarc = 0; tmprarc < sampleGuns.Count; tmprarc++)
                    {
                        if (sampleGuns[tmprarc].rarity == roomDif)
                        {
                            tmprar.Add(tmprarc);
                        }
                    }

                    tmptype = rnd.Next(0, tmprar.Count);

                    if (tmptype < tmprar.Count && tmprar[tmptype] < sampleGuns.Count)
                    {
                        insertGun(contentManager, ds.rooms[i].Item1, ds.rooms[i].Item2, 17, 17, tmprar[tmptype]);
                    }
                }
            }

            SoundEffectInstance soundEffectInstance = backgroundSong.CreateInstance();

            soundEffectInstance.IsLooped = true;
            soundEffectInstance.Volume = 0.3f;

            soundEffectInstance.Play();
        }

        //TODO:
        /// <summary>
        /// DONT USE IT!!!!!!!
        /// </summary>
        /// <param name="contentManager"></param>
        /// <param name="path"></param>
        public GameWorld(ContentManager contentManager, string path)
        {
            cursor = contentManager.Load<Texture2D>("cursor");

            backgroundSong = contentManager.Load<SoundEffect>("background_music0");

            darknessEffect = contentManager.Load<Texture2D>("darkness");
            backgroundTexture = contentManager.Load<Texture2D>("background1");

            mapObjects = new List<MapObject>();

            for (int i = 0; i < 4; i++)
            {
                sampleBlocks.Add(new Block(i, 0, 0, contentManager));
            }

            for (int i = 0; i < 2; i++)
            {
                sampleGhosts.Add(new Ghost(contentManager, i, 0, 0, 0, 0));
            }

            for (int i = 0; i < 8; i++)
            {
                sampleGuns.Add(new Gun(contentManager, i, 0, 0));
            }

            List<string> read;
             
            using (StreamReader sr = new StreamReader(path))
            {
                read = sr.ReadToEnd().Split('\n').ToList();
            }

            blocks = new List<List<Block>>();

            //for blocks
            List<string> blockStrList = read[0].Split('♦').ToList();

            for (int i = 0; i < blockStrList.Count; i++)
            {
                List<string> realBlocks = blockStrList[i].Split(' ').ToList();

                List<Block> blocksList = new List<Block>();

                int currentYcoord = 0;

                for (int j = 0; j < realBlocks.Count - 1; j += 2)
                {
                    int tmpblocktype = Int32.Parse(realBlocks[j + 1]);

                    int tmpblockcount=Int32.Parse(realBlocks[j]);
                    
                    for (int k = 0; k < tmpblockcount; k++)
                    {
                        blocksList.Add(new Block(tmpblocktype, i, currentYcoord, contentManager, sampleBlocks[tmpblocktype]));

                        currentYcoord++;
                    }
                }

                blocks.Add(blocksList);
            }

            //for objects
            for (int i = 1; i < read.Count; i++)
            {
                if (read[i].Trim('\n').Trim('\r') == "Hero")
                {
                    referenceToHero = AddObject(new Hero(contentManager, read, i + 1, sampleGuns));
                }
                else if (read[i].Trim('\n').Trim('\r') == "Ghost")
                {
                    AddObject(new Ghost(contentManager, read, i + 1, sampleGhosts));
                }
            }

            SoundEffectInstance soundEffectInstance = backgroundSong.CreateInstance();

            soundEffectInstance.IsLooped = true;
            soundEffectInstance.Volume = 0.7f;

            soundEffectInstance.Play();
        }

        public void update(ContentManager contentManager)
        {
            mapObjects.Sort((a, b) => a.Y.CompareTo(b.Y));

            int l = 1;

            for (int i = 0; i < mapObjects.Count; i += l)
            {
                l = 1;

                if (GetDist(referenceToHero.X, referenceToHero.Y, mapObjects[i].X, mapObjects[i].Y) <= 35)
                {
                    mapObjects[i].Update(contentManager, this, i);
                }

                if(!mapObjects[i].alive)
                {
                    l = 0;
                    mapObjects.RemoveAt(i);
                }
                
            }
        }

        public void draw(SpriteBatch spriteBatch, int x, int y)
        {
            //spriteBatch.Draw(backgroundTexture, new Vector2(0, 0), Color.White);

            int tmpx = 0;
            int tmpy = 0;

            if(referenceToHero!=null)
            {
                tmpx = -(int)(referenceToHero.X * BlockWidth);
                tmpy = -(int)(referenceToHero.Y * blockDrawY);
            }

            int drawx = tmpx + x + 960;
            int drawy = tmpy + y + 540;

            int startx = drawx / blocks[0][0].textures[0].Width, endx = startx * -1 + 1920 / blocks[0][0].textures[0].Width, starty = drawy / blockDrawY, endy = starty * -1 + 1080 / blockDrawY + 1;

            endx++;

            startx *= -1;
            starty *= -1;

            startx = Math.Max(startx, 0);
            starty = Math.Max(starty, 0);

            endx = Math.Min(endx, blocks.Count);
            endy = Math.Min(endy, blocks[0].Count); 

            int mapObjectsJ = 0, l = 1;

            //main loop
            for (int j = starty; j < endy; j += l)
            {
                l = 1;

                if (mapObjectsJ<mapObjects.Count && mapObjects[mapObjectsJ].Y < j)
                {
                    l = 0;

                    if (GetDist(referenceToHero.X, referenceToHero.Y, mapObjects[mapObjectsJ].X, mapObjects[mapObjectsJ].Y) <= 40)
                    {
                        mapObjects[mapObjectsJ].Draw(spriteBatch, drawx + (int)(mapObjects[mapObjectsJ].X * BlockWidth), drawy + (int)(mapObjects[mapObjectsJ].Y * blockDrawY));
                    }

                    mapObjectsJ++;
                }
                else
                {
                    for (int i = startx; i < endx; i++)
                    {
                        if (blocks[i][j].type != 0)
                        {
                            blocks[i][j].draw(spriteBatch, drawx + i * BlockWidth, drawy + j * blockDrawY - blocks[i][j].textures[0].Height + blockDrawY);
                        }
                    }
                }
            }

            //effects
            spriteBatch.Draw(darknessEffect, new Vector2(0, 0), Color.White);

            var mouseState = Mouse.GetState();

            //drawing cursor
            spriteBatch.Draw(cursor, new Vector2(mouseState.X - cursor.Width / 2, mouseState.Y - cursor.Height / 2), Color.White);

            if (referenceToHero != null)
            {
                //hero hp, inventory & other
                ((Hero)referenceToHero).DrawInterface(spriteBatch);
            }
        }

        public double GetDist(double x, double y, double x1, double y1)
        {
            double a = Math.Abs(x - x1);
            double b = Math.Abs(y - y1);

            return Math.Sqrt(a * a + b * b);
        }

        public MapObject GetClosestObject(double x, double y, int indexToIgnore)
        {
            int mi = 0;
            double md = -1;

            for (int i = 0; i < mapObjects.Count; i++)
            {
                double tmpd = this.GetDist(x, y, mapObjects[i].X, mapObjects[i].Y);

                if (tmpd < md)
                {
                    mi = i;

                    md = tmpd;
                }
            }

            if(md>-1)
            {
                return mapObjects[mi];
            }

            return null;
        }

        /// <summary>
        /// Get closest object of given type
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="indexToIgnore"></param>
        /// <param name="typeAsString"></param>
        /// <returns></returns>
        public MapObject GetClosestObject(double x, double y, int indexToIgnore, string typeAsString)
        {
            int mi = 0;
            double md = double.MaxValue;

            for (int i = 0; i < mapObjects.Count; i++)
            {
                if (mapObjects[i].GetTypeAsString() == typeAsString)
                {
                    double tmpd = this.GetDist(x, y, mapObjects[i].X, mapObjects[i].Y);

                    if (tmpd < md)
                    {
                        mi = i;

                        md = tmpd;
                    }
                }
            }

            if (md > -1)
            {
                return mapObjects[mi];
            }

            return null;
        }
       
        /// <summary>
        /// Adds object to mapObjects list 
        /// </summary>
        /// <param name="mapObject"></param>
        /// <returns>reference to added object</returns>
        public MapObject AddObject(MapObject mapObject)
        {
            mapObjects.Add(mapObject);

            MapObject reference = mapObjects[mapObjects.Count - 1];

            mapObjects.Sort((a, b) => a.Y.CompareTo(b.Y));

            return reference;
        }

        private void insertRoomObtaclesAt(ContentManager contentManager, int x, int y, int xsize, int ysize, string roomType, int maxSize, int minObtacleNumber, int maxObtacleNumber)
        {
            var rnd = new Random();

            int obtaclesNumber = rnd.Next(minObtacleNumber, maxObtacleNumber);

            for (int k = 0; k < obtaclesNumber; k++)
            {
                int tmpx = rnd.Next(x + 2, x + xsize - 1);
                int tmpy = rnd.Next(y + 2, y + ysize - 1);

                if (tmpx >= 0 && tmpy >= 0 && tmpx < blocks.Count && tmpy < blocks[tmpx].Count)
                {
                    blocks[tmpx][tmpy] = new Block(2, tmpx, tmpy, contentManager);
                }

                int px1 = tmpx;
                int py1 = tmpy;

                tmpx = x + xsize - tmpx + x;

                if (tmpx >= 0 && tmpy >= 0 && tmpx < blocks.Count && tmpy < blocks[tmpx].Count)
                {
                    blocks[tmpx][tmpy] = new Block(2, tmpx, tmpy, contentManager);
                }

                tmpy = y + ysize - tmpy + y;

                if (tmpx >= 0 && tmpy >= 0 && tmpx < blocks.Count && tmpy < blocks[tmpx].Count)
                {
                    blocks[tmpx][tmpy] = new Block(2, tmpx, tmpy, contentManager);
                }

                tmpx = px1;

                if (tmpx >= 0 && tmpy >= 0 && tmpx < blocks.Count && tmpy < blocks[tmpx].Count)
                {
                    blocks[tmpx][tmpy] = new Block(2, tmpx, tmpy, contentManager);
                }
            }
        }
        
        private void insertMobs(ContentManager contentManager, int x, int y, int xsize, int ysize, int number, int type)
        {
            int c = 0;

            var rnd = new Random();

            while (c < number)
            {
                double tmpx = x + rnd.NextDouble() * xsize;
                double tmpy = y + rnd.NextDouble() * ysize;

                if ((int)tmpx >= 0 && (int)tmpy >= 0 && (int)tmpx < blocks.Count && (int)tmpy < blocks[(int)tmpx].Count && blocks[(int)tmpx][(int)tmpy].passable)
                {
                    AddObject(new Ghost(contentManager, type, tmpx, tmpy, tmpx, tmpy, sampleGhosts[type]));

                    c++;
                }
            }
        }

        private void insertGun(ContentManager contentManager, int x, int y, int xsize, int ysize, int type)
        {
            var rnd = new Random();
            bool placed = false;

            while (!placed)
            {
                double tmpx = x + rnd.NextDouble() * xsize;
                double tmpy = y + rnd.NextDouble() * ysize;

                if ((int)tmpx >= 0 && (int)tmpy >= 0 && (int)tmpx < blocks.Count && (int)tmpy < blocks[(int)tmpx].Count && blocks[(int)tmpx][(int)tmpy].passable)
                {
                    AddObject(new Gun(contentManager, type, tmpx, tmpy, sampleGuns[type]));

                    placed = true;
                }
            }
        }

        public void RemoveObjectAt(int index)
        {
            if (index < mapObjects.Count && index >= 0)
            {
                mapObjects.RemoveAt(index);
            }
        }

        public void RemoveObject(MapObject mapObject)
        {
            try
            {
                mapObjects.Remove(mapObject);
            }
            catch
            {
                //We just dont care
            }
        }

        public void Save(string path)
        {
            if(!File.Exists(path))
            {
                var z = File.Create(path);

                z.Close();
            }
            else
            {
                File.Delete(path);

                var z = File.Create(path);

                z.Close();
            }

            using (StreamWriter sw = new StreamWriter(new FileStream(path, FileMode.Append)))
            {
                for (int i = 0; i < blocks.Count; i++)
                {
                    int tmpc=1;

                    for (int j = 1; j < blocks[i].Count; j++)
                    {
                        if (blocks[i][j].type == blocks[i][j - 1].type)
                        {
                            tmpc++;
                        }
                        else
                        {
                            sw.Write(tmpc.ToString() + " ");
                            sw.Write(blocks[i][j - 1].type + " ");

                            tmpc = 1;
                        }
                    }

                    sw.Write(tmpc.ToString() + " ");
                    sw.Write(blocks[i][blocks[i].Count - 1].type + " ");

                    sw.Write("♦");
                }

                sw.WriteLine();

                foreach(var currentObject in mapObjects)
                {
                    List<string> tmplist = currentObject.SaveList();
                    
                    sw.WriteLine(currentObject.GetTypeAsString());

                    foreach(var currentString in tmplist)
                    {
                        sw.WriteLine(currentString);
                    }
                }
            }
        }
    }
}