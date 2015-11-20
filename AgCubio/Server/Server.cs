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

        static void Main(string[] args)
        {
            new Server();
            Console.Read();
        }

        public Server()
        {
            NamesSockets = new Dictionary<string, Socket>();
            Network.Server_Awaiting_Client_Loop(new Network.Callback(SetUpClient));
        }

        private void SetUpClient(Preserved_State_Object state)
        {
            NamesSockets.Add(state.data, state.socket);

            //Reset the callback

            //NEXT SEND CUBE TO CLIENT
        }
    }
}
