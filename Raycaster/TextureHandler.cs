/**
 * Owain Bell - 2017
 * */
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/**
 * Class splits textures into frames among other things
 */
namespace RaycastEngine
{
    class TextureHandler
    {
        //--store our "slice" rects--//
        Rectangle[] slices;

        public TextureHandler(int texWidth)
        {
            //--init array--//
            slices = new Rectangle[texWidth];

            //--for clarity in slice loop--//
            int texHeight = texWidth;

            //--loop through creating a "slice" for each texture x--//
            for (int x = 0; x < texWidth; x++)
            {
                //tex width and height are always equal so safe to use tex width instead of height here
                slices[x] = new Rectangle(x, 0, 1, texHeight);
            }
        }

        public Rectangle[] getSlices()
        {
            return slices;
        }
    }
}
