namespace ClusterClient.Clients.Models.Builders
{
    public interface IClusterRequestBuilder
    {
        ClusterRequest Create(string queryParameters = null);
    }
}