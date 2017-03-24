using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.FelicaReader.Abstractions
{
    public class FelicaReaderTimeoutException : Exception
    {
        public FelicaReaderTimeoutException()
            : base()
        {

        }

        public FelicaReaderTimeoutException(string msg)
            : base(msg)
        {

        }
    }
}
