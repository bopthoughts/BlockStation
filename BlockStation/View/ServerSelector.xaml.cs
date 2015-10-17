using BlockStation.gui;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BlockStation
{
    /// <summary>
    /// Interaktionslogik für ServerSelector.xaml
    /// </summary>
    public partial class ServerSelector : Window
    {

        string ServerPfad;
        public PocketMine_MP_Server server;

        public ServerSelector()
        {
            InitializeComponent();
            Utils.SetLanguage(this);

            if (Properties.Settings.Default.SaveLastServer == true && Properties.Settings.Default.LastServerDir != "")
            {
                ServerPfad = Properties.Settings.Default.LastServerDir;
                textbox_server_dir.Text = Properties.Settings.Default.LastServerDir;
                button_open.IsEnabled = true;
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.pocketmine.net/?lang=en");
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            string Pfad = string.Empty;

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "PocketMine-MP.phar Datei|PocketMine-MP.phar";
            if (openFileDialog1.ShowDialog() == true)
            {
                Pfad = openFileDialog1.FileName;
                int i = 0;
                while (i < 18)
                {
                    Pfad = Pfad.Remove(Pfad.Length - 1, 1);
                    i++;
                }
                if (Utils.checkServerFolder(Pfad))
                {
                    textbox_server_dir.Text = Pfad;
                    ServerPfad = Pfad;
                    button_open.IsEnabled = true;
                }
                else
                {
                    MessageBox.Show("PocketMine Server kann nicht geöffnet werden, da eine benötigte\nDatei fehlt.\n\nFehlende Datei: \"" + Utils.getMissingFile(Pfad)+ "\"", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }

        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            PocketMine_MP_Server server = new PocketMine_MP_Server(ServerPfad);
            Properties.Settings.Default.LastServerDir = ServerPfad;
            Properties.Settings.Default.Save();
            MainWindow mw = new MainWindow(server);
            mw.InitializeComponent();
            this.Close();
            mw.Show();
        }

        private void Info_Click(object sender, RoutedEventArgs e)
        {
            Info info = new Info();
            info.InitializeComponent();
            info.version.Content = Properties.App.Default.Version + " " + Properties.App.Default.VersionText;
            info.build.Content = Properties.App.Default.Build;
            info.ShowDialog();
        }

        private void button2_Click_1(object sender, RoutedEventArgs e)
        {
            Settings s = new Settings();
            s.InitializeComponent();
            s.ShowDialog();
            Utils.SetLanguage(this);
        }
    }
}
