using Godot;
using System.Collections.Generic;

public partial class ConditionListEditor : VBoxContainer
{
    private sealed class ConditionData
    {
        public string Summary = "";
        public EditorConditionData Data = new EditorConditionData();
    }


    private Button addConditionButton;
    private VBoxContainer conditionsList;

    private readonly List<ConditionData> conditions = new();

    private Window conditionEditorWindow;
    private ConditionEditor conditionEditor;

    private int chapter = 1;

    private EventRepository eventRepository;
    private DecisionDatabase decisionDatabase;

    private string projectPath = "";


    public override void _Ready()
    {
        addConditionButton =
            GetNode<Button>(
                "ConditionsHeader/AddConditionButton"
            );

        conditionsList =
            GetNode<VBoxContainer>(
                "ConditionsList"
            );


        addConditionButton.Pressed +=
            OnAddConditionPressed;
    }


    public void SetRepository(
        EventRepository repository)
    {
        eventRepository =
            repository;


        if (eventRepository != null)
        {
            decisionDatabase =
                new DecisionDatabase(
                    eventRepository
                );
        }
        else
        {
            decisionDatabase =
                null;
        }


        UpdateAllSummaries();


        if (IsInsideTree())
        {
            Refresh();
        }
    }


    public void SetProjectPath(
        string path)
    {
        projectPath =
            path ?? "";


        GD.Print(
            "ConditionListEditor: ruta del proyecto establecida: ",
            projectPath
        );
    }


    public void SetChapter(
        int eventChapter)
    {
        chapter =
            Mathf.Clamp(
                eventChapter,
                1,
                7
            );


        GD.Print(
            "ConditionListEditor: capítulo establecido: ",
            chapter
        );
    }


    public void SetConditions(
        List<EditorConditionData> newConditions)
    {
        conditions.Clear();


        if (newConditions != null)
        {
            foreach (
                EditorConditionData condition
                in newConditions)
            {
                if (condition == null)
                {
                    continue;
                }


                EditorConditionData copy =
                    CopyData(condition);


                conditions.Add(
                    new ConditionData
                    {
                        Data =
                            copy,

                        Summary =
                            BuildSummary(copy)
                    }
                );
            }
        }


        Refresh();


        GD.Print(
            "ConditionListEditor: cargadas ",
            conditions.Count,
            " condiciones."
        );
    }


    public List<EditorConditionData> GetConditions()
    {
        List<EditorConditionData> result =
            new List<EditorConditionData>();


        foreach (
            ConditionData condition
            in conditions)
        {
            result.Add(
                CopyData(
                    condition.Data
                )
            );
        }


        return result;
    }


    public void SelectCondition(
        int index)
    {
        if (
            index < 0 ||
            index >= conditions.Count)
        {
            GD.PrintErr(
                "ConditionListEditor: índice de condición inválido: ",
                index
            );

            return;
        }

        OpenEditor(
            index
        );
    }


    public void LoadTestData()
    {
        conditions.Clear();


        conditions.Add(
            new ConditionData
            {
                Data =
                    new EditorConditionData
                    {
                        TypeId =
                            "characterattribute",

                        AttributeId =
                            "prudence",

                        MinimumValue =
                            6,

                        MaximumValue =
                            10
                    }
            }
        );


        conditions.Add(
            new ConditionData
            {
                Data =
                    new EditorConditionData
                    {
                        TypeId =
                            "prestige",

                        MinimumValue =
                            10,

                        MaximumValue =
                            30
                    }
            }
        );


        UpdateAllSummaries();

        Refresh();
    }


    private void OnAddConditionPressed()
    {
        EditorConditionData newCondition =
            new EditorConditionData
            {
                TypeId =
                    "characterattribute",

                AttributeId =
                    "",

                MinimumValue =
                    -10,

                MaximumValue =
                    10
            };


        conditions.Add(
            new ConditionData
            {
                Data =
                    newCondition,

                Summary =
                    BuildSummary(
                        newCondition
                    )
            }
        );


        int index =
            conditions.Count - 1;


        Refresh();


        GD.Print(
            "Condición añadida: ",
            conditions[index].Summary
        );


        OpenEditor(index);
    }


