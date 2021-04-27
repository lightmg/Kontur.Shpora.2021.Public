using System;

namespace ClusterClient.Clients.Models
{
    public readonly struct ClusterRequestMeasurement
    {
        public ClusterRequestMeasurement(TimeSpan elapsedTime, bool isSucceed)
        {
            ElapsedTime = elapsedTime;
            IsSucceed = isSucceed;
        }

        public TimeSpan ElapsedTime { get; }
        public bool IsSucceed { get; }
    }
}