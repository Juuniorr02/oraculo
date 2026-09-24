using System.Collections.Generic;
using System.Text.Json.Serialization;

public class EditorEventData
{
    public string Id { get; set; } = "";

    public string Title { get; set; } = "";

    public int Chapter { get; set; } = 1;

    public int Year { get; set; } = 1;

    public int WorldYear { get; set; } = 1134;

    public List<EditorConditionData> Conditions { get; set; } = new();

    public List<EditorPageData> Pages { get; set; } = new();
}
