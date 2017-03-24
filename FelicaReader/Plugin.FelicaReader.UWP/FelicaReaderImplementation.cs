using Plugin.FelicaReader.Abstractions;
using Pcsc;
using Windows.Devices.SmartCards;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Pcsc.Common;

namespace Plugin.FelicaReader
{
    public class FelicaReaderImplementation : IFelicaReader
    {
        private SmartCardReader reader;

        private bool isEnabled;
        public bool IsEnabled
        {
            get
            {
                return isEnabled;
            }
            private set
            {
                isEnabled = value;
            }
        }

        private bool isSupported;
        public bool IsSupported
        {
            get
            {
                return isSupported;
            }
            private set
            {
                isSupported = value;
            }
        }

        private Subject<IFelicaCardMedia> felicaCardSubject;

        public FelicaReaderImplementation()
        {
            this.IsSupported = false;
            this.IsEnabled = false;

            this.felicaCardSubject = new Subject<IFelicaCardMedia>();
        }

        private async Task<SmartCard> FindSmartCard(SmartCardReader cardReader, CancellationToken cts)
        {
            while (true)
            {
                if (cts.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }

                IReadOnlyList<SmartCard> cards = await cardReader.FindAllCardsAsync();
                if (cards != null && cards.Count > 0)
                {
                    var card = cards[0];
                    return card;
                }
            }
        }

        private const int ReadLoopCountMax = 3;

        public void FindCard(int timeoutInSecs = 10,
            CancellationTokenSource tokenSource = null)
        {
            Task findCardTask = Task.Factory.StartNew(async () =>
            {
                // SmartCardReaderÇéÊìæ
                var deviceInfo = await SmartCardReaderUtils.GetFirstSmartCardReaderInfo(SmartCardReaderKind.Nfc);

                this.IsSupported = deviceInfo != null;
                this.IsEnabled = (deviceInfo != null) && deviceInfo.IsEnabled;

                if (!this.IsSupported)
                {
                    throw new FelicaReaderNotSupportedException("NFC is not supported");
                }
                else if (!this.IsEnabled)
                {
                    throw new FelicaReaderNotEnabledException("NFC is not enabled");
                }

                this.reader = await SmartCardReader.FromIdAsync(deviceInfo.Id);

                // FelicaÉJÅ[ÉhÇíTçı
                var cts = tokenSource ?? new CancellationTokenSource();
                try
                {
                    var task = this.FindSmartCard(this.reader, cts.Token);
                    task.Wait(timeoutInSecs * 1000);

                    if (task.Status == TaskStatus.RanToCompletion)
                    {
                        var smartCard = task.Result;
                        SmartCardConnection connection = await smartCard.ConnectAsync();

                        IccDetection cardIdentification = new IccDetection(smartCard, connection);
                        await cardIdentification.DetectCardTypeAync();

                        if (cardIdentification.PcscDeviceClass == Pcsc.Common.DeviceClass.StorageClass
                            && cardIdentification.PcscCardName == Pcsc.CardName.FeliCa)
                        {
                            this.felicaCardSubject.OnNext(new FelicaCardMediaImplementation(connection));
                        }
                        else
                        {
                            throw new UnknownCardException(
                                String.Format("Unknown device: PcscDeviceClass={0}, PcscCardName={1}",
                                cardIdentification.PcscDeviceClass,
                                cardIdentification.PcscCardName));
                        }
                    }
                    else if (task.Status == TaskStatus.Canceled)
                    {
                        throw new FelicaReaderTimeoutException("Read Timeout");
                    }
                    else
                    {
                        // TimeoutÇ™î≠ê∂ÇµÇΩèÍçá
                        throw new FelicaReaderTimeoutException("Read Timeout");
                    }
                }
                catch (AggregateException)
                {
                    throw new FelicaReaderException();
                }

            });

            return;
        }

        public void EnableForeground()
        {
            // TODO; set flag
        }

        public void DisableForeground()
        {
            // TODO; set flag
        }

        public IObservable<IFelicaCardMedia> WhenCardFound()
        {
            return this.felicaCardSubject.AsObservable<IFelicaCardMedia>();
        }
    }
}