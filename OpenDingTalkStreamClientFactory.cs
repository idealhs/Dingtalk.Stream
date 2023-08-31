namespace Dingtalk.Stream
{
    public static class OpenDingTalkStreamClientFactory
    {
        public static IOpenDingTalkStreamClient CreateClient(string appKey, string appSecret)
        {
            return new OpenDingTalkStreamClient(appKey, appSecret);
        }
    }
}