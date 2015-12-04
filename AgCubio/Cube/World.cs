// Created by Daniel Avery and Keeton Hodgson
// November 2015

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Newtonsoft.Json;
using System.Drawing;

namespace AgCubio
{
    /// <summary>
    /// The world model for AgCubio
    /// </summary>
    public class World
    {
        // ---------------------- WORLD ATTRIBUTES ----------------------

        /// <summary>
        /// Width of the world (integer 0-)
        /// </summary>
        public readonly int WORLD_WIDTH;

        /// <summary>
        /// Height of the world (integer 0-)
        /// </summary>
        public readonly int WORLD_HEIGHT;

        /// <summary>
        /// Number of updates the server attemps to execute per second (integer 0-)
        /// </summary>
        public readonly int HEARTBEATS_PER_SECOND;

        // -------------------- FOOD/VIRUS ATTRIBUTES -------------------

        /// <summary>
        /// Default mass of food cubes (integer 0-)
        /// </summary>
        public readonly int FOOD_MASS;

        /// <summary>
        /// Default width of food cubes
        /// </summary>
        public readonly double FOOD_WIDTH;

        /// <summary>
        /// Default mass of viruses (integer 0-)
        /// </summary>
        public readonly int VIRUS_MASS;

        /// <summary>
        /// Default width of viruses
        /// </summary>
        public readonly double VIRUS_WIDTH;

        /// <summary>
        /// Percent of food that becomes a virus (integer 0-100)
        /// </summary>
        public readonly int VIRUS_PERCENT;

        /// <summary>
        /// Maximum total food cubes in the world (integer 0-)
        /// </summary>
        public readonly int MAX_FOOD_COUNT;

        // ---------------------- PLAYER ATTRIBUTES ---------------------

        /// <summary>
        /// Starting mass for all players (integer 0-)
        /// </summary>
        public readonly int PLAYER_START_MASS;

        /// <summary>
        /// Starting width of players
        /// </summary>
        public readonly double PLAYER_START_WIDTH;

        /// <summary>
        /// Maximum player speed - for small cube sizes (integer 0-10)
        /// </summary>
        public readonly double MAX_SPEED;

        /// <summary>
        /// Minimum player speed - for large cube sizes (integer 0-10)
        /// </summary>
        public readonly double MIN_SPEED;

        /// <summary>
        /// Constant in the linear equation for calculating a cube's speed
        /// </summary>
        public readonly double SPEED_CONSTANT;

        /// <summary>
        /// Slope in the linear equation for calculating a cubes's speed
        /// </summary>
        public readonly double SPEED_SLOPE;

        /// <summary>
        /// Scaler for how quickly a player cube loses mass (double 0-1)
        /// </summary>
        public readonly double ATTRITION_RATE_SCALER;

        /// <summary>
        /// Minimum mass before a player can spit (integer 0-)
        /// </summary>
        public readonly int MIN_SPLIT_MASS;

        /// <summary>
        /// How far a cube can be thrown when split (integer 0-)
        /// </summary>
        public readonly int MAX_SPLIT_DISTANCE;

        /// <summary>
        /// Maximum total cubes a single player is allowed (integer 0-)
        /// </summary>
        public readonly int MAX_SPLIT_COUNT;

        /// <summary>
        /// Distance between cubes before a larger eats a smaller
        /// </summary>
        public readonly double ABSORB_DISTANCE_DELTA;

        // ---------------------- OTHER ATTRIBUTES ----------------------

        /// <summary>
        /// Dictionary for storing all the cubes. Uid's map to cubes
        /// </summary>
        public Dictionary<int, Cube> Cubes;

        /// <summary>
        /// Keeps track of all food.
        /// </summary>
        public HashSet<Cube> Food;

        /// <summary>
        /// Dictionary for tracking split cubes
        /// </summary>
        private Dictionary<int, List<int>> SplitCubeUids;

        /// <summary>
        /// Our Uid counter
        /// </summary>
        private int Uid;

        /// <summary>
        /// Previously used Uid's that can now be reused (cubes were deleted)
        /// </summary>
        private Stack<int> Uids;

        /// <summary>
        /// Random number generator
        /// </summary>
        private Random Rand;

        // --------------------------------------------------------------


