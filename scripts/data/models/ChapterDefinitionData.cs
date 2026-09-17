using System.Collections.Generic;

public class ChapterDefinitionData
{
    public string Id { get; set; } = "";

    public int Number { get; set; } = 1;

    public string Title { get; set; } = "";

    public int StartYear { get; set; } = 1;

    public int StartMonth { get; set; } = 1;

    public List<string> AttributeIds { get; set; } = new();

    public List<ChapterPageDefinitionData> Pages { get; set; } = new();
}