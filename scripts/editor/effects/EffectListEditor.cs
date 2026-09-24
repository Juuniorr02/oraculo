using Godot;
using System;
using System.Collections.Generic;


public partial class EffectListEditor : VBoxContainer
{
    private sealed class EffectData
    {
        public string Summary = "";
        public EditorEffectData Data = new EditorEffectData();
    }


    private Button addEffectButton;
    private VBoxContainer effectsList;


    private readonly List<EffectData> effects = new();


    private Window effectEditorWindow;
    private EffectEditor effectEditor;


    private int chapter = 1;


    private EventRepository eventRepository;
    private AttributeRepository attributeRepository;


    private string projectPath = "";


    public override void _Ready()
    {
        addEffectButton =
            GetNode<Button>(
                "EffectsHeader/AddEffectButton"
            );


        effectsList =
            GetNode<VBoxContainer>(
                "EffectsList"
            );


        addEffectButton.Pressed +=
            OnAddEffectPressed;
    }


    public void SetRepository(
    EventRepository repository)
{
    eventRepository =
        repository;

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


        if (string.IsNullOrWhiteSpace(
            projectPath))
        {
            attributeRepository =
                null;


            GD.PrintErr(
                "EffectListEditor: la ruta del proyecto está vacía."
            );


            return;
        }


        attributeRepository =
            new AttributeRepository(
                projectPath
            );


        GD.Print(
            "EffectListEditor: ruta del proyecto establecida: ",
            projectPath
        );


        UpdateAllSummaries();


        if (IsInsideTree())
        {
            Refresh();
        }
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
            "EffectListEditor: capítulo establecido: ",
            chapter
        );
    }


    public void SetEffects(
        List<EditorEffectData> newEffects)
    {
        effects.Clear();


        if (newEffects != null)
        {
            foreach (
                EditorEffectData effect
                in newEffects)
            {
                if (effect == null)
                {
                    continue;
                }


                EditorEffectData copy =
                    CopyData(
                        effect
                    );


                effects.Add(
                    new EffectData
                    {
                        Data =
                            copy,

                        Summary =
                            BuildSummary(
                                copy
                            )
                    }
                );
            }
        }


        Refresh();


        GD.Print(
            "EffectListEditor: cargados ",
            effects.Count,
            " efectos."
        );
    }


    public List<EditorEffectData> GetEffects()
    {
        List<EditorEffectData> result =
            new List<EditorEffectData>();


        foreach (
            EffectData effect
            in effects)
        {
            result.Add(
                CopyData(
                    effect.Data
                )
            );
        }


        return result;
    }


    public void SelectEffect(
        int index)
    {
        if (
            index < 0 ||
            index >= effects.Count)
        {
            GD.PrintErr(
                "EffectListEditor: índice de efecto inválido: ",
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
        effects.Clear();


        effects.Add(
            new EffectData
            {
                Data =
                    new EditorEffectData
                    {
                        TypeId =
                            "characterattribute",

                        AttributeId =
                            "prudence",

                        Value =
                            2
                    }
            }
        );


        effects.Add(
            new EffectData
            {
                Data =
                    new EditorEffectData
                    {
                        TypeId =
                            "prestige",

                        Value =
                            5
                    }
            }
        );


        effects.Add(
            new EffectData
            {
                Data =
                    new EditorEffectData
                    {
                        TypeId =
                            "relationship",

                        RelationshipCharacterId =
                            "aurelian",

                        Value =
                            3
                    }
            }
        );


        UpdateAllSummaries();


        Refresh();
    }


    private void OnAddEffectPressed()
    {
        EditorEffectData newEffect =
            new EditorEffectData
            {
                TypeId =
                    "prestige",

                Value =
                    0
            };


        effects.Add(
            new EffectData
            {
                Data =
                    newEffect,

                Summary =
                    BuildSummary(
                        newEffect
                    )
            }
        );


        int index =
            effects.Count - 1;


        Refresh();


        GD.Print(
            "Efecto añadido: ",
            effects[index].Summary
        );


        OpenEditor(
            index
        );
    }


    private void Refresh()
    {
        foreach (
            Node child
            in effectsList.GetChildren())
        {
            child.QueueFree();
        }


        for (
            int i = 0;
            i < effects.Count;
            i++)
        {
            int index =
                i;


            Button button =
                CreateListButton(
                    effects[index].Summary
                );


            button.Pressed +=
                () =>
                {
                    OpenEditor(
                        index
                    );
                };


            effectsList.AddChild(
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
                    Control.SizeFlags.ExpandFill
            };


        StyleBoxFlat normal =
            new StyleBoxFlat();


        normal.BgColor =
            new Color("222730");


        normal.BorderColor =
            new Color("313743");


        normal.SetBorderWidthAll(
            1
        );


        normal.SetCornerRadiusAll(
            3
        );


        normal.ContentMarginLeft =
            12;


        normal.ContentMarginRight =
            12;


        normal.ContentMarginTop =
            8;


        normal.ContentMarginBottom =
            8;


        StyleBoxFlat hover =
            new StyleBoxFlat();


        hover.BgColor =
            new Color("252D35");


        hover.BorderColor =
            new Color("4FA6A6");


        hover.SetBorderWidthAll(
            1
        );


        hover.SetCornerRadiusAll(
            3
        );


        hover.ContentMarginLeft =
            12;


        hover.ContentMarginRight =
            12;


        hover.ContentMarginTop =
            8;


        hover.ContentMarginBottom =
            8;


        StyleBoxFlat pressed =
            new StyleBoxFlat();


        pressed.BgColor =
            new Color("27343A");


        pressed.BorderColor =
            new Color("4FA6A6");


        pressed.SetBorderWidthAll(
            1
        );


        pressed.SetCornerRadiusAll(
            3
        );


        pressed.ContentMarginLeft =
            12;


        pressed.ContentMarginRight =
            12;


        pressed.ContentMarginTop =
            8;


        pressed.ContentMarginBottom =
            8;


        StyleBoxFlat focus =
            new StyleBoxFlat();


        focus.BgColor =
            new Color("222730");


        focus.BorderColor =
            new Color("4FA6A6");


        focus.SetBorderWidthAll(
            1
        );


        focus.SetCornerRadiusAll(
            3
        );


        focus.ContentMarginLeft =
            12;


        focus.ContentMarginRight =
            12;


        focus.ContentMarginTop =
            8;


        focus.ContentMarginBottom =
            8;


        button.AddThemeColorOverride(
            "font_color",
            new Color("A8AFBC")
        );


        button.AddThemeColorOverride(
            "font_hover_color",
            new Color("E7EAF0")
        );


        button.AddThemeColorOverride(
            "font_pressed_color",
            new Color("E7EAF0")
        );


        button.AddThemeColorOverride(
            "font_focus_color",
            new Color("E7EAF0")
        );


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


        return button;
    }


    private void OpenEditor(
        int index)
    {
        if (
            index < 0 ||
            index >= effects.Count)
        {
            return;
        }


        CloseEditor();


        effectEditorWindow =
            new Window
            {
                Title =
                    "Editar efecto",

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


        effectEditorWindow.CloseRequested +=
            CloseEditor;


        AddChild(
            effectEditorWindow
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


        effectEditorWindow.AddChild(
            container
        );


        effectEditor =
            GD.Load<PackedScene>(
                "res://scenes/EffectEditor.tscn"
            ).Instantiate<EffectEditor>();


        effectEditor.CustomMinimumSize =
            new Vector2(
                0,
                350
            );


        effectEditor.SizeFlagsVertical =
            Control.SizeFlags.ExpandFill;


        container.AddChild(
            effectEditor
        );


        effectEditor.SetChapter(
            chapter
        );


        effectEditor.SetRepository(
            eventRepository
        );


        effectEditor.SetProjectPath(
            projectPath
        );


        effectEditor.LoadEffect(
            CopyData(
                effects[index].Data
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
                DeleteEffect(
                    index
                );
            };


        cancelButton.Pressed +=
            OnCancelled;


        saveButton.Pressed +=
            () =>
            {
                SaveEffect(
                    index
                );
            };


        effectEditor.Cancelled +=
            OnCancelled;


        effectEditorWindow.PopupCentered();
    }


    private void SaveEffect(
        int index)
    {
        if (
            effectEditor == null ||
            index < 0 ||
            index >= effects.Count)
        {
            CloseEditor();


            return;
        }


        effects[index].Data =
            CopyData(
                effectEditor.GetEffectData()
            );


        effects[index].Summary =
            BuildSummary(
                effects[index].Data
            );


        Refresh();


        GD.Print(
            "Efecto guardado: ",
            effects[index].Summary
        );


        CloseEditor();
    }


    private void DeleteEffect(
        int index)
    {
        if (
            index >= 0 &&
            index < effects.Count)
        {
            GD.Print(
                "Efecto eliminado: ",
                effects[index].Summary
            );


            effects.RemoveAt(
                index
            );


            Refresh();
        }


        CloseEditor();
    }


    private void OnCancelled()
    {
        GD.Print(
            "Edición de efecto cancelada."
        );


        CloseEditor();
    }


    private void CloseEditor()
    {
        if (effectEditorWindow == null)
        {
            return;
        }


        Window window =
            effectEditorWindow;


        effectEditorWindow =
            null;


        effectEditor =
            null;


        window.QueueFree();
    }


    private void UpdateAllSummaries()
    {
        foreach (
            EffectData effect
            in effects)
        {
            effect.Summary =
                BuildSummary(
                    effect.Data
                );
        }
    }


    private string GetAttributeDisplayName(
        string attributeId)
    {
        if (string.IsNullOrWhiteSpace(
            attributeId))
        {
            return "Seleccionar atributo";
        }


        if (attributeRepository == null)
        {
            return "Atributo no disponible";
        }


        AttributeDefinitionData attribute =
            attributeRepository.Load(
                attributeId
            );


        if (attribute == null)
        {
            return "Atributo no encontrado";
        }


        if (!string.IsNullOrWhiteSpace(
            attribute.DisplayName))
        {
            return attribute.DisplayName;
        }


        return "Atributo sin nombre";
    }


    private static EditorEffectData CopyData(
        EditorEffectData source)
    {
        if (source == null)
        {
            return new EditorEffectData();
        }


        return new EditorEffectData
        {
            TypeId =
                source.TypeId,

            AttributeId =
                source.AttributeId,

            Value =
                source.Value,

            DecisionId =
                source.DecisionId,

            RelationshipCharacterId =
                source.RelationshipCharacterId,

            RelationshipTitle =
                source.RelationshipTitle,

            AddTitle =
                source.AddTitle
        };
    }


    private string BuildSummary(
        EditorEffectData data)
    {
        if (data == null)
        {
            return "Efecto vacío";
        }


        return data.TypeId switch
        {
            "characterattribute" =>
                $"{GetAttributeDisplayName(data.AttributeId)} {FormatSignedValue(data.Value)}",

            "prestige" =>
                $"Prestigio {FormatSignedValue(data.Value)}",

            "decision" =>
                BuildDecisionSummary(
                    data.DecisionId
                ),

            "empireattribute" =>
                $"{GetEmpireAttributeName(data.AttributeId)} {FormatSignedValue(data.Value)}",

            "relationship" =>
                $"Relación con {GetDisplayId(data.RelationshipCharacterId, "Personaje")} {FormatSignedValue(data.Value)}",

            "relationshiptitle" =>
                $"{(data.AddTitle ? "Añadir" : "Quitar")} título \"{GetDisplayId(data.RelationshipTitle, "Título")}\" a {GetDisplayId(data.RelationshipCharacterId, "Personaje")}",

            _ =>
                "Efecto desconocido"
        };
    }


    private string BuildDecisionSummary(
    string decisionId)
{
    if (string.IsNullOrWhiteSpace(
        decisionId))
    {
        return "Decisión: escribir nombre o ID";
    }


    return
        $"Decisión: {decisionId.Trim()}";
}


    private static string FormatSignedValue(
        int value)
    {
        return value > 0
            ? $"+{value}"
            : value.ToString();
    }


    private static string GetDisplayId(
        string value,
        string fallback)
    {
        return string.IsNullOrWhiteSpace(value)
            ? fallback
            : value;
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
                "Atributo del imperio"
        };
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