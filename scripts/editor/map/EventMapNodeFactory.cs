using Godot;
using System;


internal static class EventMapNodeFactory
{
    // ============================================================
    // CREATE
    // ============================================================

    public static GraphNode Create(
        EventMapEditor editor,
        MapNodeData data)
    {
        if (
            editor == null ||
            data == null)
        {
            return null;
        }


        GraphNode graphNode =
            new GraphNode();


        graphNode.Name =
            "GraphNode_" +
            Guid.NewGuid().ToString(
                "N"
            );


        graphNode.Title =
            GetNodeTypeText(
                data.Type
            );


        float nodeWidth =
            GetNodeWidth(
                data.Type
            );


        float nodeHeight =
            GetNodeHeight(
                data.Type
            );


        graphNode.CustomMinimumSize =
            new Vector2(
                nodeWidth,
                nodeHeight
            );


        graphNode.Size =
            new Vector2(
                nodeWidth,
                nodeHeight
            );


        graphNode.SetMeta(
            "map_key",
            data.Key
        );


        graphNode.SetMeta(
            "map_type",
            (int)data.Type
        );


        graphNode.AddThemeStyleboxOverride(
            "titlebar",
            CreateTitleBarStyleBox(
                GetNodeTitleColor(
                    data.Type
                )
            )
        );


        graphNode.AddThemeStyleboxOverride(
            "titlebar_selected",
            CreateTitleBarStyleBox(
                GetNodeTitleSelectedColor(
                    data.Type
                )
            )
        );


        // ========================================================
        // CONTENT
        // ========================================================

        VBoxContainer content =
            new VBoxContainer();


        content.AddThemeConstantOverride(
            "separation",
            5
        );


        float contentWidth =
            nodeWidth - 24.0f;


        content.CustomMinimumSize =
            new Vector2(
                contentWidth,
                0
            );


        content.SizeFlagsHorizontal =
            Control.SizeFlags.Fill;


        graphNode.AddChild(
            content
        );


        Label titleLabel =
            new Label();


        titleLabel.Text =
            data.Title;


        titleLabel.AutowrapMode =
            TextServer.AutowrapMode.WordSmart;


        titleLabel.CustomMinimumSize =
            new Vector2(
                contentWidth,
                0
            );


        titleLabel.SizeFlagsHorizontal =
            Control.SizeFlags.Fill;


        titleLabel.AddThemeFontSizeOverride(
            "font_size",
            16
        );


        content.AddChild(
            titleLabel
        );


        if (!string.IsNullOrWhiteSpace(
            data.Id))
        {
            Label idLabel =
                new Label();


            idLabel.Text =
                $"ID: {data.Id}";


            idLabel.AutowrapMode =
                TextServer.AutowrapMode.Arbitrary;


            idLabel.CustomMinimumSize =
                new Vector2(
                    contentWidth,
                    0
                );


            idLabel.SizeFlagsHorizontal =
                Control.SizeFlags.Fill;


            content.AddChild(
                idLabel
            );
        }


        foreach (
            string line
            in data.InfoLines)
        {
            Label infoLabel =
                new Label();


            infoLabel.Text =
                line;


            infoLabel.AutowrapMode =
                TextServer.AutowrapMode.Arbitrary;


            infoLabel.CustomMinimumSize =
                new Vector2(
                    contentWidth,
                    0
                );


            infoLabel.SizeFlagsHorizontal =
                Control.SizeFlags.Fill;


            content.AddChild(
                infoLabel
            );
        }


        // ========================================================
        // RELACIONES
        // ========================================================

        Label relationLabel =
            new Label();


        int incoming =
            EventMapGraphBuilder.GetIncomingEdges(
                editor,
                data.Key
            ).Count;


        int outgoing =
            EventMapGraphBuilder.GetOutgoingEdges(
                editor,
                data.Key
            ).Count;


        relationLabel.Text =
            $"Relaciones: {incoming} ← / {outgoing} →";


        relationLabel.AutowrapMode =
            TextServer.AutowrapMode.Arbitrary;


        relationLabel.CustomMinimumSize =
            new Vector2(
                contentWidth,
                0
            );


        relationLabel.SizeFlagsHorizontal =
            Control.SizeFlags.Fill;


        content.AddChild(
            relationLabel
        );


        // ========================================================
        // OPEN
        // ========================================================

        if (
            data.Type ==
            MapNodeType.Event
            ||
            data.Type ==
            MapNodeType.Interlude)
        {
            Button openButton =
                new Button();


            openButton.Text =
                "Abrir";


            openButton.CustomMinimumSize =
                new Vector2(
                    0,
                    30
                );


            openButton.SizeFlagsHorizontal =
                Control.SizeFlags.Fill;


            openButton.Pressed +=
                () =>
                {
                    editor.OpenContent(
                        data
                    );
                };


            content.AddChild(
                openButton
            );
        }
        else if (
            data.Type ==
            MapNodeType.Decision
            &&
            !string.IsNullOrWhiteSpace(
                data.SourceEventId))
        {
            Button openButton =
                new Button();


            openButton.Text =
                "Abrir evento";


            openButton.CustomMinimumSize =
                new Vector2(
                    0,
                    30
                );


            openButton.SizeFlagsHorizontal =
                Control.SizeFlags.Fill;


            openButton.Pressed +=
                () =>
                {
                    editor.EmitSignal(
                        EventMapEditor.SignalName.EventSelected,
                        data.SourceEventId
                    );
                };


            content.AddChild(
                openButton
            );
        }


        // ========================================================
        // PORT
        // ========================================================

        Color portColor =
            GetNodePortColor(
                data.Type
            );


        graphNode.SetSlot(
            0,
            true,
            0,
            portColor,
            true,
            0,
            portColor
        );


        return graphNode;
    }