    private void Refresh()
    {
        foreach (
            Node child
            in conditionsList.GetChildren())
        {
            child.QueueFree();
        }


        for (
            int i = 0;
            i < conditions.Count;
            i++)
        {
            int index =
                i;


            Button button =
                CreateListButton(
                    conditions[index].Summary
                );


            button.Pressed +=
                () =>
                {
                    OpenEditor(index);
                };


            conditionsList.AddChild(
                button
            );
        }
    }


    private static Button CreateListButton(
    string text)
{
    Button button =
        new Button
        {
            Text =
                text,

            CustomMinimumSize =
                new Vector2(
                    0,
                    46
                ),

            Alignment =
                HorizontalAlignment.Left,

            SizeFlagsHorizontal =
                Control.SizeFlags.ExpandFill,

            ClipText =
                true
        };


    StyleBoxFlat normal =
        new StyleBoxFlat
        {
            BgColor =
                new Color(
                    "222730"
                ),

            BorderColor =
                new Color(
                    "313743"
                ),

            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,

            CornerRadiusTopLeft = 3,
            CornerRadiusTopRight = 3,
            CornerRadiusBottomLeft = 3,
            CornerRadiusBottomRight = 3,

            ContentMarginLeft = 12,
            ContentMarginRight = 12,
            ContentMarginTop = 8,
            ContentMarginBottom = 8
        };


    StyleBoxFlat hover =
        new StyleBoxFlat
        {
            BgColor =
                new Color(
                    "252D35"
                ),

            BorderColor =
                new Color(
                    "4FA6A6"
                ),

            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,

            CornerRadiusTopLeft = 3,
            CornerRadiusTopRight = 3,
            CornerRadiusBottomLeft = 3,
            CornerRadiusBottomRight = 3,

            ContentMarginLeft = 12,
            ContentMarginRight = 12,
            ContentMarginTop = 8,
            ContentMarginBottom = 8
        };


    StyleBoxFlat pressed =
        new StyleBoxFlat
        {
            BgColor =
                new Color(
                    "27343A"
                ),

            BorderColor =
                new Color(
                    "4FA6A6"
                ),

            BorderWidthLeft = 2,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,

            CornerRadiusTopLeft = 3,
            CornerRadiusTopRight = 3,
            CornerRadiusBottomLeft = 3,
            CornerRadiusBottomRight = 3,

            ContentMarginLeft = 11,
            ContentMarginRight = 12,
            ContentMarginTop = 8,
            ContentMarginBottom = 8
        };


    StyleBoxFlat focus =
        new StyleBoxFlat
        {
            BgColor =
                new Color(
                    "222730"
                ),

            BorderColor =
                new Color(
                    "4FA6A6"
                ),

            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,

            CornerRadiusTopLeft = 3,
            CornerRadiusTopRight = 3,
            CornerRadiusBottomLeft = 3,
            CornerRadiusBottomRight = 3,

            ContentMarginLeft = 12,
            ContentMarginRight = 12,
            ContentMarginTop = 8,
            ContentMarginBottom = 8
        };


    button.AddThemeStyleboxOverride(
        "normal",
        normal
    );

    button.AddThemeStyleboxOverride(
        "hover",
        hover
    );

    button.AddThemeStyleboxOverride(
        "pressed",
        pressed
    );

    button.AddThemeStyleboxOverride(
        "focus",
        focus
    );


    button.AddThemeColorOverride(
        "font_color",
        new Color(
            "A8AFBC"
        )
    );

    button.AddThemeColorOverride(
        "font_hover_color",
        new Color(
            "E7EAF0"
        )
    );

    button.AddThemeColorOverride(
        "font_pressed_color",
        new Color(
            "E7EAF0"
        )
    );

    button.AddThemeColorOverride(
        "font_focus_color",
        new Color(
            "E7EAF0"
        )
    );


    return button;
}


