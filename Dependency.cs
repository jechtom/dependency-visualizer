using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DependencyVisualizer
{
    public class Dependency
    {
        public string Name { get; set; }

        public string Path { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public DependencyType DependencyType { get; set; }

        public string Version { get; set; }
        
        public bool IsFromPackagesConfig { get; set; }
        
        public override string ToString() => $"{Name};{Version};{DependencyType};{Path}"; 
    }
}