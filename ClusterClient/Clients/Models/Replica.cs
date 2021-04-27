namespace ClusterClient.Clients.Models
{
    public class Replica
    {
        public Replica(string url)
        {
            Url = url;
        }

        public string Url { get; }

        public static Replica FromUrl(string url) => new(url);
    }
}