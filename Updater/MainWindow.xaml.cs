using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Updater
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string downloadlink = "";

        static string ProgramFilesx86()
        {
            if (8 == IntPtr.Size
                || (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
            {
                return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            }

            return Environment.GetEnvironmentVariable("ProgramFiles");
        }

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                // Lädt die Updatedatei herunter
                var downloader = new WebClient();
                downloader.DownloadFile("https://raw.githubusercontent.com/haecker-felix/BlockStation/master/BlockStation/blockstation.update", System.IO.Path.GetTempPath() + "blockstation.update");
                StreamReader update_file = new StreamReader(System.IO.Path.GetTempPath() + "blockstation.update");


                // Liest die Update datei ein
                int counter = 0;
                string current_line;
                while ((current_line = update_file.ReadLine()) != null)
                {
                    if (counter == 0)
                    {
                        new_build.Content = current_line;
                    }
                    else if (counter == 1)
                    {
                        downloadlink = current_line;
                    }
                    else if (counter == 2)
                    {
                        update_hint.Content = current_line;
                    }
                    counter++;
                }
                update_file.Close();
            }
            catch
            {
                MessageBox.Show("Es konnte nicht nach Aktualisierungen gesucht werden.", "Fehler!");
                this.Close();
            }

            


        }

        private void install_update_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                try
                {
                    File.Delete(ProgramFilesx86() + "\\BlockStation\\BlockStation.exe");
                    File.Delete(ProgramFilesx86() + "\\BlockStation\\BlockStation_new.exe");
                    File.Delete(ProgramFilesx86() + "\\BlockStation\\BlockStation_.exe");
                }
                catch
                {

                }


                var update_download = new WebClient();
                status.Content = "Update wird heruntergeladen...";
                update_download.DownloadFile(downloadlink, ProgramFilesx86() + "\\BlockStation\\BlockStation_new.exe");
                status.Content = "Alte Version wird gelöscht...";
                File.Delete(ProgramFilesx86() + "\\BlockStation\\BlockStation.exe");
                status.Content = "Neue Version wird installiert...";
                File.Copy(ProgramFilesx86() + "\\BlockStation\\BlockStation_new.exe", ProgramFilesx86() + "\\BlockStation\\BlockStation.exe");
                status.Content = "Temporäre Updatedatei wird gelöscht...";
                File.Delete(ProgramFilesx86() + "\\BlockStation\\BlockStation_new.exe");
                install_update.IsEnabled = false;
                MessageBox.Show("BlockStation wurde auf die neueste Version aktualisiert.", "Erfolg!");
                this.Close();
            }
            catch
            {
                MessageBox.Show("Das Update konnte nicht installiert werden.\nBitte installieren sie BlockStation manuell neu.", "Fehler!");
                this.Close();

            }

        }
    }
}
