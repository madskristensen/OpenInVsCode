using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;

namespace OpenInVsCode
{
    public class Options : DialogPage
    {
        const string _pathToExe = @"C:\Program Files (x86)\Microsoft VS Code\Code.exe";

        [Category("General")]
        [DisplayName("Path to code.exe")]
        [Description("Specify the path to code.exe.")]
        [DefaultValue(_pathToExe)]
        public string PathToExe { get; set; } = _pathToExe;

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
