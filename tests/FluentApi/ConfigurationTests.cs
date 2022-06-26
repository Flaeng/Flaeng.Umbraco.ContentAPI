using System.Text;

using Flaeng.Umbraco.ContentAPI.Options;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Flaeng.Umbraco.ContentAPI.Tests;

public class ConfigurationTests
{
    readonly IOptions<ContentApiOptions>? options;
    readonly string json = @"
        {
            ""ContentApiOptions"": {
                ""EnableCaching"": true,
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
        var builder = WebApplication.CreateBuilder();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        builder.Configuration.AddJsonStream(stream);
        builder.Services.AddOptions<ContentApiOptions>(IContentApiOptions.ConfigurationName);

        var section = builder.Configuration.GetSection(IContentApiOptions.ConfigurationName);
        builder.Services.Configure<ContentApiOptions>(section);

        var app = builder.Build();
        options = app.Services.GetService<IOptions<ContentApiOptions>>();
    }

    [Fact]
    public void Can_do_simple_configuration_of_options()
    {
        Assert.NotNull(options);
        Assert.NotNull(options!.Value);

        Assert.True(options.Value.EnableCaching);
    }

    // [Fact]
    // public void Can_configure_umbraco_options()
    // {
    //     Assert.NotNull(options);
    //     Assert.NotNull(options!.Value);

    //     Assert.False(options.Value.UmbracoOptions.ExposeMedia);
    //     Assert.True(options.Value.UmbracoOptions.ExposeUsers);
    // }

    // [Fact]
    // public void Can_configure_creation_options()
    // {
    //     Assert.NotNull(options);
    //     Assert.NotNull(options!.Value);

    //     Assert.Contains(options.Value.CreationOptions, x => x.ContentTypeAlias == "test");
    //     Assert.DoesNotContain(options.Value.EditingOptions, x => x.ContentTypeAlias == "test");
    // }

}