    private void OpenEditor(
        int index)
    {
        if (
            index < 0 ||
            index >= conditions.Count)
        {
            return;
        }


        CloseEditor();


        conditionEditorWindow =
            new Window
            {
                Title =
                    "Editar condición",

                Size =
                    new Vector2I(
                        600,
                        500
                    ),

                MinSize =
                    new Vector2I(
                        500,
                        400
                    ),

                Exclusive =
                    true,

                Unresizable =
                    false
            };


        conditionEditorWindow.CloseRequested +=
            CloseEditor;


        AddChild(
            conditionEditorWindow
        );


        VBoxContainer container =
            new VBoxContainer();


        container.SetAnchorsAndOffsetsPreset(
            LayoutPreset.FullRect
        );


        container.AddThemeConstantOverride(
            "separation",
            10
        );


        conditionEditorWindow.AddChild(
            container
        );


        conditionEditor =
            GD.Load<PackedScene>(
                "res://scenes/ConditionEditor.tscn"
            ).Instantiate<ConditionEditor>();


        conditionEditor.CustomMinimumSize =
            new Vector2(
                0,
                350
            );


        conditionEditor.SizeFlagsVertical =
            Control.SizeFlags.ExpandFill;


        container.AddChild(
            conditionEditor
        );


        conditionEditor.SetChapter(
            chapter
        );


        conditionEditor.SetRepository(
            eventRepository
        );


        conditionEditor.SetProjectPath(
            projectPath
        );


        conditionEditor.LoadCondition(
            CopyData(
                conditions[index].Data
            )
        );


        HBoxContainer buttons =
            new HBoxContainer
            {
                Alignment =
                    BoxContainer.AlignmentMode.End
            };


        container.AddChild(
            buttons
        );


        Button deleteButton =
            CreateActionButton(
                "Eliminar"
            );


        Button cancelButton =
            CreateActionButton(
                "Cancelar"
            );


        Button saveButton =
            CreateActionButton(
                "Guardar"
            );


        buttons.AddChild(deleteButton);
        buttons.AddChild(cancelButton);
        buttons.AddChild(saveButton);


        deleteButton.Pressed +=
            () =>
            {
                DeleteCondition(index);
            };


        cancelButton.Pressed +=
            OnCancelled;


        saveButton.Pressed +=
            () =>
            {
                SaveCondition(index);
            };


        conditionEditorWindow.PopupCentered();
    }


    private void SaveCondition(
        int index)
    {
        if (
            conditionEditor == null ||
            index < 0 ||
            index >= conditions.Count)
        {
            CloseEditor();

            return;
        }


        conditions[index].Data =
            CopyData(
                conditionEditor.GetConditionData()
            );


        conditions[index].Summary =
            BuildSummary(
                conditions[index].Data
            );


        Refresh();


        GD.Print(
            "Condición guardada: ",
            conditions[index].Summary
        );


        CloseEditor();
    }


    private void DeleteCondition(
        int index)
    {
        if (
            index >= 0 &&
            index < conditions.Count)
        {
            GD.Print(
                "Condición eliminada: ",
                conditions[index].Summary
            );


            conditions.RemoveAt(
                index
            );


            Refresh();
        }


        CloseEditor();
    }


    private void OnCancelled()
    {
        GD.Print(
            "Edición de condición cancelada."
        );


        CloseEditor();
    }


    private void CloseEditor()
    {
        if (conditionEditorWindow == null)
        {
            return;
        }


        Window window =
            conditionEditorWindow;


        conditionEditorWindow =
            null;


        conditionEditor =
            null;


        window.QueueFree();
    }


    private void UpdateAllSummaries()
    {
        foreach (
            ConditionData condition
            in conditions)
        {
            condition.Summary =
                BuildSummary(
                    condition.Data
                );
        }
    }


