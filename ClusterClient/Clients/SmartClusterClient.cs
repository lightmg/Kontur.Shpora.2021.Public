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
    public class SmartClusterClient : IClusterClient
    {
        private readonly IClusterRequestBuilder requestBuilder;
        private readonly IClusterRequestSender sender;
        private readonly MeasuredReplica[] measuredReplicas;

        public SmartClusterClient(IClusterRequestBuilder requestBuilder,
            IClusterRequestSender sender,
            IEnumerable<Replica> replicas)
        {
            this.requestBuilder = requestBuilder;
            this.sender = sender;
            measuredReplicas = replicas.Select(MeasuredReplica.Empty).ToArray();
        }

        public async Task<string> SendRequestAsync(string query, TimeSpan timeout)
        {
            var replicasByPriority = measuredReplicas
                .OrderByDescending(r => r.SuccessPercentage)
                .ThenBy(r => r.AverageLatency);

            var request = requestBuilder.Create(query);
            var tasks = new List<Task<string>>();

            foreach (var measuredReplica in replicasByPriority)
            {
                var requestTask = measuredReplica.SendMeasuredRequest(sender, request);
                tasks.Add(requestTask);

                var delayTask = Delay<string>(timeout / measuredReplicas.Length);

                var completedTask = await Task.WhenAny(tasks.Prepend(delayTask));
                if (completedTask != delayTask && completedTask.IsCompletedSuccessfully)
                    return completedTask.Result;
            }

            throw new TimeoutException();
        }

        private static async Task<T> Delay<T>(TimeSpan delay)
        {
            await Task.Delay(delay);
            return default;
        }

        private class MeasuredReplica
        {
            private readonly ConcurrentBag<ClusterRequestMeasurement> measurements;

            public Replica Replica { get; }
            public TimeSpan AverageLatency { get; private set; }
            public double SuccessPercentage { get; private set; }

            private MeasuredReplica(Replica replica)
            {
                measurements = new ConcurrentBag<ClusterRequestMeasurement>();
                Replica = replica;
                AverageLatency = TimeSpan.Zero;
                SuccessPercentage = 100D;
            }

            public void AddMeasurement(ClusterRequestMeasurement measurement) => measurements.Add(measurement);

            public async Task<string> SendMeasuredRequest(IClusterRequestSender sender, ClusterRequest request)
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    var result = await sender.SendRequestAsync(request, Replica);
                    sw.Stop();
                    AddMeasurement(new ClusterRequestMeasurement(sw.Elapsed, true));
                    return result;
                }
                catch
                {
                    sw.Stop();
                    AddMeasurement(new ClusterRequestMeasurement(sw.Elapsed, false));
                    throw;
                }
            }

            private void UpdateData()
            {
                var succeedRequestsTotalTime = TimeSpan.Zero;
                var succeedRequests = 0D;

                foreach (var measurement in measurements.Where(x => x.IsSucceed))
                {
                    succeedRequestsTotalTime += measurement.ElapsedTime;
                    succeedRequests++;
                }

                AverageLatency = succeedRequestsTotalTime / succeedRequests;
                SuccessPercentage = succeedRequests / measurements.Count * 100;
            }

            public static MeasuredReplica Empty(Replica replica) => new(replica);

            public static implicit operator Replica(MeasuredReplica measuredReplica) => measuredReplica.Replica;
        }
    }
}