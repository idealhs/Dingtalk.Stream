using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dingtalk.Stream
{
    public interface IOpenDingTalkStreamClient
    {
        public event Action<JsonDocument> OnReceived;

        public event Func<JsonDocument, Task> OnReceivedAsync;

        Task StartAsync();

        Task StopAsync();
    }
}