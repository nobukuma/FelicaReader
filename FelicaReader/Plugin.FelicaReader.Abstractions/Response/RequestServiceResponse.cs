using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.FelicaReader.Abstractions.Response
{
    public class RequestServiceResponse
    {
        public byte[] PacketData { get; set; }

        public byte[] IDm { get; set; }

        public byte NodeNumber { get; set; }

        public ushort[] NodeList { get; set; }

        public bool HasData
        {
            get
            {
                return PacketData.Length > 0;
            }
        }

        public static RequestServiceResponse ParsePackage(
            byte[] packatData)
        {
            if (packatData == null || packatData.Length == 0)
            {
                return new RequestServiceResponse()
                {
                    PacketData = new byte[0],
                    IDm = new byte[0],
                    NodeList = new ushort[0],
                };
            }

            // カード情報の解析
            // len 0x0d IDm(8 byte) systemcode number(1 byte) system code list<<data>>
            byte responseLen = packatData[0];

            byte[] idm = new byte[8];
            Array.Copy(packatData, 2, idm, 0, idm.Length);

            byte outNodeNumber = packatData[10];
            ushort[] outSystemCodeList = new ushort[outNodeNumber];

            for (int i = 0;i < (int)outNodeNumber; i++)
            {
                outSystemCodeList[i] = (ushort)(packatData[i * 2 + 12] << 8 | packatData[i * 2 + 11]);
            }

            return new RequestServiceResponse()
            {
                PacketData = packatData,
                IDm = idm,
                NodeNumber = outNodeNumber,
                NodeList = outSystemCodeList,
            };
        }
    }
}
