using System;

namespace Plugin.FelicaReader.Abstractions
{
    public class UnknownCardException : Exception
    {
        public UnknownCardException(string msg)
         : base(msg)
        {
        }
    }
}
