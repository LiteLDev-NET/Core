using LiteLoader.InterfaceAPI.Interop;
using LiteLoader.NET.Internal;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using static LiteLoader.I18N.I18N;

namespace LiteLoader.NET.PluginSystem;

class PluginAssemblyLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver resolver;

    internal readonly Assembly pluginAssembly;

    private readonly PluginOwnData data;

    public PluginAssemblyLoadContext(string pluginPath)
    {
        resolver = new AssemblyDependencyResolver(pluginPath);
        pluginAssembly = LoadFromAssemblyPath(pluginPath);

        data = new(pluginAssembly);

        var attrArr = pluginAssembly.GetCustomAttributes<CustomLibPathAttribute>();
        foreach (var attr in attrArr)
        {
            if (attr is not null)
                data.CustomLibPath.Add(attr.Path);
        }
    }

#nullable enable
    protected override Assembly? Load(AssemblyName assemblyName)
    {
        string? assemblyPath = resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
        {
            return LoadFromAssemblyPath(assemblyPath);
        }

        var (result, path) = ResolveAssemblyPath(assemblyName);

        switch (result)
        {
            case ResolveAssemblyPathResults.Succeeded:
                return LoadFromAssemblyPath(path);

            case ResolveAssemblyPathResults.IncompatibleVersion:
                Global.logger.Warn.WriteLine(Tr("llnet.loader.loadAssembly.versionNotMatch"), assemblyName.Name, assemblyName.Version?.ToString());
                return LoadFromAssemblyPath(path);

            case ResolveAssemblyPathResults.Failed:
                Global.logger.Error.WriteLine(Tr("llnet.loader.loadAssembly.failed"));
                return null;
        }

        return null;
    }


    private enum ResolveAssemblyPathResults
    {
        Succeeded,
        IncompatibleVersion,
        Failed
    }

    private (ResolveAssemblyPathResults result, string path) ResolveAssemblyPath(AssemblyName assemblyName)
    {
        foreach (var customPath in data.CustomLibPath)
        {
            var path = Path.Combine(customPath, assemblyName.Name + ".dll");
            if (File.Exists(path))
            {
                //var metadataReader = new PEReader(File.OpenRead(path)).GetMetadataReader(MetadataReaderOptions.Default);
                if (MetadataReader.GetAssemblyName(path).Version != assemblyName.Version)
                    return (ResolveAssemblyPathResults.IncompatibleVersion, path);
                else
                    return (ResolveAssemblyPathResults.Succeeded, path);
            }
        }

        var dotnetLibPath = Path.Combine(Global.DotnetRuntimeDir, assemblyName.Name + ".dll");
        if (File.Exists(dotnetLibPath))
        {
            if (MetadataReader.GetAssemblyName(dotnetLibPath).Version != assemblyName.Version)
                return (ResolveAssemblyPathResults.IncompatibleVersion, dotnetLibPath);
            else
                return (ResolveAssemblyPathResults.Succeeded, dotnetLibPath);
        }

        return (ResolveAssemblyPathResults.Failed, string.Empty);
    }

    private unsafe void RegisterCurrentPlugin(IPluginInitializer initializer, string pluginName)
    {
        var handle = HandleHelper.GetModuleHandle(pluginAssembly);
        var version = initializer.Version;

        var metaData = initializer.MetaData;
        RegisterPluginArgs args = default;
        GCHandle gcHandle = default;

        if (metaData != null)
        {
            var arr = new RegisterPluginArgs.Pair[metaData.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = new() { key = metaData[i].Item1, value = metaData[i].Item2 };
            }

#pragma warning disable CS8500
            gcHandle = GCHandle.Alloc(arr);
            fixed (RegisterPluginArgs.Pair* ptr = &arr[0])
            {
                args.array = ptr;
                args.length = (ulong)arr.Length;
            }
#pragma warning restore CS8500
        }


        LLAPI.RegisterPlugin(handle, pluginName, initializer.Introduction, &version, ref args);

        if (gcHandle.IsAllocated)
            gcHandle.Free();
    }

    internal unsafe bool LoadPlugin()
    {
        var pluginRegistered = false;

        foreach (var type in pluginAssembly.GetTypes())
        {
            if (type.IsAssignableTo(typeof(IPluginInitializer)))
            {
                var attr = type.GetCustomAttribute<PluginMainAttribute>();
                if (attr == null)
                {
                    Global.logger.Warn.WriteLine(Tr("llnet.loader.loadPlugin.initializer.missingAttr"), pluginAssembly.FullName, type.FullName);
                    continue;
                }
                var pluginName = attr.Name ?? pluginAssembly.GetName().Name ?? "NullPluginName";


                if (Activator.CreateInstance(type) is not IPluginInitializer initializer)
                {
                    Global.logger.Warn.WriteLine(Tr("llnet.loader.loadPlugin.initializer.constructionFailed"), pluginName, type.FullName);
                    continue;
                }



                if (!pluginRegistered)
                {
                    RegisterCurrentPlugin(initializer, pluginName);
                }


                initializer.OnInitialize();

                Global.logger.Info.WriteLine(Tr("llnet.loader.loadPlugin.initializer.succeeded"), pluginName, type.FullName);

                pluginRegistered = true;
            }
        }

        if (pluginRegistered)
        {
            Global.logger.Info.WriteLine(Tr("llnet.loader.loadPlugin.succeeded"));
            return true;
        }
        else
        {
            Global.logger.Info.WriteLine(Tr("llnet.loader.loadPlugin.failed"));
            return false;
        }
    }
}
#nullable disable
