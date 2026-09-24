using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;


public partial class EventEditor : Control
{
    [Signal]
    public delegate void EventSavedEventHandler();

    [Signal]
    public delegate void EventCancelledEventHandler();

    [Signal]
    public delegate void ApplicationCloseConfirmedEventHandler();


    private Label titleLabel;

    private LineEdit idEdit;
    private LineEdit titleEdit;

    private OptionButton chapterOption;

    private SpinBox yearSpinBox;
    private SpinBox worldYearSpinBox;

    private TabBar tabBar;

    private EventPageEditor pageEditor;
    private OptionsEditor optionsEditor;

    private Button eventConditionsButton;
    private Button closeButton;
    private Button cancelButton;
    private Button saveButton;

    private Window eventConditionsWindow;
    private ConditionListEditor eventConditionsEditor;

    private Control contentContainer;

    private Control eventInfoContainer;
    private Control buttonsContainer;

    private EditorEventData eventData;

    private EventRepository eventRepository;
    private ChapterRepository chapterRepository;
    private AttributeRepository attributeRepository;

    private string eventId = "";
    private string projectPath = "";

    private bool updatingYears = false;
    private bool creatingNewEvent = false;

    private UnsavedChangesGuard<EditorEventData>
        unsavedChangesGuard;

    private const int BaseWorldYear = 1134;
    private const int BaseYear = 1;

    private string pendingPageId = "";
    private int pendingDecisionIndex = -1;

    private bool applicationCloseRequested = false;


    // ============================================================
    // READY
    // ============================================================

    public override void _Ready()
    {
        titleLabel =
            GetNode<Label>(
                "VBoxContainer/Header/Title"
            );

        idEdit =
            GetNode<LineEdit>(
                "VBoxContainer/EventInfo/IdContainer/IdEdit"
            );

        titleEdit =
            GetNode<LineEdit>(
                "VBoxContainer/EventInfo/TitleContainer/TitleEditRow/TitleEdit"
            );

        chapterOption =
            GetNode<OptionButton>(
                "VBoxContainer/EventInfo/BasicInfo/ChapterContainer/ChapterOption"
            );

        yearSpinBox =
            GetNode<SpinBox>(
                "VBoxContainer/EventInfo/BasicInfo/YearContainer/YearSpinBox"
            );

        worldYearSpinBox =
            GetNode<SpinBox>(
                "VBoxContainer/EventInfo/BasicInfo/WorldYearContainer/WorldYearSpinBox"
            );

        tabBar =
            GetNode<TabBar>(
                "VBoxContainer/TabBar"
            );

        contentContainer =
            GetNode<Control>(
                "VBoxContainer/Content"
            );

        eventInfoContainer =
            GetNode<Control>(
                "VBoxContainer/EventInfo"
            );

        buttonsContainer =
            GetNode<Control>(
                "VBoxContainer/Buttons"
            );

        pageEditor =
            GetNode<EventPageEditor>(
                "VBoxContainer/Content/EventPageEditor"
            );

        optionsEditor =
            GetNode<OptionsEditor>(
                "VBoxContainer/Content/OptionsEditor"
            );

        eventConditionsButton =
            GetNode<Button>(
                "VBoxContainer/EventInfo/TitleContainer/TitleEditRow/EventConditionsButton"
            );

        closeButton =
            GetNode<Button>(
                "VBoxContainer/Header/CloseButton"
            );

        cancelButton =
            GetNode<Button>(
                "VBoxContainer/Buttons/CancelButton"
            );

        saveButton =
            GetNode<Button>(
                "VBoxContainer/Buttons/SaveButton"
            );


        eventConditionsButton.Pressed +=
            OnEventConditionsPressed;

        closeButton.Pressed +=
            OnClosePressed;

        cancelButton.Pressed +=
            OnCancelPressed;

        saveButton.Pressed +=
            OnSavePressed;

        titleEdit.TextChanged +=
            OnTitleChanged;

        yearSpinBox.ValueChanged +=
            OnYearChanged;

        worldYearSpinBox.ValueChanged +=
            OnWorldYearChanged;

        chapterOption.ItemSelected +=
            OnChapterSelected;

        tabBar.TabChanged +=
            OnTabChanged;


        idEdit.Editable =
            false;


        // --------------------------------------------------------
        // WORKSPACE
        // --------------------------------------------------------

        contentContainer.SizeFlagsHorizontal =
            Control.SizeFlags.ExpandFill;

        contentContainer.SizeFlagsVertical =
            Control.SizeFlags.ExpandFill;


        pageEditor.SizeFlagsHorizontal =
            Control.SizeFlags.ExpandFill;

        pageEditor.SizeFlagsVertical =
            Control.SizeFlags.ExpandFill;


        optionsEditor.SizeFlagsHorizontal =
            Control.SizeFlags.ExpandFill;

        optionsEditor.SizeFlagsVertical =
            Control.SizeFlags.ExpandFill;


        unsavedChangesGuard =
            new UnsavedChangesGuard<EditorEventData>(
                this,
                GetCurrentEventState,
                CloseEditor
            );


        InitializeYears();


        OnTabChanged(
            tabBar.CurrentTab
        );


        GD.Print(
            "EventEditor iniciado."
        );
    }


