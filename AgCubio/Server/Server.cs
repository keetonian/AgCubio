using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AgCubio
{
    /// <summary>
    /// 
    /// </summary>
    class Server
    {
        // I'd argue for a hashset.
        private HashSet<Socket> Sockets;

        // Cool. We own the world.
        private World World;

        // Our Uid counter
        private int Uid;

        //Previously used Uid's that can now be reused (cubes were deleted)
        private Stack<int> Uids;

        //Random number generator
        private Random RandomNumber;

        //Timer that controls updates
        private Timer Heartbeat;

        //Do we need this?
        private StringBuilder DataReceived;

        //Do we need this?
        private StringBuilder DataSent;


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
            RandomNumber = new Random();
            DataSent = new StringBuilder();
            DataReceived = new StringBuilder();

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

            //For original UID's: have a counter that counts up and gives unique uid's
            //When a cube is removed, store the uid in a stack to be reused.
            //If there is nothing on the stack, increment the counter and use that UID.
            //IF there is something on the stack, pop it and use that as the uid.


            // Generate 2 random starting coords within our world, check if other players are there, then send if player won't get eaten immediately. (helper method)
            double x, y;
            FindStartingCoords(out x, out y);
            Cube cube = new Cube(x, y, GetUid(), false, state.data, World.PLAYER_START_MASS, GetColor(), 0);

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

            //Network.Send(state.socket, DataSent.ToString());
            double x, y;
            FindStartingCoords(out x, out y);
            //Network.Send(state.socket, JsonConvert.SerializeObject(new Cube(x, y, GetUid(), true, "", World.FOOD_MASS, GetColor(), 0)));
        }


        /// <summary>
        /// Finds starting coordinates for a new player cube so that it isn't immediately consumed
        /// </summary>
        private void FindStartingCoords(out double x, out double y)
        {
            //Implement this
            x = RandomNumber.Next((int)World.PLAYER_START_WIDTH, World.WIDTH - (int)World.PLAYER_START_WIDTH);
            y = RandomNumber.Next((int)World.PLAYER_START_WIDTH, World.HEIGHT - (int)World.PLAYER_START_WIDTH);

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
            return RandomNumber.Next(Int32.MinValue, Int32.MaxValue);
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

                if (World.MAX_FOOD_COUNT > World.Food.Count)
                {
                    double x, y;
                    FindStartingCoords(out x, out y);
                    Cube c = new Cube(x, y, GetUid(), true, "", World.FOOD_MASS, GetColor(), 0);
                    World.Food.Add(c);

                    data.Append(JsonConvert.SerializeObject(c) + "\n");
                }
                //Appends all of the player cube data- 
                //They should be constantly changing, therefore, we send them every time.
                data.Append(World.SerializePlayers());
                
            }

            //Needs a way to send all cubes that were destroyed with a mass of 0, which is a more advanced game mechanic.


            //Would this be where we send the data? It's easy to do it here, granted, but shouldn't it be in the callback?
            lock(Sockets)
            {
                foreach (Socket s in Sockets)
                {
                    //Do we need to thread this, or is it automatically threaded by virtue of just using sockets?
                    Network.Send(s, data.ToString());
                }
            }
        }
    }
}