using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Nfc.Tech;
using System.Threading.Tasks;
using Plugin.FelicaReader.Abstractions;

namespace Plugin.FelicaReader
{
    public class FelicaCardMediaImplementation : IFelicaCardMedia
    {
        private NfcF nfc;

        public byte[] IDm => this.nfc?.Tag.GetId();

        public bool IsConnected => this.nfc?.IsConnected ?? false;

        public FelicaCardMediaImplementation(NfcF nfc)
        {
            this.nfc = nfc;
        }

        public void Dispose()
        {
            this.nfc.Close();
            return;
        }

        public Task<byte[]> GetIdm()
        {
            return Task.FromResult<byte[]>(IDm);
        }

        public Task<byte[]> Send(byte[] data)
        {
            if (!this.IsConnected)
            {
                throw new InvalidOperationException("Not Connected");
            }

            try
            {
                byte[] res = nfc.Transceive(data);
                return Task.FromResult<byte[]>(res);
            }
            catch (Java.Lang.Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                throw e;
            }
        }
    }
}