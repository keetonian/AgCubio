// Created by Daniel Avery and Keeton Hodgson
// November 2015

/*FROM WEBSITE:
As stated above, the Client GUI should not be changed for this project. 
The one exception you may want to make is the following: 

Allow the Client to press a magic key (the '!') and turn off the view scaling, allowing you to see the entire world. 
This will allow the server developer to have a visual on what is happening. 

Additionally, you could instrument your server so that if you send the magic name: "observer", no player cube is created.
*/

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace AgCubio
{
    /// <summary>
    /// GUI client controller for AgCubio
    /// </summary>
    public partial class Display : Form
    {
        /// <summary>
        /// World model storing all the cubes
        /// </summary>
        private World World;

        /// <summary>
        /// Our networking socket, saved so that it doesn't go out of scope
        /// </summary>
        private Socket socket;

        /// <summary>
        /// Thread running networking code
        /// </summary>
        private Thread NetworkThread;

        /// <summary>
        /// Stores data received from the server
        /// </summary>
        private StringBuilder CubeData;

        /// <summary>
        /// Previous mouse locations (for when mouse goes offscreen), player cube id, frames elapsed in the last second, and playtime count (in seconds) -
        /// If a game lasts over ~64 years we may be in trouble
        /// </summary>
        private int PrevMouseLoc_x, PrevMouseLoc_y, PlayerID, FramesElapsed, Playtime;

        /// <summary>
        /// Set containing uid's of split player cubes
        /// </summary>
        private HashSet<int> PlayerSplitID = new HashSet<int>();

        /// <summary>
        /// Game timer
        /// </summary>
        private System.Windows.Forms.Timer FPStimer;

        /// <summary>
        /// Maximum player mass achieved
        /// </summary>
        private double MaxMass;


        /// <summary>
        /// Constructor to handle initial setup of the view
        /// </summary>
        public Display()
        {
            // Initialize the world model with display size, setup up a StringBuilder for getting data from server
            World = new World();
            CubeData = new StringBuilder();

            // Set up the timer with a tick event that triggers every second (for FPS)
            FPStimer = new System.Windows.Forms.Timer();
            FPStimer.Interval = 1000;
            FPStimer.Tick += FPStimer_Tick;

            // Prevent 'flickering' issue and initialize the form
            DoubleBuffered = true;
            InitializeComponent();

            // Resize GUI components
            this.Resize += Display_Resize;

            // Background color. May be cool to have player control this.
            this.BackColor = Color.WhiteSmoke;
        }


        /// <summary>
        /// Centers GUI elements when the window is resized.
        /// </summary>
        private void Display_Resize(object sender, EventArgs e)
        {
            // Resize main screen
            this.nameLabel.Left = Width / 2 - (this.connectButton.Width + this.textBoxName.Width + this.textBoxServer.Width + this.nameLabel.Width + this.addressLabel.Width + 20) / 2;
            this.textBoxName.Left = this.nameLabel.Right + 5;
            this.addressLabel.Left = this.textBoxName.Right + 5;
            this.textBoxServer.Left = this.addressLabel.Right + 5;
            this.connectButton.Left = this.textBoxServer.Right + 5;

            // Resize statistics screen
            this.ExitToMainScreen.Left = Width / 2 - this.ExitToMainScreen.Size.Width / 2;
            this.Statistics.Left = Width / 2 - this.Statistics.Size.Width / 2;
            this.MaxMassLabel.Left = Width / 2 - (this.MaxPlayerMass.Width + this.MaxMassLabel.Width + 20) / 2;
            this.MaxPlayerMass.Left = this.MaxMassLabel.Right + 20;
            this.PlaytimeLabel.Left = Width / 2 - (this.PlaytimeVal.Width + this.PlaytimeLabel.Width + 20) / 2;
            this.PlaytimeVal.Left = this.PlaytimeLabel.Right + 20;
        }


        /// <summary>
        /// Timer tick handler - prints the elapsed frames in the last second (FPS), resets elapsed frames, and increments playtime
        /// </summary>
        private void FPStimer_Tick(object sender, EventArgs e)
        {
            this.FPSvalue.Text = "" + FramesElapsed;
            FramesElapsed = 0;
            Playtime++;
        }


        /// <summary>
        /// Allows text values to be prioritized and refreshed when repainting
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }


        /// <summary>
        /// Paints the state of the world.
        /// Needs to constantly repaint itself, because the view is constantly changing.
        /// The view is based on the player position and size.
        /// </summary>
        private void Display_Paint(object sender, PaintEventArgs e)
        {
            if (!socket.Connected)
            {
                FPStimer.Stop();

                //Disconnection: proceed with this method call.
                MessageBox.Show("You've been disconnected.");
                ShowMainScreen();
                
                return;
            }

            GetCubes();

            lock (World)
            {
                // Initialize total player mass to 0, get player x and y coordinates
                double totalMass = 0;

                // Iterate over split player cubes in the world and increment total player mass
                foreach (int i in PlayerSplitID)
                    totalMass += World.Cubes[i].Mass;

                // If the total mass of the player cube(s) is 0, it means that the game has ended
                if (totalMass == 0)
                {
                    EndGame();
                    return;
                }
                else if (totalMass > MaxMass)
                    MaxMass = totalMass;
                this.MassValue.Text = "" + (int)totalMass;


                // Set the scale, based on the (virtual) width of the player
                double scale = 100 / Math.Sqrt(totalMass);

                // Get player coordinates, use to calculate actual mouse position
                double Px = World.Cubes[PlayerID].loc_x, Py = World.Cubes[PlayerID].loc_y;

                PrevMouseLoc_x = (int)((Display.MousePosition.X - Width / 2 )/scale + Px);
                PrevMouseLoc_y = (int)((Display.MousePosition.Y - Height / 2 )/scale + Py );

                Brush brush;
                RectangleF rectangle;

                foreach (Cube c in World.Cubes.Values)
                {
                    if (c.Mass > 0) // Avoid painting if mass is 0 - also solves an issue where names are still displayed after some cubes are 'eaten'
                    {
                        // Painting food
                        if (c.food)
                        {
                            // Food is scaled, and has an extra scaling factor (so we can see it at larger cube sizes - temporary design decision to deal with a faulty server)
                            rectangle = new RectangleF((int)((c.loc_x - Px - c.width / 2) * scale + Width / 2),
                            (int)((c.loc_y - Py - c.width / 2) * scale + Height / 2), (int)(c.width * scale), (int)(c.width * scale));

                            // Food is painted with solid colors
                            brush = new SolidBrush(Color.FromArgb(c.argb_color));
                            e.Graphics.FillRectangle(brush, rectangle);
                        }

                        // Painting player cubes
                        else
                        {
                            // Location is calculated differently for user and other players - user is centered, other players' coordinates are scaled
                            rectangle = (c.uid == PlayerID) ? new RectangleF((int)(Width / 2 - c.width * scale / 2), (int)(Height / 2 - c.width * scale / 2), (int)(c.width * scale), (int)(c.width* scale)) :
                                new RectangleF((int)((c.loc_x - Px - c.width * scale / 2) * scale + Width / 2), (int)((c.loc_y - Py - c.width * scale / 2) * scale + Height / 2), (int)(c.width* scale), (int)(c.width* scale));

                            // Players are painted with a diagonal gradient, ranging from the actual (server-defined) color to its negative
                            brush = new LinearGradientBrush(rectangle, Color.FromArgb(c.argb_color), Color.FromArgb(c.argb_color^0xFFFFFF), 225);
                            e.Graphics.FillRectangle(brush, rectangle);

                            // Players also have their names printed on them, centered
                            StringFormat stringFormat = new StringFormat();
                            stringFormat.Alignment = StringAlignment.Center;
                            stringFormat.LineAlignment = StringAlignment.Center;
                            int stringSize = (int)(c.width / (c.Name.Length + 1)) + 10;
                            e.Graphics.DrawString(c.Name, new Font(FontFamily.GenericSerif, stringSize, FontStyle.Italic, GraphicsUnit.Pixel),
                                new SolidBrush(Color.Black), rectangle, stringFormat);
                        }
                    }
                }
            }

            // Increment the number of frames (used in showing FPS) before repainting
            FramesElapsed++;
            this.Invalidate();
        }


        /// <summary>
        /// Show endgame player stats and replay button
        /// </summary>
        private void EndGame()
        {
            // Stop the game timer
            FPStimer.Stop();

            // Show endgame statistics

            // Exit button
            this.ExitToMainScreen.Show();

            // Statistics label
            this.Statistics.Show();

            // Greatest mass achieved
            this.MaxMassLabel.Show();
            this.MaxPlayerMass.Text = "" + (int)MaxMass;
            this.MaxPlayerMass.Show();

            // Playtime
            this.PlaytimeLabel.Show();
            this.PlaytimeVal.Text = "" + Playtime / 3600 + "h " + (Playtime % 3600) / 60 + "m " + Playtime % 60 + "s";
            this.PlaytimeVal.Show();
        }


        /// <summary>
        /// Resets the world and shows the main screen
        /// </summary>
        private void ShowMainScreen()
        {
            // Reset the world
            lock(World)
                World = new World();

            // Get rid of the network thread so it isn't updating while no work is being done
            //NetworkThread.Abort();

            // Close the socket, not being used until player signs in again.
            socket.Close();
            
            // Prevent further painting (would be bad when there is no socket connection)
            this.Paint -= this.Display_Paint;

            // Show the original items

            // Player name
            this.nameLabel.Show();
            this.textBoxName.Show();

            // Server address
            this.addressLabel.Show();
            this.textBoxServer.Show();
            this.textBoxServer.ReadOnly = false; // Allow editing again

            // Connect button
            this.connectButton.Show();
        }


        /// <summary>
        /// Shows a dialog box for handling server connection issues
        /// </summary>
        private void UnableToConnect()
        {
            DialogResult result = MessageBox.Show("Unable to connect to server", "Connection Error",
                MessageBoxButtons.RetryCancel, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);

            if (result == DialogResult.Retry)
            {
                Connect_Click(null, null);
                return;
            }

            ShowMainScreen();
        }


        /// <summary>
        /// Event for clicking the connect button.
        /// </summary>
        private void Connect_Click(object sender, EventArgs e)
        {
            try
            {
                // Save the socket so that it doesn't go out of scope or get garbage collected (happened a few times)
                socket = Network.Connect_to_Server(new Network.Callback(SendName), textBoxServer.Text);
                
                // Hide menu (and stat) items
                this.connectButton.Hide();
                this.textBoxName.Hide();
                this.textBoxServer.Hide();
                this.textBoxServer.ReadOnly = true;
                this.nameLabel.Hide();
                this.addressLabel.Hide();
                this.Statistics.Hide();
                this.MaxPlayerMass.Hide();
                this.MaxMassLabel.Hide();
                this.PlaytimeLabel.Hide();
                this.PlaytimeVal.Hide();
                
                // Start the game timer
                FPStimer.Start();
                Playtime = 0;
            }
            // Catch any errors that might occur in connecting (like if an invalid IP address is supplied)
            catch (FormatException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        /// <summary>
        /// Callback method - sends the player name to the server
        /// </summary>
        private void SendName(Preserved_State_Object state)
        {
            // Pop a dialog box if the connection is not established
            if (!socket.Connected)
            {
                this.Invoke(new Action(UnableToConnect));
                return;
            }

            // Save the network thread to a private member in the GUI (for deactivation later)
            NetworkThread = Thread.CurrentThread;

            // Prevent network from getting upset over empty string names
            string name = (textBoxName.Text == "") ? " " : textBoxName.Text;

            // Provide the next callback and send the player name to the server
            state.callback = new Network.Callback(GetPlayerCube);
            Network.Send(state.socket, name);
        }


        /// <summary>
        /// Callback method - gets the player cube from the server
        /// </summary>
        private void GetPlayerCube(Preserved_State_Object state)
        {
            // Get the player cube (and add its uid to the set of split player cubes)
            Cube c = JsonConvert.DeserializeObject<Cube>(state.data.ToString());
            PlayerSplitID.Add(PlayerID = c.uid);

            // Set the max mass to the initial player mass
            MaxMass = c.Mass;

            // Add the player cube to the world
            lock (World)
            {
                World.Cubes.Add(c.uid, c);
            }

            // Begin painting the world
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Display_Paint);
            this.Invalidate();

            // Set the default move coordinates to the player block's starting location
            PrevMouseLoc_x = (int)c.loc_x;
            PrevMouseLoc_y = (int)c.loc_y;

            // Provide the next callback and start getting game data from the server
            state.callback = new Network.Callback(SendReceiveData);
            Network.I_Want_More_Data(state);
        }

        private void Display_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.socket.Shutdown(SocketShutdown.Both);
            this.socket.Close();
        }

        /// <summary>
        /// Callback method - sends moves, receives data from the server
        /// </summary>
        private void SendReceiveData(Preserved_State_Object state)
        {
            if (socket.Connected)
            {
                lock (CubeData)
                {
                    // Use the StringBuilder to append the string received from the server
                    CubeData.Append(state.data);
                }

                state.data.Clear();

                // Send a move request, following the convention: '(move, dest_x, dest_y)\n'
                string move = "(move, " + PrevMouseLoc_x + ", " + PrevMouseLoc_y + ")\n";
                Network.Send(socket, move);

                // Ask for more data
                Network.I_Want_More_Data(state);
            }
        }


        /// <summary>
        /// Deserializes cubes and adds them to the world
        /// </summary>
        private void GetCubes()
        {
            // Short circuit if client has received no new cube data
            if (CubeData.Length < 1)
                return;

            lock (CubeData)
            {
                // Obtain each cube string
                String[] cubes = Regex.Split(CubeData.ToString(), "\n");
                string lastCube;

                lock (World)
                {
                    // Parse all cubes into the world except the last one
                    for (int i = 0; i < cubes.Length - 1; i++)
                    {
                        if (cubes[i] == "") //Quick fix- sometimes gets a blank string. Needs to be fixed in server.
                            continue;
                        Cube c = JsonConvert.DeserializeObject<Cube>(cubes[i]);
                        World.Cubes[c.uid] = c;

                        // Keep track of split player cubes
                        if (c.Team_ID == PlayerID && !PlayerSplitID.Contains(c.uid))
                            PlayerSplitID.Add(c.uid);

                        // Remove cubes of zero mass from the world
                        if (c.Mass == 0)
                        {
                            World.Cubes.Remove(c.uid);
                            PlayerSplitID.Remove(c.uid);
                        }
                    }

                    // Parse the last cube into the world only if it is complete
                    lastCube = cubes[cubes.Length - 1];

                    if (lastCube.Length > 0 && lastCube.Last() == '}')
                    {
                        Cube c = JsonConvert.DeserializeObject<Cube>(lastCube);
                        World.Cubes[c.uid] = c;
                        lastCube = "";
                    }
                }

                // Reset the StringBuilder, but retain any partial string from the last cube
                CubeData = new StringBuilder(lastCube);
            }
        }


        /// <summary>
        /// Button to click to exit to the main screen after a game-over scenario
        /// </summary>
        private void ExitToMainScreen_Click(object sender, EventArgs e)
        {
            ShowMainScreen();
            this.ExitToMainScreen.Hide();
        }


        /// <summary>
        /// Allows the space bar to work, send split requests.
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Space && socket != null)
            {
                // Send a split request, following the convention: '(split, dest_x, dest_y)\n'
                string split = "(split, " + PrevMouseLoc_x + ", " + PrevMouseLoc_y + ")\n";
                Network.Send(socket, split);
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

    }
}
