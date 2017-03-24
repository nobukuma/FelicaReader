using System;

namespace Plugin.FelicaReader.Abstractions
{
    public class FelicaReaderNotEnabledException : Exception
    {
        public FelicaReaderNotEnabledException(string msg)
         : base(msg)
        {
        }
    }
}
