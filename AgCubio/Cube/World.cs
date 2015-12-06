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

        //We feel we don't need this- we feel that overlapping the center of another cube is good enough for now
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
        /// Dictionary1: Team id, Dictionary2
        /// Dictionary2: Cube id, Tuple
        /// Tuple: Location where split cube will be going (add an inertia to it)
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
        /// Tuple( (x,y) , angle)
        /// </summary>
        private Dictionary<int, Tuple<Tuple<double, double>, double>> MilitaryViruses;

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
            MilitaryViruses = new Dictionary<int, Tuple<Tuple<double, double>, double>>();

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
            this.PLAYER_START_WIDTH = Math.Sqrt(this.PLAYER_START_MASS);
            this.FOOD_WIDTH = Math.Sqrt(this.FOOD_MASS);
            this.VIRUS_WIDTH = Math.Sqrt(this.VIRUS_MASS);
            this.SPEED_SLOPE = (MIN_SPEED - MAX_SPEED) / (MIN_SPEED_MASS - PLAYER_START_MASS);
            this.SPEED_CONSTANT = (MAX_SPEED - MIN_SPEED * (PLAYER_START_MASS / MIN_SPEED_MASS)) / (1 - PLAYER_START_MASS / MIN_SPEED_MASS);

            // Generate starting food
            while (this.Food.Count < this.MAX_FOOD_COUNT)
                this.GenerateFoodorVirus();

            // Make our military viruses
            for (int i = 0; i < 4; i++)
            {
                CreateMilitaryVirus();

            }
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
        /// Serializes all players.
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
                if ((c.Mass * (1 - ATTRITION_RATE)) > this.PLAYER_START_MASS)
                    c.Mass *= (1 - this.ATTRITION_RATE);
        }


        /// <summary>
        /// Collisions - checks if one cube's center is overlapped by another cube
        ///   c1 is the potential predator
        ///   c2 is the potential prey
        /// </summary>
        private bool Collide(Cube c1, Cube c2)
        {
            if (c2.loc_x > c1.left && c2.loc_x < c1.right && c2.loc_y > c1.top && c2.loc_y < c1.bottom)
                return true;

            return false;
        }


        /// <summary>
        /// Manages cubes colliding against each other
        /// </summary>
        public string ManageCollisions()
        {
            StringBuilder destroyed = new StringBuilder();
            List<Cube> eatenFood;
            List<int> eatenPlayers = new List<int>();

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
                foreach (Cube food in Food)
                {
                    if (Collide(player, food) && player.Mass > food.Mass && food.Mass != 0) // Added food.Mass != 0 check because there might sometime happen where two players hit the same food cube at the same time
                    {
                        if (food.Mass == VIRUS_MASS)
                        {
                            if (player.food)
                                player.Mass += food.Mass;
                            else
                                VirusSplit(player.uid, food.loc_x + 10, food.loc_y + 10);
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

                // Nested loop - check against all player cubes after the current cube.
                for (int j = i + 1; j < playerList.Count; j++)
                {
                    Cube player2 = playerList[j];

                    // Check if player has already been consumed in this collisions check.
                    if (player.Mass == 0 || player2.Mass == 0)
                        continue;

                    // Check if their will be a collision between player cubes
                    if (Collide(player, player2) || Collide(player2, player))
                    {
                        // IF TEAMID = UID and COUNTDOWN < 0, then the player can eat its own split cube

                        // Check if players are part of a split (same team)
                        if (player.Team_ID != 0 && player.Team_ID == player2.Team_ID)
                        {
                            // Merge into the cube that has the focus (Uid == Team_ID)
                            Cube focus = (player.uid == player.Team_ID) ? player  : player2;
                            Cube other = (player.uid == player.Team_ID) ? player2 : player;
                            if (SplitCubeUids[focus.Team_ID][focus.uid].Cooloff > 0 || SplitCubeUids[focus.Team_ID][other.uid].Cooloff > 0)
                                continue;
                            focus.Mass += other.Mass;
                            other.Mass = 0;
                            AdjustPosition(focus.uid);

                            eatenPlayers.Add(other.uid);
                            SplitCubeUids[other.Team_ID].Remove(other.uid);
                            destroyed.Append(JsonConvert.SerializeObject(other) + "\n");
                        }

                        // If a player has over 120% mass of another player, it can eat the other player
                        else if (player.Mass / player2.Mass > 1.2 || player2.Mass / player.Mass > 1.2)
                        {
                            Cube predator = (player.Mass / player2.Mass > 1.2) ? player  : player2;
                            Cube prey     = (player.Mass / player2.Mass > 1.2) ? player2 : player;

                            int id = prey.uid;
                            predator.Mass += prey.Mass;
                            if(prey.food)
                            {
                                if (predator.food)
                                    predator.Mass += prey.Mass;
                                VirusSplit(predator.uid, prey.loc_x + 10, prey.loc_y + 10);
                            }
                            prey.Mass = 0;
                            AdjustPosition(predator.uid);

                            // If the eaten player cube is part of a split team, reassign the focus cube or remove the member's split id, as necessary
                            if (SplitCubeUids.ContainsKey(prey.Team_ID) && SplitCubeUids[prey.Team_ID].Count > 1)
                            {
                                if (prey.uid == prey.Team_ID)
                                    id = ReassignUid(prey.uid);
                                else
                                    SplitCubeUids[prey.Team_ID].Remove(prey.uid);
                            }

                            eatenPlayers.Add(id);
                            destroyed.Append(JsonConvert.SerializeObject(prey) + "\n");
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
                    CreateMilitaryVirus();
            }

            return destroyed.ToString();
        }


        /// <summary>
        /// If the player cube that has the original player id dies, then it needs to be reassigned 
        /// if the player has other cubes still split off.
        /// </summary>
        /// <returns>ID to remove</returns>
        private int ReassignUid(int cubeUid)
        {
            //TODO: MAKE THIS WORK
            // Iterate through split cubes
            foreach (int uid in SplitCubeUids[cubeUid].Keys)
            {
                // At the first new uid, swap uid's and exit
                if (uid != cubeUid)
                {
                    int otherID = Cubes[cubeUid].uid = Cubes[uid].uid;
                    Cubes[uid].uid = cubeUid;
                    SplitCubeUids[cubeUid].Remove(otherID);
                    return otherID;
                }
            }

            // Should never get here - there will always be multiple split cubes with unique uids when this method is called
            return cubeUid;
        }


        /// <summary>
        /// Makes sure the player cannot leave the world
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
        /// Generates a military virus.
        /// </summary>
        public void CreateMilitaryVirus()
        {
            double x = Rand.Next(50,this.WORLD_WIDTH - 50);
            double y = Rand.Next(50, this.WORLD_HEIGHT - 50);
            

                Cube mVirus = new Cube(x, y, GetUid(), true, "", VIRUS_MASS, Color.Red.ToArgb(), 0);
                Cubes.Add(mVirus.uid, mVirus);
                MilitaryViruses.Add(mVirus.uid, new Tuple<Tuple<double, double>, double>(new Tuple<double, double>(x, y), 0));
        }


        /// <summary>
        /// Finds starting coordinates for a new player (or virus) cube so that it isn't immediately consumed (or exploded)
        /// </summary>
        public void FindStartingCoords(out double x, out double y, bool virus)
        {
            double width = virus ? VIRUS_WIDTH : PLAYER_START_WIDTH;

            // Assign random coordinates
            x = Rand.Next((int)width, WORLD_WIDTH - (int)width);
            y = Rand.Next((int)width, WORLD_HEIGHT - (int)width);

            // Retry if coordinates are contained by any other player cube
            foreach (Cube player in Cubes.Values)
                if ((x > player.left && x < player.right) && (y < player.bottom && y > player.top))
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
        /// Gives the cube a nice, vibrant, visible color
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
                mass = VIRUS_MASS;
                width = (int)VIRUS_WIDTH;

                // Make sure viruses can't spawn on top of players
                FindStartingCoords(out x, out y, true);
            }
            // Otherwise create food
            else
            {
                color = GetColor();
                mass = (random > 960 && random < 990) ? FOOD_MASS * 2 : (random > 990) ? FOOD_MASS * 3 : FOOD_MASS; // 3% of food is double size, 1% of food is triple size
                x = Rand.Next((int)FOOD_WIDTH, WORLD_WIDTH - (int)FOOD_WIDTH);
                y = Rand.Next((int)FOOD_WIDTH, WORLD_HEIGHT - (int)FOOD_WIDTH);
            }

            Cube foodOrVirus = new Cube(x, y, GetUid(), true, "", mass, color, 0);
            Food.Add(foodOrVirus);
            random = Rand.Next(1000);
            if (random < 2)
                CreateMilitaryVirus();
            return foodOrVirus;
        }


        /// <summary>
        /// Moves the military viruses in a clover shape.
        /// </summary>
        public void MilitaryVirusMove()
        {
            List<int> keys = new List<int>(MilitaryViruses.Keys);
            foreach (int i in keys)
            {
                double x = MilitaryViruses[i].Item1.Item1;
                double y = MilitaryViruses[i].Item1.Item2;
                double angle = MilitaryViruses[i].Item2;
                angle += ((2 * Math.PI) / 180);

                //Do a grade according to angle
                // Goes in a clover shape
                if (angle > 4 * Math.PI)
                    angle = 0;

                MilitaryViruses[i] = new Tuple<Tuple<double, double>, double>(new Tuple<double, double>(x, y), angle);

                if (angle < 2 * Math.PI)
                {
                    x += (20 * Math.Sin(angle * 2));
                    y += (70 * Math.Sin(angle));

                    Cubes[i].loc_x = x;
                    Cubes[i].loc_y = y;
                }
                else
                {
                    x += (70 * Math.Sin(angle));
                    y += (20 * Math.Sin(angle * 2));

                    Cubes[i].loc_x = x;
                    Cubes[i].loc_y = y;
                }
               
            }
        }


        /// <summary>
        /// Controls a cube's movements
        /// </summary>
        public void Move(int PlayerUid, double x, double y)
        {
            if (SplitCubeUids.ContainsKey(PlayerUid) && SplitCubeUids[PlayerUid].Count > 1)
            {
                List<int> temp = new List<int>(SplitCubeUids[PlayerUid].Keys);
                foreach (int uid in temp)
                {
                    double x0 = Cubes[uid].loc_x;
                    double y0 = Cubes[uid].loc_y;
                    SplitCubeUids[PlayerUid][uid].Cooloff--;

                    if (SplitCubeUids[Cubes[uid].Team_ID][uid].Countdown > 0)
                        MoveSplitCube(uid, x, y);
                    else
                    MoveCube(uid, x, y);

                    List<int> temp2 = new List<int>(SplitCubeUids[PlayerUid].Keys);
                    foreach (int team in temp2)
                    {
                        if (uid == team)
                            continue;
                        if (SplitCubeUids[PlayerUid][team].Cooloff > 0)
                            CheckOverlap(uid, Cubes[team], x0, y0);
                        else
                            if(SplitCubeUids[PlayerUid][uid].Cooloff > 0)
                                CheckOverlap(uid, Cubes[team], x0, y0);
                    }
                }
            }
            else
                MoveCube(PlayerUid, x, y);
        }

        /// <summary>
        /// Moves a split cube if it still has time left on it's countdown.
        /// </summary>
        public void MoveSplitCube(int CubeUid, double x, double y)
        {

            // Get the actual cube
            Cube cube = Cubes[CubeUid];
            SplitCubeUids[cube.Team_ID][cube.uid].Countdown--;

            double cubeWidth = Cubes[CubeUid].width;

            //Direction, in vector form
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
        }


        /// <summary>
        /// Helper method - checks for overlap between split cubes and cancels the directional movement that causes overlap
        /// x0 and y0 are the original positions
        /// </summary>
        public void CheckOverlap(int movingUid, Cube teammate, double x0, double y0)
        {
            Cube moving = Cubes[movingUid];

            if (((moving.left < teammate.right && moving.left > teammate.left) // THis is when the left side of the moving cube intersects with the right side of another cube
                || (moving.right < teammate.right && moving.right > teammate.left)) // This is when the right side of the moving cube intersects with the left side of another cube
                && ((moving.top < teammate.bottom && moving.top > teammate.top) // This is when the top of the moving cube is higher than the bottom of another cube
                || (moving.bottom < teammate.bottom && moving.bottom > teammate.top))) // This is when the bottom of the moving cube is lower than the top of another cube
            {
                double relativeX = moving.loc_x - teammate.loc_x;
                double relativeY = moving.loc_y - teammate.loc_y;

                if (relativeX == 0)
                { }   //Do nothing
                else if (relativeX < 0)
                    Cubes[movingUid].loc_x = teammate.left - (moving.width / 2);
                else
                    Cubes[movingUid].loc_x = teammate.right + (moving.width / 2);

                if(relativeY == 0)
                { } // Do nothing
                else if (relativeY < 0)
                    Cubes[movingUid].loc_y = teammate.top - (moving.width/2);
                else
                    Cubes[movingUid].loc_y = teammate.bottom + (moving.width / 2);
                AdjustPosition(movingUid);

                //FALLACIES:
                // 1. Does not take into consideration different sizes of cubes: a cube could have both top and bottom within the top and bottom of another cube
                // 2. One size fits all: the if checks need to be broken up more, a more broad approach is needed
                // For now, this will do.
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
            if (!SplitCubeUids.ContainsKey(CubeUid)) // Should only need to go by the CubeID, right? Only the player can call a split?
            {
                if (Cubes[CubeUid].Mass < this.MIN_SPLIT_MASS)
                    return;

                Cubes[CubeUid].Team_ID = CubeUid;
                SplitCubeUids[CubeUid] = new Dictionary<int, SplitCubeData>();
                SplitCubeUids[CubeUid][CubeUid] = new SplitCubeData(Cubes[CubeUid].loc_y, Cubes[CubeUid].loc_y, 0);
            }


            List<int> temp = new List<int>(SplitCubeUids[CubeUid].Keys);
            foreach (int uid in temp)
            {
                if (SplitCubeUids[CubeUid].Count >= this.MAX_SPLIT_COUNT)
                    continue;

                double mass = Cubes[uid].Mass;

                if (mass < this.MIN_SPLIT_MASS)
                    continue;

                // Halve the mass of the original cube, create a new cube
                Cubes[uid].Mass = mass / 2;

                //## NOTE: NEEDS TO NOT PUT THE CUBE ALREADY AT X,Y. ##
                double xx = x - Cubes[uid].loc_x;
                double yy = y - Cubes[uid].loc_y;
                UnitVector(ref xx, ref yy);

                // For use in putting into SplitCubeUids at the very bottom of this method. This is the direction vector the cube needs to go
                double xxx = xx;
                double yyy = yy;

                xx = (xx < 0) ? Cubes[uid].left - (Cubes[uid].width + 1 / 2) : Cubes[uid].right + (Cubes[uid].width + 1 / 2);
                yy = (yy < 0) ? Cubes[uid].top - (Cubes[uid].width + 1 / 2) : Cubes[uid].bottom + (Cubes[uid].width + 1 / 2);

                Cube newCube = new Cube(xx, yy, GetUid(), false, Cubes[CubeUid].Name, mass / 2, Cubes[CubeUid].argb_color, CubeUid);

                // Add the new cube to the world
                Cubes.Add(newCube.uid, newCube);

                SplitCubeUids[CubeUid][newCube.uid] = new SplitCubeData(xxx, yyy, this.MAX_SPLIT_DISTANCE);
            }
        }



        /// <summary>
        /// Manages splitting when hit a virus
        ///   Non optimal code
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

                double xx = newCube.loc_x - Cubes[CubeUid].loc_x;
                double yy = newCube.loc_y - Cubes[CubeUid].loc_y;
                UnitVector(ref xx, ref yy);
                // Add the new cube in to the world and the split set, and adjust its position
                Cubes.Add(newCube.uid, newCube);
                SplitCubeUids[teamID][newCube.uid] = new SplitCubeData(xx, yy, this.MAX_SPLIT_DISTANCE);
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