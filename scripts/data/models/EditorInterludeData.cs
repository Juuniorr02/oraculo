using System.Collections.Generic;

public class EditorInterludeData
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";

    public InterludeStyle Style { get; set; } =
        InterludeStyle.Letter;

    public List<EditorInterludePageData> Pages { get; set; } =
        new();

    public List<EditorConditionData> Conditions { get; set; } =
        new();

    public List<EditorEffectData> Effects { get; set; } =
        new();

    public string NextEventId { get; set; } = "";
    public string NextChapterId { get; set; } = "";
}