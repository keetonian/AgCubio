using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AgCubio
{
    /// <summary>
    /// 
    /// </summary>
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
            Preserved_State_Object state = (Preserved_State_Object)state_in_an_ar_object.AsyncState;
          
            try
            {
                state.socket.EndConnect(state_in_an_ar_object);
            }
            catch (SocketException)
            {
                //Manage problems with a socket connection, return to above program.
                state.socket.Close();
                state.socket.Dispose();
                state.callback_function.DynamicInvoke(state);
                return;
            }

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
            int bytesRead;
            try
            {
                bytesRead = state.socket.EndReceive(state_in_an_ar_object);
                if (bytesRead > 0)
                {
                    state.cubedata = Encoding.UTF8.GetString(state.buffer, 0, bytesRead);
                    state.callback_function.DynamicInvoke(state);
                }
                else
                    state.socket.Close();
            }
            catch(Exception)
            {
                //If there is a problem with the socket, close it, then let the above program find the closure, try again.
                state.socket.Close();
                state.socket.Dispose();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        public static void I_Want_More_Data(Preserved_State_Object state)
        {
            state.socket.BeginReceive(state.buffer, 0, Preserved_State_Object.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="data"></param>
        public static void Send(Socket socket, String data)
        {
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
