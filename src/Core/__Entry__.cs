#pragma warning disable IDE1006
using static LiteLoader.I18N.I18N;
using LiteLoader.NET.PluginSystem;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace LiteLoader.NET
{
    public unsafe static class __Entry__
    {
        public static void InitAndLoadPlugins(string dotnetRuntimeDir)
        {
            Global.DotnetRuntimeDir = dotnetRuntimeDir;


            ulong count = 0;
            Global.logger.Info.WriteLine(Tr("llnet.loader.loadMain.loadingPlugins"));
            foreach (var fileInfo in new DirectoryInfo(Global.PluginsDir).EnumerateFiles())
            {
                if (fileInfo is not null)
                {
                    using var file = fileInfo.OpenRead();
                    PEReader reader = new(file);

                    if (reader.HasMetadata)
                    {
                        try
                        {
                            var pluginLoadContext = new PluginAssemblyLoadContext(fileInfo.FullName, true);
                            Global.PluginLoadContexts.Add(pluginLoadContext.PluginHandle, pluginLoadContext);

                            pluginLoadContext.LoadPlugin();

                            ++count;
                            Global.logger.Info.WriteLine(Tr("llnet.loader.loadMain.success"), pluginLoadContext.PluginName);
                        }
                        catch (Exception ex)
                        {
                            Global.logger.Error.WriteLine(Tr("llnet.loader.loadMain.error"), ex.GetType().FullName);
                            Global.logger.Error.WriteLine(Environment.NewLine);
                            Global.logger.Error.WriteLine(ex.ToString());
                            continue;
                        }

                    }
                }
            }
            Global.logger.Info.WriteLine(Tr("llnet.loader.loadMain.done"), count);
        }
    }
}
#pragma warning restore IDE1006
