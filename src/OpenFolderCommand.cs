using System;
using System.ComponentModel.Design;
using System.IO;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace OpenInVsCode
{
    internal sealed class OpenFolderCommand
    {
        private readonly Package _package;

        private OpenFolderCommand(Package package)
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

        public static OpenFolderCommand Instance { get; private set; }

        private IServiceProvider ServiceProvider
        {
            get { return _package; }
        }

        public static void Initialize(Package package)
        {
            Instance = new OpenFolderCommand(package);
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
                    System.Windows.Forms.MessageBox.Show("Couldn't resolve the folder");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private static void OpenVsCode(string path)
        {
            bool isDirectory = Directory.Exists(path);

            var start = new System.Diagnostics.ProcessStartInfo()
            {
                WorkingDirectory = path,
                FileName = "code",
                Arguments = isDirectory ? "." : $"\"{path}\"",
                CreateNoWindow = true,
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
            };

            using (System.Diagnostics.Process.Start(start))
                Telemetry.TrackEvent("Open in VS Code");
        }
    }
}
