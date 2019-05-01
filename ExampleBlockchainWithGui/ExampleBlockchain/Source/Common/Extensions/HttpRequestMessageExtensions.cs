using System.Net.Http;
using System.Web.Http.Routing;

namespace BlockchainApp.Source.Common.Extensions
{
    public static class HttpRequestMessageExtensions
    {
        public static string Root(this HttpRequestMessage request)
        {
            return new UrlHelper(request).Content("~").SkipLast(1);
        }
    }
}
