using Plugin.FelicaReader.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.SmartCards;

namespace Plugin.FelicaReader
{
    public class FelicaCardMediaImplementation : IFelicaCardMedia
    {
        private SmartCardConnection connection;

        public FelicaCardMediaImplementation(SmartCardConnection connection)
        {
            this.connection = connection;
        }

        public void Dispose()
        {
            if (this.connection != null)
            {
                this.connection.Dispose();
            }
            this.connection = null;
        }

        public async Task<byte[]> GetIdm()
        {
            if (this.connection == null)
            {
                throw new InvalidOperationException("Not Connected");
            }

            var felicaAccess = new Felica.AccessHandler(connection);
            byte[] uid = await felicaAccess.GetUidAsync();
            return uid;
        }

        public async Task<byte[]> Send(byte[] data)
        {
            if (this.connection == null)
            {
                throw new InvalidOperationException("Not Connected");
            }

            var felicaAccess = new Felica.AccessHandler(connection);
            byte[] result = await felicaAccess.TransparentExchangeAsync(data);
            return result;
        }

    }
}
