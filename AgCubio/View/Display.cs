﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

        private StringBuilder CubeData;

        private int PlayerID;

        private int PrevMouseLoc_x, PrevMouseLoc_y;

        HashSet<int> PlayerSplitID = new HashSet<int>();

        //Used to call the paint event.
        Timer timer;

        //Used to tell if the socket is connected.
        private bool Connected;

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
            DoubleBuffered = true;

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
        private void Display_Paint(object sender, PaintEventArgs e)
        {
            if (!Connected)
            {
                // Disconnection: proceed with this method call.
                MessageBox.Show("You've been disconnected.");
                ShowMainScreen();
                return;
            }

            GetCubes();

            lock (World)
            {
                //TODO: ENDGAME if world.cubes.containskey(playerid) is false.
                //Does this even go here?


                int split = 0;
                double totalMass = 0, Px = 0, Py = 0;
                
                foreach (int i in PlayerSplitID)
                {
                    if (World.Cubes.ContainsKey(i))
                    {
                        totalMass += World.Cubes[i].Mass;
                        split++;

                        //Some efforts to make perspective work when there are multiple cubes.
                        //Doesn't work yet.
                        Px += World.Cubes[i].loc_x;
                        Py += World.Cubes[i].loc_y;
                    }
                }

                //If the total mass of the player cube(s) is 0, it means that the game has ended.
                if (totalMass == 0)
                {
                    EndGame();
                    return;
                }

                else if (totalMass > MaxMass && MaxMass != 0) // second check?
                    MaxMass = totalMass;

                Px = Px / split;
                Py = Py / split;

                double scale = 5500 / (totalMass * (Math.Sqrt(split))); //Math.Sqrt(s);
                double width = Math.Sqrt(totalMass);

                //Mouse location is slightly off.
                PrevMouseLoc_x = (int)(Display.MousePosition.X + Px + width - Width / 2);
                PrevMouseLoc_y = (int)(Display.MousePosition.Y + Py + width - Height / 2);

                // DEBUG STUFF ---
                System.Diagnostics.Debug.WriteLine(PrevMouseLoc_x + " , " + PrevMouseLoc_y);
                // ---

                foreach (Cube c in World.Cubes.Values)
                {
                    Brush brush;
                    RectangleF rectangle;
                    // Draw food
                    if (c.food)
                    {
                        brush = new SolidBrush(Color.FromArgb(c.argb_color));
                        rectangle = new RectangleF((int)((c.loc_x - Px - c.width * scale * 2.5) * scale + Width / 2), (int)((c.loc_y - Py - c.width * scale * 2.5) * scale + Height / 2), (int)(c.width * scale * 5), (int)(c.width * scale * 5));
                        e.Graphics.FillRectangle(brush, rectangle);
                    }
                    // Draw user
                    else if (c.uid == PlayerID)
                    {
                        rectangle = new RectangleF((int)(Width / 2 - c.width / 2), (int)(Height / 2 - c.width / 2), (int)(c.width), (int)(c.width));
                        brush = new LinearGradientBrush(rectangle, Color.FromArgb(c.argb_color), Color.WhiteSmoke, LinearGradientMode.BackwardDiagonal);
                        e.Graphics.FillRectangle(brush, rectangle);

                        StringFormat stringFormat = new StringFormat();
                        stringFormat.Alignment = StringAlignment.Center;
                        stringFormat.LineAlignment = StringAlignment.Center;
                        int stringSize = (int)(c.width / (c.Name.Length + 1)) + 10;
                        e.Graphics.DrawString(c.Name, new Font(FontFamily.GenericSerif, stringSize, FontStyle.Italic, GraphicsUnit.Pixel),
                            new SolidBrush(Color.Black), rectangle /*new Point((int)c.loc_x,(int)c.loc_y)*/, stringFormat);
                    }
                    // Draw other players
                    // Writing the player name - sometimes, if the player is consumed, then the string is still written. Therefore, we only put a name if the mass is > 0
                    else if (c.Mass > 0)
                    {
                        rectangle = new RectangleF((int)((c.loc_x - Px) * scale + Width / 2), (int)((c.loc_y - Py) * scale + Height / 2), (int)(c.width), (int)(c.width));
                        brush = new LinearGradientBrush(rectangle, Color.FromArgb(c.argb_color), Color.WhiteSmoke, LinearGradientMode.BackwardDiagonal);
                        e.Graphics.FillRectangle(brush, rectangle);

                        StringFormat stringFormat = new StringFormat();
                        stringFormat.Alignment = StringAlignment.Center;
                        stringFormat.LineAlignment = StringAlignment.Center;
                        int stringSize = (int)(c.width / (c.Name.Length + 1)) + 10;
                        e.Graphics.DrawString(c.Name, new Font(FontFamily.GenericSerif, stringSize, FontStyle.Italic, GraphicsUnit.Pixel),
                            new SolidBrush(Color.Black), rectangle /*new Point((int)c.loc_x,(int)c.loc_y)*/, stringFormat);

                        if (c.Team_ID == PlayerID && !PlayerSplitID.Contains(c.uid))
                        {
                            PlayerSplitID.Add(c.uid);
                        }
                    }
                    else//mass is 0.
                    {
                        if (PlayerSplitID.Contains(c.uid))
                        {
                            //Does this code run?
                            PlayerSplitID.Remove(c.uid);
                        }
                    }
                }
            }
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
            this.MassLabel.Show();
            this.PlayerMass.Show();
            this.PlayerMass.Text = MaxMass.ToString();
            this.MassLabel.Left = Width / 2 - (this.PlayerMass.Width + this.MassLabel.Width + 20) / 2;
            this.PlayerMass.Left = this.MassLabel.Left + 20;
        }


        /// <summary>
        /// Shows the main screen again.
        /// </summary>
        private void ShowMainScreen()
        {
            timer.Stop();

            //Make sure the socket is disconnected.
            CleanSocket();

            //Make sure program knows we are disconnected right now.
            Connected = false;
            
            //Disconnects the paint method for now.
            //For some reason just stopping the timer didn't stop this from repainting?
            this.Paint -= new System.Windows.Forms.PaintEventHandler(this.Display_Paint);

            //Reset the world.
            World = new World(Width, Height);

            //Show the normal labels and everything.
            this.connectButton.Show();
            this.textBoxName.Show();
            this.textBoxServer.Show();
            this.nameLabel.Show();
            this.addressLabel.Show();
        }


        /// <summary>
        /// Disconnects the socket.
        /// </summary>
        private void CleanSocket()
        {
            socket.Blocking = true;

            //Get rid of old data.
            CubeData = new StringBuilder();
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
                this.nameLabel.Hide();
                this.addressLabel.Hide();
                Connected = true;

                this.Invalidate();
                if (timer == null)
                {
                    timer = new Timer();
                    timer.Interval = 25;
                    timer.Tick += new EventHandler(Timer_Tick);
                    timer.Start();
                }
                else
                {
                    timer.Start();
                }
            }
            catch (FormatException ex)
            {
                MessageBox.Show(ex.Message); // TODO: MAKE THIS WORK
                Connected = false;
            }
        }


        /// <summary>
        /// Paints the screen at each timer click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Timer_Tick(Object sender, EventArgs e)
        {
            this.Invalidate();
        }


        /// <summary>
        /// Sends the player name to the server. Callback method.
        /// </summary>
        /// <param name="state"></param>
        private void SendName(Preserved_State_Object state)
        {
            if (!socket.Connected)
            {
                Connected = false;
                this.Invoke(new MainMenu(UnableToConnect));
                return;
            }

            state.callback_function = new Callback(GetPlayerCube);
            Network.Send(state.socket, textBoxName.Text);
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

            lock (World)
            {
                World.Cubes.Add(c.uid, c);
            }

            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Display_Paint);
            this.Invalidate();

            // Set the default move coordinates to the player block's starting location
            PrevMouseLoc_x = (int)Width / 2;
            PrevMouseLoc_y = (int)Height / 2;

            Network.I_Want_More_Data(state);
        }

        /// <summary>
        /// Sends and receives data from the server. Callback method.
        /// </summary>
        /// <param name="state"></param>
        private void SendMoveReceiveData(Preserved_State_Object state)
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


        /// <summary>
        /// Deserializes cubes. Threadsafe.
        /// </summary>
        private void GetCubes()
        {
            if (!socket.Connected)
            {
                Connected = false;
                return;
            }

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

                        if (c.Mass == 0)
                        {
                            //TODO: ENDgame scenario if player dies, does not have any split off cubes.
                            World.Cubes.Remove(c.uid);
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
            this.PlayerMass.Hide();
            this.MassLabel.Hide();
        }


        /// <summary>
        /// Allows the space bar to work, send split requests.
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Space && Connected)
            {
                string split = "(split, " + PrevMouseLoc_x + ", " + PrevMouseLoc_y + ")\n";
                Network.Send(socket, split);
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
