﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgCubio
{
    public class World
    {

        readonly int Width;

        readonly int Height;

        /// <summary>
        /// Need to decide how to track all of these, delete them, and create them on a screen.
        /// </summary>
        public HashSet<Cube> Cubes
        {
            get;
            set;
        }

        public World(int width, int height)
        {
            Width = width;
            Height = height;
        }
        /*

        The World Class

The world represents the "state" of the simulation.This class is responsible for tracking at least the following data: 
the world Width and Height(please use read only 'constants'), all the cubes in the game.You may of course store additional information.

    */
    }
}