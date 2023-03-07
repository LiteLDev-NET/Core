#pragma warning disable IDE1006
using static LiteLoader.I18N.I18N;
using LiteLoader.NET.PluginSystem;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace LiteLoader.NET
{
    public unsafe static class __Entry__
    {
        public delegate void EntryPropotype(char* dotnetRuntimeDirStrPtr);

        public static void InitAndLoadPlugins(char* dotnetRuntimeDirStrPtr)
        {
            try
            {
                var dotnetRuntimeDir = new string(dotnetRuntimeDirStrPtr);
                bool sharedLibNotFound = true;


                if (!string.IsNullOrEmpty(dotnetRuntimeDir))
                {
                    var runtimeLibPathDir = Path.Combine(dotnetRuntimeDir, "shared", "Microsoft.NETCore.App");
                    if (Directory.Exists(runtimeLibPathDir))
                    {
                        var dirs = Directory.GetDirectories(runtimeLibPathDir);

                        foreach (var dir in dirs)
                        {
                            var path = dir.Replace("\\", "/");
                            var arr = path.Split('/');

                            if (arr.Length > 0 && arr[^1].StartsWith("7.0."))
                            {
                                if (File.Exists(Path.Combine(path, "coreclr.dll")))
                                {
                                    Global.DotnetRuntimeSharedLibDir = path;
                                    sharedLibNotFound = false;
                                }
                            }
                        }
                    }
                }


                if (sharedLibNotFound)
                {
                    Global.logger.Warn.WriteLine(Tr("llnet.loader.loadMain.sharedLibNotFound"));
                }

                Global.DotnetRuntimeDir = dotnetRuntimeDir;


                ulong count = 0;
                Global.logger.Info.WriteLine(Tr("llnet.loader.loadMain.loadingPlugins"));
                foreach (var fileInfo in new DirectoryInfo(Global.PluginsDir).EnumerateFiles())
                {
                    if (fileInfo is not null)
                    {
                        if (fileInfo.Extension != ".dll")
                            continue;

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

            catch (Exception ex)
            {
                Global.logger.Error.WriteLine(ex);
            }
        }
    }
}
#pragma warning restore IDE1006
