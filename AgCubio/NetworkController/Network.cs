// Created by Daniel Avery and Keeton Hodgson
// November 2015

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AgCubio
{
    /// <summary>
    /// Handles all networking code for AgCubio
    /// </summary>
    public static class Network
    {
        /// <summary>
        /// Begins establishing a connection to the server
        /// </summary>
        public static Socket Connect_to_Server(Delegate callback_function, string hostname)
        {
            // Store the server IP address and remote endpoint
            //   MSDN: localhost can be found with the "" string.
            IPAddress ipAddress = (hostname.ToUpper() == "LOCALHOST") ? Dns.GetHostEntry("").AddressList[0] : IPAddress.Parse(hostname);
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

            // Make a new socket and preserved state object and begin connecting
            Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Preserved_State_Object state = new Preserved_State_Object(socket, callback_function);

            // Begin establishing a connection
            socket.BeginConnect(remoteEP, new AsyncCallback(Connected_to_Server), state);

            // Return the socket
            return state.socket;
        }


        /// <summary>
        /// Finish establishing a connection to the server, invoke the callback in the preserved state object, and begin receiving data
        /// </summary>
        public static void Connected_to_Server(IAsyncResult state_in_an_ar_object)
        {
            // Get the state from the parameter
            Preserved_State_Object state = (Preserved_State_Object)state_in_an_ar_object.AsyncState;

            try
            {
                state.socket.EndConnect(state_in_an_ar_object);
            }
            catch (SocketException)
            {
                // Manage problems with a socket connection, return to above program
                state.socket.Close();
                state.callback_function.DynamicInvoke(state);
                return;
            }

            // Invoke the callback
            state.callback_function.DynamicInvoke(state);

            // Begin receiving data from the server
            state.socket.BeginReceive(state.buffer, 0, Preserved_State_Object.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
        }


        /// <summary>
        /// Handles reception and storage of data
        /// </summary>
        public static void ReceiveCallback(IAsyncResult state_in_an_ar_object)
        {
            // Get the state from the parameter, declare a variable for holding count of received bytes
            Preserved_State_Object state = (Preserved_State_Object)state_in_an_ar_object.AsyncState;
            int bytesRead;

            try
            {
                bytesRead = state.socket.EndReceive(state_in_an_ar_object);

                // If bytes were read, save the decoded string and invoke the callback
                if (bytesRead > 0)
                {
                    state.cubedata = Encoding.UTF8.GetString(state.buffer, 0, bytesRead);
                    state.callback_function.DynamicInvoke(state);
                }
                // Otherwise we are disconnected - close the socket
                else
                    state.socket.Close();
            }
            catch(Exception)
            {
                // If there is a problem with the socket, close it, then let the above program find the closure, try again.
                state.socket.Close();
                state.socket.Dispose();
            }
        }


        /// <summary>
        /// Tells server we are ready to receive more data
        /// </summary>
        public static void I_Want_More_Data(Preserved_State_Object state)
        {
            state.socket.BeginReceive(state.buffer, 0, Preserved_State_Object.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
        }


        /// <summary>
        /// Sends encoded data to the server
        /// </summary>
        public static void Send(Socket socket, String data)
        {
            byte[] byteData = Encoding.UTF8.GetBytes(data);
            socket.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(SendCallBack), null);            
        }


        /// <summary>
        /// Does nothing...
        /// </summary>
        public static void SendCallBack(IAsyncResult state)
        {
            // We found this method unnecessary - we never ran into a problem with our send having leftover data
        }
    }


    /// <summary>
    /// Preserves the state of the socket, and the current callback function
    /// </summary>
    public class Preserved_State_Object
    {
        /// <summary>
        /// Constructs a Preserved_State_Object with the given socket and callback
        /// </summary>
        public Preserved_State_Object(Socket socket, Delegate callback_function)
        {
            this.socket = socket;
            this.callback_function = callback_function;
        }

        /// <summary>
        /// Networking socket
        /// </summary>
        public Socket socket { get; set; }

        /// <summary>
        /// Current callback function
        /// </summary>
        public Delegate callback_function { get; set; }

        /// <summary>
        /// Buffer size of 2^15
        /// </summary>
        public const int BufferSize = 32768;

        /// <summary>
        /// Byte array for reading bytes from the server
        /// </summary>
        public byte[] buffer = new byte[BufferSize];

        /// <summary>
        /// String for storing cube data received from the server
        /// </summary>
        public String cubedata;
    }
}
