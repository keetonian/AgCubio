using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Timers;
using MySql.Data.MySqlClient;

// Created by Keeton Hodgson and Daniel Avery
namespace AgCubio
{
    /// <summary>
    /// Server manages all data interactions with clients
    /// </summary>
    public class Server
    {
        /// <summary>
        /// Contains our socket connections and stat trackers for each connection
        /// </summary>
        private Dictionary<Socket, ScoreInformation> Sockets;

        /// <summary>
        /// World object for storing and managing the game state
        /// </summary>
        private World World;

        /// <summary>
        /// Timer that controls updates
        /// </summary>
        private Timer Heartbeat;

        /// Move requests for players, updated with each timer tick
        ///   cube uid => coordinates
        /// </summary>
        private Dictionary<int, Tuple<double, double>> DataReceived;

        /// <summary>
        /// String we need to connect to our database
        /// </summary>
        private const string connectionString = "server=atr.eng.utah.edu;database=cs3500_hodgson;uid=cs3500_hodgson;password=AveryIsMyPassword";

        /// <summary>
        /// We count the id's in c# instead of the DB so that it is easier to put the same id in both of our DB tables
        /// </summary>
        private int GameIdCounter;


        /// <summary>
        /// Create a server and pause the console
        /// </summary>
        public static void Main(string[] args)
        {
            new Server();
            Console.ReadLine();
        }


        /// <summary>
        /// Constructor - initializes server fields and begins awaiting client connections
        /// </summary>
        public Server()
        {
            World        = new World("..\\..\\..\\Project1/Resources/World_Params.xml");
            Sockets      = new Dictionary<Socket, ScoreInformation>();
            DataReceived = new Dictionary<int, Tuple<double, double>>();
            Heartbeat    = new Timer(1000 / World.HEARTBEATS_PER_SECOND);
            Heartbeat.Elapsed += new ElapsedEventHandler(HeartBeatTick);
            Heartbeat.Start();

            CreateDatabaseTables();

            // Set up game server
            Network.Server_Awaiting_Client_Loop(new Network.Callback(SetUpClient), 11000);

            // Set up web server
            Network.Server_Awaiting_Client_Loop(new Network.Callback(HighScores), 11100);

            Console.WriteLine("Server awaiting client connection...");
        }




        /// <summary>
        /// Sets up the database for the web server
        /// </summary>
        private void CreateDatabaseTables()
        {
            string dropOldTables = "DROP TABLE IF EXISTS Eaten, Players;";
            string createPlayerTable = @"CREATE TABLE Players(GameId INT PRIMARY KEY, Name VARCHAR(25), Lifetime VARCHAR(25),
                MaxMass DOUBLE (10,2), HighestRank INT, CubesEaten INT, NumPlayersEaten INT, TimeofDeath VARCHAR(25));";
            string createEatenTable = @"CREATE TABLE Eaten(Id INT PRIMARY KEY AUTO_INCREMENT, GameId INT, Name VARCHAR(25),
                EatenPlayer VARCHAR(25), FOREIGN KEY(GameId) REFERENCES Players(GameId) ON DELETE CASCADE)";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    MySqlCommand dropTables = new MySqlCommand(dropOldTables, conn);
                    MySqlCommand createPlayers = new MySqlCommand(createPlayerTable, conn);
                    MySqlCommand createEaten = new MySqlCommand(createEatenTable, conn);

