using System.Linq;

namespace ClusterClient.Clients.Models.Builders
{
    public static class ClusterRequestBuilderExtensions
    {
        public static ClusterRequest Create(this IClusterRequestBuilder requestBuilder,
            params (string key, string value)[] queryParameters)
        {
            var query = string.Join('&', queryParameters.Select(x => $"{x.key}={x.value}"));
            return requestBuilder.Create(query);
        }
    }
}