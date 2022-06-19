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
        List<UmbracoProperty>? properties = null,
        Action<Mock<IPublishedContent>>? configure = null
        );
    int AddMediaType(
        string alias,
        Action<Mock<IPublishedContentType>>? configure = null
        );
    int AddMedia(
        string contentTypeAlias,
        string name,
        int? parentId = null,
        List<UmbracoProperty>? properties = null,
        Action<Mock<IPublishedContent>>? configure = null
        );
    int AddMemberType(
        string alias,
        Action<Mock<IPublishedContentType>>? configure = null
        );
    int AddMember(
        string contentTypeAlias,
        string name,
        int? parentId = null,
        List<UmbracoProperty>? properties = null,
        Action<Mock<IPublishedContent>>? configure = null
        );
    int AddMemberGroupType(
        string alias,
        Action<Mock<IPublishedContentType>>? configure = null
        );
    int AddMemberGroup(
        int memberId,
        int memberGroupId
        );
    int AddUserType(
        string alias,
        Action<Mock<IPublishedContentType>>? configure = null
        );
    int AddUser(
        string userTypeAlias,
        string name,
        List<UmbracoProperty>? properties = null,
        Action<Mock<IPublishedContent>>? configure = null
        );
    int AddForm(
        string alias,
        string name,
        List<object> formFields,
        Action<Mock<IPublishedContentType>>? configure = null
        );
    void AddDictionaryItem(string key, string text);
    IUmbracoApp Build();
}
public class UmbracoBuilder : IUmbracoApp, IUmbracoBuilder
{
    private readonly Mock<UmbracoHelper> umbracoHelper = new Mock<UmbracoHelper>();
    public UmbracoHelper UmbracoHelper => umbracoHelper.Object;

    private readonly Mock<IUmbracoContext> umbracoContext = new Mock<IUmbracoContext>();
    public IUmbracoContext UmbracoContext => umbracoContext.Object;

    private readonly Mock<IAppPolicyCache> runtimeCache = new Mock<IAppPolicyCache>();
    private readonly Mock<IRequestCache> requestCache = new Mock<IRequestCache>();
    private readonly Mock<IsolatedCaches> isolatedCaches = new Mock<IsolatedCaches>(new Mock<Func<Type, IAppPolicyCache>>().Object);
    public AppCaches AppCaches { get; }

    private readonly Mock<IPublishedContentCache> contentCache = new Mock<IPublishedContentCache>();

    private readonly Mock<IContentTypeService> contentTypeServiceMock = new Mock<IContentTypeService>();
    public IContentTypeService ContentTypeService => contentTypeServiceMock.Object;

    private readonly Mock<IPublishedMediaCache> mediaCacheMock = new();
    public IPublishedMediaCache MediaCache => mediaCacheMock.Object;

    public static IUmbracoBuilder Create() => new UmbracoBuilder();

    public IUmbracoApp Build() => this;

    readonly Dictionary<int, IContentType> contentTypeById = new();
    readonly Dictionary<string, IPublishedContentType> contentTypeList = new();
    readonly Dictionary<string, IList<IPublishedContent>> contentByContentTypeList = new();
    readonly Dictionary<int, IPublishedContent> contentById = new();
    readonly Dictionary<int, IList<IPublishedContent>> contentByParentId = new();
    readonly Dictionary<string, string> CultureDictionary = new();
    
    private UmbracoBuilder()
    {
        umbracoContext.Setup(x => x.Content).Returns(contentCache.Object);

        runtimeCache
            .Setup(x => x.Get(It.IsAny<string>(), It.IsAny<Func<object?>>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>(), It.IsAny<string[]>()))
            .Returns<string, Func<object?>, TimeSpan?, bool, string[]>((p1, p2, p3, p4, p5) => p2.Invoke());
        AppCaches = new AppCaches(runtimeCache.Object, requestCache.Object, isolatedCaches.Object);

        umbracoContext.Setup(x => x.Media).Returns(mediaCacheMock.Object);

        // umbracoHelper
        //     .Setup(x => x.GetDictionaryValue(It.IsAny<string>()))
        //     .Returns<string>(key => CultureDictionary.TryGetValue(key, out var value) ? value : String.Empty);
        // umbracoHelper
        //     .Setup(x => x.GetDictionaryValue(It.IsAny<string>(), It.IsAny<string>()))
        //     .Returns<string, string>((key, altText) => CultureDictionary.TryGetValue(key, out var value) ? value : altText);
    }

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
        contentTypeList.Add(alias.ToLower(), contentType.Object);
        contentCache.Setup(x => x.GetContentType(alias)).Returns(contentType.Object);
        contentCache.Setup(x => x.GetByContentType(contentType.Object)).Returns(() => contentByContentTypeList[alias.ToLower()]);

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
        List<UmbracoProperty>? properties = null,
        Action<Mock<IPublishedContent>>? configure = null
        )
    {
        if (!contentTypeList.TryGetValue(contentTypeAlias.ToLower(), out var contentType))
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

        List<UmbracoProperty> propertyList = new();

        propertyList.Add(new UmbracoProperty("Id", contentIdCounter));
        propertyList.Add(new UmbracoProperty("Name", name));
        propertyList.Add(new UmbracoProperty("Level", level));

        if (properties != null)
            propertyList.AddRange(properties);

        Dictionary<string, UmbracoProperty> propertiesLookup = propertyList.ToDictionary(x => x.Alias.ToLower());

        content
            .Setup(x => x.GetProperty(It.IsAny<string>()))
            .Returns<string>(p1 => propertiesLookup.TryGetValue(p1, out var result) ? result : null);

        if (!contentByContentTypeList.TryGetValue(contentTypeAlias.ToLower(), out var contentList))
            contentByContentTypeList.Add(contentTypeAlias.ToLower(), contentList = new List<IPublishedContent>());
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

    public int AddMediaType(string alias, Action<Mock<IPublishedContentType>>? configure = null)
    {
        throw new NotImplementedException();
    }

    public int AddMedia(string contentTypeAlias, string name, int? parentId = null, List<UmbracoProperty>? properties = null, Action<Mock<IPublishedContent>>? configure = null)
    {
        throw new NotImplementedException();
    }

    public int AddMemberType(string alias, Action<Mock<IPublishedContentType>>? configure = null)
    {
        throw new NotImplementedException();
    }

    public int AddMember(string contentTypeAlias, string name, int? parentId = null, List<UmbracoProperty>? properties = null, Action<Mock<IPublishedContent>>? configure = null)
    {
        throw new NotImplementedException();
    }

    public int AddMemberGroupType(string alias, Action<Mock<IPublishedContentType>>? configure = null)
    {
        throw new NotImplementedException();
    }

    public int AddMemberGroup(int memberId, int memberGroupId)
    {
        throw new NotImplementedException();
    }

    public int AddUserType(string alias, Action<Mock<IPublishedContentType>>? configure = null)
    {
        throw new NotImplementedException();
    }

    public int AddUser(string userTypeAlias, string name, List<UmbracoProperty>? properties = null, Action<Mock<IPublishedContent>>? configure = null)
    {
        throw new NotImplementedException();
    }

    public int AddForm(string alias, string name, List<object> formFields, Action<Mock<IPublishedContentType>>? configure = null)
    {
        throw new NotImplementedException();
    }

    public void AddDictionaryItem(string key, string text) => CultureDictionary.Add(key, text);

    public class UmbracoProperty : IPublishedProperty
    {
        public IPublishedPropertyType PropertyType => throw new NotImplementedException();
        public string Alias { get; init; }

        readonly Dictionary<string, object?> values = new Dictionary<string, object?>();

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