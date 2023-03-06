using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteLoader.NET.PluginSystem;

[AttributeUsage(AttributeTargets.Assembly)]
public class CustomLibPathAttribute : Attribute
{
    public string Path { get; set; }

    public CustomLibPathAttribute(string path)
    {
        Path = path;
    }
}
