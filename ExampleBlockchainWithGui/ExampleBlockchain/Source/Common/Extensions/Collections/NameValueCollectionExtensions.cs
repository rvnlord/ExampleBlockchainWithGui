using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using MoreLinq;

namespace BlockchainApp.Source.Common.Extensions.Collections
{
    public static class NameValueCollectionExtensions
    {
        public static IEnumerable<KeyValuePair<string, string>> AsEnumerable(this NameValueCollection nvc)
        {
            return nvc.AllKeys.SelectMany(nvc.GetValues, (k, v) => new KeyValuePair<string, string>(k, v));
        }

        public static Dictionary<string, string> ToDictionary(this NameValueCollection nvc)
        {
            return nvc.AsEnumerable().ToDictionary();
        }
    }
}