    // ============================================================
    // WORKSPACE MODE
    // ============================================================

    private void ApplyNarrativeWorkspaceMode(
    bool narrative)
{
    if (eventInfoContainer != null)
    {
        eventInfoContainer.Visible =
            !narrative;
    }


    if (buttonsContainer != null)
    {
        buttonsContainer.Visible =
            !narrative;
    }


    if (contentContainer != null)
    {
        contentContainer.SizeFlagsHorizontal =
            Control.SizeFlags.ExpandFill;

        contentContainer.SizeFlagsVertical =
            Control.SizeFlags.ExpandFill;
    }


    if (pageEditor != null)
    {
        pageEditor.SizeFlagsHorizontal =
            Control.SizeFlags.ExpandFill;

        pageEditor.SizeFlagsVertical =
            Control.SizeFlags.ExpandFill;
    }


    if (optionsEditor != null)
    {
        optionsEditor.SizeFlagsHorizontal =
            Control.SizeFlags.ExpandFill;

        optionsEditor.SizeFlagsVertical =
            Control.SizeFlags.ExpandFill;
    }
}


    // ============================================================
    // CONFIGURATION
    // ============================================================

    public void SetRepository(
        EventRepository repository)
    {
        eventRepository =
            repository;


        optionsEditor.SetRepository(
            eventRepository
        );
    }


    public void SetProjectPath(
        string path)
    {
        projectPath =
            path ?? "";


        optionsEditor.SetProjectPath(
            projectPath
        );


        if (string.IsNullOrWhiteSpace(
            projectPath))
        {
            chapterRepository =
                null;

            attributeRepository =
                null;

            chapterOption.Clear();


            return;
        }


        chapterRepository =
            new ChapterRepository(
                projectPath
            );

        attributeRepository =
            new AttributeRepository(
                projectPath
            );


        PopulateChapterOptions();
    }


    public void SetValidationTarget(
        string pageId,
        int decisionIndex)
    {
        pendingPageId =
            pageId ?? "";

        pendingDecisionIndex =
            decisionIndex;
    }


    // ============================================================
    // CREATE / LOAD
    // ============================================================

    public void CreateNewEvent()
    {
        creatingNewEvent =
            true;

        eventId =
            "";


        int defaultChapter =
            GetDefaultChapterNumber();


        eventData =
            new EditorEventData
            {
                Id = "",
                Title = "",
                Chapter = defaultChapter,
                Year = 1,
                WorldYear = BaseWorldYear,
                Conditions =
                    new List<EditorConditionData>(),
                Pages =
                    new List<EditorPageData>()
            };


        LoadEventIntoEditor();


        SaveOriginalState();
    }


