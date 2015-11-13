using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgCubio
{
    /// <summary>
    /// 
    /// </summary>
    public class World
    {

        readonly int Width;

        readonly int Height;

        public double playerX { get; set; }

        public double playerY { get; set; }

        public double scale { get; set; }

        /// <summary>
        /// Need to decide how to track all of these, delete them, and create them on a screen.
        /// </summary>
        public Dictionary<int,Cube> Cubes
        {
            get;
            set;
        }

        public World(int width, int height)
        {
            Width = width;
            Height = height;
            Cubes = new Dictionary<int,Cube>();
        }

//        public double getX(this Cube cube)
//        {
//            //rectangle = new RectangleF((int)((c.loc_x - Px - c.width * scale * 2.5) * scale + Width / 2), (int)((c.loc_y - Py - c.width * scale * 2.5) * scale + Height / 2), 
//              //  (int)(c.width * scale * 5), (int)(c.width * scale * 5));


////            rectangle = new RectangleF((int)((c.loc_x - Px) * scale + Width / 2), (int)((c.loc_y - Py) * scale + Height / 2), (int)(c.width), (int)(c.width));


//            return (cube.loc_x - playerX - (cube.width/2) * scale + Width/2);
//        }

//        public double getY(this Cube cube)
//        {
//            return (cube.loc_x - playerX - (cube.width/2) * scale + Height/2);
//        }
        /*

        The World Class

The world represents the "state" of the simulation.This class is responsible for tracking at least the following data: 
the world Width and Height(please use read only 'constants'), all the cubes in the game.You may of course store additional information.

    */
    }
}
