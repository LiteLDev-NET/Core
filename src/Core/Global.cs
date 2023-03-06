using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteLoader.NET
{
    internal static class Global
    {
        internal static readonly Logger logger = new("LiteLoader.NET");

        internal const string PluginsDir = "plugins";

        internal static string DotnetRuntimeDir = string.Empty;
    }
}
