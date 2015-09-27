namespace BlockStation
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
    public partial class App : System.Windows.Application
    {
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent()
        {
            this.StartupUri = new System.Uri("ServerSelector.xaml", System.UriKind.Relative);

        }
    
        //-----------------------------------------------------------------------------------------------
        public static string AppVersion = "0.4";
        public static string AppVersionText = "Alpha";
        public static string BuildDate = "27.09.2015";
        public static string BuildVersion = "20150927";
        public static string CopyHint = "Sie sind nicht zur Weitergabe dieses Programmes berechtigt.";
        public static string Autor = "Felix Häcker";
        //-----------------------------------------------------------------------------------------------

        [System.STAThreadAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public static void Main()
        {
            BlockStation.App app = new BlockStation.App();
            app.InitializeComponent();
            app.Run();
        }
    }
}