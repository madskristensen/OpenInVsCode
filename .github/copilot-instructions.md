# Open in Visual Studio Code Extension

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Project Overview
Open in Visual Studio Code is a Visual Studio extension (.vsix) that adds context menu commands to open solutions, projects, folders, or files directly in Visual Studio Code. The extension detects VS Code installation automatically and provides configuration options in Visual Studio's Tools > Options.

## Build Requirements - CRITICAL
**WINDOWS ONLY**: This project can ONLY be built on Windows with Visual Studio installed. Do not attempt to build on Linux/macOS.

### Required Software
- Windows 10/11
- Visual Studio 2017/2019/2022 (Community, Professional, or Enterprise)
- Visual Studio SDK (included with VS installation when "Visual Studio extension development" workload is selected)
- .NET Framework 4.6 or higher

### Installation Commands
Install Visual Studio with the extension development workload:
```bash
# Download Visual Studio installer from https://visualstudio.microsoft.com/downloads/
# During installation, select "Visual Studio extension development" workload
# This includes the Visual Studio SDK and VSSDK Build Tools
```

## Working Effectively

### Bootstrap and Build
**NEVER CANCEL**: Build operations can take 5-10 minutes. Always set timeouts to 15+ minutes.

```bash
# Navigate to repository root
cd /path/to/OpenInVsCode

# Restore NuGet packages - takes 1-2 minutes, NEVER CANCEL, set timeout to 5+ minutes
nuget restore -Verbosity quiet
# OR use dotnet (if available on Windows):
dotnet restore OpenInVsCode.sln

# Build the extension - takes 5-10 minutes, NEVER CANCEL, set timeout to 15+ minutes  
msbuild /p:configuration=Release /p:DeployExtension=false /p:ZipPackageCompressionLevel=normal /v:m

# Build output: src/bin/Release/OpenInVsCode.vsix (installable extension package)
```

**Linux/macOS Warning**: If you attempt to build on non-Windows systems, you will encounter:
```
error MSB4019: The imported project ".../Microsoft.VsSDK.targets" was not found
```
This is expected - stop immediately and use a Windows environment.

### Development and Testing
**MANUAL VALIDATION REQUIRED**: After any code changes, you MUST test the extension in Visual Studio:

1. **Install the extension for testing**:
   ```bash
   # Double-click the generated VSIX file to install
   # OR use Visual Studio's experimental instance:
   devenv.exe /rootsuffix Exp
   ```

2. **Test scenarios - ALWAYS run these after changes**:
   - Right-click a solution in Solution Explorer → "Open in Visual Studio Code"
   - Right-click a project in Solution Explorer → "Open in Visual Studio Code"  
   - Right-click a folder in Solution Explorer → "Open in Visual Studio Code"
   - Right-click a file in Solution Explorer → "Open in Visual Studio Code"
   - Use keyboard shortcut `Ctrl+Shift+Y` to open current file
   - Test with non-default VS Code installation path
   - Verify extension options in Tools → Options → Web → Open In Visual Studio Code

3. **Expected behavior validation**:
   - VS Code opens with the correct file/folder/project loaded
   - Extension auto-detects VS Code installation path
   - Manual path configuration works in options dialog
   - All context menu items appear correctly
   - Keyboard shortcut functions

### Build Artifacts and Packaging
- Main output: `src/bin/Release/OpenInVsCode.vsix` (installable extension)
- Debug symbols: `src/bin/Release/OpenInVsCode.pdb`
- Manifest: `src/source.extension.vsixmanifest` (defines extension metadata)

## Code Analysis and Quality
Always run these before committing - CI will fail otherwise:
```bash
# Run Code Analysis in Debug configuration - takes 2-3 minutes, set timeout to 5+ minutes
msbuild /p:configuration=Debug /p:RunCodeAnalysis=true /v:m

# Organize usings (in Visual Studio):
# Right-click any C# file → "Organize Usings" → "Remove and Sort"
# OR use BatchFormat extension for bulk operations
```

## Project Structure
Key files and directories you'll work with:

### Core Implementation
- `src/Commands/OpenVsCodeCommand.cs` - Main command logic and VS Code detection
- `src/Options.cs` - Extension options/settings page
- `src/VSPackage.cs` - Extension package entry point
- `src/Helpers/ProjectHelpers.cs` - Solution/project path utilities
- `src/Helpers/Logger.cs` - Extension logging utilities

### Configuration Files
- `OpenInVsCode.sln` - Visual Studio solution file
- `src/OpenInVsCode.csproj` - C# project file with SDK references
- `src/source.extension.vsixmanifest` - Extension manifest (version, dependencies, etc.)
- `src/VSCommands.vsct` - Command definitions and UI integration
- `appveyor.yml` - CI/CD pipeline configuration

