using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace LiteLoader.NET.PluginSystem;

class PluginAssemblyLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver resolver;

    public PluginAssemblyLoadContext(string pluginPath)
    {
        resolver = new AssemblyDependencyResolver(pluginPath);
    }

#nullable enable
    protected override Assembly? Load(AssemblyName assemblyName)
    {
        string? assemblyPath = resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
        {
            return LoadFromAssemblyPath(assemblyPath);
        }

        return null;
    }
}
#nullable disable
