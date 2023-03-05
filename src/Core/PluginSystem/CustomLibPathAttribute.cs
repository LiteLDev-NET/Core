using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteLoader.NET.PluginSystem;

public class CustomLibPathAttribute
{
    string Path { get; set; }

    public CustomLibPathAttribute(string path)
    {
        Path = path;
    }
}
