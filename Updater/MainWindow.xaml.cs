using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Updater
{

    public static class IconHelper
    {
        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x,
    int y, int width, int height, uint flags);

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr
    lParam);

        const int GWL_EXSTYLE = -20;
        const int WS_EX_DLGMODALFRAME = 0x0001;
        const int SWP_NOSIZE = 0x0001;
        const int SWP_NOMOVE = 0x0002;
        const int SWP_NOZORDER = 0x0004;
        const int SWP_FRAMECHANGED = 0x0020;
        const uint WM_SETICON = 0x0080;

        public static void RemoveIcon(Window window)
        {
            // Get this window's handle
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            // Change the extended window style to not show a window icon
            int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_DLGMODALFRAME);
            // Update the window's non-client area to reflect the changes
            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE |
    SWP_NOZORDER | SWP_FRAMECHANGED);
        }
    }

    public partial class MainWindow : Window
    {
        string downloadlink = "";
        BackgroundWorker worker;
        bool success = false;

        protected override void OnSourceInitialized(EventArgs e)
        {
            IconHelper.RemoveIcon(this);
        }

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
            Environment.Exit(0);
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
                catch(Exception err)
                {
                    MessageBox.Show(err.ToString(), "Error");
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
                catch (Exception err)
                {
                    MessageBox.Show(err.ToString(), "Error");
                    this.Close();
                }


        }
    }
}
