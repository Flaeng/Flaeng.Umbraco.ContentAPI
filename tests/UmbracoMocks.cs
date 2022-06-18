using System;
using Moq;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common;
using static Flaeng.Umbraco.ContentAPI.Tests.UmbracoBuilder;

namespace Flaeng.Umbraco.ContentAPI.Tests;

public interface IUmbracoApp
{
    UmbracoHelper UmbracoHelper { get; }
    IUmbracoContext UmbracoContext { get; }
    AppCaches AppCaches { get; }
    IContentTypeService ContentTypeService { get; }
}
public interface IUmbracoBuilder
{
    int AddContentType(
        string alias,
        int[]? childrenContentTypeId = null,
        Action<Mock<IPublishedContentType>>? configure = null
        );
    int AddContent(
        string contentTypeAlias,
        string name,
        int? parentId = null,
        Action<Mock<IPublishedContent>>? configure = null,
        Action<List<UmbracoProperty>>? configureProperties = null
        );
    IUmbracoApp Build();
}
public class UmbracoBuilder : IUmbracoApp, IUmbracoBuilder
{
    private Mock<UmbracoHelper> umbracoHelper = new Mock<UmbracoHelper>();
    public UmbracoHelper UmbracoHelper => umbracoHelper.Object;

    private Mock<IUmbracoContext> umbracoContext = new Mock<IUmbracoContext>();
    public IUmbracoContext UmbracoContext => umbracoContext.Object;

    private Mock<IAppPolicyCache> runtimeCache = new Mock<IAppPolicyCache>();
    private Mock<IRequestCache> requestCache = new Mock<IRequestCache>();
    private Mock<IsolatedCaches> isolatedCaches = new Mock<IsolatedCaches>(new Mock<Func<Type, IAppPolicyCache>>().Object);
    public AppCaches AppCaches { get; }

    private Mock<IPublishedContentCache> contentCache = new Mock<IPublishedContentCache>();

    private Mock<IContentTypeService> contentTypeServiceMock = new Mock<IContentTypeService>();
    public IContentTypeService ContentTypeService => contentTypeServiceMock.Object;

    public static IUmbracoBuilder Create() => new UmbracoBuilder();
    private UmbracoBuilder()
    {
        umbracoContext.Setup(x => x.Content).Returns(contentCache.Object);

        runtimeCache
            .Setup(x => x.Get(It.IsAny<string>(), It.IsAny<Func<object?>>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>(), It.IsAny<string[]>()))
            .Returns<string, Func<object?>, TimeSpan?, bool, string[]>((p1, p2, p3, p4, p5) => p2.Invoke());
        AppCaches = new AppCaches(runtimeCache.Object, requestCache.Object, isolatedCaches.Object);
    }

    public IUmbracoApp Build() => this;

    readonly Dictionary<int, IContentType> contentTypeById = new();
    readonly Dictionary<string, IPublishedContentType> contentTypeList = new();
    readonly Dictionary<string, IList<IPublishedContent>> contentByContentTypeList = new();
    readonly Dictionary<int, IPublishedContent> contentById = new();
    readonly Dictionary<int, IList<IPublishedContent>> contentByParentId = new();

    int contentTypeIdCounter = 1;
    public int AddContentType(
        string alias,
        int[]? childrenContentTypeId = null,
        Action<Mock<IPublishedContentType>>? configure = null
        )
    {
        var contentType = new Mock<IPublishedContentType>();
        contentType.Setup(x => x.Id).Returns(contentTypeIdCounter);
        contentType.Setup(x => x.Alias).Returns(alias);
        configure?.Invoke(contentType);
        contentTypeList.Add(alias, contentType.Object);
        contentCache.Setup(x => x.GetContentType(alias)).Returns(contentType.Object);
        contentCache.Setup(x => x.GetByContentType(contentType.Object)).Returns(() => contentByContentTypeList[alias]);

        var contentTypeModel = new Mock<IContentType>();
        contentTypeModel.Setup(x => x.Id).Returns(contentTypeIdCounter);
        contentTypeModel.Setup(x => x.Alias).Returns(alias);
        contentTypeById.Add(contentTypeIdCounter, contentTypeModel.Object);

        if (childrenContentTypeId != null)
        {
            Func<IEnumerable<IContentType>> getChildren = ()
                => childrenContentTypeId.Select(x => contentTypeById[x]).ToList();

            contentTypeServiceMock
                .Setup(x => x.GetChildren(contentTypeIdCounter))
                .Returns(getChildren);
        }

        return contentTypeIdCounter++;
    }

