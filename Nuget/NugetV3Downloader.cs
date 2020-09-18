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
    public class NugetV3Downloader : NugetDownloaderBase, INugetDownloader
    {
        HttpClient _client;

        public NugetV3Downloader(string baseAddress)
        {
            _client = new HttpClient()
            {
                BaseAddress = new Uri(baseAddress)
            };
        }

        public async Task<Package> FetchDependenciesAsync(Dependency dep)
        {
            string url = $"v3-flatcontainer/{dep.Name.ToLowerInvariant()}/{dep.Version.ToLowerInvariant()}/{dep.Name.ToLowerInvariant()}.nuspec";
            HttpResponseMessage response = await _client.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            response.EnsureSuccessStatusCode();

            using (var memStream = new MemoryStream())
            {
                await response.Content.CopyToAsync(memStream);
                
                memStream.Position = 0;

                var doc = new XmlDocument();
                doc.Load(memStream);
                    
                var package = new Package()
                {
                    Name = dep.Name,
                    Version = dep.Version,
                    Type = PackageType.Nuget,
                    Dependencies = ReadDependencies(doc).ToList()
                };
                return package;
            }
        }
    }
}
