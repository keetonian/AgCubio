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
        /// Width, derived as the (almost) square root of the width (adjusted slightly to deal with a temporary faulty server)
        /// </summary>
        public double width
        {
            get { return Math.Pow(Mass,0.65); }
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
        /// Overrides hash code- gets the uid instead.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.uid;
        }
    }
}
