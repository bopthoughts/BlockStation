using System;
using System.Windows;
using System.Windows.Controls;
using System.Timers;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace BlockStation.View
{

    public class PlayerListItem
    {
        public string Online { get; set; }

        public string Name { get; set; }

        public string LastOnline { get; set; }

        public string FirstOnline { get; set; }
    }

    /// <summary>
    /// Interaktionslogik für ServerTab.xaml
    /// </summary>
    public partial class ServerTab : Grid
    {
        bool ExitApp = false;

        PocketMine_MP_Server server;
        System.Timers.Timer updateTimer;

        Player selectedplayer;

        //
        // Server Einstellungen
        //
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


        //
        // Konstruktor
        //
        public ServerTab(PocketMine_MP_Server s)
        {
            // GUI Stuff
            InitializeComponent();
            Utils.SetLanguage(this);

            //Setzt den Server
            server = s;

            //Whitelist einstellungen laden
            if (server.EnableWhitelist)
            {
                ActivateWhitelist.IsEnabled = false;
            }
            else if (server.EnableWhitelist == false)
            {
                DeactivateWhitelist.IsEnabled = false;
            }


            Whitelist.IsSynchronizedWithCurrentItem = true;
            Whitelist.ItemsSource = server.Whitelist.getListData();
            OPList.ItemsSource = server.OPList.getListData();
            Whitelist.SelectionMode = SelectionMode.Single;

            //PlayerListview.SelectionMode = SelectionMode.Single;

            // Servereinstellungen laden
            loadServerInfo();
            loadServerProperties();

            // Timer für updates
            updateTimer = new System.Timers.Timer(Properties.Settings.Default.UpdateTimer);
            updateTimer.AutoReset = true;
            updateTimer.Elapsed += update;
            updateTimer.Enabled = true;

            refreshPlayerList();
            server.ServerOutputChanged += ReceiveServerOutput;
            server.ServerGetOnline += ServerGetOnline;
            server.ServerGetOffline += ServerGetOffline;
            server.PlayerJoinedServer += PlayerJoinedServer;
            server.PlayerLeaveServer += PlayerLeaveServer;
            server.NewChatMessage += NewChatMessage;
            server.ServerCrash += ServerCrash;
            server.ProcessStopped += ServerProcessStopped;
            server.ServerStarted += ServerStarted;
        }


        //
        // Eventhandler
        //
        private void ServerStarted(object sender, EventArgs e)
        {

        }

        private void ServerProcessStopped(object sender, EventArgs e)
        {

            Dispatcher.Invoke(new Action(() =>
            {
                loadServerInfo();


                if (ExitApp == true)
                {
                    Environment.Exit(0);
                }
            }
            ));
        }

        private void ServerCrash(object sender, EventArgs e)
        {

        }

        private void PlayerLeaveServer(object sender, EventArgs e)
        {


        }

        private void PlayerJoinedServer(object sender, EventArgs e)
        {

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

        //
        // Eventhandler (GUI)
        //
        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            server.Start();
            HelpCommand.IsEnabled = false;
        }

        private void StopServer_Click(object sender, RoutedEventArgs e)
        {
            server.Stop();
        }

        private void EnterCommand_Click(object sender, RoutedEventArgs e)
        {
            server.SendCommand(CommandBar.Text);
            CommandBar.Text = "";
        }

        private void MainWindow1_Closed(object sender, CancelEventArgs e)
        {
            if (server.ServerProcess)
            {
                server.Stop();
                ExitApp = true;
                e.Cancel = true;
            }

        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://minecraft-de.gamepedia.com/Server.properties");
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            try
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

                loadServerProperties();

            }
            catch (System.FormatException)
            {
                Utils.ShowInvalidFieldWarning();
                loadServerProperties();
            }

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
            if (AddPlayerToWhitelistName.Text != "")
            {
                server.AddPlayerToWhitelist(AddPlayerToWhitelistName.Text);
                Whitelist.Items.Refresh();
                refreshPlayerList();
                AddPlayerToWhitelistName.Text = "";
            }

        }

        private void RemovePlayerFromWhitelist_Click(object sender, RoutedEventArgs e)
        {
            if (Whitelist.SelectedValue != null)
            {
                server.RemovePlayerFromWhitelist(Whitelist.SelectedValue.ToString());
                Whitelist.Items.Refresh();
                refreshPlayerList();
            }
        }

        private void SendCommand_Click(object sender, RoutedEventArgs e)
        {
            if (PrivateMessagePanel.IsVisible)
            {
                if(MessageBar.Text != "")
                {
                    server.SendCommand("tell " + PrivateMessageTo.Content.ToString() + " " + MessageBar.Text);
                    MessageOutput.Text += "ADMIN >> " + PrivateMessageTo.Content.ToString() + ": " + MessageBar.Text + "\n";
                }
            }
            else
            {
                if (MessageBar.Text != "")
                    server.SendCommand("say " + MessageBar.Text);
            }

            MessageBar.Text = "";
        }

        private void AddPlayerToOPList_Click(object sender, RoutedEventArgs e)
        {
            if (AddPlayerToOPListName.Text != "")
            {
                server.AddPlayerToOPList(AddPlayerToOPListName.Text);
                OPList.Items.Refresh();
                refreshPlayerList();
                AddPlayerToOPListName.Text = "";
            }
        }

        private void RemovePlayerFromOPList_Click(object sender, RoutedEventArgs e)
        {
            if (OPList.SelectedValue != null)
            {
                server.RemovePlayerFromOPList(OPList.SelectedValue.ToString());
                OPList.Items.Refresh();
                refreshPlayerList();
            }
        }

        private void KickPlayer_Click(object sender, RoutedEventArgs e)
        {
            server.KickPlayer(selectedplayer);
        }

        private void BanPlayer_Click(object sender, RoutedEventArgs e)
        {
            server.BanPlayer(selectedplayer);
        }

        private void PardonPlayer_Click(object sender, RoutedEventArgs e)
        {
            server.PardonPlayer(selectedplayer);
        }

        private void MessageBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            MessageOutput.ScrollToEnd();
        }

        private void ServerOutput_TextChanged(object sender, TextChangedEventArgs e)
        {
            ServerOutput.ScrollToEnd();
        }

        //
        // Sonstiges (GUI)
        //
        private void update(Object source, ElapsedEventArgs e)
        {
            loadServerInfo();
        }

        private void PlayerListview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PlayerList.SelectedItem != null)
            {
                BanPlayer.IsEnabled = false;
                KickPlayer.IsEnabled = false;

                PlayerListItem item = (PlayerListItem)PlayerList.SelectedItem;
                

                selectedplayer = server.getPlayer(item.Name);

                if (selectedplayer.Online)
                {
                    KickPlayer.IsEnabled = true;
                    BanPlayer.IsEnabled = true;
                    PardonPlayer.IsEnabled = true;
                }

                if (server.Online)
                {
                    BanPlayer.IsEnabled = true;
                    PardonPlayer.IsEnabled = true;
                }
                SelectedPlayer.Content = selectedplayer.Name;
            }

        }

        private void ReceiveServerOutput(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>

            {
                ServerOutput.Text = server.ServerOutput;
            }
            ));
        }

        private void refreshPlayerList()
        {
            PlayerList.Items.Clear();
            ChatPlayerList.Items.Clear();

            // SPIELERLISTE
            foreach (Player p in server.PlayerIndex.Values)
            {
                string img = "/BlockStation;component/Resource/Image/Icons/offline.png";
                string LastOnline;
                string FirstOnline;

                if (p.Online)
                {
                    img = "/BlockStation;component/Resource/Image/Icons/online.png";
                }

                if(p.LastOnline.Year == 0001)
                {
                    LastOnline = "-";
                    FirstOnline = "-";
                }
                else
                {
                    LastOnline = p.LastOnline.ToString();
                    FirstOnline = p.FirstTimeOnline.ToString();
                }

                PlayerList.Items.Add(new PlayerListItem {
                    Online = img,
                    Name = p.Name,
                    LastOnline = LastOnline.ToString(),
                    FirstOnline = FirstOnline.ToString() });
            }

            // CHATLISTE
            foreach (Player p in server.PlayerIndex.Values)
            {
                string img = "/BlockStation;component/Resource/Image/Icons/offline.png";
                if (p.Online)
                {
                    img = "/BlockStation;component/Resource/Image/Icons/online.png";
                    ChatPlayerList.Items.Add(new PlayerListItem { Online = img, Name = p.Name });
                }

            }


        }

        private void loadServerInfo()
        {
            try
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    

                    status_max_player.Content = server.MaxPlayers;
                    status_motd.Content = server.Motd;
                    status_player_online.Content = server.OnlinePlayers;
                    status_pm_version.Content = server.Version;
                    status_latency.Content = server.Latency;

                    if (server.Online)
                    {
                        Apply.IsEnabled = false;
                        SaveWarning.Visibility = Visibility.Visible;
                        StartServer.IsEnabled = false;
                        StopServer.IsEnabled = true;
                    }
                    else
                    {
                        Apply.IsEnabled = true;
                        SaveWarning.Visibility = Visibility.Hidden;
                        StartServer.IsEnabled = true;
                        StopServer.IsEnabled = false;
                    }

                    refreshPlayerList();
                }
                ));
            }
            catch (Exception)
            {
                //
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
            catch (Exception)
            {
                server.WriteServerSettings();
                server.ReadServerSettings();
                loadServerProperties();
                Console.WriteLine("Incomplete server.properties file");
            }

        }

        private void ChatPlayerListview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PrivateMessagePanel.Visibility = Visibility.Visible;

            if(ChatPlayerList.SelectedValue != null)
            {
                PlayerListItem item = (PlayerListItem)ChatPlayerList.SelectedItem;
                PrivateMessageTo.Content = item.Name;
            }
                

        }

        private void ClosePrivateMessage_Click(object sender, RoutedEventArgs e)
        {
            PrivateMessagePanel.Visibility = Visibility.Hidden;
        }
    }
}
