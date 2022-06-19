using System.Text.Json;

using Flaeng.Umbraco.ContentAPI.Converters;
using Flaeng.Umbraco.ContentAPI.Models;

using Moq;

using Newtonsoft.Json.Linq;

using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Strings;

namespace Flaeng.Umbraco.ContentAPI.Tests.HalJson;

public class ContentPropertyTests : BaseTests
{
    readonly JToken token;

    public ContentPropertyTests()
    {
        var content = new Mock<IPublishedContent>();
        var properties = new List<IPublishedProperty>();
        content.Setup(x => x.ContentType).Returns(new Mock<IPublishedContentType>().Object);
        content.Setup(x => x.Properties).Returns(properties);

        static IPublishedProperty CreateProperty(string alias, object value)
        {
            var mock = new Mock<IPublishedProperty>();
            mock.Setup(x => x.Alias).Returns(alias);
            mock.Setup(x => x.GetValue(It.IsAny<string>(), It.IsAny<string>())).Returns(value);
            return mock.Object;
        }

        properties.Add(CreateProperty("string", "this is text"));
        properties.Add(CreateProperty("number", 42));
        properties.Add(CreateProperty("date", new DateTime(2022, 6, 19, 9, 41, 42)));
        properties.Add(CreateProperty("html_encoded_string", new HtmlEncodedString("<p>this is a paragraph</p>")));

        var response = new ObjectResponse(content.Object);
        var json = JsonSerializer.Serialize<ObjectResponse>(response, SerializerOptions);
        token = JToken.Parse(json);
    }

    [Fact]
    public void can_parse_string()
    {
        Assert.Equal("this is text", token["string"]);
    }

    [Fact]
    public void can_parse_int()
    {
        Assert.Equal(42, token["number"]);
    }

    [Fact]
    public void can_parse_date()
    {
        Assert.Equal("2022-06-19T09:41:42", token["date"]);
    }

    [Fact]
    public void can_parse_html_encoded_string()
    {
        Assert.Equal("<p>this is a paragraph</p>", token["html_encoded_string"]);
    }

}