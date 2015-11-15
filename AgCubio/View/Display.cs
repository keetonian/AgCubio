using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace AgCubio
{
    public partial class Display : Form
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        public delegate void Callback(Preserved_State_Object state);

        //Used to call void functions out of another thread.
        private delegate void MainMenu();

        private World World;

        private Socket socket;

        private Thread NetworkThread;

        private StringBuilder CubeData;

        private int PlayerID;

        private int PrevMouseLoc_x, PrevMouseLoc_y;

        private HashSet<int> PlayerSplitID = new HashSet<int>();

        private System.Windows.Forms.Timer FPStimer;

        private int FramesElapsed;

        //Maximum player mass achieved.
        private double MaxMass;


        /// <summary>
        /// 
        /// </summary>
        public Display()
        {
            World = new World(Width, Height);
            CubeData = new StringBuilder();
            InitializeComponent();
            FPStimer = new System.Windows.Forms.Timer();
            FPStimer.Interval = 1000; // Ticks every second
            FPStimer.Tick += FPStimer_Tick;
            DoubleBuffered = true;
        }

        private void FPStimer_Tick(object sender, EventArgs e)
        {
            this.FPSvalue.Text = "" + FramesElapsed;
            FramesElapsed = 0;
        }


        /// <summary>
        /// Used to set controls on the initial text boxes.
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
                return cp;
            }
        }


        /// <summary>
        /// Paints the state of the world.
        /// Needs to constantly repaint itself, because the view is constantly changing.
        /// The view is based on the player position and size.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                //TODO: ENDGAME if world.cubes.containskey(playerid) is false.
                //DOes this even go here?


                double totalMass = 0, Px = 0, Py = 0;

                foreach (int i in PlayerSplitID)
                {
                    Cube c = World.Cubes[i];
                    totalMass += c.Mass;

                    // Calculate center of mass coordinates
                    Px += c.Mass * c.loc_x;
                    Py += c.Mass * c.loc_y;
                }

                //If the total mass of the player cube(s) is 0, it means that the game has ended.
                if (totalMass == 0)
                {
                    EndGame();
                    return;
                }

                else if (totalMass > MaxMass)
                    MaxMass = totalMass;

                this.MassValue.Text = "" + (int)totalMass;

                Px = World.Cubes[PlayerID].loc_x;//= totalMass;
                Py = World.Cubes[PlayerID].loc_y;//= totalMass;

                double width = Math.Sqrt(totalMass);
                double scale = 100 / width;

                //Mouse location is slightly off.
                PrevMouseLoc_x = (int)((Control.MousePosition.X - Width / 2 )/scale + Px); // Display.MousePosition.X
                PrevMouseLoc_y = (int)((Control.MousePosition.Y - Height / 2 )/scale + Py );

                System.Diagnostics.Debug.WriteLine("MX: " + PrevMouseLoc_x + ", MY: " + PrevMouseLoc_y);

                Brush brush;
                RectangleF rectangle;

                foreach (Cube c in World.Cubes.Values)
                {
                    if (c.Mass > 0)
                    {
                        if (c.food)
                        {
                            rectangle = new RectangleF((int)((c.loc_x - Px - c.width * scale * 3) * scale + Width / 2),
                            (int)((c.loc_y - Py - c.width * scale * 3) * scale + Height / 2), (int)(c.width * scale * 6), (int)(c.width * scale * 6));

                            brush = new SolidBrush(Color.FromArgb(c.argb_color));
                            e.Graphics.FillRectangle(brush, rectangle);
                        }

                        else
                        {
                            rectangle = (c.uid == PlayerID) ? new RectangleF((int)(Width / 2 - c.width / 2), (int)(Height / 2 - c.width / 2), (int)(c.width), (int)(c.width)) :
                                new RectangleF((int)((c.loc_x - Px - c.width/4) * scale + Width / 2), (int)((c.loc_y - Py - c.width/4) * scale + Height / 2), (int)(c.width), (int)(c.width));

                            brush = new LinearGradientBrush(rectangle, Color.FromArgb(c.argb_color), Color.FromArgb(c.argb_color^0xFFFFFF), 225);
                            e.Graphics.FillRectangle(brush, rectangle);

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

            FramesElapsed++;
            this.Invalidate();
        }


        /// <summary>
        /// Shows player stats at the end and a button to replay the game.
        /// </summary>
        private void EndGame()
        {
            this.ExitToMainScreen.Show();
            this.ExitToMainScreen.Left = Width / 2 - this.ExitToMainScreen.Size.Width / 2;
            this.Statistics.Show();
            this.Statistics.Left = Width / 2 - this.Statistics.Size.Width / 2;
            this.MaxMassLabel.Show();
            this.MaxPlayerMass.Text = "" + (int)MaxMass;
            this.MaxPlayerMass.Show();
            this.MaxMassLabel.Left = Width / 2 - (this.MaxPlayerMass.Width + this.MaxMassLabel.Width + 20) / 2;
            this.MaxPlayerMass.Left = this.MaxMassLabel.Right + 20;
        }


        /// <summary>
        /// Shows the main screen again.
        /// </summary>
        private void ShowMainScreen()
        {
            //Reset the world.
            lock(World)
                World = new World(Width, Height);

            //Get rid of the network thread so it isn't updating while no work is being done.
            NetworkThread.Abort();

            //Close the socket, not being used until player signs in again.
            socket.Close();
            socket.Dispose();

            //Show the normal labels and everything.
            this.nameLabel.Left = Width / 2 - (this.connectButton.Width + this.textBoxName.Width + this.textBoxServer.Width + this.nameLabel.Width + this.addressLabel.Width + 20) / 2;
            this.nameLabel.Show();
            this.textBoxName.Left = this.nameLabel.Right + 5;
            this.textBoxName.Show();
            this.addressLabel.Left = this.textBoxName.Right + 5;
            this.addressLabel.Show();
            this.textBoxServer.Left = this.addressLabel.Right + 5;
            this.textBoxServer.Show();
            this.connectButton.Left = this.textBoxServer.Right + 5;
            this.connectButton.Show();
            this.textBoxServer.ReadOnly = false;
            

            this.Paint -= this.Display_Paint;
        }


        /// <summary>
        /// Is called if the program is unable to connect to a server.
        /// </summary>
        private void UnableToConnect()
        {
            DialogResult result = MessageBox.Show("Unable to connect", "Connection Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
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
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Connect_Click(object sender, EventArgs e)
        {
            try
            {
                //save the socket so that it doesn't go out of scope or get garbage collected (happened a few times).
                socket = Network.Connect_to_Server(new Callback(SendName), textBoxServer.Text);
                
                this.connectButton.Hide();
                this.textBoxName.Hide();
                this.textBoxServer.Hide();
                this.textBoxServer.ReadOnly = true;
                this.nameLabel.Hide();
                this.addressLabel.Hide();
                this.MaxPlayerMass.Hide();
                this.MaxMassLabel.Hide();
                this.Statistics.Hide();

                lock(CubeData)
                    CubeData = new StringBuilder();

                FPStimer.Start();
            }
            catch (FormatException ex)
            {
                MessageBox.Show(ex.Message); // TODO: MAKE THIS WORK
            }
        }


        /// <summary>
        /// Sends the player name to the server. Callback method.
        /// </summary>
        /// <param name="state"></param>
        private void SendName(Preserved_State_Object state)
        {
            NetworkThread = Thread.CurrentThread;
            if (!socket.Connected)
            {
                this.Invoke(new MainMenu(UnableToConnect));
                return;
            }

            state.callback_function = new Callback(GetPlayerCube);
            string name = (textBoxName.Text == "") ? " " : textBoxName.Text;
            Network.Send(state.socket, name);
        }


        /// <summary>
        /// Gets the player cube from the server. Callback method.
        /// </summary>
        /// <param name="state"></param>
        private void GetPlayerCube(Preserved_State_Object state)
        {
            state.callback_function = new Callback(SendMoveReceiveData);

            Cube c = JsonConvert.DeserializeObject<Cube>(state.cubedata);
            PlayerID = c.uid;
            PlayerSplitID.Add(PlayerID);

            MaxMass = c.Mass;

            lock (World)
            {
                World.Cubes.Add(c.uid, c);
            }

            lock(CubeData)
                CubeData = new StringBuilder();

            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Display_Paint);
            this.Invalidate();

            // Set the default move coordinates to the player block's starting location
            PrevMouseLoc_x = (int)c.loc_x;
            PrevMouseLoc_y = (int)c.loc_y;

            Network.I_Want_More_Data(state);
        }

        /// <summary>
        /// Sends and receives data from the server. Callback method.
        /// </summary>
        /// <param name="state"></param>
        private void SendMoveReceiveData(Preserved_State_Object state)
        {
            if (socket.Connected)
            {
                lock (CubeData)
                {
                    CubeData.Append(state.cubedata);
                }

                // Send a move request following the convention: '(move, dest_x, dest_y)\n';

                string move = "(move, " + PrevMouseLoc_x + ", " + PrevMouseLoc_y + ")\n";
                Network.Send(socket, move);

                // Ask for more data
                Network.I_Want_More_Data(state);
            }
        }


        /// <summary>
        /// Deserializes cubes. Threadsafe.
        /// </summary>
        private void GetCubes()
        {
            if (!socket.Connected)
                return;

            //run on another thread
            if (CubeData.Length < 1)
                return;

            lock (CubeData)
            {
                String[] cubes = Regex.Split(CubeData.ToString(), "\n");
                string lastCube;

                lock (World)
                {
                    // Parse all cubes into the world except the last one
                    for (int i = 0; i < cubes.Length - 1; i++)
                    {
                        Cube c = JsonConvert.DeserializeObject<Cube>(cubes[i]);
                        World.Cubes[c.uid] = c;

                        if (c.Team_ID == PlayerID && !PlayerSplitID.Contains(c.uid))
                            PlayerSplitID.Add(c.uid);

                        if (c.Mass == 0)
                        {
                            //TODO: ENDgame scenario if player dies, does not have any split off cubes.
                            World.Cubes.Remove(c.uid);
                            PlayerSplitID.Remove(c.uid);
                        }
                    }

                    // Parse the last cube into the world only if it is complete
                    lastCube = cubes[cubes.Length - 1];
                    if (lastCube.Length > 0 && lastCube.Last() == '}')
                    {
                        Cube c = JsonConvert.DeserializeObject<Cube>(lastCube);
                        World.Cubes.Add(c.uid, c);
                        lastCube = "";
                    }
                }

                CubeData = new StringBuilder(lastCube);
            }
        }


        /// <summary>
        /// Button to click to exit to the main screen after a game-over scenario.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitToMainScreen_Click(object sender, EventArgs e)
        {
            ShowMainScreen();
            this.ExitToMainScreen.Hide();
        }


        /// <summary>
        /// Allows the space bar to work, send split requests.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Space && socket != null)
            {
                string split = "(split, " + PrevMouseLoc_x + ", " + PrevMouseLoc_y + ")\n";
                Network.Send(socket, split);
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

    }
}
