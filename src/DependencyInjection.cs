using System;

using Flaeng.Umbraco.ContentAPI;
using Flaeng.Umbraco.ContentAPI.Options;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

public static class IServiceCollectionExtensions
{
    public static void AddContentAPI(this IServiceCollection services, Action<IContentApiOptions> configure = null)
    {
        configure ??= x => { };
        services.Configure<IContentApiOptions>(configure);

        services.AddScoped<IFilterHandler, DefaultFilterHandler>();
        services.AddScoped<ILinkFormatter, DefaultLinkFormatter>();
        services.AddScoped<ILinkPopulator, DefaultLinkPopulator>();
        services.AddScoped<IResponseBuilder, DefaultResponseBuilder>();
    }
}

public static class IApplicationBuilderExtensions
{
    public static void UseContentAPI(this IApplicationBuilder builder)
    {
        // Not sure what to put here yet.
        // But we might as well make it now so we have it in the future
    }
}