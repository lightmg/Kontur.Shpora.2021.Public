namespace ClusterClient.Clients.Models.Builders
{
    public class DefaultClusterRequestBuilder : IClusterRequestBuilder
    {
        public ClusterRequest Create(string queryParameters = null) =>
            new ClusterRequest(queryParameters, request =>
            {
                request.Proxy = null;
                request.KeepAlive = true;
                request.ServicePoint.UseNagleAlgorithm = false;
                request.ServicePoint.ConnectionLimit = 100500;
            });
    }
}