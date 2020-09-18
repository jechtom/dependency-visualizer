using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace DependencyVisualizer
{
    public class Package
    {
        public string Name { get; set; }
        
        public string Path { get; set; }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public PackageType Type { get; set; }
        
        public List<Dependency> Dependencies { get; set; }
        
        public string Version { get; set; }
    }
}
