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
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private int tmpx = 0, tmpy = 0;
        private GameWorld testworld;
        private SimpleFps fpsc = new SimpleFps();
        private SpriteFont tmpfont;
        private DungeonSynthesizer dungeonSynthesizer;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;

            _graphics.ApplyChanges();

            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;

            _graphics.ApplyChanges();
            
            this.IsFixedTimeStep = true;
            this.TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d);
            _graphics.ApplyChanges();

            this.Window.IsBorderless = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
                
            tmpfont = Content.Load<SpriteFont>("mainfont");

            dungeonSynthesizer = new DungeonSynthesizer(Content, 1024, 1024);

            //dungeonSynthesizer.AlternativeGenerate(30, 4, 12);
            //dungeonSynthesizer.Reset(1024, 1024);
            dungeonSynthesizer.RandomSeeds(4096, 4097, 16, 5);
            dungeonSynthesizer.GenerateCorridors(250, 300);

            dungeonSynthesizer.ReplaceRooms(13, 13);
            dungeonSynthesizer.PlaceWalls();

            //testworld = new GameWorld(Content/*, "info/worlds/world1"*/);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            //testworld.update(Content);

            KeyboardState ks = Keyboard.GetState();

            if (ks.IsKeyDown(Keys.LeftAlt)&&ks.IsKeyDown(Keys.F1))
            {
                fpsc.Update(gameTime);
            }

            if(!IsActive)
            {
                //testworld.Save("info/worlds/world1");
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();

            //testworld.draw(_spriteBatch, tmpx, tmpy);
            dungeonSynthesizer.Visualize(_spriteBatch, 0, 0, 1, 1);

            KeyboardState ks = Keyboard.GetState();

            if (ks.IsKeyDown(Keys.LeftAlt) && ks.IsKeyDown(Keys.F1))
            {
                fpsc.DrawFps(_spriteBatch, tmpfont, new Vector2(0, 0), Color.White);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
