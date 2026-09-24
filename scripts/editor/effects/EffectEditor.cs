using Godot;
using System.Collections.Generic;


public partial class EffectEditor : PanelContainer
{
    [Signal]
    public delegate void SavedEventHandler();

    [Signal]
    public delegate void CancelledEventHandler();


    private OptionButton typeOption;

    private Button removeButton;

    private VBoxContainer fieldsContainer;


    private EffectDefinitionDatabase effectDatabase;

    private CharacterDatabase characterDatabase;

    private RelationshipDatabase relationshipDatabase;

    private EditorEffectData effectData;

    private EventRepository eventRepository;

    private AttributeRepository attributeRepository;

    private ChapterRepository chapterRepository;

    private int chapter = 1;


    public override void _Ready()
    {
        typeOption =
            GetNode<OptionButton>(
                "MarginContainer/VBoxContainer/Header/TypeOption"
            );


        removeButton =
            GetNode<Button>(
                "MarginContainer/VBoxContainer/Header/RemoveButton"
            );


        fieldsContainer =
            GetNode<VBoxContainer>(
                "MarginContainer/VBoxContainer/FieldsContainer"
            );


        effectDatabase =
            new EffectDefinitionDatabase();


        effectData =
            new EditorEffectData();


        PopulateEffectTypes();


        typeOption.ItemSelected +=
            OnEffectTypeSelected;


        removeButton.Pressed +=
            OnRemovePressed;


        GD.Print(
            "EffectEditor iniciado."
        );
    }


    public void SetRepository(
        EventRepository repository)
    {
        eventRepository =
            repository;
    }


    public void SetProjectPath(
        string projectPath)
    {
        if (string.IsNullOrWhiteSpace(
            projectPath))
        {
            characterDatabase = null;
            relationshipDatabase = null;
            attributeRepository = null;
            chapterRepository = null;


            GD.PrintErr(
                "EffectEditor: la ruta del proyecto está vacía."
            );


            return;
        }


        characterDatabase =
            new CharacterDatabase(
                projectPath
            );


        relationshipDatabase =
            new RelationshipDatabase(
                projectPath
            );


        attributeRepository =
            new AttributeRepository(
                projectPath
            );


        chapterRepository =
            new ChapterRepository(
                projectPath
            );


        GD.Print(
            "EffectEditor: ruta del proyecto establecida: ",
            projectPath
        );
    }


    public void SetChapter(
        int eventChapter)
    {
        chapter =
            eventChapter;


        GD.Print(
            "EffectEditor: capítulo establecido: ",
            chapter
        );
    }


    public void LoadEffect(
        EditorEffectData data)
    {
        if (data == null)
        {
            return;
        }


        effectData =
            data;


        SelectEffectType(
            effectData.TypeId
        );
    }


    public EditorEffectData GetEffectData()
    {
        return effectData;
    }


    private void PopulateEffectTypes()
    {
        typeOption.Clear();


        foreach (
            EffectTypeDefinition definition
            in effectDatabase.GetDefinitions())
        {
            typeOption.AddItem(
                definition.DisplayName
            );


            int index =
                typeOption.ItemCount - 1;


            typeOption.SetItemMetadata(
                index,
                definition.Id
            );
        }
    }


    private void SelectEffectType(
        string typeId)
    {
        for (
            int i = 0;
            i < typeOption.ItemCount;
            i++)
        {
            string id =
                typeOption.GetItemMetadata(
                    i
                ).AsString();


            if (id == typeId)
            {
                typeOption.Select(
                    i
                );


                UpdateEffectType();


                return;
            }
        }


        if (typeOption.ItemCount > 0)
        {
            typeOption.Select(
                0
            );


            UpdateEffectType();
        }
    }


    private void OnEffectTypeSelected(
        long index)
    {
        UpdateEffectType();
    }


