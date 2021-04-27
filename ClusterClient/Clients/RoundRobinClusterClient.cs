using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ClusterClient.Clients.Models;
using ClusterClient.Clients.Models.Builders;
using ClusterClient.Clients.Sending;

namespace ClusterClient.Clients
{
    public class RoundRobinClusterClient : ClusterClient
    {
        public RoundRobinClusterClient(IClusterRequestBuilder requestBuilder, IClusterRequestSender sender,
            IEnumerable<Replica> replicas) : base(requestBuilder, sender, replicas)
        {
        }

        protected override async Task<string> SendRequest(ICollection<ReplicaRequestSender> replicas,
            ClusterRequest request, TimeSpan timeout)
        {
            var remainingReplicas = replicas.Count;
            var stopWatch = Stopwatch.StartNew();

            foreach (var replica in replicas)
            {
                var delayStep = (timeout - stopWatch.Elapsed) / remainingReplicas--;
                var delayTask = Delay<string>(delayStep);
                var requestTask = replica.SendRequest(request);

                var completedTask = await Task.WhenAny(requestTask, delayTask);

                if (completedTask != delayTask && completedTask.IsCompletedSuccessfully) 
                    return completedTask.Result;
            }

            throw new TimeoutException();
        }
    }
}