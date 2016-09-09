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
        private Options _options;

        private OpenVsCodeCommand(Package package, Options options)
        {
            _package = package;
            _options = options;

            var commandService = (OleMenuCommandService)ServiceProvider.GetService(typeof(IMenuCommandService));

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

        public static void Initialize(Package package, Options options)
        {
            Instance = new OpenVsCodeCommand(package, options);
        }

        private void OpenFolderInVs(object sender, EventArgs e)
        {
            try
            {
                var dte = (DTE2)ServiceProvider.GetService(typeof(DTE));
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

        private void OpenVsCode(string path)
        {
            EnsurePathExist();
            bool isDirectory = Directory.Exists(path);
            string cwd = File.Exists(path) ? Path.GetDirectoryName(path) : path;

            var start = new System.Diagnostics.ProcessStartInfo()
            {
                WorkingDirectory = cwd,
                FileName = _options.PathToExe,
                Arguments = isDirectory ? "." : $"\"{path}\"",
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
            };

            using (System.Diagnostics.Process.Start(start))
            {
                string evt = isDirectory ? "directory" : "file";
            }
        }

        private void EnsurePathExist()
        {
            if (File.Exists(_options.PathToExe))
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
                _options.PathToExe = dialog.FileName;
                _options.SaveSettingsToStorage();
            }
        }
    }
}