    private void UpdateEffectType()
    {
        ClearFields();


        if (typeOption.Selected < 0)
        {
            return;
        }


        string typeId =
            typeOption.GetItemMetadata(
                typeOption.Selected
            ).AsString();


        effectData.TypeId =
            typeId;


        switch (typeId)
        {
            case "characterattribute":
                BuildCharacterAttributeFields();
                break;


            case "prestige":
                BuildPrestigeFields();
                break;


            case "decision":
                BuildDecisionFields();
                break;


            case "empireattribute":
                BuildEmpireAttributeFields();
                break;


            case "relationship":
                BuildRelationshipFields();
                break;


            case "relationshiptitle":
                BuildRelationshipTitleFields();
                break;
        }
    }


    private void BuildCharacterAttributeFields()
    {
        AddSectionLabel(
            "ATRIBUTO"
        );


        if (
            attributeRepository == null ||
            chapterRepository == null)
        {
            AddPlaceholderField(
                "No hay un proyecto cargado."
            );


            AddValueField();

            return;
        }


        ChapterDefinitionData currentChapter =
            GetCurrentChapterData();


        if (currentChapter == null)
        {
            AddPlaceholderField(
                "No se ha encontrado el capítulo."
            );


            AddValueField();

            return;
        }


        OptionButton attributeOption =
            CreateStyledOptionButton();


        attributeOption.AddItem(
            "Seleccionar atributo..."
        );


        attributeOption.SetItemMetadata(
            0,
            ""
        );


        foreach (
            string attributeId
            in currentChapter.AttributeIds)
        {
            if (string.IsNullOrWhiteSpace(
                attributeId))
            {
                continue;
            }


            AttributeDefinitionData attribute =
                attributeRepository.Load(
                    attributeId
                );


            if (attribute == null)
            {
                continue;
            }


            attributeOption.AddItem(
                attribute.DisplayName
            );


            int index =
                attributeOption.ItemCount - 1;


            attributeOption.SetItemMetadata(
                index,
                attribute.Id
            );
        }


        attributeOption.ItemSelected +=
            index =>
            {
                Variant metadata =
                    attributeOption.GetItemMetadata(
                        (int)index
                    );


                effectData.AttributeId =
                    metadata.AsString();


                GD.Print(
                    "EffectEditor: atributo seleccionado: ",
                    effectData.AttributeId
                );
            };


        SelectAttribute(
            attributeOption
        );


        fieldsContainer.AddChild(
            attributeOption
        );


        AddValueField();
    }


    private ChapterDefinitionData GetCurrentChapterData()
    {
        if (chapterRepository == null)
        {
            return null;
        }


        List<ChapterDefinitionData> chapters =
            chapterRepository.LoadAll();


        foreach (
            ChapterDefinitionData chapterData
            in chapters)
        {
            if (chapterData == null)
            {
                continue;
            }


            if (chapterData.Number == chapter)
            {
                return chapterData;
            }
        }


        return null;
    }


    private void SelectAttribute(
        OptionButton attributeOption)
    {
        if (
            string.IsNullOrWhiteSpace(
                effectData.AttributeId))
        {
            attributeOption.Select(
                0
            );


            return;
        }


        for (
            int i = 1;
            i < attributeOption.ItemCount;
            i++)
        {
            Variant metadata =
                attributeOption.GetItemMetadata(
                    i
                );


            if (
                metadata.AsString() ==
                effectData.AttributeId)
            {
                attributeOption.Select(
                    i
                );


                return;
            }
        }


        attributeOption.Select(
            0
        );
    }


    private void BuildPrestigeFields()
    {
        AddValueField();
    }


    private void BuildDecisionFields()
    {
        AddSectionLabel(
            "DECISIÓN"
        );


        LineEdit decisionEdit =
            new LineEdit
            {
                PlaceholderText =
                    "Nombre o ID de la decisión...",

                CustomMinimumSize =
                    new Vector2(
                        0,
                        40
                    )
            };


        decisionEdit.Text =
            effectData.DecisionId ?? "";


        decisionEdit.TextChanged +=
            text =>
            {
                effectData.DecisionId =
                    text.Trim();


                GD.Print(
                    "EffectEditor: decisión persistente establecida: ",
                    effectData.DecisionId
                );
            };


        fieldsContainer.AddChild(
            decisionEdit
        );
    }