    int contentIdCounter = 1;
    public int AddContent(
        string contentTypeAlias,
        string name,
        int? parentId = null,
        Action<Mock<IPublishedContent>>? configure = null,
        Action<List<UmbracoProperty>>? configureProperties = null
        )
    {
        if (!contentTypeList.TryGetValue(contentTypeAlias, out var contentType))
            throw new Exception($"Missing ContentType with alias '{contentTypeAlias}'");

        var content = new Mock<IPublishedContent>();
        content.Setup(x => x.Id).Returns(contentIdCounter);
        content.Setup(x => x.Name).Returns(name);

        if (parentId.HasValue)
            content.Setup(x => x.Parent).Returns(contentById[parentId.Value]);

        var level = getLevel(parentId);
        content.Setup(x => x.Level).Returns(level);

        Func<IEnumerable<IPublishedContent>> getChildrenMethod(int contentIdCounter)
            => () => contentByParentId[contentIdCounter];

        content.Setup(x => x.Children).Returns(getChildrenMethod(contentIdCounter));

        content.Setup(x => x.ContentType).Returns(contentType);
        configure?.Invoke(content);
        contentCache.Setup(x => x.GetById(contentIdCounter)).Returns(content.Object);

        List<UmbracoProperty> properties = new();

        properties.Add(new UmbracoProperty("Id", contentIdCounter));
        properties.Add(new UmbracoProperty("Name", name));
        properties.Add(new UmbracoProperty("Level", level));

        configureProperties?.Invoke(properties);

        Dictionary<string, UmbracoProperty> propertiesLookup = properties.ToDictionary(x => x.Alias.ToLower());

        content
            .Setup(x => x.GetProperty(It.IsAny<string>()))
            .Returns<string>(p1 => propertiesLookup.TryGetValue(p1, out var result) ? result : null);

        if (!contentByContentTypeList.TryGetValue(contentTypeAlias, out var contentList))
            contentByContentTypeList.Add(contentTypeAlias, contentList = new List<IPublishedContent>());
        contentList.Add(content.Object);

        contentById.Add(contentIdCounter, content.Object);

        if (parentId.HasValue)
        {
            if (!contentByParentId.TryGetValue(parentId.Value, out var parentChildList))
                contentByParentId.Add(parentId.Value, parentChildList = new List<IPublishedContent>());
            parentChildList.Add(content.Object);
        }

        return contentIdCounter++;
    }

    private int getLevel(int? parentId)
        => parentId.HasValue ? contentById[parentId.Value].Level + 1 : 0;

    public class UmbracoProperty : IPublishedProperty
    {
        public IPublishedPropertyType PropertyType => throw new NotImplementedException();
        public string Alias { get; init; }

        Dictionary<string, object?> values = new Dictionary<string, object?>();

        public UmbracoProperty(string alias)
        {
            this.Alias = alias;
        }

        public UmbracoProperty(string alias, object? value)
        {
            this.Alias = alias;
            values.Add(String.Empty, value);
        }

        public object? GetSourceValue(string? culture = null, string? segment = null) => throw new NotImplementedException();
        public object? GetValue(string? culture = null, string? segment = null) => values[culture ?? String.Empty];
        public object? GetXPathValue(string? culture = null, string? segment = null) => throw new NotImplementedException();
        public bool HasValue(string? culture = null, string? segment = null) => values.ContainsKey(culture ?? String.Empty);
    }
}