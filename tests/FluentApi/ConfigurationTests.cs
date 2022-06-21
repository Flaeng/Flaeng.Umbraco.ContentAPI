using System.Text;

using Flaeng.Umbraco.ContentAPI.Options;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Flaeng.Umbraco.ContentAPI.Tests;

public class ConfigurationTests
{
    readonly IOptions<ContentApiOptions>? bindedOptions;
    readonly IOptions<ContentApiOptions>? configuredOptions;
    readonly IOptions<ContentApiOptions>? gottenOptions;
    readonly string json = @"
        {
            ""ContentApiOptions"": {
                ""EnableSmartCaching"": true,
                ""UmbracoOptions"": {
                    ""ExposeUsers"": true
                },
                ""CreationOptions"": [
                    {
                        ""ContentTypeAlias"": ""test""
                    }
                ]
            }
        }";

    public ConfigurationTests()
    {
        {
            var builder = WebApplication.CreateBuilder();
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            builder.Configuration.AddJsonStream(stream);
            builder.Services.AddOptions<ContentApiOptions>(IContentApiOptions.ConfigurationName);

            var section = builder.Configuration.GetSection(IContentApiOptions.ConfigurationName);
            builder.Services.Configure<ContentApiOptions>(section);

            var app = builder.Build();
            configuredOptions = app.Services.GetService<IOptions<ContentApiOptions>>();
            var opt = configuredOptions?.Value;
        }

        {
            var builder = WebApplication.CreateBuilder();
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            builder.Configuration.AddJsonStream(stream);
            builder.Services.AddOptions<ContentApiOptions>(IContentApiOptions.ConfigurationName);

            var options = new ContentApiOptions();
            builder.Configuration
                .GetSection(IContentApiOptions.ConfigurationName)
                .Bind(options);

            var app = builder.Build();
            bindedOptions = app.Services.GetService<IOptions<ContentApiOptions>>();
            var opt = bindedOptions?.Value;
        }

        {
            var builder = WebApplication.CreateBuilder();
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            builder.Configuration.AddJsonStream(stream);
            builder.Services.AddOptions<ContentApiOptions>(IContentApiOptions.ConfigurationName);

            var options = builder.Configuration
                .GetSection(IContentApiOptions.ConfigurationName)
                .Get<ContentApiOptions>();

            var app = builder.Build();
            gottenOptions = app.Services.GetService<IOptions<ContentApiOptions>>();
            var opt = bindedOptions?.Value;
        }
    }

    [Fact]
    public void Can_configure_options_from_config() => VerifyOptions(configuredOptions);

    [Fact]
    public void Can_bind_options_from_config() => VerifyOptions(bindedOptions);

    [Fact]
    public void Can_get_gotten_options_from_config() => VerifyOptions(gottenOptions);

    private void VerifyOptions(IOptions<ContentApiOptions>? opts)
    {
        Assert.NotNull(opts);
        Assert.NotNull(opts!.Value);

        Assert.False(opts.Value.EnableCaching);
        Assert.True(opts.Value.EnableSmartCaching);

        Assert.False(opts.Value.UmbracoOptions.ExposeMedia);
        Assert.True(opts.Value.UmbracoOptions.ExposeUsers);

        Assert.Contains(opts.Value.CreationOptions, x => x.ContentTypeAlias == "test");
        Assert.DoesNotContain(opts.Value.EditingOptions, x => x.ContentTypeAlias == "test");
    }

}