using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;

namespace OpenInVsCode
{
    public class Options : DialogPage
    {
        const string _pathToExe = @"C:\Program Files (x86)\Microsoft VS Code\Code.exe";
        const string _pathToExe64 = @"C:\Program Files\Microsoft VS Code\Code.exe";

        [Category("General")]
        [DisplayName("Path to code.exe")]
        [Description("Specify the path to code.exe.")]
        [DefaultValue(_pathToExe)]
        public string PathToExe { get; set; } = _pathToExe;

        [Category("General")]
        [DisplayName("Open solution/project as regular file")]
        [Description("When true, opens solutions/projects as regular files and does not load folder path into VS Code.")]
        public bool OpenSolutionProjectAsRegularFile { get; set; }

        public Options()
        {
            CheckFor64Bit();
        }

        private void CheckFor64Bit()
        {
            if (File.Exists(_pathToExe64))
            {
                PathToExe = _pathToExe64;
            }
        }

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
