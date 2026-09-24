using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;


public class EditorPageData
{
    [JsonInclude]
    public string Id { get; private set; } =
        Guid.NewGuid().ToString("N");

    public string Title { get; set; } = "";


    public EditorPageType Type { get; set; } =
        EditorPageType.Normal;


    public string Text { get; set; } = "";


    // Ruta de la ilustración seleccionada en el equipo del autor.
    public string Illustration { get; set; } = "";


    public EditorIllustrationPlacement IllustrationPlacement { get; set; } =
        EditorIllustrationPlacement.Center;


    public List<EditorDecisionData> Decisions { get; set; } =
        new();
}


public enum EditorIllustrationPlacement
{
    Center,
    FullPage,
    Top,
    Bottom,
    Left,
    Right
}