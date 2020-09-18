using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace DependencyVisualizer.CSharpProjectFiles
{
    public class PackageReader
    {
        private readonly ILogger _logger = Log.ForContext<PackageReader>();

        public IEnumerable<Package> ReadProjects(string path)
        {
            switch (path)
            {
                case string _ when path.EndsWith(".csproj"):
                    var reader = new CsprojFileReader(path);
                    Package project = reader.ReadProject();
                    yield return project;
                    break;
            }
        }
    }
}
