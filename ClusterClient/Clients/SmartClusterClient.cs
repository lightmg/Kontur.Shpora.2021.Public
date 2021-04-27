using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ClusterClient.Clients.Models;
using ClusterClient.Clients.Models.Builders;
using ClusterClient.Clients.Sending;

namespace ClusterClient.Clients
{
    public class SmartClusterClient : ClusterClient
    {
        public SmartClusterClient(IClusterRequestBuilder requestBuilder,
            IClusterRequestSender sender,
            IEnumerable<Replica> replicas) : base(requestBuilder, sender, replicas)
        { }

        protected override async Task<string> SendRequest(ICollection<ReplicaRequestSender> replicas,
            ClusterRequest request, TimeSpan timeout)
        {
            var tasks = new List<Task<string>>();
            var remainingReplicas = replicas.Count;
            var stopWatch = Stopwatch.StartNew();

            foreach (var replica in replicas)
            {
                var delayStep = (timeout - stopWatch.Elapsed) / remainingReplicas--;
                tasks.Add(replica.SendRequest(request));

                var delayTask = Delay<string>(delayStep);
                var completedTask = await Task.WhenAny(tasks.Append(delayTask));

                if (completedTask != delayTask)
                {
                    if (completedTask.IsCompletedSuccessfully)
                        return completedTask.Result;

                    tasks.Remove(completedTask);
                }
            }

            throw new TimeoutException();
        }
    }
}