                    conn.Open();
                    dropTables.ExecuteNonQuery();
                    createPlayers.ExecuteNonQuery();
                    createEaten.ExecuteNonQuery();
                    conn.Close();
                }
                catch (Exception e)
                { Console.WriteLine(e.Message); }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private void HighScores(Preserved_State_Object state)
        {
            string query = Regex.Split(state.data.ToString(), "\r\n")[0];
            string score = @"GET /scores";//actually just test if matches string
            string games = @"GET /games\?player="; // Maybe?
            string eaten = @"GET /eaten\?id=";
            string ending = @" HTTP/1.1";

            if (Regex.IsMatch(query, score))
            {
                /* Upon receiving "GET /scores HTTP/1.1" the server will send back an HTML web page containing a table of information reporting all recorded scores.
                This should include, the length of time alive, the maximum mass, the highest rank, and the number of cubes eaten.
                The HTML table should have one row for each player/game in the database and one column for each of the above required pieces of information.
                A superior solution would create links in this table to the information described below (e.g., clicking on a players name would invoke case 2 below).*/


                //scores
                string dbQuery = "Select * from Players";// Id can link to players eaten?

                Network.Send(state.socket, StatsHTML(new MySqlCommand(dbQuery), 1, "AgCubio Stats | High Scores"), true);

            }
            else if (Regex.IsMatch(query, games))
            {
                /*Upon receiving "GET /games?player=Joe HTTP/1.1" the server will send back an HTML web page containing a table of information reporting all games by the player "Joe".
                There should be one row for each game played by the player named in the line of text (e.g., "Joe" in the example above) and a column for each piece of information.
                In addition to the above information, the time of death should be shown and the number of players eaten should be shown.
                A superior solution would also have links to the main score table page and to the list of eaten players for a particular game.*/

                query = Regex.Replace(query, "(" + games + ")|(" + ending + ")", "");// get the name

                string dbQuery = "Select * from Players where name = '" + query + "'";

                Network.Send(state.socket, StatsHTML(new MySqlCommand(dbQuery), 2, "AgCubio Stats | Player: " + query), true);

            }
            else if (Regex.IsMatch(query, eaten))
            {
                /*Upon receiving "GET /eaten?id=35 HTTP/1.1" the server should send back an HTML page containing information about the specified game session (e.g., 35 in this example).
                The page should contain all information about the players game, but most importantly highlight the names of players who were eaten. A superior solution would turn "eaten player" names into links to their high scores.
                If there the specified game does not exist, treat this as an "anything else" case as discussed below. As always, a superior solution will have links from this page to other related pages.*/

                // eaten
                query = Regex.Replace(query, "(" + eaten + ")|(" + ending + ")", ""); //get the id

                string dbQuery = "Select * from Eaten natural join Players where GameId = " + query;

                Network.Send(state.socket, StatsHTML(new MySqlCommand(dbQuery), 3, "AgCubio Stats | Game ID: " + query), true );

            }
            else
            {

                /*If the first line of text sent by the browser to the server is anything else, the server should send back an HTML page containing an error message. The error message should be meaningful and contain a summary of valid options.*/
                Network.Send(state.socket, HTMLGenerator.GenerateError("Invalid web address"), true);

            }

        }


        /// <summary>
        /// 
        /// </summary>
        private string StatsHTML(MySqlCommand query, int queryNum = 0, string title = "")
        {
            string html = "";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    // Open a connection
                    conn.Open();
                    query.Connection = conn;
                    string body;

                    // Execute the command and cycle through the DataReader object
                    using (MySqlDataReader reader = query.ExecuteReader())
                    {
                        string table;
                        StringBuilder rows = new StringBuilder();

                        if (queryNum == 1)
                        {
                            rows.Append(HTMLGenerator.TableRow(HTMLGenerator.TableHData("Game Id") +
                                HTMLGenerator.TableHData("Name") +
                                HTMLGenerator.TableHData("Lifetime") +
                                HTMLGenerator.TableHData("Max Mass") +
                                HTMLGenerator.TableHData("Highest Rank") +
                                HTMLGenerator.TableHData("Number of Cubes Eaten")));

                            while (reader.Read())
                            {
                                string id = HTMLGenerator.TableData(HTMLGenerator.GenerateLink("http://localhost:11100/eaten?id=" + reader["GameId"].ToString(), reader["GameId"].ToString()));
                                string name = HTMLGenerator.TableData(HTMLGenerator.GenerateLink("http://localhost:11100/games?player=" + reader["Name"].ToString(), reader["Name"].ToString()));
                                string lifetime = HTMLGenerator.TableData(reader["Lifetime"].ToString());
                                string maxmass = HTMLGenerator.TableData(reader["MaxMass"].ToString());
                                string highestRank = HTMLGenerator.TableData(reader["HighestRank"].ToString());
                                string cubesEaten = HTMLGenerator.TableData(reader["CubesEaten"].ToString());

                                rows.Append(HTMLGenerator.TableRow(id + name + lifetime + maxmass + highestRank + cubesEaten));
                            }
                        }
                        else if (queryNum == 2)
                        {
                            rows.Append(HTMLGenerator.TableRow(HTMLGenerator.TableHData("GameId") +
                               HTMLGenerator.TableHData("Name") +
                               HTMLGenerator.TableHData("Lifetime") +
                               HTMLGenerator.TableHData("Max Mass") +
                               HTMLGenerator.TableHData("Highest Rank") +
                               HTMLGenerator.TableHData("Number of Cubes Eaten") +
                               HTMLGenerator.TableHData("Time Of Death") +
                               HTMLGenerator.TableHData("Number of Players Eaten")));
                            

                            while (reader.Read())
                            {
                                string id = HTMLGenerator.TableData(HTMLGenerator.GenerateLink("http://localhost:11100/eaten?id=" + reader["GameId"].ToString(), reader["GameId"].ToString()));
                                string name = HTMLGenerator.TableData(HTMLGenerator.GenerateLink("http://localhost:11100/games?player=" + reader["Name"].ToString(), reader["Name"].ToString()));
                                string lifetime = HTMLGenerator.TableData(reader["Lifetime"].ToString());
                                string maxmass = HTMLGenerator.TableData(reader["MaxMass"].ToString());
                                string highestRank = HTMLGenerator.TableData(reader["HighestRank"].ToString());
                                string cubesEaten = HTMLGenerator.TableData(reader["CubesEaten"].ToString());
                                string timeofdeath = HTMLGenerator.TableData(reader["TimeofDeath"].ToString());
                                string numplayerseat = HTMLGenerator.TableData(reader["NumPlayersEaten"].ToString());

                                rows.Append(HTMLGenerator.TableRow(id + name + lifetime + maxmass + highestRank + cubesEaten + timeofdeath + numplayerseat));
                            }
                        }
                        else if (queryNum == 3)
                        {
                            rows.Append(HTMLGenerator.TableRow(HTMLGenerator.TableHData("GameId") +
                              HTMLGenerator.TableHData("Name") +
                              HTMLGenerator.TableHData("Lifetime") +
                              HTMLGenerator.TableHData("Max Mass") +
                              HTMLGenerator.TableHData("Highest Rank") +
                              HTMLGenerator.TableHData("Number of Cubes Eaten") +
                              HTMLGenerator.TableHData("Time Of Death") +
                              HTMLGenerator.TableHData("Number of Players Eaten") +
                              HTMLGenerator.TableHData("Eaten Player Names")));

                            int add = 0;
                            StringBuilder eaten = new StringBuilder();
                            string others = "";
                            while (reader.Read())
                            {
                                string id = HTMLGenerator.TableData(HTMLGenerator.GenerateLink("http://localhost:11100/eaten?id=" + reader["GameId"].ToString(), reader["GameId"].ToString()));
                                string name = HTMLGenerator.TableData(HTMLGenerator.GenerateLink("http://localhost:11100/games?player=" + reader["Name"].ToString(), reader["Name"].ToString()));
                                string lifetime = HTMLGenerator.TableData(reader["Lifetime"].ToString());
                                string maxmass = HTMLGenerator.TableData(reader["MaxMass"].ToString());
                                string highestRank = HTMLGenerator.TableData(reader["HighestRank"].ToString());
                                string cubesEaten = HTMLGenerator.TableData(reader["CubesEaten"].ToString());
                                string timeofdeath = HTMLGenerator.TableData(reader["TimeofDeath"].ToString());
                                string numplayerseat = HTMLGenerator.TableData(reader["NumPlayersEaten"].ToString());
                                string eatenname = HTMLGenerator.TableData(HTMLGenerator.GenerateLink("http://localhost:11100/games?player=" + reader["EatenPlayer"].ToString(), reader["EatenPlayer"].ToString()));
                                eaten.Append("<p>" + HTMLGenerator.GenerateLink("http://localhost:11100/games?player=" + reader["EatenPlayer"].ToString(), reader["EatenPlayer"].ToString()) + "</p>");
                                if (add < 1)
                                    others = id + name + lifetime + maxmass + highestRank + cubesEaten + timeofdeath + numplayerseat;
                                add++;
                            }
                            others += HTMLGenerator.TableData(eaten.ToString());
                            rows.Append(HTMLGenerator.TableRow(others));
                        }
                        else
                        {

                        }
                        
                        table = HTMLGenerator.Table(rows.ToString());
                        body = HTMLGenerator.Body(table, title);
                        html = HTMLGenerator.GenerateHeader(title, body);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    html = HTMLGenerator.GenerateError("Something went wrong in the database query");
                }
            }
            return html;
        }


        /// <summary>
        /// Main callback method for setting up a client.
        /// </summary>
        private void SetUpClient(Preserved_State_Object state)
        {
            Console.WriteLine("User " + state.data + " has connected to the server.");

            // Generate 2 random starting coords within our world, check if other players are there, then send if player won't get eaten immediately (helper method)
            double x, y;
            Cube cube;
            string worldData;

            lock (World)
            {
                World.FindStartingCoords(out x, out y, false);
                cube = new Cube(x, y, World.GetUid(), false, state.data.ToString(), World.PLAYER_START_MASS, World.GetColor(), 0);
                World.Cubes[cube.uid] = cube;
                worldData = World.SerializeAllCubes();

                World.DatabaseStats.Add(cube.uid, new World.StatTracker(state.data.ToString()));
            }

            state.CubeID = cube.uid;
            state.data.Clear();
            state.callback = new Network.Callback(ManageData);

            // Send the client's cube and then all of the world data
            Network.Send(state.socket, JsonConvert.SerializeObject(cube) + "\n");
            Network.Send(state.socket, worldData);

            lock (Sockets)
            {
                Sockets.Add(state.socket, new ScoreInformation(cube.uid));
            }

            // Ask for more data from client
            Network.I_Want_More_Data(state);
        }


        /// <summary>
        /// Method to send and receive data from client
        /// </summary>
        private void ManageData(Preserved_State_Object state)
        {
            // Try to perform the complete move or split actions
            string[] actions = Regex.Split(state.data.ToString(), @"\n");

            for (int i = 0; i < actions.Length - 1; i++)
                TryMoveOrSplit(actions[i], state);

            // Try to perform the last move or split action if it is complete, otherwise append what is there for later
            string lastAction = actions.Last();

            if (lastAction.Length > 1 && lastAction.Last() == ')')
                TryMoveOrSplit(lastAction, state);
            else
                state.data = new StringBuilder(lastAction);

            // Call for more client actions
            Network.I_Want_More_Data(state);
        }


        /// <summary>
        /// Parses client requests into moves or splits
        /// </summary>
        private void TryMoveOrSplit(String str, Preserved_State_Object state)
        {
            // Get the coordinates for the move or split
            MatchCollection values = Regex.Matches(str, @"-*\d+");
            double x = double.Parse(values[0].Value);
            double y = double.Parse(values[1].Value);

            // Handle moving or splitting
            //   *NOTE: Cubes are not actually moved here, as that could lead to more or less movement per player in a given amount of time (based on connection speed)
            //          - instead, movement direction is appended to a stringbuilder and dealt with all at the same time in the server's heartbeat tick
            if (str[1] == 'm')
                lock (DataReceived) { DataReceived[state.CubeID] = new Tuple<double, double>(x, y); }
            else if (str[1] == 's')
                lock (World) { World.Split(state.CubeID, x, y); }
        }


        /// <summary>
        /// Every timer tick:
        ///   Updates the world (adds food, etc)
        ///   Sends updates to the clients
        /// </summary>
        private void HeartBeatTick(object state, ElapsedEventArgs e)
        {
            // Prevent this eventhandler from firing again while we're handling it
            Heartbeat.Enabled = false;

            // Data to send to all of the clients
            StringBuilder data = new StringBuilder();
            List<Tuple<int, double>> sortedByMass = new List<Tuple<int, double>>();

            lock (World)
            {
                // Players get a little smaller each tick, and military viruses move along their path
                World.PlayerAttrition();
                World.MoveMilitaryVirus();

                // Move all players according to last mouse position
                lock (DataReceived)
                {
                    // Uid's to be removed
                    List<int> toBeRemoved = new List<int>();
                    // Move all player cubes according to last mouse position
                    foreach (int uid in DataReceived.Keys)
                    {
                        // If the cube has been removed from the world, mark it to be removed and skip moving
                        if (!World.Cubes.ContainsKey(uid))
                        {
                            toBeRemoved.Add(uid);
                            continue;
                        }
                        // Get all of the current masses for ranking purposes
                        sortedByMass.Add(new Tuple<int, double>(uid, World.DatabaseStats[uid].CurrentMass));

                        World.Move(uid, DataReceived[uid].Item1, DataReceived[uid].Item2); 
                    }

                    // Remove marked cubes
                    foreach (int uid in toBeRemoved)
                        DataReceived.Remove(uid);
                }

                // Check for collisions, eat some food
                data.Append(World.ManageCollisions());

                // Add food to the world if necessary and append it to the data stream
                for (int i = 0; i < World.FOOD_PER_HEARTBEAT && World.Food.Count < World.MAX_FOOD_COUNT; i++)
                    data.Append(JsonConvert.SerializeObject(World.GenerateFoodorVirus()) + "\n");

                // Appends all of the player cube data - they should be constantly changing (mass, position, or both), therefore, we send them every time
                data.Append(World.SerializePlayers());
            }

            sortedByMass.Sort((a, b) => b.Item2.CompareTo(a.Item2));
            
            // Send data to sockets
            lock (Sockets)
            {
                

                List<Socket> disconnected = new List<Socket>();
                foreach(Socket s in Sockets.Keys)
                {
                    for (int i = 0; i < 5 && i < sortedByMass.Count; i++)
                    {
                        if (sortedByMass[i].Item1 == Sockets[s].Uid && (Sockets[s].HighestRank == 0 || Sockets[s].HighestRank > (i + 1)))
                            Sockets[s].HighestRank = (i + 1);
                    }

                    // Remove sockets that are no longer connected
                    if (!s.Connected)
                    {
                        disconnected.Add(s);
                        continue;
                    }

                    Network.Send(s, data.ToString());
                }

                foreach(Socket s in disconnected)
                {
                    // Add data to database
                    ScoreInformation score = Sockets[s];
                    TimeSpan playtime = score.Playtime.Elapsed;
                    String formattedPlaytime = (playtime.Days * 24 + playtime.Hours) + "h " + playtime.Minutes + "m " + playtime.Seconds + "s";
                    World.StatTracker stats;
                    lock (World) { stats = World.DatabaseStats[Sockets[s].Uid]; }
                                        
                    // A little tricky here - we don't actually want to set the highest rank in the DB if it has not been set in the server. String magic does the job
                    string highestRankColumn = (score.HighestRank == 0) ? "" : " HighestRank,";
                    string highestRankValue = (score.HighestRank == 0) ? "" : " " + score.HighestRank + ",";

                    string insertPlayerData = String.Format("INSERT INTO Players(GameId, Name, Lifetime, MaxMass,{0} CubesEaten, TimeofDeath, NumPlayersEaten) "
                        + "VALUES({1}, '{2}', '{3}', {4},{5} {6}, '{7}',{8});",
                        highestRankColumn, ++GameIdCounter, stats.Name, formattedPlaytime, stats.MaxMass, highestRankValue, stats.CubesConsumed, DateTime.Now, stats.PlayersEaten.Count);
                    
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        try
                        {
                            MySqlCommand insertData = new MySqlCommand(insertPlayerData, conn);

                            conn.Open();
                            insertData.ExecuteNonQuery();

                            if(stats.PlayersEaten.Count == 0)
                                (new MySqlCommand(String.Format("INSERT INTO Eaten(GameId, Name) VALUES({0}, '{1}');",
                                  GameIdCounter, stats.Name), conn)).ExecuteNonQuery();
                            else
                                foreach (string eatenName in stats.PlayersEaten)
                                    (new MySqlCommand(String.Format("INSERT INTO Eaten(GameId, Name, EatenPlayer) VALUES({0}, '{1}', '{2}');",
                                     GameIdCounter, stats.Name, eatenName), conn)).ExecuteNonQuery();

                            conn.Close();
                        }
                        catch(Exception ex)
                        { Console.WriteLine(ex.Message); }
                    }

                    // Remove this player
                    lock (World) { World.DatabaseStats.Remove(Sockets[s].Uid); }
                    lock(DataReceived) { DataReceived.Remove(Sockets[s].Uid); }
                    Sockets.Remove(s);
                }
            }

            // Event can now fire again
            Heartbeat.Enabled = true;
        }


        /// <summary>
        /// Keeps track of a few of the player stat elements that the server has easier access to than the world
        ///   Uid
        ///   Playtime
        ///   HighestRank
        /// </summary>
        class ScoreInformation
        {
            public int Uid;
            public Stopwatch Playtime;
            public int HighestRank;

            public ScoreInformation(int uid)
            {
                Uid = uid;
                Playtime = new Stopwatch();
                Playtime.Start();
            }
        }
    }


    public static class HTMLGenerator
    {

        public static string GenerateError(string message)
        {
            return HTMLGenerator.GenerateHeader("Error", HTMLGenerator.Body("<p>" + message + "</p>", "Error!"));
        }

        public static string GenerateHeader(string title, string content = "")
        {
            return @"HTTP/1.1 200 OK \r\n
Connection: close \r\n
Content-Type: text/html; charset=UTF-8 \r\n
\r\n

<!DOCTYPE html>

<html lang=""en"" mlns=""http://www.w3.org/1999/xhtml"">
    <head>
        <meta charset=""utf-8""/>
        <title>" + title + @"</title>
    </head>" + content +
    "</html>";
        }

        /// <summary>
        /// Data cells inside of table rows
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string TableData(string data)
        {
            return @"<td style=""white-space: nowrap;"" bgcolor=""efefef"">" + data + "</td>";
        }

        public static string TableHData(string data)
        {
            return @"<th bgcolor=""red"">" + data + "</th>";
        }


        /// <summary>
        /// Table rows, contains table datas
        /// </summary>
        /// <param name="rowContents"></param>
        /// <returns></returns>
        public static string TableRow(string rowContents)
        {
            return @"<tr align=""center"">" + rowContents + "</tr>";
        }


        public static string Table(string tableContents)
        {
            return @"<div align=""center""><table border=""1"" style=""border-collapse: collapse; font-size: 30px;"" cellpadding=""10"">" + tableContents + "</table></div>";
        }

        public static string Body(string bodyContents, string title = "")
        {
            return @"<body><div align=""center""><h1>" + title + "</h1>" + bodyContents + "<h1>" + HTMLGenerator.GenerateLink("http://localhost:11100/scores", "High Scores") + "</h1></div></body>";
        }


        /// <summary>
        /// Creates a link
        /// </summary>
        /// <param name="link"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public static string GenerateLink(string link, string info)
        {
            return String.Format(@"<a href=""{0}"">{1}</a>", link, info);
        }
    }
}