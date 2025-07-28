using System.Diagnostics.CodeAnalysis;

namespace DesktopApp
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        [Experimental("WFO5001")]
        static void Main()
        {
            Application.SetColorMode(SystemColorMode.System);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var form1 = new MainForm();
            Application.Run(form1);
        }
    }
}
