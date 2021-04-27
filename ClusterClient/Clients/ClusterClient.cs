using System;
using System.Threading.Tasks;

namespace ClusterClient.Clients
{
    public interface IClusterClient
    {
        Task<string> SendRequestAsync(string query, TimeSpan timeout);
    }
}