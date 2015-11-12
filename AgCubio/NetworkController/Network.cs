using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AgCubio
{
    public static class Network
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback_function"></param>
        /// <param name="hostname"></param>
        /// <returns></returns>
        public static Socket Connect_to_Server(Delegate callback_function, string hostname)
        {
            /*
            hostname - the name of the server to connect to

            callback function - a function inside the View to be called when a connection is made

            This function should attempt to connect to the server via a provided hostname. 
            It should save the callback function (in a state object) for use when data arrives.
    
            It will need to open a socket and then use the BeginConnect method. 
            Note this method take the "state" object and "regurgitates" it back to you when a connection is made, 
            thus allowing "communication" between this function and the Connected_to_Server function.
            */

            //MSDN: localhost can be found with the "" string.
            IPAddress ipAddress = (hostname.ToUpper() == "LOCALHOST") ? Dns.GetHostEntry("").AddressList[0] : IPAddress.Parse(hostname);
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

            Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Preserved_State_Object state = new Preserved_State_Object(socket, callback_function);

            socket.BeginConnect(remoteEP, new AsyncCallback(Connected_to_Server), state); // Need the cast? Yes.

            return state.socket;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="state_in_an_ar_object"></param>
        public static void Connected_to_Server(IAsyncResult state_in_an_ar_object)
        {
            /*
            This function is reference by the BeginConnect method above and is "called" by the OS when the socket connects to the server. 
            The "state_in_an_ar_object" object contains a field "AsyncState" which contains the "state" object saved away in the above function.

            Once a connection is established the "saved away" callback function needs to called. 
            Additionally, the network connection should "BeginReceive" expecting more data to arrive (and provide the ReceiveCallback function for this purpose)
            */

            Preserved_State_Object state = (Preserved_State_Object)state_in_an_ar_object.AsyncState;
            state.socket.EndConnect(state_in_an_ar_object);

            state.callback_function.DynamicInvoke(state);     

            state.socket.BeginReceive(state.buffer, 0, Preserved_State_Object.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="state_in_an_ar_object"></param>
        public static void ReceiveCallback(IAsyncResult state_in_an_ar_object)
        {
            Preserved_State_Object state = (Preserved_State_Object)state_in_an_ar_object.AsyncState;
            int bytesRead = state.socket.EndReceive(state_in_an_ar_object);

            if (bytesRead > 0)
            {
                state.cubedata = Encoding.UTF8.GetString(state.buffer, 0, bytesRead);
                state.callback_function.DynamicInvoke(state);
            }
            else
                state.socket.Close();

            /*
            The ReceiveCallback method is called by the OS when new data arrives. 
            This method should check to see how much data has arrived. 
            If 0, the connection has been closed (presumably by the server). 
            On greater than zero data, this method should call the callback function provided above.

            For our purposes, this function should not request more data. 
            It is up to the code in the callback function above to request more data.

            */


        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        public static void I_Want_More_Data(Preserved_State_Object state)
        {
            /*
            This is a small helper function that the client View code will call whenever it wants more data. 
            Note: the client will probably want more data every time it gets data.
            */

            state.socket.BeginReceive(state.buffer, 0, Preserved_State_Object.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="data"></param>
        public static void Send(Socket socket, String data)
        {
            /*
            This function (along with it's helper 'SendCallback') will allow a program to send data over a socket. 
            This function needs to convert the data into bytes and then send them using socket.BeginSend.
            */



            byte[] byteData = Encoding.UTF8.GetBytes(data);
            socket.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(SendCallBack), null);            
        }


        /// <summary>
        /// 
        /// </summary>
        public static void SendCallBack(IAsyncResult state)
        {
            /*
            This function "assists" the Send function. If all the data has been sent, then life is good and nothing needs to be done 
            (note: you may, when first prototyping your program, put a WriteLine in here to see when data goes out).

            If there is more data to send, the SendCallBack needs to arrange to send this data (see the ChatClient example program).
            */

        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class Preserved_State_Object
    {
        /// <summary>
        /// 
        /// </summary>
        public Preserved_State_Object(Socket socket, Delegate callback_function)
        {
            this.socket = socket;
            this.callback_function = callback_function;
        }

        /// <summary>
        /// Player name
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Socket socket { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Delegate callback_function { get; set; }

        /// <summary>
        /// Buffer size of 2^15
        /// </summary>
        public const int BufferSize = 32768;

        /// <summary>
        /// 
        /// </summary>
        public byte[] buffer = new byte[BufferSize];

        /// <summary>
        /// 
        /// </summary>
        public String cubedata;
    }
}
