using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClusterClient.Clients.Models;
using ClusterClient.Clients.Models.Builders;
using ClusterClient.Clients.Sending;

namespace ClusterClient.Clients
{
    public class ParallelClusterClient : ClusterClient
    {
        public ParallelClusterClient(IClusterRequestBuilder requestBuilder, IClusterRequestSender sender,
            IEnumerable<Replica> replicas) : base(requestBuilder, sender, replicas)
        {
        }

        protected override async Task<string> SendRequest(ICollection<ReplicaRequestSender> replicas,
            ClusterRequest request, TimeSpan timeout)
        {
            var delayTask = ThrowTimeoutExceptionAfter<string>(timeout);
            var tasks = replicas.Select(x => x.SendRequest(request)).Append(delayTask).ToList();
            Task<string> completedTask;
            do
            {
                completedTask = await Task.WhenAny(tasks);

                if (completedTask.IsCompletedSuccessfully)
                    return completedTask.Result;

                tasks.Remove(completedTask);
            } while (tasks.Count > 1 && completedTask != delayTask);

            throw new TimeoutException();
        }
    }
}