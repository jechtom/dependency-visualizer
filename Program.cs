using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DependencyVisualizer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using (var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger())
            {
                Log.Logger = logger;

                Log.Logger.Information("Dependency Visualizer");

                string rootPath = @"c:\Avast\Repos\";

                var downloader = new Nuget.AggregateDownloader(
                    
                );

                var database = new PackageDatabase();
                var processFiles = new TransformManyBlock<string, Package>(new CSharpProjectFiles.PackageReader().ReadProjects);
                var buildDatabase = new ActionBlock<Package>(database.AddProject);

                processFiles.LinkTo(buildDatabase, new DataflowLinkOptions() { PropagateCompletion = true });

                foreach (string path in Directory.EnumerateFiles(rootPath, "*.*", SearchOption.AllDirectories)) await processFiles.SendAsync(path);
                processFiles.Complete();
                
                await buildDatabase.Completion;
                logger.Information("Found {projects} projects", database.ProjectsCount);
                var deps = database.ResolveAllDependencies().ToList();
                logger.Information("Found {dependencies} dependencies", deps.Count);

                database.DumpFile("dependencies.json");

                //foreach(var g in deps.Where(d => d.DependencyType == DependencyType.Nuget).GroupBy(d => d.Name))
                //{
                //    logger.Information("Nuget {g}", g.Key);
                //    foreach (var item in g)
                //    {
                //        logger.Information(" - ver {ver}", item.Version);
                //    }
                //}
            }
        }
    }
}
