using System;

using Flaeng.Umbraco.ContentAPI.Handlers;
using Flaeng.Umbraco.ContentAPI.Options;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

public static class IServiceCollectionExtensions
{
    public static void AddContentAPI(this IServiceCollection services, Action<IContentApiOptions> configure = null)
    {
        services.AddOptions<ContentApiOptions>();
        if (configure != null)
            services.Configure<ContentApiOptions>(configure);

        //Handlers
        services.AddScoped<IFilterInterpreter, DefaultFilterInterpreter>();
        services.AddScoped<ILinkFormatter, DefaultLinkFormatter>();
        services.AddScoped<ILinkPopulator, DefaultLinkPopulator>();
        services.AddScoped<IRequestInterpreter, DefaultRequestInterpreter>();
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