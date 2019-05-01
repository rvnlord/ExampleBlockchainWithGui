using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Results;

namespace BlockchainApp.Source.Common.Extensions
{
    public static class ApiControllerExtensions
    {
        public static RedirectResult RedirectRelative(this ApiController apiController, string relativeLocation)
        {
            var miRedirect = apiController.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(mi => mi.Name == "Redirect" && mi.GetParameters().Length == 1)
                .Single(mi => mi.GetParameters().Single().ParameterType == typeof(string));

            return (RedirectResult)miRedirect?.Invoke(apiController, new object[] { $"{apiController.Request.Root()}{relativeLocation}" });
        }
    }
}
