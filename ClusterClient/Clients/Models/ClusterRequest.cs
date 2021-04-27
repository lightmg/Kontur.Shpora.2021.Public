using System;
using System.Net;
using Fclp.Internals.Extensions;

namespace ClusterClient.Clients.Models
{
    public class ClusterRequest
    {
        private readonly string queryParameters;
        private readonly Action<HttpWebRequest> modifier;

        public ClusterRequest(string queryParameters, Action<HttpWebRequest> modifier)
        {
            this.queryParameters = queryParameters;
            this.modifier = modifier ?? DoNothing;
        }

        public HttpWebRequest CreateWebRequestTo(Replica replica)
        {
            var url = GetUrl(replica.Url, queryParameters);
            var webRequest = WebRequest.CreateHttp(url);
            modifier.Invoke(webRequest);
            return webRequest;
        }

        private static string GetUrl(string targetUrl, string queryParameters)
        {
            if (queryParameters.IsNullOrWhiteSpace())
                return targetUrl;

            return queryParameters.StartsWith('?')
                ? targetUrl + queryParameters
                : targetUrl + '?' + queryParameters;
        }

        private static void DoNothing<T>(T param)
        {
        }
    }
}