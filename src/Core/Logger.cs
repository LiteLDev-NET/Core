using static LiteLoader.InterfaceAPI.Interop.LoggerManager;
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

        public void WriteLine(string str)
        {
            LoggerManager.WriteLine(loggerId, type, str);
        }

        public void WriteLine(object obj)
        {
            WriteLine(obj.ToString() ?? "null");
        }

        public void WriteLine(string format, params object?[] args)
        {
            //WriteLine(string.Format(format, args));
            WriteLine(format);
        }
    }

    private readonly ulong id;

    public Logger(string title)
    {
        if (!CreateLogger(ref id, title))
        {
            throw new ArgumentException();
        }

        Debug = new(OutputStreamType.debug, id);
        Info = new(OutputStreamType.info, id);
        Warn = new(OutputStreamType.warn, id);
        Error = new(OutputStreamType.error, id);
        Fatal = new(OutputStreamType.fatal, id);
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
            SetTitle(id, value);
        }
    }

    public OutputStream Debug { get; private set; }

    public OutputStream Info { get; private set; }

    public OutputStream Warn { get; private set; }

    public OutputStream Error { get; private set; }

    public OutputStream Fatal { get; private set; }
}
