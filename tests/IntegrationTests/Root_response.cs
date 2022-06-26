namespace Flaeng.Umbraco.ContentAPI.Tests.IntegrationTests;

public class Root_response : BaseIntegrationTests
{

    [Fact]
    public void Has_self_link()
    {
        Assert.Equal("/api/contentapi", rootRequestResponse["_links"]!.Value<string>("self"));
    }

    [Fact]
    public void Has_employee_href()
    {
        Assert.Equal("/api/contentapi/employee", rootRequestResponse["_links"]!["employee"]!.Value<string>("href"));
    }

    [Fact]
    public void Has_scrumteam_href()
    {
        Assert.Equal("/api/contentapi/scrumteam", rootRequestResponse["_links"]!["scrumteam"]!.Value<string>("href"));
    }

    [Fact]
    public void Has_project_href()
    {
        Assert.Equal("/api/contentapi/project", rootRequestResponse["_links"]!["project"]!.Value<string>("href"));
    }

    [Fact]
    public void Has_customer_href()
    {
        Assert.Equal("/api/contentapi/customer", rootRequestResponse["_links"]!["customer"]!.Value<string>("href"));
    }

}