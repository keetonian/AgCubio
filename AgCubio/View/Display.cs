using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
        public delegate void Callback(Preserved_State_Object state);

        private World World;

        private Socket socket;

        private StringBuilder CubeData;

        private int PlayerID;

        private double PrevMouseLoc_x, PrevMouseLoc_y;

        
        /// <summary>
        /// 
        /// </summary>
        public Display()
        {
            World = new World(1000,1000);
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

            lock(World)
            {
                foreach (Cube c in World.Cubes.Values)
                {
                    //Cube
                    Brush brush = new SolidBrush(Color.FromArgb(c.argb_color));
                    RectangleF rectangle = new RectangleF((int)c.left, (int)c.top, (int)c.width, (int)c.width);
                    e.Graphics.FillRectangle(brush, rectangle);

                    //writing the player name. Sometimes, if the player is consumed, then the string is still written. Therefore, we only put a name if the mass is > 0
                    if (!c.food && c.Mass > 0)
                    {
                        StringFormat stringFormat = new StringFormat();
                        stringFormat.Alignment = StringAlignment.Center;
                        stringFormat.LineAlignment = StringAlignment.Center;
                        int stringSize = (int)(c.width / c.Name.Length) + 10;
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
            this.button1.Hide();
            this.textBoxName.Hide();
            this.textBoxServer.Hide();
            this.label1.Hide();
            this.label2.Hide();

            //save the socket so that it doesn't go out of scope or get garbage collected (happened a few times).
            try
            {
                socket = Network.Connect_to_Server(new Callback(SendName), textBoxServer.Text);
            }
            catch(FormatException ex)
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
            state.callback_function = new Callback(GetData);

            Cube c = JsonConvert.DeserializeObject<Cube>(state.cubedata);
            PlayerID = c.uid;

            // Set the default move coordinates to the player block's starting location
            PrevMouseLoc_x = c.loc_x;
            PrevMouseLoc_y = c.loc_y;

            Network.I_Want_More_Data(state);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        private void GetData(Preserved_State_Object state)
        {
            lock(CubeData)
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
            
            lock(CubeData)
            {
                String[] cubes = Regex.Split(CubeData.ToString(), "\n");
                string lastCube;

                lock(World)
                {
                    // Parse all cubes into the world except the last one
                    for (int i = 0; i < cubes.Length - 1; i++)
                    {
                        Cube c = JsonConvert.DeserializeObject<Cube>(cubes[i]);
                        if(c.Mass == 0)
                        {
                            World.Cubes.Remove(c.uid);
                        }
                        World.Cubes[c.uid] = c;
                    }

                    // Parse the last cube into the world only if it is complete
                    lastCube = cubes[cubes.Length - 1];
                    if(lastCube.Length > 0 && lastCube.Last() == '}')
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
        private void Display_MouseMove(object sender, MouseEventArgs e)
        {
            // Get new coordinates to move to from the mouse
            PrevMouseLoc_x = e.X;
            PrevMouseLoc_y = e.Y;
        }


        /// <summary>
        /// 
        /// </summary>
        private void Display_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Space)
            {
                // Send a move request following the convention: '(move, dest_x, dest_y)\n';
                string split = "(split, " + PrevMouseLoc_x + ", " + PrevMouseLoc_y + ")\n";
                Network.Send(socket, split);
            }
        }
    }
}
