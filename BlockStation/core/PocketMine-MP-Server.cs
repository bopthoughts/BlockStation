using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Threading;
using System.IO;
using Kajabity.Tools.Java;
using Microsoft.VisualBasic;
using System.Windows;
using System.ComponentModel;
using fNbt;

namespace BlockStation
{
    public class Player
    {
        public Player(string n)
        {
            Name = n;

        }

        public string Name { get; set; }
        public short Health { get; set; }
        public DateTime LastOnline { get; set; }
        public DateTime FirstTimeOnline { get; set; }

        public override string ToString()
        {
            return this.Name;
        }

    }


    public class PocketMine_MP_Server
    {
        // Konstruktor
        public PocketMine_MP_Server(string d)
        {
            Whitelist = new List<Player>();
            PlayerList = new Dictionary<string, Player>();


            dir = d;
            read_server_props();


            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += new DoWorkEventHandler(CheckServerAvailability);

            query = new Query.MCQuery();
            ReadPlayerData();


        }

        // Alle Spieler
        public IDictionary<string, Player> PlayerList;

        // Whitelist
        public List<Player> Whitelist;

        // Prozess für PocketMine
        private Process PocketMineProcess;

        // Server Ausgabe
        private string ServerOutput;

        // Public Server Properties
        public int OnlinePlayers
        {
            get
            {
                var info = query.Info();
                if (Online)
                {
                    return info.OnlinePlayers;
                }
                else
                {
                    return 0;
                }
            }
        }
        public int Latency
        {
            get
            {
                var info = query.Info();
                if (Online)
                {
                    return info.Latency;
                }
                else
                {
                    return 0;
                }
            }
        }
        public string Version
        {
            get
            {
                var info = query.Info();
                if (Online)
                {
                    return info.Plugins;
                }
                else
                {
                    return "";
                }
            }
        }
        public bool Online
        {
            get {
                if (worker.IsBusy)
                {
                    return online;
                }
                else
                {
                    worker.RunWorkerAsync();
                    return online;
                }
            }
            set { online = value; }
        }
        public string Name
        {
            get { return prop_server_name; }
            set { prop_server_name = value; }
        }
        public int Port
        {
            get { return Int32.Parse(prop_server_port); }
            set { prop_server_port = value.ToString(); }
        }
        public int MaxPlayers
        {
            get { return Int32.Parse(prop_max_players); }
            set { prop_max_players = value.ToString(); }
        }
        public string Motd
        {
            get { return prop_motd; }
            set { prop_motd = value; }
        }
        public bool EnableQuery
        {
            get {
                if (prop_enable_query == "on")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set {
                if (value == false)
                {
                    prop_enable_query = "off";
                }
                else
                {
                    prop_enable_query = "on";
                }
            }
        }
        public bool EnableRCON
        {
            get
            {
                if (prop_enable_rcon == "on")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value == false)
                {
                    prop_enable_rcon = "off";
                }
                else
                {
                    prop_enable_rcon = "on";
                }
            }
        }
        public bool EnableWhitelist
        {
            get {
                if(prop_white_list == "1")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set {
                if(value == true)
                {
                    prop_white_list = "1";
                }
                else
                {
                    prop_white_list = "0";
                }
            }
        }
        public int SpawnProtectionRadius
        {
            get { return Int32.Parse(prop_spawn_protection); }
            set { prop_spawn_protection = value.ToString(); }
        }
        public string WorldName
        {
            get { return prop_level_name; }
            set { prop_level_name = value.ToString(); }
        }
        public string GeneratorSettings
        {
            get { return prop_generator_settings; }
            set { prop_generator_settings = value.ToString(); }
        }
        public bool EnablePlayerAchievements
        {
            get {
                if(prop_announce_player_achievements == "on")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set {
                if(value == true)
                {
                    prop_announce_player_achievements = "on";
                }
                else
                {
                    prop_announce_player_achievements = "off";
                }
            }
        }
        public bool SpawnAnimals
        {
            get
            {
                if (prop_spawn_animals == "on")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value == true)
                {
                    prop_spawn_animals = "on";
                }
                else
                {
                    prop_spawn_animals = "off";
                }
            }
        }
        public int Difficulty
        {
            get { return Int32.Parse(prop_difficulty); }
            set { prop_difficulty = value.ToString(); }
        }
        public string WorldSeed
        {
            get { return prop_level_seed; }
            set { prop_level_seed = value.ToString(); }
        }
        public bool EnablePVP
        {
            get
            {
                if (prop_pvp == "on")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value == true)
                {
                    prop_pvp = "on";
                }
                else
                {
                    prop_pvp = "off";
                }
            }
        }
        public string MemoryLimit
        {
            get { return prop_memory_limit; }
            set { prop_memory_limit = value; }
        }
        public string RCONPassword
        {
            get { return prop_rcon_password; }
            set { prop_rcon_password = value.ToString(); }
        }
        public bool EnableAutoSave
        {
            get
            {
                if (prop_auto_save == "on")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value == true)
                {
                    prop_auto_save = "on";
                }
                else
                {
                    prop_auto_save = "off";
                }
            }
        }
        public bool EnableHardcore
        {
            get
            {
                if (prop_hardcore == "on")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value == true)
                {
                    prop_hardcore = "on";
                }
                else
                {
                    prop_hardcore = "off";
                }
            }
        }
        public bool ForceGamemode
        {
            get
            {
                if (prop_force_gamemode == "on")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value == true)
                {
                    prop_force_gamemode = "on";
                }
                else
                {
                    prop_force_gamemode = "off";
                }
            }
        }
        public bool SpawnMobs
        {
            get
            {
                if (prop_spawn_mobs == "on")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value == true)
                {
                    prop_spawn_mobs = "on";
                }
                else
                {
                    prop_spawn_mobs = "off";
                }
            }
        }
        public bool AllowFlight
        {
            get
            {
                if (prop_allow_flight == "on")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value == true)
                {
                    prop_allow_flight = "on";
                }
                else
                {
                    prop_allow_flight = "off";
                }
            }
        }
        public int Gamemode
        {
            get { return Int32.Parse(prop_gamemode); }
            set { prop_gamemode = value.ToString(); }
        }
        public string WorldType
        {
            get { return prop_level_type; }
            set { prop_level_type = value.ToString(); }
        }
        public string Directory
        {
            get { return dir; }
            set { dir = value; }
        }

        //Server einstellungen
        private bool online; // Wenn nicht entsteht ne entlosschleife!
        private string dir;
        private string prop_server_name = "";
        private string prop_server_port = "";
        private string prop_max_players = "";
        private string prop_motd = "";
        private string prop_spawn_mobs = "";
        private string prop_allow_flight = "";
        private string prop_enable_query = "";
        private string prop_enable_rcon = "";
        private string prop_white_list = "";
        private string prop_spawn_protection = "";
        private string prop_level_name = "";
        private string prop_generator_settings = "";
        private string prop_announce_player_achievements = "";
        private string prop_spawn_animals = "";
        private string prop_difficulty = "";
        private string prop_level_seed = "";
        private string prop_pvp = "";
        private string prop_memory_limit = "";
        private string prop_rcon_password = "";
        private string prop_auto_save = "";
        private string prop_hardcore = "";
        private string prop_force_gamemode = "";
        private string prop_gamemode = "";
        private string prop_level_type = "";

        // Backgroundworker für lastige Aufgaben
        private BackgroundWorker worker;

        // Query
        Query.MCQuery query;

        // Whitelist Player


        //////////////////////////////////////////////////////////////////////////////////////////////


        // Prüft die Server Verfügbarkeit anhand Query
        private void CheckServerAvailability(object sender, DoWorkEventArgs e)
        {
            var info = query.Info();
            query.Connect("localhost", Port);
            if (query.Success())
            {
                online = true;
            }
            else
            {
                online = false;
            }
        }

        // Gibt die Server Ausgabe als String zurück
        public string getServerOutput()
        {
            return ServerOutput;
        }

        // Startet den Server
        public void start_server ()
        {
            
            if (PocketMineProcess == null)
            {
                PocketMineProcess = new Process();
                PocketMineProcess.StartInfo.CreateNoWindow = true;
                PocketMineProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                PocketMineProcess.StartInfo.WorkingDirectory = dir;
                PocketMineProcess.StartInfo.FileName = "cmd.exe";
                PocketMineProcess.StartInfo.UseShellExecute = false;
                PocketMineProcess.StartInfo.RedirectStandardInput = true;
                PocketMineProcess.StartInfo.RedirectStandardOutput = true;
                PocketMineProcess.OutputDataReceived += OutputDataReceived;


                try
                {
                    PocketMineProcess.Start();
                }
                catch (Exception err)
                {
                    //print("This is not a valid PockeMine directory.");
                    Console.Write("Fehler");
                    Console.Write(err);
                    return;
                }

                PocketMineProcess.StandardInput.WriteLine("bin\\php\\php.exe " + "PocketMine-MP.phar --disable-ansi %*");
                PocketMineProcess.BeginOutputReadLine();
                IsServerRunning();

            }
        }

        // Speichert die Serverausgabe im ServerOutput String
        private void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            ServerOutput += "\n";
            ServerOutput += e.Data;

            if (ServerOutput.Length > 20000)
                ServerOutput = ServerOutput.Substring(20000, 0); 
        }

        // Lädt den Server neu
        public void reload_server ()
        {
            if (PocketMineProcess != null)
                PocketMineProcess.StandardInput.WriteLine("reload");
        }

        // Startet den Server neu.
        public void restart_server()
        {
            stop_server();
            start_server();
        }

        // Fährt den Server herunter bzw. stopt ihn
        public void stop_server ()
        {
            if (PocketMineProcess != null)
            {
                PocketMineProcess.StandardInput.WriteLine("stop");
                System.Threading.Thread.Sleep(2000);
                PocketMineProcess.StandardInput.WriteLine("exit");
                PocketMineProcess.OutputDataReceived += OutputDataReceived;
                PocketMineProcess = null;
                IsServerRunning();
            }
           
        }

        // Gibt zurück ob der ServerPROZESS momentan aktiv ist
        public bool IsServerRunning()
        {
            if (PocketMineProcess == null)
            {
                return false;
            }
            else
            {
                return true;
            }
                
        }

        // Sendet ein Befehl an den Server
        public void send_command (string command)
        {
            if(Online)
                PocketMineProcess.StandardInput.WriteLine(command);
        }

        // Lese Servereinstellungen
        public void read_server_props()
        {
            FileStream fileStream = new FileStream(dir+"server.properties", FileMode.Open);
            JavaProperties server_settings = new JavaProperties();
            server_settings.Load(fileStream);
            fileStream.Close();

            prop_server_name = server_settings.GetProperty("server-name");
            prop_server_port = server_settings.GetProperty("server-port");
            prop_allow_flight = server_settings.GetProperty("allow-flight");
            prop_announce_player_achievements = server_settings.GetProperty("announce-player-achievements");
            prop_pvp = server_settings.GetProperty("pvp");
            prop_memory_limit = server_settings.GetProperty("memory-limit");
            prop_force_gamemode = server_settings.GetProperty("force-gamemode");
            prop_spawn_protection = server_settings.GetProperty("spawn-protection");
            prop_level_name = server_settings.GetProperty("level-name");
            prop_generator_settings = server_settings.GetProperty("generator-settings");
            prop_rcon_password = server_settings.GetProperty("rcon.password");
            prop_enable_rcon = server_settings.GetProperty("enable-rcon");
            prop_spawn_animals = server_settings.GetProperty("spawn-animals");
            prop_difficulty = server_settings.GetProperty("difficulty");
            prop_level_seed = server_settings.GetProperty("level-seed");
            prop_spawn_mobs = server_settings.GetProperty("spawn-mobs");
            prop_auto_save = server_settings.GetProperty("auto-save");
            prop_hardcore = server_settings.GetProperty("hardcore");
            prop_white_list = server_settings.GetProperty("white-list");
            prop_enable_query = server_settings.GetProperty("enable-query");
            prop_level_type = server_settings.GetProperty("level-type");
            prop_motd = server_settings.GetProperty("motd");
            prop_max_players = server_settings.GetProperty("max-players");
            prop_gamemode = server_settings.GetProperty("gamemode");

        }

        // Schreibe Servereinstellungen
        public void write_server_props()
        {
            string[] lines = { "server-name=" + prop_server_name,
                "server-port=" + prop_server_port,
                "memory-limit=" + prop_memory_limit,
                "gamemode=" + prop_gamemode,
                "max-players=" + prop_max_players,
                "spawn-protection=" + prop_spawn_protection,
                "white-list=" + prop_white_list,
                "enable-query=" + prop_enable_query,
                "enable-rcon=" + prop_enable_rcon,
                "motd=" + prop_motd,
                "announce-player-achievements=" + prop_announce_player_achievements,
                "allow-flight=" + prop_allow_flight,
                "spawn-animals=" + prop_spawn_animals,
                "spawn-mobs=" + prop_spawn_mobs,
                "force-gamemode=" + prop_force_gamemode,
                "hardcore=" + prop_hardcore,
                "pvp=" + prop_pvp,
                "difficulty=" + prop_difficulty,
                "generator-settings" + prop_generator_settings,
                "level-name=" + prop_level_name,
                "level-seed=" + prop_level_seed,
                "level-type=" + prop_level_type,
                "rcon.password=" + prop_rcon_password,
                "auto-save=" + prop_auto_save
            };
            System.IO.File.WriteAllLines(dir + "server.properties", lines);
        }

        // Spieler zur Whitelist hinzufügen
        public void add_player_to_whitelist(string playername)
        {
            if(playername != "")
            {
                Player tmp = new Player(playername);
                if (PlayerList.ContainsKey(playername))
                {
                    Console.WriteLine("Folgender Spieler ist bereits im Verzeichnis.");
                }
                else
                {
                    Console.WriteLine("Folgender Spieler wurde zum Verzeichnis hinzugefügt.");
                    PlayerList.Add(tmp.Name, tmp);
                }
                Whitelist.Add(tmp);
            }
            if (Online)
            {
                send_command("whitelist add " + playername);
            }
            write_whitelist();
            ReadPlayerData();
        }

        // Spieler von der Whitelist entfernen
        public void remove_player_from_whitelist (string playername)
        {
            if (playername != "")
            {
                Player tmp = new Player(playername);
                Player rückgabe;
                if (!(PlayerList.TryGetValue(playername, out rückgabe)))
                {
                    MessageBox.Show("Es wurde ein unbekannter Spieler von der Whitelist entfernt.\nDas darf nicht passieren!");
                }
                Whitelist.Remove(rückgabe);
            }
            if (Online)
            {
                send_command("whitelist remove " + playername);
            }
            write_whitelist();
            ReadPlayerData();
        }

        // Whitelist schreiben
        public void write_whitelist() {
            File.WriteAllLines(dir + "white-list.txt", Whitelist.ConvertAll(Convert.ToString));
            ReadPlayerData();
        }

        // Liest alle Spieler ein
        public void ReadPlayerData()
        {
            string[] PlayerDataPath = System.IO.Directory.GetFiles(dir + "\\players\\", "*.dat");
            var myFile = new NbtFile();

            Whitelist.Clear();
            PlayerList.Clear();

            int counter = 0;

            // Lesen der Spielerdaten
            Console.WriteLine(">>> Player:");
            foreach (string path in PlayerDataPath)
            {
                myFile.LoadFromFile(path);
                var Tag = myFile.RootTag;
                Console.WriteLine(Tag.Get<NbtString>("NameTag").Value);

                Player tmp = new Player(Tag.Get<NbtString>("NameTag").Value);

                long a1 = Tag.Get<NbtLong>("lastPlayed").Value;
                double a2 = Convert.ToDouble(a1);
                tmp.LastOnline = Utils.JavaTimeStampToDateTime(a2);

                long b1 = Tag.Get<NbtLong>("firstPlayed").Value;
                double b2 = Convert.ToDouble(b1);
                tmp.FirstTimeOnline = Utils.JavaTimeStampToDateTime(b2);


                PlayerList.Add(Tag.Get<NbtString>("NameTag").Value, tmp);
                counter++;
            }

            // Whitelist lesen
            StreamReader whitelistfile = new StreamReader(dir + "white-list.txt");
            string line;
            Console.WriteLine(">>> Whitelist:");

            while ((line = whitelistfile.ReadLine()) != null)
            {
                if(line != "")
                {
                    Player tmp = new Player(line);
                    Whitelist.Add(tmp);

                    if (PlayerList.ContainsKey(line))
                    {
                        Console.WriteLine("Folgender Spieler ist bereits im Verzeichnis.");
                    }
                    else
                    {
                        Console.WriteLine("Folgender Spieler wurde zum Verzeichnis hinzugefügt.");
                        PlayerList.Add(tmp.Name, tmp);
                    }

                    Console.WriteLine(line);
                    counter++;
                } 
            }
            whitelistfile.Close();


        }
    }

}
