using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DependencyVisualizer.Nuget
{
    public class AggregateDownloader : INugetDownloader
    {
        private readonly INugetDownloader[] downloaders;

        public AggregateDownloader(params INugetDownloader[] downloaders)
        {
            this.downloaders = downloaders ?? throw new ArgumentNullException(nameof(downloaders));
        }

        public async Task<Package> FetchDependenciesAsync(Dependency dep)
        {
            foreach (var item in downloaders)
            {
                var result = await item.FetchDependenciesAsync(dep);
                if (result != null) return result;
            }

            return null;
        }
    }
}
