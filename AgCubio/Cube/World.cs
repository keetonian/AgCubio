// Created by Daniel Avery and Keeton Hodgson
// November 2015

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Newtonsoft.Json;

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
        public readonly int WIDTH;

        /// <summary>
        /// Height of the world
        /// </summary>
        public readonly int HEIGHT;

        /// <summary>
        /// Number of updates the server attemps to execute per second
        /// </summary>
        public readonly int HEARTBEATS_PER_SECOND;

        /// <summary>
        /// Maximum player speed (for small cube sizes)
        /// </summary>
        public readonly int MAX_SPEED;

        /// <summary>
        /// Minimum player speed (for large cube sizes)
        /// </summary>
        public readonly int MIN_SPEED;

        /// <summary>
        /// Scaler for how quickly a player cube loses mass
        /// </summary>
        public readonly double ATTRITION_RATE_SCALER;

        /// <summary>
        /// Default mass of food cubes
        /// </summary>
        public readonly int FOOD_MASS;

        /// <summary>
        /// Starting mass for all players
        /// </summary>
        public readonly int PLAYER_START_MASS;

        /// <summary>
        /// Maximum total food cubes in the world
        /// </summary>
        public readonly int MAX_FOOD_COUNT;

        /// <summary>
        /// Minimum mass before a player can spit
        /// </summary>
        public readonly int MIN_SPLIT_MASS;

        /// <summary>
        /// How far a cube can be thrown when split
        /// </summary>
        public readonly int MAX_SPLIT_DISTANCE;

        /// <summary>
        /// Maximum total cubes a single player is allowed
        /// </summary>
        public readonly int MAX_SPLIT_COUNT;

        /// <summary>
        /// Percent overlap before a larger cube eats a smaller
        /// </summary>
        public readonly double ABSORB_PERCENT_COVERAGE;

        /// <summary>
        /// Starting width of players
        /// </summary>
        public readonly double PLAYER_START_WIDTH;

        /// <summary>
        /// Dictionary for storing all the cubes. Uid's map to cubes
        /// </summary>
        public Dictionary<int,Cube> Cubes { get; set; }

        /// <summary>
        /// Keeps track of all food.
        /// </summary>
        public HashSet<Cube> Food { get; set; }


        /// <summary>
        /// Constructs a new world of the specified dimensions in the xml file
        /// </summary>
        public World(string filename)
        {
            Cubes = new Dictionary<int,Cube>();
            Food = new HashSet<Cube>();
            using (XmlReader reader = XmlReader.Create(filename))
            {
                //TODO: implement xml file stuff.
                while(reader.Read())
                {
                    if(reader.IsStartElement())
                    {
                        switch (reader.Name)
                        {
                            case "width":
                                reader.Read();
                                int.TryParse(reader.Value,out this.WIDTH);
                                break;

                            case "height":
                                reader.Read();
                                int.TryParse(reader.Value, out this.HEIGHT);
                                break;

                            case "max_split_distance":
                                reader.Read();
                                int.TryParse(reader.Value, out this.MAX_SPLIT_DISTANCE);
                                break;

                            case "top_speed":
                                reader.Read();
                                int.TryParse(reader.Value, out this.MAX_SPEED);
                                break;

                            case "low_speed":
                                reader.Read();
                                int.TryParse(reader.Value, out this.MIN_SPEED);
                                break;

                            case "attrition_rate":
                                reader.Read();
                                double.TryParse(reader.Value, out this.ATTRITION_RATE_SCALER);
                                break;

                            case "food_value":
                                reader.Read();
                                int.TryParse(reader.Value, out this.FOOD_MASS);
                                break;

                            case "player_start_mass":
                                reader.Read();
                                int.TryParse(reader.Value, out this.PLAYER_START_MASS);
                                break;

                            case "max_food":
                                reader.Read();
                                int.TryParse(reader.Value, out this.MAX_FOOD_COUNT);
                                break;

                            case "min_split_mass":
                                reader.Read();
                                int.TryParse(reader.Value, out this.MIN_SPLIT_MASS);
                                break;

                            case "absorb_constant":
                                reader.Read();
                                double.TryParse(reader.Value, out this.ABSORB_PERCENT_COVERAGE);
                                break;
                            /*Good idea: Shall we implement it?
                            case "max_view_range":
                                reader.Read();
                                int.TryParse(reader.Value, out this.HEIGHT);
                                break;
                            */
                            case "heartbeats_per_second":
                                reader.Read();
                                int.TryParse(reader.Value, out this.HEARTBEATS_PER_SECOND);
                                break;
                        }
                    }
                }
            }

            this.PLAYER_START_WIDTH = Math.Sqrt(this.PLAYER_START_MASS);
        }


        /// <summary>
        /// Default constructor: uses predetermined values
        /// </summary>
        public World()
        {
            Cubes = new Dictionary<int, Cube>();
            Food = new HashSet<Cube>();
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
            this.PLAYER_START_WIDTH = Math.Sqrt(this.PLAYER_START_MASS);
        }


        /// <summary>
        /// Serializes all cubes in the world to send them to a new player.
        /// </summary>
        public string SerializeAllCubes()
        {
            StringBuilder info = new StringBuilder();
            foreach(Cube c in Food)
            {
                info.Append(JsonConvert.SerializeObject(c) + "\n");
            }

            info.Append(SerializePlayers());

            return info.ToString();
        }


        /// <summary>
        /// Serializes all players.
        /// </summary>
        /// <returns></returns>
        public string SerializePlayers()
        {
            StringBuilder players = new StringBuilder();
            foreach (Cube c in Cubes.Values)
            {
                players.Append(JsonConvert.SerializeObject(c) + "\n");
            }
            return players.ToString();
        }


        /// <summary>
        /// Atrophy! Players decrease in size
        /// </summary>
        public void PlayerAttrition()
        {
            foreach(Cube c in Cubes.Values)
            {
                c.Mass = c.Mass - c.Mass * this.ATTRITION_RATE_SCALER;
            }
        }
    }
}
