/**
 * Sprite for the raycaster.
 *
 * Holds a world position + texture and projects itself into screen space
 * relative to the camera, following the standard billboard maths from
 * http://lodev.org/cgtutor/raycasting3.html
 * */
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace RaycastEngine
{
	internal class Sprite
	{
		//--world position of the sprite--//
		public double x;
		public double y;

		//--texture drawn for this sprite--//
		public Texture2D texture;

		//========================================================//
		//== values below are recomputed every frame by project()==//
		//========================================================//

		//--perpendicular depth into the screen (same units as the wall ZBuffer)--//
		public double transformY;

		//--centre column of the sprite on screen--//
		public int screenX;

		//--on screen size in pixels (square texture -> width == height)--//
		public int width;
		public int height;

		//--screen rectangle the sprite spans (may extend off screen, SpriteBatch clips)--//
		public int drawStartX;
		public int drawEndX;
		public int drawStartY;
		public int drawEndY;

		//--is any part of the sprite in front of the camera and on screen?--//
		public bool visible;

		public Sprite(double x, double y, Texture2D texture)
		{
			this.x = x;
			this.y = y;
			this.texture = texture;
		}

		/// <summary>
		/// Projects the sprite into screen space relative to the camera.
		/// Must be called every frame before drawing.
		/// </summary>
		/// <param name="pos">camera position</param>
		/// <param name="dir">camera facing direction</param>
		/// <param name="plane">camera plane (FOV)</param>
		/// <param name="w">viewport width</param>
		/// <param name="h">viewport height</param>
		public void project(Vector2 pos, Vector2 dir, Vector2 plane, int w, int h)
		{
			//--sprite position relative to the camera--//
			double spriteX = x - pos.X;
			double spriteY = y - pos.Y;

			//--inverse of the camera matrix [ plane | dir ]--//
			//--required to transform the sprite into camera space--//
			double invDet = 1.0 / (plane.X * dir.Y - dir.X * plane.Y);

			double transformX = invDet * (dir.Y * spriteX - dir.X * spriteY);
			//--transformY is the depth "into" the screen (the Z used for occlusion)--//
			transformY = invDet * (-plane.Y * spriteX + plane.X * spriteY);

			//--anything at or behind the camera plane can't be seen--//
			if (transformY <= 0)
			{
				visible = false;
				return;
			}

			//--horizontal screen position of the sprite centre--//
			screenX = (int)((w / 2) * (1 + transformX / transformY));

			//--uniform scale based on depth (square textures so width == height)--//
			height = Math.Abs((int)(h / transformY));
			width = Math.Abs((int)(h / transformY));

			//--vertical extents, centred on the horizon (h/2) like the walls--//
			drawStartY = -height / 2 + h / 2;
			drawEndY = height / 2 + h / 2;

			//--horizontal extents centred on screenX--//
			drawStartX = -width / 2 + screenX;
			drawEndX = width / 2 + screenX;

			//--visible only if some of it actually lands on the screen--//
			visible = drawEndX > 0 && drawStartX < w;
		}
	}
}