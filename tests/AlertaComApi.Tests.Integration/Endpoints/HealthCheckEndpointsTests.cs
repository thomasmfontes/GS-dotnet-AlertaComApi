using System.Net;
using AlertaComApi.Tests.Integration.Fixtures;
using FluentAssertions;
using Xunit;

namespace AlertaComApi.Tests.Integration.Endpoints
{
    public class HealthCheckEndpointsTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public HealthCheckEndpointsTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Health_DeveRetornar200OuDegraded_ComJsonDetalhado()
        {
            // Act
            var response = await _client.GetAsync("/health");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("status");
            content.Should().Contain("checks");
        }

        [Fact]
        public async Task HealthLive_DeveRetornar200_LivenessProbe()
        {
            // Act
            var response = await _client.GetAsync("/health/live");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Healthy");
        }
    }
}
