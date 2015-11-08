using System;
using System.Windows;
using System.Windows.Controls;
using System.Timers;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Documents;
using Microsoft.Win32;
using BlockStation.View;

namespace BlockStation
{

    public partial class MainWindow : Window
    {
        Update.Update u;
        string ServerPfad;
        public PocketMine_MP_Server server;
        BackgroundWorker worker;


        public MainWindow()
        {
            InitializeComponent();
            Utils.SetLanguage(this);

            if (Properties.Settings.Default.SaveLastServer == true && Properties.Settings.Default.LastServerDir != "")
            {
                ServerPfad = Properties.Settings.Default.LastServerDir;
                textbox_server_dir.Text = Properties.Settings.Default.LastServerDir;
                button_open.IsEnabled = true;
            }

            Name.Content = Properties.App.Default.Name;
            Version.Content = Properties.App.Default.Version;
            Build.Content = Properties.App.Default.Build;

            worker = new BackgroundWorker();
            u = new Update.Update(worker);

            savelastdir.IsChecked = Properties.Settings.Default.SaveLastServer;
            timebox.Text = Properties.Settings.Default.UpdateTimer.ToString();
            switch (Properties.Settings.Default.Language)
            {
                case "de": rbDeutsch.IsChecked = true; break;
                case "en": rbEnglisch.IsChecked = true; break;
                case "pt": rbPortugisisch.IsChecked = true; break;
            }
            if (App.UpdateMode)
            {
                ServerControl.Visibility = Visibility.Hidden;
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
                    MessageBox.Show("PocketMine Server kann nicht geöffnet werden, da eine benötigte\nDatei fehlt.\n\nFehlende Datei: \"" + Utils.getMissingFile(Pfad) + "\"", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }

        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            PocketMine_MP_Server server = new PocketMine_MP_Server(ServerPfad);

            Properties.Settings.Default.LastServerDir = ServerPfad;
            Properties.Settings.Default.Save();

            ServerTab st = new ServerTab(server);
            st.InitializeComponent();

            TabItem new_tab = new TabItem();
            new_tab.Content = st;
            new_tab.Header = server.Name;

            ServerControl.Items.Add(new_tab);

            timebox.Text = Properties.Settings.Default.UpdateTimer.ToString();

            switch (Properties.Settings.Default.Language.ToString())
            {
                case "de": rbDeutsch.IsChecked = true; break;
                case "en": rbEnglisch.IsChecked = true; break;
                case "pt": rbPortugisisch.IsChecked = true; break;
            }
            switch (Properties.Settings.Default.SaveLastServer)
            {
                case true: savelastdir.IsChecked = true; break;
                case false: savelastdir.IsChecked = false; break;
            }
        }


        private void CloseTab_Click(object sender, RoutedEventArgs e)
        {
            ServerControl.Items.Remove(ServerControl.SelectedItem);
            ServerControl.SelectedItem = MainTab;
        }

        private void ServerControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ServerControl.SelectedItem == MainTab)
                CloseTab.Visibility = Visibility.Hidden;
            else
                CloseTab.Visibility = Visibility.Visible;
        }

        private void OpenTab_Click(object sender, RoutedEventArgs e)
        {
            ServerControl.SelectedItem = MainTab;
        }

        private void OpenWebsite_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/haecker-felix/BlockStation");
        }

        private void ReportError_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/haecker-felix/BlockStation/issues/new");
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Properties.Settings.Default.UpdateTimer = int.Parse(timebox.Text);

                if (rbDeutsch.IsChecked == true)
                {
                    Properties.Settings.Default.Language = "de";
                }
                if (rbEnglisch.IsChecked == true)
                {
                    Properties.Settings.Default.Language = "en";
                }
                if (rbPortugisisch.IsChecked == true)
                {
                    Properties.Settings.Default.Language = "pt";
                }


                if ((bool)savelastdir.IsChecked)
                {
                    Properties.Settings.Default.SaveLastServer = true;
                }
                else
                {
                    Properties.Settings.Default.SaveLastServer = false;
                }
                Properties.Settings.Default.Save();
                this.InitializeComponent();
            }
            catch
            {

            }
            
        }

        private void button_Copy_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SearchUpdatesClick(object sender, RoutedEventArgs e)
        {

            SearchForUpdates.IsEnabled = false;

            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = false;
            worker.DoWork += new DoWorkEventHandler(u.Search);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(UpdateSearchIsFinish);
            worker.ProgressChanged += new ProgressChangedEventHandler(ProgressChanged);
            worker.RunWorkerAsync();
        }

        private void UpdateSearchIsFinish(object sender, RunWorkerCompletedEventArgs e)
        {
            ProgressBar.Value = 0;
            if (u.update[1])
            {
                StableBuild.Content = u.build[1];
                StableChangeLog.Text = u.changelog[1];
                StableVersion.Content = u.version[1];
            }
            if (u.update[2])
            {
                UnstableBuild.Content = u.build[2];
                UnstableChangeLog.Text = u.changelog[2];
                UnstableVersion.Content = u.version[2];
            }
            if (u.update[3])
            {
                DevelopmentBuild.Content = u.build[3];
                DevelopmentChangeLog.Text = u.changelog[3];
                DevelopmentVersion.Content = u.version[3];
            }
            UpdateChannel_SelectionChanged(null, null);
        }

        void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // The progress percentage is a property of e
            ProgressBar.Value = e.ProgressPercentage;
        }

        private void UpdateChannel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (u.update[UpdateChannel.SelectedIndex+1])
            {
                InstallVersion.IsEnabled = true;
            }
            else
            {
                InstallVersion.IsEnabled = false;
            }
            
        }

        private void InstallVersion_Click(object sender, RoutedEventArgs e)
        {
            u.channel = UpdateChannel.SelectedIndex + 1;

            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = false;
            worker.DoWork += new DoWorkEventHandler(u.PrepareUpdate);
            worker.ProgressChanged += new ProgressChangedEventHandler(ProgressChanged);
            worker.RunWorkerAsync();
        }

        private void MainWindow1_Activated(object sender, EventArgs e)
        {
            if (App.UpdateMode)
            {
                ServerControl.Visibility = Visibility.Hidden;
                u.InstallUpdate();
            }
        }
    }
}
