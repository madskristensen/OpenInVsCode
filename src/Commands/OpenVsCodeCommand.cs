using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;
using Microsoft;

namespace OpenInVsCode
{
    internal sealed class OpenVsCodeCommand
    {
        private readonly Package _package;
        private readonly Options _options;

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

                var currentCommandID = new CommandID(PackageGuids.guidOpenCurrentInVsCmdSet, PackageIds.OpenCurrentInVs);
                var currentItem = new MenuCommand(OpenCurrentFileInVs, currentCommandID);
                commandService.AddCommand(currentItem);
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

        private void OpenCurrentFileInVs(object sender, EventArgs e)
        {
            try
            {
                var dte = (DTE2) ServiceProvider.GetService(typeof(DTE));
                Assumes.Present(dte);

                var activeDocument = dte.ActiveDocument;

                if (activeDocument != null)
                {
                    var path = activeDocument.FullName;

                    if (!string.IsNullOrEmpty(path))
                    {
                        int line = 0;
                        int column = 0;

                        if (activeDocument.Selection is TextSelection selection)
                        {
                            line = selection.ActivePoint.Line;
                            // note:
                            // LineCharOffset not DisplayColumn
                            // as it described `code -h`: -g --goto <file:line[:character]>
                            column = selection.ActivePoint.LineCharOffset;
                        }

                        OpenVsCode(path, line, column);
                    }
                    else
                    {
                        MessageBox.Show("Couldn't resolve the folder");
                    }
                }
                else
                {
                    MessageBox.Show("Couldn't find active document");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private void OpenFolderInVs(object sender, EventArgs e)
        {
            try
            {
                var dte = (DTE2)ServiceProvider.GetService(typeof(DTE));
                Assumes.Present(dte);

                string path = ProjectHelpers.GetSelectedPath(dte, _options.OpenSolutionProjectAsRegularFile);

                if (!string.IsNullOrEmpty(path))
                {
                    int line = 0;
                    int column = 0;

                    if (dte.ActiveDocument?.Selection is TextSelection selection)
                    {
                        line = selection.ActivePoint.Line;
                        column = selection.ActivePoint.LineCharOffset;
                    }

                    OpenVsCode(path, line, column);
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

        private void OpenVsCode(string path, int line = 0, int column = 0)
        {
            EnsurePathExist();
            bool isDirectory = Directory.Exists(path);

            var args = isDirectory
                ? "."
                : line > 0
                    ? column > 0
                        ? $"-g {path}:{line}:{column}"
                        : $"-g {path}:{line}"
                    : $"{path}";
            if (!string.IsNullOrEmpty(_options.CommandLineArguments))
            {
                args = $"{args} {_options.CommandLineArguments}";
            }

            var start = new System.Diagnostics.ProcessStartInfo()
            {
                FileName = $"\"{_options.PathToExe}\"",
                Arguments = args,
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
            };

            if (isDirectory)
            {
                start.WorkingDirectory = path;
            }

            using (System.Diagnostics.Process.Start(start))
            {

            }
        }

        private void EnsurePathExist()
        {
            if (File.Exists(_options.PathToExe))
                return;

            if (!string.IsNullOrEmpty(VsCodeDetect.InRegistry()))
            {
                SaveOptions(_options, VsCodeDetect.InRegistry());
            }
            else if (!string.IsNullOrEmpty(VsCodeDetect.InEnvVarPath()))
            {
                SaveOptions(_options, VsCodeDetect.InEnvVarPath());
            }
            else if (!string.IsNullOrEmpty(VsCodeDetect.InLocalAppData()))
            {
                SaveOptions(_options, VsCodeDetect.InLocalAppData());
            }
            else
            {
                var box = MessageBox.Show(
                    "I can't find Visual Studio Code (Code.exe). Would you like to help me find it?", Vsix.Name,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (box == DialogResult.No)
                    return;

                var dialog = new OpenFileDialog
                {
                    DefaultExt = ".exe",
                    FileName = "Code.exe",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    CheckFileExists = true
                };

                var result = dialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    SaveOptions(_options, dialog.FileName);
                }
            }
        }

        private void SaveOptions(Options options, string path)
        {
            options.PathToExe = path;
            options.SaveSettingsToStorage();
        }

    }

    internal static class VsCodeDetect
    {
        internal static string InRegistry()
        {
            var key = Registry.CurrentUser;
            var name = "Icon";
            try
            {
                var subKey = key.OpenSubKey(@"SOFTWARE\Classes\*\shell\VSCode\");
                var value = subKey.GetValue(name).ToString();
                if (File.Exists(value))
                {
                    return value;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        internal static string InLocalAppData()
        {
            var localAppData = Environment.GetEnvironmentVariable("LOCALAPPDATA");

            var codePartDir = @"Programs\Microsoft VS Code";
            var codeDir = Path.Combine(localAppData, codePartDir);
            var drives = DriveInfo.GetDrives();

            foreach (var drive in drives)
            {
                if (drive.DriveType == DriveType.Fixed)
                {
                    var path = Path.Combine(drive.Name[0] + codeDir.Substring(1), "code.exe");
                    if (File.Exists(path))
                    {
                        return path;
                    }
                }
            }

            return null;
        }

        internal static string InEnvVarPath()
        {
            var envPath = Environment.GetEnvironmentVariable("Path");
            var paths = envPath.Split(';');
            var parentDir = "Microsoft VS Code";
            foreach (var path in paths)
            {
                if (path.ToLower().Contains("code"))
                {
                    var temp = Path.Combine(path.Substring(0, path.IndexOf(parentDir, StringComparison.InvariantCultureIgnoreCase)),
                        parentDir, "code.exe");
                    if (File.Exists(temp))
                    {
                        return temp;
                    }
                }
            }
            return null;
        }
    }
}
