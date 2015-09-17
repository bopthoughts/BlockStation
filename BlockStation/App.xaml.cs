namespace BlockStation
{


    /// <summary>
    /// App
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
    public partial class App : System.Windows.Application
    {

        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent()
        {

#line 4 "..\..\..\App.xaml"
            this.StartupUri = new System.Uri("ServerSelector.xaml", System.UriKind.Relative);

#line default
#line hidden
        }

        //-----------------------------------------------------------------------------------------------
        public static string AppVersion = "Preview";
        public static string AppVersionText = "INDEV";
        public static string BuildDate = "16.09.2015";
        public static string BuildVersion = "16092015r3";
        public static string CopyHint = "Sie sind nicht zur Weitergabe dieses Programmes berechtigt.";
        public static string Autor = "Felix Häcker";
        //-----------------------------------------------------------------------------------------------
        /// <summary>
        /// Application Entry Point.
        /// </summary>
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