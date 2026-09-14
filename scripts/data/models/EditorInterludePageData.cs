using System;
using System.Text.Json.Serialization;

public class EditorInterludePageData
{
    [JsonInclude]
    public string Id { get; private set; } =
        Guid.NewGuid().ToString("N");

    public string Text { get; set; } = "";

    public string Illustration { get; set; } = "";

    public EditorIllustrationPlacement IllustrationPlacement { get; set; } =
        EditorIllustrationPlacement.Center;
}