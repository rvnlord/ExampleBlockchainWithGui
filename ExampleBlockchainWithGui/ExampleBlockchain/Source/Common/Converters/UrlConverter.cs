using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainApp.Source.Common.Converters
{
    public static class UrlConverter
    {
        // string (ws address) --> string (http api address)
        public static string WsAddressToHttpApiAddress(this string wsAddress)
        {
            return wsAddress.Replace(":5", ":3") + "/BlockchainApi";
        }
    }
}
