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
        /// Maximum total military viruses in the world (integer 0-)
        /// </summary>
        public readonly int MAX_MILITARY_VIRUS_COUNT;

        /// <summary>
        /// Maximum total food cubes in the world (integer 0-)
        /// </summary>
        public readonly int MAX_FOOD_COUNT;

        /// <summary>
        /// Maximum food to add to the world during each heartbeat tick (integer 0-)
        /// </summary>
        public readonly int FOOD_PER_HEARTBEAT;

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
        /// Mass at which minimum speed is reached
        /// </summary>
        public readonly int MIN_SPEED_MASS;

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
        public readonly double ATTRITION_RATE;

        /// <summary>
        /// Minimum mass before a player can spit (integer 0-)
        /// </summary>
        public readonly int MIN_SPLIT_MASS;

        /// <summary>
        /// How far a cube can be thrown when split (integer 0-)
        /// Note: this is a time distance, so how long a cube can go at an increased speed in the direction the mouse points.
        /// </summary>
        public readonly int MAX_SPLIT_DISTANCE;

        /// <summary>
        /// Maximum total cubes a single player is allowed (integer 0-)
        /// </summary>
        public readonly int MAX_SPLIT_COUNT;

        // We feel we don't need this - instead, overlapping the center of another cube is good enough for now
        /*/// <summary>
        /// Distance between cubes before a larger eats a smaller
        /// </summary>
        public readonly double ABSORB_DISTANCE_DELTA;*/

        // ---------------------- OTHER FIELDS --------------------------

        /// <summary>
        /// (Client): Dictionary for storing all the cubes. Uid's map to cubes
        /// (Server): Stores only player cubes
        /// </summary>
        public Dictionary<int, Cube> Cubes;

        /// <summary>
        /// (Server): Keeps track of all food.
        /// </summary>
        public HashSet<Cube> Food;

        /// <summary>
        /// (Server): Dictionary for tracking split cubes
        ///   Dictionary1: Team id, Dictionary2
        ///   Dictionary2: Cube id, Tuple
        ///   Tuple: Location where split cube will be going (add an inertia to it)
        /// </summary>
        private Dictionary<int, Dictionary<int, SplitCubeData>> SplitCubeUids;


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

        /// <summary>
        /// int = uid
        /// double = angle
        /// </summary>
        private Dictionary<int, MilitaryVirusData> MilitaryViruses;

        // --------------------------------------------------------------


        /// <summary>
        /// Constructs a new world using parameters specified in an xml file
        /// </summary>
        public World(string filename)
        {
            // Initialize fields
            SplitCubeUids = new Dictionary<int, Dictionary<int, SplitCubeData>>();
            Cubes = new Dictionary<int, Cube>();
            Food = new HashSet<Cube>();
            Rand = new Random();
            Uids = new Stack<int>();
            Uid = 1; // Start at 1 so that no cube has a uid of 0
            MilitaryViruses = new Dictionary<int, MilitaryVirusData>();

            // Read parameters from xml
            using (XmlReader reader = XmlReader.Create(filename))
            {
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

                            case "max_speed":
                                reader.Read();
                                double.TryParse(reader.Value, out this.MAX_SPEED);
                                break;

                            case "min_speed":
                                reader.Read();
                                double.TryParse(reader.Value, out this.MIN_SPEED);
                                break;

                            case "attrition_rate":
                                reader.Read();
                                double.TryParse(reader.Value, out this.ATTRITION_RATE);
                                break;

                            case "food_mass":
                                reader.Read();
                                int.TryParse(reader.Value, out this.FOOD_MASS);
                                break;

                            case "player_start_mass":
                                reader.Read();
                                int.TryParse(reader.Value, out this.PLAYER_START_MASS);
                                break;

                            case "max_food_count":
                                reader.Read();
                                int.TryParse(reader.Value, out this.MAX_FOOD_COUNT);
                                break;

                            case "max_military_virus_count":
                                reader.Read();
                                int.TryParse(reader.Value, out this.MAX_MILITARY_VIRUS_COUNT);
                                break;

                            case "food_per_heartbeat":
                                reader.Read();
                                int.TryParse(reader.Value, out this.FOOD_PER_HEARTBEAT);
                                break;

                            case "min_split_mass":
                                reader.Read();
                                int.TryParse(reader.Value, out this.MIN_SPLIT_MASS);
                                break;

                            case "max_split_count":
                                reader.Read();
                                int.TryParse(reader.Value, out this.MAX_SPLIT_COUNT);
                                break;

                            /*case "absorb_constant":
                                reader.Read();
                                double.TryParse(reader.Value, out this.ABSORB_DISTANCE_DELTA);
                                break;*/

                            case "heartbeats_per_second":
                                reader.Read();
                                int.TryParse(reader.Value, out this.HEARTBEATS_PER_SECOND);
                                break;

                            case "virus_percent":
                                reader.Read();
                                int.TryParse(reader.Value, out this.VIRUS_PERCENT);
                                break;

                            case "virus_mass":
                                reader.Read();
                                int.TryParse(reader.Value, out this.VIRUS_MASS);
                                break;

                            case "min_speed_mass":
                                reader.Read();
                                int.TryParse(reader.Value, out this.MIN_SPEED_MASS);
                                break;
                        }
                    }
                }
            }

            // Calculate remaining parameters
            PLAYER_START_WIDTH = Math.Sqrt(PLAYER_START_MASS);
            FOOD_WIDTH         = Math.Sqrt(FOOD_MASS);
            VIRUS_WIDTH        = Math.Sqrt(VIRUS_MASS);
            SPEED_SLOPE        = (MIN_SPEED - MAX_SPEED) / (MIN_SPEED_MASS - PLAYER_START_MASS);
            SPEED_CONSTANT     = (MAX_SPEED - MIN_SPEED * (PLAYER_START_MASS / MIN_SPEED_MASS)) / (1 - PLAYER_START_MASS / MIN_SPEED_MASS);

            // Generate starting food
            while (Food.Count < MAX_FOOD_COUNT)
                GenerateFoodorVirus();

            // Generate military viruses
            for (int i = 0; i < MAX_MILITARY_VIRUS_COUNT; i++)
                GenerateMilitaryVirus();
            }


        /// <summary>
        /// Much simpler constructor, for client use
        /// </summary>
        public World()
        {
            Cubes = new Dictionary<int, Cube>();
        }


        /// <summary>
        /// Serializes all cubes in the world to send them to a new player
        /// </summary>
        public string SerializeAllCubes()
        {
            StringBuilder info = new StringBuilder();

            foreach (Cube c in Food)
                info.Append(JsonConvert.SerializeObject(c) + "\n");

            return info.Append(SerializePlayers()).ToString();
        }


        /// <summary>
        /// Serializes all players
        ///   Players should all be (almost) constantly changing
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
                if ((c.Mass * (1 - ATTRITION_RATE)) > PLAYER_START_MASS)
                    c.Mass *= (1 - ATTRITION_RATE);
        }


        /// <summary>
        /// Manages cubes colliding against each other -- WAAAAAAAY TO COMPLICATED
        /// </summary>
        public string ManageCollisions()
        {
            StringBuilder destroyed = new StringBuilder();
            List<int> eatenPlayers  = new List<int>();
            List<Cube> eatenFood;

            // Get a data structure that can be used in a loop easily
            List<Cube> playerList = new List<Cube>(Cubes.Values);

            // Using for loops to make the algorithm a little less costly - check each player cube only once against each other
            for (int i = 0; i < playerList.Count; i++)
            {
                Cube player = playerList[i];

                if (player.Mass == 0)
                    continue;

                eatenFood = new List<Cube>();

                // Check against all of the food cubes
                //   There has to be a faster way of doing this
                foreach (Cube food in Food) // THIS IS WHERE THE MOST TIME IS SPENT IN THIS CODE. THIS FOREACH LOOP- SPECIFICALLY THE NEXT IF STATEMENT FOR COLLISION CHECKING
                {
                    if (player.Collides(food) && player.Mass > food.Mass && food.Mass != 0) // Added food.Mass != 0 check because there might sometime happen where two players hit the same food cube at the same time
                    {
                        if (food.Mass == VIRUS_MASS)
                        {
                            if (player.food) // Military virus
                                player.Mass += food.Mass;
                            else
                                VirusSplit(player.uid, food.loc_x, food.loc_y);
                        }
                        else
                            player.Mass += food.Mass;

                        // Adjust cube position if edges go out of bounds
                        AdjustPosition(player.uid);

                        food.Mass = 0;
                        destroyed.Append(JsonConvert.SerializeObject(food) + "\n");
                        eatenFood.Add(food);
                    }
                }

                // Nested loop - check against all player cubes after the current cube
                for (int j = i + 1; j < playerList.Count; j++)
                {
                    Cube player2 = playerList[j];

                    // Check if player has already been consumed in this collisions check
                    if (player2.Mass == 0)
                        continue;

                    // Check if there will be a collision between player cubes
                    if (player.Collides(player2) || player2.Collides(player))
                    {
                        // Check if players are part of a split (same team)
                        if (player.Team_ID != 0 && player.Team_ID == player2.Team_ID)
                        {
                            // Merge into the cube that has the focus (Uid == Team_ID)
                            Cube focus = (player.uid == player.Team_ID) ? player  : player2;
                            Cube other = (player.uid == player.Team_ID) ? player2 : player;

                            // But split cubes cannot merge unless their cooloff periods have expired
                            if (SplitCubeUids[focus.Team_ID][focus.uid].Cooloff > 0 || SplitCubeUids[focus.Team_ID][other.uid].Cooloff > 0) //BUG! KEYNOTFOUND EXCPTION WHEN REMERGING- other.uid did not exist.
                                continue;

                            focus.Mass += other.Mass;
                            other.Mass = 0;
                            AdjustPosition(focus.uid);

                            eatenPlayers.Add(other.uid);
                            SplitCubeUids[other.Team_ID].Remove(other.uid);
                            destroyed.Append(JsonConvert.SerializeObject(other) + "\n");
                        }

                        // If a player has over 120% mass of another player, it can eat the other player
                        else if (player.Mass / player2.Mass >= 1.2 || player2.Mass / player.Mass >= 1.2)
                        {
                            Cube predator = (player.Mass / player2.Mass >= 1.2) ? player  : player2;
                            Cube prey     = (player.Mass / player2.Mass >= 1.2) ? player2 : player;

                            int id = prey.uid;
                            predator.Mass += prey.Mass;

                            if(prey.food) // If prey is a military virus...
                            {
                                if (predator.food) // ...and predator is a military virus, predator eats
                                    predator.Mass += prey.Mass;

                                // Otherwise predator splits
                                VirusSplit(predator.uid, prey.loc_x, prey.loc_y);
                            }

                            prey.Mass = 0;
                            AdjustPosition(predator.uid);

                            // If the eaten player cube is part of a split team, reassign the focus cube or remove the member's split id, as necessary
                            if (SplitCubeUids.ContainsKey(prey.Team_ID) && SplitCubeUids[prey.Team_ID].Count > 1)
                            {
                                if (prey.uid == prey.Team_ID)
                                    ReassignUid(prey.uid, ref id);
                                else
                                    SplitCubeUids[prey.Team_ID].Remove(prey.uid);
                            }

                            eatenPlayers.Add(id);
                            destroyed.Append(JsonConvert.SerializeObject(Cubes[id]) + "\n");
                        }
                    }
                }

                // Remove eaten food
                foreach (Cube c in eatenFood)
                {
                    Food.Remove(c);
                    Uids.Push(c.uid);
                }
            }

            // Remove eaten players
            foreach (int i in eatenPlayers)
            {
                Cubes.Remove(i);
                Uids.Push(i);

                if (MilitaryViruses.Remove(i))
                    GenerateMilitaryVirus();
            }

            return destroyed.ToString();
        }


        /// <summary>
        /// Reassigns the uid-bearing split cube (for when the original is going to be eaten)
        /// </summary>
        private void ReassignUid(int teamID, ref int standInUid)
        {
            Cube original = Cubes[teamID];

            // Iterate through split cubes
            foreach (int uid in SplitCubeUids[teamID].Keys)
            {
                // At the first new uid, swap important cube info so that another cube gets eaten in the original's place
                if (uid != teamID && Cubes[uid].Mass != 0)
                {
                    Cube standIn = Cubes[uid];

                    double x = original.loc_x;
                    double y = original.loc_y;
                    double mass = original.Mass;

                    original.loc_y = standIn.loc_y;
                    original.loc_x = standIn.loc_x;
                    original.Mass = standIn.Mass;

                    standIn.loc_x = x;
                    standIn.loc_y = y;
                    standIn.Mass = mass;

                    // Remove the stand-in's uid from the team
                    SplitCubeUids[teamID].Remove(uid);
                    standInUid = uid;
                    return;
                }
            }
        }


        /// <summary>
        /// Adjust coordinates so that a player cannot leave the world
        /// </summary>
        private void AdjustPosition(int uid)
        {
            Cube player = Cubes[uid];

            if (player.left <= 0)
                player.loc_x = (1 + player.width/2);
            else if (player.right >= this.WORLD_WIDTH)
                player.loc_x = this.WORLD_WIDTH - (1 + player.width / 2);
            if (player.top <=  0)
                player.loc_y = (1 + player.width / 2);
            else if (player.bottom >= this.WORLD_HEIGHT)
                player.loc_y = this.WORLD_HEIGHT - (1 + player.width / 2);
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
        /// Generates a military virus
        /// </summary>
        public void GenerateMilitaryVirus()
        {
            double x = Rand.Next((int)(WORLD_WIDTH*.05), (int)(WORLD_WIDTH - WORLD_WIDTH*.05));
            double y = Rand.Next((int)(WORLD_HEIGHT*.05), (int)(WORLD_HEIGHT - WORLD_HEIGHT*.05));

            Cube mVirus = new Cube(x, y, GetUid(), true, "", VIRUS_MASS, Color.Red.ToArgb(), 0);
            Cubes.Add(mVirus.uid, mVirus);
            double angle = Rand.Next(720) * 2 * Math.PI / 180;
            MilitaryViruses.Add(mVirus.uid, new MilitaryVirusData(mVirus.loc_x, mVirus.loc_y, angle));
        }


        /// <summary>
        /// Finds starting coordinates for a new player (or virus) cube so that it isn't immediately consumed (or exploded)
        /// </summary>
        public void FindStartingCoords(out double x, out double y, bool virus)
        {
            // Set width based on whether the cube is a virus or player
            double width = virus ? VIRUS_WIDTH : PLAYER_START_WIDTH;

            // Assign random coordinates
            x = Rand.Next((int)width, WORLD_WIDTH - (int)width);
            y = Rand.Next((int)width, WORLD_HEIGHT - (int)width);

            // Retry if new cube would be overlapped by another cube
            foreach (Cube player in Cubes.Values)
                if (player.Overlaps(new Cube(x, y, width*width)))
                    FindStartingCoords(out x, out y, virus);
        }


        /// <summary>
        /// Helper method: creates a unique uid to give a cube
        /// </summary>
        public int GetUid()
        {
            return (Uids.Count > 0) ? Uids.Pop() : Uid++;
        }


        /// <summary>
        /// Helper method: gives a cube a nice, vibrant, visible color
        /// </summary>
        public int GetColor()
        {
            return ~(Rand.Next(Int32.MinValue, Int32.MaxValue) & 0xf0f0f0);
        }


        /// <summary>
        /// Adds a new food or virus cube to the world
        /// </summary>
        public Cube GenerateFoodorVirus()
        {
            int random = Rand.Next(100);
            int color, mass, width;
            double x, y;

            // Create a virus some percent of the time
            if (random < VIRUS_PERCENT)
            {
                color = Color.LightGreen.ToArgb();
                mass  = VIRUS_MASS;
                width = (int)VIRUS_WIDTH;

                // Make sure viruses can't spawn on top of players
                FindStartingCoords(out x, out y, true);
            }
            // Otherwise create food
            else
            {
                color = GetColor();
                mass = (96 < random && random < 99) ? FOOD_MASS * 2 : ((random > 99) ? FOOD_MASS * 3 : FOOD_MASS); // 3% of food is double size, 1% of food is triple size
                x = Rand.Next((int)FOOD_WIDTH, WORLD_WIDTH  - (int)FOOD_WIDTH);
                y = Rand.Next((int)FOOD_WIDTH, WORLD_HEIGHT - (int)FOOD_WIDTH);
            }

            Cube foodOrVirus = new Cube(x, y, GetUid(), true, "", mass, color, 0);
            Food.Add(foodOrVirus);
            return foodOrVirus;
        }


        /// <summary>
        /// Moves the military viruses in a four-leaf clover-shaped patrol
        /// </summary>
        public void MoveMilitaryVirus()
        {
            foreach (int uid in new List<int>(MilitaryViruses.Keys))
            {
                double angle = MilitaryViruses[uid].Angle;
                angle += ((Math.PI) / 180); 

                // Do a grade according to angle
                //   goes in a clover shape
                if (angle > 4 * Math.PI)
                    angle = 0;

                MilitaryViruses[uid].Angle = angle;

                if (angle < 2 * Math.PI)
                {
                    Cubes[uid].loc_x = MilitaryViruses[uid].X + (WORLD_WIDTH*.02 * Math.Sin(angle * 2));
                    Cubes[uid].loc_y = MilitaryViruses[uid].Y + (WORLD_HEIGHT*.07 * Math.Sin(angle));
                }
                else
                {
                    Cubes[uid].loc_x = MilitaryViruses[uid].X + (WORLD_WIDTH*.07 * Math.Sin(angle));
                    Cubes[uid].loc_y = MilitaryViruses[uid].Y + (WORLD_HEIGHT*.02 * Math.Sin(angle * 2));
                }
            }
        }


        /// <summary>
        /// Controls a cube's movements
        /// </summary>
        public void Move(int PlayerUid, double x, double y)
        {
            // Check if there are split cubes for the player
            if (SplitCubeUids.ContainsKey(PlayerUid) && SplitCubeUids[PlayerUid].Count > 1)
            {
                foreach (int uid in new List<int>(SplitCubeUids[PlayerUid].Keys))
                {
                    // Decrement the split cooloff period
                    SplitCubeUids[PlayerUid][uid].Cooloff--;

                    if (SplitCubeUids[PlayerUid][uid].Countdown > 0)
                    {
                        MoveSplitCube(uid, x, y);
                        continue;
                    }
                    else
                        MoveCube(uid, x, y);

                    List<int> temp2 = new List<int>(SplitCubeUids[PlayerUid].Keys);
                    foreach (int team in temp2)
                    {
                        if (uid == team)
                            continue;

                        if ((SplitCubeUids[PlayerUid][uid].Cooloff > 0 || SplitCubeUids[PlayerUid][team].Cooloff > 0) && SplitCubeUids[PlayerUid][team].Countdown <= 0)
                            CorrectOverlap(Cubes[uid], Cubes[team]);
                    }
                }
            }
            // Normal movement:
            else
                MoveCube(PlayerUid, x, y);
        }

        /// <summary>
        /// Moves a split cube if it still has time left on it's countdown.
        /// </summary>
        public void MoveSplitCube(int CubeUid, double x, double y)
        {
            // Get the actual cube, decrement the countdown
            Cube cube = Cubes[CubeUid];
            SplitCubeUids[cube.Team_ID][cube.uid].Countdown--;

            double cubeWidth = Cubes[CubeUid].width;

            // Get direction, in vector form
            double xx = SplitCubeUids[cube.Team_ID][cube.uid].X;
            double yy = SplitCubeUids[cube.Team_ID][cube.uid].Y;

            x -= cube.loc_x;
            y -= cube.loc_y;  
            UnitVector(ref x, ref y);
                        
            double speed = GetSpeed(CubeUid);

            // Normalize and scale the vector:
            UnitVector(ref xx, ref yy);
            xx *= (speed * 3);
            yy *= (speed * 3);

            // Set the new position, make sure it's within the world boundaries.
            Cubes[CubeUid].loc_x += (cube.left + xx < 0 || cube.right + yy > this.WORLD_WIDTH) ? 0 : xx;
            Cubes[CubeUid].loc_y += (cube.top + yy < 0 || cube.bottom + xx > this.WORLD_HEIGHT) ? 0 : yy;
            AdjustPosition(CubeUid);
        }


        /// <summary>
        /// Helper method - checks for overlap between split cubes and cancels the directional movement that causes overlap
        /// </summary>
        public void CorrectOverlap(Cube moving, Cube teammate)
        {
            if (moving.Overlaps(teammate))
            {
                double relativeX = moving.loc_x - teammate.loc_x;
                double relativeY = moving.loc_y - teammate.loc_y;
                double relative  = Math.Abs(relativeX) - Math.Abs(relativeY);

                // Set moving cube's coordinates so slightly-overlapped cube edges touch instead
                if (relative < 0)
                    moving.loc_y = (relativeY > 0) ? teammate.bottom + moving.width / 2 : teammate.top - moving.width / 2;
                else if (relative > 0)
                    moving.loc_x = (relativeX > 0) ? teammate.right + moving.width / 2  : teammate.left - moving.width / 2;
                else
                {
                    moving.loc_x = (relativeX > 0) ? teammate.right + moving.width / 2  : teammate.left - moving.width / 2;
                    moving.loc_y = (relativeY > 0) ? teammate.bottom + moving.width / 2 : teammate.top - moving.width / 2;
                }
            }
        }


        /// <summary>
        /// Helper method for move, moves split cubes as well
        /// TODO: Needs to check for boundaries of cubes, not allow them to occupy the same spaces.
        /// </summary>
        private void MoveCube(int CubeUid, double x, double y)
        {
            // Get the actual cube
            Cube cube = Cubes[CubeUid];
            double cubeWidth = Cubes[CubeUid].width;

            // Get the relative mouse position:
            x -= cube.loc_x;
            y -= cube.loc_y;

            // If the mouse is in the very center of the cube, then don't do anything.
            if (Math.Abs(x) < 1 && Math.Abs(y) < 1)
                return;

            double speed = GetSpeed(CubeUid);

            // Normalize and scale the vector:
            UnitVector(ref x, ref y);
            x *= speed;
            y *= speed;

            // Set the new position
            Cubes[CubeUid].loc_x += (cube.left + x < 0 || cube.right + x > this.WORLD_WIDTH) ? 0 : x;
            Cubes[CubeUid].loc_y += (cube.top + y < 0 || cube.bottom + y > this.WORLD_HEIGHT) ? 0 : y;
            AdjustPosition(CubeUid);
        }


        /// <summary>
        /// Gets the speed of the cube
        /// </summary>
        public double GetSpeed(int CubeUid)
        {
            double speed = SPEED_SLOPE * Cubes[CubeUid].Mass + SPEED_CONSTANT;
            return (speed < MIN_SPEED) ? MIN_SPEED : ((speed > MAX_SPEED) ? MAX_SPEED : speed);
        }


        /// <summary>
        /// Manages split requests
        /// </summary>
        public void Split(int CubeUid, double x, double y)
        {
            // Check if a split team exists
            if (!SplitCubeUids.ContainsKey(CubeUid))
            {
                // Check if the cube is big enough to split
                if (Cubes[CubeUid].Mass < MIN_SPLIT_MASS)
                    return;

                // Set up a split team
                Cubes[CubeUid].Team_ID = CubeUid;
                SplitCubeUids[CubeUid] = new Dictionary<int, SplitCubeData>();
                SplitCubeUids[CubeUid][CubeUid] = new SplitCubeData(Cubes[CubeUid].loc_y, Cubes[CubeUid].loc_y, 0);
            }

            List<int> temp = new List<int>(SplitCubeUids[CubeUid].Keys);
            foreach (int uid in temp)
            {
                if (SplitCubeUids[CubeUid].Count >= MAX_SPLIT_COUNT)
                    continue;

                double mass = Cubes[uid].Mass;

                if (mass < MIN_SPLIT_MASS)
                    continue;

                // Halve the mass of the original cube, create a new cube
                Cubes[uid].Mass = mass / 2;

                // Get the directional vector
                double xx = x - Cubes[uid].loc_x;
                double yy = y - Cubes[uid].loc_y;
                UnitVector(ref xx, ref yy);

                // For use in putting into SplitCubeUids at the very bottom of this method. This is the direction vector the cube needs to keep going
                double xxx = xx;
                double yyy = yy;

                // Get a starting position for the cube that isn't right in the center of the original cube and is in the direction that it needs to go.
                xx = Cubes[uid].loc_x + (xx * Cubes[uid].width / 2);
                yy = Cubes[uid].loc_y + (yy * Cubes[uid].width / 2);

                Cube newCube = new Cube(xx, yy, GetUid(), false, Cubes[CubeUid].Name, mass / 2, Cubes[CubeUid].argb_color, CubeUid);

                // Add the new cube to the world
                Cubes.Add(newCube.uid, newCube);

                SplitCubeUids[CubeUid][newCube.uid] = new SplitCubeData(xxx, yyy, MAX_SPLIT_DISTANCE);
            }
            }



        /// <summary>
        /// Manages splitting when hit a virus
        /// </summary>
        public void VirusSplit(int CubeUid, double x, double y)
        {
            Cube cube = Cubes[CubeUid];

            // If the cube's team id is not yet set and tracked in SplitCubeUids, set it up (because the cube is about to split)
            if (!SplitCubeUids.ContainsKey(cube.Team_ID))
            {
                Cubes[CubeUid].Team_ID = CubeUid;
                SplitCubeUids[CubeUid] = new Dictionary<int, SplitCubeData>();
                SplitCubeUids[CubeUid][CubeUid] = new SplitCubeData(cube.loc_x, cube.loc_y, 0);
            }

            // Store the team id, and number of split cubes
            int teamID = cube.Team_ID;
            int numSplitCubes = SplitCubeUids[teamID].Count;

            // If at max split count, just add on virus mass and return
            if (numSplitCubes >= this.MAX_SPLIT_COUNT)
            {
                cube.Mass += VIRUS_MASS;
                return;
            }

            // Store the original cube mass
            double mass = cube.Mass;
            
            // Find the number of split cubes to make
            int maxSplits = (int)(mass / PLAYER_START_MASS);
            int numSplits = (maxSplits > MAX_SPLIT_COUNT - numSplitCubes) ? MAX_SPLIT_COUNT - numSplitCubes + 1 : maxSplits + 1; // +1 to account for reassigning original cube

            // Find the leftover mass (if all split cubes get the player starting mass)
            double leftoverMass = mass - (numSplits * PLAYER_START_MASS);

            // Generate all but the last split cube (the last will 'replace' the original later)
            while (numSplits > 1)
            {
                Cube newCube = GenerateSplitCube(ref numSplits, ref leftoverMass, teamID, CubeUid, x, y);

                double xx = newCube.loc_x - cube.loc_x;
                double yy = newCube.loc_y - cube.loc_y;
                UnitVector(ref xx, ref yy);

                // Add the new cube in to the world and the split set, and adjust its position
                Cubes.Add(newCube.uid, newCube);
                SplitCubeUids[teamID][newCube.uid] = new SplitCubeData(xx, yy, MAX_SPLIT_DISTANCE);
                AdjustPosition(newCube.uid);
            }

            // Alter the original cube to be a split cube now
            Cube replacement = GenerateSplitCube(ref numSplits, ref leftoverMass, teamID, CubeUid, x, y);
            cube.Mass  = replacement.Mass;
            cube.loc_x = replacement.loc_x;
            cube.loc_y = replacement.loc_y;
            AdjustPosition(cube.uid);
        }


        /// <summary>
        /// Generates a (virus-resultant) split cube:
        ///   random mass (greater than player start mass, but plus a random remainder portion of the original cube mass, based on how many splits there will be)
        ///   random start coordinates (along a circle surrounding the original virus position)
        /// </summary>
        private Cube GenerateSplitCube(ref int numSplits, ref double leftoverMass, int teamID, int CubeUid, double x, double y)
        {
            // Randomly decide whether to add to or subtract from original coordinates
            int xdir = Rand.Next(2) * 2 - 1;
            int ydir = Rand.Next(2) * 2 - 1;

            // Randomly allocate mass for the split cube
            double splitMass;

            if (--numSplits == 0) // Last cube gets all leftover mass
                splitMass = PLAYER_START_MASS + leftoverMass;
            else
            {
                double portion = 1.0 / Rand.Next(3, 10) * leftoverMass;
                splitMass = PLAYER_START_MASS + portion;
                leftoverMass -= portion;
            }

            double xDelta = Math.Sqrt(MAX_SPLIT_DISTANCE * MAX_SPLIT_DISTANCE - Rand.Next((int)(MAX_SPLIT_DISTANCE * MAX_SPLIT_DISTANCE)));
            double yDelta = Math.Sqrt(MAX_SPLIT_DISTANCE * MAX_SPLIT_DISTANCE - xDelta * xDelta);

            return new Cube(x + xDelta * xdir, y + yDelta * ydir, GetUid(), false, Cubes[CubeUid].Name, splitMass, Cubes[CubeUid].argb_color, teamID);
        }
        class MilitaryVirusData
        {
            public double X
            { get; set; }

            public double Y
            { get; set; }

            public double Angle
            { get; set; }

            public MilitaryVirusData(double x, double y, double angle)
            {
                X = x;
                Y = y;
                Angle = angle;
            }
        }

        /// <summary>
        /// Stores directional and cooldown data for split cubes
        /// </summary>
        class SplitCubeData
        {
            /// <summary>
            /// X directional vector
            /// </summary>
            public double X
            { get; set; }

            /// <summary>
            /// Y directional vector
            /// </summary>
            public double Y
            { get; set; }

            /// <summary>
            /// How long the cube goes on its trajectory
            /// </summary>
            public int Countdown
            { get; set; }

            /// <summary>
            /// Time before it can merge back to another cube.
            /// </summary>
            public int Cooloff
            { get; set; }


            /// <summary>
            /// Data for split cubes.
            /// </summary>
            public SplitCubeData(double x, double y, int countdown)
            {
                X = x;
                Y = y;
                Countdown = countdown;
                Cooloff = 800;
            }
        }
    }
}