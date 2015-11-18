// Created by Daniel Avery and Keeton Hodgson
// November 2015

using System.Collections.Generic;

namespace AgCubio
{
    /// <summary>
    /// 
    /// </summary>
    public class World
    {
        /// <summary>
        /// Width of the world
        /// </summary>
        readonly int Width;

        /// <summary>
        /// Height of the world
        /// </summary>
        readonly int Height;

        /// <summary>
        /// Dictionary for storing all the cubes. Uid's map to cubes
        /// </summary>
        public Dictionary<int,Cube> Cubes
        {
            get;
            set;
        }


        /// <summary>
        /// Constructs a new world of the specified dimensions
        /// </summary>
        public World(int width, int height)
        {
            Width = width;
            Height = height;
            Cubes = new Dictionary<int,Cube>();
        }
    }
}
