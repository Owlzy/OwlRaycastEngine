/**
 * Owain Bell - 2017
 * */
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;

namespace RaycastEngine
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class MainGame : Game
	{
		//--set constant, texture size to be the wall (and sprite) texture size--//
		private static int texSize = 256;

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

		Texture2D[] textures = new Texture2D[5];

		//--test texture--//
		Texture2D floor;
		Texture2D sky;

		//-test effect--//
		Effect effect;

		//--single test sprite + the shader that culls it against walls--//
		private Sprite testSprite;
		private Effect spriteCuller;

		//--array of levels, levels reffer to "floors" of the world--//
		Level[] levels;

		//--struct to represent rects and tints of a level--//
		public struct Level
		{
			public Rectangle[] sv;
			public Rectangle[] cts;

			//--current slice tint (for lighting)--//
			public Color[] st;
			public int[] currTexNum;
		}

		public MainGame()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";

			graphics.GraphicsProfile = GraphicsProfile.HiDef;
			graphics.PreferredBackBufferWidth = 1024;  // set these values to the desired width and height of your window 
			graphics.PreferredBackBufferHeight = 700; // (if your computer struggles, either turn down number of levels rendered or turn this to something low, like 800 x 600)
													  // graphics.IsFullScreen = true;
			graphics.ApplyChanges();
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any required services and
		/// initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			//--get viewport--//
			view = graphics.GraphicsDevice.Viewport;

			//--set view width and height--//
			width = view.Bounds.Width;
			height = view.Bounds.Height;

			//--init texture slices--//
			slices = slicer.getSlices();

			//--inits the levels--//
			levels = createLevels(4);

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
			textures[0] = getTexture("stone");
			textures[1] = getTexture("left_bot_house");
			textures[2] = getTexture("right_bot_house");
			textures[3] = getTexture("left_top_house");
			textures[4] = getTexture("right_top_house");

			floor = getTexture("floor");
			sky = getTexture("sky");

			//--load the sprite texture and the wall aware culling shader--//
			Texture2D fireball = getTexture("fireball");
			spriteCuller = Content.Load<Effect>("Effects/SpriteCuller");

			//--a single test sprite, placed in open space down the start corridor--//
			testSprite = new Sprite(12.5, 11.5, fireball);
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
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);

			//--draw sky and floor--//
			spriteBatch.Begin();
			spriteBatch.Draw(floor,
			   new Rectangle(0, (int)(height * 0.5f), width, (int)(height * 0.5f)),
			   new Rectangle(0, 0, texSize, texSize),
			   Color.White);
			spriteBatch.Draw(sky,
			new Rectangle(0, 0, width, (int)(height * 0.5f)),
			new Rectangle(0, 0, texSize, texSize),
			Color.White);
			spriteBatch.End();

			//--draw walls--//
			spriteBatch.Begin();

			for (int x = 0; x < width; x++)
			{
				for (int i = levels.Length - 1; i >= 0; i--)
				{
					spriteBatch.Draw(textures[levels[i].currTexNum[x]], levels[i].sv[x], levels[i].cts[x], levels[i].st[x]);
				}
			}

			spriteBatch.End();

			//--draw sprite(s) on top, culled against the walls--//
			drawSprite(testSprite, BlendState.Additive);

			base.Draw(gameTime);
		}

		/// <summary>
		/// Draws a sprite as a billboard, using the SpriteCuller effect to discard
		/// any columns that are hidden behind a nearer wall (read from the camera ZBuffer).
		/// </summary>
		private void drawSprite(Sprite sprite, BlendState blend)
		{
			//--project the sprite into screen space relative to the camera--//
			sprite.project(Camera.getPos(), Camera.getDir(), Camera.getPlane(), width, height);

			if (!sprite.visible) return;

			double[] zBuffer = Camera.getZBuffer();

			//--screen columns the sprite actually covers (clamped to the viewport)--//
			int startCol = Math.Max(0, sprite.drawStartX);
			int endCol = Math.Min(width - 1, sprite.drawEndX);

			//--screen space -> clip space transform for the culler's vertex shader--//
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, width, height, 0, 0, 1);

			//--full screen rectangle the sprite spans (SpriteBatch clips off screen parts)--//
			Rectangle dest = new Rectangle(
				sprite.drawStartX,
				sprite.drawStartY,
				sprite.drawEndX - sprite.drawStartX,
				sprite.drawEndY - sprite.drawStartY);

			//--walk the columns, grouping un-occluded ones into contiguous runs--//
			int col = startCol;
			while (col <= endCol)
			{
				//--skip columns where a wall is nearer than the sprite (occluded)--//
				if (!(sprite.transformY < zBuffer[col]))
				{
					col++;
					continue;
				}

				//--found the start of a visible run, extend it as far as possible--//
				int runStart = col;
				while (col <= endCol && sprite.transformY < zBuffer[col])
					col++;
				int runEnd = col; //--exclusive end column--//

				//--tell the shader to keep only this run of columns--//
				spriteCuller.Parameters["WorldViewProjection"].SetValue(projection);
				spriteCuller.Parameters["startXScreen"].SetValue((float)runStart);
				spriteCuller.Parameters["endX"].SetValue((float)runEnd);
				spriteCuller.Parameters["viewWidth"].SetValue((float)width);

				//--draw the whole sprite quad; the shader discards everything outside the run--//
				spriteBatch.Begin(SpriteSortMode.Deferred, blend, SamplerState.PointClamp, null, null, spriteCuller);
				spriteBatch.Draw(sprite.texture, dest, null, Color.White);
				spriteBatch.End();
			}
		}

		//returns an initialised Level struct
		public Level[] createLevels(int numLevels)
		{
			Level[] arr = new Level[numLevels];
			for (int i = 0; i < numLevels; i++)
			{
				arr[i] = new Level();
				arr[i].sv = SliceView();
				arr[i].cts = new Rectangle[width];
				arr[i].st = new Color[width];
				arr[i].currTexNum = new int[width];

				for (int j = 0; j < arr[i].currTexNum.Length; j++)
				{
					arr[i].currTexNum[j] = 1;
				}
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