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
using static dung.GameWorld;

namespace dung
{
    public class DungeonSynthesizer
    {
        private List<List<int>> mainArray;
        public Texture2D texture, texture2;
        public List<Tuple<int, int>> rooms { get; private set; }
        public List<int> roomsRarity { get; private set; }

        public DungeonSynthesizer(ContentManager contentManager, int x, int y)
        {
            texture = contentManager.Load<Texture2D>("synthesizer_visual");
            texture2 = contentManager.Load<Texture2D>("syntvis1");

            roomsRarity = new List<int>();
            rooms = new List<Tuple<int, int>>();

            Reset(x, y);
        }

        public void Reset(int x, int y)
        {
            mainArray = new List<List<int>>();
            rooms = new List<Tuple<int, int>>();

            for (int i = 0; i < x; i++)
            {
                List<int> tmplist = new List<int>();

                for (int j = 0; j < y; j++)
                {
                    tmplist.Add(0);
                }

                mainArray.Add(tmplist);
            }
        }

        public List<List<int>> GetList()
        {
            return mainArray;
        }

        public void RandomSeeds(int min, int max, int dist, int maxRarity)
        {
            try
            {
                var rnd = new Random();

                int tmpn = rnd.Next(min, max), c=0;

                while (c < tmpn)
                {
                    int x = rnd.Next(0, mainArray.Count / dist);
                    int y = rnd.Next(0, mainArray[0].Count / dist);

                    x *= dist;
                    y *= dist;

                    if (mainArray[x][y] == 0)
                    {
                        int q = (int)(GetDist(x, y, mainArray.Count / 2, mainArray[0].Count / 2));
                        int fs = (mainArray.Count / dist)/maxRarity/2;

                        q /= dist;
                        q /= fs;

                        mainArray[x][y] = 2;

                        rooms.Add(new Tuple<int, int>(x, y));

                        roomsRarity.Add(maxRarity - q);

                        c++;
                    }
                }
            }
            catch
            {
                
            }
        }

        public void GenerateCorridors(int mindist, int maxdist)
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                int cd1=Int32.MaxValue, cd2=Int32.MaxValue, ci1=-1, ci2=-1;

                for (int j = 0; j < rooms.Count; j++)
                {
                    if (i != j)
                    {
                        int tmpv = Math.Abs(rooms[i].Item1 - rooms[j].Item1) + Math.Abs(rooms[i].Item2 - rooms[j].Item2);

                        if (tmpv >= mindist && tmpv <= maxdist)
                        {
                            if (tmpv < cd1)
                            {
                                cd2 = cd1;
                                ci2 = ci1;

                                cd1 = tmpv;
                                ci1 = j;
                            }
                            else if (tmpv < cd2)
                            {
                                cd2 = tmpv;
                                ci2 = j;
                            }
                        }
                    }
                }

                if(ci1!=-1)
                {
                    int stepx = 0, stepy = 0;

                    if ((rooms[i].Item1 - rooms[ci1].Item1) != 0)
                    {
                        stepx = (rooms[i].Item1 - rooms[ci1].Item1) / Math.Abs(rooms[i].Item1 - rooms[ci1].Item1);
                    }

                    if ((rooms[i].Item2 - rooms[ci1].Item2) != 0)
                    {
                        stepy = (rooms[i].Item2 - rooms[ci1].Item2) / Math.Abs(rooms[i].Item2 - rooms[ci1].Item2);
                    }
                    
                    int tmpx = rooms[i].Item1, tmpy=rooms[i].Item2;

                    while (tmpx != rooms[ci1].Item1)
                    {
                        tmpx -= stepx;

                        mainArray[tmpx][tmpy] = 1;
                    }
                    
                    while (tmpy != rooms[ci1].Item2)
                    {
                        tmpy -= stepy;

                        mainArray[tmpx][tmpy] = 1;
                    }
                }