    public void LoadEvent(
        string id)
    {
        if (string.IsNullOrWhiteSpace(
            id))
        {
            return;
        }


        creatingNewEvent =
            false;

        eventId =
            id;


        if (eventRepository == null)
        {
            return;
        }


        EditorEventData loadedEvent =
            eventRepository.Load(
                id
            );


        if (loadedEvent == null)
        {
            creatingNewEvent =
                true;


            int defaultChapter =
                GetDefaultChapterNumber();


            eventData =
                new EditorEventData
                {
                    Id = id,
                    Title = "",
                    Chapter = defaultChapter,
                    Year = 1,
                    WorldYear = BaseWorldYear,
                    Conditions =
                        new List<EditorConditionData>(),
                    Pages =
                        new List<EditorPageData>()
                };
        }
        else
        {
            eventData =
                loadedEvent;


            if (eventData.Conditions == null)
            {
                eventData.Conditions =
                    new List<EditorConditionData>();
            }


            if (eventData.Pages == null)
            {
                eventData.Pages =
                    new List<EditorPageData>();
            }
        }


        LoadEventIntoEditor();


        SaveOriginalState();
    }


    public void SaveCurrentEvent()
    {
        OnSavePressed();
    }


    // ============================================================
    // LOAD
    // ============================================================

    private void LoadEventIntoEditor()
    {
        if (eventData == null)
        {
            return;
        }


        if (eventData.Conditions == null)
        {
            eventData.Conditions =
                new List<EditorConditionData>();
        }


        if (eventData.Pages == null)
        {
            eventData.Pages =
                new List<EditorPageData>();
        }


        idEdit.Text =
            eventData.Id;


        titleEdit.Text =
            eventData.Title;


        titleLabel.Text =
            creatingNewEvent
                ? "Nuevo evento"
                : "Editor de evento: " +
                  eventData.Id;


        LoadChapter(
            eventData.Chapter
        );


        updatingYears =
            true;


        yearSpinBox.Value =
            eventData.Year;

        worldYearSpinBox.Value =
            eventData.WorldYear;


        updatingYears =
            false;


        pageEditor.SetPages(
            new List<EditorPageData>(
                eventData.Pages
            )
        );


        optionsEditor.SetRepository(
            eventRepository
        );

        optionsEditor.SetProjectPath(
            projectPath
        );

        optionsEditor.SetChapter(
            eventData.Chapter
        );

        optionsEditor.SetPages(
            new List<EditorPageData>(
                eventData.Pages
            )
        );


        SelectValidationTarget();
    }


    // ============================================================
    // TAB
    // ============================================================

    private void OnTabChanged(
    long tab)
{
    bool showingNarrative =
        tab == 0;


    // Durante _Ready todavía no existe eventData.
    // En ese momento solo cambiamos la visibilidad,
    // sin intentar sincronizar ni recargar páginas.
    if (eventData == null)
    {
        ApplyNarrativeWorkspaceMode(
            showingNarrative
        );


        if (pageEditor != null)
        {
            pageEditor.Visible =
                showingNarrative;
        }


        if (optionsEditor != null)
        {
            optionsEditor.Visible =
                !showingNarrative;
        }


        return;
    }


    SynchronizeNarrativeAndOptions();


    ApplyNarrativeWorkspaceMode(
        showingNarrative
    );


    if (pageEditor != null)
    {
        pageEditor.Visible =
            showingNarrative;
    }


    if (optionsEditor != null)
    {
        optionsEditor.Visible =
            !showingNarrative;
    }


    if (showingNarrative)
    {
        pageEditor.SetPages(
            new List<EditorPageData>(
                eventData.Pages ??
                new List<EditorPageData>()
            )
        );
    }
    else
    {
        optionsEditor.SetRepository(
            eventRepository
        );


        optionsEditor.SetProjectPath(
            projectPath
        );


        optionsEditor.SetChapter(
            eventData.Chapter
        );


        optionsEditor.SetPages(
            new List<EditorPageData>(
                eventData.Pages ??
                new List<EditorPageData>()
            )
        );
    }
}


