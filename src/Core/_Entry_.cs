using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable IDE1006
namespace LiteLoader.NET
{
    public unsafe static class __Entry__
    {
        public static readonly Logger logger = new("LiteLoader.NET");

        public const string PluginsDir = "plugins";

        public static void InitAndLoadPlugins(char* dotnetRuntimeDir)
        {

        }
    }
}
#pragma warning restore IDE1006
