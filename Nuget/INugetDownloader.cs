using System.Threading.Tasks;

namespace DependencyVisualizer.Nuget
{
    public interface INugetDownloader
    {
        Task<Package> FetchDependenciesAsync(Dependency dep);
    }
}