// Created by Daniel Avery and Keeton Hodgson
// November 2015

using Newtonsoft.Json;
using System;

namespace AgCubio
{
    /// <summary>
    /// A cube
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
        public int Team_ID { get; set; }

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
        /// Width, derived as the (almost) square root of the width
        /// </summary>
        public double width
        {
            get { return Math.Sqrt(Mass); }
            private set { }
        }

        /// <summary>
        /// Returns the right x value of the cube
        /// </summary>
        public double right
        {
            get { return this.loc_x + this.width / 2; }
            private set { }
        }

        /// <summary>
        /// Returns the left x value of the cube
        /// </summary>
        public double left
        {
            get { return this.loc_x - this.width / 2; }
            private set { }
        }

        /// <summary>
        /// Returns the top y value of the cube
        /// </summary>
        public double top
        {
            get { return this.loc_y - this.width / 2; }
            private set { }
        }

        /// <summary>
        /// Returns the bottom y value of the cube
        /// </summary>
        public double bottom
        {
            get { return this.loc_y + this.width / 2; }
            private set { }
        }


        /// <summary>
        /// Constructs a Cube
        /// </summary>
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


        /// <summary>
        /// Constructs a simple Cube (only used for comparison purposes when finding start coords)
        /// </summary>
        public Cube(double x, double y, double mass)
        {
            loc_x = x;
            loc_y = y;
            Mass = mass;
        }


        /// <summary>
        /// Checks if this cube is overlapping the center of another given cube (parameter)
        /// </summary>
        public bool Collides(Cube other)
        {
            return (this.left < other.loc_x && other.loc_x < this.right) && (this.top < other.loc_y && other.loc_y < this.bottom);
        }


        /// <summary>
        /// Checks if this cube is overlapping any part of another given cube (parameter)
        /// </summary>
        public bool Overlaps(Cube other)
        {
            return ((other.left < this.left   && this.left   < other.right )   // FIRST BLOCK: horizontal alignment - this cube's left or right
                ||  (other.left < this.right  && this.right  < other.right ))  //   edge is between other's left and right edges
                && ((other.top  < this.top    && this.top    < other.bottom)   // SECOND BLOCK: vertical alignment - this cube's top or bottom
                ||  (other.top  < this.bottom && this.bottom < other.bottom)); //   edge is between other's top and bottom edges
        }


        /// <summary>
        /// Overrides Object.GetHashCode based on uid
        /// </summary>
        public override int GetHashCode()
        {
            return this.uid;
        }


        /// <summary>
        /// Overrides Object.Equals based on uid
        /// </summary>
        public override bool Equals(object obj)
        {
            return (obj != null && (obj is Cube) && this.uid == ((Cube)obj).uid);
        }


        /// <summary>
        /// Overrides == operator based on uid
        /// </summary>
        public static bool operator ==(Cube c1, Cube c2)
        {
            return (((object)c1 == null && (object)c2 == null)
                ||  ((object)c1 != null && c1.Equals(c2)));
        }


        /// <summary>
        /// Overrides != operator based on uid
        /// </summary>
        public static bool operator !=(Cube c1, Cube c2)
        {
            return !(c1 == c2);
        }
    }
}
