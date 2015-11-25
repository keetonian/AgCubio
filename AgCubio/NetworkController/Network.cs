// Created by Daniel Avery and Keeton Hodgson
// November 2015

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AgCubio
{
    /// <summary>
    /// Handles all networking code for AgCubio
    /// </summary>
    public static class Network
    {
        /// <summary>
        /// Callback delegate for networking
        /// </summary>
        public delegate void Callback(Preserved_State_Object state);

        /// <summary>
        /// Port number that this network code uses.
        /// </summary>
        private const int Port = 11000;

        /// <summary>
        /// Begins establishing a connection to the server
        /// </summary>
        public static Socket Connect_to_Server(Callback callback, string hostname)
        {
            // Store the server IP address and remote endpoint
            //   MSDN: localhost can be found with the "" string.
            IPAddress ipAddress = (hostname.ToUpper() == "LOCALHOST") ? Dns.GetHostEntry("").AddressList[0] : IPAddress.Parse(hostname);
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, Port);

            // Make a new socket and preserved state object and begin connecting
            Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Preserved_State_Object state = new Preserved_State_Object(socket, callback);

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
                state.callback.DynamicInvoke(state);
                return;
            }

            // Invoke the callback
            state.callback.DynamicInvoke(state);

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
                    state.data = Encoding.UTF8.GetString(state.buffer, 0, bytesRead);
                    state.callback.DynamicInvoke(state);
                }
                // Otherwise we are disconnected - close the socket
                else
                {
                    state.socket.Shutdown(SocketShutdown.Both);
                    state.socket.Close();
                }
            }
            catch (Exception)
            {
                // If there is a problem with the socket, close it, then let the above program find the closure, try again.
                state.socket.Shutdown(SocketShutdown.Both);
                state.socket.Close();
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
            Tuple<Socket, byte[]> state = new Tuple<Socket, byte[]>(socket, byteData);
            socket.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallBack), state);
        }


        /// <summary>
        /// Helper method for Send - arranges for any leftover data to be sent
        /// </summary>
        public static void SendCallBack(IAsyncResult state_in_an_ar_object)
        {
            Tuple<Socket, byte[]> state = (Tuple<Socket, byte[]>)state_in_an_ar_object.AsyncState;
            int bytesSent = state.Item1.EndSend(state_in_an_ar_object);

            if (bytesSent == state.Item2.Length)
                return;
            else
            {
                byte[] bytes = new byte[state.Item2.Length - bytesSent];
                Array.ConstrainedCopy(state.Item2, bytesSent, bytes, 0, bytes.Length);
                Tuple<Socket, byte[]> newState = new Tuple<Socket, byte[]>(state.Item1, bytes);
                state.Item1.BeginSend(state.Item2, bytesSent, state.Item2.Length, 0, new AsyncCallback(SendCallBack), newState);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback"></param>
        public static void Server_Awaiting_Client_Loop(Delegate callback)
        {
            IPAddress IP = IPAddress.Parse("127.0.0.1");
            TcpListener server = new TcpListener(IPAddress.Any, 11000);
            server.Start();
            Preserved_State_Object state = new Preserved_State_Object(server, callback);
            state.callback.DynamicInvoke(state);

            // Probably don't need another thread, just trying things.
            //new Thread(() => server.BeginAcceptSocket(new AsyncCallback(Accept_a_New_Client), state));
            server.BeginAcceptSocket(new AsyncCallback(Accept_a_New_Client), state);
            //server.BeginAcceptTcpClient(new AsyncCallback(Accept_a_New_Client), state);
            //server.Pending()?


            System.Diagnostics.Debug.WriteLine("Server Awaiting client");

            /*
            This is the heart of the server code. It should ask the OS to listen for a connection and save the callback function with that request. 
            Upon a connection request coming in the OS should invoke the Accept_a_New_Client method (see below).

            Note: while this method is called "Loop", it is not a traditional loop, but an "event loop" 
            (i.e., this method sets up the connection listener, which, when a connection occurs, sets up a new connection listener. for another connection).

            */
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="ar"></param>
        public static void Accept_a_New_Client(IAsyncResult ar)
        {
            System.Diagnostics.Debug.WriteLine("Server Accept Client");

            Preserved_State_Object state = (Preserved_State_Object)ar.AsyncState;
            state.socket = state.server.EndAcceptSocket(ar);

            state.socket.BeginReceive(state.buffer, 0, Preserved_State_Object.BufferSize, 0, new AsyncCallback(ReceiveCallback), state); //Get the name, then give them their cube.
            state.server.BeginAcceptSocket(new AsyncCallback(Accept_a_New_Client), new Preserved_State_Object(state.server, state.callback));

            /*
            This code should be invoked by the OS when a connection request comes in. It should:
            1.Create a new socket
            2.Call the callback provided by the above method
            3.Await a new connection request.

            Note: the callback method referenced in the above function should have been transferred to this 
            function via the AsyncResult parameter and should be invoked at this point.

            WARNING!!!After accepting a new client, the Networking code should NOT start listening for data!
            It is the job of the game server(presumably via the callback method) to request data!

            */
        }
    }


    /// <summary>
    /// Preserves the state of the socket, and the current callback function
    /// </summary>
    public class Preserved_State_Object
    {
        /// <summary>
        /// Constructs a client-type Preserved_State_Object with the given socket and callback
        /// </summary>
        public Preserved_State_Object(Socket socket, Delegate callback)
        {
            this.socket = socket;
            this.callback = callback;
        }


        /// <summary>
        /// Constructs a server-type Preserved_State_Object with the given TcpListener and callback
        /// </summary>
        public Preserved_State_Object(TcpListener server, Delegate callback)
        {
            this.server = server;
            this.callback = callback;
        }

        /// <summary>
        /// Current callback function
        /// </summary>
        public Delegate callback;

        /// <summary>
        /// Buffer size of 2^10
        /// </summary>
        public const int BufferSize = 1024;

        /// <summary>
        /// Byte array for reading bytes from the server
        /// </summary>
        public byte[] buffer = new byte[BufferSize];

        /// <summary>
        /// Networking socket
        /// </summary>
        public Socket socket;

        // FOR CLIENT USE:

        /// <summary>
        /// String for storing cube data received from the server
        /// </summary>
        public String data;

        // FOR SERVER USE:

        /// <summary>
        /// 
        /// </summary>
        public TcpListener server;
    }
}
