// Cube.cs
// Created by Daniel Avery and Keeton Hodgson
// November 7th, 2015

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgCubio
{

    /// <summary>
    /// A cube.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Cube
    {
        /// <summary>
        /// X coordinate location
        /// </summary>
        [JsonProperty]
        public double loc_x { get; set; }

        /// <summary>
        /// Y coordinate location
        /// </summary>
        [JsonProperty]
        public double loc_y { get; set; }

        /// <summary>
        /// The team ID.
        /// Keeps track of players when they split.
        /// Food id: 0.
        /// </summary>
        [JsonProperty]
        public int Team_ID { get; private set; }

        /// <summary>
        /// ID of this current cube
        /// </summary>
        [JsonProperty]
        public int uid { get; set; }

        /// <summary>
        /// True = food
        /// False = player
        /// </summary>
        [JsonProperty]
        public bool food { get; private set; }

        /// <summary>
        /// Name of this cube:
        /// "" is standard for food items
        /// A player may enter any name.
        /// </summary>
        [JsonProperty]
        public string Name { get; private set; }

        /// <summary>
        /// The mass of this cube
        /// </summary>
        [JsonProperty]
        public double Mass { get; set; }

        /// <summary>
        /// Color of this cube
        /// </summary>
        [JsonProperty]
        public int argb_color { get; private set; }

        /// <summary>
        /// Width, derived as the square root of the width
        /// </summary>
        public double width
        {
            get { return Math.Sqrt(Mass); }
            private set { }
        }

        /// <summary>
        /// Coordinate (X) of the left side of the cube
        /// </summary>
        public double left
        {
            get { return loc_x - (width / 2); }
            private set { }
        }

        /// <summary>
        /// Coordinate (X) of the right side of the cube
        /// </summary>
        public double right
        {
            get { return loc_x + (width / 2); }
            private set { }
        }

        /// <summary>
        /// Coordinate (Y) of the top of the cube
        /// </summary>
        public double top
        {
            get { return loc_y + (width / 2); }
            private set { }
        }

        /// <summary>
        /// Coordinate (Y) of the bottom of the cube
        /// </summary>
        public double bottom
        {
            get { return loc_y - (width / 2); }
            private set { }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <param name="uid">cube id</param>
        /// <param name="food">True if this is a food item, false if it is a player</param>
        /// <param name="name">Name of the cube. "" is for food</param>
        /// <param name="mass">Cube mass</param>
        /// <param name="color">Cube color</param>
        [JsonConstructor]
        public Cube(double x, double y, int uid, bool food, string name, double mass, int color, int team_id)
        {
            loc_x = x;
            loc_y = y;
            this.uid = uid;
            this.food = food;
            Name = name;
            Mass = mass;
            argb_color = color;
            Team_ID = team_id;
        }



    }
}
