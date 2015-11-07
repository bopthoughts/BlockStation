using BlockStation.gui;
using System;
using System.Windows;

namespace BlockStation
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
    public partial class App : System.Windows.Application
    {
        public static bool UpdateMode = false;

        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeGUI()
        {
            this.StartupUri = new System.Uri("View\\MainWindow.xaml", System.UriKind.Relative);

        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                if (e.Args[0] == "update")
                {
                    UpdateMode = true;
                }
                else
                {
                    System.IO.File.Delete(Utils.ProgramFilesx86() + "\\BlockStation\\BlockStation_old.exe");
                }
            }
            catch
            {

            }
            
        }

        [System.STAThreadAttribute()]
        public static void Main()
        {
            string[] args = Environment.GetCommandLineArgs();

            BlockStation.App app = new BlockStation.App();
            app.InitializeGUI();
            app.Run();


        }
    }
}