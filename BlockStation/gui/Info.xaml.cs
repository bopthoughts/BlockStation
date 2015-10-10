using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace BlockStation.gui
{
    /// <summary>
    /// Interaktionslogik für Info.xaml
    /// </summary>
    public partial class Info : Window
    {
        private bool updateAvailable;

        public Info()
        {
            InitializeComponent();
        }

        private void OpenWebsite_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/haecker-felix/BlockStation");
        }

        private void ReportError_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/haecker-felix/BlockStation/issues/new");
        }

        private void SearchForUpdates_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker;
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = false;
            worker.DoWork += new DoWorkEventHandler(CheckForUpdates);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(UpdateAction);
            SearchForUpdates.Content = "Wird geprüft...";
            SearchForUpdates.IsEnabled = false;
            worker.RunWorkerAsync();
        }

        private void UpdateAction(object sender, RunWorkerCompletedEventArgs e)
        {
            if (updateAvailable)
            {
                SearchForUpdates.Content = "Nach Updates suchen";
                SearchForUpdates.IsEnabled = true;
                MessageBoxResult result = MessageBox.Show("Es ist ein Update verfügbar. \nMöchte sie das Update installieren?", "Aktualisierung", MessageBoxButton.YesNo);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        try
                        {
                            ProcessStartInfo updater = new ProcessStartInfo(Utils.ProgramFilesx86() + "\\BlockStation\\Updater.exe");
                            updater.Verb = "runas";
                            System.Diagnostics.Process.Start(updater);
                            this.Close();
                            Environment.Exit(0);
                        }
                        catch
                        {
                            MessageBox.Show("Updater konnte nicht geöffnet werden.", "Fehler!");
                        }
                        break;
                    case MessageBoxResult.No:
                        break;
                }
            }
            else
            {
                SearchForUpdates.Content = "Nach Updates suchen";
                SearchForUpdates.IsEnabled = true;
                MessageBox.Show("Keine Aktualisierung verfügbar.", "Aktualisierung");
            }

        }

        private void CheckForUpdates(object sender, DoWorkEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                var downloader = new WebClient();

                bool error = false;

                // blockstation.update herunterladen
                try
                {
                    downloader.DownloadFile("https://raw.githubusercontent.com/haecker-felix/BlockStation/master/BlockStation/blockstation.update", System.IO.Path.GetTempPath() + "blockstation.update");
                }
                catch
                {
                    MessageBox.Show("Die Updatedatei konnte nicht heruntergeladen werden.\nBitte prüfen Sie die Internetverbindung.", "Aktualisierung");
                    error = true;
                }

                if (!error)
                {
                    // blockstation.update parsen
                    try
                    {
                        StreamReader streamReader = new StreamReader(System.IO.Path.GetTempPath() + "blockstation.update");
                        string update = streamReader.ReadLine();
                        streamReader.Close();

                        if (Int32.Parse(update) > Int32.Parse(App.BuildVersion))
                        {
                            updateAvailable = true;
                        }
                        else
                        {
                            updateAvailable = false;
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Die Updatedatei konnte nicht eingelesen werden.", "Fehler!");
                    }

                    // blockstation.update wieder löschen.
                    try
                    {
                        File.Delete(System.IO.Path.GetTempPath() + "blockstation.update");
                    }
                    catch
                    {
                        MessageBox.Show("Die Temporäre Updatedatei konnte nicht gelöscht werden.", "Fehler!");
                    }
                } 
            }
            ));

        }
    }
}
