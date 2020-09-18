using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DependencyVisualizer
{
    public class PackageDatabase
    {
        private readonly ILogger _logger = Log.ForContext<PackageDatabase>();
        private Dictionary<string, Package> _projects;

        public PackageDatabase()
        {
            _projects = new Dictionary<string, Package>(StringComparer.OrdinalIgnoreCase);
        }

        public int ProjectsCount => _projects.Count;

        public void AddProject(Package project)
        {
            if (project is null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            _projects.Add(project.Path, project);
        }

        public IEnumerable<Dependency> ResolveAllDependencies()
        {
            var processedDependencies = new HashSet<string>();

            string GetDepCompareId(Dependency dep) => $"{dep.Name.ToLowerInvariant()}.{dep.Version?.ToLowerInvariant()}";

            foreach(var dep in _projects.Values.SelectMany(p => p.Dependencies))
            {
                string compareId = GetDepCompareId(dep);
                if (!processedDependencies.Add(compareId)) continue;
                yield return dep;
            }
        }

        public void DumpFile(string path)
        {
            using(var fileStream = new StreamWriter(path))
            {
                var serializer = new Newtonsoft.Json.JsonSerializer();
                serializer.Serialize(fileStream, _projects.Values);
            }
            _logger.Information("Output file: {path}", path);
        }
    }
}
