using Flaeng.Umbraco.ContentAPI.Handlers;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Moq;

using Umbraco.Cms.Core.Web;

namespace Flaeng.Umbraco.ContentAPI.Tests.Handlers;

public class Expander
{
    public class can_convert_expand_string_to_array_when : BaseTests
    {
        [Fact]
        public void is_empty()
        {
            var expander = new DefaultResponseBuilder(
                GetDummyUmbracoContextAccessor(),
                new Mock<IHttpContextAccessor>().Object,
                new Mock<ILinkPopulator>().Object);

            var arr = expander.ConvertExpandStringToArray("");

            Assert.Equal(new string[0], arr);
        }
        [Fact]
        public void is_friends()
        {
            var expander = new DefaultResponseBuilder(
                GetDummyUmbracoContextAccessor(),
                new Mock<IHttpContextAccessor>().Object,
                new Mock<ILinkPopulator>().Object);

            var arr = expander.ConvertExpandStringToArray("friends");

            Assert.Equal(new[] { "friends" }, arr);
        }
        [Fact]
        public void is_friends_and_photos()
        {
            var expander = new DefaultResponseBuilder(
                GetDummyUmbracoContextAccessor(),
                new Mock<IHttpContextAccessor>().Object,
                new Mock<ILinkPopulator>().Object);

            var arr = expander.ConvertExpandStringToArray("friends,photos");

            Assert.Equal(new[] { "friends", "photos" }, arr);
        }
        [Fact]
        public void is_friends_with_photos()
        {
            var expander = new DefaultResponseBuilder(
                GetDummyUmbracoContextAccessor(),
                new Mock<IHttpContextAccessor>().Object,
                new Mock<ILinkPopulator>().Object);

            var arr = expander.ConvertExpandStringToArray("friends.photos");

            Assert.Equal(new[] { "friends", "friends.photos" }, arr);
        }
        [Fact]
        public void is_friends_with_photos_and_photos()
        {
            var expander = new DefaultResponseBuilder(
                GetDummyUmbracoContextAccessor(),
                new Mock<IHttpContextAccessor>().Object,
                new Mock<ILinkPopulator>().Object);

            var arr = expander.ConvertExpandStringToArray("friends.photos,photos");

            Assert.Equal(new[] { "friends", "friends.photos", "photos" }, arr);
        }
    }
}