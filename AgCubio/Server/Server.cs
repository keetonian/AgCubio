using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AgCubio
{
    class Server
    {
        private Dictionary<string, Socket> NamesSockets;

        private World World;

        private int Uid;

        private Stack<int> Uids;

        Random RandomNumber;


        static void Main(string[] args)
        {
            new Server();
            Console.Read();
        }

        public Server()
        {
            World = new World(); //Use the file path later.
            NamesSockets = new Dictionary<string, Socket>();
            Network.Server_Awaiting_Client_Loop(new Network.Callback(SetUpClient));

            Uids = new Stack<int>();
            RandomNumber = new Random();
        }

        private void SetUpClient(Preserved_State_Object state)
        {
            NamesSockets.Add(state.data, state.socket);

            //For original UID's: have a counter that counts up and gives unique uid's
            //When a cube is removed, store the uid in a stack to be reused.
            //If there is nothing on the stack, increment the counter and use that UID.
            //IF there is something on the stack, pop it and use that as the uid.


            // Generate 2 random starting coords within our world, check if other players are there, then send if player won't get eaten immediately. (helper method)
            double x, y;
            FindStartingCoords(out x, out y);
            Cube cube = new Cube(x,y,GetUid(),false,state.data,World.PLAYER_START_MASS,GetColor(),0);

            //Network.Send();
            

            //Flow: Get name, send cube, then send all world info, then start the flow back and forth as you receive and send information and requests.
            //Reset the callback

            //NEXT SEND CUBE TO CLIENT
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


    }
}
