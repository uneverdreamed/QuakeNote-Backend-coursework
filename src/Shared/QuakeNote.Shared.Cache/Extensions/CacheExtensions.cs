using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace QuakeNote.Shared.Cache.Extensions;

public static class CacheExtensions
{
    public static IServiceCollection AddQuakeNoteCache(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("Redis");
        var host = section["Host"] ?? "localhost";
        var port = int.Parse(section["Port"] ?? "6379");
        var password = section["Password"]
            ?? throw new InvalidOperationException("Redis:Password is not configured.");

        var options = new ConfigurationOptions
        {
            EndPoints = { { host, port } },
            Password = password,
            AbortOnConnectFail = false,
            ConnectRetry = 3
        };

        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(options));
        services.AddSingleton<ICacheService, RedisCacheService>();
        return services;
    }
}