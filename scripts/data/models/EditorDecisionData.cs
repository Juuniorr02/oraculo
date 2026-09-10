using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class EditorDecisionData
{
    [JsonInclude]
    public string Id { get; private set; } =
        Guid.NewGuid().ToString("N");

    public string Text { get; set; } = "";

    public string Description { get; set; } = "";

    public List<EditorConditionData> Conditions { get; set; } = new();

    public List<EditorEffectData> Effects { get; set; } = new();

    // Identificador de la página a la que lleva esta decisión.
    public string NextPageId { get; set; } = "";
}
 