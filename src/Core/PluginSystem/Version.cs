using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using LLVersion = LiteLoader.InterfaceAPI.Interop.Version;

namespace LiteLoader.NET.PluginSystem;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct Version
{
    [Flags]
    public enum Status
    {
        Dev,
        Beta,
        Release
    };

    public int major;
    public int minor;
    public int revision;
    public Status status;

    public Version(int major = 0, int minor = 0, int revision = 0, Status status = Status.Release)
    {
        fixed (Version* ptr = &this)
        {
            LLVersion.ctor(ptr, major, minor, revision, (int)status);
        }
    }

    public static bool operator >(Version l, Version r)
    {
        return LLVersion.operator_geraterThan(&l, &r);
    }

    public static bool operator <(Version l, Version r)
    {
        return LLVersion.operator_lessThan(&l, &r);
    }

    public static bool operator >=(Version l, Version r)
    {
        return LLVersion.operator_geraterThanOrEqual(&l, &r);
    }

    public static bool operator <=(Version l, Version r)
    {
        return LLVersion.operator_lessThanOrEqual(&l, &r);
    }

    public static bool operator ==(Version l, Version r)
    {
        return LLVersion.operator_equality(&l, &r);
    }

    public static bool operator !=(Version l, Version r)
    {
        return !(l == r);
    }

    public override bool Equals(object? obj)
    {
        if (obj is Version version)
            return Equals(version);
        return false;
    }

    public override int GetHashCode()
    {
        return major ^ minor ^ revision ^ (int)status;
    }

    public static implicit operator System.Version(Version version)
    {
        return new System.Version(version.major, version.minor, (int)version.status, version.revision);
    }
}
