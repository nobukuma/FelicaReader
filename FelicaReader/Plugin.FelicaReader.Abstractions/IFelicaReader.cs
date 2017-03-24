using System;
using System.Threading;
using System.Threading.Tasks;

namespace Plugin.FelicaReader.Abstractions
{
    /// <summary>
    /// Interface for FelicaReader
    /// </summary>
    public interface IFelicaReader
    {
        void EnableForeground();

        void DisableForeground();

        void FindCard(int timeoutInSecs = 10, CancellationTokenSource tokenSource = null);

        bool IsSupported { get; }

        bool IsEnabled { get; }

        IObservable<IFelicaCardMedia> WhenCardFound();

    }
}