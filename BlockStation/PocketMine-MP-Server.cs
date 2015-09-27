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

namespace BlockStation
{
    public class Player
    {
        public string Name { get; set; }

        public override string ToString()
        {
            return this.Name;
        }

    }


    public class PocketMine_MP_Server
    {
        // PocketMine Speicherort
        public string dir;

        // Aktueller Serverstatus (Aktiv oder Inaktiv)
        public string ServerStatus;

        // Prozess für PocketMine
        public Process PocketMineProcess;

        // Server Ausgabe
        public string ServerOutput;
        
        // Server Propteries
        public string prop_server_name = "";
        public string prop_server_port = "";
        public string prop_max_players = "";
        public string prop_motd = "";
        public string prop_spawn_mobs = "";
        public string prop_allow_flight = "";
        public string prop_enable_query = "";
        public string prop_enable_rcon = "";
        public string prop_white_list = "";
        public string prop_spawn_protection = "";
        public string prop_level_name = "";
        public string prop_generator_settings = "";
        public string prop_announce_player_achievements = "";
        public string prop_spawn_animals = "";
        public string prop_difficulty = "";
        public string prop_level_seed = "";
        public string prop_pvp = "";
        public string prop_memory_limit = "";
        public string prop_rcon_password = "";
        public string prop_auto_save = "";
        public string prop_hardcore = "";
        public string prop_force_gamemode = "";
        public string prop_gamemode = "";
        public string prop_level_type = "";

        BackgroundWorker worker;
        bool status = false;


        // Query
        Query.MCQuery query;

        // Whitelist Player
        public List<Player> whitelist_player;

        public PocketMine_MP_Server(string d)
        {
            whitelist_player = new List<Player>();

            dir = d;
            read_server_props();


            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;

            query = new Query.MCQuery();
        }

        // Gibt die PocketMine Version zurück
        public string pm_version()
        {
            var info = query.Info();
            if (isServerOnline())
            {
                return info.Plugins;
            }
            else
            {
                return "";
            }
        }

        // Gibt die Latenzzeit zurück zurück
        public int latency()
        {
            var info = query.Info();
            if (isServerOnline())
            {
                return info.Latency;
            }
            else
            {
                return 0;
            }
        }

        // gibt Serverstatus als Bool zurück
        public bool isServerOnline()
        {
            worker.DoWork += new DoWorkEventHandler(isServerOnlineWORK);
            if (worker.IsBusy)
            {
                return status;
            }
            else
            {
                worker.RunWorkerAsync();
                return status;
            }
        }

        private void isServerOnlineWORK(object sender, DoWorkEventArgs e)
        {
            var info = query.Info();
            query.Connect("localhost");
            if (query.Success())
            {
                status = true;
            }
            else
            {
                status = false;
            }
        }

        // Gibt die maximale Spieleranzahl zurück
        public int max_players()
        {
            var info = query.Info();
            if (isServerOnline())
            {
                return info.MaxPlayers;
            }
            else
            {
                return 0;
            }
        }

        // Gibt die Spieleranzahl zurück
        public int player_online()
        {
            var info = query.Info();
            if (isServerOnline())
            {
                return info.OnlinePlayers;
            }
            else
            {
                return 0;
            }
        }

        // Gibt die "Message of the Day" zurück
        public string motd()
        {
            var info = query.Info();
            if (isServerOnline())
            {
                return info.Name;
            }
            else
            {
                return "";
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
                ServerStatus = "Wird gestartet...";
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

        // Gibt zurück ob der Server momentan aktiv ist
        public bool IsServerRunning()
        {
            if (PocketMineProcess == null)
            {
                ServerStatus = "Inaktiv";
                return false;
            }
            else
            {
                ServerStatus = "Aktiv";
                return true;
            }
                
        }

        // Sendet ein Befehl an den Server
        public void send_command (string command)
        {
            if(PocketMineProcess != null)
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

        // Lese Whitelist
        public void read_whitelist()
        {
            StreamReader tmp = new StreamReader(dir + "white-list.txt");

            string line;
            int counter = 0;

            whitelist_player.Clear();

            while ((line = tmp.ReadLine()) != null)
            {
                whitelist_player.Add(new Player { Name = line });
                counter++;
            }

            tmp.Close();
        }

        // Spieler zur Whitelist hinzufügen
        public void add_player_to_whitelist(Player player)
        {
            if(player.Name != "")
            {
                whitelist_player.Add(player);
            }
            if (isServerOnline() && player.Name != "")
            {
                send_command("whitelist add " + player.Name);
            }
            write_whitelist();
            read_whitelist();
        }

        // Spieler von der Whitelist entfernen
        public void remove_player_from_whitelist (int id, string player)
        {
            if (player != "")
            {
                whitelist_player.RemoveAt(id);
            }
            if (isServerOnline() && player != "")
            {
                send_command("whitelist remove " + player);
            }
            write_whitelist();
            read_whitelist();
        }

        // Whitelist schreiben
        public void write_whitelist() {
            File.WriteAllLines(dir + "white-list.txt", whitelist_player.ConvertAll(Convert.ToString));
            read_whitelist();
        }
    }
}
