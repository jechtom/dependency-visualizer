using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace DependencyVisualizer.CSharpProjectFiles
{
    public class CsprojFileReader
    {
        private readonly ILogger _logger = Log.ForContext<CsprojFileReader>();
        private readonly string _path;

        public CsprojFileReader(string path)
        {
            _path = path ?? throw new ArgumentNullException(nameof(path));
        }

        private IEnumerable<Dependency> ReadDependencies(XmlDocument csproj)
        {
            // ProjectReferences - references to other projects in solution

            var projectReferences = csproj.SelectNodes("*[local-name() = 'Project']/*[local-name() = 'ItemGroup']/*[local-name() = 'ProjectReference']");

            foreach (XmlNode projectReference in projectReferences)
            {
                string pathToProject = projectReference.Attributes.GetNamedItem("Include").Value;

                yield return new Dependency()
                {
                    DependencyType = DependencyType.Project,
                    Name = Path.GetFileNameWithoutExtension(pathToProject),
                    Path = Path.GetFullPath(Path.Combine(_path, pathToProject))
                };
            }

            // PackageReference element - unified way to define dependencies in .NET Core / .NET Framework

            var packageReferences = csproj.SelectNodes("*[local-name() = 'Project']/*[local-name() = 'ItemGroup']/*[local-name() = 'PackageReference']");

            foreach (XmlNode packageReference in packageReferences)
            {
                string packageName = packageReference.Attributes.GetNamedItem("Include").Value;

                // read version from attribute 'version' or nested element 'version'
                string version = packageReference.Attributes.GetNamedItem("Version")?.Value;
                if (version == null)
                {
                    version = packageReference.SelectSingleNode("*[local-name() = 'Version']").InnerText;
                }

                yield return new Dependency()
                {
                    DependencyType = DependencyType.Nuget,
                    Name = packageName,
                    Version = version
                };
            }

            // Reference element - .NET Framework for GAC, file reference and DLL references from packages.config file

            var fileReferences = csproj.SelectNodes("*[local-name() = 'Project']/*[local-name() = 'ItemGroup']/*[local-name() = 'Reference']");

            foreach (XmlNode fileReference in fileReferences)
            {
                string fileReferenceName = fileReference.Attributes.GetNamedItem("Include").Value;
                var fileReferenceParsedName = new System.Reflection.AssemblyName(fileReferenceName);
                string hintPath = fileReference.SelectSingleNode("*[local-name() = 'HintPath']")?.InnerText;

                yield return new Dependency()
                {
                    DependencyType = hintPath == null ? DependencyType.GAC : DependencyType.File,
                    Path = hintPath == null ? null : Path.GetFullPath(Path.Combine(_path, hintPath)),
                    Name = fileReferenceParsedName.Name,
                    Version = fileReferenceParsedName.Version?.ToString()
                };
            }

            // packages.config file - legacy .NET Framework dependencies file

            string packagesConfigFilePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(_path), "packages.config"));
            if(File.Exists(packagesConfigFilePath))
            {
                foreach (var dep in new PackagesConfigDependencyReader(packagesConfigFilePath).ReadDependencies())
                {
                    yield return dep;
                }
            }
        }

        public Package ReadProject()
        {
            _logger.Debug("Reading csproj: {path}", _path);

            // remark: using non-namespace specific queries as newer projects don't have XML namespace
            var doc = new XmlDocument();
            doc.Load(_path);

            string projectAssemblyName = doc.SelectSingleNode("*[local-name() = 'Project']/*[local-name() = 'PropertyGroup']/*[local-name() = 'AssemblyName']")?.InnerText;

            var project = new Package()
            {
                Name = projectAssemblyName ?? Path.GetFileNameWithoutExtension(_path),
                Path = _path,
                Dependencies = ReadDependencies(doc).ToList()
            };

            // remove duplicates if reference is added with packages.config and then also in Reference element as DLL reference in project file
            DeduplicatePackagesConfigDependencies(project);

            return project;
        }

        private void DeduplicatePackagesConfigDependencies(Package project)
        {
            var depsToDelete = new List<Dependency>();

            foreach(var dep in project.Dependencies.Where(d => d.IsFromPackagesConfig))
            {
                // dependencies from packages.config contains also file reference to DLL (if Nuget contains DLL reference) with name and version in given path - this can be removed as duplicate
                string expectedDllPath = $@"\{dep.Name}.{dep.Version}\";
                foreach(var depToDelete in project.Dependencies
                    .Where(d => d.DependencyType == DependencyType.File && d.Path.Contains(expectedDllPath, StringComparison.OrdinalIgnoreCase)))
                {
                    depsToDelete.Add(depToDelete);
                }
            }

            foreach (var depToDelete in depsToDelete)
            {
                project.Dependencies.Remove(depToDelete);
            }
        }
    }
}
