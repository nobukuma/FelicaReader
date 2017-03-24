using Android.App;
using Android.Content;
using Android.Nfc;
using Android.Nfc.Tech;
using Plugin.FelicaReader.Abstractions;
using System;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;

namespace Plugin.FelicaReader
{
    public sealed class NFCTechs
    {
        //public const string IsoDep = "android.nfc.tech.IsoDep";
        //public const string NfcA = "android.nfc.tech.NfcA";
        //public const string NfcB = "android.nfc.tech.NfcB";
        public const string NfcF = "android.nfc.tech.NfcF";
        //public const string NfcV = "android.nfc.tech.NfcV";
        //public const string Ndef = "android.nfc.tech.Ndef";
        //public const string NdefFormatable = "android.nfc.tech.NdefFormatable";
        //public const string MifareClassic = "android.nfc.tech.MifareClassic";
        //public const string MifareUltralight = "android.nfc.tech.MifareUltralight";
    }

    public class FelicaReaderImplementation : IFelicaReader
    {
        private Activity activity;
        private System.Type type;

        private Subject<IFelicaCardMedia> felicaCardSubject;

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

        public FelicaReaderImplementation(Activity activity, System.Type type)
        {
            this.activity = activity;
            this.type = type;

            this.felicaCardSubject = new Subject<IFelicaCardMedia>();

            NfcManager nfcManager = (NfcManager)Android.App.Application.Context.GetSystemService(Context.NfcService);
            if (nfcManager == null)
            {
                IsEnabled = false;
                IsSupported = false;
            }
            else
            {
                IsSupported = true;
                IsEnabled = nfcManager.DefaultAdapter.IsEnabled;
            }
        }

        public void DisableForeground()
        {
            NfcManager nfcManager = (NfcManager)Android.App.Application.Context.GetSystemService(Context.NfcService);
            var nfcDevice = nfcManager.DefaultAdapter;
            nfcDevice.DisableForegroundDispatch(this.activity);
        }

        public void EnableForeground()
        {
            this.SetupForeGroundNFC();
        }

        private void SetupForeGroundNFC()
        {
            NfcManager nfcManager = (NfcManager)Android.App.Application.Context.GetSystemService(Context.NfcService);
            var nfcDevice = nfcManager.DefaultAdapter;

            var intent = new Intent(activity, type).AddFlags(ActivityFlags.SingleTop);

            nfcDevice.EnableForegroundDispatch(
                this.activity,
                PendingIntent.GetActivity(this.activity, 0, intent, 0),
                new[] { new IntentFilter(NfcAdapter.ActionTechDiscovered) },
                    new string[][] {new string[] {
                        NFCTechs.NfcF,
                    },
                }
            );

            return;
        }

        public void FindCard(int timeoutInSecs = 10, CancellationTokenSource tokenSource = null)
        {
            return;
        }

        public IObservable<IFelicaCardMedia> WhenCardFound()
        {
            return this.felicaCardSubject;
        }
    }
}