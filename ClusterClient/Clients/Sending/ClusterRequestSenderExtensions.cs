namespace ClusterClient.Clients.Sending
{
    public static class ClusterRequestSenderExtensions
    {
        public static IClusterRequestSender WithRequestTimeLogging(this IClusterRequestSender sender) =>
            sender is not ClusterRequestSenderLoggerWrapper wrapper
                ? new ClusterRequestSenderLoggerWrapper(sender)
                : wrapper;
    }
}