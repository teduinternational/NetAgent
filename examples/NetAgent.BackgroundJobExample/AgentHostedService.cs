using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetAgent.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.BackgroundJobExample
{
    public class AgentHostedService : BackgroundService
    {
        private readonly ILogger<AgentHostedService> _logger;
        private readonly IAgent _agent;

        public AgentHostedService(ILogger<AgentHostedService> logger, IAgent agent)
        {
            _logger = logger;
            _agent = agent;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🧠 Agent background job started");

            while (!stoppingToken.IsCancellationRequested)
            {
                string goal = "Tóm tắt tin tức công nghệ hôm nay"; // ví dụ giả định

                try
                {
                    var result = await _agent.ExecuteGoalAsync(goal);
                    _logger.LogInformation("✅ Agent result: {Result}", result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Agent execution failed");
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }

            _logger.LogInformation("🧠 Agent background job stopped");
        }
    }
}
