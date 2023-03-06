using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteLoader.NET.PluginSystem;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
public class PluginMainAttribute : Attribute
{
    public string? Name { get; set; }
}
