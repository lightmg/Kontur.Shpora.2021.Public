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
    public interface IClusterClient
    {
        Task<string> SendRequestAsync(string query, TimeSpan timeout);
    }
    
    public abstract class ClusterClient : IClusterClient
    {
        private readonly IClusterRequestBuilder requestBuilder;
        private readonly ReplicaRequestSender[] measuredReplicas;

        protected ClusterClient(IClusterRequestBuilder requestBuilder, IClusterRequestSender sender,
            IEnumerable<Replica> replicas)
        {
            this.requestBuilder = requestBuilder;
            measuredReplicas = replicas.Select(replica => new ReplicaRequestSender(replica, sender)).ToArray();
        }

        public async Task<string> SendRequestAsync(string query, TimeSpan timeout)
        {
            var replicas = measuredReplicas
                .OrderByDescending(x => x.SuccessPercentage)
                .ThenBy(x => x.AverageLatency)
                .ToArray();

            var request = requestBuilder.Create(("query", query));
            return await SendRequest(replicas, request, timeout);
        }

        protected static Task<T> Delay<T>(TimeSpan timeout) =>
            Task.Delay(timeout).ContinueWith(_ => default(T));

        protected static Task<T> ThrowTimeoutExceptionAfter<T>(TimeSpan timeout) =>
            Task.Delay(timeout).ContinueWith<T>(_ => throw new TimeoutException());

        protected abstract Task<string> SendRequest(ICollection<ReplicaRequestSender> replicas, ClusterRequest request,
            TimeSpan timeout);

        protected class ReplicaRequestSender
        {
            private readonly IClusterRequestSender sender;
            private readonly ConcurrentBag<ClusterRequestMeasurement> measurements;
            private readonly Replica replica;

            public TimeSpan AverageLatency { get; private set; }
            public double SuccessPercentage { get; private set; }

            public ReplicaRequestSender(Replica replica, IClusterRequestSender sender)
            {
                measurements = new ConcurrentBag<ClusterRequestMeasurement>();
                this.replica = replica;
                this.sender = sender;
                AverageLatency = TimeSpan.Zero;
                SuccessPercentage = 100D;
            }

            public async Task<string> SendRequest(ClusterRequest request)
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    var result = await sender.SendRequestAsync(request, replica);
                    sw.Stop();
                    measurements.Add(new ClusterRequestMeasurement(sw.Elapsed, true));
                    return result;
                }
                catch
                {
                    sw.Stop();
                    measurements.Add(new ClusterRequestMeasurement(sw.Elapsed, false));
                    throw;
                }
                finally
                {
                    UpdateData();
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

            public static implicit operator Replica(ReplicaRequestSender replicaRequestSender) =>
                replicaRequestSender.replica;
        }
    }
}