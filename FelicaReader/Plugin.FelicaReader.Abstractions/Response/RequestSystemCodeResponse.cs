using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.FelicaReader.Abstractions.Response
{
    public class RequestSystemCodeResponse
    {
        public byte[] PacketData { get; set; }

        public byte[] IDm { get; set; }

        public byte SystemCodeNumber { get; set; }

        public ushort[] SystemCodeList { get; set; }

        public bool HasData
        {
            get
            {
                return PacketData.Length > 0;
            }
        }

        public static RequestSystemCodeResponse ParsePackage(
            byte[] packatData)
        {
            if (packatData == null || packatData.Length == 0)
            {
                return new RequestSystemCodeResponse()
                {
                    PacketData = new byte[0],
                    IDm = new byte[0],
                    SystemCodeList = new ushort[0],
                };
            }

            // カード情報の解析
            // len 0x0d IDm(8 byte) systemcode number(1 byte) system code list<<data>>
            byte responseLen = packatData[0];

            byte[] idm = new byte[8];
            Array.Copy(packatData, 2, idm, 0, idm.Length);

            byte outSystemCodeNumber = packatData[10];
            ushort[] outSystemCodeList = new ushort[outSystemCodeNumber];

            for (int i = 0;i < (int)outSystemCodeNumber; i++)
            {
                outSystemCodeList[i] = (ushort)(packatData[i * 2 + 11] << 8 | packatData[i * 2 + 12]);
            }

            return new RequestSystemCodeResponse()
            {
                PacketData = packatData,
                IDm = idm,
                SystemCodeNumber = outSystemCodeNumber,
                SystemCodeList = outSystemCodeList,
            };
        }
    }
}
