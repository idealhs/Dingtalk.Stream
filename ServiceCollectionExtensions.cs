using Microsoft.Extensions.DependencyInjection;

namespace Dingtalk.Stream;

/// <summary>
/// 服务集合扩展方法
/// Service collection extension methods
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加钉钉 Stream 服务
    /// Add DingTalk Stream service
    /// </summary>
    /// <param name="services">服务集合 / Service collection</param>
    /// <param name="appKey">应用密钥 / App key</param>
    /// <param name="appSecret">应用密钥 / App secret</param>
    /// <returns>服务集合 / Service collection</returns>
    public static IServiceCollection AddDingTalkStream(
        this IServiceCollection services,
        string appKey,
        string appSecret)
    {
        services.AddSingleton<IOpenDingTalkStreamClient>(_ => 
            OpenDingTalkStreamClientFactory.CreateClient(appKey, appSecret));
        services.AddHostedService<DingTalkStreamHostedService>();
        return services;
    }
} 