    private void BuildEmpireAttributeFields()
    {
        AddSectionLabel(
            "ATRIBUTO DEL IMPERIO"
        );


        OptionButton empireAttributeOption =
            CreateStyledOptionButton();


        empireAttributeOption.AddItem(
            "Seleccionar atributo del imperio..."
        );


        empireAttributeOption.SetItemMetadata(
            0,
            ""
        );


        AddEmpireAttribute(
            empireAttributeOption,
            "stability",
            "Estabilidad"
        );


        AddEmpireAttribute(
            empireAttributeOption,
            "prosperity",
            "Prosperidad"
        );


        AddEmpireAttribute(
            empireAttributeOption,
            "church_influence",
            "Influencia de la Iglesia"
        );


        AddEmpireAttribute(
            empireAttributeOption,
            "social_tension",
            "Tensión Social"
        );


        AddEmpireAttribute(
            empireAttributeOption,
            "northern_threat",
            "Amenaza del Norte"
        );


        AddEmpireAttribute(
            empireAttributeOption,
            "legitimacy",
            "Legitimidad"
        );


        empireAttributeOption.ItemSelected +=
            index =>
            {
                Variant metadata =
                    empireAttributeOption.GetItemMetadata(
                        (int)index
                    );


                effectData.AttributeId =
                    metadata.AsString();


                GD.Print(
                    "EffectEditor: atributo del imperio seleccionado: ",
                    effectData.AttributeId
                );
            };


        SelectEmpireAttribute(
            empireAttributeOption
        );


        fieldsContainer.AddChild(
            empireAttributeOption
        );


        AddValueField();
    }


    private void AddEmpireAttribute(
        OptionButton optionButton,
        string id,
        string displayName)
    {
        optionButton.AddItem(
            displayName
        );


        int index =
            optionButton.ItemCount - 1;


        optionButton.SetItemMetadata(
            index,
            id
        );
    }


    private void SelectEmpireAttribute(
        OptionButton optionButton)
    {
        if (
            string.IsNullOrWhiteSpace(
                effectData.AttributeId))
        {
            optionButton.Select(
                0
            );


            return;
        }


        for (
            int i = 1;
            i < optionButton.ItemCount;
            i++)
        {
            Variant metadata =
                optionButton.GetItemMetadata(
                    i
                );


            if (
                metadata.AsString() ==
                effectData.AttributeId)
            {
                optionButton.Select(
                    i
                );


                return;
            }
        }


        optionButton.Select(
            0
        );
    }


    private void BuildRelationshipFields()
    {
        AddSectionLabel(
            "PERSONAJE"
        );


        if (characterDatabase == null)
        {
            AddPlaceholderField(
                "No hay un proyecto cargado."
            );


            AddValueField();

            return;
        }


        OptionButton characterOption =
            CreateCharacterOption();


        characterOption.ItemSelected +=
            index =>
            {
                Variant metadata =
                    characterOption.GetItemMetadata(
                        (int)index
                    );


                effectData.RelationshipCharacterId =
                    metadata.AsString();


                GD.Print(
                    "EffectEditor: personaje de relación seleccionado: ",
                    effectData.RelationshipCharacterId
                );
            };


        SelectCharacter(
            characterOption,
            effectData.RelationshipCharacterId
        );


        fieldsContainer.AddChild(
            characterOption
        );


        AddSectionLabel(
            "CAMBIO"
        );


        SpinBox valueSpinBox =
            CreateStyledSpinBox();


        valueSpinBox.MinValue =
            -10;


        valueSpinBox.MaxValue =
            10;


        valueSpinBox.Step =
            1;


        valueSpinBox.Value =
            Mathf.Clamp(
                effectData.Value,
                -10,
                10
            );


        valueSpinBox.ValueChanged +=
            value =>
            {
                effectData.Value =
                    Mathf.Clamp(
                        Mathf.RoundToInt(
                            (float)value
                        ),
                        -10,
                        10
                    );
            };


        fieldsContainer.AddChild(
            valueSpinBox
        );
    }


