using System;

using Moq;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Dictionary;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Templates;
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
        string name,
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
    public UmbracoHelper UmbracoHelper { get; init; }

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
    readonly Dictionary<string, object> CultureDictionary = new();

    private UmbracoBuilder()
    {
        var cultureDictionaryFactory = new Mock<ICultureDictionaryFactory>();
        var cultureDictionary = new Mock<ICultureDictionary>();
        cultureDictionary
            .Setup(x => x.GetChildren(It.IsAny<string>()))
            .Returns<string>(key => (Dictionary<string, string>)CultureDictionary[key]);
        cultureDictionaryFactory
            .Setup(x => x.CreateDictionary())
            .Returns(() => cultureDictionary.Object);

        var componentRenderer = new Mock<IUmbracoComponentRenderer>();
        var contentQuery = new Mock<IPublishedContentQuery>();

        contentQuery
            .Setup(x => x.Content(It.IsAny<int>()))
            .Returns<int>(i => contentById.TryGetValue(i, out var value) ? value : default);

        UmbracoHelper = new UmbracoHelper(
            cultureDictionaryFactory.Object,
            componentRenderer.Object,
            contentQuery.Object
            );

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
        string name,
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

            contentTypeServiceMock
                .Setup(x => x.GetAll())
                .Returns(contentTypeList.Values.Select(ConvertPublishedContentTypeToContentType(name)).ToList());
        }

        return contentTypeIdCounter++;
    }

    private Func<IPublishedContentType, IContentType> ConvertPublishedContentTypeToContentType(string name)
    {
        return contentType =>
        {
            var result = new Mock<IContentType>();
            result.Setup(x => x.Alias).Returns(contentType.Alias);
            result.Setup(x => x.Name).Returns(name);
            return result.Object;
        };
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

        // var idPropertyType = new Mock<IPublishedPropertyType>();
        // var namePropertyType = new Mock<IPublishedPropertyType>();
        // var levelPropertyType = new Mock<IPublishedPropertyType>();

        // propertyList.Add(new UmbracoProperty(idPropertyType.Object, "Id", contentIdCounter));
        // propertyList.Add(new UmbracoProperty(namePropertyType.Object, "Name", name));
        // propertyList.Add(new UmbracoProperty(levelPropertyType.Object, "Level", level));

        if (properties != null)
            propertyList.AddRange(properties);

        Dictionary<string, UmbracoProperty> propertiesLookup = propertyList.ToDictionary(x => x.Alias.ToLower());

        content
            .Setup(x => x.GetProperty(It.IsAny<string>()))
            .Returns<string>(p1 => propertiesLookup.TryGetValue(p1, out var result) ? result : null);

        content
            .Setup(x => x.Properties)
            .Returns(propertiesLookup.Values);

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

    public abstract class UmbracoProperty : IPublishedProperty
    {
        public abstract IPublishedPropertyType PropertyType { get; init; }
        public abstract string Alias { get; init; }

        public abstract object? GetSourceValue(string? culture = null, string? segment = null);
        public abstract object? GetValue(string? culture = null, string? segment = null);
        public abstract object? GetXPathValue(string? culture = null, string? segment = null);
        public abstract bool HasValue(string? culture = null, string? segment = null);
    }
    public class UmbracoProperty<T> : UmbracoProperty, IPublishedProperty
    {
        public override IPublishedPropertyType PropertyType { get; init; }
        public override string Alias { get; init; }
        readonly Dictionary<string, object?> values = new Dictionary<string, object?>();

        public UmbracoProperty(string alias, T value)
        {
            values.Add(String.Empty, value);

            this.Alias = alias;
            var propertyType = new Mock<IPublishedPropertyType>();
            propertyType.Setup(x => x.ClrType).Returns(typeof(T).GetType());
            propertyType.Setup(x => x.ModelClrType).Returns(typeof(T).GetType());
            this.PropertyType = propertyType.Object;
        }

        public override object? GetSourceValue(string? culture = null, string? segment = null)
            => throw new NotImplementedException();

        public override object? GetValue(string? culture = null, string? segment = null)
            => values[culture ?? String.Empty];

        public override object? GetXPathValue(string? culture = null, string? segment = null)
            => throw new NotImplementedException();

        public override bool HasValue(string? culture = null, string? segment = null)
            => values.ContainsKey(culture ?? String.Empty);
    }
}