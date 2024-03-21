namespace ChipUtillo
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            Application.ThreadException += Application_ThreadException;
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Application.Exit(new System.ComponentModel.CancelEventArgs(false));
            using(StreamWriter sw=new StreamWriter("log.txt", true))
            {
                sw.WriteLine(e.Exception.ToString());
                sw.WriteLine();
            }
            try
            {
                Form1.asusCtrl.SetFanSpeeds(0);
                Form1.asusCtrl.Dispose();
            }
            catch
            {

            }

            FanModeManager.SetFanStatus(Form1.prevFanMod);
        }
    }
}