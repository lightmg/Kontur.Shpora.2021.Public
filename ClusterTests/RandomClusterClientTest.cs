using System.Linq;
using ClusterClient.Clients;
using ClusterClient.Clients.Models;
using ClusterClient.Clients.Models.Builders;
using ClusterClient.Clients.Sending;
using FluentAssertions;
using NUnit.Framework;

namespace ClusterTests
{
	public class RandomClusterClientTest : ClusterTest
	{
		protected override IClusterClient CreateClient(string[] replicaAddresses)
			=> new RandomClusterClient(
				new ClusterRequestSender().WithRequestTimeLogging(), 
				new DefaultClusterRequestBuilder(), 
				replicaAddresses.Select(Replica.FromUrl)
			);

		[Test]
		public void ClientShouldReturnSuccessIn50Percent()
		{
			CreateServer(1, status:500);
			CreateServer(1);

			Enumerable.Range(0, 200)
				.Select(_ =>
				{
					try
					{
						ProcessRequests(Timeout, 1);
						return 1;
					}
					catch
					{
						return 0;
					}
				})
				.Sum().Should().BeCloseTo(100, 20);
		}
	}
}