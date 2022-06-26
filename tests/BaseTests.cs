using Moq;

using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.UmbracoContext;

namespace Flaeng.Umbraco.ContentAPI.Tests;

public abstract class BaseTests
{
    protected IUmbracoContextAccessor GetDummyUmbracoContextAccessor()
    {
        var mock = new Mock<IUmbracoContextAccessor>();
        IUmbracoContext context = null!;
        mock.Setup(x => x.TryGetUmbracoContext(out context!)).Returns(true);
        return mock.Object;
    }
}