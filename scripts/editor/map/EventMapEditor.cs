using Godot;
using System;
using System.Collections.Generic;


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


    internal string projectPath = "";


    internal EventRepository eventRepository;
    internal InterludeRepository interludeRepository;


    internal GraphEdit graphEdit;

    internal LineEdit searchEdit;

    internal OptionButton typeFilterButton;
    internal OptionButton chapterFilterButton;


    private Button refreshButton;
    private Button arrangeButton;
    private Button centerButton;


    internal Label statisticsLabel;


    internal Label selectedTitleLabel;
    internal Label selectedInfoLabel;

    internal ScrollContainer selectedInfoScroll;

    internal VBoxContainer selectedConnectionsList;


    internal List<MapNodeData> allNodes =
        new List<MapNodeData>();

    internal List<MapEdgeData> allEdges =
        new List<MapEdgeData>();


    internal Dictionary<string, MapNodeData> nodeLookup =
        new Dictionary<string, MapNodeData>(
            StringComparer.OrdinalIgnoreCase
        );


    internal MapNodeData selectedNode;


    // ============================================================
    // READY
    // ============================================================

    public override void _Ready()
    {
        BuildInterface();

        GD.Print(
            "EventMapEditor iniciado."
        );
    }


    // ============================================================
    // PROJECT
    // ============================================================

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


        eventRepository =
            new EventRepository(
                projectPath
            );


        interludeRepository =
            new InterludeRepository(
                projectPath
            );


        RefreshMap();
    }


    public void RefreshMap()
    {
        if (string.IsNullOrWhiteSpace(
            projectPath))
        {
            return;
        }


        EventMapGraphBuilder.LoadContent(
            this
        );


        EventMapGraphBuilder.PopulateFilters(
            this
        );


        EventMapGraphBuilder.RebuildGraph(
            this
        );
    }


    // ============================================================
    // INTERFACE
    // ============================================================

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
            "Mapa narrativo";


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
            "Buscar contenido, decisión o flag...";


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
                150,
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


        typeFilterButton.AddItem(
            "Decisiones"
        );


        typeFilterButton.AddItem(
            "Flags"
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
                130,
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
            "Volver a leer los datos del proyecto";


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
            "Aplicar de nuevo la distribución del mapa";


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


        // ========================================================
        // GRAPH
        // ========================================================

        graphEdit =
            new GraphEdit();


        graphEdit.SizeFlagsHorizontal =
            Control.SizeFlags.ExpandFill;


        graphEdit.SizeFlagsVertical =
            Control.SizeFlags.ExpandFill;


        graphEdit.ShowGrid =
            true;


        graphEdit.ShowZoomButtons =
            true;


        graphEdit.ShowZoomLabel =
            true;


        graphEdit.ShowMinimapButton =
            true;


        graphEdit.MinimapEnabled =
            true;


        graphEdit.SnappingEnabled =
            true;


        graphEdit.SnappingDistance =
            20;


        graphEdit.ZoomMin =
            0.30f;


        graphEdit.ZoomMax =
            1.75f;


        graphEdit.Zoom =
            0.75f;


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
                320,
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


        selectedInfoScroll =
            new ScrollContainer();


        selectedInfoScroll.CustomMinimumSize =
            new Vector2(
                0,
                280
            );


        selectedInfoScroll.SizeFlagsVertical =
            Control.SizeFlags.ExpandFill;


        sidebarContent.AddChild(
            selectedInfoScroll
        );


        selectedInfoLabel =
            new Label();


        selectedInfoLabel.Text =
            "";


        selectedInfoLabel.AutowrapMode =
            TextServer.AutowrapMode.WordSmart;


        selectedInfoLabel.SizeFlagsHorizontal =
            Control.SizeFlags.ExpandFill;


        selectedInfoScroll.AddChild(
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
            "Relaciones";


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


    // ============================================================
    // SELECTION
    // ============================================================

    private void OnGraphNodeSelected(
        Node node)
    {
        if (node == null)
        {
            return;
        }


        if (!node.HasMeta(
            "map_key"))
        {
            return;
        }


        string key =
            node.GetMeta(
                "map_key"
            ).AsString();


        if (!nodeLookup.TryGetValue(
            key,
            out MapNodeData data))
        {
            return;
        }


        selectedNode =
            data;


        EventMapInfoPanel.Update(
            this
        );
    }


    internal void FocusNode(
        string key)
    {
        if (graphEdit == null)
        {
            return;
        }


        foreach (
            Node child
            in graphEdit.GetChildren())
        {
            if (child is not GraphNode graphNode)
            {
                continue;
            }


            if (!graphNode.HasMeta(
                "map_key"))
            {
                continue;
            }


            string nodeKey =
                graphNode.GetMeta(
                    "map_key"
                ).AsString();


            if (!string.Equals(
                nodeKey,
                key,
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


    // ============================================================
    // CONTENT OPENING
    // ============================================================

    internal void OpenContent(
        MapNodeData data)
    {
        if (data == null)
        {
            return;
        }


        if (data.Type ==
            MapNodeType.Event)
        {
            EmitSignal(
                SignalName.EventSelected,
                data.Id
            );

            return;
        }


        if (data.Type ==
            MapNodeType.Interlude)
        {
            EmitSignal(
                SignalName.InterludeSelected,
                data.Id
            );
        }
    }


    // ============================================================
    // UI EVENTS
    // ============================================================

    private void OnSearchChanged(
        string newText)
    {
        EventMapGraphBuilder.RebuildGraph(
            this
        );
    }


    private void OnFilterChanged(
        long index)
    {
        EventMapGraphBuilder.RebuildGraph(
            this
        );
    }


    private void OnArrangePressed()
    {
        EventMapGraphBuilder.RebuildGraph(
            this
        );
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
            0.75f;
    }


    // ============================================================
    // INFO PANEL
    // ============================================================

    internal void UpdateSelectionPanel()
    {
        EventMapInfoPanel.Update(
            this
        );
    }
}