    private void BuildRelationshipTitleFields()
    {
        AddSectionLabel(
            "PERSONAJE"
        );


        if (characterDatabase == null ||
            relationshipDatabase == null)
        {
            AddPlaceholderField(
                "No hay un proyecto cargado."
            );


            AddSectionLabel(
                "OPERACIÓN"
            );


            AddPlaceholderField(
                "Añadir o quitar título."
            );


            AddSectionLabel(
                "TÍTULO"
            );


            AddPlaceholderField(
                "Aquí aparecerán los títulos disponibles."
            );


            return;
        }


        OptionButton characterOption =
            CreateCharacterOption();


        OptionButton titleOption =
            CreateStyledOptionButton();


        titleOption.AddItem(
            "Seleccionar título..."
        );


        titleOption.SetItemMetadata(
            0,
            ""
        );


        PopulateRelationshipTitles(
            titleOption,
            effectData.RelationshipCharacterId
        );


        characterOption.ItemSelected +=
            index =>
            {
                Variant metadata =
                    characterOption.GetItemMetadata(
                        (int)index
                    );


                effectData.RelationshipCharacterId =
                    metadata.AsString();


                GD.Print(
                    "EffectEditor: personaje de título seleccionado: ",
                    effectData.RelationshipCharacterId
                );


                PopulateRelationshipTitles(
                    titleOption,
                    effectData.RelationshipCharacterId
                );
            };


        SelectCharacter(
            characterOption,
            effectData.RelationshipCharacterId
        );


        fieldsContainer.AddChild(
            characterOption
        );


        AddSectionLabel(
            "OPERACIÓN"
        );


        OptionButton operationOption =
            CreateStyledOptionButton();


        operationOption.AddItem(
            "Añadir"
        );


        operationOption.AddItem(
            "Quitar"
        );


        operationOption.SetItemMetadata(
            0,
            true
        );


        operationOption.SetItemMetadata(
            1,
            false
        );


        operationOption.Select(
            effectData.AddTitle
                ? 0
                : 1
        );


        operationOption.ItemSelected +=
            index =>
            {
                Variant metadata =
                    operationOption.GetItemMetadata(
                        (int)index
                    );


                effectData.AddTitle =
                    metadata.AsBool();
            };


        fieldsContainer.AddChild(
            operationOption
        );


        AddSectionLabel(
            "TÍTULO"
        );


        titleOption.ItemSelected +=
            index =>
            {
                Variant metadata =
                    titleOption.GetItemMetadata(
                        (int)index
                    );


                effectData.RelationshipTitle =
                    metadata.AsString();


                GD.Print(
                    "EffectEditor: título de relación seleccionado: ",
                    effectData.RelationshipTitle
                );
            };


        SelectRelationshipTitle(
            titleOption
        );


        fieldsContainer.AddChild(
            titleOption
        );
    }


    private OptionButton CreateCharacterOption()
    {
        OptionButton characterOption =
            CreateStyledOptionButton();


        characterOption.AddItem(
            "Seleccionar personaje..."
        );


        characterOption.SetItemMetadata(
            0,
            ""
        );


        List<EditorCharacterData> characters =
            characterDatabase.LoadAll();


        foreach (
            EditorCharacterData character
            in characters)
        {
            if (character == null)
            {
                continue;
            }


            if (string.IsNullOrWhiteSpace(
                character.Id))
            {
                continue;
            }


            characterOption.AddItem(
                character.Name
            );


            int index =
                characterOption.ItemCount - 1;


            characterOption.SetItemMetadata(
                index,
                character.Id
            );
        }


        return characterOption;
    }


    private void SelectCharacter(
        OptionButton characterOption,
        string characterId)
    {
        if (string.IsNullOrWhiteSpace(
            characterId))
        {
            characterOption.Select(
                0
            );


            return;
        }


        for (
            int i = 1;
            i < characterOption.ItemCount;
            i++)
        {
            Variant metadata =
                characterOption.GetItemMetadata(
                    i
                );


            if (
                metadata.AsString() ==
                characterId)
            {
                characterOption.Select(
                    i
                );


                return;
            }
        }


        characterOption.Select(
            0
        );
    }


