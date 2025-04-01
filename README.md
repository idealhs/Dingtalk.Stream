# **Dingtalk Stream Mode .NET Unofficial SDK**

钉钉 Stream 模式的非官方 .NET 实现。
Unofficial .NET implementation of Dingtalk stream mode.

它处理了认证、保活和推送事件的响应。
It handles the authentication, keep-alive and response of push event.

## 关于 Stream 模式 / About Stream Mode

钉钉文档链接：
See Dingtalk doc here:

[Stream Mode Doc](https://open.dingtalk.com/document/resourcedownload/introduction-to-stream-mode)

本 SDK 基于以下文档开发：
This SDK was developed under the guide of doc:

[Guide Doc](https://open.dingtalk.com/document/direction/stream-mode-protocol-access-description)

## 兼容性 / Compatibility

本包支持 .NET Standard 2.1。
This package supports .NET Standard 2.1.

## 使用方法 / Usage

```c#
using Dingtalk.Stream;
using System.Text.Json;

// 创建客户端实例 / Create client instance
var dingClient = OpenDingTalkStreamClientFactory.CreateClient("your appkey", "your appSecret");

// 同步处理事件 / Handle events synchronously
void ShowDocRaw(JsonDocument jsDoc)
{
    Console.WriteLine(jsDoc.RootElement.GetRawText());
}

// 异步处理事件 / Handle events asynchronously
async Task ShowDataInDoc(JsonDocument jsDoc)
{
    Console.WriteLine(jsDoc.RootElement.GetProperty("data").GetString());
}

// 建议使用异步方法，它不会阻塞 WebSocket 的持续推送数据
// It is recommended to use the asynchronous method, which will not block the continuous push data of WebSocket
dingClient.OnReceived += ShowDocRaw;
dingClient.OnReceivedAsync += ShowDataInDoc;

// 启动客户端 / Start the client
_ = dingClient.StartAsync();

Console.ReadLine();

// 停止客户端 / Stop the client
await dingClient.StopAsync();
```

## ASP.NET Core 集成 / ASP.NET Core Integration

你可以通过依赖注入将客户端集成到 ASP.NET Core 应用中：
You can also integrate the client into your ASP.NET Core application using dependency injection:

```csharp
// 在 Program.cs 或 Startup.cs 中注册服务
// Register service in Program.cs or Startup.cs
builder.Services.AddDingTalkStream("your appkey", "your appSecret");

// 在你的控制器或服务中使用
// Use in your controller or service
public class YourService
{
    private readonly IOpenDingTalkStreamClient _client;

    public YourService(IOpenDingTalkStreamClient client)
    {
        _client = client;
    }

    public void SetupHandlers()
    {
        _client.OnReceived += ShowDocRaw;
        _client.OnReceivedAsync += ShowDataInDoc;
    }
}
```

客户端将在应用启动时自动启动，并在应用关闭时自动停止。
The client will be automatically started when your application starts and stopped when it shuts down.

## 注意事项 / Attention

目前仅支持事件推送，本 SDK 尚未实现回调推送。
Only event push supported now, this SDK doesn't implement callback push yet.
