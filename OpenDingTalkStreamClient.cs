using Dingtalk.Stream.Entities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Dingtalk.Stream
{
    public class OpenDingTalkStreamClient : IOpenDingTalkStreamClient, IDisposable
    {
        private const string EventType = "EVENT";
        private const string SubscriptionType = "Event";
        private const string AnyEventType = "*";
        private const string SystemType = "SYSTEM";
        private const string PingTopic = "ping";
        private const string DisconnectTopic = "disconnect";
        private const string PushResponseData = "{\"status\":\"SUCCESS\",\"message\":\"OK\"}";
        private const string StreamHost = @"https://api.dingtalk.com";
        private const string StreamRoute = @"/v1.0/gateway/connections/open";
        private const string DisconnectReason = "Disconnect by dingtalk stream server, Ok for reconnect.";


        private readonly JsonSerializerOptions _dingtalkJsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        public event Action<JsonDocument> OnReceived;
        public event Func<JsonDocument, Task> OnReceivedAsync;

        private CancellationTokenSource _cts;

        private HttpClient _httpClient;
        private ClientWebSocket _wsClient;
        private bool disposedValue;
        private readonly string _appKey;
        private readonly string _appSecret;

        public OpenDingTalkStreamClient(string appKey, string appSecret)
        {
            _httpClient = new();
            _wsClient = new();

            _appKey = appKey;
            _appSecret = appSecret;
        }

        private async Task<string> GetDingtalkStreamUrlAsync()
        {
            var subscriptions = new List<object>(1)
        {
            new
            {
                Type = SubscriptionType,
                Topic = AnyEventType
            }
        };

            var body = new
            {
                ClientId = _appKey,
                ClientSecret = _appSecret,
                Subscriptions = subscriptions
            };

            var requestUri = new UriBuilder(StreamHost)
            {
                Path = StreamRoute
            };

            var response = await _httpClient.PostAsJsonAsync(requestUri.ToString(), body, _dingtalkJsonOptions, _cts.Token);
            var requestDto = await response.Content.ReadFromJsonAsync<ConnectRequestDto>(_dingtalkJsonOptions, _cts.Token);

            return $"{requestDto!.Endpoint}?ticket={requestDto.Ticket}";
        }

        private async Task ConnectWebSocketAsync(CancellationToken ct = default)
        {
            if (_wsClient.State is not WebSocketState.Open)
            {
                var uri = new Uri(await GetDingtalkStreamUrlAsync());
                await _wsClient.ConnectAsync(uri, ct);
            }
        }

        private async Task ResponsePingAsync(JsonDocument eventJson, CancellationToken ct = default)
        {
            var pong = new
            {
                Code = 200,
                Headers = new
                {
                    ContentType = Application.Json,
                    MessageId = eventJson.RootElement.GetProperty("headers").GetProperty("messageId").GetString(),
                },
                Message = "OK",
                Data = eventJson.RootElement.GetProperty("data").GetString()
            };

            var pongJson = JsonSerializer.Serialize(pong, _dingtalkJsonOptions);
            var pongBytes = Encoding.UTF8.GetBytes(pongJson);
            await _wsClient!.SendAsync(pongBytes, WebSocketMessageType.Binary, true, ct);
        }

        private async Task ResponsePushAsync(JsonDocument eventJson, CancellationToken ct = default)
        {
            var response = new
            {
                Code = 200,
                Message = "OK",
                Headers = new
                {
                    MessageId = eventJson.RootElement.GetProperty("headers").GetProperty("messageId").GetString(),
                    ContentType = Application.Json
                },
                Data = PushResponseData
            };

            var responseJson = JsonSerializer.Serialize(response, _dingtalkJsonOptions);
            var responseBytes = Encoding.UTF8.GetBytes(responseJson);
            await _wsClient!.SendAsync(responseBytes, WebSocketMessageType.Text, true, ct);
        }

        public async Task StartAsync()
        {
            _cts = new();
            var ct = _cts.Token;

            await ConnectWebSocketAsync(ct);

            var buffer = new Memory<byte>(new byte[4096]);
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var res = await _wsClient!.ReceiveAsync(buffer, ct);
                    var eventData = Encoding.UTF8.GetString(buffer.Span[..res.Count]);
                    var eventJson = JsonDocument.Parse(eventData);
                    eventJson.RootElement.TryGetProperty("headers", out var headers);
                    eventJson.RootElement.TryGetProperty("type", out var type);
                    switch (type.GetString())
                    {
                        case not null and SystemType when headers.TryGetProperty("topic", out var topic) && topic.GetString() == PingTopic:
                            {
                                await ResponsePingAsync(eventJson, ct);
                                break;
                            }
                        case not null and SystemType when headers.TryGetProperty("topic", out var topic) && topic.GetString() == DisconnectTopic:
                            {
                                await _wsClient.CloseAsync(WebSocketCloseStatus.NormalClosure, DisconnectReason, ct);
                                await Task.Delay(10000, ct);
                                await ConnectWebSocketAsync(ct);
                                break;
                            }
                        case not null and EventType:
                            {
                                await ResponsePushAsync(eventJson, ct);
                                if (OnReceivedAsync is not null)
                                {
                                    _ = OnReceivedAsync(eventJson);
                                }
                                if (OnReceived is not null)
                                {
                                    OnReceived(eventJson);
                                }
                                break;
                            }
                    }
                }
                catch
                {
                    await ConnectWebSocketAsync(ct);
                    await Task.Delay(10000, ct);
                }
            }
        }

        public async Task StopAsync()
        {
            await _wsClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "", _cts.Token);
            _cts.Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _wsClient?.Dispose();
                    _httpClient?.Dispose();
                    _cts?.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}