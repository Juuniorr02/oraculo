using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;


internal static class EventMapInfoPanel
{
    // ============================================================
    // UPDATE
    // ============================================================

    public static void Update(
        EventMapEditor editor)
    {
        if (
            editor == null ||
            editor.selectedTitleLabel == null ||
            editor.selectedInfoLabel == null ||
            editor.selectedConnectionsList == null)
        {
            return;
        }


        ClearConnections(
            editor
        );


        if (editor.selectedNode == null)
        {
            editor.selectedTitleLabel.Text =
                "Ningún nodo seleccionado";


            editor.selectedInfoLabel.Text =
                "Selecciona un nodo del mapa.";


            return;
        }


        editor.selectedTitleLabel.Text =
            editor.selectedNode.Title;


        editor.selectedInfoLabel.Text =
            BuildDetails(
                editor,
                editor.selectedNode
            );


        ShowConnections(
            editor,
            editor.selectedNode
        );
    }


    // ============================================================
    // DETAILS
    // ============================================================

    private static string BuildDetails(
        EventMapEditor editor,
        MapNodeData node)
    {
        if (node == null)
        {
            return "";
        }


        StringBuilder result =
            new StringBuilder();


        result.AppendLine(
            GetNodeTypeText(
                node.Type
            )
        );


        result.AppendLine();


        switch (node.Type)
        {
            case MapNodeType.Event:

                AppendContentJsonDetails(
                    editor,
                    result,
                    "events",
                    node.Id
                );

                break;


            case MapNodeType.Interlude:

                AppendContentJsonDetails(
                    editor,
                    result,
                    "interludes",
                    node.Id
                );

                break;


            case MapNodeType.Decision:

                AppendDecisionDetails(
                    editor,
                    result,
                    node
                );

                break;


            case MapNodeType.PersistentDecision:

                AppendFlagDetails(
                    editor,
                    result,
                    node
                );

                break;
        }


        return result.ToString();
    }


    // ============================================================
    // EVENT / INTERLUDE
    // ============================================================

    private static void AppendContentJsonDetails(
        EventMapEditor editor,
        StringBuilder result,
        string folderName,
        string id)
    {
        if (
            editor == null ||
            string.IsNullOrWhiteSpace(id))
        {
            result.AppendLine(
                "Sin ID."
            );

            return;
        }


        string filePath =
            EventMapGraphBuilder.FindContentFile(
                editor,
                folderName,
                id
            );


        if (string.IsNullOrWhiteSpace(
            filePath))
        {
            result.AppendLine(
                $"ID: {id}"
            );


            result.AppendLine();


            result.AppendLine(
                "No se encontró el archivo JSON."
            );


            return;
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


            AppendJsonElement(
                result,
                document.RootElement,
                0,
                ""
            );
        }
        catch (
            Exception exception)
        {
            result.AppendLine(
                $"Error leyendo datos: {exception.Message}"
            );
        }
    }


    // ============================================================
    // DECISION
    // ============================================================

    private static void AppendDecisionDetails(
        EventMapEditor editor,
        StringBuilder result,
        MapNodeData node)
    {
        if (
            editor == null ||
            node == null ||
            string.IsNullOrWhiteSpace(
                node.SourceEventId))
        {
            result.AppendLine(
                "No se ha podido localizar el evento origen."
            );

            return;
        }


        string filePath =
            EventMapGraphBuilder.FindContentFile(
                editor,
                "events",
                node.SourceEventId
            );


        if (string.IsNullOrWhiteSpace(
            filePath))
        {
            result.AppendLine(
                $"Evento origen: {node.SourceEventId}"
            );


            return;
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


            bool found =
                TryFindDecisionElement(
                    document.RootElement,
                    node.SourceDecisionId,
                    node.Title,
                    out JsonElement decisionElement
                );


            if (!found)
            {
                result.AppendLine(
                    $"Evento origen: {node.SourceEventId}"
                );


                result.AppendLine(
                    $"Página: {node.SourcePageId}"
                );


                result.AppendLine();


                result.AppendLine(
                    "No se encontró la definición completa de la decisión."
                );


                return;
            }


            AppendJsonElement(
                result,
                decisionElement,
                0,
                "Decisión"
            );
        }
        catch (
            Exception exception)
        {
            result.AppendLine(
                $"Error leyendo decisión: {exception.Message}"
            );
        }
    }


    private static bool TryFindDecisionElement(
        JsonElement element,
        string decisionId,
        string decisionText,
        out JsonElement result)
    {
        if (
            element.ValueKind ==
            JsonValueKind.Object)
        {
            if (
                DecisionMatchesElement(
                    element,
                    decisionId,
                    decisionText))
            {
                result =
                    element;


                return true;
            }


            foreach (
                JsonProperty property
                in element.EnumerateObject())
            {
                if (
                    TryFindDecisionElement(
                        property.Value,
                        decisionId,
                        decisionText,
                        out result
                    ))
                {
                    return true;
                }
            }
        }


        if (
            element.ValueKind ==
            JsonValueKind.Array)
        {
            foreach (
                JsonElement child
                in element.EnumerateArray())
            {
                if (
                    TryFindDecisionElement(
                        child,
                        decisionId,
                        decisionText,
                        out result
                    ))
                {
                    return true;
                }
            }
        }


        result =
            default;


        return false;
    }


