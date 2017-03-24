using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace Pcsc
{
    public class SmartCardReaderDetectionResult
    {
        public DeviceInformation DeviceInfo { get; set; }

        public System.String Id { get; set; }

        public bool IsEnabled { get; set; }
    }
}