    // ============================================================
    // SIZE
    // ============================================================

    public static float GetNodeWidth(
        MapNodeType type)
    {
        return type switch
        {
            MapNodeType.Event => 320,
            MapNodeType.Interlude => 320,
            MapNodeType.Decision => 340,
            MapNodeType.PersistentDecision => 300,
            _ => 320
        };
    }


    public static float GetNodeHeight(
        MapNodeType type)
    {
        return type switch
        {
            MapNodeType.Event => 210,
            MapNodeType.Interlude => 175,
            MapNodeType.Decision => 210,
            MapNodeType.PersistentDecision => 160,
            _ => 180
        };
    }


    // ============================================================
    // TITLE
    // ============================================================

    private static StyleBoxFlat CreateTitleBarStyleBox(
        Color color)
    {
        StyleBoxFlat styleBox =
            new StyleBoxFlat();


        styleBox.BgColor =
            color;


        styleBox.CornerRadiusTopLeft =
            6;


        styleBox.CornerRadiusTopRight =
            6;


        styleBox.BorderWidthTop =
            1;


        styleBox.BorderWidthLeft =
            1;


        styleBox.BorderWidthRight =
            1;


        styleBox.BorderColor =
            color.Lightened(
                0.18f
            );


        styleBox.ContentMarginLeft =
            10;


        styleBox.ContentMarginRight =
            10;


        styleBox.ContentMarginTop =
            6;


        styleBox.ContentMarginBottom =
            6;


        return styleBox;
    }


    private static Color GetNodeTitleColor(
        MapNodeType type)
    {
        return type switch
        {
            MapNodeType.Event =>
                new Color(
                    0.20f,
                    0.40f,
                    0.68f
                ),

            MapNodeType.Interlude =>
                new Color(
                    0.48f,
                    0.30f,
                    0.66f
                ),

            MapNodeType.Decision =>
                new Color(
                    0.24f,
                    0.50f,
                    0.80f
                ),

            MapNodeType.PersistentDecision =>
                new Color(
                    0.72f,
                    0.52f,
                    0.12f
                ),

            _ =>
                new Color(
                    0.35f,
                    0.35f,
                    0.35f
                )
        };
    }


    private static Color GetNodeTitleSelectedColor(
        MapNodeType type)
    {
        return type switch
        {
            MapNodeType.Event =>
                new Color(
                    0.30f,
                    0.58f,
                    0.95f
                ),

            MapNodeType.Interlude =>
                new Color(
                    0.68f,
                    0.46f,
                    0.88f
                ),

            MapNodeType.Decision =>
                new Color(
                    0.36f,
                    0.68f,
                    1.00f
                ),

            MapNodeType.PersistentDecision =>
                new Color(
                    0.92f,
                    0.70f,
                    0.22f
                ),

            _ =>
                new Color(
                    0.55f,
                    0.55f,
                    0.55f
                )
        };
    }


    private static Color GetNodePortColor(
        MapNodeType type)
    {
        return type switch
        {
            MapNodeType.Event =>
                Colors.SteelBlue,

            MapNodeType.Interlude =>
                Colors.MediumPurple,

            MapNodeType.Decision =>
                Colors.CornflowerBlue,

            MapNodeType.PersistentDecision =>
                Colors.Goldenrod,

            _ =>
                Colors.White
        };
    }


    private static string GetNodeTypeText(
        MapNodeType type)
    {
        return type switch
        {
            MapNodeType.Event =>
                "EVENTO",

            MapNodeType.Interlude =>
                "INTERLUDIO",

            MapNodeType.Decision =>
                "DECISIÓN",

            MapNodeType.PersistentDecision =>
                "FLAG",

            _ =>
                "NODO"
        };
    }
}