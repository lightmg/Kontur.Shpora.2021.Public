using System;
using System.Threading.Tasks;

namespace ClusterClient.Clients
{
    public interface IClusterClient
    {
        Task<string> ProcessRequestAsync(string query, TimeSpan timeout);
    }
}