        /// <summary>
        /// Constructs a new world of the specified dimensions in the xml file
        /// </summary>
        public World(string filename)
        {
            SplitCubeUids = new Dictionary<int, List<int>>();
            Cubes = new Dictionary<int, Cube>();
            Food = new HashSet<Cube>();
            Rand = new Random();
            Uids = new Stack<int>();
            
            using (XmlReader reader = XmlReader.Create(filename))
            {
                //TODO: implement xml file stuff.
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        switch (reader.Name)
                        {
                            case "width":
                                reader.Read();
                                int.TryParse(reader.Value, out this.WORLD_WIDTH);
                                break;

                            case "height":
                                reader.Read();
                                int.TryParse(reader.Value, out this.WORLD_HEIGHT);
                                break;

                            case "max_split_distance":
                                reader.Read();
                                int.TryParse(reader.Value, out this.MAX_SPLIT_DISTANCE);
                                break;

                            case "top_speed":
                                reader.Read();
                                double.TryParse(reader.Value, out this.MAX_SPEED);
                                break;

                            case "low_speed":
                                reader.Read();
                                double.TryParse(reader.Value, out this.MIN_SPEED);
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
                                double.TryParse(reader.Value, out this.ABSORB_DISTANCE_DELTA);
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
            this.FOOD_WIDTH = Math.Sqrt(this.FOOD_MASS);
            //temporary - should be in xml
            this.VIRUS_MASS = 30;
            this.VIRUS_PERCENT = 2;
            this.VIRUS_WIDTH = Math.Sqrt(this.VIRUS_MASS);

            double maxSpeedMass = 1 / (10 * ATTRITION_RATE_SCALER);
            this.SPEED_SLOPE = (MIN_SPEED - MAX_SPEED) / (maxSpeedMass - PLAYER_START_MASS);
            this.SPEED_CONSTANT = (MAX_SPEED - MIN_SPEED * (PLAYER_START_MASS / maxSpeedMass)) / (1 - PLAYER_START_MASS / maxSpeedMass);

            while (this.Food.Count < this.MAX_FOOD_COUNT)
                this.GenerateFoodorVirus();
        }


        /// <summary>
        /// Default constructor: uses predetermined values
        /// </summary>
        public World()
        {
            SplitCubeUids = new Dictionary<int, List<int>>();
            Cubes = new Dictionary<int, Cube>();
            Food = new HashSet<Cube>();
            Rand = new Random();
            Uids = new Stack<int>();

            this.ABSORB_DISTANCE_DELTA = 5;
            this.ATTRITION_RATE_SCALER = .0005;
            this.FOOD_MASS = 1;
            this.FOOD_WIDTH = Math.Sqrt(this.FOOD_MASS);
            this.HEARTBEATS_PER_SECOND = 30;
            this.WORLD_HEIGHT = 1000;
            this.WORLD_WIDTH = 1000;
            this.MAX_FOOD_COUNT = 5000;
            this.MAX_SPEED = 1;
            this.MAX_SPLIT_COUNT = 15;
            this.MAX_SPLIT_DISTANCE = 30;
            this.MIN_SPEED = 0.4;
            this.MIN_SPLIT_MASS = 25;
            this.PLAYER_START_MASS = 10;
            this.PLAYER_START_WIDTH = Math.Sqrt(this.PLAYER_START_MASS);
            this.VIRUS_MASS = 30;
            this.VIRUS_PERCENT = 2;
            this.VIRUS_WIDTH = Math.Sqrt(this.VIRUS_MASS);

            double maxSpeedMass = 1 / (10 * ATTRITION_RATE_SCALER);
            this.SPEED_SLOPE    = (MIN_SPEED - MAX_SPEED) / (maxSpeedMass - PLAYER_START_MASS);
            this.SPEED_CONSTANT = (MAX_SPEED - MIN_SPEED * (PLAYER_START_MASS / maxSpeedMass)) / (1 - PLAYER_START_MASS / maxSpeedMass);

            while (this.Food.Count < this.MAX_FOOD_COUNT)
                this.GenerateFoodorVirus();
        }


        /// <summary>
        /// Serializes all cubes in the world to send them to a new player.
        /// </summary>
        public string SerializeAllCubes()
        {
            StringBuilder info = new StringBuilder();
            foreach(Cube c in Food)
                info.Append(JsonConvert.SerializeObject(c) + "\n");

            info.Append(SerializePlayers());

            return info.ToString();
        }


        /// <summary>
        /// Serializes all players.
        /// </summary>
        public string SerializePlayers()
        {
            StringBuilder players = new StringBuilder();

            foreach (Cube c in Cubes.Values)
                players.Append(JsonConvert.SerializeObject(c) + "\n");

            return players.ToString();
        }


        /// <summary>
        /// Atrophy! Players decrease in size
        /// </summary>
        public void PlayerAttrition()
        {
            foreach (Cube c in Cubes.Values)
                if ((c.Mass * (1 - ATTRITION_RATE_SCALER)) > this.PLAYER_START_MASS)
                    c.Mass *= (1 - this.ATTRITION_RATE_SCALER);
        }


        /// <summary>
        /// Manages cubes colliding against each other
        /// </summary>
        public string ManageCollisions()
        {
            StringBuilder destroyed = new StringBuilder();
            List<Cube> eatenFood;
            List<int> eatenPlayers = new List<int>();
            // 3 Parts:
            // Players and Food (Check!)
            // Players and Players
            //  -- If a split cube of a player dies, and it is the one with that player's uid as its' uid, then we need to assign another one of that player's cubes to that players uid.
            // Players and Viruses

            // There has to be a faster way of doing this, as this is very slow and costly.
            // Use a different storing method for food? Store food by location on the screen, then we just check local areas?
            // Or is there a different solution? Paralell foreach? Other?
            foreach (Cube player in Cubes.Values)
            {
                eatenFood = new List<Cube>();
                if (player.Mass == 0)
                    continue;

                foreach (Cube food in Food)
                {
                    if (food.loc_x > player.left && food.loc_x < player.right && food.loc_y > player.top && food.loc_y < player.bottom && player.Mass > food.Mass)
                    {
                        player.Mass += food.Mass;

                        // Adjust cube position if edges go out of bounds
                        AdjustPosition(player.uid);

                        food.Mass = 0;
                        destroyed.Append(JsonConvert.SerializeObject(food) + "\n");
                        Uids.Push(food.uid);
                        eatenFood.Add(food);
                    }
                }

                // BEGINNING IMPLEMENTATION: EATING PLAYERS. NOT DONE YET.
                // DOESNT LIKE NESTING FOREACH LOOPS AND MODIFYING THIS INSIDE OF THE FOREACH. FIND A BETTER IMPLEMENTATION.
                // WE COULD NEST FOR LOOPS, SINCE WE ARE NOT DELETING ANYTHING, JUST MODIFYING DATA.
                /*
                foreach(Cube players in Cubes.Values)
                {
                    if (player.uid == players.uid)
                        continue;
                    if (players.loc_x > player.left && players.loc_x < player.right && players.loc_y > player.top && players.loc_y < player.bottom && player.Mass > players.Mass)
                    {
                        // IF TEAMID = UID and COUNTDOWN < 0, then the player can eat its own split cube
                        // NOT IMPLEMENTED
                        if(player.Mass > players.Mass)
                        {
                            player.Mass += players.Mass;
                            players.Mass = 0;
                            destroyed.Append(JsonConvert.SerializeObject(players) + "\n");
                            AdjustPosition(player.uid);
                            eatenPlayers.Add(players.uid); //NEEDS TO REMOVE THESE
                            Uids.Push(players.uid);// ONLY IF THIS IS THE LAST CUBE OF THE PLAYER OR ITS NOT THEIR UNIQUE ID

                            //IF PLAYER HAS OTHER CUBES, then assign uid if this is the special cube
                        }
                        else
                        {
                            players.Mass += player.Mass;
                            player.Mass = 0;
                            destroyed.Append(JsonConvert.SerializeObject(player) + "\n");
                            AdjustPosition(players.uid);
                            eatenPlayers.Add(player.uid); // NEEDS TO REMOVE THESE

                            Uids.Push(player.uid);// ONLY IF THIS IS THE LAST CUBE OF THE PLAYER OR ITS NOT THEIR UNIQUE ID
                            //IF PLAYER HAS OTHER CUBES, then assign PLAYERuid if this is the special cube with the main uid (teamid)
                        }

                        // Adjust cube position if edges go out of bounds

                    }
                }*/

                // Remove eaten food and players.
                foreach (Cube c in eatenFood)
                    Food.Remove(c);
                foreach (int i in eatenPlayers)
                    Cubes.Remove(i);

            }

            return destroyed.ToString();
        }


        /// <summary>
        /// 
        /// </summary>
        private void AdjustPosition(int uid)
        {
            Cube player = Cubes[uid];
            if (player.left < 0)
                player.loc_x -= player.left;
            else if (player.right > this.WORLD_WIDTH)
                player.loc_x -= player.right - this.WORLD_WIDTH;
            if (player.top < 0)
                player.loc_y -= player.top;
            else if (player.bottom > this.WORLD_HEIGHT)
                player.loc_y -= player.bottom - this.WORLD_HEIGHT;
        }


        /// <summary>
        /// Creates a unit vector out of the given x and y coordinates
        /// </summary>
        public static void UnitVector(ref double x, ref double y)
        {
            double scale = Math.Sqrt(x * x + y * y);
            x /= scale;
            y /= scale;
        }


        /// <summary>
        /// Finds starting coordinates for a new player cube so that it isn't immediately consumed
        /// </summary>
        public void FindStartingCoords(out double x, out double y)
        {
            // Assign random coordinates
            x = Rand.Next((int)PLAYER_START_WIDTH, WORLD_WIDTH - (int)PLAYER_START_WIDTH);
            y = Rand.Next((int)PLAYER_START_WIDTH, WORLD_HEIGHT - (int)PLAYER_START_WIDTH);

            // Retry if coordinates are contained by any other player cube
            foreach (Cube player in Cubes.Values)
                if ((x > player.left && x < player.right) && (y < player.bottom && y > player.top))
                    FindStartingCoords(out x, out y);
        }


        /// <summary>
        /// Helper method: creates a unique uid to give a cube
        /// NOTE: Move this to world?
        /// </summary>
        public int GetUid()
        {
            return (Uids.Count > 0) ? Uids.Pop() : Uid++;
        }


        /// <summary>
        /// Gives the cube a nice, vibrant, visible color
        /// </summary>
        public int GetColor()
        {
            return ~(Rand.Next(Int32.MinValue, Int32.MaxValue) & 0xf0f0f0);
        }


        /// <summary>
        /// Adds a new food cube to the world
        /// </summary>
        public Cube GenerateFoodorVirus()
            {
            // On a random scale needs to create viruses too (5% of total food? Less?)
            // Viruses: specific color, specific size or size range. I'd say a size of ~100 or so.
            // Cool thought: viruses can move, become npc's that can try to chase players, or just move erratically

            //Another thought: randomly allow a food piece to get 1 size bigger (mass++) each time this is called.

            int random = Rand.Next(100);
            bool virus = (random < VIRUS_PERCENT);

            //create a virus 1% of the time
            int color  = virus ? Color.LightGreen.ToArgb() : GetColor();
            int mass   = virus ? VIRUS_MASS  : ((random > 99) ? FOOD_MASS + 1 : FOOD_MASS);
            int width  = virus ? (int)VIRUS_WIDTH : (int)FOOD_WIDTH;

            Cube foodOrVirus = new Cube(Rand.Next(width, WORLD_WIDTH - width), Rand.Next(width, WORLD_HEIGHT - width), GetUid(), true, "", mass, color, 0);
            Food.Add(foodOrVirus);
            return foodOrVirus;
        }


        /// <summary>
        /// Controls a cube's movements
        /// NOTE: This method needs to be controlled by the heartbeat.
        /// NOTE: THis method needs to have bounds (so player can't go outside of the world).
        /// </summary>
        public void Move(int PlayerUid, double x, double y)
        {
            if (SplitCubeUids.ContainsKey(PlayerUid) && SplitCubeUids[PlayerUid].Count > 0)
                foreach (int uid in SplitCubeUids[PlayerUid])
                    MoveCube(uid, x, y);
            else
                MoveCube(PlayerUid, x, y);
        }


        /// <summary>
        /// Helper method for move, moves split cubes as well
        /// TODO: Needs to check for boundaries of cubes, not allow them to occupy the same spaces.
        /// </summary>
        private void MoveCube(int CubeUid, double x, double y)
        {
            // Store cube width
            double cubeWidth = Cubes[CubeUid].width;

            // Get the relative mouse position:
            x -= Cubes[CubeUid].loc_x;
            y -= Cubes[CubeUid].loc_y;

            // If the mouse is in the very center of the cube, then don't do anything.
            if (Math.Abs(x) < 1 && Math.Abs(y) < 1)
                return;

            double speed = SPEED_SLOPE * Cubes[CubeUid].Mass + SPEED_CONSTANT;
            speed = (speed < MIN_SPEED) ? MIN_SPEED : ((speed > MAX_SPEED) ? MAX_SPEED : speed);

            // Normalize and scale the vector:
            UnitVector(ref x, ref y);
            x *= speed;
            y *= speed;

            // Add normalized values to the cube's location. 
            // TODO: add in updates according to the heartbeat, and add in a speed scalar.

            Cubes[CubeUid].loc_x += (Cubes[CubeUid].left + x < 0 || Cubes[CubeUid].right + x > this.WORLD_WIDTH) ? 0 : x;
            Cubes[CubeUid].loc_y += (Cubes[CubeUid].top + y < 0 || Cubes[CubeUid].bottom + y > this.WORLD_HEIGHT) ? 0 : y;
        }


        /// <summary>
        /// Manages split requests
        /// </summary>
        public void Split(int CubeUid, double x, double y)
        {
            if (!SplitCubeUids.ContainsKey(CubeUid))
            {
                if (Cubes[CubeUid].Mass < this.MIN_SPLIT_MASS)
                    return;
                
                SplitCubeUids[CubeUid] = new List<int>() { CubeUid };
            }

            int[] temp = SplitCubeUids[CubeUid].ToArray();
            foreach (int uid in temp)
            {
                if (SplitCubeUids[CubeUid].Count >= this.MAX_SPLIT_COUNT)
                    return;
                double mass = Cubes[uid].Mass;
                if (mass < this.MIN_SPLIT_MASS)
                    continue;

                // Halve the mass of the original cube, create a new cube
                Cubes[uid].Mass = mass / 2;
                Cube newCube = new Cube(x, y, GetUid(), false, Cubes[CubeUid].Name, mass / 2, Cubes[CubeUid].argb_color, CubeUid);

                // Add the new cube to the world
                Cubes.Add(newCube.uid, newCube);

                SplitCubeUids[CubeUid].Add(newCube.uid);
            }

            
            // Assign a teamid- that is the original uid
            // Still have a cube with original uid
            // 
            // Needs another data structure that keeps track of all of this player's cubes
            // These cubes need to be able to split again if they can, all of them.
            // Somehow needs to be able to merge back together as well at some point.
            // Perhaps in some data structure, have a stopwatch with each new cube? After the stopwatch hits some amount, that specific cube can merge back in if it is touching another cube?

            /*From Website:
            Timer: When a cube splits it should have a time set. 
            The cube should be marked as not being allowed to merge until the time elapses.

            Momentum: When a cube splits, it should not immediately jump to the final "split point", 
            but should instead have a momentum that moves it smoothly toward that spot for a short period of time.
            */

        }


        /// <summary>
        /// 
        /// </summary>
        class SplitCubeData
        {
            /// <summary>
            /// Unit vector direction
            /// </summary>
            public Tuple<double, double> direction;

            /// <summary>
            /// Speed that it is going at- needs to decrease quick with time
            /// </summary>
            public double inertia;
            
            /// <summary>
            /// Countdown until it can merge again
            /// </summary>
            public int countdown
            {
                get { return countdown--; }
                set { }
            }

            /// <summary>
            /// 
            /// </summary>
            public int CubeUid;

            /// <summary>
            /// Passed in so that member variables can be accessed.
            /// </summary>
            World Parent;

            /* Design notes:
            Has an initial direction, multiplied by an initial scalar that decreases quickly. Mouse directions after this initial are added on to it, with their speed.
            Has a max distance of where the cube can split to- that split needs to happen quickly, and quickly scale back down to normal speed once that spot is attained.
            Everything needs to happen gradually.
            Work out the storing of data here, and the actual moving in the Move() method in the World class.

            This data structure needs to be stored in the hash set that right now just stores the splitcubeuid's.

            */

            /// <summary>
            /// 
            /// </summary>
            public SplitCubeData(double x, double y, World parent, int cubeUid)
            {
                inertia = 2;//initial speed
                countdown = 200; //decremented each tick
                Parent = parent;
                int dist = Parent.MAX_SPLIT_DISTANCE;
                CubeUid = cubeUid;

                x -= Parent.Cubes[CubeUid].loc_x;
                y -= Parent.Cubes[CubeUid].loc_y;

                inertia = 2;//-(1 * Parent.Cubes[CubeUid].Mass / 58) + 59 / 58; // NEED A NEW SPEED SCALAR

                UnitVector(ref x, ref y);
                direction = new Tuple<double, double>(x, y);
            }
        }
    }
}