    private void SynchronizeNarrativeAndOptions()
    {
        if (eventData == null)
        {
            return;
        }


        if (optionsEditor != null)
        {
            optionsEditor.GetPages();
        }


        if (pageEditor != null)
        {
            List<EditorPageData> narrativePages =
                pageEditor.GetPages();


            if (narrativePages != null)
            {
                eventData.Pages =
                    new List<EditorPageData>(
                        narrativePages
                    );
            }
        }


        if (eventData.Pages == null)
        {
            eventData.Pages =
                new List<EditorPageData>();
        }


        NormalizePageTree(
            eventData.Pages
        );
    }


    // ============================================================
    // UPDATE
    // ============================================================

    private void UpdateEventDataFromEditor()
    {
        if (eventData == null)
        {
            eventData =
                new EditorEventData();
        }


        eventData.Id =
            idEdit.Text.Trim();


        eventData.Title =
            titleEdit.Text;


        if (chapterOption.Selected >= 0)
        {
            Variant metadata =
                chapterOption.GetItemMetadata(
                    chapterOption.Selected
                );


            int selectedChapter =
                metadata.AsInt32();


            if (selectedChapter > 0)
            {
                eventData.Chapter =
                    selectedChapter;
            }
        }


        eventData.Year =
            Mathf.RoundToInt(
                (float)yearSpinBox.Value
            );


        eventData.WorldYear =
            Mathf.RoundToInt(
                (float)worldYearSpinBox.Value
            );


        SynchronizeNarrativeAndOptions();
    }


    private void NormalizePageTree(
        List<EditorPageData> collection)
    {
        if (collection == null)
        {
            return;
        }


        foreach (
            EditorPageData page
            in collection)
        {
            if (page == null)
            {
                continue;
            }


            if (page.Decisions == null)
            {
                page.Decisions =
                    new List<EditorDecisionData>();
            }


            foreach (
                EditorDecisionData decision
                in page.Decisions)
            {
                if (decision == null)
                {
                    continue;
                }


                if (decision.Conditions == null)
                {
                    decision.Conditions =
                        new List<EditorConditionData>();
                }


                if (decision.Effects == null)
                {
                    decision.Effects =
                        new List<EditorEffectData>();
                }


                if (decision.Pages == null)
                {
                    decision.Pages =
                        new List<EditorPageData>();
                }


                NormalizePageTree(
                    decision.Pages
                );
            }
        }
    }


    // ============================================================
    // CHAPTERS
    // ============================================================

    private void PopulateChapterOptions()
    {
        chapterOption.Clear();


        if (chapterRepository == null)
        {
            return;
        }


        List<ChapterDefinitionData> chapters =
            chapterRepository.LoadAll();


        chapters.Sort(
            (a, b) =>
                a.Number.CompareTo(
                    b.Number
                )
        );


        foreach (
            ChapterDefinitionData chapter
            in chapters)
        {
            if (chapter == null)
            {
                continue;
            }


            int index =
                chapterOption.ItemCount;


            chapterOption.AddItem(
                chapter.Number +
                ". " +
                chapter.Title
            );


            chapterOption.SetItemMetadata(
                index,
                chapter.Number
            );
        }
    }


    private int GetDefaultChapterNumber()
    {
        if (chapterRepository == null)
        {
            return 1;
        }


        List<ChapterDefinitionData> chapters =
            chapterRepository.LoadAll();


        chapters.Sort(
            (a, b) =>
                a.Number.CompareTo(
                    b.Number
                )
        );


        foreach (
            ChapterDefinitionData chapter
            in chapters)
        {
            if (
                chapter != null &&
                chapter.Number > 0)
            {
                return chapter.Number;
            }
        }


        return 1;
    }


    private void LoadChapter(
        int chapter)
    {
        for (
            int i = 0;
            i < chapterOption.ItemCount;
            i++)
        {
            Variant metadata =
                chapterOption.GetItemMetadata(
                    i
                );


            if (metadata.AsInt32() == chapter)
            {
                chapterOption.Select(
                    i
                );


                return;
            }
        }
    }


