using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;


public partial class EventMapEditor : Control
{
    [Signal]
    public delegate void EventSelectedEventHandler(
        string eventId
    );


    [Signal]
    public delegate void InterludeSelectedEventHandler(
        string interludeId
    );


    private enum ContentType
    {
        Event,
        Interlude
    }


    private class MapNodeData
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public int Chapter { get; set; } = 0;
        public int Year { get; set; } = 0;
        public ContentType Type { get; set; }
        public List<string> Targets { get; set; } =
            new List<string>();
    }


    private class MapEdgeData
    {
        public string FromId { get; set; } = "";
        public string FromType { get; set; } = "";
        public string ToId { get; set; } = "";
    }


    private string projectPath = "";


    private GraphEdit graphEdit;

    private LineEdit searchEdit;

    private OptionButton typeFilterButton;
    private OptionButton chapterFilterButton;

    private Button refreshButton;
    private Button arrangeButton;
    private Button centerButton;

    private Label statisticsLabel;

    private Label selectedTitleLabel;
    private Label selectedInfoLabel;

    private VBoxContainer selectedConnectionsList;


    private List<MapNodeData> allNodes =
        new List<MapNodeData>();

    private List<MapEdgeData> allEdges =
        new List<MapEdgeData>();


    private Dictionary<string, MapNodeData> nodeLookup =
        new Dictionary<string, MapNodeData>(
            StringComparer.OrdinalIgnoreCase
        );


    private MapNodeData selectedNode;


    public override void _Ready()
    {
        BuildInterface();

        GD.Print(
            "EventMapEditor iniciado."
        );
    }


    public void SetProjectPath(
        string path)
    {
        projectPath =
            path ?? "";

        if (string.IsNullOrWhiteSpace(
            projectPath))
        {
            GD.PrintErr(
                "EventMapEditor: la ruta del proyecto está vacía."
            );

            return;
        }

        RefreshMap();
    }


    public void RefreshMap()
    {
        if (string.IsNullOrWhiteSpace(
            projectPath))
        {
            return;
        }

        LoadContent();

        PopulateFilters();

        RebuildGraph();
    }


    private void BuildInterface()
    {
        VBoxContainer root =
            new VBoxContainer();

        root.SetAnchorsAndOffsetsPreset(
            LayoutPreset.FullRect
        );

        root.AddThemeConstantOverride(
            "separation",
            8
        );

        AddChild(
            root
        );


        // ========================================================
        // TOOLBAR
        // ========================================================

        HBoxContainer toolbar =
            new HBoxContainer();

        toolbar.CustomMinimumSize =
            new Vector2(
                0,
                42
            );

        toolbar.AddThemeConstantOverride(
            "separation",
            8
        );

        root.AddChild(
            toolbar
        );


        Label title =
            new Label();

        title.Text =
            "Mapa de eventos";

        title.CustomMinimumSize =
            new Vector2(
                180,
                0
            );

        title.VerticalAlignment =
            VerticalAlignment.Center;

        toolbar.AddChild(
            title
        );


        searchEdit =
            new LineEdit();

        searchEdit.PlaceholderText =
            "Buscar evento o interludio...";

        searchEdit.SizeFlagsHorizontal =
            Control.SizeFlags.ExpandFill;

        searchEdit.TextChanged +=
            OnSearchChanged;

        toolbar.AddChild(
            searchEdit
        );


        typeFilterButton =
            new OptionButton();

        typeFilterButton.CustomMinimumSize =
            new Vector2(
                120,
                0
            );

        typeFilterButton.AddItem(
            "Todos"
        );

        typeFilterButton.AddItem(
            "Eventos"
        );

        typeFilterButton.AddItem(
            "Interludios"
        );

        typeFilterButton.Select(
            0
        );

        typeFilterButton.ItemSelected +=
            OnFilterChanged;

        toolbar.AddChild(
            typeFilterButton
        );


        chapterFilterButton =
            new OptionButton();

        chapterFilterButton.CustomMinimumSize =
            new Vector2(
                110,
                0
            );

        chapterFilterButton.AddItem(
            "Todos los capítulos"
        );

        chapterFilterButton.Select(
            0
        );

        chapterFilterButton.ItemSelected +=
            OnFilterChanged;

        toolbar.AddChild(
            chapterFilterButton
        );


        refreshButton =
            new Button();

        refreshButton.Text =
            "Actualizar";

        refreshButton.TooltipText =
            "Volver a leer los JSON";

        refreshButton.Pressed +=
            RefreshMap;

        toolbar.AddChild(
            refreshButton
        );


        arrangeButton =
            new Button();

        arrangeButton.Text =
            "Ordenar";

        arrangeButton.TooltipText =
            "Reorganizar los nodos";

        arrangeButton.Pressed +=
            OnArrangePressed;

        toolbar.AddChild(
            arrangeButton
        );


        centerButton =
            new Button();

        centerButton.Text =
            "Centrar";

        centerButton.TooltipText =
            "Centrar el mapa";

        centerButton.Pressed +=
            OnCenterPressed;

        toolbar.AddChild(
            centerButton
        );


        // ========================================================
        // BODY
        // ========================================================

        HBoxContainer body =
            new HBoxContainer();

        body.SizeFlagsVertical =
            Control.SizeFlags.ExpandFill;

        body.AddThemeConstantOverride(
            "separation",
            8
        );

        root.AddChild(
            body
        );


        graphEdit =
            new GraphEdit();

        graphEdit.SizeFlagsHorizontal =
            Control.SizeFlags.ExpandFill;

        graphEdit.SizeFlagsVertical =
            Control.SizeFlags.ExpandFill;

        graphEdit.ShowGrid = true;
        graphEdit.ShowZoomButtons = true;
        graphEdit.ShowZoomLabel = true;
        graphEdit.ShowMinimapButton = true;
        graphEdit.MinimapEnabled = true;

        graphEdit.SnappingEnabled = true;
        graphEdit.SnappingDistance = 20;

        graphEdit.ZoomMin = 0.35f;
        graphEdit.ZoomMax = 1.75f;
        graphEdit.Zoom = 0.8f;

        graphEdit.ConnectionLinesCurvature =
            0.45f;

        graphEdit.ConnectionLinesThickness =
            3.0f;

        graphEdit.NodeSelected +=
            OnGraphNodeSelected;

        body.AddChild(
            graphEdit
        );


        // ========================================================
        // SIDEBAR
        // ========================================================

        PanelContainer sidebar =
            new PanelContainer();

        sidebar.CustomMinimumSize =
            new Vector2(
                300,
                0
            );

        body.AddChild(
            sidebar
        );


        VBoxContainer sidebarContent =
            new VBoxContainer();

        sidebarContent.AddThemeConstantOverride(
            "separation",
            8
        );

        sidebar.AddChild(
            sidebarContent
        );


        Label sidebarTitle =
            new Label();

        sidebarTitle.Text =
            "Información";

        sidebarTitle.AddThemeFontSizeOverride(
            "font_size",
            18
        );

        sidebarContent.AddChild(
            sidebarTitle
        );


        selectedTitleLabel =
            new Label();

        selectedTitleLabel.Text =
            "Ningún nodo seleccionado";

        selectedTitleLabel.AutowrapMode =
            TextServer.AutowrapMode.WordSmart;

        sidebarContent.AddChild(
            selectedTitleLabel
        );


        selectedInfoLabel =
            new Label();

        selectedInfoLabel.Text =
            "";

        selectedInfoLabel.AutowrapMode =
            TextServer.AutowrapMode.WordSmart;

        sidebarContent.AddChild(
            selectedInfoLabel
        );


        HSeparator separator =
            new HSeparator();

        sidebarContent.AddChild(
            separator
        );


        Label connectionsTitle =
            new Label();

        connectionsTitle.Text =
            "Conexiones";

        sidebarContent.AddChild(
            connectionsTitle
        );


        ScrollContainer connectionsScroll =
            new ScrollContainer();

        connectionsScroll.SizeFlagsVertical =
            Control.SizeFlags.ExpandFill;

        sidebarContent.AddChild(
            connectionsScroll
        );


        selectedConnectionsList =
            new VBoxContainer();

        selectedConnectionsList.SizeFlagsHorizontal =
            Control.SizeFlags.ExpandFill;

        connectionsScroll.AddChild(
            selectedConnectionsList
        );


        statisticsLabel =
            new Label();

        statisticsLabel.Text =
            "";

        statisticsLabel.AutowrapMode =
            TextServer.AutowrapMode.WordSmart;

        sidebarContent.AddChild(
            statisticsLabel
        );
    }


    private void LoadContent()
    {
        allNodes.Clear();
        allEdges.Clear();
        nodeLookup.Clear();


        LoadFolder(
            "events",
            ContentType.Event
        );

        LoadFolder(
            "interludes",
            ContentType.Interlude
        );


        BuildEdges();
    }


    private void LoadFolder(
        string folderName,
        ContentType contentType)
    {
        string folderPath =
            Path.Combine(
                projectPath,
                folderName
            );


        if (!Directory.Exists(
            folderPath))
        {
            return;
        }


        string[] files =
            Directory.GetFiles(
                folderPath,
                "*.json"
            );


        Array.Sort(
            files,
            StringComparer.OrdinalIgnoreCase
        );


        foreach (
            string filePath
            in files)
        {
            LoadContentFile(
                filePath,
                contentType
            );
        }
    }


    private void LoadContentFile(
        string filePath,
        ContentType contentType)
    {
        try
        {
            string json =
                File.ReadAllText(
                    filePath
                );


            using JsonDocument document =
                JsonDocument.Parse(
                    json
                );


            JsonElement root =
                document.RootElement;


            string fallbackId =
                Path.GetFileNameWithoutExtension(
                    filePath
                );


            string id =
                GetStringProperty(
                    root,
                    "Id"
                );


            if (string.IsNullOrWhiteSpace(id))
            {
                id =
                    fallbackId;
            }


            string title =
                GetStringProperty(
                    root,
                    "Title"
                );


            if (string.IsNullOrWhiteSpace(title))
            {
                title =
                    id;
            }


            int chapter =
                GetIntProperty(
                    root,
                    "Chapter"
                );


            int year =
                GetIntProperty(
                    root,
                    "Year"
                );


            MapNodeData node =
                new MapNodeData
                {
                    Id = id,
                    Title = title,
                    Chapter = chapter,
                    Year = year,
                    Type = contentType
                };


            if (nodeLookup.ContainsKey(
                id))
            {
                GD.PrintErr(
                    "EventMapEditor: ID duplicado: ",
                    id
                );

                return;
            }


            allNodes.Add(
                node
            );

            nodeLookup[
                id
            ] = node;
        }
        catch (
            Exception exception)
        {
            GD.PrintErr(
                "EventMapEditor: error leyendo ",
                filePath,
                "\n",
                exception.Message
            );
        }
    }


    private void BuildEdges()
    {
        string eventsPath =
            Path.Combine(
                projectPath,
                "events"
            );

        string interludesPath =
            Path.Combine(
                projectPath,
                "interludes"
            );


        for (
            int index = 0;
            index < allNodes.Count;
            index++)
        {
            MapNodeData node =
                allNodes[index];


            string folderName =
                node.Type == ContentType.Event
                    ? "events"
                    : "interludes";


            string folderPath =
                node.Type == ContentType.Event
                    ? eventsPath
                    : interludesPath;


            string filePath =
                Path.Combine(
                    folderPath,
                    node.Id + ".json"
                );


            if (!File.Exists(
                filePath))
            {
                string alternative =
                    FindFileIgnoringNameCase(
                        folderPath,
                        node.Id
                    );

                if (!string.IsNullOrWhiteSpace(
                    alternative))
                {
                    filePath =
                        alternative;
                }
            }


            if (!File.Exists(
                filePath))
            {
                continue;
            }


            try
            {
                string json =
                    File.ReadAllText(
                        filePath
                    );


                using JsonDocument document =
                    JsonDocument.Parse(
                        json
                    );


                HashSet<string> targets =
                    new HashSet<string>(
                        StringComparer.OrdinalIgnoreCase
                    );


                CollectNextReferences(
                    document.RootElement,
                    targets
                );


                foreach (
                    string target
                    in targets)
                {
                    node.Targets.Add(
                        target
                    );


                    allEdges.Add(
                        new MapEdgeData
                        {
                            FromId = node.Id,
                            FromType =
                                node.Type == ContentType.Event
                                    ? "Event"
                                    : "Interlude",
                            ToId = target
                        }
                    );
                }
            }
            catch (
                Exception exception)
            {
                GD.PrintErr(
                    "EventMapEditor: error construyendo conexiones de ",
                    node.Id,
                    "\n",
                    exception.Message
                );
            }
        }
    }


    private void CollectNextReferences(
        JsonElement element,
        HashSet<string> targets)
    {
        if (element.ValueKind ==
            JsonValueKind.Object)
        {
            foreach (
                JsonProperty property
                in element.EnumerateObject())
            {
                string propertyName =
                    property.Name;


                if (
                    string.Equals(
                        propertyName,
                        "NextEventId",
                        StringComparison.OrdinalIgnoreCase
                    )
                    ||
                    string.Equals(
                        propertyName,
                        "NextInterludeId",
                        StringComparison.OrdinalIgnoreCase
                    )
                    ||
                    string.Equals(
                        propertyName,
                        "NextContentId",
                        StringComparison.OrdinalIgnoreCase
                    )
                    ||
                    string.Equals(
                        propertyName,
                        "NextId",
                        StringComparison.OrdinalIgnoreCase
                    ))
                {
                    if (
                        property.Value.ValueKind ==
                        JsonValueKind.String)
                    {
                        string value =
                            property.Value.GetString();


                        if (!string.IsNullOrWhiteSpace(
                            value))
                        {
                            targets.Add(
                                value
                            );
                        }
                    }
                }


                CollectNextReferences(
                    property.Value,
                    targets
                );
            }

            return;
        }


        if (
            element.ValueKind ==
            JsonValueKind.Array)
        {
            foreach (
                JsonElement child
                in element.EnumerateArray())
            {
                CollectNextReferences(
                    child,
                    targets
                );
            }
        }
    }


    private string FindFileIgnoringNameCase(
        string folderPath,
        string id)
    {
        if (!Directory.Exists(
            folderPath))
        {
            return "";
        }


        foreach (
            string file
            in Directory.GetFiles(
                folderPath,
                "*.json"))
        {
            string fileId =
                Path.GetFileNameWithoutExtension(
                    file
                );


            if (
                string.Equals(
                    fileId,
                    id,
                    StringComparison.OrdinalIgnoreCase
                ))
            {
                return file;
            }
        }


        return "";
    }


    private void PopulateFilters()
    {
        if (chapterFilterButton == null)
        {
            return;
        }


        chapterFilterButton.Clear();

        chapterFilterButton.AddItem(
            "Todos los capítulos"
        );


        HashSet<int> chapters =
            new HashSet<int>();


        foreach (
            MapNodeData node
            in allNodes)
        {
            if (node.Chapter > 0)
            {
                chapters.Add(
                    node.Chapter
                );
            }
        }


        List<int> ordered =
            new List<int>(
                chapters
            );

        ordered.Sort();


        foreach (
            int chapter
            in ordered)
        {
            chapterFilterButton.AddItem(
                $"Capítulo {chapter:D2}"
            );

            chapterFilterButton.SetItemMetadata(
                chapterFilterButton.ItemCount - 1,
                chapter
            );
        }


        chapterFilterButton.Select(
            0
        );
    }


    private void RebuildGraph()
    {
        if (graphEdit == null)
        {
            return;
        }


        graphEdit.ClearConnections();


        foreach (
            Node child
            in graphEdit.GetChildren())
        {
            child.QueueFree();
        }


        selectedNode =
            null;


        UpdateSelectionPanel();


        List<MapNodeData> visibleNodes =
            GetVisibleNodes();


        Dictionary<
            string,
            GraphNode> graphNodes =
            new Dictionary<
                string,
                GraphNode>(
                    StringComparer.OrdinalIgnoreCase
                );


        Dictionary<
            string,
            int> layers =
            CalculateLayers(
                visibleNodes
            );


        Dictionary<
            int,
            int> layerCounters =
            new Dictionary<
                int,
                int
            >();


        visibleNodes.Sort(
            (left, right) =>
            {
                int layerLeft =
                    layers[left.Id];

                int layerRight =
                    layers[right.Id];


                if (layerLeft != layerRight)
                {
                    return layerLeft.CompareTo(
                        layerRight
                    );
                }


                int chapterCompare =
                    left.Chapter.CompareTo(
                        right.Chapter
                    );


                if (chapterCompare != 0)
                {
                    return chapterCompare;
                }


                int yearCompare =
                    left.Year.CompareTo(
                        right.Year
                    );


                if (yearCompare != 0)
                {
                    return yearCompare;
                }


                return string.Compare(
                    left.Id,
                    right.Id,
                    StringComparison.OrdinalIgnoreCase
                );
            }
        );


        foreach (
            MapNodeData data
            in visibleNodes)
        {
            if (!layerCounters.ContainsKey(
                layers[data.Id]))
            {
                layerCounters[
                    layers[data.Id]
                ] = 0;
            }


            int row =
                layerCounters[
                    layers[data.Id]
                ];


            layerCounters[
                layers[data.Id]
            ]++;


            GraphNode graphNode =
                CreateGraphNode(
                    data
                );


            graphNode.PositionOffset =
                new Vector2(
                    80 +
                    layers[data.Id] * 380,
                    80 +
                    row * 175
                );


            graphEdit.AddChild(
                graphNode
            );


            graphNodes[
                data.Id
            ] = graphNode;
        }


        foreach (
            MapEdgeData edge
            in allEdges)
        {
            if (!graphNodes.ContainsKey(
                edge.FromId))
            {
                continue;
            }


            if (!graphNodes.ContainsKey(
                edge.ToId))
            {
                continue;
            }


            graphEdit.ConnectNode(
                graphNodes[edge.FromId].Name,
                0,
                graphNodes[edge.ToId].Name,
                0,
                false
            );
        }


        UpdateStatistics(
            visibleNodes.Count
        );
    }


    private List<MapNodeData> GetVisibleNodes()
    {
        List<MapNodeData> result =
            new List<MapNodeData>();


        string searchText =
            searchEdit?.Text.Trim() ?? "";


        int typeFilter =
            typeFilterButton?.Selected ?? 0;


        int chapterFilter =
            -1;


        if (
            chapterFilterButton != null &&
            chapterFilterButton.Selected > 0)
        {
            Variant metadata =
                chapterFilterButton.GetItemMetadata(
                    chapterFilterButton.Selected
                );


            if (metadata.VariantType ==
                Variant.Type.Int)
            {
                chapterFilter =
                    metadata.AsInt32();
            }
        }


        foreach (
            MapNodeData node
            in allNodes)
        {
            if (
                typeFilter == 1 &&
                node.Type != ContentType.Event)
            {
                continue;
            }


            if (
                typeFilter == 2 &&
                node.Type != ContentType.Interlude)
            {
                continue;
            }


            if (
                chapterFilter >= 0 &&
                node.Chapter != chapterFilter)
            {
                continue;
            }


            if (
                !string.IsNullOrWhiteSpace(
                    searchText)
            )
            {
                bool matches =
                    node.Id.Contains(
                        searchText,
                        StringComparison.OrdinalIgnoreCase
                    )
                    ||
                    node.Title.Contains(
                        searchText,
                        StringComparison.OrdinalIgnoreCase
                    );


                if (!matches)
                {
                    continue;
                }
            }


            result.Add(
                node
            );
        }


        return result;
    }


    private GraphNode CreateGraphNode(
        MapNodeData data)
    {
        GraphNode graphNode =
            new GraphNode();


        graphNode.Name =
            "GraphNode_" +
            Guid.NewGuid().ToString(
                "N"
            );


        graphNode.Title =
            data.Type == ContentType.Event
                ? "EVENTO"
                : "INTERLUDIO";


        graphNode.CustomMinimumSize =
            new Vector2(
                270,
                120
            );


        graphNode.SetMeta(
            "content_id",
            data.Id
        );

        graphNode.SetMeta(
            "content_type",
            data.Type == ContentType.Event
                ? "Event"
                : "Interlude"
        );


        VBoxContainer content =
            new VBoxContainer();

        content.AddThemeConstantOverride(
            "separation",
            4
        );

        graphNode.AddChild(
            content
        );


        Label idLabel =
            new Label();

        idLabel.Text =
            data.Id;

        idLabel.AutowrapMode =
            TextServer.AutowrapMode.WordSmart;

        content.AddChild(
            idLabel
        );


        Label titleLabel =
            new Label();

        titleLabel.Text =
            data.Title;

        titleLabel.AutowrapMode =
            TextServer.AutowrapMode.WordSmart;

        content.AddChild(
            titleLabel
        );


        Label infoLabel =
            new Label();

        string info =
            "";


        if (data.Chapter > 0)
        {
            info +=
                $"Capítulo {data.Chapter:D2}";
        }


        if (data.Year > 0)
        {
            if (!string.IsNullOrWhiteSpace(
                info))
            {
                info +=
                    "  ·  ";
            }


            info +=
                $"Año {data.Year}";
        }


        if (string.IsNullOrWhiteSpace(
            info))
        {
            info =
                "Sin datos temporales";
        }


        infoLabel.Text =
            info;

        content.AddChild(
            infoLabel
        );


        Label targetLabel =
            new Label();

        targetLabel.Text =
            $"Salidas: {data.Targets.Count}";

        content.AddChild(
            targetLabel
        );


        Button openButton =
            new Button();

        openButton.Text =
            "Abrir";

        openButton.CustomMinimumSize =
            new Vector2(
                0,
                32
            );

        openButton.Pressed +=
            () =>
            {
                OpenContent(
                    data
                );
            };

        content.AddChild(
            openButton
        );


        graphNode.SetSlot(
            0,
            true,
            0,
            Colors.White,
            true,
            0,
            Colors.White
        );


        return graphNode;
    }


    private Dictionary<
        string,
        int> CalculateLayers(
            List<MapNodeData> nodes)
    {
        Dictionary<
            string,
            int> layers =
            new Dictionary<
                string,
                int>(
                    StringComparer.OrdinalIgnoreCase
                );


        HashSet<string> visibleIds =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase
            );


        foreach (
            MapNodeData node
            in nodes)
        {
            visibleIds.Add(
                node.Id
            );

            layers[
                node.Id
            ] = 0;
        }


        Dictionary<
            string,
            int> indegree =
            new Dictionary<
                string,
                int>(
                    StringComparer.OrdinalIgnoreCase
                );


        foreach (
            MapNodeData node
            in nodes)
        {
            indegree[
                node.Id
            ] = 0;
        }


        foreach (
            MapEdgeData edge
            in allEdges)
        {
            if (
                !visibleIds.Contains(
                    edge.FromId)
                ||
                !visibleIds.Contains(
                    edge.ToId))
            {
                continue;
            }


            indegree[
                edge.ToId
            ]++;
        }


        Queue<string> queue =
            new Queue<string>();


        foreach (
            MapNodeData node
            in nodes)
        {
            if (
                indegree[node.Id] == 0)
            {
                queue.Enqueue(
                    node.Id
                );
            }
        }


        HashSet<string> processed =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase
            );


        while (
            queue.Count > 0)
        {
            string current =
                queue.Dequeue();


            if (!processed.Add(
                current))
            {
                continue;
            }


            foreach (
                MapEdgeData edge
                in allEdges)
            {
                if (!string.Equals(
                    edge.FromId,
                    current,
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }


                if (!visibleIds.Contains(
                    edge.ToId))
                {
                    continue;
                }


                layers[
                    edge.ToId
                ] =
                    Math.Max(
                        layers[edge.ToId],
                        layers[current] + 1
                    );


                indegree[
                    edge.ToId
                ]--;


                if (
                    indegree[edge.ToId] == 0)
                {
                    queue.Enqueue(
                        edge.ToId
                    );
                }
            }
        }


        int fallbackLayer =
            0;


        foreach (
            MapNodeData node
            in nodes)
        {
            if (!processed.Contains(
                node.Id))
            {
                layers[
                    node.Id
                ] =
                    fallbackLayer++;

            }
        }


        return layers;
    }


    private void UpdateSelectionPanel()
    {
        if (
            selectedTitleLabel == null ||
            selectedInfoLabel == null ||
            selectedConnectionsList == null)
        {
            return;
        }


        foreach (
            Node child
            in selectedConnectionsList.GetChildren())
        {
            child.QueueFree();
        }


        if (selectedNode == null)
        {
            selectedTitleLabel.Text =
                "Ningún nodo seleccionado";

            selectedInfoLabel.Text =
                "Selecciona un evento o interludio.";

            return;
        }


        selectedTitleLabel.Text =
            selectedNode.Title;


        selectedInfoLabel.Text =
            $"{GetContentTypeText(selectedNode.Type)}\n" +
            $"ID: {selectedNode.Id}\n" +
            $"Capítulo: {FormatNumber(selectedNode.Chapter)}\n" +
            $"Año: {FormatNumber(selectedNode.Year)}";


        if (selectedNode.Targets.Count == 0)
        {
            Label emptyLabel =
                new Label();

            emptyLabel.Text =
                "Sin conexiones de salida.";

            selectedConnectionsList.AddChild(
                emptyLabel
            );

            return;
        }


        foreach (
            string targetId
            in selectedNode.Targets)
        {
            Button targetButton =
                new Button();

            targetButton.Text =
                targetId;

            targetButton.Alignment =
                HorizontalAlignment.Left;

            targetButton.Pressed +=
                () =>
                {
                    FocusContentNode(
                        targetId
                    );
                };

            selectedConnectionsList.AddChild(
                targetButton
            );
        }
    }


    private void OnGraphNodeSelected(
        Node node)
    {
        if (node == null)
        {
            return;
        }


        if (!node.HasMeta(
            "content_id"))
        {
            return;
        }


        string id =
            node.GetMeta(
                "content_id"
            ).AsString();


        if (!nodeLookup.TryGetValue(
            id,
            out MapNodeData data))
        {
            return;
        }


        selectedNode =
            data;


        UpdateSelectionPanel();
    }


    private void FocusContentNode(
        string id)
    {
        foreach (
            Node child
            in graphEdit.GetChildren())
        {
            if (
                child is not GraphNode graphNode)
            {
                continue;
            }


            if (!graphNode.HasMeta(
                "content_id"))
            {
                continue;
            }


            string nodeId =
                graphNode.GetMeta(
                    "content_id"
                ).AsString();


            if (!string.Equals(
                nodeId,
                id,
                StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }


            graphEdit.SetSelected(
                graphNode
            );


            return;
        }
    }


    private void UpdateStatistics(
        int visibleCount)
    {
        int broken =
            0;


        foreach (
            MapEdgeData edge
            in allEdges)
        {
            if (!nodeLookup.ContainsKey(
                edge.ToId))
            {
                broken++;
            }
        }


        statisticsLabel.Text =
            $"Nodos visibles: {visibleCount}\n" +
            $"Nodos totales: {allNodes.Count}\n" +
            $"Conexiones: {allEdges.Count}\n" +
            $"Referencias no encontradas: {broken}";
    }


    private void OpenContent(
        MapNodeData data)
    {
        if (data == null)
        {
            return;
        }


        if (
            data.Type ==
            ContentType.Event)
        {
            EmitSignal(
                SignalName.EventSelected,
                data.Id
            );

            return;
        }


        EmitSignal(
            SignalName.InterludeSelected,
            data.Id
        );
    }


    private string GetContentTypeText(
        ContentType type)
    {
        return
            type == ContentType.Event
                ? "Evento"
                : "Interludio";
    }


    private string FormatNumber(
        int value)
    {
        return value > 0
            ? value.ToString()
            : "No definido";
    }


    private string GetStringProperty(
        JsonElement element,
        string propertyName)
    {
        if (
            element.ValueKind !=
            JsonValueKind.Object)
        {
            return "";
        }


        foreach (
            JsonProperty property
            in element.EnumerateObject())
        {
            if (
                !string.Equals(
                    property.Name,
                    propertyName,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }


            if (
                property.Value.ValueKind ==
                JsonValueKind.String)
            {
                return property.Value.GetString() ?? "";
            }
        }


        return "";
    }


    private int GetIntProperty(
        JsonElement element,
        string propertyName)
    {
        if (
            element.ValueKind !=
            JsonValueKind.Object)
        {
            return 0;
        }


        foreach (
            JsonProperty property
            in element.EnumerateObject())
        {
            if (
                !string.Equals(
                    property.Name,
                    propertyName,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }


            if (
                property.Value.ValueKind ==
                JsonValueKind.Number
                &&
                property.Value.TryGetInt32(
                    out int value))
            {
                return value;
            }
        }


        return 0;
    }


    private void OnSearchChanged(
        string newText)
    {
        RebuildGraph();
    }


    private void OnFilterChanged(
        long index)
    {
        RebuildGraph();
    }


    private void OnArrangePressed()
    {
        if (graphEdit == null)
        {
            return;
        }


        graphEdit.ArrangeNodes();
    }


    private void OnCenterPressed()
    {
        if (graphEdit == null)
        {
            return;
        }


        graphEdit.ScrollOffset =
            Vector2.Zero;

        graphEdit.Zoom =
            0.8f;
    }
}