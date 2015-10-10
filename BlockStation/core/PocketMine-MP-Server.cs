﻿using System;
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
using System.Text.RegularExpressions;

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
            // Initialisierungen
            Whitelist = new List<Player>();
            PlayerList = new Dictionary<string, Player>();
            query = new Query.MCQuery();
            dir = d;

            // Durchgehender OnlineCheck
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = false;
            worker.DoWork += new DoWorkEventHandler(CheckServerAvailability);

            ReadServerSettings();
            ReadPlayerData();
        }

        // Events
        public event EventHandler ServerOutputChanged;
        public event EventHandler PlayerJoinedServer;
        public event EventHandler PlayerLeaveServer;
        public event EventHandler ServerGetOnline;
        public event EventHandler ServerGetOffline;

        // Alle Spieler
        public IDictionary<string, Player> PlayerList;

        // Whitelist
        public List<Player> Whitelist;

        // Prozess für PocketMine
        private Process PocketMineProcess;

        // Backgroundworker für lastige Aufgaben
        private BackgroundWorker worker;

        // Query
        Query.MCQuery query;

        // Public Server Properties
        public bool ServerProcess
        {
            get
            {
                return serverprocess;
            }
            set
            {
                serverprocess = value;
            }
        }
        public string ServerOutput
        {
            get
            {
                return serveroutput;
            }
        }
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
                    Console.WriteLine("Latency: " + info.Latency);
                    return info.Latency;

                }
                else
                {
                    Console.WriteLine("Latency: 0 da Server offline");
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
            set {
                if (online == true && value == false)
                {
                    if (ServerGetOffline != null)
                    {
                        ServerGetOffline.Invoke(this, EventArgs.Empty);
                    }
                }
                if (online == false && value == true)
                {
                    if(ServerGetOnline != null)
                    {
                        ServerGetOnline.Invoke(this, EventArgs.Empty);
                    }
                }
                online = value;
            }
        }
        public string Name
        {
            get { return prop_server_name; }
            set { prop_server_name = value; WriteServerSettings(); }
        }
        public int Port
        {
            get { return Int32.Parse(prop_server_port); }
            set { prop_server_port = value.ToString(); WriteServerSettings(); }
        }
        public int MaxPlayers
        {
            get { return Int32.Parse(prop_max_players); }
            set { prop_max_players = value.ToString(); }
        }
        public string Motd
        {
            get { return prop_motd; }
            set { prop_motd = value; WriteServerSettings(); }
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
                WriteServerSettings();
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
                WriteServerSettings();
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
                WriteServerSettings();
            }
        }
        public int SpawnProtectionRadius
        {
            get { return Int32.Parse(prop_spawn_protection); }
            set { prop_spawn_protection = value.ToString(); WriteServerSettings(); }
        }
        public string WorldName
        {
            get { return prop_level_name; }
            set { prop_level_name = value.ToString(); WriteServerSettings(); }
        }
        public string GeneratorSettings
        {
            get { return prop_generator_settings; }
            set { prop_generator_settings = value.ToString(); WriteServerSettings(); }
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
                WriteServerSettings();
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
                WriteServerSettings();
            }
        }
        public int Difficulty
        {
            get { return Int32.Parse(prop_difficulty); }
            set { prop_difficulty = value.ToString(); WriteServerSettings(); }
        }
        public string WorldSeed
        {
            get { return prop_level_seed; }
            set { prop_level_seed = value.ToString(); WriteServerSettings(); }
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
                WriteServerSettings();
            }
        }
        public string MemoryLimit
        {
            get { return prop_memory_limit; }
            set { prop_memory_limit = value; WriteServerSettings(); }
        }
        public string RCONPassword
        {
            get { return prop_rcon_password; }
            set { prop_rcon_password = value.ToString(); WriteServerSettings(); }
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
                WriteServerSettings();
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
                WriteServerSettings();
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
                WriteServerSettings();
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
                WriteServerSettings();
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
                WriteServerSettings();
            }
        }
        public int Gamemode
        {
            get { return Int32.Parse(prop_gamemode); }
            set { prop_gamemode = value.ToString(); WriteServerSettings(); }
        }
        public string WorldType
        {
            get { return prop_level_type; }
            set { prop_level_type = value.ToString(); WriteServerSettings(); }
        }
        public string Directory
        {
            get { return dir; }
            set { dir = value; }
        }

        //Server einstellungen
        private bool serverprocess;
        private string serveroutput;
        private bool online = false;
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

        // Startet den Server
        public void Start ()
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
            }
        }

        // Lädt den Server neu
        public void Reload()
        {
            if (PocketMineProcess != null)
                PocketMineProcess.StandardInput.WriteLine("reload");
        }

        // Startet den Server neu.
        public void Restart()
        {
            Stop();
            Start();
        }

        // Fährt den Server herunter bzw. stopt ihn
        public void Stop()
        {
            if (PocketMineProcess != null)
            {
                PocketMineProcess.StandardInput.WriteLine("stop");
                PocketMineProcess = null;
                CheckServerAvailability(this, null);
            }

        }

        // Prüft die Server Verfügbarkeit
        private void CheckServerAvailability(object sender, DoWorkEventArgs e)
        {
            if (PocketMineProcess == null)
            {
                ServerProcess = false;
                Online = false;
            }
            else
            {
                var info = query.Info();
                query.Connect("localhost", Port);
                if (query.Success())
                {
                    Online = true;
                }
                else
                {
                    Online = false;
                }
                ServerProcess = true;
            }

        }

        // Speichert die Serverausgabe im ServerOutput String
        private void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if(e.Data != null)
            {
                if (Regex.IsMatch(e.Data, "\\bjoined\\b", RegexOptions.IgnoreCase))
                {
                    PlayerJoinedServer.Invoke(this, EventArgs.Empty);
                    Console.WriteLine("Ein Spieler ist beigetreten!");
                }
                if (Regex.IsMatch(e.Data, "\\blogged out\\b", RegexOptions.IgnoreCase))
                {
                    Console.WriteLine("Ein Spieler hat den Server verlassen!");
                    PlayerLeaveServer.Invoke(this, EventArgs.Empty);
                }
            }
            serveroutput += "\n";
            serveroutput += e.Data;

            if (serveroutput.Length > 20000)
                serveroutput = serveroutput.Substring(20000, 0);
            ServerOutputChanged.Invoke(this, EventArgs.Empty);
        }

        // Sendet ein Befehl an den Server
        public void SendCommand (string command)
        {
            if(Online)
                PocketMineProcess.StandardInput.WriteLine(command);
        }

        // Lese Servereinstellungen
        public void ReadServerSettings()
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
        private void WriteServerSettings()
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
        public void AddPlayerToWhitelist(string playername)
        {
            if(playername != "")
            {
                Player tmp = new Player(playername);
                if (!(PlayerList.ContainsKey(playername)))
                {
                    PlayerList.Add(tmp.Name, tmp);
                }
                Whitelist.Add(tmp);
            }
            if (Online)
            {
                SendCommand("whitelist add " + playername);
            }
            File.WriteAllLines(dir + "white-list.txt", Whitelist.ConvertAll(Convert.ToString));
            ReadPlayerData();
        }

        // Spieler von der Whitelist entfernen
        public void RemovePlayerFromWhitelist (string playername)
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
                SendCommand("whitelist remove " + playername);
            }
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
            foreach (string path in PlayerDataPath)
            {
                myFile.LoadFromFile(path);
                var Tag = myFile.RootTag;

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

            while ((line = whitelistfile.ReadLine()) != null)
            {
                if(line != "")
                {
                    Player tmp = new Player(line);
                    Whitelist.Add(tmp);

                    if (!(PlayerList.ContainsKey(line)))
                    {
                        PlayerList.Add(tmp.Name, tmp);
                    }
                    counter++;
                } 
            }
            whitelistfile.Close();


        }
    }

}