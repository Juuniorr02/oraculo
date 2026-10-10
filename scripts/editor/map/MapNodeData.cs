using System.Collections.Generic;

internal enum MapNodeType
{
    Event,
    Interlude,
    Decision,
    PersistentDecision
}

internal enum MapEdgeType
{
    Contains,
    Produces,
    Dependency,
    Continuity
}

internal sealed class MapNodeData
{
    public string Key { get; set; } = "";
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";

    public int Chapter { get; set; } = 0;
    public int Year { get; set; } = 0;

    public MapNodeType Type { get; set; }

    public string SourceEventId { get; set; } = "";
    public string SourcePageId { get; set; } = "";
    public string SourceDecisionId { get; set; } = "";

    public List<string> InfoLines { get; set; } = new List<string>();
}

internal sealed class MapEdgeData
{
    public string FromKey { get; set; } = "";
    public string ToKey { get; set; } = "";

    public MapEdgeType Type { get; set; }

    public string Label { get; set; } = "";
}
