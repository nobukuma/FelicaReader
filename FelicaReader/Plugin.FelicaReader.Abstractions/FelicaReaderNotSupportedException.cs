using System;

namespace Plugin.FelicaReader.Abstractions
{
    public class FelicaReaderNotSupportedException : Exception
    {
        public FelicaReaderNotSupportedException(string msg)
         : base(msg)
        {
        }
    }
}
