using System.IO;
using System.Threading.Tasks;
using ClusterClient.Clients.Models;

namespace ClusterClient.Clients.Sending
{
    public class ClusterRequestSender : IClusterRequestSender
    {
        public async Task<string> SendRequestAsync(ClusterRequest request, Replica replica)
        {
            using var response = await request.CreateWebRequestTo(replica).GetResponseAsync();
            await using var responseStream = response.GetResponseStream();
            return await new StreamReader(responseStream!).ReadToEndAsync();
        }
    }
}