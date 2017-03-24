using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.FelicaReader.Abstractions.Response
{
    public class PollingResponse
    {
        public byte[] PacketData { get; set; }

        public byte[] IDm { get; set; }

        public byte[] PMm { get; set; }

        public bool HasData
        {
            get
            {
                return PacketData.Length > 0;
            }
        }

        public static PollingResponse ParsePackage(
            byte[] packatData)
        {
            if (packatData == null || packatData.Length == 0)
            {
                return new PollingResponse()
                {
                    PacketData = new byte[0],
                    IDm = new byte[0],
                    PMm = new byte[0],
                };
            }

            // カード情報の解析
            // len 0x0d IDm(8 byte) systemcode number(1 byte) system code list<<data>>
            byte responseLen = packatData[0];

            byte[] idm = new byte[8];
            Array.Copy(packatData, 2, idm, 0, idm.Length);

            byte[] pmm = new byte[8];
            Array.Copy(packatData, 10, pmm, 0, idm.Length);

            return new PollingResponse()
            {
                PacketData = packatData,
                IDm = idm,
                PMm = pmm,
            };
        }
    }
}