    private static EditorConditionData CopyData(
        EditorConditionData source)
    {
        if (source == null)
        {
            return new EditorConditionData();
        }


        return new EditorConditionData
        {
            TypeId =
                source.TypeId,

            AttributeId =
                source.AttributeId,

            MinimumValue =
                source.MinimumValue,

            MaximumValue =
                source.MaximumValue,

            DecisionId =
                source.DecisionId,

            RelationshipCharacterId =
                source.RelationshipCharacterId,

            RelationshipTitle =
                source.RelationshipTitle
        };
    }


    private string BuildSummary(
        EditorConditionData data)
    {
        if (data == null)
        {
            return "Condición vacía";
        }


        string requirement =
            BuildRangeRequirement(
                data.MinimumValue,
                data.MaximumValue
            );


        return data.TypeId switch
        {
            "characterattribute" =>
                $"{GetDisplayId(data.AttributeId, "Atributo")} {requirement}",

            "prestige" =>
                $"Prestigio {requirement}",

            "year" =>
                $"Año {requirement}",

            "decision" =>
                BuildDecisionSummary(
                    data.DecisionId
                ),

            "empireattribute" =>
                $"{GetEmpireAttributeName(data.AttributeId)} {requirement}",

            "relationship" =>
                $"Relación con {GetDisplayId(data.RelationshipCharacterId, "Personaje")} {requirement}",

            "relationshiptitle" =>
                $"Título con {GetDisplayId(data.RelationshipCharacterId, "Personaje")}: {GetDisplayId(data.RelationshipTitle, "Título")}",

            _ =>
                "Condición desconocida"
        };
    }


    private string BuildDecisionSummary(
        string decisionId)
    {
        if (string.IsNullOrWhiteSpace(decisionId))
        {
            return "Decisión: seleccionar decisión";
        }


        if (decisionDatabase == null)
        {
            return
                $"Decisión: {decisionId}";
        }


        DecisionDatabase.DecisionReference decision =
            decisionDatabase.GetDecision(
                decisionId
            );


        if (decision == null)
        {
            return
                $"Decisión no encontrada: {decisionId}";
        }


        string eventName =
            string.IsNullOrWhiteSpace(
                decision.EventTitle)
                ? decision.EventId
                : decision.EventTitle;


        string decisionText =
            string.IsNullOrWhiteSpace(
                decision.Text)
                ? "Decisión sin texto"
                : decision.Text;


        return
            $"Decisión: {eventName} → Página {decision.PageNumber} → {decisionText}";
    }


    private static string BuildRangeRequirement(
        int minimum,
        int maximum)
    {
        if (
            minimum > -10 &&
            maximum >= 10)
        {
            return $"≥ {minimum}";
        }


        if (
            minimum <= -10 &&
            maximum < 10)
        {
            return $"≤ {maximum}";
        }


        if (minimum == maximum)
        {
            return $"= {minimum}";
        }


        return $"entre {minimum} y {maximum}";
    }


    private static string GetEmpireAttributeName(
        string attributeId)
    {
        return attributeId switch
        {
            "stability" =>
                "Estabilidad",

            "prosperity" =>
                "Prosperidad",

            "church_influence" =>
                "Influencia de la Iglesia",

            "social_tension" =>
                "Tensión Social",

            "northern_threat" =>
                "Amenaza del Norte",

            "legitimacy" =>
                "Legitimidad",

            _ =>
                GetDisplayId(
                    attributeId,
                    "Atributo del imperio"
                )
        };
    }


    private static string GetDisplayId(
        string value,
        string fallback)
    {
        return string.IsNullOrWhiteSpace(value)
            ? fallback
            : value;
    }


    private static Button CreateActionButton(
        string text)
    {
        return new Button
        {
            Text =
                text,

            CustomMinimumSize =
                new Vector2(
                    110,
                    35
                )
        };
    }
}