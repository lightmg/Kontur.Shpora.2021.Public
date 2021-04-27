using System.Diagnostics;
using System.Threading.Tasks;
using ClusterClient.Clients.Models;
using log4net;

namespace ClusterClient.Clients.Sending
{
    public class ClusterRequestSenderLoggerWrapper : IClusterRequestSender
    {
        private readonly IClusterRequestSender sender;
        private readonly ILog log;

        public ClusterRequestSenderLoggerWrapper(IClusterRequestSender sender)
        {
            this.sender = sender;
            log = LogManager.GetLogger(typeof(ClusterRequestSenderLoggerWrapper));
        }

        public async Task<string> SendRequestAsync(ClusterRequest request, Replica replica)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                return await sender.SendRequestAsync(request, replica);
            }
            finally
            {
                sw.Stop();
                log.InfoFormat("Response from {0} finished after {1} ms", replica.Url, sw.ElapsedMilliseconds);
            }
        }
    }
}