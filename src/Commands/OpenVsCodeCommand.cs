using System;
using System.ComponentModel.Design;
using System.IO;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace OpenInVsCode
{
    internal sealed class OpenVsCodeCommand
    {
        private readonly Package _package;

        private OpenVsCodeCommand(Package package)
        {
            _package = package;

            OleMenuCommandService commandService = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(PackageGuids.guidOpenInVsCmdSet, PackageIds.OpenInVs);
                var menuItem = new MenuCommand(OpenFolderInVs, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        public static OpenVsCodeCommand Instance { get; private set; }

        private IServiceProvider ServiceProvider
        {
            get { return _package; }
        }

        public static void Initialize(Package package)
        {
            Instance = new OpenVsCodeCommand(package);
        }

        private void OpenFolderInVs(object sender, EventArgs e)
        {
            var dte = (DTE2)ServiceProvider.GetService(typeof(DTE));

            try
            {
                string path = ProjectHelpers.GetSelectedPath(dte);

                if (!string.IsNullOrEmpty(path))
                {
                    OpenVsCode(path);
                }
                else
                {
                    MessageBox.Show("Couldn't resolve the folder");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private static void OpenVsCode(string path)
        {
            EnsurePathExist();
            bool isDirectory = Directory.Exists(path);
            string cwd = File.Exists(path) ? Path.GetDirectoryName(path) : path;

            var start = new System.Diagnostics.ProcessStartInfo()
            {
                WorkingDirectory = cwd,
                FileName = VSPackage.Options.PathToExe,
                Arguments = isDirectory ? "." : $"\"{path}\"",
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
            };

            using (System.Diagnostics.Process.Start(start))
            {
                string evt = isDirectory ? "directory" : "file";
                Telemetry.TrackEvent($"Open {evt}");
            }
        }

        private static void EnsurePathExist()
        {
            if (File.Exists(VSPackage.Options.PathToExe))
                return;

            var box = MessageBox.Show("I can't find Visual Studio Code (Code.exe). Would you like to help me find it?", Vsix.Name, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (box == DialogResult.No)
                return;

            var dialog = new OpenFileDialog();
            dialog.DefaultExt = ".exe";
            dialog.FileName = "Code.exe";
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            dialog.CheckFileExists = true;

            var result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                VSPackage.Options.PathToExe = dialog.FileName;
                VSPackage.Options.SaveSettingsToStorage();
            }
        }
    }
}
