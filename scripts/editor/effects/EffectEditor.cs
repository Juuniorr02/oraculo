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

    private AttributeDatabase attributeDatabase;

    private DecisionDatabase decisionDatabase;

    private CharacterDatabase characterDatabase;

    private RelationshipDatabase relationshipDatabase;

    private EditorEffectData effectData;

    private int chapter = 1;

    private EventRepository eventRepository;


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


        attributeDatabase =
            new AttributeDatabase();


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
    }


    public void SetProjectPath(
        string projectPath)
    {
        if (string.IsNullOrWhiteSpace(
            projectPath))
        {
            characterDatabase = null;
            relationshipDatabase = null;

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


        GD.Print(
            "EffectEditor: ruta del proyecto establecida: ",
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
            "Atributo"
        );


        OptionButton attributeOption =
            new OptionButton();


        attributeOption.CustomMinimumSize =
            new Vector2(
                0,
                35
            );


        attributeOption.AddItem(
            "Seleccionar atributo..."
        );


        attributeOption.SetItemMetadata(
            0,
            ""
        );


        foreach (
            AttributeDefinitionData attribute
            in attributeDatabase.GetAttributesForChapter(
                chapter))
        {
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
            "Decisión"
        );


        Button decisionButton =
            new Button();


        decisionButton.CustomMinimumSize =
            new Vector2(
                0,
                40
            );


        decisionButton.Alignment =
            HorizontalAlignment.Left;


        decisionButton.Text =
            GetCurrentDecisionDisplayName();


        decisionButton.Pressed +=
            OpenDecisionSelector;


        fieldsContainer.AddChild(
            decisionButton
        );
    }


    private string GetCurrentDecisionDisplayName()
    {
        if (
            string.IsNullOrWhiteSpace(
                effectData.DecisionId))
        {
            return "Seleccionar decisión...";
        }


        if (decisionDatabase == null)
        {
            return "Decisión no disponible";
        }


        return decisionDatabase.GetDecisionDisplayName(
            effectData.DecisionId
        );
    }


    private void OpenDecisionSelector()
    {
        if (decisionDatabase == null)
        {
            GD.PrintErr(
                "EffectEditor: DecisionDatabase no está disponible."
            );

            return;
        }


        Window window =
            new Window
            {
                Title =
                    "Seleccionar decisión",

                Size =
                    new Vector2I(
                        750,
                        600
                    ),

                MinSize =
                    new Vector2I(
                        550,
                        400
                    ),

                Exclusive =
                    true,

                Unresizable =
                    false
            };


        VBoxContainer container =
            new VBoxContainer();


        container.SetAnchorsAndOffsetsPreset(
            LayoutPreset.FullRect
        );


        container.AddThemeConstantOverride(
            "separation",
            8
        );


        MarginContainer margin =
            new MarginContainer();


        margin.SetAnchorsAndOffsetsPreset(
            LayoutPreset.FullRect
        );


        margin.AddThemeConstantOverride(
            "margin_left",
            12
        );

        margin.AddThemeConstantOverride(
            "margin_top",
            12
        );

        margin.AddThemeConstantOverride(
            "margin_right",
            12
        );

        margin.AddThemeConstantOverride(
            "margin_bottom",
            12
        );


        VBoxContainer content =
            new VBoxContainer();


        margin.AddChild(
            content
        );


        Label searchLabel =
            new Label
            {
                Text =
                    "Buscar decisión"
            };


        content.AddChild(
            searchLabel
        );


        LineEdit searchEdit =
            new LineEdit
            {
                PlaceholderText =
                    "Buscar por evento, ID o texto de decisión..."
            };


        searchEdit.CustomMinimumSize =
            new Vector2(
                0,
                35
            );


        content.AddChild(
            searchEdit
        );


        ItemList decisionList =
            new ItemList();


        decisionList.CustomMinimumSize =
            new Vector2(
                0,
                400
            );


        decisionList.SizeFlagsVertical =
            Control.SizeFlags.ExpandFill;


        decisionList.AllowReselect =
            true;


        content.AddChild(
            decisionList
        );


        HBoxContainer buttons =
            new HBoxContainer
            {
                Alignment =
                    BoxContainer.AlignmentMode.End
            };


        Button cancelButton =
            new Button
            {
                Text =
                    "Cancelar"
            };


        Button selectButton =
            new Button
            {
                Text =
                    "Seleccionar"
            };


        cancelButton.CustomMinimumSize =
            new Vector2(
                110,
                35
            );


        selectButton.CustomMinimumSize =
            new Vector2(
                110,
                35
            );


        buttons.AddChild(
            cancelButton
        );


        buttons.AddChild(
            selectButton
        );


        content.AddChild(
            buttons
        );


        window.AddChild(
            margin
        );


        AddChild(
            window
        );


        List<DecisionDatabase.DecisionReference> decisions =
            decisionDatabase.GetAllDecisions();


        PopulateDecisionList(
            decisionList,
            decisions,
            ""
        );


        SelectCurrentDecisionInList(
            decisionList
        );


        searchEdit.TextChanged +=
            text =>
            {
                PopulateDecisionList(
                    decisionList,
                    decisions,
                    text
                );


                SelectCurrentDecisionInList(
                    decisionList
                );
            };


        decisionList.ItemActivated +=
            index =>
            {
                SelectDecisionFromList(
                    decisionList,
                    window
                );
            };


        cancelButton.Pressed +=
            () =>
            {
                window.QueueFree();
            };


        selectButton.Pressed +=
            () =>
            {
                SelectDecisionFromList(
                    decisionList,
                    window
                );
            };


        window.CloseRequested +=
            () =>
            {
                window.QueueFree();
            };


        window.PopupCentered();
    }


    private void PopulateDecisionList(
        ItemList decisionList,
        List<DecisionDatabase.DecisionReference> decisions,
        string searchText)
    {
        decisionList.Clear();


        string search =
            searchText?.Trim() ?? "";


        foreach (
            DecisionDatabase.DecisionReference decision
            in decisions)
        {
            if (
                !MatchesSearch(
                    decision,
                    search))
            {
                continue;
            }


            string displayName =
                decisionDatabase.GetDecisionDisplayName(
                    decision.Id
                );


            int index =
                decisionList.AddItem(
                    displayName
                );


            decisionList.SetItemMetadata(
                index,
                decision.Id
            );
        }
    }


    private static bool MatchesSearch(
        DecisionDatabase.DecisionReference decision,
        string search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return true;
        }


        return
            ContainsIgnoreCase(
                decision.EventTitle,
                search
            )
            ||
            ContainsIgnoreCase(
                decision.EventId,
                search
            )
            ||
            ContainsIgnoreCase(
                decision.Text,
                search
            );
    }


    private static bool ContainsIgnoreCase(
        string value,
        string search)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }


        return value.IndexOf(
            search,
            System.StringComparison.OrdinalIgnoreCase
        ) >= 0;
    }


    private void SelectCurrentDecisionInList(
        ItemList decisionList)
    {
        if (
            string.IsNullOrWhiteSpace(
                effectData.DecisionId))
        {
            return;
        }


        for (
            int i = 0;
            i < decisionList.ItemCount;
            i++)
        {
            Variant metadata =
                decisionList.GetItemMetadata(
                    i
                );


            if (
                metadata.AsString() ==
                effectData.DecisionId)
            {
                decisionList.Select(
                    i
                );


                decisionList.EnsureCurrentIsVisible();


                return;
            }
        }
    }


    private void SelectDecisionFromList(
        ItemList decisionList,
        Window window)
    {
        int[] selectedItems =
            decisionList.GetSelectedItems();


        if (
            selectedItems.Length == 0)
        {
            return;
        }


        int selected =
            selectedItems[0];


        Variant metadata =
            decisionList.GetItemMetadata(
                selected
            );


        string decisionId =
            metadata.AsString();


        if (string.IsNullOrWhiteSpace(decisionId))
        {
            return;
        }


        effectData.DecisionId =
            decisionId;


        GD.Print(
            "EffectEditor: decisión seleccionada: ",
            effectData.DecisionId
        );


        window.QueueFree();


        RefreshCurrentFields();
    }


    private void RefreshCurrentFields()
    {
        string currentType =
            effectData.TypeId;


        UpdateEffectType();


        if (currentType == "decision")
        {
            GD.Print(
                "EffectEditor: selector de decisión actualizado."
            );
        }
    }


    private void BuildEmpireAttributeFields()
    {
        AddSectionLabel(
            "Atributo del imperio"
        );


        OptionButton empireAttributeOption =
            new OptionButton();


        empireAttributeOption.CustomMinimumSize =
            new Vector2(
                0,
                35
            );


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
            "Personaje"
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
            "Cambio"
        );


        SpinBox valueSpinBox =
            new SpinBox();


        valueSpinBox.CustomMinimumSize =
            new Vector2(
                0,
                35
            );


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
        "Personaje"
    );


    if (characterDatabase == null ||
        relationshipDatabase == null)
    {
        AddPlaceholderField(
            "No hay un proyecto cargado."
        );

        AddSectionLabel(
            "Operación"
        );

        AddPlaceholderField(
            "Añadir o quitar título."
        );

        AddSectionLabel(
            "Título"
        );

        AddPlaceholderField(
            "Aquí aparecerán los títulos disponibles."
        );

        return;
    }


    OptionButton characterOption =
        CreateCharacterOption();


    OptionButton titleOption =
        new OptionButton();


    titleOption.CustomMinimumSize =
        new Vector2(
            0,
            35
        );


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
        "Operación"
    );


    OptionButton operationOption =
        new OptionButton();


    operationOption.CustomMinimumSize =
        new Vector2(
            0,
            35
        );


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
        "Título"
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
            new OptionButton();


        characterOption.CustomMinimumSize =
            new Vector2(
                0,
                35
            );


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
            "Valor"
        );


        SpinBox valueSpinBox =
            new SpinBox();


        valueSpinBox.CustomMinimumSize =
            new Vector2(
                0,
                35
            );


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
            new Label();


        label.Text =
            text;


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
                35
            );


        lineEdit.PlaceholderText =
            placeholder;


        lineEdit.Editable =
            false;


        fieldsContainer.AddChild(
            lineEdit
        );
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