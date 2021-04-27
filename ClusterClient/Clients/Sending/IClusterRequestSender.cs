using System.Threading.Tasks;
using ClusterClient.Clients.Models;

namespace ClusterClient.Clients.Sending
{
    public interface IClusterRequestSender
    {
        Task<string> SendRequestAsync(ClusterRequest request, Replica replica);
    }
}