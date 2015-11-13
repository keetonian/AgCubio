using Newtonsoft.Json;
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

        private World World;

        private Socket socket;

        private StringBuilder CubeData;

        private int PlayerID;

        private int PrevMouseLoc_x, PrevMouseLoc_y;


        /// <summary>
        /// 
        /// </summary>
        public Display()
        {
            World = new World(1000, 1000);
            CubeData = new StringBuilder();
            InitializeComponent();
            DoubleBuffered = true;
        }


        /// <summary>
        /// 
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
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Display_Paint(object sender, PaintEventArgs e)
        {
            GetCubes();

            lock (World)
            {
                //TODO: ENDGAME if world.cubes.containskey(playerid) is false.
                //DOes this even go here?

                double s =  1000/ World.Cubes[PlayerID].width;
                double scale = 5500/World.Cubes[PlayerID].Mass;//Math.Sqrt(s);

                double Px = World.Cubes[PlayerID].loc_x;
                double Py = World.Cubes[PlayerID].loc_y;

                //Mouse location is slightly off.
                PrevMouseLoc_x = (int)(Display.MousePosition.X + Px - Width/2);
                PrevMouseLoc_y = (int)(Display.MousePosition.Y + Py - Height/2);

                System.Diagnostics.Debug.WriteLine(PrevMouseLoc_x + " , " + PrevMouseLoc_y);


                foreach (Cube c in World.Cubes.Values)
                {
                    Brush brush;
                    RectangleF rectangle;
                    //Food
                    if (c.food)
                    {
                        brush = new SolidBrush(Color.FromArgb(c.argb_color));
                        rectangle = new RectangleF((int)((c.loc_x - Px - c.width*scale*2.5)*scale + Width/2), (int)((c.loc_y - Py - c.width * scale * 2.5) *scale + Height/2), (int)(c.width*scale*5), (int)(c.width*scale*5));
                        e.Graphics.FillRectangle(brush, rectangle);
                    }
                    else if(c.uid == PlayerID)
                    {
                        rectangle = new RectangleF((int)(Width/2 - c.width/2), (int)(Height/ 2 -c.width/2), (int)(c.width), (int)(c.width));
                        brush = new LinearGradientBrush(rectangle, Color.FromArgb(c.argb_color), Color.WhiteSmoke, LinearGradientMode.BackwardDiagonal);
                        e.Graphics.FillRectangle(brush, rectangle);

                        StringFormat stringFormat = new StringFormat();
                        stringFormat.Alignment = StringAlignment.Center;
                        stringFormat.LineAlignment = StringAlignment.Center;
                        int stringSize = (int)(c.width / (c.Name.Length + 1)) + 10;
                        e.Graphics.DrawString(c.Name, new Font(FontFamily.GenericSerif, stringSize, FontStyle.Italic, GraphicsUnit.Pixel),
                            new SolidBrush(Color.Black), rectangle /*new Point((int)c.loc_x,(int)c.loc_y)*/, stringFormat);
                    }
                    //Player
                    //writing the player name. Sometimes, if the player is consumed, then the string is still written. Therefore, we only put a name if the mass is > 0
                    else if (c.Mass > 0)
                    {
                        rectangle = new RectangleF((int)((c.loc_x - Px)*scale + Width/2), (int)((c.loc_y - Py)*scale + Height/2), (int)(c.width), (int)(c.width));
                        brush = new LinearGradientBrush(rectangle, Color.FromArgb(c.argb_color), Color.WhiteSmoke, LinearGradientMode.BackwardDiagonal);
                        e.Graphics.FillRectangle(brush, rectangle);

                        StringFormat stringFormat = new StringFormat();
                        stringFormat.Alignment = StringAlignment.Center;
                        stringFormat.LineAlignment = StringAlignment.Center;
                        int stringSize = (int)(c.width / (c.Name.Length+1)) + 10;
                        e.Graphics.DrawString(c.Name, new Font(FontFamily.GenericSerif, stringSize, FontStyle.Italic, GraphicsUnit.Pixel),
                            new SolidBrush(Color.Black), rectangle /*new Point((int)c.loc_x,(int)c.loc_y)*/, stringFormat);
                    }
                }
            }            
            this.Invalidate();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            //save the socket so that it doesn't go out of scope or get garbage collected (happened a few times).
            try
            {
                socket = Network.Connect_to_Server(new Callback(SendName), textBoxServer.Text);

                this.connectButton.Hide();
                this.textBoxName.Hide();
                this.textBoxServer.Hide();
                this.nameLabel.Hide();
                this.addressLabel.Hide();
            }
            catch (FormatException ex)
            {
                MessageBox.Show(ex.Message); // TODO: MAKE THIS WORK
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        private void SendName(Preserved_State_Object state)
        {
            state.callback_function = new Callback(GetPlayerCube);
            Network.Send(state.socket, textBoxName.Text);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        private void GetPlayerCube(Preserved_State_Object state)
        {
            state.callback_function = new Callback(SendMoveReceiveData);

            Cube c = JsonConvert.DeserializeObject<Cube>(state.cubedata);
            PlayerID = c.uid;

            lock (World)
            {
                World.Cubes.Add(c.uid, c);
            }

            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Display_Paint);
            this.Invalidate();

            // Set the default move coordinates to the player block's starting location
            PrevMouseLoc_x = (int)Width/2;
            PrevMouseLoc_y = (int)Height/2;

            Network.I_Want_More_Data(state);
        }

        /// <summary>
        /// 
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
        /// 
        /// </summary>
        private void GetCubes()
        {
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
        /// 
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
