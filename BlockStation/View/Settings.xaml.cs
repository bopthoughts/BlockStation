using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Interaktionslogik für Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            Utils.SetLanguage(this);
            timebox.Text = Properties.Settings.Default.UpdateTimer.ToString();

            switch (Properties.Settings.Default.Language.ToString())
            {
                case "de": rbDeutsch.IsChecked = true; break;
                case "en": rbEnglisch.IsChecked = true; break;
                case "fr": rbPortugisisch.IsChecked = true; break;
            }
            switch (Properties.Settings.Default.SaveLastServer)
            {
                case true: savelastdir.IsChecked = true; break;
                case false: savelastdir.IsChecked = false; break;
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.UpdateTimer = int.Parse(timebox.Text);

            if(rbDeutsch.IsChecked == true)
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
            this.Close();
        }

        private void button_Copy_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
