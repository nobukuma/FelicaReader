using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.FelicaReader.Abstractions
{
    /// <summary>
    /// Interface for FelicaCardMedia
    /// </summary>
    public interface IFelicaCardMedia : IDisposable
    {
        Task<byte[]> GetIdm();

        Task<byte[]> Send(byte[] data);
    }
}
