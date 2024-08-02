using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CargoMaker.Health
{
    public class HealthPing
    {
        private readonly ILogger<HealthPing> log;

        public HealthPing(ILogger<HealthPing> logger)
        {
            log = logger;
        }

        [Function("Ping")]
        public IActionResult Ping([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health/ping")] HttpRequest req)
        {
            log.LogInformation("Health ping triggered.");

            return new OkObjectResult(new { status = "up" });
        }
    }
}