    private void PopulateRelationshipTitles(
        OptionButton titleOption,
        string characterId)
    {
        titleOption.Clear();


        titleOption.AddItem(
            "Seleccionar título..."
        );


        titleOption.SetItemMetadata(
            0,
            ""
        );


        if (string.IsNullOrWhiteSpace(
            characterId))
        {
            titleOption.Select(
                0
            );


            return;
        }


        List<string> titles =
            relationshipDatabase.GetTitles(
                characterId
            );


        foreach (
            string title
            in titles)
        {
            if (string.IsNullOrWhiteSpace(
                title))
            {
                continue;
            }


            titleOption.AddItem(
                title
            );


            int index =
                titleOption.ItemCount - 1;


            titleOption.SetItemMetadata(
                index,
                title
            );
        }


        SelectRelationshipTitle(
            titleOption
        );
    }


    private void SelectRelationshipTitle(
        OptionButton titleOption)
    {
        if (string.IsNullOrWhiteSpace(
            effectData.RelationshipTitle))
        {
            titleOption.Select(
                0
            );


            return;
        }


        for (
            int i = 1;
            i < titleOption.ItemCount;
            i++)
        {
            Variant metadata =
                titleOption.GetItemMetadata(
                    i
                );


            if (
                metadata.AsString() ==
                effectData.RelationshipTitle)
            {
                titleOption.Select(
                    i
                );


                return;
            }
        }


        titleOption.Select(
            0
        );
    }


    private void AddValueField()
    {
        AddSectionLabel(
            "VALOR"
        );


        SpinBox valueSpinBox =
            CreateStyledSpinBox();


        valueSpinBox.MinValue =
            -1000000;


        valueSpinBox.MaxValue =
            1000000;


        valueSpinBox.Step =
            1;


        valueSpinBox.Value =
            effectData.Value;


        valueSpinBox.ValueChanged +=
            value =>
            {
                effectData.Value =
                    Mathf.RoundToInt(
                        (float)value
                    );
            };


        fieldsContainer.AddChild(
            valueSpinBox
        );
    }


    private void AddSectionLabel(
        string text)
    {
        Label label =
            new Label
            {
                Text =
                    text
            };


        label.AddThemeColorOverride(
            "font_color",
            new Color("6F7785")
        );


        label.AddThemeFontSizeOverride(
            "font_size",
            11
        );


        fieldsContainer.AddChild(
            label
        );
    }


    private void AddPlaceholderField(
        string placeholder)
    {
        LineEdit lineEdit =
            new LineEdit();


        lineEdit.CustomMinimumSize =
            new Vector2(
                0,
                36
            );


        lineEdit.PlaceholderText =
            placeholder;


        lineEdit.Editable =
            false;


        StyleBoxFlat normal =
            new StyleBoxFlat();


        normal.BgColor =
            new Color("1D2128");


        normal.BorderColor =
            new Color("313743");


        normal.SetBorderWidthAll(
            1
        );


        normal.SetCornerRadiusAll(
            3
        );


        lineEdit.AddThemeStyleboxOverride(
            "normal",
            normal
        );


        lineEdit.AddThemeColorOverride(
            "font_color",
            new Color("6F7785")
        );


        fieldsContainer.AddChild(
            lineEdit
        );
    }


    private OptionButton CreateStyledOptionButton()
    {
        OptionButton option =
            new OptionButton();


        option.CustomMinimumSize =
            new Vector2(
                0,
                36
            );


        return option;
    }


    private SpinBox CreateStyledSpinBox()
    {
        SpinBox spinBox =
            new SpinBox();


        spinBox.CustomMinimumSize =
            new Vector2(
                0,
                36
            );


        return spinBox;
    }


    private static Button CreateActionButton(
        string text)
    {
        Button button =
            new Button
            {
                Text =
                    text,

                CustomMinimumSize =
                    new Vector2(
                        110,
                        35
                    )
            };


        return button;
    }


    private void ClearFields()
    {
        foreach (
            Node child
            in fieldsContainer.GetChildren())
        {
            child.QueueFree();
        }
    }


    private void OnRemovePressed()
    {
        EmitSignal(
            SignalName.Cancelled
        );
    }
}