using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;

namespace OpenInVsCode
{
    internal static class ProjectHelpers
    {
        public static string GetSelectedPath(DTE2 dte)
        {
            var items = (Array)dte.ToolWindows.SolutionExplorer.SelectedItems;
            var files = new List<string>();

            foreach (UIHierarchyItem selItem in items)
            {
                ProjectItem item = selItem.Object as ProjectItem;

                if (item != null)
                    files.Add(item.GetFilePath());

                Project proj = selItem.Object as Project;

                if (proj != null)
                    return proj.GetRootFolder();

                Solution sol = selItem.Object as Solution;

                if (sol != null)
                    return Path.GetDirectoryName(sol.FileName);
            }

            return files.Count > 0 ? String.Join(" ", files) : null;
        }

        public static string GetFilePath(this ProjectItem item)
        {
            return $"\"{item.FileNames[1]}\""; // Indexing starts from 1
        }

        public static string GetRootFolder(this Project project)
        {
            if (string.IsNullOrEmpty(project.FullName))
                return null;

            string fullPath;

            try
            {
                fullPath = project.Properties.Item("FullPath").Value as string;
            }
            catch (ArgumentException)
            {
                try
                {
                    // MFC projects don't have FullPath, and there seems to be no way to query existence
                    fullPath = project.Properties.Item("ProjectDirectory").Value as string;
                }
                catch (ArgumentException)
                {
                    // Installer projects have a ProjectPath.
                    fullPath = project.Properties.Item("ProjectPath").Value as string;
                }
            }

            if (string.IsNullOrEmpty(fullPath))
                return File.Exists(project.FullName) ? Path.GetDirectoryName(project.FullName) : null;

            if (Directory.Exists(fullPath))
                return fullPath;

            if (File.Exists(fullPath))
                return Path.GetDirectoryName(fullPath);

            return null;
        }
    }
}
