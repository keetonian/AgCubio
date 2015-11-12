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

        private bool connected;


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
            //if (!connected)
            //    return;

            GetCubes();

            lock(World)
            {
                foreach (Cube c in World.Cubes.Values)
                {
                    Brush brush = new SolidBrush(Color.FromArgb(c.argb_color));

                    e.Graphics.FillRectangle(brush, new Rectangle((int)c.left, (int)c.top, (int)c.width, (int)c.width));
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
            PlayerID = c.Team_ID;

            connected = true;

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
        


        private void Display_MouseMove(object sender, MouseEventArgs e)
        {
            if (!connected)
                return;

            Cube player = World.Cubes[PlayerID];

            System.Diagnostics.Debug.WriteLine("Mouse_X: " + e.X + " Cube_X: " + player.loc_x + " Mouse_Y: " + e.Y + " Cube_Y: " + player.loc_y);
            double angle = Math.Atan2(player.loc_y - e.Y, e.X - player.loc_x);
            //System.Diagnostics.Debug.WriteLine(angle * 180 / Math.PI);
            //Need a direction
            //Need a speed
            string move = "(move, " + e.X + ", " + e.Y + ")\n";
            Network.Send(socket, move);
            //'(move, dest_x, dest_y)\n';
        }

        private void Display_MouseHover(object sender, EventArgs e)
        {
            
        }
    }
}
