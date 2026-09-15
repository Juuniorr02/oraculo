using System.Collections.Generic;

public class EditorInterludeData
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";

    public int Chapter { get; set; } = 1;

    public InterludeStyle Style { get; set; } =
        InterludeStyle.Letter;

    public List<EditorInterludePageData> Pages { get; set; } =
        new();

    public List<EditorConditionData> Conditions { get; set; } =
        new();

    public List<EditorEffectData> Effects { get; set; } =
        new();

    public string NextEventId { get; set; } = "";

    public int NextChapter { get; set; } = 0;
}