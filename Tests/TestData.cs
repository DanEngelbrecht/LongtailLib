using System;
using System.IO.Abstractions;

namespace Tests
{
    internal static class TestData
    {
        public static string GetProjectRootPath(IFileSystem fileSystem)
        {
            string cwd = fileSystem.Directory.GetCurrentDirectory();
            string projectRootDir = nameof(Tests);
            while (!projectRootDir.Equals(fileSystem.Directory.GetParent(cwd).Name, StringComparison.InvariantCultureIgnoreCase))
            {
                cwd = fileSystem.Directory.GetParent(cwd).FullName;
            }
            return fileSystem.Directory.GetParent(cwd).FullName;
        }

        public static string GetTestDataPath(IFileSystem fileSystem)
        {
            return fileSystem.Path.Combine(GetProjectRootPath(fileSystem), "TestData");
        }
    }
}
