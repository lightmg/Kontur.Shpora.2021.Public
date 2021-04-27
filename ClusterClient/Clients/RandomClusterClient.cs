using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ClusterClient.Clients.Models;
using ClusterClient.Clients.Models.Builders;
using ClusterClient.Clients.Sending;
using log4net;

namespace ClusterClient.Clients
{
    public class RandomClusterClient : IClusterClient
    {
        private readonly ILog log = LogManager.GetLogger(typeof(RandomClusterClient));
        private readonly Random random = new();

        private readonly IClusterRequestBuilder requestBuilder;
        private readonly IClusterRequestSender sender;
        private readonly HashSet<Replica> replicas;

        public RandomClusterClient(IClusterRequestSender sender, IClusterRequestBuilder requestBuilder,
            IEnumerable<Replica> replicas)
        {
            this.sender = sender;
            this.requestBuilder = requestBuilder;
            this.replicas = replicas.ToHashSet();
        }

        public async Task<string> SendRequestAsync(string query, TimeSpan timeout)
        {
            var index = random.Next(replicas.Count);
            var replica = replicas.ElementAt(index);

            log.InfoFormat($"Processing request to {replica.Url}");
            var resultTask = sender.SendRequestAsync(requestBuilder.Create(("query", query)), replica);

            await Task.WhenAny(resultTask, Task.Delay(timeout));
            if (!resultTask.IsCompleted)
                throw new TimeoutException();

            return resultTask.Result;
        }
    }
}