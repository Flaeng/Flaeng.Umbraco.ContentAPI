namespace Flaeng.Umbraco.ContentAPI.Tests.HalJson;

public class ExpandRequestTests : BaseTests
{
    readonly int ironmanId;

    public ExpandRequestTests()
    {
        var builder = UmbracoBuilder.Create();

        // Yes, I am stealing demo-data from this talk: https://www.youtube.com/watch?v=g8E1B7rTZBI 

        var friendsContentTypeId = builder.AddContentType("friends", "Friends");
        var photosContentTypeId = builder.AddContentType("photos", "Photos");
        var updatesContentTypeId = builder.AddContentType("updates", "Updates");
        var profilesContentTypeId = builder.AddContentType("profiles", "Profiles",
            childrenContentTypeId: new[] { friendsContentTypeId, photosContentTypeId, updatesContentTypeId });

        ironmanId = builder.AddContent("profiles", "ironman");
        var spideyId = builder.AddContent("profiles", "spidey");
        var blackwidow = builder.AddContent("profiles", "blackwidow");
        var pepper = builder.AddContent("profiles", "pepper");

        builder.AddContent("friends", "spidey", parentId: ironmanId);
        builder.AddContent("friends", "blackwidow", parentId: ironmanId);
        builder.AddContent("friends", "pepper", parentId: ironmanId);

        var update1Id = builder.AddContent("updates", "Working a new Iron Man suit!");
        var update2Id = builder.AddContent("updates", "New suit's gonna have a selfie stick! Oh yeah!");
        var update3Id = builder.AddContent("updates", "Selfie stick broke. Oh well. Back to the drawing board");

        Initialize(builder.Build());
    }

    [Fact]
    public void object_request_returns_links()
    {
        var result = Controller!.Get("profiles");

        var response = AssertAndGetCollectionResponse(result);

        Assert.Contains(response.Items, x => x.Name == "ironman");
        var ironman = response.Items.Single(x => x.Name == "ironman");
        Assert.NotNull(ironman.Links);
        Assert.NotEmpty(ironman.Links);
        Assert.Contains(ironman.Links, x => x.Key == "friends" && x.Value != null && x.Value.Href == $"/api/contentapi/profiles/{ironmanId}/friends");
        Assert.Contains(ironman.Links, x => x.Key == "photos" && x.Value != null && x.Value.Href == $"/api/contentapi/profiles/{ironmanId}/photos");
        Assert.Contains(ironman.Links, x => x.Key == "updates" && x.Value != null && x.Value.Href == $"/api/contentapi/profiles/{ironmanId}/updates");
    }

    // [Fact]
    // public void Can_expand()
    // {
    //     QueryString = "expand=photos";
    //     var result = Controller!.Get($"profiles/{ironmanId}");

    //     var response = AssertAndGetObjectResponse(result);
    //     var json = JsonSerializer.Serialize<ObjectResponse>(response, SerializerOptions);

    //     var token = JToken.Parse(json);
    //     Assert.Equal(ironmanId, token.Value<int>("id"));

    //     var photos = token.Value<JToken[]>("photos");
    //     Assert.NotNull(photos);
    //     Assert.NotEmpty(photos);
    // }

    // [Fact]
    // public void Can_expand_to_level_2()
    // {
    //     QueryString = "expand=photos.comments";
    //     var result = Controller!.Get($"profiles/{ironmanId}");

    //     var response = AssertAndGetObjectResponse(result);
    //     var json = JsonSerializer.Serialize<ObjectResponse>(response, SerializerOptions);

    //     var token = JToken.Parse(json);
    //     Assert.Equal(ironmanId, token.Value<int>("id"));

    //     var photos = token.Value<JToken[]>("photos");
    //     Assert.NotNull(photos);
    //     Assert.NotEmpty(photos);

    //     var photoComments = photos!.First().Value<JToken[]>("comments");
    //     Assert.NotNull(photoComments);
    //     Assert.NotEmpty(photoComments);
    // }

    // [Fact]
    // public void Can_expand_multiple()
    // {
    //     QueryString = "expand=photos,friends";
    //     var result = Controller!.Get($"profiles/{ironmanId}");

    //     var response = AssertAndGetObjectResponse(result);
    //     var json = JsonSerializer.Serialize<ObjectResponse>(response, SerializerOptions);

    //     var token = JToken.Parse(json);
    //     Assert.Equal(ironmanId, token.Value<int>("id"));

    //     var photos = token.Value<JToken[]>("photos");
    //     Assert.NotNull(photos);
    //     Assert.NotEmpty(photos);

    //     var friends = token.Value<JToken[]>("friends");
    //     Assert.NotNull(friends);
    //     Assert.NotEmpty(friends);
    // }

    // [Fact]
    // public void Can_expand_multiple_on_multiple_levels()
    // {
    //     QueryString = "expand=photos.comments,friends";
    //     var result = Controller!.Get($"profiles/{ironmanId}");

    //     var response = AssertAndGetObjectResponse(result);
    //     var json = JsonSerializer.Serialize<ObjectResponse>(response, SerializerOptions);
    //     Debug.WriteLine(json);

    //     var token = JToken.Parse(json);
    //     Assert.Equal(ironmanId, token.Value<int>("id"));

    //     var photos = token.Value<JToken[]>("photos");
    //     Assert.NotNull(photos);
    //     Assert.NotEmpty(photos);

    //     var photoComments = photos!.First().Value<JToken[]>("comments");
    //     Assert.NotNull(photoComments);
    //     Assert.NotEmpty(photoComments);

    //     var friends = token.Value<JToken[]>("friends");
    //     Assert.NotNull(friends);
    //     Assert.NotEmpty(friends);
    // }

    // [Fact]
    // public void Can_expand_properties()
    // {
    // }

    // [Fact]
    // public void Can_expand_child_nodes()
    // {
    // }

}