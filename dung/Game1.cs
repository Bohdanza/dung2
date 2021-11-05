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
using System.Threading;

namespace dung
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private int tmpx = 0, tmpy = 0;
        private GameWorld testworld;
        private SimpleFps fpsc = new SimpleFps();
        private SpriteFont tmpfont, loadingFont;
        private DungeonSynthesizer dungeonSynthesizer;
        private Thread newGameWorldThread;
        private button createWorldButton;
        private bool worldActive = false;
        private List<Texture2D> loadingScreenTextures;
        private int loadingScreenPhase;
        private Texture2D backgroundmenu;
        private List<Texture2D> storyImages;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            _graphics.ApplyChanges();
            
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;

            _graphics.ApplyChanges();
            
            this.IsFixedTimeStep = true;
            this.TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d);
            _graphics.ApplyChanges();

            this.Window.IsBorderless = true;

            _graphics.IsFullScreen = true;
            _graphics.ApplyChanges();
        }

        public void CreateWorld()
        {
            testworld = new GameWorld(this.Content);
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
            loadingFont = Content.Load<SpriteFont>("button_font");

            backgroundmenu = Content.Load<Texture2D>("backgroundmenu");

            var tmptex1 = Content.Load<Texture2D>("newgamebutton");

            createWorldButton = new button(0, 130, 80, tmptex1.Width, tmptex1.Height, tmptex1, Content.Load<Texture2D>("newgamebuttonpressed"), Content.Load<SpriteFont>("button_font"), "New game", Color.White);

            newGameWorldThread = new Thread(new ThreadStart(CreateWorld));

            loadingScreenTextures = new List<Texture2D>();
            loadingScreenPhase = 0;

            while(File.Exists("Content/loadingscreen"+loadingScreenPhase.ToString()+".xnb"))
            {
                loadingScreenTextures.Add(Content.Load<Texture2D>("loadingscreen" + loadingScreenPhase.ToString()));

                loadingScreenPhase++;
            }

            loadingScreenPhase = 0;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardState ks = Keyboard.GetState();

            if (ks.IsKeyDown(Keys.LeftAlt)&&ks.IsKeyDown(Keys.F1))
            {
                fpsc.Update(gameTime);
            }

            if (worldActive && !IsActive && !newGameWorldThread.IsAlive)
            {
                testworld.Save("info/worlds/world1");
            }

            if(!worldActive)
            {
                createWorldButton.update();

                if (createWorldButton.pressed)
                {
                    worldActive = true;

                    IsMouseVisible = false;

                    _graphics.ApplyChanges();

                    newGameWorldThread.Start();
                }
            }    
            else if(newGameWorldThread.IsAlive)
            {
                loadingScreenPhase++;

                loadingScreenPhase %= loadingScreenTextures.Count;
            }
            else
            {
                testworld.update(Content);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();

            if (!worldActive)
            {
                _spriteBatch.Draw(backgroundmenu, new Vector2(0, 0), Color.White);

                createWorldButton.draw(_spriteBatch);
            }
            else if (newGameWorldThread.IsAlive)
            {
                _spriteBatch.Draw(loadingScreenTextures[loadingScreenPhase], new Vector2(0, 0), Color.White);

                _spriteBatch.DrawString(loadingFont, "Loading", new Vector2(960 - loadingFont.MeasureString("Loading").X / 2, 870), Color.White);
            }
            else
            {
                testworld.draw(_spriteBatch, tmpx, tmpy);
                //dungeonSynthesizer.Visualize(_spriteBatch, 0, 0, 1, 1);
            }

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
