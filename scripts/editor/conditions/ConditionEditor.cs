using Godot;
using System.Collections.Generic;

public partial class ConditionEditor : PanelContainer
{
    private OptionButton typeOption;

    private VBoxContainer fieldsContainer;

    private ConditionDefinitionDatabase conditionDatabase;

    private AttributeDatabase attributeDatabase;

    private DecisionDatabase decisionDatabase;

    private CharacterDatabase characterDatabase;

    private RelationshipDatabase relationshipDatabase;

    private EditorConditionData conditionData;

    private int chapter = 1;

    private EventRepository eventRepository;


    public override void _Ready()
    {
        typeOption =
            GetNode<OptionButton>(
                "MarginContainer/VBoxContainer/Header/TypeOption"
            );

        fieldsContainer =
            GetNode<VBoxContainer>(
                "MarginContainer/VBoxContainer/FieldsContainer"
            );


        conditionDatabase =
            new ConditionDefinitionDatabase();


        attributeDatabase =
            new AttributeDatabase();


        conditionData =
            new EditorConditionData();


        PopulateConditionTypes();


        typeOption.ItemSelected +=
            OnConditionTypeSelected;


        SelectConditionType(
            conditionData.TypeId
        );


        GD.Print(
            "ConditionEditor iniciado."
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
            GD.PrintErr(
                "ConditionEditor: la ruta del proyecto está vacía."
            );

            return;
        }


        attributeDatabase =
            new AttributeDatabase(
                projectPath
            );


        characterDatabase =
            new CharacterDatabase(
                projectPath
            );


        relationshipDatabase =
            new RelationshipDatabase(
                projectPath
            );


        if (
            conditionData != null &&
            (
                conditionData.TypeId ==
                "relationship" ||
                conditionData.TypeId ==
                "relationshiptitle"
            ))
        {
            RefreshCurrentFields();
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
            "ConditionEditor: capítulo establecido: ",
            chapter
        );


        if (
            fieldsContainer == null ||
            conditionData == null)
        {
            return;
        }


        RefreshCurrentFields();
    }


    public void LoadCondition(
        EditorConditionData data)
    {
        if (data == null)
        {
            return;
        }


        conditionData =
            data;


        SelectConditionType(
            conditionData.TypeId
        );
    }


    public EditorConditionData GetConditionData()
    {
        return conditionData;
    }


    private void PopulateConditionTypes()
    {
        typeOption.Clear();


        foreach (
            ConditionTypeDefinition definition
            in conditionDatabase.GetDefinitions())
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


    private void SelectConditionType(
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


                UpdateConditionType();


                return;
            }
        }


