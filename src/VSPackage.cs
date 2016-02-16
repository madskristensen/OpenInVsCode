using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace OpenInVsCode
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.guidPackageString)]
    public sealed class VSPackage : Package
    {

        protected override void Initialize()
        {
            Logger.Initialize(this, Vsix.Name);
            Telemetry.Initialize(this, Vsix.Version, "939ca576-9e8b-474a-a9d7-92117432e5d6");
            OpenVsCodeCommand.Initialize(this);

            base.Initialize();
        }
    }
}
