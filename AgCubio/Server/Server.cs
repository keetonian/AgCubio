using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace AgCubio
{
    /// <summary>
    /// Server manages all data interactions with clients
    /// </summary>
    public class Server
    {
        /// <summary>
        /// Client connections
        /// </summary>
        private List<Socket> Sockets;

        /// <summary>
        /// World object for storing and managing the game state
        /// </summary>
        private World World;

        /// <summary>
        /// Timer that controls updates
        /// </summary>
        private Timer Heartbeat;

        /// <summary>
        /// Move requests for players, updated with each timer tick
        ///   cube uid => coordinates
        /// </summary>
        private Dictionary<int, Tuple<double, double>> DataReceived;


        /// <summary>
        /// Create a server and pause the console
        /// </summary>
        public static void Main(string[] args)
        {
            new Server();
            Console.ReadLine();
        }


        /// <summary>
        /// Constructor - initializes server fields and begins awaiting client connections
        /// </summary>
        public Server()
        {
            World        = new World("..\\..\\..\\Project1/Resources/World_Params.xml");
            Sockets      = new List<Socket>();
            Heartbeat    = new Timer(HeartBeatTick, null, 0, 1000 / World.HEARTBEATS_PER_SECOND);
            DataReceived = new Dictionary<int, Tuple<double, double>>();

            Network.Server_Awaiting_Client_Loop(new Network.Callback(SetUpClient));
            Console.WriteLine("Server awaiting client connection...");
        }


        /// <summary>
        /// Main callback method for setting up a client.
        /// </summary>
        private void SetUpClient(Preserved_State_Object state)
        {
            Console.WriteLine("User " + state.data + " has connected to the server.");

            // Generate 2 random starting coords within our world, check if other players are there, then send if player won't get eaten immediately (helper method)
            double x, y;
            Cube cube;
            string worldData;

            lock (World)
            {
                World.FindStartingCoords(out x, out y, false);
                cube = new Cube(x, y, World.GetUid(), false, state.data.ToString(), World.PLAYER_START_MASS, World.GetColor(), 0);
                World.Cubes[cube.uid] = cube;
                worldData = World.SerializeAllCubes();
            }

            state.CubeID = cube.uid;
            state.data.Clear();
            state.callback = new Network.Callback(ManageData);

            // Send the client's cube and then all of the world data
            Network.Send(state.socket, JsonConvert.SerializeObject(cube) + "\n");
            Network.Send(state.socket, worldData);

            lock (Sockets)
            {
                Sockets.Add(state.socket);
            }

            // Ask for more data from client
            Network.I_Want_More_Data(state);
        }


        /// <summary>
        /// Method to send and receive data from client
        /// </summary>
        private void ManageData(Preserved_State_Object state)
        {
            // Try to perform the complete move or split actions
            string[] actions = Regex.Split(state.data.ToString(), @"\n");

            for (int i = 0; i < actions.Length - 1; i++)
                TryMoveOrSplit(actions[i], state);

            // Try to perform the last move or split action if it is complete, otherwise append what is there for later
            string lastAction = actions.Last();

            if (lastAction.Length > 1 && lastAction.Last() == ')')
                TryMoveOrSplit(lastAction, state);
            else
                state.data = new StringBuilder(lastAction);

            // Call for more client actions
            Network.I_Want_More_Data(state);
        }


        /// <summary>
        /// Parses client requests into moves or splits
        /// </summary>
        private void TryMoveOrSplit(String str, Preserved_State_Object state)
        {
            // Get the coordinates for the move or split
            MatchCollection values = Regex.Matches(str, @"-*\d+");
            double x = double.Parse(values[0].Value);
            double y = double.Parse(values[1].Value);

            // Handle moving or splitting
            //   *NOTE: Cubes are not actually moved here, as that could lead to more or less movement per player in a given amount of time (based on connection speed)
            //          - instead, movement direction is appended to a stringbuilder and dealt with all at the same time in the server's heartbeat tick
            if (str[1] == 'm')
                lock (DataReceived) { DataReceived[state.CubeID] = new Tuple<double, double>(x, y); }
            else if (str[1] == 's')
                lock (World) { World.Split(state.CubeID, x, y); }
        }


        /// <summary>
        /// Every timer tick:
        ///   Updates the world (adds food, etc)
        ///   Sends updates to the clients
        /// </summary>
        private void HeartBeatTick(object state)
        {
            // Data to send to all of the clients
            StringBuilder data = new StringBuilder();

            lock (World)
            {
                // Players get a little smaller each tick, and military viruses move along their path
                World.PlayerAttrition();
                World.MoveMilitaryVirus();

                // Move all players according to last mouse position
                lock (DataReceived)
                {
                    // Uid's to be removed
                    List<int> toBeRemoved = new List<int>();

                    // Move all player cubes according to last mouse position
                    foreach (int uid in DataReceived.Keys)
                    {
                        // If the cube has been removed from the world, mark it to be removed and skip moving
                        if (!World.Cubes.ContainsKey(uid))
                        {
                            toBeRemoved.Add(uid);
                            continue;
                        }

                        World.Move(uid, DataReceived[uid].Item1, DataReceived[uid].Item2); 
                    }

                    // Remove marked cubes
                    foreach (int uid in toBeRemoved)
                        DataReceived.Remove(uid);
                }

                // Check for collisions, eat some food
                data.Append(World.ManageCollisions());

                // Add food to the world if necessary and append it to the data stream
                for (int i = 0; i < World.FOOD_PER_HEARTBEAT && World.Food.Count < World.MAX_FOOD_COUNT; i++)
                    data.Append(JsonConvert.SerializeObject(World.GenerateFoodorVirus()) + "\n");

                // Appends all of the player cube data - they should be constantly changing (mass, position, or both), therefore, we send them every time
                data.Append(World.SerializePlayers());
            }

            // Send data to sockets
            lock (Sockets)
            {
                for (int i = Sockets.Count-1; i >= 0; i--)
                {
                    // Remove sockets that are no longer connected
                    if (!Sockets[i].Connected)
                    {
                        Sockets.RemoveAt(i);
                        continue;
                    }

                    Network.Send(Sockets[i], data.ToString());
                }
            }
        }
    }
}