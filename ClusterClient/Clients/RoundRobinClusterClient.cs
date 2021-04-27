using System;
using System.Threading.Tasks;

namespace ClusterClient.Clients
{
    public class RoundRobinClusterClient : IClusterClient
    {
        public RoundRobinClusterClient(string[] replicaAddresses)
        {
        }

        public Task<string> SendRequestAsync(string query, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }
    }
}
