using BlockStation.gui;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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
                    ServerDir.Text = Pfad;
                    ServerPfad = Pfad;
                    OpenServer.IsEnabled = true;
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
            MainWindow mw = new MainWindow(server);
            mw.InitializeComponent();
            this.Close();
            mw.Show();
        }

        private void Info_Click(object sender, RoutedEventArgs e)
        {
            Info info = new Info();
            info.InitializeComponent();
            info.version.Content = App.AppVersion + " " + App.AppVersionText;
            info.build.Content = App.BuildVersion;
            info.Show();
        }
    }
}
