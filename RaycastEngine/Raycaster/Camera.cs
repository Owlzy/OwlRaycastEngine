using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaycastEngine
{
    class Camera
    {

        //--camera position, init to start position--//
        private static Vector2 pos = new Vector2(22.5f, 11.5f);

        //--current facing direction, init to values coresponding to FOV--//
        private static Vector2 dir = new Vector2(-1.0f, 0.0f);

        //--the 2d raycaster version of camera plane, adjust y component to change FOV (ratio between this and dir x resizes FOV)--//
        private static Vector2 plane = new Vector2(0.0f, 0.66f);

        //--viewport width and height--//
        private static int w;
        private static int h;

        //--world map--//
        private static Map map;
        private static int[,] worldMap;

        //--texture width--//
        private static int texWidth;

        //--sliced view--//
        private static Rectangle[] sv;

        //--current texture slices--//
        private static Rectangle[] cts;

        //--slices--//
        private static Rectangle[] s;

        //--slice tints--//
        private static Color[] st;

        //--move speed--//
        private static double moveSpeed = 0.04;

        //--rotate speed--//
        private static double rotSpeed = 0.02;

        //--cam x pre calc--//
        private static double[] camX;

        public Camera(int width, int height, int texWid, Rectangle[] slicedView, Rectangle[] currTexSlices, Rectangle[] slices, Color[] sliceTints)
        {
            w = width;
            h = height;
            texWidth = texWid;
            sv = slicedView;
            s = slices;
            cts = currTexSlices;
            st = sliceTints;

            //--init cam pre calc array--//
            camX = new double[w];
            preCalcCamX();

            map = new Map();
            worldMap = map.getGrid();

            raycast();
        }

        public void update()
        {
            //--do raycast--//
            raycast();

            //=========================//
            //=====take user input=====//
            //=========================//

            KeyboardState state = Keyboard.GetState();

            bool lArrowKeyDown = state.IsKeyDown(Keys.Left) || state.IsKeyDown(Keys.A);

            if (lArrowKeyDown)
            {
                rotate(rotSpeed);
            }

            bool rArrowKeyDown = state.IsKeyDown(Keys.Right) || state.IsKeyDown(Keys.D);

            if (rArrowKeyDown)
            {
                rotate(-rotSpeed);
            }

            bool uArrowKeyDown = state.IsKeyDown(Keys.Up) || state.IsKeyDown(Keys.W);

            if (uArrowKeyDown)
            {
                move(moveSpeed);
            }

            bool dArrowKeyDown = state.IsKeyDown(Keys.Down) || state.IsKeyDown(Keys.S);

            if (dArrowKeyDown)
            {
                move(-moveSpeed);
            }

            //=========================//
            //=====user input end======//
            //=========================//
        }

        public void raycast()
        {
            /**
             * credit : Raycast loop and setting up of vectors for matrix calculations
             * courtesy - http://lodev.org/cgtutor/raycasting.html
             */

            //--rays start at camera position--//
            double rayPosX = pos.X;
            double rayPosY = pos.Y;

            for (int x = 0; x < w; x++)
            {
                //calculate ray position and direction
                double cameraX = camX[x];//x-coordinate in camera space
                double rayDirX = dir.X + plane.X * cameraX;
                double rayDirY = dir.Y + plane.Y * cameraX;

                //which box of the map we're in
                int mapX = (int)rayPosX;
                int mapY = (int)rayPosY;

                //length of ray from current position to next x or y-side
                double sideDistX;
                double sideDistY;

                //length of ray from one x or y-side to next x or y-side
                double deltaDistX = Math.Sqrt(1 + (rayDirY * rayDirY) / (rayDirX * rayDirX));
                double deltaDistY = Math.Sqrt(1 + (rayDirX * rayDirX) / (rayDirY * rayDirY));
                double perpWallDist;

                //what direction to step in x or y-direction (either +1 or -1)
                int stepX;
                int stepY;

                int hit = 0; //was there a wall hit?
                int side = -1; //was a NS or a EW wall hit?

                //calculate step and initial sideDist
                if (rayDirX < 0)
                {
                    stepX = -1;
                    sideDistX = (rayPosX - mapX) * deltaDistX;
                }
                else
                {
                    stepX = 1;
                    sideDistX = (mapX + 1.0 - rayPosX) * deltaDistX;
                }
                if (rayDirY < 0)
                {
                    stepY = -1;
                    sideDistY = (rayPosY - mapY) * deltaDistY;
                }
                else
                {
                    stepY = 1;
                    sideDistY = (mapY + 1.0 - rayPosY) * deltaDistY;
                }
                //perform DDA
                while (hit == 0)
                {
                    //jump to next map square, OR in x-direction, OR in y-direction
                    if (sideDistX < sideDistY)
                    {
                        sideDistX += deltaDistX;
                        mapX += stepX;
                        side = 0;
                    }
                    else
                    {
                        sideDistY += deltaDistY;
                        mapY += stepY;
                        side = 1;
                    }
                    //Check if ray has hit a wall
                    if (worldMap[mapX, mapY] > 0) hit = 1;
                }

                //Calculate distance of perpendicular ray (oblique distance will give fisheye effect!)
                if (side == 0) perpWallDist = (mapX - rayPosX + (1 - stepX) / 2) / rayDirX;
                else perpWallDist = (mapY - rayPosY + (1 - stepY) / 2) / rayDirY;

                //Calculate height of line to draw on screen
                int lineHeight = (int)(h / perpWallDist);

                //calculate lowest and highest pixel to fill in current stripe
                int drawStart = -lineHeight / 2 + h / 2;
                int drawEnd = lineHeight / 2 + h / 2;

                //texturing calculations
                int texNum = worldMap[mapX, mapY] - 1; //1 subtracted from it so that texture 0 can be used!

                //calculate value of wallX
                double wallX; //where exactly the wall was hit
                if (side == 0) wallX = rayPosY + perpWallDist * rayDirY;
                else wallX = rayPosX + perpWallDist * rayDirX;
                wallX -= Math.Floor(wallX);

                //x coordinate on the texture
                int texX = (int)(wallX * texWidth);
                if (side == 0 && rayDirX > 0) texX = texWidth - texX - 1;
                if (side == 1 && rayDirY < 0) texX = texWidth - texX - 1;

                //--set current texture slice to be slice x--//
                cts[x] = s[texX];

                //--set height of slice--//
                sv[x].Height = lineHeight;

                //--set draw start of slice--//
                sv[x].Y = drawStart;

                //--due to modern way of drawing using quads this should be down here to ovoid glitches at the edges--//
                if (drawStart < 0) drawStart = 0;
                if (drawEnd >= h) drawEnd = h - 1;

                //--add a bit of tint for simple lighting / to differentiate between walls of a corner--//
                st[x] = Color.White;
                if (side == 1) st[x] = Color.Gainsboro;//subtle tint

                //--distance based dimming of light--//
                double shadowDepth = 12.0 * perpWallDist;//increase shadow depth to make level darker
                st[x].R -= (byte)(shadowDepth);
                st[x].G -= (byte)(shadowDepth);
                st[x].B -= (byte)(shadowDepth);
            }

        }

        /// <summary>
        /// Moves camera by move speed
        /// </summary>
        /// <param name="mSpeed">Move speed</param>
        public void move(double mSpeed)
        {
            if (worldMap[(int)(pos.X + dir.X * mSpeed * 12), (int)pos.Y] > 0 == false) pos.X += (float)(dir.X * mSpeed);
            if (worldMap[(int)pos.X, (int)(pos.Y + dir.Y * mSpeed * 12)] > 0 == false) pos.Y += (float)(dir.Y * mSpeed);
        }

        /// <summary>
        /// Rotates camera by rotate speed
        /// </summary>
        /// <param name="rSpeed">Rotate speed</param>
        public void rotate(double rSpeed)
        {
            //both camera direction and camera plane must be rotated
            double oldDirX = dir.X;
            dir.X = (float)(dir.X * Math.Cos(rSpeed) - dir.Y * Math.Sin(rSpeed));
            dir.Y = (float)(oldDirX * Math.Sin(rSpeed) + dir.Y * Math.Cos(rSpeed));
            double oldPlaneX = plane.X;
            plane.X = (float)(plane.X * Math.Cos(rSpeed) - plane.Y * Math.Sin(rSpeed));
            plane.Y = (float)(oldPlaneX * Math.Sin(rSpeed) + plane.Y * Math.Cos(rSpeed));
        }

        /// <summary>
        /// precalculates camera x coordinate
        /// </summary>
        public static void preCalcCamX()
        {
            for (int x = 0; x < w; x++)
                camX[x] = 2 * x / (double)w - 1; //x-coordinate in camera space
        }
    }
}
