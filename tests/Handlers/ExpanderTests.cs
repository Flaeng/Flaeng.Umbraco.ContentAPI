using Flaeng.Umbraco.ContentAPI.Handlers;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Moq;

namespace Flaeng.Umbraco.ContentAPI.Tests.Handlers;

public class Expander
{
    public class can_convert_expand_string_to_array_when
    {
        [Fact]
        public void is_empty()
        {
            var expander = new DefaultExpander(
                new Mock<ILogger<DefaultExpander>>().Object,
                new Mock<IHttpContextAccessor>().Object);

            var arr = expander.ConvertExpandStringToArray("");

            Assert.Equal(new string[0], arr);
        }
        [Fact]
        public void is_friends()
        {
            var expander = new DefaultExpander(
                new Mock<ILogger<DefaultExpander>>().Object,
                new Mock<IHttpContextAccessor>().Object);

            var arr = expander.ConvertExpandStringToArray("friends");

            Assert.Equal(new[] { "friends" }, arr);
        }
        [Fact]
        public void is_friends_and_photos()
        {
            var expander = new DefaultExpander(
                new Mock<ILogger<DefaultExpander>>().Object,
                new Mock<IHttpContextAccessor>().Object);

            var arr = expander.ConvertExpandStringToArray("friends,photos");

            Assert.Equal(new[] { "friends", "photos" }, arr);
        }
        [Fact]
        public void is_friends_with_photos()
        {
            var expander = new DefaultExpander(
                new Mock<ILogger<DefaultExpander>>().Object,
                new Mock<IHttpContextAccessor>().Object);

            var arr = expander.ConvertExpandStringToArray("friends.photos");

            Assert.Equal(new[] { "friends", "friends.photos" }, arr);
        }
        [Fact]
        public void is_friends_with_photos_and_photos()
        {
            var expander = new DefaultExpander(
                new Mock<ILogger<DefaultExpander>>().Object,
                new Mock<IHttpContextAccessor>().Object);

            var arr = expander.ConvertExpandStringToArray("friends.photos,photos");

            Assert.Equal(new[] { "friends", "friends.photos", "photos" }, arr);
        }
    }
}