using BlockStation.gui;
using System;
using System.Windows;

namespace BlockStation
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
    public partial class App : System.Windows.Application
    {
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeGUI()
        {
            this.StartupUri = new System.Uri("View\\ServerSelector.xaml", System.UriKind.Relative);

        }

        [System.STAThreadAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public static void Main()
        {
            try
            {
                BlockStation.App app = new BlockStation.App();
                app.InitializeGUI();
                app.Run();
            }
            catch (Exception e)
            {
                MessageBox.Show("Es ist ein unbekannter Fehler augetreten.\nBitte melden sie diesen Fehler:\n\n" + e, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }
}