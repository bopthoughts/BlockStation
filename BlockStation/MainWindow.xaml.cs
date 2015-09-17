using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using System.Threading;
using System.Windows.Controls;
using System.Timers;

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

            //!
            //ServerDir.Content = server.dir;

            // Servereinstellungen laden
            loadServerInfo();
            loadServerProperties();


            // Timer für updates
            updateTimer = new System.Timers.Timer(100);
            updateTimer.AutoReset = true;
            updateTimer.Elapsed += update;
            updateTimer.Enabled = false;
        }

        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            server.start_server();
            StopServer.IsEnabled = true;
            StartServer.IsEnabled = false;
            EnterCommand.IsEnabled = true;
            CommandBar.IsEnabled = true;
            RestartServer.IsEnabled = true;

            updateTimer.Enabled = true;


        }

        private void RestartServer_Click(object sender, RoutedEventArgs e)
        {
            server.restart_server();
        }

        public void RefreshOutput_Click(object sender, RoutedEventArgs e)
        {
            //Hier funktioniert es.
            this.ServerOutput.Text = server.getServerOutput();
        }

        private void EnterCommand_Click(object sender, RoutedEventArgs e)
        {
            server.send_command(CommandBar.Text);
            CommandBar.Text = "";
        }

        private void MainWindow1_Closed(object sender, EventArgs e)
        {
            if(server.IsServerRunning())
            {
                server.stop_server();
                MessageBox.Show("Der noch laufende PocketMine-MP Server wurde heruntergefahren.", "Hinweis!");
            }
            
        }

        private void update(Object source, ElapsedEventArgs e)
        {
            loadServerInfo();
        }

        private void loadServerInfo()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                ServerOutput.Text = server.getServerOutput();
                //ServerStatus.Content = server.ServerStatus;
                ServerName.Content = server.prop_server_name;
            }
            ));
        }

        private void loadServerProperties()
        {
            level_type.Text = server.prop_level_type;
            gamemode.Text = server.prop_gamemode;
            server_name.Text = server.prop_server_name;
            enable_query.Text = server.prop_enable_query;
            white_list.Text = server.prop_white_list;
            hardcore.Text = server.prop_hardcore;
            auto_save.Text = server.prop_auto_save;
            spawn_mobs.Text = server.prop_spawn_mobs;
            level_seed.Text = server.prop_level_seed;
            difficulty.Text = server.prop_difficulty;
            spawn_animals.Text = server.prop_spawn_animals;
            enable_rcon.Text = server.prop_enable_rcon;
            rcon_password.Text = server.prop_rcon_password;
            motd.Text = server.prop_motd;
            generator_settings.Text = server.prop_generator_settings;
            level_name.Text = server.prop_level_name;
            spawn_protection.Text = server.prop_spawn_protection;
            force_gamemode.Text = server.prop_force_gamemode;
            memory_limit.Text = server.prop_memory_limit;
            server_port.Text = server.prop_server_port;
            pvp.Text = server.prop_pvp;
            max_players.Text = server.prop_max_players;
            announce_player_achievements.Text = server.prop_announce_player_achievements;
            allow_flight.Text = server.prop_allow_flight;
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
            server.prop_server_name = server_name.Text;
            server.prop_server_port = server_port.Text;
            server.prop_motd = motd.Text;
            server.prop_max_players = max_players.Text;
            server.prop_allow_flight = allow_flight.Text;
            server.prop_announce_player_achievements = announce_player_achievements.Text;
            server.prop_pvp = pvp.Text;
            server.prop_memory_limit = memory_limit.Text;
            server.prop_force_gamemode = force_gamemode.Text;
            server.prop_spawn_protection = spawn_protection.Text;
            server.prop_level_name = level_name.Text;
            server.prop_generator_settings = generator_settings.Text;
            server.prop_rcon_password = rcon_password.Text;
            prop_enable_rcon = enable_rcon.Text;
            prop_spawn_animals = spawn_animals.Text;
            prop_difficulty = difficulty.Text;
            prop_level_seed = level_seed.Text;
            prop_spawn_mobs = spawn_mobs.Text;
            prop_auto_save = auto_save.Text;
            prop_hardcore = hardcore.Text;
            prop_white_list = white_list.Text;
            prop_enable_query = enable_query.Text;
            prop_gamemode = gamemode.Text;
            prop_level_type = level_type.Text;

            server.write_server_props();

            if(server.IsServerRunning() == true)
            {
                // Display message box
                MessageBoxResult result = MessageBox.Show("Möchten sie den Server neustarten, damit die Einstellungen\nübernommen werden?", "Einstellungen wurden geschrieben!", MessageBoxButton.YesNo);

                // Process message box results
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        server.write_server_props();
                        server.stop_server();
                        server.write_server_props();
                        server.read_server_props();
                        server.start_server();
                        server.read_server_props();
                        break;
                    case MessageBoxResult.No:
                        server.write_server_props();
                        server.read_server_props();
                        break;
                }
            }

            loadServerProperties();
        }

        private void ReloadServer_Click(object sender, RoutedEventArgs e)
        {
            server.reload_server();
        }

        private void ToolServerConsole_Click(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedValue = tabServerConsole;
        }

        private void toolServerProperties_Click(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedValue = tabServerProperties;
        }

        private void StopServer_Click(object sender, RoutedEventArgs e)
        {
            server.stop_server();
            StopServer.IsEnabled = false;
            StartServer.IsEnabled = true;
            EnterCommand.IsEnabled = false;
            CommandBar.IsEnabled = false;
            updateTimer.Enabled = false;
            RestartServer.IsEnabled = false;
        }
    }
}
