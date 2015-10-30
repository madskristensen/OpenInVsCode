using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace OpenInVsCode
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", Version, IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.guidPackageString)]
    public sealed class VSPackage : Package
    {
        public const string Version = "1.0";
        public const string Title = "Open in Visual Studio Code";

        protected override void Initialize()
        {
            Telemetry.Initialize(this, Version, "939ca576-9e8b-474a-a9d7-92117432e5d6");
            OpenFolderCommand.Initialize(this);

            base.Initialize();
        }
    }
}
