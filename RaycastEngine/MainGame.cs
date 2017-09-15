/**
 * Owain Bell - 2017
 * */
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace RaycastEngine
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class MainGame : Game
    {
        //--set constant, texture size to be the wall (and sprite) texture size--//
        private static int texSize = 128;

        //--create slicer and declare slices--//
        private static TextureHandler slicer = new TextureHandler(texSize);
        private static Rectangle[] slices;

        //--viewport and width / height--//
        private static Viewport view;
        private static int width;
        private static int height;

        //--define camera--//
        private Camera camera;

        //--graphics manager and sprite batch--//
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        //--test texture--//
        Texture2D texture;

        //-test effect--//
        //Effect effect;

        //test sprite
        //Texture2D sprite;

        //--array of levels, levels reffer to "floors" of the world--//
        Level[] levels;

        //--struct to represent rects and tints of a level--//
        public struct Level
        {
            public Rectangle[] sv;
            public Rectangle[] cts;

            //--current slice tint (for lighting)--//
            public Color[] st;
        }

        public MainGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.PreferredBackBufferWidth = 1920;  // set these values to the desired width and height of your window 
            graphics.PreferredBackBufferHeight = 1080; // (if your computer struggles, either turn down number of levels rendered or turn this to something low, like 800 x 600)
            graphics.ApplyChanges();

            //--get viewport--//
            view = graphics.GraphicsDevice.Viewport;

            //--set view width and height--//
            width = view.Bounds.Width;
            height = view.Bounds.Height;

            //--init texture slices--//
            slices = slicer.getSlices();

            //--inits the levels--//
            levels = createLevels(5);

            //--init camera--//
            camera = new Camera(width, height, texSize, slices, levels);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            texture = getTexture("stone");
            //effect = Content.Load<Effect>("Effects/SpriteCuller");
            //sprite = getTexture("wood");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            camera.update();

            //effect.Parameters["startX"].SetValue(34);
            //effect.Parameters["endX"].SetValue(45);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            spriteBatch.Begin(SpriteSortMode.Immediate);

            for (int x = 0; x < width; x++)
            {
                for (int i = levels.Length - 1; i >= 0; i--)
                {
                    spriteBatch.Draw(texture, levels[i].sv[x], levels[i].cts[x], levels[i].st[x]);
                }
            }

            spriteBatch.End();

            //--testing sprite--//
            //   spriteBatch.Begin();
            //   effect.CurrentTechnique.Passes[0].Apply();
            //   spriteBatch.Draw(sprite, Vector2.Zero, Color.White);
            //   spriteBatch.End();

            base.Draw(gameTime);
        }

        public Level[] createLevels(int numLevels)
        {
            Level[] arr = new Level[numLevels];
            for (int i = 0; i < numLevels; i++)
            {
                arr[i] = new Level();
                arr[i].sv = SliceView();
                arr[i].cts = new Rectangle[width];
                arr[i].st = new Color[width];
            }
            return arr;
        }

        /// <summary>
        /// Creates rectangle slices for each x in width.
        /// </summary>
        public Rectangle[] SliceView()
        {
            Rectangle[] arr = new Rectangle[width];
            for (int x = 0; x < width; x++)
            {
                arr[x] = new Rectangle(x, 0, 1, height);
            }
            return arr;
        }

        /// <summary>
        /// Returns texture by texture name string
        /// </summary>
        /// <param name="textureName">Texture name string.</param>
        public Texture2D getTexture(string textureName)
        {
            return Content.Load<Texture2D>("Textures/" + textureName);
        }
    }
}