    private void OnChapterSelected(
        long index)
    {
        if (
            index < 0 ||
            index >= chapterOption.ItemCount)
        {
            return;
        }


        Variant metadata =
            chapterOption.GetItemMetadata(
                (int)index
            );


        int chapter =
            metadata.AsInt32();


        eventData.Chapter =
            chapter;


        optionsEditor.SetChapter(
            chapter
        );
    }


    // ============================================================
    // YEARS
    // ============================================================

    private void InitializeYears()
    {
        updatingYears =
            true;


        yearSpinBox.MinValue =
            1;

        yearSpinBox.Step =
            1;

        yearSpinBox.Value =
            1;


        worldYearSpinBox.MinValue =
            1;

        worldYearSpinBox.Step =
            1;

        worldYearSpinBox.Value =
            BaseWorldYear;


        updatingYears =
            false;
    }


    private void OnYearChanged(
        double value)
    {
        if (updatingYears)
        {
            return;
        }


        updatingYears =
            true;


        int year =
            Mathf.RoundToInt(
                (float)value
            );


        worldYearSpinBox.Value =
            BaseWorldYear +
            year -
            BaseYear;


        updatingYears =
            false;
    }


    private void OnWorldYearChanged(
        double value)
    {
        if (updatingYears)
        {
            return;
        }


        updatingYears =
            true;


        int worldYear =
            Mathf.RoundToInt(
                (float)value
            );


        int year =
            BaseYear +
            worldYear -
            BaseWorldYear;


        if (year < 1)
        {
            year = 1;
        }


        yearSpinBox.Value =
            year;


        updatingYears =
            false;
    }


    // ============================================================
    // VALIDATION TARGET
    // ============================================================

    private void SelectValidationTarget()
{
    if (string.IsNullOrWhiteSpace(
        pendingPageId))
    {
        return;
    }


    bool selectingDecision =
        pendingDecisionIndex >= 0;


    if (selectingDecision)
    {
        // Primero cambiamos de pestaña.
        // OnTabChanged puede reconstruir OptionsEditor,
        // así que la selección concreta debe hacerse DESPUÉS.
        tabBar.CurrentTab =
            1;


        bool pageSelected =
            optionsEditor.SelectPageById(
                pendingPageId
            );


        if (!pageSelected)
        {
            GD.PrintErr(
                "EventEditor: no se encontró la página de decisión: ",
                pendingPageId
            );


            pendingPageId =
                "";

            pendingDecisionIndex =
                -1;


            return;
        }


        bool decisionSelected =
            optionsEditor.SelectDecisionByIndex(
                pendingDecisionIndex
            );


        if (!decisionSelected)
        {
            GD.PrintErr(
                "EventEditor: no se encontró la opción de decisión: ",
                pendingDecisionIndex + 1
            );
        }
    }
    else
    {
        // Para páginas normales hacemos lo mismo:
        // primero cambiamos de pestaña y luego seleccionamos.
        tabBar.CurrentTab =
            0;


        bool pageSelected =
            pageEditor.SelectPageById(
                pendingPageId
            );


        if (!pageSelected)
        {
            GD.PrintErr(
                "EventEditor: no se encontró la página: ",
                pendingPageId
            );
        }
    }


    pendingPageId =
        "";

    pendingDecisionIndex =
        -1;
}


    // ============================================================
    // VALIDATION
    // ============================================================

    public ValidationResult ValidateCurrentEvent()
    {
        if (eventData == null)
        {
            ValidationResult result =
                new ValidationResult();


            result.AddError(
                "No hay ningún evento cargado para validar."
            );


            return result;
        }


        UpdateEventDataFromEditor();


        EventValidator validator =
            new EventValidator(
                chapterRepository,
                attributeRepository
            );


        return validator.Validate(
            eventData
        );
    }


    // ============================================================
    // SAVE
    // ============================================================

