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
    public class NugetV2Downloader : NugetDownloaderBase, INugetDownloader
    {
        HttpClient _client;

        public NugetV2Downloader(string baseAddress)
        {
            _client = new HttpClient()
            {
                BaseAddress = new Uri(baseAddress)
            };
        }

        public async Task<Package> FetchDependenciesAsync(Dependency dep)
        {
            HttpResponseMessage response = await _client.GetAsync($"package/{dep.Name}/{dep.Version}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            response.EnsureSuccessStatusCode();

            using (var memStream = new MemoryStream())
            {
                await response.Content.CopyToAsync(memStream);

                memStream.Position = 0;

                using (var archive = new ZipArchive(memStream, ZipArchiveMode.Read))
                {
                    var doc = new XmlDocument();
                    using (var xmlDocStream = archive.GetEntry($"{dep.Name}.nuspec").Open())
                    {
                        doc.Load(xmlDocStream);
                    }

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
}
