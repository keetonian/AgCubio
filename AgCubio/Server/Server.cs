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
            

            //Flow: Get name, send cube, then send all world info, then start the flow back and forth as you receive and send information and requests.
            //Reset the callback

            //NEXT SEND CUBE TO CLIENT
        }


        private void FindStartingCoords(out x, out y)
        {

        }

        private int GetUid()
        {

        }

        private int GetColor()
        {

        }


    }
}
