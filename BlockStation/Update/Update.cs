using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Permissions;
using System.Text;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;

namespace BlockStation.Update
{
    class Update
    {

        // 0 = Aktuell installierte Version
        // 1 = Stable
        // 2 = Unstable
        // 3 = Development

        public BackgroundWorker bw;

        public int channel;

        public string[] UpdateFileURL = { "",
            "https://raw.githubusercontent.com/haecker-felix/BlockStation/master/BlockStation/Update/blockstation-stable.update",
            "https://raw.githubusercontent.com/haecker-felix/BlockStation/master/BlockStation/Update/blockstation-unstable.update",
            "https://raw.githubusercontent.com/haecker-felix/BlockStation/master/BlockStation/Update/blockstation-development.update" };
        public string[] UpdateFile = { "",
            "blockstation-stable.update",
            "blockstation-unstable.update",
            "blockstation-development.update" };

        public bool[] update = new bool[4];
        public string[] url = new string[4];
        public string[] version = new string[4];
        public string[] build = new string[4];
        public string[] changelog = new string[4];

        public Update(BackgroundWorker b){
            bw = b;
        }

        public void Search(object sender, DoWorkEventArgs e)
        {
            var Downloader = new WebClient();

            bw.ReportProgress(10);

            // Das ganze für jede Version prüfen
            for (int i = 1; i != 4; i++)
            {
                Console.WriteLine(i);
                // Herunterladen
                try
                {
                    Downloader.DownloadFile(UpdateFileURL[i], System.IO.Path.GetTempPath() + UpdateFile[i]);
                    bw.ReportProgress(+20);
                }
                catch (Exception error)
                {
                    Console.WriteLine(error.ToString());
                }

                // Parsen
                try
                {
                    var File = System.IO.File.ReadAllLines(System.IO.Path.GetTempPath() + UpdateFile[i]);
                    int counter = 0;

                    foreach (var Line in File)
                    {
                        switch (counter)
                        {
                            case 0: build[i] = Line; Console.WriteLine(build[i]); break;
                            case 1: version[i] = Line; Console.WriteLine(version[i]); break;
                            case 2: changelog[i] = Line; Console.WriteLine(changelog[i]); break;
                            case 3: url[i] = Line; Console.WriteLine(url[i]); break;
                        }
                        counter++;
                    }
                }
                catch (Exception error)
                {
                    Console.WriteLine(error.ToString());
                }

                // Mit der installierten version vergleichen
                for (int b = 1; b != 4; b++)
                {
                    if (Int32.Parse(build[i]) > Int32.Parse(Properties.App.Default.Build))
                    {
                        update[i] = true;
                    }
                    else
                    {
                        update[i] = false;
                    }
                    
                }

                // blockstation.update wieder löschen.
                try
                {
                    File.Delete(System.IO.Path.GetTempPath() + UpdateFile[i]);
                }
                catch (Exception error)
                {
                    Console.WriteLine(error.ToString());
                }
                bw.ReportProgress(100);
            }
        }

        public void PrepareUpdate(object sender, DoWorkEventArgs e)
        {
            bw.ReportProgress(10);
            var Downloader = new WebClient();
            Downloader.DownloadFile(url[channel], System.IO.Path.GetTempPath() + "BlockStation_new.exe");
            bw.ReportProgress(90);

            // Neustarten mit admin rechte im Update modus
            ProcessStartInfo proc = new ProcessStartInfo();
            proc.UseShellExecute = true;
            proc.WorkingDirectory = Environment.CurrentDirectory;
            proc.FileName = Utils.ProgramFilesx86() + "\\BlockStation\\BlockStation.exe";
            proc.Verb = "runas";
            proc.Arguments = "update";
            bw.ReportProgress(95);
            try
            {
                Process.Start(proc);
            }
            catch
            {
                return;
            }
            bw.ReportProgress(100);
            Environment.Exit(0);  // Quit itself
        }

        public void InstallUpdate()
        {
            System.IO.File.Move(Utils.ProgramFilesx86() + "\\BlockStation\\BlockStation.exe", Utils.ProgramFilesx86() + "\\BlockStation\\BlockStation_old.exe");
            System.IO.File.Move(System.IO.Path.GetTempPath() + "BlockStation_new.exe", Utils.ProgramFilesx86() + "\\BlockStation\\BlockStation.exe");

            // Neustarten mit admin rechte im Update modus
            ProcessStartInfo proc = new ProcessStartInfo();
            proc.UseShellExecute = true;
            proc.WorkingDirectory = Environment.CurrentDirectory;
            proc.FileName = Utils.ProgramFilesx86() + "\\BlockStation\\BlockStation.exe";
            try
            {
                Process.Start(proc);
            }
            catch
            {
                return;
            }
            Environment.Exit(0);  // Quit itself
        }
    }
}
