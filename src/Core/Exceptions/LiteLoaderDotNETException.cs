using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteLoader.NET.Exceptions;

public class LiteLoaderDotNETException : ApplicationException
{
    public LiteLoaderDotNETException(string message)
        : base(message)
    {
    }

    public LiteLoaderDotNETException()
    {
    }
}
