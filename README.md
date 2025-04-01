# **Dingtalk Stream Mode .NET Unofficial SDK**

Unofficial .NET implementation of Dingtalk stream mode.

It handles the authcation, keep-alive and response of push event.

## About Stream Mode

See Dingtalk doc here:

[Stream Mode Doc](https://open.dingtalk.com/document/resourcedownload/introduction-to-stream-mode)

This SDK was develop under the guide of doc :

[Guide Doc](https://open.dingtalk.com/document/direction/stream-mode-protocol-access-description)

## Usage

```c#
using Dingtalk.Stream;
using System.Text.Json;

var dingClient = OpenDingTalkStreamClientFactory.CreateClient("your appkey", "your appSecret");

void ShowDocRaw(JsonDocument jsDoc)
{
    Console.WriteLine(jsDoc.RootElement.GetRawText());
}

async Task ShowDataInDoc(JsonDocument jsDoc)
{
    Console.WriteLine(jsDoc.RootElement.GetProperty("data").GetString());
}

//It is recommended to use the asynchronous method, which will not block the continuous push data of Websocket.
dingClient.OnReceived += ShowDocRaw;
dingClient.OnReceivedAsync += ShowDataInDoc;

_ = dingClient.StartAsync();

Console.ReadLine();

await dingClient.StopAsync();
```

## ASP.NET Core Integration

You can also integrate the client into your ASP.NET Core application using dependency injection:

```csharp
// In Program.cs or Startup.cs
builder.Services.AddDingTalkStream("your appkey", "your appSecret");

// In your controller or service
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

The client will be automatically started when your application starts and stopped when it shuts down.

## Attention

Only event push supported now, this SDK doesn't implement callback push yet.
