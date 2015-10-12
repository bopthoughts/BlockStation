using System;
using System.Windows;
using System.Windows.Controls;
using System.Timers;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;

namespace BlockStation
{

    public partial class MainWindow : Window
    {

        PocketMine_MP_Server server;
        System.Timers.Timer updateTimer;


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


        public MainWindow(PocketMine_MP_Server s)
        {
            // GUI Stuff
            InitializeComponent();

            //Setzt den Server
            server = s;

            //Whitelist einstellungen laden
            if (server.EnableWhitelist)
            {
                ActivateWhitelist.IsEnabled = false;
            }
            else if(server.EnableWhitelist == false)
            {
                DeactivateWhitelist.IsEnabled = false;
            }
            else
            {
                MessageBox.Show("Es konnte nicht geprüft werden, ob die Whitelist aktiviert ist.", "Fehler!");
            }

            Whitelist.IsSynchronizedWithCurrentItem = true;
            Whitelist.ItemsSource = server.Whitelist;
            Whitelist.SelectionMode = SelectionMode.Single;

            PlayerListview.SelectionMode = SelectionMode.Single;

            // Servereinstellungen laden
            loadServerInfo();
            loadServerProperties();

            // Timer für updates
            updateTimer = new System.Timers.Timer(100);
            updateTimer.AutoReset = true;
            updateTimer.Elapsed += update;
            updateTimer.Enabled = false;



            refreshPlayerList();
            server.ServerOutputChanged += receiveServerOutput;
            server.ServerGetOnline += ServerGetOnline;
            server.ServerGetOffline += ServerGetOffline;
            server.PlayerJoinedServer += PlayerJoinedServer;
            server.PlayerLeaveServer += PlayerLeaveServer;
            server.NewChatMessage += NewChatMessage;
        }

        private void PlayerLeaveServer(object sender, EventArgs e)
        {


        }

        private void PlayerJoinedServer(object sender, EventArgs e)
        {

        }