### Key Dependencies
- Microsoft.VisualStudio.SDK (15.0.1) - Core VS extensibility
- Microsoft.VSSDK.BuildTools (17.14.2101) - Build tools for VSIX creation
- .NET Framework 4.6 - Target framework

## CI/CD Pipeline
AppVeyor builds on Visual Studio 2022 with these steps:
1. Increment VSIX version and update build version
2. Restore NuGet packages (`nuget restore -Verbosity quiet`)
3. Build (`msbuild /p:configuration=Release /p:DeployExtension=false /p:ZipPackageCompressionLevel=normal /v:m`)
4. Publish artifacts and deploy to Visual Studio Gallery

**Build time expectations**:
- NuGet restore: 1-2 minutes
- Full build: 5-10 minutes
- Total CI pipeline: 10-15 minutes

## Common Development Tasks

### Debugging the Extension
```bash
# Launch Visual Studio Experimental Instance with extension loaded
devenv.exe /rootsuffix Exp

# Or set project debug properties:
# Start Action: External Program
# Start Program: $(DevEnvDir)\devenv.exe  
# Start Arguments: /rootsuffix Exp
```

### Updating Extension Version
1. Edit `src/source.extension.vsixmanifest` - update Version attribute
2. Build automatically updates `src/source.extension.cs` with new version

### Adding New Commands
1. Define command in `src/VSCommands.vsct` (buttons, key bindings, command placement)
2. Add command handler in `src/Commands/OpenVsCodeCommand.cs`
3. Register command in `Initialize()` method

### Modifying Options
1. Add properties to `src/Options.cs` with appropriate attributes
2. Use options in command logic via injected `Options` instance
3. Options automatically appear in Tools → Options → Web → Open In Visual Studio Code

## Troubleshooting

### Build Failures
- **"Microsoft.VsSDK.targets not found"**: Install Visual Studio SDK via VS installer
- **"Package reference could not be resolved"**: Run `nuget restore` first
- **Build timeout**: Use 15+ minute timeouts, builds can be slow

### Extension Testing Issues  
- **Extension not loading**: Check Visual Studio's experimental registry hive
- **Commands not appearing**: Verify VSCommands.vsct syntax and GUID references
- **VS Code not opening**: Check extension detects Code.exe path correctly

### Development Environment
- **Only build on Windows**: Linux/macOS builds will fail due to Visual Studio SDK requirements
- **Use Visual Studio**: While VS Code can edit the code, debugging and testing requires Visual Studio
- **Experimental instance**: Always test in VS experimental instance to avoid disrupting your main VS installation

### Key Code Dependencies
When making changes, always verify these integrations still work:
- **OpenVsCodeCommand.cs**: Core functionality - any changes require full test suite
- **VSCommands.vsct**: UI integration - test all context menus and keyboard shortcuts
- **Options.cs**: Settings page - verify Tools → Options → Web → Open In Visual Studio Code
- **ProjectHelpers.cs**: Solution Explorer integration - test with different project types
- **VsCodeDetect methods**: VS Code path detection - test with various installation paths

## Common Commands and Expected Outputs

### Repository Root Contents
```bash
ls -la /path/to/OpenInVsCode
```
Expected output:
```
.editorconfig
.git/
.gitattributes  
.gitignore
CONTRIBUTING.md
LICENSE
OpenInVsCode.sln          # Main solution file
README.md
appveyor.yml             # CI configuration
art/                     # Screenshots and images
src/                     # Source code directory
```

### Source Directory Structure  
```bash
ls -la src/
```
Expected output:
```
Commands/                # Command implementations
  OpenVsCodeCommand.cs   # Main extension logic
Helpers/                 # Utility classes
  Logger.cs             # Extension logging
  ProjectHelpers.cs     # VS project integration
OpenInVsCode.csproj     # Project file with SDK references  
Options.cs              # Extension settings/options
Properties/             # Assembly metadata
Resources/              # Icons and assets
VSCommands.cs          # Auto-generated command definitions
VSCommands.vsct        # Command UI definitions
VSPackage.cs           # Extension package entry point
source.extension.cs    # Auto-generated manifest code
source.extension.ico   # Extension icon
source.extension.vsixmanifest  # Extension manifest
```

### Package Information
```bash
# View main project dependencies
type src/OpenInVsCode.csproj | findstr PackageReference
```
Expected packages:
- Microsoft.VisualStudio.SDK (15.0.1) 
- Microsoft.VSSDK.BuildTools (17.14.2101)

## Contributing Guidelines
Before submitting changes:
1. Build successfully in Release configuration  
2. Run Code Analysis and fix all CA issues
3. Test all extension functionality manually
4. Organize usings in all modified C# files
5. Follow existing code style and indentation
6. Prepend feature name to commit messages (e.g., "Commands: Add new VS Code detection method")

Remember: This extension integrates deeply with Visual Studio's shell and project system. Always validate changes don't break core functionality like Solution Explorer integration or keyboard shortcuts.