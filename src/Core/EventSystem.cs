using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using LiteLoader.NET.Exceptions.EventExceptions;
using LiteLoader.NET.PluginSystem;

namespace LiteLoader.NET.Event;

public interface IEventListener
{
}

[Flags]
public enum EventPriority
{
    LOWEST,
    LOW,
    NORMAL,
    HIGH,
    HIGHEST,
    MONITOR
}

[AttributeUsage(AttributeTargets.Method)]
public class EventHandlerAttribute : System.Attribute
{
    public bool IgnoreCancelled
    {
        [return: MarshalAs(UnmanagedType.U1)]
        get;
        [param: MarshalAs(UnmanagedType.U1)]
        set;
    }

    public EventPriority Priority { get; set; }

    public EventHandlerAttribute()
    {
        Priority = EventPriority.NORMAL;
        IgnoreCancelled = false;
    }
}

public interface IEvent
{
    bool IsCancelled
    {
        [return: MarshalAs(UnmanagedType.U1)]
        get;
    }

    void Call()
    {
        EventManager.CallEvent(this);
    }
}


public interface ICancellable
{
    bool IsCancelled
    {
        [param: MarshalAs(UnmanagedType.U1)]
        set;
    }
}

public abstract class EventBase : IEvent
{
    private bool isCancelled;

    public virtual bool IsCancelled
    {
        [return: MarshalAs(UnmanagedType.U1)]
        get
        {
            return isCancelled;
        }
        [param: MarshalAs(UnmanagedType.U1)]
        set
        {
            if (!(this is ICancellable))
            {
                throw new CancelEventException();
            }
            isCancelled = value;
        }
    }

    public void Call()
    {
        EventManager.CallEvent(this);
    }

    public EventBase()
    {
    }
}

#pragma warning disable CS8600
#pragma warning disable CS8500
public static class EventManager
{
    private const int IS_NORMAL = 0;
    private const int IS_INSTANCE = 64;
    private const int IS_REF = 128;
    private const int IS_IGNORECANCELLED = 256;
    private const int IS_INSTANCE_AND_REF = IS_INSTANCE | IS_REF;
    private const int IS_INSTANCE_AND_IGNORECANCELLED = IS_INSTANCE | IS_IGNORECANCELLED;
    private const int IS_REF_AND_IGNORECANCELLED = IS_REF | IS_IGNORECANCELLED;
    private const int IS_INSTANCE_AND_REF_AND_IGNORECANCELLED = IS_INSTANCE | IS_REF | IS_IGNORECANCELLED;


    internal static readonly Dictionary<ulong, List<(IntPtr, bool, bool, bool, Type, IntPtr)>[]> eventManagerData = new();

    internal static readonly Dictionary<Type, ulong> eventIds = new();

    internal static readonly Random rand = new();

    internal static readonly List<ulong> initializedNativeEvents = new List<ulong>();

    internal static Logger logger = new("LiteLoader.NET");

    public unsafe static void RegisterListener<TListener>() where TListener : IEventListener
    {
        IntPtr handle = new IntPtr(PluginOwnData.GetCurrentModule(Assembly.GetCallingAssembly()));
        RegisterListener<TListener>(handle);
    }


