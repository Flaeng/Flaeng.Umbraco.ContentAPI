namespace Flaeng.Umbraco.ContentAPI.Tests.HalJson;

public class UmbracoDictionaryRequestTests : BaseTests
{
    public UmbracoDictionaryRequestTests()
    {
        var builder = UmbracoBuilder.Create();

        builder.AddDictionaryItem("unknown_error_dialog_title", "Unknown Error");
        builder.AddDictionaryItem("unknown_error_dialog_body", "An unknown error occured");

        Initialize(builder.Build(), opts => opts.ExposeTranslationDictionary());
    }

    [Fact]
    public void Can_get_dictionary()
    {
        // var result = Controller!.Get("dictionary");

        // var response = AssertAndGetCollectionResponse(result);

        // Assert.Equal(2, response!.TotalItemCount);
    }
    
}