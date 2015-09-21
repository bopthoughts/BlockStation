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
        public MainWindow()
        {
            InitializeComponent();

            string downloadlink = "";
            string update_hint = "";

            // Lädt die Updatedatei herunter
            var downloader = new WebClient();
            downloader.DownloadFile("https://raw.githubusercontent.com/haecker-felix/BlockStation/master/BlockStation/blockstation.update", System.IO.Path.GetTempPath() + "blockstation.update");
            StreamReader update_file = new StreamReader(System.IO.Path.GetTempPath() + "blockstation.update");
            update_file.Close();

            // Liest die Update datei ein
            int counter = 0;
            string current_line;
            while ((current_line = update_file.ReadLine()) != null)
            {
                if(counter == 0)
                {
                    new_build.Content = current_line;
                }else if(counter == 1)
                {
                    downloadlink = current_line;
                }
                else if (counter == 2)
                {
                    update_hint = current_line;
                }
                counter++;
            }

        }
    }
}