        private void receiveServerOutput(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                ServerOutput.Text = server.ServerOutput;
            }
            ));
        }

        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            server.Start();
            StopServer.IsEnabled = true;
            StartServer.IsEnabled = false;
            RestartServer.IsEnabled = true;
            updateTimer.Enabled = true;
            HelpCommand.IsEnabled = false;
        }

        private void RestartServer_Click(object sender, RoutedEventArgs e)
        {
            server.Restart();
        }

        private void EnterCommand_Click(object sender, RoutedEventArgs e)
        {
            server.SendCommand(CommandBar.Text);
            CommandBar.Text = "";
        }

        private void MainWindow1_Closed(object sender, EventArgs e)
        {
            if(server.ServerProcess)
            {
                server.Stop();
                MessageBox.Show("Der noch laufende PocketMine-MP Server wurde heruntergefahren.", "Hinweis!");
            }
            
        }

        private void update(Object source, ElapsedEventArgs e)
        {
            loadServerInfo();
        }

        private void ServerGetOnline(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                status_online.Content = "Online";

                EnterCommand.IsEnabled = true;
                CommandBar.IsEnabled = true;
                HelpCommand.IsEnabled = true;

                SendMessage.IsEnabled = true;
                MessageBar.IsEnabled = true;
            }
            ));
        }

        private void ServerGetOffline(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                status_online.Content = "Offline";
                EnterCommand.IsEnabled = false;
                CommandBar.IsEnabled = false;
                HelpCommand.IsEnabled = false;

                SendMessage.IsEnabled = false;
                MessageBar.IsEnabled = false;
            }
            ));
        }

        private void NewChatMessage(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                MessageOutput.Text += server.message[1] + ":" + server.message[2] + "\n";
            }
            ));
        }

        private void loadServerInfo()
        {
            try
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    ServerName.Content = server.Name;

                    status_max_player.Content = server.MaxPlayers;
                    status_motd.Content = server.Motd;
                    status_player_online.Content = server.OnlinePlayers;
                    status_pm_version.Content = server.Version;
                    status_latency.Content = server.Latency;
                }
                ));
            }
            catch
            {
                Console.WriteLine("Aborted.");
            }


            
        }

        private void loadServerProperties()
        {
            try
            {
                level_type.Text = server.WorldType;
                gamemode.Text = server.Gamemode.ToString();
                server_name.Text = server.Name;
                enable_query.IsChecked = server.EnableQuery;
                whitelist.IsChecked = server.EnableWhitelist;
                hardcore.IsChecked = server.EnableHardcore;
                auto_save.IsChecked = server.EnableAutoSave;
                spawn_mobs.IsChecked = server.SpawnMobs;
                level_seed.Text = server.WorldSeed;
                difficulty.Text = server.Difficulty.ToString();
                spawn_animals.IsChecked = server.SpawnAnimals;
                enable_rcon.IsChecked = server.EnableRCON;
                rcon_password.Text = server.RCONPassword;
                motd.Text = server.Motd;
                generator_settings.Text = server.GeneratorSettings;
                level_name.Text = server.WorldName;
                spawn_protection.Text = server.SpawnProtectionRadius.ToString();
                force_gamemode.IsChecked = server.ForceGamemode;
                memory_limit.Text = server.MemoryLimit.ToString();
                server_port.Text = server.Port.ToString();
                pvp.IsChecked = server.EnablePVP;
                max_players.Text = server.MaxPlayers.ToString();
                announce_player_achievements.IsChecked = server.EnablePlayerAchievements;
                allow_flight.IsChecked = server.AllowFlight;
            }
            catch(Exception e)
            {
                MessageBox.Show("Die Datei \"server.properties\" konnte nicht eingelesen werden.\n\n" + e, "Fehler!");
            }
            
        }

        private void ServerOutput_TextChanged(object sender, TextChangedEventArgs e)
        {
            ServerOutput.ScrollToEnd();
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://minecraft-de.gamepedia.com/Server.properties");
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            server.Name = server_name.Text;
            server.Port = int.Parse(server_port.Text);
            server.Motd = motd.Text;
            server.MaxPlayers = int.Parse(max_players.Text);
            server.AllowFlight = (bool)allow_flight.IsChecked;
            server.EnablePlayerAchievements = (bool)announce_player_achievements.IsChecked;
            server.EnablePVP = (bool)pvp.IsChecked;
            server.MemoryLimit = memory_limit.Text;
            server.ForceGamemode = (bool)force_gamemode.IsChecked;
            server.SpawnProtectionRadius = int.Parse(spawn_protection.Text);
            server.WorldName = level_name.Text;
            server.GeneratorSettings = generator_settings.Text;
            server.RCONPassword = rcon_password.Text;
            server.EnableRCON = (bool)enable_rcon.IsChecked;
            server.SpawnAnimals = (bool)spawn_animals.IsChecked;
            server.Difficulty = int.Parse(difficulty.Text);
            server.WorldSeed = level_seed.Text;
            server.SpawnMobs = (bool)spawn_mobs.IsChecked;
            server.EnableAutoSave = (bool)auto_save.IsChecked;
            server.EnableHardcore = (bool)hardcore.IsChecked;
            server.EnableWhitelist = (bool)whitelist.IsChecked;
            server.EnableQuery = (bool)enable_query.IsChecked;
            server.Gamemode = int.Parse(gamemode.Text);
            server.WorldType = level_type.Text;

            if(server.ServerProcess == true)
            {
                // Display message box
                MessageBoxResult result = MessageBox.Show("Möchten sie den Server neustarten, damit die Einstellungen\nübernommen werden?", "Einstellungen wurden geschrieben!", MessageBoxButton.YesNo);

                // Process message box results
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        loadServerProperties();
                        server.Stop();
                        server.Start();
                        loadServerProperties();
                        break;
                    case MessageBoxResult.No:
                        loadServerProperties();
                        break;
                }
            }
            loadServerProperties();


        }

        private void StopServer_Click(object sender, RoutedEventArgs e)
        {
            server.Stop();
            StopServer.IsEnabled = false;
            StartServer.IsEnabled = true;
            //updateTimer.Enabled = false;
            RestartServer.IsEnabled = false;
            loadServerInfo();
        }

        private void DeactivateWhitelist_Click(object sender, RoutedEventArgs e)
        {
            server.EnableWhitelist = false;
            server.SendCommand("whitelist off");
            loadServerProperties();
            ActivateWhitelist.IsEnabled = true;
            DeactivateWhitelist.IsEnabled = false;

        }

        private void ActivateWhitelist_Click(object sender, RoutedEventArgs e)
        {
            server.EnableWhitelist = true;
            server.SendCommand("whitelist on");
            loadServerProperties();
            ActivateWhitelist.IsEnabled = false;
            DeactivateWhitelist.IsEnabled = true;
        }

        private void HelpCommand_Click(object sender, RoutedEventArgs e)
        {
            server.SendCommand("help");
        }

        private void AddPlayerToWhitelist_Click(object sender, RoutedEventArgs e)
        {
            if(AddPlayerToWhitelistName.Text == "")
            {
                MessageBox.Show("Feld darf nicht leer sein.");
            }
            else
            {
                server.AddPlayerToWhitelist(AddPlayerToWhitelistName.Text);
                Whitelist.Items.Refresh();
                refreshPlayerList();
                AddPlayerToWhitelistName.Text = "";
            }

        }

        private void RemovePlayerFromWhitelist_Click(object sender, RoutedEventArgs e)
        {
            server.RemovePlayerFromWhitelist(Whitelist.SelectedValue.ToString());
            Whitelist.Items.Refresh();
            refreshPlayerList();
        }

        private void PlayerListview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Player tmp;
                server.PlayerList.TryGetValue(PlayerListview.SelectedValue.ToString(), out tmp);
                playername.Content = tmp.Name;
                lastonline.Content = tmp.LastOnline.ToString();
                firsttimeonline.Content = tmp.LastOnline.ToString();
            }
            catch
            {

            }

        }

        private void refreshPlayerList()
        {
            PlayerListview.Items.Clear();
            foreach (var pair in server.PlayerList)
            {
                PlayerListview.Items.Add(pair.Key);
            }
        }

        private void SendCommand_Click(object sender, RoutedEventArgs e)
        {
            server.SendCommand("say " + MessageBar.Text);
            MessageBar.Text = "";
        }

        private void MessageBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            MessageOutput.ScrollToEnd();
        }
    }
}
