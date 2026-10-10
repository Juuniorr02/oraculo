
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;


internal static class EventMapGraphBuilder
{
    // ============================================================
    // LOAD
    // ============================================================

    public static void LoadContent(
        EventMapEditor editor)
    {
        if (editor == null)
        {
            return;
        }

        editor.allNodes.Clear();
        editor.allEdges.Clear();
        editor.nodeLookup.Clear();

        LoadEvents(editor);
        LoadInterludes(editor);

        // Las conexiones se crean cuando ya existen ambos tipos de nodo.
        BuildEventContinuity(editor);

        BuildDecisionStructure(editor);
        BuildInterludeContinuity(editor);
    }


    public static void LoadEvents(
        EventMapEditor editor)
    {
        if (
            editor == null ||
            editor.eventRepository == null)
        {
            return;
        }

        List<EditorEventData> events =
            editor.eventRepository.LoadAll();

        if (events == null)
        {
            return;
        }

        foreach (EditorEventData eventData in events)
        {
            if (eventData == null)
            {
                continue;
            }

            string id = eventData.Id ?? "";

            if (string.IsNullOrWhiteSpace(id))
            {
                continue;
            }

            string title =
                string.IsNullOrWhiteSpace(eventData.Title)
                    ? id
                    : eventData.Title;

            MapNodeData node =
                new MapNodeData
                {
                    Key = BuildContentKey(
                        MapNodeType.Event,
                        id
                    ),

                    Id = id,
                    Title = title,
                    Chapter = eventData.Chapter,
                    Year = eventData.Year,
                    Type = MapNodeType.Event
                };

            node.InfoLines.Add(
                $"Capítulo: {FormatNumber(eventData.Chapter)}"
            );

            node.InfoLines.Add(
                $"Año base: {FormatNumber(eventData.Year)}"
            );

            node.InfoLines.Add(
                $"Año mundial: {FormatNumber(eventData.WorldYear)}"
            );

            int conditionCount =
                eventData.Conditions != null
                    ? eventData.Conditions.Count
                    : 0;

            node.InfoLines.Add(
                $"Condiciones: {conditionCount}"
            );

            AddNode(editor, node);

            BuildEventConditionDependencies(
                editor,
                eventData,
                node
            );
        }
    }


    public static void LoadInterludes(
        EventMapEditor editor)
    {
        if (
            editor == null ||
            editor.interludeRepository == null)
        {
            return;
        }

        List<EditorInterludeData> interludes =
            editor.interludeRepository.LoadAll();

        if (interludes == null)
        {
            return;
        }

        foreach (EditorInterludeData interlude in interludes)
        {
            if (interlude == null)
            {
                continue;
            }

            string id = interlude.Id ?? "";

            if (string.IsNullOrWhiteSpace(id))
            {
                continue;
            }

            string title =
                string.IsNullOrWhiteSpace(interlude.Title)
                    ? id
                    : interlude.Title;

            MapNodeData node =
                new MapNodeData
                {
                    Key = BuildContentKey(
                        MapNodeType.Interlude,
                        id
                    ),

                    Id = id,
                    Title = title,
                    Chapter = interlude.Chapter,
                    Type = MapNodeType.Interlude
                };

            node.InfoLines.Add(
                $"Capítulo: {FormatNumber(interlude.Chapter)}"
            );

            AddNode(editor, node);

            BuildInterludeConditionDependencies(
                editor,
                interlude,
                node
            );
        }
    }


    // ============================================================
    // EVENT CONTINUITY
    // ============================================================

