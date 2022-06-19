using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Flaeng.Umbraco.ContentAPI;

public static class IServiceCollectionExtensions
{
    public static void AddContentAPI(this IServiceCollection services, Action<ContentApiOptions> configure = null)
    {
        configure ??= x => { };
        services.Configure<ContentApiOptions>(configure);

        services.AddScoped<IFilterHandler, DefaultFilterHandler>();
        services.AddScoped<ILinkPopulator, DefaultLinkPopulator>();
        services.AddScoped<IResponseBuilder, DefaultResponseBuilder>();
    }
}

public static class IApplicationBuilderExtensions
{
    public static void UseContentAPI(this IApplicationBuilder builder)
    {
    }
}