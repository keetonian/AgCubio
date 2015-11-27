using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace AgCubio
{
    /// <summary>
    /// 
    /// </summary>
    class Server
    {
        // Another option: dictionary, where the socket is the Key, a stringbuilder the value, 
        // and we can just constantly put stuff into it and in the networking loop it automatically sends what we want.
        // This way we won't need to send stuff in other places in this code.
        private HashSet<Socket> Sockets;

        // Cool. We own the world.
        private World World;

        // Our Uid counter
        private int Uid;

        //Previously used Uid's that can now be reused (cubes were deleted)
        private Stack<int> Uids;

        //Random number generator
        private Random Rand;

        //Timer that controls updates
        private Timer Heartbeat;

        //Do we need this?
        private StringBuilder DataReceived;

        //Do we need this?
        private StringBuilder DataSent;

        //Queue to hold all the eaten food, to be sent and deleted
        private Queue<Cube> Deceased;


        static void Main(string[] args)
        {
            new Server();
            Console.ReadLine();
        }


        /// <summary>
        /// Constructor. Sets up the server
        /// </summary>
        public Server()
        {
            World = new World(); //Use the file path later.
            Sockets = new HashSet<Socket>();

            //Initialize many of our member variables.
            Heartbeat = new Timer(HeartBeatTick, null, 0, 1000 / World.HEARTBEATS_PER_SECOND);
            Uids = new Stack<int>();
            Rand = new Random();
            DataSent = new StringBuilder();
            DataReceived = new StringBuilder();
            Deceased = new Queue<Cube>();

            while (World.Food.Count < World.MAX_FOOD_COUNT)
                GenerateFood();

            //Start the client loop
            Network.Server_Awaiting_Client_Loop(new Network.Callback(SetUpClient));
        }


        /// <summary>
        /// Main callback method for setting up a client.
        /// </summary>
        private void SetUpClient(Preserved_State_Object state)
        {
            lock(Sockets)
            {
                Sockets.Add(state.socket);
            }

            Console.WriteLine("User " + state.data + " has connected to the server");

            //For original UID's: have a counter that counts up and gives unique uid's
            //When a cube is removed, store the uid in a stack to be reused.
            //If there is nothing on the stack, increment the counter and use that UID.
            //IF there is something on the stack, pop it and use that as the uid.


            // Generate 2 random starting coords within our world, check if other players are there, then send if player won't get eaten immediately. (helper method)
            double x, y;
            FindStartingCoords(out x, out y);
            Cube cube = new Cube(x, y, GetUid(), false, state.data, World.PLAYER_START_MASS, GetColor(), 0);
            state.CubeID = cube.uid;

            string worldData;
            lock(World)
            {
                World.Cubes.Add(cube.uid, cube);
                worldData = World.SerializeAllCubes();
            }

            state.callback = new Network.Callback(ManageData);

            //Sends the client's cube and then all of the world data.
            Network.Send(state.socket, JsonConvert.SerializeObject(cube) + "\n");
            Thread.Sleep(1000); // Temporary fix... Maybe. Do we even need this?

            Network.Send(state.socket, worldData);

            //Asks for more data from client.
            Network.I_Want_More_Data(state);
        }


        /// <summary>
        /// Method to send and receive data from client
        /// NOTE: SERVER IS CURRENTLY SENDING ALL DATA FROM THE HeartBeatTick function. should this change to here? (Concern: threading is here, but is it there?)
        /// </summary>
        private void ManageData(Preserved_State_Object state)
        {
            DataReceived.Append(state.data);

            Console.WriteLine(state.data);
            //"(move, " + PrevMouseLoc_x + ", " + PrevMouseLoc_y + ")\n";

            string[] actions = Regex.Split(state.data, @"\n");
            foreach(string s in actions)
            {
                if(s.ToUpper().Contains("MOVE"))
                {
                    try
                    {
                        int x, y;
                        MatchCollection values = Regex.Matches(s, @"\d+");
                        Move(state.CubeID, double.Parse(values[0].Value), double.Parse(values[1].Value));
                    }
                    catch (Exception)
                    { }

                }
            }

            //Network.Send(state.socket, DataSent.ToString());

            Network.I_Want_More_Data(state);
        }


        /// <summary>
        /// Finds starting coordinates for a new player cube so that it isn't immediately consumed
        /// </summary>
        private void FindStartingCoords(out double x, out double y)
        {
            //Implement this
            x = Rand.Next((int)World.PLAYER_START_WIDTH, World.WIDTH - (int)World.PLAYER_START_WIDTH);
            y = Rand.Next((int)World.PLAYER_START_WIDTH, World.HEIGHT - (int)World.PLAYER_START_WIDTH);

            //More complicated stuff looking at other players and what not. Recursion?
            if (true)
                return;
            else
                FindStartingCoords(out x, out y);
        }


        /// <summary>
        /// Helper method: creates a unique uid to give a cube
        /// </summary>
        /// <returns></returns>
        private int GetUid()
        {
            return (Uids.Count > 0) ? Uids.Pop() : Uid++;
        }


        /// <summary>
        /// Gives the cube a color
        /// </summary>
        /// <returns></returns>
        private int GetColor()
        {
            return Rand.Next(Int32.MinValue, Int32.MaxValue);
        }


        /// <summary>
        /// Every timer tick:
        ///     Updates the world (adds food, etc)
        ///     Sends updates to the clients
        /// </summary>
        private void HeartBeatTick(object state)
        {
            //TODO: Process move requests, split requests, and make the game mechanics (eating, etc) work.


            //Data to send to all of the clients
            StringBuilder data = new StringBuilder();

            lock (World)
            {
                // Players get a little smaller each tick
                World.PlayerAttrition();

                // Add food to the world if necessary and append it to the data stream
                if (World.Food.Count < World.MAX_FOOD_COUNT)
                {
                    Cube food = GenerateFood();
                    data.Append(JsonConvert.SerializeObject(food) + "\n");
                }

                // Arrange for eaten food to be sent and then removed
                while(this.Deceased.Count > 0)
                {
                    Cube fatality = Deceased.Dequeue();
                    data.Append(JsonConvert.SerializeObject(fatality) + "\n");
                    World.Food.Remove(fatality);
                }

                //Appends all of the player cube data - they should be constantly changing, therefore, we send them every time
                data.Append(World.SerializePlayers());
            }
            
            //Would this be where we send the data? It's easy to do it here, granted, but shouldn't it be in the callback?
            lock(Sockets)
            {
                foreach (Socket s in Sockets)
                {
                    //Do we need to thread this, or is it automatically threaded by virtue of just using sockets?
                    Network.Send(s, data.ToString());
                }

                // Alternative Route?
                //Parallel.ForEach<Socket>(Sockets, s => { Network.Send(s, data.ToString()); });
            }

            //Needs a way to send all cubes that were destroyed with a mass of 0, which is a more advanced game mechanic.
        }


        /// <summary>
        /// Adds a new food cube to the world
        /// </summary>
        public Cube GenerateFood()
        {
            // On a random scale needs to create viruses too (5% of total food? Less?)
            // Viruses: specific color, specific size or size range. I'd say a size of ~100 or so.
            // Cool thought: viruses can move, become npc's that can try to chase players, or just move erratically

            Cube food = new Cube(Rand.Next(World.WIDTH), Rand.Next(World.HEIGHT), GetUid(), true, "", World.FOOD_MASS, GetColor(), 0);
            World.Food.Add(food);
            return food;
        }


        /// <summary>
        /// Controls a cube's movements
        /// </summary>
        public void Move(int CubeUid, double x, double y)
        {
            // Get the relative mouse position:
            x = x - World.Cubes[CubeUid].loc_x;
            y = y - World.Cubes[CubeUid].loc_y;

            // If the mouse is in the very center of the cube, then don't do anything.
            if (Math.Abs(x) < 1 && Math.Abs(y) < 1)
                return;

            // Normalize the vector:
            double scale = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
            double newX = x / scale;
            double newY = y / scale;

            // Add normalized values to the cube's location. 
            // TODO: add in updates according to the heartbeat, and add in a speed scalar.
            World.Cubes[CubeUid].loc_x += newX;
            World.Cubes[CubeUid].loc_y += newY;
        }
    }
}