using LiteLoader.NET.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace LiteLoader.NET.PluginSystem;

public sealed class PluginOwnData
{
    private static readonly Dictionary<nint, PluginOwnData> Data = new();

    internal static PluginOwnData? GetPluginOwnData(nint handle)
    {
        if (Data.TryGetValue(handle, out var pluginOwnData)) return pluginOwnData;
        else return null;
    }

    public List<string> CustomLibPath { get; private set; } = new();

    public List<(ulong, Type)> RegisteredEvent { get; private set; } = new();

    internal PluginOwnData(Assembly assembly)
    {
        var handle = HandleHelper.GetModuleHandle(assembly.Location);
        Data.Add(handle, this);
    }
}



internal static class AssemblyOwnData__
{
    static AssemblyOwnData__()
    {
        var asm = Assembly.GetExecutingAssembly();
        ManagedAssemblyHandle.Add(asm, HandleHelper.GetModuleHandle(asm.Location));
    }

    public static Dictionary<Assembly, nint> ManagedAssemblyHandle = new();

    public static Dictionary<Assembly, List<string>> CustomLibPath = new();

    public static nint GetCurrentModule(Assembly assembly)
    {
        if (ManagedAssemblyHandle.TryGetValue(assembly, out nint result))
        {
            return result;
        }
        else
        {
            var handle = HandleHelper.GetModuleHandle(assembly.Location);
            ManagedAssemblyHandle.Add(assembly, handle);
            return handle;
        }
    }

    //public static Dictionary<IntPtr, List<(Delegate, IntPtr, IntPtr, IntPtr)>> HookedFunction = new Dictionary<IntPtr, List<(Delegate, IntPtr, IntPtr, IntPtr)>>();

    public static Dictionary<IntPtr, List<(Type, ulong)>> RegisteredEvent = new Dictionary<IntPtr, List<(Type, ulong)>>();

    //public static Dictionary<IntPtr, List<INativeEventListener>> SubscribedNativeEvent = new Dictionary<IntPtr, List<INativeEventListener>>();

    //public static Dictionary<IntPtr, List<string>> RegisteredCommand = new Dictionary<IntPtr, List<string>>();

    //public static Dictionary<IntPtr, List<(LiteLoader.Schedule.ScheduleTask, GCHandle)>> RegisteredSchedule = new Dictionary<IntPtr, List<(LiteLoader.Schedule.ScheduleTask, GCHandle)>>();

    //public static Dictionary<IntPtr, List<ExportedFuncInstance>> ExportedRemoteCallFunctions = new Dictionary<IntPtr, List<ExportedFuncInstance>>();

    //public static Dictionary<IntPtr, List<ImportedFuncInstance>> ImportedRemoteCallFunctions = new Dictionary<IntPtr, List<ImportedFuncInstance>>();

    //public static Dictionary<ulong, (IntPtr, IFunctionCaller)> RemoteCallData = new Dictionary<ulong, (IntPtr, IFunctionCaller)>();

    //public static void AddHookedFunction(IntPtr handle, Delegate hook, IntPtr address, IntPtr pHook, IntPtr pOriginal)
    //{
    //    List<(Delegate, IntPtr, IntPtr, IntPtr)> list;
    //    if (HookedFunction.ContainsKey(handle))
    //    {
    //        list = HookedFunction[handle];
    //    }
    //    else
    //    {
    //        list = new List<(Delegate, IntPtr, IntPtr, IntPtr)>();
    //        HookedFunction.Add(handle, list);
    //    }
    //    (Delegate, IntPtr, IntPtr, IntPtr) item = (hook, address, pHook, pOriginal);
    //    list.Add(item);
    //}

    public static void AddRegisteredEvent(IntPtr handle, Type type, ulong id)
    {
        List<(Type, ulong)> list;
        if (!RegisteredEvent.ContainsKey(handle))
        {
            list = new List<(Type, ulong)>();
            RegisteredEvent.Add(handle, list);
        }
        else
        {
            list = RegisteredEvent[handle];
        }
        (Type, ulong) item = (type, id);
        list.Add(item);
    }

    //public static void AddSubscribedNativeEvent(IntPtr handle, INativeEventListener listener)
    //{
    //    List<INativeEventListener> list;
    //    if (!SubscribedNativeEvent.ContainsKey(handle))
    //    {
    //        list = new List<INativeEventListener>();
    //        SubscribedNativeEvent.Add(handle, list);
    //    }
    //    else
    //    {
    //        list = SubscribedNativeEvent[handle];
    //    }
    //    list.Add(listener);
    //}

    //public static void AddRegisteredCommand(IntPtr handle, string name)
    //{
    //    List<string> list;
    //    if (RegisteredCommand.ContainsKey(handle))
    //    {
    //        list = RegisteredCommand[handle];
    //    }
    //    else
    //    {
    //        list = new List<string>();
    //        RegisteredCommand.Add(handle, list);
    //    }
    //    list.Add(name);
    //}

    //public static void AddRegisteredSchedule(IntPtr handle, LiteLoader.Schedule.ScheduleTask task, GCHandle gch)
    //{
    //    List<(LiteLoader.Schedule.ScheduleTask, GCHandle)> list;
    //    if (RegisteredSchedule.ContainsKey(handle))
    //    {
    //        list = RegisteredSchedule[handle];
    //    }
    //    else
    //    {
    //        list = new List<(LiteLoader.Schedule.ScheduleTask, GCHandle)>();
    //        RegisteredSchedule.Add(handle, list);
    //    }
    //    (LiteLoader.Schedule.ScheduleTask, GCHandle) item = (task, gch);
    //    list.Add(item);
    //}

    //public static void AddRemoteCallData(ulong hashVal, IntPtr handle, IFunctionCaller funcHandle)
    //{
    //    (IntPtr, IFunctionCaller) value = (handle, funcHandle);
    //    RemoteCallData.TryAdd(hashVal, value);
    //}

    //public static void AddExportedRemoteCallFunctions(IntPtr handle, ExportedFuncInstance instance)
    //{
    //    List<ExportedFuncInstance> list;
    //    if (ExportedRemoteCallFunctions.ContainsKey(handle))
    //    {
    //        list = ExportedRemoteCallFunctions[handle];
    //    }
    //    else
    //    {
    //        list = new List<ExportedFuncInstance>();
    //        ExportedRemoteCallFunctions.Add(handle, list);
    //    }
    //    list.Add(instance);
    //}

    //public static void AddImportedRemoteCallFunctions(IntPtr handle, ImportedFuncInstance instance)
    //{
    //    List<ImportedFuncInstance> list;
    //    if (ImportedRemoteCallFunctions.ContainsKey(handle))
    //    {
    //        list = ImportedRemoteCallFunctions[handle];
    //    }
    //    else
    //    {
    //        list = new List<ImportedFuncInstance>();
    //        ImportedRemoteCallFunctions.Add(handle, list);
    //    }
    //    list.Add(instance);
    //}
}