    private void OnSavePressed()
    {
        if (eventRepository == null)
        {
            GD.PrintErr(
                "EventEditor: no existe EventRepository."
            );


            return;
        }


        UpdateEventDataFromEditor();


        if (string.IsNullOrWhiteSpace(
            eventData.Title))
        {
            GD.PrintErr(
                "EventEditor: el evento necesita un título."
            );


            return;
        }


        if (string.IsNullOrWhiteSpace(
            eventData.Id))
        {
            eventData.Id =
                GenerateEventIdFromTitle(
                    eventData.Title
                );


            idEdit.Text =
                eventData.Id;
        }


        if (
            creatingNewEvent &&
            eventRepository.Exists(
                eventData.Id))
        {
            GD.PrintErr(
                "EventEditor: ya existe el ID ",
                eventData.Id
            );


            return;
        }


        if (!eventRepository.Save(
            eventData))
        {
            GD.PrintErr(
                "EventEditor: error guardando."
            );


            return;
        }


        creatingNewEvent =
            false;


        eventId =
            eventData.Id;


        titleLabel.Text =
            "Editor de evento: " +
            eventData.Id;


        SaveOriginalState();


        EmitSignal(
            SignalName.EventSaved
        );
    }


    // ============================================================
    // EVENT CONDITIONS
    // ============================================================

    private void OnEventConditionsPressed()
    {
        if (eventData == null)
        {
            eventData =
                new EditorEventData();
        }


        if (eventData.Conditions == null)
        {
            eventData.Conditions =
                new List<EditorConditionData>();
        }


        CloseEventConditionsWindow();


        eventConditionsWindow =
            new Window
            {
                Title =
                    "Condiciones del evento",

                Size =
                    new Vector2I(
                        650,
                        560
                    ),

                MinSize =
                    new Vector2I(
                        550,
                        450
                    ),

                Exclusive =
                    true,

                Unresizable =
                    false
            };


        eventConditionsWindow.CloseRequested +=
            CloseEventConditionsWindow;


        AddChild(
            eventConditionsWindow
        );


        VBoxContainer root =
            new VBoxContainer();

        root.SetAnchorsAndOffsetsPreset(
            LayoutPreset.FullRect
        );

        root.AddThemeConstantOverride(
            "separation",
            10
        );

        eventConditionsWindow.AddChild(
            root
        );


        eventConditionsEditor =
            new ConditionListEditor();

        eventConditionsEditor.SizeFlagsVertical =
            Control.SizeFlags.ExpandFill;

        eventConditionsEditor.CustomMinimumSize =
            new Vector2(
                0,
                400
            );


        HBoxContainer conditionsHeader =
            new HBoxContainer
            {
                Name =
                    "ConditionsHeader"
            };


        Button addConditionButton =
            new Button
            {
                Name =
                    "AddConditionButton",

                Text =
                    "Añadir condición",

                CustomMinimumSize =
                    new Vector2(
                        150,
                        35
                    )
            };

        conditionsHeader.AddChild(
            addConditionButton
        );


        VBoxContainer conditionsList =
            new VBoxContainer
            {
                Name =
                    "ConditionsList",

                SizeFlagsVertical =
                    Control.SizeFlags.ExpandFill
            };


        eventConditionsEditor.AddChild(
            conditionsHeader
        );

        eventConditionsEditor.AddChild(
            conditionsList
        );


        root.AddChild(
            eventConditionsEditor
        );


        eventConditionsEditor.SetRepository(
            eventRepository
        );

        eventConditionsEditor.SetProjectPath(
            projectPath
        );

        eventConditionsEditor.SetChapter(
            eventData.Chapter
        );

        eventConditionsEditor.SetConditions(
            new List<EditorConditionData>(
                eventData.Conditions
            )
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
                    "Cancelar",

                CustomMinimumSize =
                    new Vector2(
                        110,
                        35
                    )
            };


        Button saveButton =
            new Button
            {
                Text =
                    "Guardar",

                CustomMinimumSize =
                    new Vector2(
                        110,
                        35
                    )
            };


        buttons.AddChild(
            cancelButton
        );

        buttons.AddChild(
            saveButton
        );

        root.AddChild(
            buttons
        );


        cancelButton.Pressed +=
            CloseEventConditionsWindow;

        saveButton.Pressed +=
            SaveEventConditions;


        eventConditionsWindow.PopupCentered();
    }


