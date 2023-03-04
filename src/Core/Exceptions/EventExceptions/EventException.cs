using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteLoader.NET.Exceptions.EventExceptions;

public class EventException : LiteLoaderDotNETException
{
    public EventException()
    {
    }

    public EventException(string message)
        : base(message)
    {
    }
}
