using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace DependencyVisualizer.CSharpProjectFiles
{
    public class PackagesConfigDependencyReader
    {
        private readonly ILogger _logger = Log.ForContext<PackagesConfigDependencyReader>();
        private readonly string _path;

        public PackagesConfigDependencyReader(string path)
        {
            _path = path;
        }

        public IEnumerable<Dependency> ReadDependencies()
        {
            _logger.Debug("Reading packages.config: {path}", _path);

            var doc = new XmlDocument();
            doc.Load(_path);

            var packages = doc.SelectNodes("packages/package");

            foreach (XmlNode package in packages)
            {
                string id = package.Attributes.GetNamedItem("id").Value;
                string version = package.Attributes.GetNamedItem("version").Value;

                yield return new Dependency()
                {
                    DependencyType = DependencyType.Nuget,
                    Name = id,
                    Version = version,
                    IsFromPackagesConfig = true
                };
            }
        }
    }
}
