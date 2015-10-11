using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        BackgroundWorker worker;
        bool success = false;

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

            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = false;
            worker.DoWork += new DoWorkEventHandler(InstallUpdate);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(UpdateInstalled);
            worker.ProgressChanged += new ProgressChangedEventHandler(ProgressChanged);
            worker.RunWorkerAsync();

            
        }

        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progress.Value = e.ProgressPercentage;
        }

        private void UpdateInstalled(object sender, RunWorkerCompletedEventArgs e)
        {


            if(success)
                MessageBox.Show("BlockStation wurde auf die neueste Version aktualisiert.", "Erfolg!");
        }

        private void InstallUpdate(object sender, DoWorkEventArgs e)
        {

                try
                {

                    worker.ReportProgress(10);
                    // Lädt die Updatedatei herunter
                    var downloader = new WebClient();
                    downloader.DownloadFile("https://raw.githubusercontent.com/haecker-felix/BlockStation/master/BlockStation/blockstation.update", System.IO.Path.GetTempPath() + "blockstation.update");
                    StreamReader update_file = new StreamReader(System.IO.Path.GetTempPath() + "blockstation.update");
                    worker.ReportProgress(20);

                    // Liest die Update datei ein
                    int counter = 0;
                    string current_line;
                    while ((current_line = update_file.ReadLine()) != null)
                    {
                        if (counter == 0)
                        {
                            //new_build.Content = current_line;
                        }
                        else if (counter == 1)
                        {
                            downloadlink = current_line;
                        }
                        else if (counter == 2)
                        {
                            //update_hint.Content = current_line;
                        }
                        counter++;
                    }
                    update_file.Close();
                    worker.ReportProgress(30);
                }
                catch
                {
                    MessageBox.Show("Update konnte nicht installiert werden.\nBitte prüfen Sie ihre Internetverbindung..", "Fehler!");
                    this.Close();
                }
                worker.ReportProgress(50);
                try
                {

                        File.Delete(ProgramFilesx86() + "\\BlockStation\\BlockStation.exe");
                        File.Delete(ProgramFilesx86() + "\\BlockStation\\BlockStation_new.exe");
                        File.Delete(ProgramFilesx86() + "\\BlockStation\\BlockStation_.exe");


                    var update_download = new WebClient();
                    worker.ReportProgress(60);
                    update_download.DownloadFile(downloadlink, ProgramFilesx86() + "\\BlockStation\\BlockStation_new.exe");
                    worker.ReportProgress(70);
                    File.Delete(ProgramFilesx86() + "\\BlockStation\\BlockStation.exe");
                    worker.ReportProgress(80);
                    File.Copy(ProgramFilesx86() + "\\BlockStation\\BlockStation_new.exe", ProgramFilesx86() + "\\BlockStation\\BlockStation.exe");
                    worker.ReportProgress(90);
                    File.Delete(ProgramFilesx86() + "\\BlockStation\\BlockStation_new.exe");
                    worker.ReportProgress(100);
                    success = true;
                    Environment.Exit(0);
                }
                catch(Exception err)
                {
                    MessageBox.Show("Das Update konnte nicht installiert werden.\n\nDetails:\n" + err.ToString(), "Fehler!");
                    success = false;
                Environment.Exit(0);
            }

            
        }
    }
}