        if (typeOption.ItemCount > 0)
        {
            typeOption.Select(
                0
            );


            UpdateConditionType();
        }
    }


    private void OnConditionTypeSelected(
        long index)
    {
        UpdateConditionType();
    }


    private void UpdateConditionType()
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


        conditionData.TypeId =
            typeId;


        switch (typeId)
        {
            case "characterattribute":
                BuildCharacterAttributeFields();
                break;


            case "prestige":
                BuildPrestigeFields();
                break;


            case "year":
                BuildYearFields();
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
        GD.Print(
            "ConditionEditor: construyendo atributos para capítulo: ",
            chapter
        );


        AddSectionLabel(
            "Atributo"
        );


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


                conditionData.AttributeId =
                    metadata.AsString();


                GD.Print(
                    "ConditionEditor: atributo seleccionado: ",
                    conditionData.AttributeId
                );
            };


        SelectAttribute(
            attributeOption
        );


        fieldsContainer.AddChild(
            attributeOption
        );


        AddRangeFields(
            -10,
            10
        );
    }


    private void SelectAttribute(
        OptionButton attributeOption)
    {
        if (
            string.IsNullOrWhiteSpace(
                conditionData.AttributeId))
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
                conditionData.AttributeId)
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
        AddRangeFields(
            0,
            30
        );
    }


    private void BuildYearFields()
    {
        AddRangeFields(
            1,
            100
        );
    }


    private void BuildDecisionFields()
    {
        AddSectionLabel(
            "Decisión"
        );


        Button decisionButton =
            new Button
            {
                Text =
                    GetCurrentDecisionDisplayName(),

                Alignment =
                    HorizontalAlignment.Left,

                CustomMinimumSize =
                    new Vector2(
                        0,
                        40
                    )
            };


        decisionButton.AddThemeColorOverride(
            "font_color",
            new Color("A8AFBC")
        );


        decisionButton.AddThemeColorOverride(
            "font_hover_color",
            new Color("E7EAF0")
        );


        decisionButton.AddThemeColorOverride(
            "font_pressed_color",
            new Color("E7EAF0")
        );


        decisionButton.AddThemeColorOverride(
            "font_focus_color",
            new Color("E7EAF0")
        );


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
                conditionData.DecisionId))
        {
            return "Seleccionar decisión...";
        }


        if (decisionDatabase == null)
        {
            return "Decisión no disponible";
        }


        return decisionDatabase.GetDecisionDisplayName(
            conditionData.DecisionId
        );
    }


    private void OpenDecisionSelector()
    {
        if (decisionDatabase == null)
        {
            GD.PrintErr(
                "ConditionEditor: DecisionDatabase no está disponible."
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
            decisionList,
            decisions
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
                    decisionList,
                    decisions
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
        ItemList decisionList,
        List<DecisionDatabase.DecisionReference> decisions)
    {
        if (
            string.IsNullOrWhiteSpace(
                conditionData.DecisionId))
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
                conditionData.DecisionId)
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
        int selected =
            decisionList.GetSelectedItems().Length > 0
                ? decisionList.GetSelectedItems()[0]
                : -1;


        if (selected < 0)
        {
            return;
        }


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


        conditionData.DecisionId =
            decisionId;


        GD.Print(
            "ConditionEditor: decisión seleccionada: ",
            conditionData.DecisionId
        );


        window.QueueFree();


        RefreshCurrentFields();
    }


    private void RefreshCurrentFields()
    {
        string currentType =
            conditionData.TypeId;


        UpdateConditionType();


        if (currentType == "decision")
        {
            GD.Print(
                "ConditionEditor: selector de decisión actualizado."
            );
        }
    }


    private void BuildEmpireAttributeFields()
    {
        AddSectionLabel(
            "Atributo del imperio"
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


                conditionData.AttributeId =
                    metadata.AsString();


                GD.Print(
                    "ConditionEditor: atributo del imperio seleccionado: ",
                    conditionData.AttributeId
                );
            };


        SelectEmpireAttribute(
            empireAttributeOption
        );


        fieldsContainer.AddChild(
            empireAttributeOption
        );


        AddRangeFields(
            0,
            10
        );
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
                conditionData.AttributeId))
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
                conditionData.AttributeId)
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


        OptionButton characterOption =
            CreateCharacterOption();


        fieldsContainer.AddChild(
            characterOption
        );


        AddSectionLabel(
            "Comparación"
        );


        OptionButton comparisonOption =
            CreateStyledOptionButton();


        comparisonOption.AddItem(
            "Igual"
        );


        comparisonOption.SetItemMetadata(
            0,
            "equal"
        );


        comparisonOption.AddItem(
            "Mayor que"
        );


        comparisonOption.SetItemMetadata(
            1,
            "greater"
        );


        comparisonOption.AddItem(
            "Mayor o igual que"
        );


        comparisonOption.SetItemMetadata(
            2,
            "greater_equal"
        );


        comparisonOption.AddItem(
            "Menor que"
        );


        comparisonOption.SetItemMetadata(
            3,
            "less"
        );


        comparisonOption.AddItem(
            "Menor o igual que"
        );


        comparisonOption.SetItemMetadata(
            4,
            "less_equal"
        );


        SelectRelationshipComparison(
            comparisonOption
        );


        comparisonOption.ItemSelected +=
            index =>
            {
                UpdateRelationshipComparison(
                    comparisonOption
                );
            };


        fieldsContainer.AddChild(
            comparisonOption
        );


        AddSectionLabel(
            "Valor"
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
            GetRelationshipConditionValue();


        valueSpinBox.ValueChanged +=
            value =>
            {
                string comparison =
                    comparisonOption.GetItemMetadata(
                        comparisonOption.Selected
                    ).AsString();


                SetRelationshipConditionValue(
                    Mathf.RoundToInt(
                        (float)value
                    ),
                    comparison
                );
            };


        fieldsContainer.AddChild(
            valueSpinBox
        );


        SelectRelationshipCharacter(
            characterOption
        );
    }


    private void BuildRelationshipTitleFields()
    {
        AddSectionLabel(
            "Personaje"
        );


        OptionButton characterOption =
            CreateCharacterOption();


        fieldsContainer.AddChild(
            characterOption
        );


        AddSectionLabel(
            "Título"
        );


        OptionButton titleOption =
            CreateStyledOptionButton();


        titleOption.AddItem(
            "Seleccionar título..."
        );


        titleOption.SetItemMetadata(
            0,
            ""
        );


        characterOption.ItemSelected +=
            index =>
            {
                PopulateRelationshipTitles(
                    characterOption,
                    titleOption
                );
            };


        PopulateRelationshipTitles(
            characterOption,
            titleOption
        );


        titleOption.ItemSelected +=
            index =>
            {
                Variant metadata =
                    titleOption.GetItemMetadata(
                        (int)index
                    );


                conditionData.RelationshipTitle =
                    metadata.AsString();


                GD.Print(
                    "ConditionEditor: título de relación seleccionado: ",
                    conditionData.RelationshipTitle
                );
            };


        fieldsContainer.AddChild(
            titleOption
        );


        SelectRelationshipCharacter(
            characterOption
        );


        SelectRelationshipTitle(
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


        if (characterDatabase == null)
        {
            return characterOption;
        }


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


            string displayName =
                string.IsNullOrWhiteSpace(
                    character.Name)
                    ? character.Id
                    : character.Name;


            characterOption.AddItem(
                displayName
            );


            int index =
                characterOption.ItemCount - 1;


            characterOption.SetItemMetadata(
                index,
                character.Id
            );
        }


        characterOption.ItemSelected +=
            index =>
            {
                Variant metadata =
                    characterOption.GetItemMetadata(
                        (int)index
                    );


                string characterId =
                    metadata.AsString();


                conditionData.RelationshipCharacterId =
                    characterId;


                GD.Print(
                    "ConditionEditor: personaje de relación seleccionado: ",
                    characterId
                );
            };


        return characterOption;
    }


    private void PopulateRelationshipTitles(
        OptionButton characterOption,
        OptionButton titleOption)
    {
        titleOption.Clear();


        titleOption.AddItem(
            "Seleccionar título..."
        );


        titleOption.SetItemMetadata(
            0,
            ""
        );


        if (relationshipDatabase == null)
        {
            return;
        }


        if (characterOption.Selected < 0)
        {
            return;
        }


        Variant metadata =
            characterOption.GetItemMetadata(
                characterOption.Selected
            );


        string characterId =
            metadata.AsString();


        if (string.IsNullOrWhiteSpace(
            characterId))
        {
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


    private void SelectRelationshipCharacter(
        OptionButton characterOption)
    {
        if (
            string.IsNullOrWhiteSpace(
                conditionData.RelationshipCharacterId))
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
                conditionData.RelationshipCharacterId)
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


    private void SelectRelationshipTitle(
        OptionButton titleOption)
    {
        if (
            string.IsNullOrWhiteSpace(
                conditionData.RelationshipTitle))
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
                conditionData.RelationshipTitle)
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


    private string GetRelationshipComparison()
    {
        int minimum =
            conditionData.MinimumValue;


        int maximum =
            conditionData.MaximumValue;


        if (
            minimum == maximum)
        {
            return "equal";
        }


        if (
            minimum > -10 &&
            maximum == 10)
        {
            return "greater_equal";
        }


        if (
            minimum == 0 &&
            maximum == 10)
        {
            return "greater_equal";
        }


        if (
            minimum == -10 &&
            maximum < 10)
        {
            return "less_equal";
        }


        if (
            minimum > -10 &&
            maximum == 10)
        {
            return "greater_equal";
        }


        return "equal";
    }


    private void SelectRelationshipComparison(
        OptionButton comparisonOption)
    {
        string comparison =
            GetRelationshipComparison();


        for (
            int i = 0;
            i < comparisonOption.ItemCount;
            i++)
        {
            Variant metadata =
                comparisonOption.GetItemMetadata(
                    i
                );


            if (
                metadata.AsString() ==
                comparison)
            {
                comparisonOption.Select(
                    i
                );


                return;
            }
        }


        comparisonOption.Select(
            0
        );
    }


    private void UpdateRelationshipComparison(
        OptionButton comparisonOption)
    {
        int value =
            GetRelationshipConditionValue();


        string comparison =
            comparisonOption.GetItemMetadata(
                comparisonOption.Selected
            ).AsString();


        SetRelationshipConditionValue(
            value,
            comparison
        );
    }


    private int GetRelationshipConditionValue()
    {
        if (
            conditionData.MinimumValue ==
            conditionData.MaximumValue)
        {
            return conditionData.MinimumValue;
        }


        if (
            conditionData.MaximumValue == 10)
        {
            return conditionData.MinimumValue;
        }


        if (
            conditionData.MinimumValue == -10)
        {
            return conditionData.MaximumValue;
        }


        return conditionData.MinimumValue;
    }


    private void SetRelationshipConditionValue(
        int value,
        string comparison)
    {
        value =
            Mathf.Clamp(
                value,
                -10,
                10
            );


        switch (comparison)
        {
            case "equal":
                conditionData.MinimumValue =
                    value;

                conditionData.MaximumValue =
                    value;
                break;


            case "greater":
                conditionData.MinimumValue =
                    Mathf.Clamp(
                        value + 1,
                        -10,
                        10
                    );

                conditionData.MaximumValue =
                    10;
                break;


            case "greater_equal":
                conditionData.MinimumValue =
                    value;

                conditionData.MaximumValue =
                    10;
                break;


            case "less":
                conditionData.MinimumValue =
                    -10;

                conditionData.MaximumValue =
                    Mathf.Clamp(
                        value - 1,
                        -10,
                        10
                    );
                break;


            case "less_equal":
                conditionData.MinimumValue =
                    -10;

                conditionData.MaximumValue =
                    value;
                break;
        }


        GD.Print(
            "ConditionEditor: condición de relación actualizada: ",
            comparison,
            " ",
            value
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


    private void AddRangeFields(
        int minimum,
        int maximum)
    {
        AddSectionLabel(
            "Valor mínimo"
        );


        SpinBox minimumSpinBox =
            CreateStyledSpinBox();


        minimumSpinBox.MinValue =
            minimum;


        minimumSpinBox.MaxValue =
            maximum;


        minimumSpinBox.Step =
            1;


        minimumSpinBox.Value =
            Mathf.Clamp(
                conditionData.MinimumValue,
                minimum,
                maximum
            );


        minimumSpinBox.ValueChanged +=
            value =>
            {
                conditionData.MinimumValue =
                    Mathf.RoundToInt(
                        (float)value
                    );
            };


        fieldsContainer.AddChild(
            minimumSpinBox
        );


        AddSectionLabel(
            "Valor máximo"
        );


        SpinBox maximumSpinBox =
            CreateStyledSpinBox();


        maximumSpinBox.MinValue =
            minimum;


        maximumSpinBox.MaxValue =
            maximum;


        maximumSpinBox.Step =
            1;


        maximumSpinBox.Value =
            Mathf.Clamp(
                conditionData.MaximumValue,
                minimum,
                maximum
            );


        maximumSpinBox.ValueChanged +=
            value =>
            {
                conditionData.MaximumValue =
                    Mathf.RoundToInt(
                        (float)value
                    );
            };


        fieldsContainer.AddChild(
            maximumSpinBox
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


    private void ClearFields()
    {
        foreach (
            Node child
            in fieldsContainer.GetChildren())
        {
            fieldsContainer.RemoveChild(
                child
            );


            child.QueueFree();
        }
    }
}