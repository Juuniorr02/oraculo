
using System;

public class ChapterPageDefinitionData
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string Type { get; set; } = "Information";
    public string Title { get; set; } = "";
    public string Text { get; set; } = "";
    public string ImagePath { get; set; } = "";
    public string ButtonText { get; set; } = "SIGUIENTE";
}
