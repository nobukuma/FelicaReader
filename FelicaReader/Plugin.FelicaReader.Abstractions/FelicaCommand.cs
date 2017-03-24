using System;
using System.Threading.Tasks;
using Plugin.FelicaReader.Abstractions.Response;

namespace Plugin.FelicaReader.Abstractions
{
    public static class FelicaCommand
    {
        private const int RetryMaxCount = 3;

        public static async Task<PollingResponse> Polling(
            this IFelicaCardMedia felicaCard,
            UInt16 systemCode)
        {
            byte systemCodeHigher = (byte)(systemCode >> 8);
            byte systemCodeLower = (byte)(systemCode & 0x00ff);

            byte[] commandData = new byte[] {
               0x00, 0x00, systemCodeHigher, systemCodeLower, 0x00, 0x0f,
            };
            commandData[0] = (byte)commandData.Length;

            for (int i = 0; i < RetryMaxCount; i++)
            {
                byte[] result = await felicaCard.Send(commandData);
                if (result.Length > 0)
                {
                    return PollingResponse.ParsePackage(result);
                }
            }
            return PollingResponse.ParsePackage(new byte[0]);
        }

        public static async Task<ReadWithoutEncryptionResponse> ReadWithoutEncryption(
            this IFelicaCardMedia felicaCard,
            byte[] idm,
            UInt16 serviceCode,
            byte blockNumber,
            byte[] blockList)
        {
            byte serviceCodeHigher = (byte)(serviceCode >> 8);
            byte serviceCodeLower = (byte)(serviceCode & 0x00ff);

            byte[] commandDataPrefix = new byte[] {
                0x00,
                0x06,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x01,
                serviceCodeLower, serviceCodeHigher,
                blockNumber,
                // block list
            };

            for (int i = 0; i < idm.Length; i++)
            {
                commandDataPrefix[i + 2] = idm[i];
            }

            byte[] commandData = new byte[commandDataPrefix.Length + blockList.Length];
            Array.Copy(commandDataPrefix, commandData, commandDataPrefix.Length);
            Array.Copy(blockList, 0, commandData, commandDataPrefix.Length, blockList.Length);
            commandData[0] = (byte)commandData.Length;

            for (int i = 0; i < RetryMaxCount; i++)
            {
                byte[] result = await felicaCard.Send(commandData);
                if (result.Length > 0)
                {
                    return ReadWithoutEncryptionResponse.ParsePacketData(result);
                }
            }

            return ReadWithoutEncryptionResponse.ParsePacketData(new byte[0]);
        }

        public static async Task<RequestServiceResponse> RequestService(
            this IFelicaCardMedia felicaCard,
            byte[] idm,
            byte blockNumber,
            ushort[] blockList)
        {
            byte[] blockByteList = new byte[blockList.Length * 2];
            for (int i = 0; i < (int)blockNumber; i++)
            {
                blockByteList[i * 2] = (byte)(blockList[i] & 0xFF);
                blockByteList[i * 2 + 1] = (byte)(blockList[i] >> 8);
            }

            return await RequestService(felicaCard, idm, blockNumber, blockByteList);
        }

        private static async Task<RequestServiceResponse> RequestService(
            this IFelicaCardMedia felicaCard,
            byte[] idm,
            byte blockNumber,
            byte[] blockList)
        {
            byte[] commandDataPrefix = new byte[] {
                0x00,
                0x02,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                blockNumber,
                // block list
            };
            for (int i = 0; i < idm.Length; i++)
            {
                commandDataPrefix[i + 2] = idm[i];
            }
            byte[] commandData = new byte[commandDataPrefix.Length + blockList.Length];
            Array.Copy(commandDataPrefix, commandData, commandDataPrefix.Length);
            Array.Copy(blockList, 0, commandData, commandDataPrefix.Length, blockList.Length);
            commandData[0] = (byte)commandData.Length;

            for (int i = 0; i < RetryMaxCount; i++)
            {
                byte[] result = await felicaCard.Send(commandData);
                if (result.Length > 0)
                {
                    return RequestServiceResponse.ParsePackage(result);
                }
            }
            return RequestServiceResponse.ParsePackage(new byte[0]);
        }

        public static async Task<RequestSystemCodeResponse> RequestSystemCode(
            this IFelicaCardMedia felicaCard,
            byte[] idm)
        {
            byte[] commandData = new byte[] {
                0x00,
                0x0c,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            };
            for (int i = 0; i < idm.Length; i++)
            {
                commandData[i + 2] = idm[i];
            }
            commandData[0] = (byte)commandData.Length;

            for (int i = 0; i < RetryMaxCount; i++)
            {
                byte[] result = await felicaCard.Send(commandData);
                if (result.Length > 0)
                {
                    return RequestSystemCodeResponse.ParsePackage(result);
                }
            }
            return RequestSystemCodeResponse.ParsePackage(new byte[0]);
        }
        public static async Task<byte[]> RequestResponse(
            this IFelicaCardMedia felicaCard,
            byte[] idm)
        {
            byte[] commandData = new byte[] {
                0x00,
                0x04,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            };
            for (int i = 0; i < idm.Length; i++)
            {
                commandData[i + 2] = idm[i];
            }
            commandData[0] = (byte)commandData.Length;

            for (int i = 0; i < RetryMaxCount; i++)
            {
                byte[] result = await felicaCard.Send(commandData);
                if (result.Length > 0)
                {
                    return result;
                }
            }
            return new byte[0];
        }

        public static async Task<byte[]> SearchServiceCode(
            this IFelicaCardMedia felicaCard,
            byte[] idm,
            ushort serviceIndex)
        {
            byte serviceCodeHigher = (byte)(serviceIndex >> 8);
            byte serviceCodeLower = (byte)(serviceIndex & 0x00ff);

            byte[] commandData = new byte[] {
                0x00,
                0x0a,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                serviceCodeLower, serviceCodeHigher,
            };
            for (int i = 0; i < idm.Length; i++)
            {
                commandData[i + 2] = idm[i];
            }
            commandData[0] = (byte)commandData.Length;

            for (int i = 0; i < RetryMaxCount; i++)
            {
                byte[] result = await felicaCard.Send(commandData);
                if (result.Length > 0)
                {
                    return result;
                }
            }
            return new byte[0];
        }
    }
}