    private static void BuildEventContinuity(
        EventMapEditor editor)
    {
        if (
            editor == null ||
            editor.eventRepository == null)
        {
            return;
        }

        List<EditorEventData> events =
            editor.eventRepository.LoadAll();

        if (events == null)
        {
            return;
        }

        foreach (EditorEventData eventData in events)
        {
            if (eventData == null)
            {
                continue;
            }

            string eventId =
                eventData.Id?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(eventId))
            {
                continue;
            }

            string nextEventId =
                eventData.NextEventId?.Trim() ?? "";

            string nextInterludeId =
                eventData.NextInterludeId?.Trim() ?? "";

            bool hasNextEvent =
                !string.IsNullOrWhiteSpace(nextEventId);

            bool hasNextInterlude =
                !string.IsNullOrWhiteSpace(nextInterludeId);

            // Un evento solo puede tener un destino.
            // Si ambos están definidos, no dibujamos ninguno.
            if (hasNextEvent && hasNextInterlude)
            {
                continue;
            }

            if (!hasNextEvent && !hasNextInterlude)
            {
                continue;
            }

            MapNodeData sourceNode =
                GetContentNode(
                    editor,
                    MapNodeType.Event,
                    eventId
                );

            if (sourceNode == null)
            {
                continue;
            }

            MapNodeData targetNode;

            string label;

            if (hasNextEvent)
            {
                targetNode =
                    GetContentNode(
                        editor,
                        MapNodeType.Event,
                        nextEventId
                    );

                label = "Siguiente evento";
            }
            else
            {
                targetNode =
                    GetContentNode(
                        editor,
                        MapNodeType.Interlude,
                        nextInterludeId
                    );

                label = "Siguiente interludio";
            }

            // No resolvemos el destino por ID genérico:
            // el tipo de destino está determinado por la propiedad.
            if (targetNode == null)
            {
                continue;
            }

            AddEdge(
                editor,
                new MapEdgeData
                {
                    FromKey = sourceNode.Key,
                    ToKey = targetNode.Key,
                    Type = MapEdgeType.Continuity,
                    Label = label
                }
            );
        }
    }


    // ============================================================
    // NODES
    // ============================================================

    public static void AddNode(
        EventMapEditor editor,
        MapNodeData node)
    {
        if (
            editor == null ||
            node == null ||
            string.IsNullOrWhiteSpace(node.Key))
        {
            return;
        }

        if (editor.nodeLookup.ContainsKey(node.Key))
        {
            return;
        }

        editor.allNodes.Add(node);

        editor.nodeLookup[node.Key] = node;
    }


    public static string BuildContentKey(
        MapNodeType type,
        string id)
    {
        string prefix =
            type == MapNodeType.Event
                ? "event:"
                : "interlude:";

        return prefix + (id ?? "");
    }


    public static string BuildDecisionKey(
        string eventId,
        string pageId,
        int decisionIndex)
    {
        return $"decision:{eventId}:{pageId}:{decisionIndex}";
    }


    public static string BuildFlagKey(
        string decisionId)
    {
        return "flag:" + (decisionId ?? "").Trim();
    }


    // ============================================================
    // EVENT CONDITIONS
    // ============================================================

    private static void BuildEventConditionDependencies(
        EventMapEditor editor,
        EditorEventData eventData,
        MapNodeData eventNode)
    {
        if (
            editor == null ||
            eventData == null ||
            eventNode == null ||
            eventData.Conditions == null)
        {
            return;
        }

        foreach (EditorConditionData condition in eventData.Conditions)
        {
            if (condition == null)
            {
                continue;
            }

            if (!string.Equals(
                condition.TypeId,
                "decision",
                StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string decisionId =
                condition.DecisionId?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(decisionId))
            {
                continue;
            }

            MapNodeData flagNode =
                EnsurePersistentDecisionNode(
                    editor,
                    decisionId,
                    eventData.Chapter
                );

            if (flagNode == null)
            {
                continue;
            }

            AddEdge(
                editor,
                new MapEdgeData
                {
                    FromKey = flagNode.Key,
                    ToKey = eventNode.Key,
                    Type = MapEdgeType.Dependency,
                    Label = "Requiere"
                }
            );
        }
    }


    // ============================================================
    // DECISIONS
    // ============================================================

    public static void BuildDecisionStructure(
        EventMapEditor editor)
    {
        if (
            editor == null ||
            editor.eventRepository == null)
        {
            return;
        }

        List<EditorEventData> events =
            editor.eventRepository.LoadAll();

        if (events == null)
        {
            return;
        }

        foreach (EditorEventData eventData in events)
        {
            if (eventData == null)
            {
                continue;
            }

            BuildEventDecisionStructure(
                editor,
                eventData,
                eventData.Pages
            );
        }
    }


    private static void BuildEventDecisionStructure(
        EventMapEditor editor,
        EditorEventData eventData,
        List<EditorPageData> pages)
    {
        if (
            editor == null ||
            eventData == null ||
            pages == null)
        {
            return;
        }

        for (int pageIndex = 0; pageIndex < pages.Count; pageIndex++)
        {
            EditorPageData page = pages[pageIndex];

            if (
                page == null ||
                page.Decisions == null)
            {
                continue;
            }

            for (
                int decisionIndex = 0;
                decisionIndex < page.Decisions.Count;
                decisionIndex++)
            {
                EditorDecisionData decision =
                    page.Decisions[decisionIndex];

                if (decision == null)
                {
                    continue;
                }

                string decisionId = decision.Id ?? "";

                string decisionKey =
                    BuildDecisionKey(
                        eventData.Id ?? "",
                        page.Id ?? "",
                        decisionIndex
                    );

                MapNodeData decisionNode =
                    new MapNodeData
                    {
                        Key = decisionKey,
                        Id = decisionId,

                        Title =
                            string.IsNullOrWhiteSpace(decision.Text)
                                ? decisionId
                                : decision.Text,

                        Chapter = eventData.Chapter,
                        Year = eventData.Year,
                        Type = MapNodeType.Decision,

                        SourceEventId = eventData.Id ?? "",
                        SourcePageId = page.Id ?? "",
                        SourceDecisionId = decisionId
                    };

                decisionNode.InfoLines.Add(
                    $"Evento: {eventData.Id}"
                );

                decisionNode.InfoLines.Add(
                    $"Página: {pageIndex + 1}"
                );

                decisionNode.InfoLines.Add(
                    $"ID: {FormatText(decisionId)}"
                );

                int conditionCount =
                    decision.Conditions != null
                        ? decision.Conditions.Count
                        : 0;

                int effectCount =
                    decision.Effects != null
                        ? decision.Effects.Count
                        : 0;

                decisionNode.InfoLines.Add(
                    $"Condiciones: {conditionCount}"
                );

                decisionNode.InfoLines.Add(
                    $"Efectos: {effectCount}"
                );

                AddNode(editor, decisionNode);

                MapNodeData eventNode =
                    GetContentNode(
                        editor,
                        MapNodeType.Event,
                        eventData.Id
                    );

                if (eventNode != null)
                {
                    AddEdge(
                        editor,
                        new MapEdgeData
                        {
                            FromKey = eventNode.Key,
                            ToKey = decisionNode.Key,
                            Type = MapEdgeType.Contains,
                            Label = "Contiene"
                        }
                    );
                }

                BuildDecisionConditions(
                    editor,
                    eventData,
                    decision,
                    decisionNode
                );

                BuildDecisionEffects(
                    editor,
                    eventData,
                    decision,
                    decisionNode
                );

                BuildEventDecisionStructure(
                    editor,
                    eventData,
                    decision.Pages
                );
            }
        }
    }


    private static void BuildDecisionConditions(
        EventMapEditor editor,
        EditorEventData eventData,
        EditorDecisionData decision,
        MapNodeData decisionNode)
    {
        if (
            editor == null ||
            eventData == null ||
            decision == null ||
            decisionNode == null ||
            decision.Conditions == null)
        {
            return;
        }

        foreach (EditorConditionData condition in decision.Conditions)
        {
            if (condition == null)
            {
                continue;
            }

            if (!string.Equals(
                condition.TypeId,
                "decision",
                StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string decisionId =
                condition.DecisionId?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(decisionId))
            {
                continue;
            }

            MapNodeData flagNode =
                EnsurePersistentDecisionNode(
                    editor,
                    decisionId,
                    eventData.Chapter
                );

            if (flagNode == null)
            {
                continue;
            }

            AddEdge(
                editor,
                new MapEdgeData
                {
                    FromKey = flagNode.Key,
                    ToKey = decisionNode.Key,
                    Type = MapEdgeType.Dependency,
                    Label = "Requiere"
                }
            );
        }
    }


    private static void BuildDecisionEffects(
        EventMapEditor editor,
        EditorEventData eventData,
        EditorDecisionData decision,
        MapNodeData decisionNode)
    {
        if (
            editor == null ||
            eventData == null ||
            decision == null ||
            decisionNode == null ||
            decision.Effects == null)
        {
            return;
        }

        foreach (EditorEffectData effect in decision.Effects)
        {
            if (effect == null)
            {
                continue;
            }

            if (!string.Equals(
                effect.TypeId,
                "decision",
                StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string decisionId =
                effect.DecisionId?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(decisionId))
            {
                continue;
            }

            MapNodeData flagNode =
                EnsurePersistentDecisionNode(
                    editor,
                    decisionId,
                    eventData.Chapter
                );

            if (flagNode == null)
            {
                continue;
            }

            AddEdge(
                editor,
                new MapEdgeData
                {
                    FromKey = decisionNode.Key,
                    ToKey = flagNode.Key,
                    Type = MapEdgeType.Produces,
                    Label = "Produce"
                }
            );
        }
    }


    private static MapNodeData EnsurePersistentDecisionNode(
        EventMapEditor editor,
        string decisionId,
        int chapter)
    {
        if (editor == null)
        {
            return null;
        }

        string safeId = decisionId?.Trim() ?? "";

        string key = BuildFlagKey(safeId);

        if (editor.nodeLookup.TryGetValue(
            key,
            out MapNodeData existing))
        {
            return existing;
        }

        MapNodeData node =
            new MapNodeData
            {
                Key = key,
                Id = safeId,
                Title = safeId,
                Chapter = chapter,
                Type = MapNodeType.PersistentDecision
            };

        node.InfoLines.Add("Estado persistente");
        node.InfoLines.Add($"ID: {safeId}");

        AddNode(editor, node);

        return node;
    }


    // ============================================================
    // INTERLUDE CONTINUITY
    // ============================================================

    private static void BuildInterludeContinuity(
        EventMapEditor editor)
    {
        if (
            editor == null ||
            editor.interludeRepository == null)
        {
            return;
        }

        List<EditorInterludeData> interludes =
            editor.interludeRepository.LoadAll();

        if (interludes == null)
        {
            return;
        }

        foreach (EditorInterludeData interlude in interludes)
        {
            if (interlude == null)
            {
                continue;
            }

            string interludeId =
                interlude.Id?.Trim() ?? "";

            string nextEventId =
                interlude.NextEventId?.Trim() ?? "";

            if (
                string.IsNullOrWhiteSpace(interludeId) ||
                string.IsNullOrWhiteSpace(nextEventId))
            {
                continue;
            }

            MapNodeData sourceNode =
                GetContentNode(
                    editor,
                    MapNodeType.Interlude,
                    interludeId
                );

            if (sourceNode == null)
            {
                continue;
            }

            // Los interludios solo pueden continuar hacia eventos.
            MapNodeData targetNode =
                GetContentNode(
                    editor,
                    MapNodeType.Event,
                    nextEventId
                );

            if (targetNode == null)
            {
                continue;
            }

            AddEdge(
                editor,
                new MapEdgeData
                {
                    FromKey = sourceNode.Key,
                    ToKey = targetNode.Key,
                    Type = MapEdgeType.Continuity,
                    Label = "Siguiente evento"
                }
            );
        }
    }


    // ============================================================
    // INTERLUDE CONDITION DEPENDENCIES
    // ============================================================

    private static void BuildInterludeConditionDependencies(
        EventMapEditor editor,
        EditorInterludeData interlude,
        MapNodeData interludeNode)
    {
        if (
            editor == null ||
            interlude == null ||
            interludeNode == null)
        {
            return;
        }

        string interludeId = interlude.Id ?? "";

        string filePath =
            FindContentFile(
                editor,
                "interludes",
                interludeId
            );

        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        try
        {
            string json = File.ReadAllText(filePath);

            using JsonDocument document =
                JsonDocument.Parse(json);

            HashSet<string> decisionIds =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase
                );

            CollectDecisionConditions(
                document.RootElement,
                decisionIds
            );

            foreach (string decisionId in decisionIds)
            {
                MapNodeData flagNode =
                    EnsurePersistentDecisionNode(
                        editor,
                        decisionId,
                        interlude.Chapter
                    );

                if (flagNode == null)
                {
                    continue;
                }

                AddEdge(
                    editor,
                    new MapEdgeData
                    {
                        FromKey = flagNode.Key,
                        ToKey = interludeNode.Key,
                        Type = MapEdgeType.Dependency,
                        Label = "Requiere"
                    }
                );
            }
        }
        catch (Exception exception)
        {
            GD.PrintErr(
                "EventMapGraphBuilder: error leyendo condiciones de interludio ",
                interludeId,
                "\n",
                exception.Message
            );
        }
    }


    private static void CollectDecisionConditions(
        JsonElement element,
        HashSet<string> decisionIds)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (JsonProperty property in element.EnumerateObject())
            {
                if (string.Equals(
                    property.Name,
                    "Conditions",
                    StringComparison.OrdinalIgnoreCase))
                {
                    CollectDecisionConditionEntries(
                        property.Value,
                        decisionIds
                    );
                }

                if (
                    property.Value.ValueKind == JsonValueKind.Object ||
                    property.Value.ValueKind == JsonValueKind.Array)
                {
                    CollectDecisionConditions(
                        property.Value,
                        decisionIds
                    );
                }
            }

            return;
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement child in element.EnumerateArray())
            {
                CollectDecisionConditions(child, decisionIds);
            }
        }
    }


    private static void CollectDecisionConditionEntries(
        JsonElement element,
        HashSet<string> decisionIds)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            string typeId =
                GetStringProperty(
                    element,
                    "TypeId"
                );

            if (string.Equals(
                typeId,
                "decision",
                StringComparison.OrdinalIgnoreCase))
            {
                string decisionId =
                    GetStringProperty(
                        element,
                        "DecisionId"
                    );

                if (!string.IsNullOrWhiteSpace(decisionId))
                {
                    decisionIds.Add(decisionId.Trim());
                }
            }

            foreach (JsonProperty property in element.EnumerateObject())
            {
                if (
                    property.Value.ValueKind == JsonValueKind.Object ||
                    property.Value.ValueKind == JsonValueKind.Array)
                {
                    CollectDecisionConditionEntries(
                        property.Value,
                        decisionIds
                    );
                }
            }

            return;
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement child in element.EnumerateArray())
            {
                CollectDecisionConditionEntries(child, decisionIds);
            }
        }
    }


    // ============================================================
    // EDGES
    // ============================================================

    public static void AddEdge(
        EventMapEditor editor,
        MapEdgeData edge)
    {
        if (
            editor == null ||
            edge == null ||
            string.IsNullOrWhiteSpace(edge.FromKey) ||
            string.IsNullOrWhiteSpace(edge.ToKey))
        {
            return;
        }

        if (string.Equals(
            edge.FromKey,
            edge.ToKey,
            StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        foreach (MapEdgeData existing in editor.allEdges)
        {
            if (
                string.Equals(
                    existing.FromKey,
                    edge.FromKey,
                    StringComparison.OrdinalIgnoreCase
                )
                &&
                string.Equals(
                    existing.ToKey,
                    edge.ToKey,
                    StringComparison.OrdinalIgnoreCase
                )
                &&
                existing.Type == edge.Type)
            {
                return;
            }
        }

        editor.allEdges.Add(edge);
    }


    // ============================================================
    // LOOKUP
    // ============================================================

    public static MapNodeData GetContentNode(
        EventMapEditor editor,
        MapNodeType type,
        string id)
    {
        if (
            editor == null ||
            string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        string key = BuildContentKey(type, id);

        return editor.nodeLookup.TryGetValue(
            key,
            out MapNodeData node)
            ? node
            : null;
    }


    // Conservado por compatibilidad con otras clases del editor.
    public static MapNodeData ResolveContentNode(
        EventMapEditor editor,
        string id)
    {
        MapNodeData eventNode =
            GetContentNode(
                editor,
                MapNodeType.Event,
                id
            );

        if (eventNode != null)
        {
            return eventNode;
        }

        return GetContentNode(
            editor,
            MapNodeType.Interlude,
            id
        );
    }


    public static string FindContentFile(
        EventMapEditor editor,
        string folderName,
        string id)
    {
        if (
            editor == null ||
            string.IsNullOrWhiteSpace(folderName) ||
            string.IsNullOrWhiteSpace(id))
        {
            return "";
        }

        string folderPath =
            Path.Combine(
                editor.projectPath,
                folderName
            );

        if (!Directory.Exists(folderPath))
        {
            return "";
        }

        string direct =
            Path.Combine(
                folderPath,
                id + ".json"
            );

        if (File.Exists(direct))
        {
            return direct;
        }

        foreach (string file in Directory.GetFiles(folderPath, "*.json"))
        {
            string fileId =
                Path.GetFileNameWithoutExtension(file);

            if (string.Equals(
                fileId,
                id,
                StringComparison.OrdinalIgnoreCase))
            {
                return file;
            }
        }

        return "";
    }


    // ============================================================
    // FILTERS
    // ============================================================

    public static void PopulateFilters(
        EventMapEditor editor)
    {
        if (
            editor == null ||
            editor.chapterFilterButton == null)
        {
            return;
        }

        editor.chapterFilterButton.Clear();

        editor.chapterFilterButton.AddItem(
            "Todos los capítulos"
        );

        HashSet<int> chapters = new HashSet<int>();

        foreach (MapNodeData node in editor.allNodes)
        {
            if (node.Chapter > 0)
            {
                chapters.Add(node.Chapter);
            }
        }

        List<int> ordered = new List<int>(chapters);

        ordered.Sort();

        foreach (int chapter in ordered)
        {
            editor.chapterFilterButton.AddItem(
                $"Capítulo {chapter:D2}"
            );

            editor.chapterFilterButton.SetItemMetadata(
                editor.chapterFilterButton.ItemCount - 1,
                chapter
            );
        }

        editor.chapterFilterButton.Select(0);
    }


    public static List<MapNodeData> GetVisibleNodes(
        EventMapEditor editor)
    {
        List<MapNodeData> result = new List<MapNodeData>();

        if (editor == null)
        {
            return result;
        }

        string searchText =
            editor.searchEdit?.Text.Trim() ?? "";

        int typeFilter =
            editor.typeFilterButton?.Selected ?? 0;

        int chapterFilter = -1;

        if (
            editor.chapterFilterButton != null &&
            editor.chapterFilterButton.Selected > 0)
        {
            Variant metadata =
                editor.chapterFilterButton.GetItemMetadata(
                    editor.chapterFilterButton.Selected
                );

            if (metadata.VariantType == Variant.Type.Int)
            {
                chapterFilter = metadata.AsInt32();
            }
        }

        foreach (MapNodeData node in editor.allNodes)
        {
            if (
                typeFilter == 1 &&
                node.Type != MapNodeType.Event)
            {
                continue;
            }

            if (
                typeFilter == 2 &&
                node.Type != MapNodeType.Interlude)
            {
                continue;
            }

            if (
                typeFilter == 3 &&
                node.Type != MapNodeType.Decision)
            {
                continue;
            }

            if (
                typeFilter == 4 &&
                node.Type != MapNodeType.PersistentDecision)
            {
                continue;
            }

            if (
                chapterFilter >= 0 &&
                node.Chapter != chapterFilter)
            {
                continue;
            }

            if (!MatchesSearch(node, searchText))
            {
                continue;
            }

            result.Add(node);
        }

        return result;
    }


    public static bool MatchesSearch(
        MapNodeData node,
        string searchText)
    {
        if (node == null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(searchText))
        {
            return true;
        }

        if (
            (node.Id ?? "").Contains(
                searchText,
                StringComparison.OrdinalIgnoreCase
            )
            ||
            (node.Title ?? "").Contains(
                searchText,
                StringComparison.OrdinalIgnoreCase
            ))
        {
            return true;
        }

        foreach (string info in node.InfoLines)
        {
            if ((info ?? "").Contains(
                searchText,
                StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }


    // ============================================================
    // GRAPH
    // ============================================================

    public static void RebuildGraph(
        EventMapEditor editor)
    {
        if (
            editor == null ||
            editor.graphEdit == null)
        {
            return;
        }

        editor.graphEdit.ClearConnections();

        foreach (Node child in editor.graphEdit.GetChildren())
        {
            if (child is GraphNode graphNode)
            {
                graphNode.QueueFree();
            }
        }

        editor.selectedNode = null;

        editor.UpdateSelectionPanel();

        List<MapNodeData> visibleNodes =
            GetVisibleNodes(editor);

        Dictionary<string, GraphNode> graphNodes =
            new Dictionary<string, GraphNode>(
                StringComparer.OrdinalIgnoreCase
            );

        visibleNodes.Sort(EventMapLayout.CompareNodes);

        foreach (MapNodeData data in visibleNodes)
        {
            GraphNode graphNode =
                EventMapNodeFactory.Create(
                    editor,
                    data
                );

            if (graphNode == null)
            {
                continue;
            }

            editor.graphEdit.AddChild(graphNode);

            graphNodes[data.Key] = graphNode;
        }

        EventMapLayout.PositionNodes(
            visibleNodes,
            graphNodes
        );

        foreach (MapEdgeData edge in editor.allEdges)
        {
            if (!graphNodes.TryGetValue(
                edge.FromKey,
                out GraphNode fromNode))
            {
                continue;
            }

            if (!graphNodes.TryGetValue(
                edge.ToKey,
                out GraphNode toNode))
            {
                continue;
            }

            editor.graphEdit.ConnectNode(
                fromNode.Name,
                0,
                toNode.Name,
                0,
                false
            );
        }

        UpdateStatistics(
            editor,
            visibleNodes.Count
        );
    }


    // ============================================================
    // RELATIONS
    // ============================================================

    public static List<MapEdgeData> GetIncomingEdges(
        EventMapEditor editor,
        string key)
    {
        List<MapEdgeData> result = new List<MapEdgeData>();

        if (
            editor == null ||
            string.IsNullOrWhiteSpace(key))
        {
            return result;
        }

        foreach (MapEdgeData edge in editor.allEdges)
        {
            if (string.Equals(
                edge.ToKey,
                key,
                StringComparison.OrdinalIgnoreCase))
            {
                result.Add(edge);
            }
        }

        return result;
    }


    public static List<MapEdgeData> GetOutgoingEdges(
        EventMapEditor editor,
        string key)
    {
        List<MapEdgeData> result = new List<MapEdgeData>();

        if (
            editor == null ||
            string.IsNullOrWhiteSpace(key))
        {
            return result;
        }

        foreach (MapEdgeData edge in editor.allEdges)
        {
            if (string.Equals(
                edge.FromKey,
                key,
                StringComparison.OrdinalIgnoreCase))
            {
                result.Add(edge);
            }
        }

        return result;
    }


    public static MapNodeData GetNodeByKey(
        EventMapEditor editor,
        string key)
    {
        if (
            editor == null ||
            string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        return editor.nodeLookup.TryGetValue(
            key,
            out MapNodeData node)
            ? node
            : null;
    }


    // ============================================================
    // STATISTICS
    // ============================================================

    private static void UpdateStatistics(
        EventMapEditor editor,
        int visibleCount)
    {
        if (
            editor == null ||
            editor.statisticsLabel == null)
        {
            return;
        }

        int events = 0;
        int interludes = 0;
        int decisions = 0;
        int flags = 0;

        int contains = 0;
        int produces = 0;
        int dependencies = 0;
        int continuity = 0;

        foreach (MapNodeData node in editor.allNodes)
        {
            switch (node.Type)
            {
                case MapNodeType.Event:
                    events++;
                    break;

                case MapNodeType.Interlude:
                    interludes++;
                    break;

                case MapNodeType.Decision:
                    decisions++;
                    break;

                case MapNodeType.PersistentDecision:
                    flags++;
                    break;
            }
        }

        foreach (MapEdgeData edge in editor.allEdges)
        {
            switch (edge.Type)
            {
                case MapEdgeType.Contains:
                    contains++;
                    break;

                case MapEdgeType.Produces:
                    produces++;
                    break;

                case MapEdgeType.Dependency:
                    dependencies++;
                    break;

                case MapEdgeType.Continuity:
                    continuity++;
                    break;
            }
        }

        editor.statisticsLabel.Text =
            $"Nodos visibles: {visibleCount}\n" +
            $"Eventos: {events}\n" +
            $"Interludios: {interludes}\n" +
            $"Decisiones: {decisions}\n" +
            $"Flags: {flags}\n\n" +
            $"Relaciones: {editor.allEdges.Count}\n" +
            $"Contiene: {contains}\n" +
            $"Produce: {produces}\n" +
            $"Dependencias: {dependencies}\n" +
            $"Continuidad: {continuity}";
    }


    // ============================================================
    // UTILITIES
    // ============================================================

    public static string FormatText(
        string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "No definido"
            : value;
    }


    public static string FormatNumber(
        int value)
    {
        return value > 0
            ? value.ToString()
            : "No definido";
    }


    public static string GetStringProperty(
        JsonElement element,
        string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return "";
        }

        foreach (JsonProperty property in element.EnumerateObject())
        {
            if (!string.Equals(
                property.Name,
                propertyName,
                StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (property.Value.ValueKind == JsonValueKind.String)
            {
                return property.Value.GetString() ?? "";
            }
        }

        return "";
    }
}
