using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DependencyVisualizer.Nuget
{
    public abstract class NugetDownloaderBase
    {
        protected virtual IEnumerable<Dependency> ReadDependencies(XmlDocument doc)
        {
            var dependencies = doc.SelectNodes("*[local-name() = 'package']/descendant::*[local-name() = 'dependencies']/descendant::*[local-name() = 'dependency']");

            foreach (XmlNode dependency in dependencies)
            {
                string id = dependency.Attributes.GetNamedItem("id").Value;
                string version = dependency.Attributes.GetNamedItem("version").Value;

                yield return new Dependency()
                {
                    DependencyType = DependencyType.Nuget,
                    Name = id,
                    Version = version
                };
            }
        }
    }
}