    private void SaveEventConditions()
    {
        if (
            eventData == null ||
            eventConditionsEditor == null)
        {
            CloseEventConditionsWindow();

            return;
        }


        eventData.Conditions =
            eventConditionsEditor.GetConditions();


        GD.Print(
            "Condiciones del evento guardadas: ",
            eventData.Conditions.Count
        );


        CloseEventConditionsWindow();
    }


    private void CloseEventConditionsWindow()
    {
        if (eventConditionsWindow == null)
        {
            return;
        }


        Window window =
            eventConditionsWindow;


        eventConditionsWindow =
            null;

        eventConditionsEditor =
            null;


        window.QueueFree();
    }


    // ============================================================
    // AUTOMATIC ID
    // ============================================================

    private void OnTitleChanged(
        string newText)
    {
        if (!creatingNewEvent)
        {
            return;
        }


        idEdit.Text =
            GenerateEventIdFromTitle(
                newText
            );
    }


    private string GenerateEventIdFromTitle(
        string title)
    {
        if (string.IsNullOrWhiteSpace(
            title))
        {
            return "";
        }


        string normalized =
            title.Trim().Normalize(
                NormalizationForm.FormD
            );


        StringBuilder builder =
            new StringBuilder();


        bool previousWasSeparator =
            false;


        foreach (
            char character
            in normalized)
        {
            UnicodeCategory category =
                CharUnicodeInfo.GetUnicodeCategory(
                    character
                );


            if (
                category ==
                UnicodeCategory.NonSpacingMark)
            {
                continue;
            }


            char lower =
                char.ToLowerInvariant(
                    character
                );


            if (
                char.IsLetterOrDigit(
                    lower))
            {
                builder.Append(
                    lower
                );


                previousWasSeparator =
                    false;


                continue;
            }


            if (
                !previousWasSeparator &&
                builder.Length > 0)
            {
                builder.Append(
                    '_'
                );


                previousWasSeparator =
                    true;
            }
        }


        string baseId =
            builder.ToString().Trim(
                '_'
            );


        if (string.IsNullOrWhiteSpace(
            baseId))
        {
            return "";
        }


        string candidate =
            baseId;


        if (eventRepository == null)
        {
            return candidate;
        }


        int suffix =
            2;


        while (
            eventRepository.Exists(
                candidate))
        {
            candidate =
                $"{baseId}_{suffix}";


            suffix++;
        }


        return candidate;
    }


    // ============================================================
    // UNSAVED
    // ============================================================

    private EditorEventData GetCurrentEventState()
    {
        UpdateEventDataFromEditor();

        return eventData;
    }


    private void SaveOriginalState()
    {
        unsavedChangesGuard?.SaveOriginalState(
            eventData
        );
    }


    public bool HasUnsavedChanges()
    {
        return
            unsavedChangesGuard != null &&
            unsavedChangesGuard.HasUnsavedChanges();
    }


    public void RequestApplicationClose()
    {
        applicationCloseRequested =
            true;


        if (unsavedChangesGuard == null)
        {
            EmitSignal(
                SignalName.ApplicationCloseConfirmed
            );


            return;
        }


        unsavedChangesGuard.RequestClose();
    }


    private void CloseEditor()
    {
        if (applicationCloseRequested)
        {
            applicationCloseRequested =
                false;


            EmitSignal(
                SignalName.ApplicationCloseConfirmed
            );


            return;
        }


        EmitSignal(
            SignalName.EventCancelled
        );
    }


    private void OnClosePressed()
    {
        RequestClose();
    }


    private void OnCancelPressed()
    {
        RequestClose();
    }


    private void RequestClose()
    {
        if (unsavedChangesGuard == null)
        {
            CloseEditor();

            return;
        }


        unsavedChangesGuard.RequestClose();
    }
}