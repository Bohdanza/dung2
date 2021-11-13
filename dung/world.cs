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
        public bool settingsOpened { get; private set; } = false;
        public bool Active { get; private set; } = true;
        private button exitButton, settingsButton;
        private Slider musicSlider, soundsSlider;
        public List<List<Block>> blocks;
        public const int blockDrawY = 36, BlockWidth = 36;
        private Texture2D darknessEffect, backgroundTexture, settingsDarkness;
        private List<MapObject> mapObjects;
        public MapObject referenceToHero { get; private set; }
        public List<Block> sampleBlocks { get; private set; } = new List<Block>();
        public List<Ghost> sampleGhosts { get; private set; } = new List<Ghost>();
        public List<Gun> sampleGuns { get; private set; } = new List<Gun>();
        public List<Coin> sampleCoins { get; private set; } = new List<Coin>();
        public List<Trader> sampleTraders { get; private set; } = new List<Trader>();
        public List<Trap> sampleTraps { get; private set; } = new List<Trap>();
        public List<Potion> samplePotions { get; private set; } = new List<Potion>();
        public List<Turret> sampleTurrets { get; private set; } = new List<Turret>();

        private SoundEffect backgroundSong;
        private Texture2D cursor, YouDiedTexture;
        private SoundEffectInstance soundEffectInstance1;
        public float soundsVolume = 0.3f;

        /// <summary>
        /// new world
        /// </summary>
        /// <param name="contentManager"></param>
        public GameWorld(ContentManager contentManager)
        {
            var rnd = new Random();

            cursor = contentManager.Load<Texture2D>("cursor");
            
            YouDiedTexture = contentManager.Load<Texture2D>("you_died_screen");

            backgroundSong = contentManager.Load<SoundEffect>("background_music0");

            darknessEffect = contentManager.Load<Texture2D>("darkness");
            backgroundTexture = contentManager.Load<Texture2D>("background1");
            settingsDarkness = contentManager.Load<Texture2D>("settings_darkness");

            var tmptex1 = contentManager.Load<Texture2D>("exitworldbuttonnormal");

            exitButton = new button(0, 960-tmptex1.Width/2, 450, tmptex1.Width, tmptex1.Height, tmptex1, contentManager.Load<Texture2D>("exitworldbuttonpressed"), contentManager.Load<SpriteFont>("button_font"), "", Color.White);

            tmptex1 = contentManager.Load<Texture2D>("settings_button_normal");

            settingsButton = new button(0, 10, 1070 - tmptex1.Height, tmptex1.Width, tmptex1.Height, tmptex1, contentManager.Load<Texture2D>("settings_button_pressed"));

            tmptex1 = contentManager.Load<Texture2D>("normal_slider");

            musicSlider = new Slider(960 - 500, 310, 1000, tmptex1.Height, 300, 0, tmptex1);
            musicSlider.LockY = true;
            musicSlider.LockX = false;

            soundsSlider = new Slider(960 - 500, 400, 1000, tmptex1.Height, 300, 0, tmptex1);
            soundsSlider.LockY = true;
            soundsSlider.LockX = false;

            mapObjects = new List<MapObject>();

            for (int i = 0; i < 8; i++)
            {
                sampleBlocks.Add(new Block(i, 0, 0, contentManager));
            }

            for (int i = 0; i < 4; i++)
            {
                sampleGhosts.Add(new Ghost(contentManager, i, 0, 0, 0, 0));
            }

            for (int i = 0; i < 15; i++)
            {
                if (i != 8 && i != 10)
                {
                    sampleGuns.Add(new Gun(contentManager, i, 0, 0));
                }
            }

            sampleCoins.Add(new Coin(contentManager, 0, 0, 8));

            for (int i = 0; i < 1; i++)
            {
                sampleTraders.Add(new Trader(contentManager, 0, 0, i));
            }

            for (int i = 0; i < 1; i++)
            {
                sampleTraps.Add(new Trap(contentManager, 0, 0, i));
            }

            samplePotions.Add(new Potion(contentManager, 0, 0, 10));
            samplePotions.Add(new Potion(contentManager, 0, 0, 15));

            for (int i = 0; i < 1; i++)
            {
                sampleTurrets.Add(new Turret(contentManager, 0, 0, i));
            }

            //generating main dungeon
            dung.DungeonSynthesizer ds = new dung.DungeonSynthesizer(contentManager, 480, 480);
            
            ds.SnakeGenerate(17, 16);
            ds.ReplaceRooms(17, 17);
            ds.PlaceWalls();
            ds.PlaceDoors();

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

                        if (vr == 0)
                        {
                            tmplist[i][j] = 3;
                        }
                    }

                    if (tmplist[i][j] == 2)
                    {
                        int vr = rnd.Next(0, 10);

                        if (vr <= 4)
                        {
                            tmplist[i][j] = 6;
                        }
                    }
                    
                    if (tmplist[i][j] == 0)
                    {
                        int vr = rnd.Next(0, 10);

                        if (vr <= 4)
                        {
                            vr = rnd.Next(0, 10);

                            if (vr <= 4)
                            {
                                tmplist[i][j] = 6;
                            }
                            else
                            {
                                tmplist[i][j] = 2;
                            }
                        }
                    }

                    if (tmplist[i][j]==5)
                    {
                        AddObject(new Door(contentManager, 0, i, j, this));
                    }

                    tmpblock.Add(new Block(tmplist[i][j], i, j, contentManager, sampleBlocks[tmplist[i][j]]));
                }

                blocks.Add(tmpblock);
            }
            
            //generating mobs, loot etc.
            referenceToHero = AddObject(new Hero(contentManager, ds.rooms[0].Item1, ds.rooms[0].Item2));

          //  AddObject(new Boss(contentManager, 0, ds.rooms[0].Item1, ds.rooms[0].Item2));
            // AddObject(new Turret(contentManager, ds.rooms[0].Item1, ds.rooms[0].Item2 + 4, 0));

            List<List<int>> fightingRooms = new List<List<int>>();

            for (int i = 1; i < ds.rooms.Count; i++)
            {
                if (ds.roomsRarity[i] >= 0)
                {
                    insertRoomObtaclesAt(contentManager, ds.rooms[i].Item1 - 8, ds.rooms[i].Item2 - 8, 16, 16, "", 7, 1, 3);

                    int roomDif = ds.roomsRarity[i];
                    int trapscount = roomDif;

                    if (roomDif == 0)
                    {
                        insertMobs(contentManager, ds.rooms[i].Item1 - 9, ds.rooms[i].Item2 - 9, 17, 17, rnd.Next(3, 5), 0);

                        // insertMobs(contentManager, ds.rooms[i].Item1 - 7, ds.rooms[i].Item2 - 7, 13, 13, 1, 2);
                        // insertObject(contentManager, ds.rooms[i].Item1 - 7, ds.rooms[i].Item2 - 7, 13, 13, new Turret(contentManager, 0, 0, sampleTurrets[0]));
                    }
                    else if (roomDif == 1)
                    {
                        insertMobs(contentManager, ds.rooms[i].Item1 - 9, ds.rooms[i].Item2 - 9, 17, 17, rnd.Next(4, 7), 0);

                        if (rnd.Next(0, 2) == 0)
                        {
                            insertMobs(contentManager, ds.rooms[i].Item1 - 9, ds.rooms[i].Item2 - 9, 17, 17, rnd.Next(1, 3), 1);
                        }
                        else
                        {
                            insertMobs(contentManager, ds.rooms[i].Item1 - 9, ds.rooms[i].Item2 - 9, 17, 17, rnd.Next(1, 3), 2);
                        }

                        //insertObject(contentManager, ds.rooms[i].Item1 - 7, ds.rooms[i].Item2 - 7, 13, 13, new Turret(contentManager, 0, 0, sampleTurrets[0]));
                    }
                    else if (roomDif == 2)
                    {
                        if (rnd.Next(0, 100) <= 75)
                        {
                            insertMobs(contentManager, ds.rooms[i].Item1 - 9, ds.rooms[i].Item2 - 9, 17, 17, rnd.Next(5, 8), 0);
                            insertMobs(contentManager, ds.rooms[i].Item1 - 9, ds.rooms[i].Item2 - 9, 17, 17, rnd.Next(2, 4), 1);
                            insertMobs(contentManager, ds.rooms[i].Item1 - 9, ds.rooms[i].Item2 - 9, 17, 17, rnd.Next(2, 4), 2);
                        }
                        else
                        {
                            insertMobs(contentManager, ds.rooms[i].Item1 - 9, ds.rooms[i].Item2 - 9, 17, 17, rnd.Next(2, 3), 0);
                            insertMobs(contentManager, ds.rooms[i].Item1 - 9, ds.rooms[i].Item2 - 9, 17, 17, 1, 3);
                        }
                        // insertObject(contentManager, ds.rooms[i].Item1 - 7, ds.rooms[i].Item2 - 7, 13, 13, new Turret(contentManager, 0, 0, sampleTurrets[0]));
                    }
                    else if (roomDif == 3)
                    {
                        insertMobs(contentManager, ds.rooms[i].Item1 - 9, ds.rooms[i].Item2 - 9, 17, 17, rnd.Next(11, 16), 0);
                        insertMobs(contentManager, ds.rooms[i].Item1 - 9, ds.rooms[i].Item2 - 9, 17, 17, rnd.Next(5, 10), 1);
                        insertMobs(contentManager, ds.rooms[i].Item1 - 9, ds.rooms[i].Item2 - 9, 17, 17, rnd.Next(3, 6), 2);

                        // insertObject(contentManager, ds.rooms[i].Item1 - 7, ds.rooms[i].Item2 - 7, 13, 13, new Turret(contentManager, 0, 0, sampleTurrets[0]));
                        //  insertObject(contentManager, ds.rooms[i].Item1 - 7, ds.rooms[i].Item2 - 7, 13, 13, new Turret(contentManager, 0, 0, sampleTurrets[0]));
                    }
                    else
                    {
                        roomDif = 4;

                        insertMobs(contentManager, ds.rooms[i].Item1 - 9, ds.rooms[i].Item2 - 9, 17, 17, rnd.Next(12, 21), 0);
                        insertMobs(contentManager, ds.rooms[i].Item1 - 9, ds.rooms[i].Item2 - 9, 17, 17, rnd.Next(7, 14), 1);
                        insertMobs(contentManager, ds.rooms[i].Item1 - 9, ds.rooms[i].Item2 - 9, 17, 17, rnd.Next(4, 7), 2);

                        //    insertObject(contentManager, ds.rooms[i].Item1 - 7, ds.rooms[i].Item2 - 7, 13, 13, new Turret(contentManager, 0, 0, sampleTurrets[0]));
                        //insertObject(contentManager, ds.rooms[i].Item1 - 7, ds.rooms[i].Item2 - 7, 17, 17, new Turret(contentManager, 0, 0, sampleTurrets[0]));
                    }

                    int tmptype = 0;

                    List<Gun> tmprar = new List<Gun>();

                    for (int tmprarc = 0; tmprarc < sampleGuns.Count; tmprarc++)
                    { 
                        if (sampleGuns[tmprarc].rarity == roomDif)
                        {
                            tmprar.Add(sampleGuns[tmprarc]);
                        }
                    }

                    tmptype = rnd.Next(0, tmprar.Count);

                    if (tmptype < tmprar.Count)
                    {
                        insertGun(contentManager, ds.rooms[i].Item1 - 6, ds.rooms[i].Item2 - 6, 13, 13, tmprar[tmptype]);
                    }

                    for (int tmpTraps = 0; tmpTraps <= trapscount; tmpTraps++)
                    {
                        insertObject(contentManager, ds.rooms[i].Item1 - 6, ds.rooms[i].Item2 - 6, 12, 12, new Trap(contentManager, 0, 0, sampleTraps[0]));
                    }
                }
                else if (ds.roomsRarity[i] == -1)
                {
                    AddObject(new Trader(contentManager, ds.rooms[i].Item1, ds.rooms[i].Item2, sampleTraders[0]));
                }
                else
                {
                    AddObject(new Boss(contentManager, 0, ds.rooms[i].Item1, ds.rooms[i].Item2));
                }
            }

            soundEffectInstance1 = backgroundSong.CreateInstance();

            soundEffectInstance1.IsLooped = true;
            soundEffectInstance1.Volume = 0.3f;

            soundEffectInstance1.Play();
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

                    int tmpblockcount = Int32.Parse(realBlocks[j]);

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
                    referenceToHero = AddObject(new Hero(contentManager, read, i + 1, sampleGuns, sampleCoins));
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
            if (settingsOpened)
            {
                settingsButton.update();

                if (settingsButton.pressed)
                {
                    settingsOpened = false;
                }

                exitButton.update();

                if (exitButton.pressed)
                {
                    this.Active = false;

                    soundEffectInstance1.Stop();
                }
                
                musicSlider.Update();

                this.soundEffectInstance1.Volume = (float)musicSlider.SliderX / musicSlider.Width;

                soundsSlider.Update();

                this.soundsVolume = (float)soundsSlider.SliderX / soundsSlider.Width;
            }
            else
            {
                settingsButton.update();

                if (settingsButton.pressed)
                {
                    settingsOpened = true;
                }
            }

            if (!settingsOpened)
            {
                int l = 1;

                for (int i = 0; i < mapObjects.Count; i += l)
                {
                    l = 1;

                    if (GetDist(referenceToHero.X, referenceToHero.Y, mapObjects[i].X, mapObjects[i].Y) <= 35)
                    {
                        mapObjects[i].Update(contentManager, this, i);
                    }

                    if (!mapObjects[i].alive)
                    {
                        l = 0;
                        mapObjects.RemoveAt(i);
                    }
                }

                mapObjects.Sort((a, b) => a.Y.CompareTo(b.Y));

                //  exitButton.update();
            }
        }

        public void draw(SpriteBatch spriteBatch, int x, int y)
        {
            spriteBatch.Draw(backgroundTexture, new Vector2(0, 0), Color.White);

            int tmpx = 0;
            int tmpy = 0;

            if (referenceToHero != null)
            {
                tmpx = -(int)(referenceToHero.X * BlockWidth);
                tmpy = -(int)(referenceToHero.Y * blockDrawY);
            }

            int drawx = tmpx + x + 960;
            int drawy = tmpy + y + 540;

            int startx = drawx / BlockWidth, endx = startx * -1 + 1920 / BlockWidth, starty = drawy / blockDrawY, endy = starty * -1 + 1080 / blockDrawY + 1;

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

                if (mapObjectsJ < mapObjects.Count && mapObjects[mapObjectsJ].Y < j)
                {
                    l = 0;

                    if (GetDist(referenceToHero.X, referenceToHero.Y, mapObjects[mapObjectsJ].X, mapObjects[mapObjectsJ].Y) <= 40)
                    {
                        mapObjects[mapObjectsJ].Draw(spriteBatch, drawx + (int)(mapObjects[mapObjectsJ].X * BlockWidth), drawy + (int)(mapObjects[mapObjectsJ].Y * blockDrawY), this);
                    }

                    mapObjectsJ++;
                }
                else
                {
                    for (int i = startx; i < endx; i++)
                    {
                        // if (blocks[i][j].type != 0)
                        //{
                            blocks[i][j].draw(spriteBatch, drawx + i * BlockWidth, drawy + j * blockDrawY - blocks[i][j].textures[0].Height + blockDrawY, this);
                        //}
                    }
                }
            }

            //effects
            spriteBatch.Draw(darknessEffect, new Vector2(0, 0), Color.White);

            var mouseState = Mouse.GetState();

            if (referenceToHero != null)
            {
                //hero hp, inventory & other
                ((Hero)referenceToHero).DrawInterface(spriteBatch);
            }

            if (referenceToHero.HP <= 0)
            {
                spriteBatch.Draw(YouDiedTexture, new Vector2(0, 0), Color.White);
            }

            if(settingsOpened)
            {
                spriteBatch.Draw(settingsDarkness, new Vector2(0, 0), Color.White);
                
                exitButton.draw(spriteBatch);

                musicSlider.Draw(spriteBatch);

                soundsSlider.Draw(spriteBatch);
            }

            settingsButton.draw(spriteBatch);

            //drawing cursor
            spriteBatch.Draw(cursor, new Vector2(mouseState.X - cursor.Width / 2, mouseState.Y - cursor.Height / 2), Color.White);
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

            if (md > -1)
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
        /// Get closest object of given types
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="indexToIgnore"></param>
        /// <param name="typeAsString"></param>
        /// <returns></returns>
        public MapObject GetClosestObject(double x, double y, int indexToIgnore, List<string> typesAsStrings)
        {
            int mi = 0;
            double md = double.MaxValue;

            for (int i = 0; i < mapObjects.Count; i++)
            {
                if (typesAsStrings.Contains(mapObjects[i].GetTypeAsString()))
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
                    int q = rnd.Next(0, 100);

                    if (q < 50)
                    {
                        blocks[tmpx][tmpy] = new Block(2, tmpx, tmpy, contentManager, sampleBlocks[2]);
                    }
                    else
                    {
                        blocks[tmpx][tmpy] = new Block(6, tmpx, tmpy, contentManager, sampleBlocks[6]);
                    }
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
                double tmpx = x + 7 + rnd.NextDouble() * (xsize - 12);
                double tmpy = y + 7 + rnd.NextDouble() * (ysize - 12);

                if ((int)tmpx >= 0 && (int)tmpy >= 0 && (int)tmpx < blocks.Count && (int)tmpy < blocks[(int)tmpx].Count && blocks[(int)tmpx][(int)tmpy].passable)
                {
                    var refer= AddObject(new Ghost(contentManager, type, tmpx, tmpy, tmpx, tmpy, sampleGhosts[type]));

                    ((Ghost)refer).influenceRect = new Rectangle(x+1, y+1, xsize, ysize);

                    c++;
                }
            }
        }

        private void insertGun(ContentManager contentManager, int x, int y, int xsize, int ysize, Gun sampleGun)
        {
            var rnd = new Random();
            bool placed = false;

            while (!placed)
            {
                double tmpx = x + rnd.NextDouble() * xsize;
                double tmpy = y + rnd.NextDouble() * ysize;

                if ((int)tmpx >= 0 && (int)tmpy >= 0 && (int)tmpx < blocks.Count && (int)tmpy < blocks[(int)tmpx].Count && blocks[(int)tmpx][(int)tmpy].passable)
                {
                    AddObject(new Gun(contentManager, sampleGun.Type, tmpx, tmpy, sampleGun));

                    placed = true;
                }
            }
        }

        private void insertObject(ContentManager contentManager, int x, int y, int xsize, int ysize, MapObject mapObject)
        {
            var rnd = new Random();
            bool placed = false;

            while (!placed)
            {
                double tmpx = x + rnd.NextDouble() * xsize;
                double tmpy = y + rnd.NextDouble() * ysize;

                if ((int)tmpx >= 0 && (int)tmpy >= 0 && (int)tmpx < blocks.Count && (int)tmpy < blocks[(int)tmpx].Count && blocks[(int)tmpx][(int)tmpy].passable)
                {
                    MapObject reference = AddObject(mapObject);

                    reference.ChangeCoords(tmpx, tmpy);

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

        public void PlaceBlock(Block blockToPlace, int x, int y)
        {
            if (x >= 0 && y >= 0 && x < blocks.Count && y < blocks[x].Count)
            {
                blocks[x][y] = blockToPlace;
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