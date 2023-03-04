﻿using static LiteLoader.InterfaceAPI.Interop.LoggerManager;
using System.Runtime.InteropServices;
using LiteLoader.InterfaceAPI.Interop;

namespace LiteLoader;

public unsafe class Logger
{
    public class OutputStream
    {
        private readonly ulong loggerId;
        private readonly OutputStreamType type;

        public OutputStream(OutputStreamType type, ulong loggerId)
        {
            this.type = type;
            this.loggerId = loggerId;
        }

        void WriteLine(string str)
        {
            fixed (char* ptr = &str.AsSpan()[0])
            {
                LoggerManager.WriteLine(loggerId, type, ptr);
            }
        }

        void WriteLine(object obj)
        {
            WriteLine(obj.ToString() ?? "null");
        }

        void WriteLine(string format, params object[] args)
        {
            WriteLine(string.Format(format, args));
        }
    }

    private readonly ulong id;

    public Logger(string title)
    {

        fixed (char* ptr = &title.AsSpan()[0])
        {
            if (!CreateLogger(ref id, ptr))
            {
                throw new ArgumentException();
            }
        }
    }

    ~Logger()
    {
        DeleteLogger(id);
    }

    public string Title
    {
        get
        {
            var buffer = new char[64];

            fixed (char* ptr = &buffer[0])
            {
                if (!GetTitle(id, ptr, 64))
                {
                    throw new InvalidOperationException();
                }

                return new string(ptr);
            }
        }
        set
        {
            fixed (char* ptr = &value.AsSpan()[0])
            {
                SetTitle(id, ptr);
            }
        }
    }
}