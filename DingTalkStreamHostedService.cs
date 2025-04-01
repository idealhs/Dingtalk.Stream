using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Dingtalk.Stream
{
    /// <summary>
    /// 钉钉 Stream 客户端的托管服务
    /// Hosted service for DingTalk Stream client
    /// </summary>
    public class DingTalkStreamHostedService : IHostedService
    {
        private readonly IOpenDingTalkStreamClient _client;
        private readonly ILogger<DingTalkStreamHostedService> _logger;
        private Task? _executingTask;

        /// <summary>
        /// 构造函数
        /// Constructor
        /// </summary>
        /// <param name="client">钉钉 Stream 客户端 / DingTalk Stream client</param>
        /// <param name="logger">日志记录器 / Logger</param>
        public DingTalkStreamHostedService(
            IOpenDingTalkStreamClient client,
            ILogger<DingTalkStreamHostedService> logger)
        {
            _client = client;
            _logger = logger;
        }

        /// <summary>
        /// 启动服务
        /// Start the service
        /// </summary>
        /// <param name="cancellationToken">取消令牌 / Cancellation token</param>
        /// <returns>表示异步操作的任务 / Task representing the asynchronous operation</returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting DingTalk Stream client...");
            _executingTask = _client.StartAsync();
            return Task.CompletedTask;
        }

        /// <summary>
        /// 停止服务
        /// Stop the service
        /// </summary>
        /// <param name="cancellationToken">取消令牌 / Cancellation token</param>
        /// <returns>表示异步操作的任务 / Task representing the asynchronous operation</returns>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping DingTalk Stream client...");
            if (_executingTask != null)
            {
                await _client.StopAsync();
                await _executingTask;
            }
        }
    } 
}