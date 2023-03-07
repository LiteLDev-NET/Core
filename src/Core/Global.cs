using LiteLoader.NET.PluginSystem;
using System.Collections.Concurrent;

namespace LiteLoader.NET
{
    internal static class Global
    {
        internal static readonly Logger logger = new("LiteLoader.NET");

        internal const string PluginsDir = "plugins";

        internal static string DotnetRuntimeDir = string.Empty;

        internal static Dictionary<nint, PluginAssemblyLoadContext> PluginLoadContexts = new();
    }
}