    private static bool DecisionMatchesElement(
        JsonElement element,
        string decisionId,
        string decisionText)
    {
        string id =
            GetStringProperty(
                element,
                "Id"
            );


        string text =
            GetStringProperty(
                element,
                "Text"
            );


        if (
            !string.IsNullOrWhiteSpace(
                decisionId)
            &&
            string.Equals(
                id,
                decisionId,
                StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }


        return
            string.IsNullOrWhiteSpace(
                decisionId)
            &&
            !string.IsNullOrWhiteSpace(
                decisionText)
            &&
            string.Equals(
                text,
                decisionText,
                StringComparison.Ordinal
            );
    }


    // ============================================================
    // FLAG
    // ============================================================

    private static void AppendFlagDetails(
        EventMapEditor editor,
        StringBuilder result,
        MapNodeData node)
    {
        if (node == null)
        {
            result.AppendLine(
                "No se ha podido localizar el flag."
            );


            return;
        }


        result.AppendLine(
            $"ID: {FormatText(node.Id)}"
        );


        result.AppendLine();


        result.AppendLine(
            "ESTADO PERSISTENTE"
        );


        result.AppendLine(
            "Este estado puede ser producido por una decisión y utilizado posteriormente como condición."
        );


        List<MapEdgeData> incoming =
            EventMapGraphBuilder.GetIncomingEdges(
                editor,
                node.Key
            );


        List<MapEdgeData> outgoing =
            EventMapGraphBuilder.GetOutgoingEdges(
                editor,
                node.Key
            );


        result.AppendLine();


        result.AppendLine(
            "PRODUCIDO POR"
        );


        bool hasProducer =
            false;


        foreach (
            MapEdgeData edge
            in incoming)
        {
            if (
                edge.Type !=
                MapEdgeType.Produces)
            {
                continue;
            }


            MapNodeData producer =
                EventMapGraphBuilder.GetNodeByKey(
                    editor,
                    edge.FromKey
                );


            if (producer == null)
            {
                continue;
            }


            result.AppendLine(
                $"  • {producer.Title}"
            );


            if (
                !string.IsNullOrWhiteSpace(
                    producer.SourceEventId))
            {
                result.AppendLine(
                    $"    Evento: {producer.SourceEventId}"
                );
            }


            hasProducer =
                true;
        }


        if (!hasProducer)
        {
            result.AppendLine(
                "  Ninguno."
            );
        }


        result.AppendLine();


        result.AppendLine(
            "REQUERIDO POR"
        );


        bool hasConsumer =
            false;


        foreach (
            MapEdgeData edge
            in outgoing)
        {
            if (
                edge.Type !=
                MapEdgeType.Dependency)
            {
                continue;
            }


            MapNodeData consumer =
                EventMapGraphBuilder.GetNodeByKey(
                    editor,
                    edge.ToKey
                );


            if (consumer == null)
            {
                continue;
            }


            result.AppendLine(
                $"  • {consumer.Title}"
            );


            result.AppendLine(
                $"    Tipo: {GetNodeTypeText(consumer.Type)}"
            );


            if (
                !string.IsNullOrWhiteSpace(
                    consumer.Id))
            {
                result.AppendLine(
                    $"    ID: {consumer.Id}"
                );
            }


            hasConsumer =
                true;
        }


        if (!hasConsumer)
        {
            result.AppendLine(
                "  Ninguno."
            );
        }
    }


    // ============================================================
    // RELATIONS
    // ============================================================

    private static void ShowConnections(
        EventMapEditor editor,
        MapNodeData node)
    {
        List<MapEdgeData> incoming =
            EventMapGraphBuilder.GetIncomingEdges(
                editor,
                node.Key
            );


        List<MapEdgeData> outgoing =
            EventMapGraphBuilder.GetOutgoingEdges(
                editor,
                node.Key
            );


        if (
            incoming.Count == 0 &&
            outgoing.Count == 0)
        {
            Label emptyLabel =
                new Label();


            emptyLabel.Text =
                "Sin relaciones.";


            editor.selectedConnectionsList.AddChild(
                emptyLabel
            );


            return;
        }


        if (incoming.Count > 0)
        {
            Label incomingLabel =
                new Label();


            incomingLabel.Text =
                "Entradas";


            editor.selectedConnectionsList.AddChild(
                incomingLabel
            );


            foreach (
                MapEdgeData edge
                in incoming)
            {
                AddConnectionButton(
                    editor,
                    edge,
                    false
                );
            }
        }


        if (outgoing.Count > 0)
        {
            Label outgoingLabel =
                new Label();


            outgoingLabel.Text =
                "Salidas";


            editor.selectedConnectionsList.AddChild(
                outgoingLabel
            );


            foreach (
                MapEdgeData edge
                in outgoing)
            {
                AddConnectionButton(
                    editor,
                    edge,
                    true
                );
            }
        }
    }


    private static void AddConnectionButton(
        EventMapEditor editor,
        MapEdgeData edge,
        bool outgoing)
    {
        MapNodeData target =
            outgoing
                ? EventMapGraphBuilder.GetNodeByKey(
                    editor,
                    edge.ToKey
                )
                : EventMapGraphBuilder.GetNodeByKey(
                    editor,
                    edge.FromKey
                );


        if (target == null)
        {
            return;
        }


        Button button =
            new Button();


        button.Text =
            $"{GetEdgeTypeText(edge.Type)}: {target.Title}";


        button.Alignment =
            HorizontalAlignment.Left;


        button.TooltipText =
            target.Id;


        string targetKey =
            target.Key;


        button.Pressed +=
            () =>
            {
                editor.FocusNode(
                    targetKey
                );
            };


        editor.selectedConnectionsList.AddChild(
            button
        );
    }


    private static void ClearConnections(
        EventMapEditor editor)
    {
        foreach (
            Node child
            in editor.selectedConnectionsList.GetChildren())
        {
            child.QueueFree();
        }
    }


    // ============================================================
    // JSON
    // ============================================================

    private static void AppendJsonElement(
        StringBuilder result,
        JsonElement element,
        int indent,
        string name)
    {
        string indentation =
            new string(
                ' ',
                indent * 2
            );


        if (
            element.ValueKind ==
            JsonValueKind.Object)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                result.AppendLine(
                    indentation +
                    FormatPropertyName(name)
                );
            }


            foreach (
                JsonProperty property
                in element.EnumerateObject())
            {
                AppendJsonElement(
                    result,
                    property.Value,
                    indent + (
                        string.IsNullOrWhiteSpace(name)
                            ? 0
                            : 1
                    ),
                    property.Name
                );
            }


            return;
        }


