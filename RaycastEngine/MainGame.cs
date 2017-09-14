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

        //--view sliced into rectangles, and current slice of texture to display for that rectangle--//
        private Rectangle[] slicedView;
        private Rectangle[] currTexSlice;

        //--current slice tint (for lighting)--//
        Color[] sliceTints;

        //--define camera--//
        private Camera camera;

        //--graphics manager and sprite batch--//
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        //--test texture--//
        Texture2D texture;

        //-test effect--//
        Effect effect;

        //test sprite
        Texture2D sprite;

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
            graphics.PreferredBackBufferWidth = 1280;  // set this value to the desired width of your window
            graphics.PreferredBackBufferHeight = 720;   // set this value to the desired height of your window
            graphics.ApplyChanges();

            //--get viewport--//
            view = graphics.GraphicsDevice.Viewport;

            //--set view width and height--//
            width = view.Bounds.Width;
            height = view.Bounds.Height;

            //--init texture slices--//
            slices = slicer.getSlices();

            //--init slices view--//
            slicedView = new Rectangle[width];

            //--slice the view--//
            sliceView();

            //--current texture slices--//
            currTexSlice = new Rectangle[width];

            //--init slice tints--//
            sliceTints = new Color[width];

            //--init camera--//
            camera = new Camera(width, height, texSize, slicedView, currTexSlice, slices, sliceTints);

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
            effect = Content.Load<Effect>("Effects/SpriteCuller");
            sprite = getTexture("wood");
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

          //Debug.WriteLine(effect.Parameters["startX"].GetValueInt32());

            effect.Parameters["startX"].SetValue(34);
            effect.Parameters["endX"].SetValue(45);

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
            spriteBatch.Begin();

            for (int x = 0; x < width; x++)
            {
                spriteBatch.Draw(texture, slicedView[x], currTexSlice[x], sliceTints[x]);
            }

            spriteBatch.End();

            //--testing sprite--//
            spriteBatch.Begin();
            effect.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(sprite, Vector2.Zero, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Creates rectangle slices for each x in width.
        /// </summary>
        public void sliceView()
        {
            for (int x = 0; x < width; x++)
            {
                slicedView[x] = new Rectangle(x, 0, 1, height);
            }
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
