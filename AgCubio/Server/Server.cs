﻿using System;
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


        /// <summary>
        /// 
        /// </summary>
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
            World = new World(/*"World_Params.xml"*/);
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
            Cube cube;
            string worldData;

            lock(World)
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

            // Ask for more data from client
            Network.I_Want_More_Data(state);
        }


        /// <summary>
        /// Method to send and receive data from client
        /// </summary>
        private void ManageData(Preserved_State_Object state)
        {
            // Action for parsing client requests into moves or splits
            Action<String> TryMoveOrSplit = new Action<String>((str) =>
            {
                // Get the coordinates for the move or split
                MatchCollection values = Regex.Matches(str, @"-*\d+");
                double x = double.Parse(values[0].Value);
                double y = double.Parse(values[1].Value);

                // Handle moving or splitting
                if (str[1] == 'm')
                    lock (DataReceived) { DataReceived[state.CubeID] = new Tuple<double, double>(x, y); }
                else if (str[1] == 's')
                    lock (World) { World.Split(state.CubeID, x, y); }

            });

            // Try to perform the complete move or split actions
            string[] actions = Regex.Split(state.data.ToString(), @"\n");
            for (int i = 0; i < actions.Length - 1; i++)
                TryMoveOrSplit(actions[i]);

            // Try to perform the last move or split action if it is complete, otherwise append what is there for later
            string lastAction = actions.Last();
            if (lastAction.Length > 1 && lastAction?.Last() == ')')
                TryMoveOrSplit(lastAction);
            else
                state.data = new StringBuilder(lastAction);

            // Call for more client actions
            Network.I_Want_More_Data(state);
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
                // Players get a little smaller each tick
                World.PlayerAttrition();

                lock (DataReceived)
                {
                    foreach (int i in DataReceived.Keys)
                        World.Move(i, DataReceived[i].Item1, DataReceived[i].Item2);
                }

                // Check for collisions, eat some food
                data.Append(World.ManageCollisions());

                // Add food to the world if necessary and append it to the data stream
                if (World.Food.Count < World.MAX_FOOD_COUNT)
                {
                    Cube food = World.GenerateFoodorVirus();
                    data.Append(JsonConvert.SerializeObject(food) + "\n");
                }

                // Appends all of the player cube data - they should be constantly changing (mass, position, or both), therefore, we send them every time
                data.Append(World.SerializePlayers());
            }

            // Send data to sockets
            lock (Sockets)
            {
                List<Socket> disconnected = new List<Socket>();

                foreach (Socket s in Sockets)
                {
                    // Set disconnected sockets to be removed (don't send)
                    if (!s.Connected)
                    {
                        disconnected.Add(s);
                        continue;
                    }

                    Network.Send(s, data.ToString());
                }

                // Remove disconnected sockets
                foreach (Socket s in disconnected)
                    Sockets.Remove(s);
            }
        }
    }
}