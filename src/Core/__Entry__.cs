#pragma warning disable IDE1006
namespace LiteLoader.NET
{
    public unsafe static class __Entry__
    {
        public static void InitAndLoadPlugins(string dotnetRuntimeDir)
        {
            Global.DotnetRuntimeDir = dotnetRuntimeDir;
        }
    }
}
#pragma warning restore IDE1006
