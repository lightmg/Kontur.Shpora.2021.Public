using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class ParallelClusterClient : IClusterClient
    {
        public ParallelClusterClient(string[] replicaAddresses)
        {
        }

        public Task<string> SendRequestAsync(string query, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }
    }
}