    public static void RegisterListener<TListener>(IntPtr handle) where TListener : IEventListener
    {
        Type typeFromHandle = typeof(TListener);
        MethodInfo[] methods = typeFromHandle.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        List<(MethodInfo, EventPriority, bool, bool, bool)> list = new List<(MethodInfo, EventPriority, bool, bool, bool)>();
        int num = 0;
        if (0 < (nint)methods.LongLength)
        {
            do
            {
                MethodInfo methodInfo = methods[num];
                object[] customAttributes = methodInfo.GetCustomAttributes(typeof(EventHandlerAttribute), inherit: false);
                if (customAttributes.Length != 0)
                {
                    if (!methodInfo.IsStatic && typeFromHandle.GetConstructor(Array.Empty<Type>()) == null)
                    {
                        string text = ">";
                        string name = methodInfo.Name;
                        string text2 = ".";
                        string name2 = typeFromHandle.Name;
                        throw new RegisterEventListenerException(string.Concat(string.Concat(string.Concat("Handler must be static or it's class must have default constructor!  at Handler:<" + name2, text2), name), text));
                    }
                    EventHandlerAttribute eventHandlerAttribute = (EventHandlerAttribute)customAttributes[0];
                    EventPriority priority = eventHandlerAttribute.Priority;
                    byte ignoreCancelled = (eventHandlerAttribute.IgnoreCancelled ? ((byte)1) : ((byte)0));
                    byte item = ((!methodInfo.IsStatic) ? ((byte)1) : ((byte)0));
                    (MethodInfo, EventPriority, bool, bool, bool) item2 = (methodInfo, priority, ignoreCancelled != 0, item != 0, false);
                    list.Add(item2);
                }
                num++;
            }
            while (num < (nint)methods.LongLength);
        }
        List<(MethodInfo, EventPriority, bool, bool, bool)>.Enumerator enumerator = list.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            return;
        }
        MethodInfo methodInfo2;
        while (true)
        {
            (MethodInfo, EventPriority, bool, bool, bool) current = enumerator.Current;
            (methodInfo2, _, _, _, _) = current;
            if (methodInfo2.ReturnType != typeof(void))
            {
                break;
            }
            ParameterInfo[] parameters = methodInfo2.GetParameters();
            bool flag;
            Type type;
            if ((nint)parameters.LongLength == 1)
            {
                flag = false;
                Type parameterType = parameters[0].ParameterType;
                type = parameterType;
                if (parameterType.IsByRef)
                {
                    flag = true;
                    type = parameterType.GetElementType()!;
                }
                Type[] interfaces = type.GetInterfaces();
                int num2 = 0;
                while (num2 < (nint)interfaces.LongLength)
                {
                    if (!(interfaces[num2] == typeof(IEvent)))
                    {
                        num2++;
                        continue;
                    }
                    goto IL_019d;
                }
            }
            string text3 = ">";
            string name3 = methodInfo2.Name;
            string text4 = ".";
            string name4 = typeFromHandle.Name;
            throw new RegisterEventListenerException(string.Concat(string.Concat(string.Concat("Handler can only have one parameter which the type is based on IEvent!  at Handler:<" + name4, text4), name3), text3));
        IL_019d:
            if (!eventIds.ContainsKey(type))
            {
                _registerEvent(type);
            }
            ulong num3 = eventIds[type];
            bool flag2 = (byte)((num3 - 1 <= 127) ? 1u : 0u) != 0;
            if (flag)
            {
                current.Item5 = true;
            }
            int item3 = (int)current.Item2;
            List<(IntPtr, bool, bool, bool, Type, IntPtr)>[] array = eventManagerData[num3];
            if (array[item3] == null)
            {
                List<(IntPtr, bool, bool, bool, Type, IntPtr)> list2 = (array[item3] = new List<(IntPtr, bool, bool, bool, Type, IntPtr)>());
            }
            List<(IntPtr, bool, bool, bool, Type, IntPtr)> obj = array[item3];
            IntPtr functionPointer = methodInfo2.MethodHandle.GetFunctionPointer();
            bool item4 = current.Item4;

            Type item5 = ((!item4) ? null : typeFromHandle);
            (IntPtr, bool, bool, bool, Type, IntPtr) item6 = (functionPointer, current.Item3, flag, item4, item5!, handle);
            obj.Add(item6);
            if (flag2)
            {
                if (!initializedNativeEvents.Contains(num3))
                {
                    MethodInfo? method = type.GetMethod("_init", BindingFlags.Static | BindingFlags.NonPublic);
                    Type[] parameters2 = Array.Empty<Type>();
                    method!.Invoke(null, parameters2);
                }
                initializedNativeEvents.Add(num3);
            }
            if (!enumerator.MoveNext())
            {
                return;
            }
        }
        string text5 = ">";
        string name5 = methodInfo2.Name;
        string text6 = ".";
        string name6 = typeFromHandle.Name;
        throw new RegisterEventListenerException(string.Concat(string.Concat(string.Concat("Handler.ReturnType must be System.Void!  at Handler:<" + name6, text6), name5), text5));
    }

    public unsafe static void CallEvent(IEvent ev)
    {
        //The blocks IL_0019, IL_002b, IL_0038, IL_005d are reachable both inside and outside the pinned region starting at IL_0055. ILSpy has duplicated these blocks in order to place them both within and outside the `fixed` statement.
        Type type = ev.GetType();
        if (!(type != typeof(EventBase)))
        {
            return;
        }
        while (type != typeof(object))
        {
            if (eventIds.TryGetValue(type!, out ulong value))
            {
                ulong key = value;
                while (true)
                {
                    fixed (List<(IntPtr, bool, bool, bool, Type, IntPtr)>* pfuncs = &eventManagerData[key][0])
                    {
                        _callEvent(ev, pfuncs);
                        do
                        {
                            type = type!.BaseType;
                            if (!(type != typeof(EventBase)) || !(type != typeof(object)))
                            {
                                return;
                            }
                        }
                        while (!eventIds.ContainsKey(type!));
                        key = eventIds[type!];
                    }
                }
            }
            type = type!.BaseType;
            if (!(type != typeof(EventBase)))
            {
                break;
            }
        }
    }

    private unsafe static void _registerEvent(Type eventType)
    {
        ulong num2;
        do
        {
            long num = (long)rand.Next() & 7L;
            num2 = (ulong)(rand.Next() ^ num);
        }
        while (eventIds.ContainsValue(num2) || (byte)((num2 - 1 <= 127) ? 1u : 0u) != 0);
        eventIds.TryAdd(eventType, num2);
        var value = new List<(IntPtr, bool, bool, bool, Type, IntPtr)>[6];
        ulong key = eventIds[eventType];
        eventManagerData.TryAdd(key, value);
        AssemblyOwnData__.AddRegisteredEvent(AssemblyOwnData__.GetCurrentModule(eventType.Assembly), eventType, num2);
    }

    private unsafe static void _callEvent<TEvent>(TEvent ev, List<(IntPtr, bool, bool, bool, Type, IntPtr)>* pfuncs) where TEvent : IEvent
    {
        for (int i = 0; i < 6; i++)
        {
            List<(IntPtr, bool, bool, bool, Type, IntPtr)> list = *(List<(IntPtr, bool, bool, bool, Type, IntPtr)>*)((long)i * 8L + (nint)pfuncs);
            if (list == null)
            {
                continue;
            }
            List<(IntPtr, bool, bool, bool, Type, IntPtr)>.Enumerator enumerator = list.GetEnumerator();
            while (enumerator.MoveNext())
            {
                (IntPtr, bool, bool, bool, Type, IntPtr) current = enumerator.Current;
                int num = (current.Item2 ? 256 : 0);
                int num2 = (current.Item3 ? 128 : 0);
                int num3 = (current.Item4 ? 64 : 0) | num2 | num;
                try
                {
                    switch (num3)
                    {
                        case IS_INSTANCE_AND_REF:
                            if (!ev.IsCancelled)
                            {
                                ((delegate*<object, ref TEvent, void>)(void*)current.Item1)(Activator.CreateInstance(current.Item5)!, ref ev);
                            }
                            break;
                        case IS_REF:
                            if (!ev.IsCancelled)
                            {
                                ((delegate*<ref TEvent, void>)(void*)current.Item1)(ref ev);
                            }
                            break;
                        case IS_INSTANCE:
                            if (!ev.IsCancelled)
                            {
                                void* ptr4 = (void*)current.Item1;
                                TEvent val5 = ev;
                                TEvent val6 = ev;
                                ((delegate*<object, TEvent, void>)ptr4)(Activator.CreateInstance(current.Item5)!, val6);
                            }
                            break;
                        case IS_NORMAL:
                            if (!ev.IsCancelled)
                            {
                                void* ptr3 = (void*)current.Item1;
                                TEvent val4 = ev;
                                ((delegate*<TEvent, void>)ptr3)(ev);
                            }
                            break;
                        case IS_IGNORECANCELLED:
                            {
                                void* ptr2 = (void*)current.Item1;
                                TEvent val3 = ev;
                                ((delegate*<TEvent, void>)ptr2)(ev);
                                break;
                            }
                        case IS_INSTANCE_AND_REF_AND_IGNORECANCELLED:
                            ((delegate*<object, ref TEvent, void>)(void*)current.Item1)(Activator.CreateInstance(current.Item5)!, ref ev);
                            break;
                        case IS_REF_AND_IGNORECANCELLED:
                            ((delegate*<ref TEvent, void>)(void*)current.Item1)(ref ev);
                            break;
                        case IS_INSTANCE_AND_IGNORECANCELLED:
                            {
                                void* ptr = (void*)current.Item1;
                                TEvent val = ev;
                                TEvent val2 = ev;
                                ((delegate*<object, TEvent, void>)ptr)(Activator.CreateInstance(current.Item5)!, val2);
                                break;
                            }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error.WriteLine($"Exception thrown when handling an event:{Environment.NewLine}{ex}");
                }
            }
        }
    }

    [return: MarshalAs(UnmanagedType.U1)]
    private static bool _isNativeEventId(ulong eventId)
    {
        return (byte)((eventId - 1 <= 127) ? 1u : 0u) != 0;
    }

    internal unsafe static void _callNativeEvent<TEvent>(TEvent ev, ulong eventId) where TEvent : IEvent
    {
        fixed (List<(IntPtr, bool, bool, bool, Type, IntPtr)>* pfuncs = &eventManagerData[eventId][0])
        {
            _callEvent(ev, pfuncs);
        }
    }

    internal static void _registerNativeEvent<TEvent>(ulong id) where TEvent : IEvent
    {
        Type typeFromHandle = typeof(TEvent);
        eventIds.Add(typeFromHandle, id);
        List<(IntPtr, bool, bool, bool, Type, IntPtr)>[] value = new List<(IntPtr, bool, bool, bool, Type, IntPtr)>[6];
        eventManagerData.Add(id, value);
    }
}
#pragma warning restore CS8600
#pragma warning restore CS8500