        if (
            element.ValueKind ==
            JsonValueKind.Array)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                result.AppendLine(
                    indentation +
                    FormatPropertyName(name)
                );
            }


            if (element.GetArrayLength() == 0)
            {
                result.AppendLine(
                    indentation +
                    "  (ninguno)"
                );


                return;
            }


            int index =
                1;


            foreach (
                JsonElement child
                in element.EnumerateArray())
            {
                result.AppendLine(
                    indentation +
                    $"  [{index}]"
                );


                AppendJsonElement(
                    result,
                    child,
                    indent + 2,
                    ""
                );


                index++;
            }


            return;
        }


        string value;


        switch (element.ValueKind)
        {
            case JsonValueKind.String:

                value =
                    element.GetString() ?? "";

                break;


            case JsonValueKind.Number:

                value =
                    element.ToString();

                break;


            case JsonValueKind.True:

                value =
                    "Sí";

                break;


            case JsonValueKind.False:

                value =
                    "No";

                break;


            case JsonValueKind.Null:

                value =
                    "null";

                break;


            default:

                value =
                    element.ToString();

                break;
        }


        if (string.IsNullOrWhiteSpace(name))
        {
            result.AppendLine(
                indentation +
                value
            );
        }
        else
        {
            result.AppendLine(
                indentation +
                $"{FormatPropertyName(name)}: {value}"
            );
        }
    }


    private static string FormatPropertyName(
        string propertyName)
    {
        return propertyName switch
        {
            "Id" => "ID",
            "Title" => "Título",
            "Chapter" => "Capítulo",
            "Year" => "Año base",
            "WorldYear" => "Año mundial",
            "Conditions" => "Condiciones",
            "Pages" => "Páginas",
            "Decisions" => "Decisiones",
            "Effects" => "Efectos",
            "Text" => "Texto",
            "Type" => "Tipo",
            "TypeId" => "Tipo",
            "AttributeId" => "Atributo",
            "DecisionId" => "Decisión",
            "Operator" => "Operador",
            "Value" => "Valor",
            "NextPageId" => "Siguiente página",
            "NextEventId" => "Siguiente evento",
            "NextInterludeId" => "Siguiente interludio",
            "NextContentId" => "Siguiente contenido",
            "ImagePath" => "Imagen",
            _ => propertyName
        };
    }


    private static string GetStringProperty(
        JsonElement element,
        string propertyName)
    {
        return EventMapGraphBuilder.GetStringProperty(
            element,
            propertyName
        );
    }


    // ============================================================
    // FORMATTING
    // ============================================================

    private static string FormatText(
        string value)
    {
        return EventMapGraphBuilder.FormatText(
            value
        );
    }


    private static string GetEdgeTypeText(
        MapEdgeType type)
    {
        return type switch
        {
            MapEdgeType.Contains =>
                "Contiene",

            MapEdgeType.Produces =>
                "Produce",

            MapEdgeType.Dependency =>
                "Requiere",

            MapEdgeType.Continuity =>
                "Continúa",

            _ =>
                "Relación"
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