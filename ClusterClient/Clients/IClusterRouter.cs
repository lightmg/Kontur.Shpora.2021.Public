using System.Collections.Generic;
using System.Threading.Tasks;
using ClusterClient.Clients.Models;

namespace ClusterClient.Clients
{
    public interface IClusterRouter
    {
        Task<string> SendRequestAsync(IEnumerable<Replica> replicas);
    }
}