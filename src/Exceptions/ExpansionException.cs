using System;

namespace Flaeng.Umbraco.ContentAPI;

[Serializable]
public class ExpansionException : HalException
{
    public string ExpansionText { get; init; }

    public ExpansionException(string expansionText)
        : base("expansion_failed", $"Failed to expand '{expansionText}'")
    {
        this.ExpansionText = expansionText;
    }
}