                if (ci2 != -1)
                {
                    int stepx = 0, stepy = 0;

                    if ((rooms[i].Item1 - rooms[ci2].Item1) != 0)
                    {
                        stepx = (rooms[i].Item1 - rooms[ci2].Item1) / Math.Abs(rooms[i].Item1 - rooms[ci2].Item1);
                    }

                    if ((rooms[i].Item2 - rooms[ci2].Item2) != 0)
                    {
                        stepy = (rooms[i].Item2 - rooms[ci2].Item2) / Math.Abs(rooms[i].Item2 - rooms[ci2].Item2);
                    }

                    int tmpx = rooms[i].Item1, tmpy = rooms[i].Item2;

                    while (tmpx != rooms[ci2].Item1)
                    {
                        tmpx -= stepx;

                        mainArray[tmpx][tmpy] = 1;
                    }

                    while (tmpy != rooms[ci2].Item2)
                    {
                        tmpy -= stepy;

                        mainArray[tmpx][tmpy] = 1;
                    }
                }
            }
        }

        public void ReplaceRooms(int x, int y)
        {
            for(int i=0; i<rooms.Count; i++)
            {
                int xi = rooms[i].Item1, yi = rooms[i].Item2;

                PlaceSquare(xi - x / 2, yi - y / 2, xi + x / 2, yi + y / 2, 1);
            }
        }
        
        public void PlaceDoors()
        {
            for (int i = 1; i < mainArray.Count - 1; i++)
            {
                for (int j = 1; j < mainArray[i].Count - 1; j++)
                {
                    int q1 = 0, q2 = 0;

                    for (int z = i - 1; z < i + 2; z++)
                    {
                        for (int k = j - 1; k < j + 2; k++)
                        {
                            if (k != j || z != i)
                            {
                                if(mainArray[z][k]==1)
                                {
                                    q1++;
                                }
                                else if(mainArray[z][k] == 2)
                                {
                                    q2++;
                                }
                            }
                        }
                    }

                    if(q1==4&&q2==4)
                    {
                        mainArray[i][j] = 5;
                    }
                }
            }
        }

        public void PlaceWalls()
        {
            for (int i = 0; i < mainArray.Count; i++)
            {
                for (int j = 0; j < mainArray[i].Count; j++)
                {
                    if(mainArray[i][j]==0)
                    {
                        //I know that looks like shit, but 2 next loops will do only 9 operations, so i wrote them instead of 9 long ifs
                        for (int i1 = i - 1; i1 < i + 2; i1++)
                        {
                            for (int j1 = j - 1; j1 < j + 2; j1++)
                            {
                                if(i1>=0&&j1>=0&&i1<mainArray.Count&&j1<mainArray[i].Count)
                                {
                                    if(mainArray[i1][j1]==1)
                                    {
                                        mainArray[i][j] = 2;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void PlaceAround(int aroundWhat, int whatToPlace)
        {
            for (int i = 0; i < mainArray.Count; i++)
            {
                for (int j = 0; j < mainArray[i].Count; j++)
                {
                    if (mainArray[i][j] == aroundWhat)
                    {
                        //I know that looks like shit, but 2 next loops will do only 9 operations, so i wrote them instead of 9 long ifs
                        for (int i1 = i - 1; i1 < i + 2; i1++)
                        {
                            for (int j1 = j - 1; j1 < j + 2; j1++)
                            {
                                if (i1 >= 0 && j1 >= 0 && i1 < mainArray.Count && j1 < mainArray[i].Count && !(i1 == i && j1 == j))
                                {
                                    mainArray[i1][j1] = whatToPlace;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Alternative generation
        /// </summary>
        /// <param name="dist">distance between two rooms</param>
        /// <param name="x">max count of rooms on x</param>
        /// <param name="y">max count of rooms on y</param>
        public void AlternativeGenerate(int dist, int maxRoom, int roomSize)
        {
            roomsRarity = new List<int>();

            Reset(maxRoom * 2 * dist - dist + roomSize / 2 + 1, maxRoom * 2 * dist - dist + roomSize / 2 + 1);

            var rnd = new Random();
            
            List<List<int>> rooms = new List<List<int>>();

            for (int i = 0; i < maxRoom * 2 - 1; i++)
            {
                List<int> tmplist = new List<int>();

                for (int j = 0; j < maxRoom * 2 - 1; j++)
                {
                    tmplist.Add(-1);
                }
                
                rooms.Add(tmplist);
            }

            for (int cr = 0; cr <= maxRoom; cr++)
            {
                rooms[rooms.Count / 2][cr] = cr;

                for (int i = cr; i < rooms.Count - cr; i++)
                {
                    for (int j = cr; j < rooms[i].Count - cr; j++)
                    {
                        rooms[i][j] = cr;
                    }
                }
            }

            int roomsToDelete = (int)(rooms.Count * rooms[0].Count * 0);

            for (int z = 0; z < roomsToDelete; z++)
            {
                int tmpx = rnd.Next(0, rooms.Count);
                int tmpy = rnd.Next(0, rooms[0].Count);

                if (rooms[tmpx][tmpy] != maxRoom)
                {
                    rooms[tmpx][tmpy] = -1;
                }
            }

            for (int i = 0; i < rooms.Count; i++)
            {
                for (int j = 0; j < rooms[i].Count; j++)
                {
                    if (rooms[i][j] != -1)
                    {
                        this.rooms.Add(new Tuple<int, int>(i * dist + roomSize / 2 + 1, j * dist + roomSize / 2 + 1));
                        this.roomsRarity.Add(rooms[i][j]);
                    }
                }
            }

            for (int i = 0; i <= maxRoom; i++)
            {
                int tmpx1 = i * dist + roomSize / 2;
                int tmpx2 = (maxRoom * 2 - 2 - i) * dist + roomSize / 2;

                PlaceSquare(tmpx1, tmpx1, tmpx1 + 1, tmpx2 + 1, 1);
                PlaceSquare(tmpx1, tmpx1, tmpx2 + 1, tmpx1 + 1, 1);
                PlaceSquare(tmpx2, tmpx1, tmpx2 + 1, tmpx2 + 1, 1);
                PlaceSquare(tmpx1, tmpx2, tmpx2 + 1, tmpx2 + 1, 1);
            }

            int tmpx11 = roomSize / 2;
            int tmpx12 = rooms.Count / 2 * dist + roomSize / 2;

            PlaceSquare(tmpx11, tmpx12, tmpx12*2+1, tmpx12 + 1, 1);
            PlaceSquare(tmpx12, tmpx11, tmpx12 + 1, tmpx12*2 + 1, 1);

            ReplaceRooms(roomSize, roomSize);
        }

        public void FinalGenerator(int dist, int maxroom, int roomSize)
        {
            int[,] rooms = new int[maxroom * 2 + 2,maxroom * 2 + 2];

            this.Reset((maxroom * 2 + 2) * dist, (maxroom * 2 + 2) * dist);

            for(int i=0; i< maxroom * 2 + 1; i++)
            {
                this.rooms.Add(new Tuple<int, int>(dist + i * dist, dist + maxroom * dist));

                if(i%2==0)
                {
                    this.roomsRarity.Add(-1);

                    PlaceSquare(dist + i * dist, mainArray.Count / 2, dist + (i + 1) * dist, mainArray.Count / 2 + 1, 1);
                }
                else
                {
                    this.roomsRarity.Add((i - 1) / 2);
                  //  PlaceSquare(dist + i * dist, mainArray.Count / 2, dist + (i + 1) * dist, mainArray.Count / 2 + 1, 1);
                }
            }

            for (int i = 1; i < maxroom * 2 + 1; i += 2)
            {
                PlaceSquare(i * dist + dist, mainArray[0].Count / 2 - (i + 2) * dist / 2, i * dist + dist + 1, mainArray[0].Count / 2 + (i + 2) * dist / 2, 1);
                PlaceSquare((i+1) * dist + dist, mainArray[0].Count / 2 - (i + 2) * dist / 2, (i+1) * dist + dist + 1, mainArray[0].Count / 2 + (i + 2) * dist / 2, 1);

                PlaceSquare(i * dist + dist, mainArray[0].Count / 2 - (i + 2) * dist / 2, (i + 1) * dist + dist, mainArray[0].Count / 2 - (i + 2) * dist / 2 + 1, 1);
                PlaceSquare(i * dist + dist, mainArray[0].Count / 2 + (i + 2) * dist / 2, (i + 1) * dist + dist, mainArray[0].Count / 2 + (i + 2) * dist / 2 + 1, 1);

                for (int j = 1; j <= (i + 2) / 2; j++)
                {
                    this.rooms.Add(new Tuple<int, int>(dist + i * dist, dist+(maxroom - j) * dist));
                    this.rooms.Add(new Tuple<int, int>(dist + i * dist, dist+(maxroom + j) * dist));

                    roomsRarity.Add((i - 1) / 2);
                    roomsRarity.Add((i - 1) / 2);
                }
            }

            ReplaceRooms(roomSize, roomSize);
            PlaceWalls();

            PlaceDoors();
        }

        public void PlaceSquare(int x1, int y1, int x2, int y2, int placeType)
        {
            int xbegin = Math.Max(0, x1), xend = Math.Min(x2, mainArray.Count);
            int ybegin = Math.Max(0, y1), yend = Math.Min(y2, mainArray[0].Count);

            for (int i = xbegin; i < xend; i++)
            {
                for (int j = ybegin; j < yend; j++)
                {
                    mainArray[i][j] = placeType;
                }
            }
        }

        public void Visualize(SpriteBatch spriteBatch, int x, int y, int width, int height)
        { 
            for (int i = 0; i < mainArray.Count; i++)
            {
                for (int j = 0; j < mainArray[i].Count; j++)
                {
                    if (this.mainArray[i][j] == 1)
                    {
                        spriteBatch.Draw(texture, new Vector2(i * width + x, j * height + y), Color.White);
                    }
                    else if (this.mainArray[i][j] == 2)
                    {
                        spriteBatch.Draw(texture, new Vector2(i * width  + x, j * height + y), Color.Black);
                    }
                }
            }

            for (int i = 0; i < rooms.Count; i++)
            { 
                spriteBatch.Draw(texture2, new Vector2(rooms[i].Item1 * width + x - texture2.Width/2, rooms[i].Item2 * height + y - texture2.Height / 2), new Color(roomsRarity[i] * 0, roomsRarity[i] * 50, roomsRarity[i] * 50));
            }
        }
       
        public double GetDist(double x, double y, double x1, double y1)
        {
            double a = Math.Abs(x - x1);
            double b = Math.Abs(y - y1);

            return Math.Sqrt(a * a + b * b);
        }
    }
}