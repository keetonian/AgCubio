// Created by Daniel Avery and Keeton Hodgson
// November 2015

using System.Collections.Generic;
using System.Xml;

namespace AgCubio
{
    /// <summary>
    /// The world model for AgCubio
    /// </summary>
    public class World
    {
        /// <summary>
        /// Width of the world
        /// </summary>
        readonly int WIDTH;

        /// <summary>
        /// Height of the world
        /// </summary>
        readonly int HEIGHT;

        /// <summary>
        /// Number of updates the server attemps to execute per second
        /// </summary>
        readonly int HEARTBEATS_PER_SECOND;

        /// <summary>
        /// Maximum player speed (for small cube sizes)
        /// </summary>
        readonly int MAX_SPEED;

        /// <summary>
        /// Minimum player speed (for large cube sizes)
        /// </summary>
        readonly int MIN_SPEED;

        /// <summary>
        /// Scaler for how quickly a player cube loses mass
        /// </summary>
        readonly double ATTRITION_RATE_SCALER;

        /// <summary>
        /// Default mass of food cubes
        /// </summary>
        readonly int FOOD_MASS;

        /// <summary>
        /// Starting mass for all players
        /// </summary>
        readonly int PLAYER_START_MASS;

        /// <summary>
        /// Maximum total food cubes in the world
        /// </summary>
        readonly int MAX_FOOD_COUNT;

        /// <summary>
        /// Minimum mass before a player can spit
        /// </summary>
        readonly int MIN_SPLIT_MASS;

        /// <summary>
        /// How far a cube can be thrown when split
        /// </summary>
        readonly int MAX_SPLIT_DISTANCE;

        /// <summary>
        /// Maximum total cubes a single player is allowed
        /// </summary>
        readonly int MAX_SPLIT_COUNT;

        /// <summary>
        /// Percent overlap before a larger cube eats a smaller
        /// </summary>
        readonly double ABSORB_PERCENT_COVERAGE;

        /// <summary>
        /// Dictionary for storing all the cubes. Uid's map to cubes
        /// </summary>
        public Dictionary<int,Cube> Cubes { get; set; }


        /// <summary>
        /// Constructs a new world of the specified dimensions in the xml file
        /// </summary>
        public World(string filename)
        {
            Cubes = new Dictionary<int,Cube>();
            using (XmlReader reader = XmlReader.Create(filename))
            {
                //TODO: implement xml file stuff.
            }
        }


        /// <summary>
        /// Default constructor: uses predetermined values
        /// </summary>
        public World()
        {
            this.ABSORB_PERCENT_COVERAGE = .25;
            this.ATTRITION_RATE_SCALER = .005;
            this.FOOD_MASS = 1;
            this.HEARTBEATS_PER_SECOND = 30;
            this.HEIGHT = 1000;
            this.WIDTH = 1000;
            this.MAX_FOOD_COUNT = 5000;
            this.MAX_SPEED = 10;
            this.MAX_SPLIT_COUNT = 15;
            this.MAX_SPLIT_DISTANCE = 30;
            this.MIN_SPEED = 1;
            this.MIN_SPLIT_MASS = 25;
            this.PLAYER_START_MASS = 10;
        }
    }
}
