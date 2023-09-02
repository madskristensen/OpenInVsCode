using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Management.Automation; 
using System.Text.RegularExpressions;

namespace OpenInVsCode
{
    public class Options : DialogPage
    {
        [Category("General")]
        [DisplayName("Command line arguments")]
        [Description("Command line arguments to pass to code.exe")]
        public string CommandLineArguments { get; set; }

        [Category("General")]
        [DisplayName("Path to code.exe")]
        [Description("Specify the path to code.exe.")]
        public string PathToExe { get {
            using var ps = PowerShell.Create(RunspaceMode.NewRunspace);
            ps.AddCommand("Get-Command").AddParameter("Name","code*.cmd").AddParameter("CommandType","Application").AddParameter("ErrorAction","SilentlyContinue");
            var codeCmd = ps.Invoke<ApplicationInfo>();

            if(!ps.HadErrors && codeCmd[0] != null && Regex.Match(codeCmd[0].Definition, @"Microsoft VS Code").Success)
            {
                string codePath = codeCmd[0].Definition;
                for (int i = 0; i < 2; i++)
                {
                    ps.Commands.Clear();
                    ps.AddCommand("Split-Path").AddParameter("Path", codePath).AddParameter("Parent").AddParameter("ErrorAction", "SilentlyContinue");
                    codePath = ps.Invoke<string>();
                }
                ps.Commands.Clear();
                ps.AddCommand("Resolve-Path").AddParameter("Path", Path.Combine(codePath, "Code*.exe")).AddParameter("ErrorAction", "SilentlyContinue");
                codePath = ps.Invoke<PathInfo>();
                if(!ps.HadErrors && codePath[0].Path != null && Regex.Match(FileVersionInfo.GetVersionInfo(codePath[0].Path).ProductName, @"Microsoft VS Code").Success){
                return codePath;
            }
            // foreach path in "%localappdata%\Programs", "%programfiles%", "%programfiles(x86)%" check for "\Microsoft VS Code*\Code*.exe"
            var paths = new string[] { Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Programs", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) };
            foreach (var path in paths)
            {
                ps.Commands.Clear();
                ps.AddCommand("Resolve-Path").AddParameter("Path", Path.Combine(path, "Microsoft VS Code*","Code*.exe")).AddParameter("ErrorAction", "SilentlyContinue");
                var resolvedPath = ps.Invoke<PathInfo>();
                if(!ps.HadErrors && resolvedPath[0].Path != null && Regex.Match(FileVersionInfo.GetVersionInfo(resolvedPath[0].Path).ProductName, @"Microsoft VS Code").Success){
                    return resolvedPath[0];
                }
            }
            return null;
        }
            set;
        }
        [Category("General")]
        [DisplayName("Open solution/project as regular file")]
        [Description("When true, opens solutions/projects as regular files and does not load folder path into VS Code.")]
        public bool OpenSolutionProjectAsRegularFile { get; set; }

        protected override void OnApply(PageApplyEventArgs e)
        {
            if (!File.Exists(PathToExe))
            {
                e.ApplyBehavior = ApplyKind.Cancel;
                MessageBox.Show($"The file \"{PathToExe}\" doesn't exist.", Vsix.Name, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            base.OnApply(e);
        }
    }
}
