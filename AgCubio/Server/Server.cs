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
    public class Server
    {
        // Another option: dictionary, where the socket is the Key, a stringbuilder the value, 
        // and we can just constantly put stuff into it and in the networking loop it automatically sends what we want.
        // This way we won't need to send stuff in other places in this code.
        // Socket, data to send to client, move + split requests gathered from client,
        private HashSet<Socket> Sockets;

        // Cool. We own the world.
        private World World;

        //Timer that controls updates
        private Timer Heartbeat;

        //Do we need this?
        //This could be what I want:
        // Dictionary
        //      Key: player uid
        //      Value: a new class containing split requests, move requests (each in new data)
        private Dictionary<int, Tuple<double, double>> DataReceived;

        //Do we need this?
        private StringBuilder DataSent;


        public static void Main(string[] args)
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

            DataSent = new StringBuilder();
            DataReceived = new Dictionary<int, Tuple<double, double>>();

            //Start the client loop
            Network.Server_Awaiting_Client_Loop(new Network.Callback(SetUpClient));
        }


        /// <summary>
        /// Main callback method for setting up a client.
        /// </summary>
        private void SetUpClient(Preserved_State_Object state)
        {
            lock (Sockets)
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
            World.FindStartingCoords(out x, out y);
            Cube cube = new Cube(x, y, World.GetUid(), false, state.data.ToString(), World.PLAYER_START_MASS, World.GetColor(), 0);

            string worldData;
            lock (World)
            {
                World.Cubes[cube.uid] = cube;
                worldData = World.SerializeAllCubes();
            }

            state.CubeID = cube.uid;
            state.data.Clear();
            state.callback = new Network.Callback(ManageData);

            // Send the client's cube and then all of the world data
            Network.Send(state.socket, JsonConvert.SerializeObject(cube) + "\n");
            Network.Send(state.socket, worldData);

            // Ask for more data from client
            Network.I_Want_More_Data(state);
        }


        /// <summary>
        /// Method to send and receive data from client
        /// NOTE: SERVER IS CURRENTLY SENDING ALL DATA FROM THE HeartBeatTick function. should this change to here? (Concern: threading is here, but is it there?)
        /// </summary>
        private void ManageData(Preserved_State_Object state)
        {

            Action<String> TryMoveOrSplit = new Action<String>((str) =>
            {
                MatchCollection values = Regex.Matches(str, @"-*\d+");
                double x = double.Parse(values[0].Value);
                double y = double.Parse(values[1].Value);

                if (str[1] == 'm')
                    lock (DataReceived) { DataReceived[state.CubeID] = new Tuple<double, double>(x, y); }
                else if (str[1] == 's')
                    lock (World) { World.Split(state.CubeID, x, y); }

            });


            string[] actions = Regex.Split(state.data.ToString(), @"\n");
            for (int i = 0; i < actions.Length - 1; i++)
                TryMoveOrSplit(actions[i]);

            string lastAction = actions.Last();
            if (lastAction.Length > 1 && lastAction?.Last() == ')')
                TryMoveOrSplit(lastAction);
            else
                state.data = new StringBuilder(lastAction);

            //Network.Send(state.socket, DataSent.ToString());

            Network.I_Want_More_Data(state);
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
                lock (DataReceived)
                {
                    foreach (int i in DataReceived.Keys)
                    {
                        World.Move(i, DataReceived[i].Item1, DataReceived[i].Item2);
                    }
                }

                //Check for collisions, eat some food.
                data.Append(World.ManageCollisions());

                // Add food to the world if necessary and append it to the data stream
                if (World.Food.Count < World.MAX_FOOD_COUNT)
                {
                    Cube food = World.GenerateFoodorVirus();
                    data.Append(JsonConvert.SerializeObject(food) + "\n");
                }

                //Appends all of the player cube data - they should be constantly changing (mass, position, or both), therefore, we send them every time
                data.Append(World.SerializePlayers());
            }

            //Would this be where we send the data? It's easy to do it here, granted, but shouldn't it be in the callback?
            lock (Sockets)
            {
                foreach (Socket s in Sockets)
                {
                    if (!s.Connected) // We need some way of getting rid of unconnected sockets.
                        continue;
                    //Do we need to thread this, or is it automatically threaded by virtue of just using sockets?
                    Network.Send(s, data.ToString());
                }

                // Alternative Route?
                //Parallel.ForEach<Socket>(Sockets, s => { Network.Send(s, data.ToString()); });
            }
        }
    }
}