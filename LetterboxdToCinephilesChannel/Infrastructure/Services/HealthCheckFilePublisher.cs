using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace LetterboxdToCinephilesChannel.Infrastructure.Services
{
    public class HealthCheckFilePublisher : IHealthCheckPublisher
    {
        private readonly ILogger<HealthCheckFilePublisher> _logger;
        private const string HealthyFilePath = "/tmp/healthy";

        public HealthCheckFilePublisher(ILogger<HealthCheckFilePublisher> logger)
        {
            _logger = logger;
        }

        public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
        {
            try
            {
                if (report.Status == HealthStatus.Healthy)
                {
                    File.WriteAllText(HealthyFilePath, "Healthy");
                }
                else
                {
                    if (File.Exists(HealthyFilePath))
                    {
                        File.Delete(HealthyFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing health check file.");
            }

            return Task.CompletedTask;
        }
    }
}
