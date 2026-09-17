using Godot;
using System.Collections.Generic;

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

    private Button closeButton;
    private Button cancelButton;
    private Button saveButton;

    private EditorEventData eventData;

    private EventRepository eventRepository;
    private ChapterRepository chapterRepository;
    private AttributeRepository attributeRepository;

    private string eventId = "";
    private string projectPath = "";

    private bool updatingYears = false;
    private bool creatingNewEvent = false;

    private UnsavedChangesGuard<EditorEventData> unsavedChangesGuard;

    private const int BaseWorldYear = 1134;
    private const int BaseYear = 1;

    private string pendingPageId = "";
    private int pendingDecisionIndex = -1;

    private bool applicationCloseRequested = false;


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
                "VBoxContainer/EventInfo/TitleContainer/TitleEdit"
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


        pageEditor =
            GetNode<EventPageEditor>(
                "VBoxContainer/Content/EventPageEditor"
            );


        optionsEditor =
            GetNode<OptionsEditor>(
                "VBoxContainer/Content/OptionsEditor"
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


        closeButton.Pressed +=
            OnClosePressed;


        cancelButton.Pressed +=
            OnCancelPressed;


        saveButton.Pressed +=
            OnSavePressed;


        yearSpinBox.ValueChanged +=
            OnYearChanged;


        worldYearSpinBox.ValueChanged +=
            OnWorldYearChanged;


        chapterOption.ItemSelected +=
            OnChapterSelected;


        tabBar.TabChanged +=
            OnTabChanged;


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


    public void SetRepository(
        EventRepository repository)
    {
        eventRepository =
            repository;


        if (optionsEditor != null)
        {
            optionsEditor.SetRepository(
                eventRepository
            );
        }
    }


    public void SetProjectPath(
        string path)
    {
        projectPath =
            path ?? "";


        if (optionsEditor != null)
        {
            optionsEditor.SetProjectPath(
                projectPath
            );
        }


        if (string.IsNullOrWhiteSpace(
            projectPath))
        {
            chapterRepository =
                null;

            attributeRepository =
                null;

            chapterOption.Clear();


            GD.PrintErr(
                "EventEditor: la ruta del proyecto está vacía."
            );

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


        GD.Print(
            "EventEditor: ruta del proyecto establecida: ",
            projectPath
        );
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


    public void CreateNewEvent()
    {
        creatingNewEvent = true;
        eventId = "";


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
                Pages = new List<EditorPageData>()
            };


        LoadEventIntoEditor();


        SaveOriginalState();


        GD.Print(
            "EventEditor: creando evento nuevo."
        );
    }


    public void LoadEvent(
        string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            GD.PrintErr(
                "EventEditor: ID de evento vacío."
            );

            return;
        }


        creatingNewEvent = false;

        eventId =
            id;


        if (eventRepository == null)
        {
            GD.PrintErr(
                "EventEditor: no se ha configurado EventRepository."
            );

            return;
        }


        EditorEventData loadedEvent =
            eventRepository.Load(
                id
            );


        if (loadedEvent == null)
        {
            GD.Print(
                "EventEditor: el evento no existe. Creando evento nuevo: ",
                id
            );


            creatingNewEvent = true;


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
                    Pages = new List<EditorPageData>()
                };
        }
        else
        {
            eventData =
                loadedEvent;


            if (eventData.Pages == null)
            {
                eventData.Pages =
                    new List<EditorPageData>();
            }


            GD.Print(
                "EventEditor: evento cargado correctamente: ",
                eventData.Id
            );
        }


        LoadEventIntoEditor();


        SaveOriginalState();
    }
    public void SaveCurrentEvent()
{
    OnSavePressed();
}


    private void LoadEventIntoEditor()
    {
        if (eventData == null)
        {
            return;
        }


        eventId =
            eventData.Id;


        idEdit.Text =
            eventData.Id;


        titleEdit.Text =
            eventData.Title;


        if (creatingNewEvent)
        {
            titleLabel.Text =
                "Nuevo evento";
        }
        else
        {
            titleLabel.Text =
                "Editor de evento: " +
                eventData.Id;
        }


        LoadChapter(
            eventData.Chapter
        );


        updatingYears = true;


        yearSpinBox.Value =
            eventData.Year;


        worldYearSpinBox.Value =
            eventData.WorldYear;


        updatingYears = false;


        pageEditor.SetPages(
            eventData.Pages
        );


        optionsEditor.SetPages(
            eventData.Pages
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


        SelectValidationTarget();


        GD.Print(
            "EventEditor: datos cargados en la interfaz."
        );


        if (!creatingNewEvent)
        {
            TestValidator();
        }
    }


    private EditorEventData GetCurrentEventState()
    {
        UpdateEventDataFromEditor();

        return eventData;
    }


    private void SaveOriginalState()
    {
        if (unsavedChangesGuard == null)
        {
            return;
        }


        unsavedChangesGuard.SaveOriginalState(
            eventData
        );
    }


    public bool HasUnsavedChanges()
    {
        if (unsavedChangesGuard == null)
        {
            return false;
        }


        return unsavedChangesGuard.HasUnsavedChanges();
    }


    public void RequestApplicationClose()
    {
        applicationCloseRequested = true;


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
            applicationCloseRequested = false;


            EmitSignal(
                SignalName.ApplicationCloseConfirmed
            );

            return;
        }


        EmitSignal(
            SignalName.EventCancelled
        );
    }


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


            if (chapter.Number <= 0)
            {
                continue;
            }


            string displayName =
                chapter.Number +
                ". " +
                chapter.Title;


            int index =
                chapterOption.ItemCount;


            chapterOption.AddItem(
                displayName
            );


            chapterOption.SetItemMetadata(
                index,
                chapter.Number
            );
        }


        GD.Print(
            "EventEditor: capítulos cargados: ",
            chapterOption.ItemCount
        );
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
            if (chapter == null)
            {
                continue;
            }


            if (chapter.Number > 0)
            {
                return chapter.Number;
            }
        }


        return 1;
    }


    private void LoadChapter(
        int chapter)
    {
        if (chapterOption.ItemCount == 0)
        {
            return;
        }


        for (
            int i = 0;
            i < chapterOption.ItemCount;
            i++)
        {
            Variant metadata =
                chapterOption.GetItemMetadata(
                    i
                );


            int chapterNumber =
                metadata.AsInt32();


            if (chapterNumber == chapter)
            {
                chapterOption.Select(
                    i
                );

                return;
            }
        }


        GD.PrintErr(
            "EventEditor: no se encontró el capítulo: ",
            chapter
        );


        chapterOption.Select(
            0
        );
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


        if (chapter <= 0)
        {
            return;
        }


        if (eventData != null)
        {
            eventData.Chapter =
                chapter;
        }


        optionsEditor.SetChapter(
            chapter
        );


        GD.Print(
            "EventEditor: capítulo cambiado a: ",
            chapter
        );
    }


    private void InitializeYears()
    {
        updatingYears = true;


        yearSpinBox.MinValue =
            1;


        yearSpinBox.Step =
            1;


        yearSpinBox.Value =
            BaseYear;


        worldYearSpinBox.MinValue =
            1;


        worldYearSpinBox.Step =
            1;


        worldYearSpinBox.Value =
            BaseWorldYear;


        updatingYears = false;
    }


    private void OnYearChanged(
        double value)
    {
        if (updatingYears)
        {
            return;
        }


        updatingYears = true;


        int year =
            Mathf.RoundToInt(
                (float)value
            );


        int worldYear =
            BaseWorldYear +
            (year - BaseYear);


        worldYearSpinBox.Value =
            worldYear;


        updatingYears = false;
    }


    private void OnWorldYearChanged(
        double value)
    {
        if (updatingYears)
        {
            return;
        }


        updatingYears = true;


        int worldYear =
            Mathf.RoundToInt(
                (float)value
            );


        int year =
            BaseYear +
            (worldYear - BaseWorldYear);


        if (year < 1)
        {
            year = 1;
        }


        yearSpinBox.Value =
            year;


        updatingYears = false;
    }


    private void OnTabChanged(
        long tab)
    {
        bool showingNarrative =
            tab == 0;


        pageEditor.Visible =
            showingNarrative;


        optionsEditor.Visible =
            !showingNarrative;


        if (!showingNarrative)
        {
            List<EditorPageData> pages =
                pageEditor.GetPages();


            optionsEditor.SetPages(
                pages
            );


            optionsEditor.SetRepository(
                eventRepository
            );


            optionsEditor.SetProjectPath(
                projectPath
            );


            optionsEditor.SetChapter(
                eventData != null
                    ? eventData.Chapter
                    : GetDefaultChapterNumber()
            );
        }
    }


    private void SelectValidationTarget()
    {
        if (string.IsNullOrWhiteSpace(
            pendingPageId))
        {
            return;
        }


        bool pageSelected =
            pageEditor.SelectPageById(
                pendingPageId
            );


        bool optionsPageSelected =
            optionsEditor.SelectPageById(
                pendingPageId
            );


        if (
            !pageSelected &&
            !optionsPageSelected)
        {
            GD.PrintErr(
                "EventEditor: no se encontró la página de validación: ",
                pendingPageId
            );


            return;
        }


        if (pendingDecisionIndex >= 0)
        {
            bool decisionSelected =
                optionsEditor.SelectDecisionByIndex(
                    pendingDecisionIndex
                );


            if (!decisionSelected)
            {
                GD.PrintErr(
                    "EventEditor: no se encontró la opción de validación: ",
                    pendingDecisionIndex + 1
                );


                return;
            }


            tabBar.CurrentTab =
                1;
        }
        else
        {
            tabBar.CurrentTab =
                0;
        }


        pendingPageId =
            "";


        pendingDecisionIndex =
            -1;
    }


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


        List<EditorPageData> narrativePages =
            pageEditor.GetPages();


        List<EditorPageData> optionPages =
            optionsEditor.GetPages();


        if (narrativePages == null)
        {
            narrativePages =
                new List<EditorPageData>();
        }


        if (optionPages == null)
        {
            optionPages =
                new List<EditorPageData>();
        }


        foreach (
            EditorPageData optionPage
            in optionPages)
        {
            if (optionPage == null)
            {
                continue;
            }


            foreach (
                EditorPageData narrativePage
                in narrativePages)
            {
                if (
                    narrativePage == null ||
                    narrativePage.Id != optionPage.Id)
                {
                    continue;
                }


                narrativePage.Decisions =
                    optionPage.Decisions;


                break;
            }
        }


        eventData.Pages =
            narrativePages;


        GD.Print(
            "EventEditor: modelo actualizado desde la interfaz."
        );


        GD.Print(
            "  ID: ",
            eventData.Id
        );


        GD.Print(
            "  Título: ",
            eventData.Title
        );


        GD.Print(
            "  Capítulo: ",
            eventData.Chapter
        );


        GD.Print(
            "  Año: ",
            eventData.Year
        );


        GD.Print(
            "  Año mundial: ",
            eventData.WorldYear
        );


        GD.Print(
            "  Páginas: ",
            eventData.Pages.Count
        );


        foreach (
            EditorPageData page
            in eventData.Pages)
        {
            if (
                page == null ||
                page.Decisions == null)
            {
                continue;
            }


            GD.Print(
                "  Página ",
                page.Id,
                " | Decisiones: ",
                page.Decisions.Count
            );


            foreach (
                EditorDecisionData decision
                in page.Decisions)
            {
                if (decision == null)
                {
                    continue;
                }


                GD.Print(
                    "    Decisión: ",
                    decision.Text,
                    " | Condiciones: ",
                    decision.Conditions.Count,
                    " | Efectos: ",
                    decision.Effects.Count
                );
            }
        }
    }


    public ValidationResult ValidateCurrentEvent()
    {
        if (eventData == null)
        {
            ValidationResult emptyResult =
                new ValidationResult();


            emptyResult.AddError(
                "No hay ningún evento cargado para validar."
            );


            return emptyResult;
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


    private void OnSavePressed()
    {
        if (eventRepository == null)
        {
            GD.PrintErr(
                "EventEditor: no se puede guardar porque no hay EventRepository."
            );


            return;
        }


        UpdateEventDataFromEditor();


        if (string.IsNullOrWhiteSpace(
            eventData.Id))
        {
            GD.PrintErr(
                "EventEditor: no se puede guardar un evento sin ID."
            );


            return;
        }


        if (
            creatingNewEvent &&
            eventRepository.Exists(
                eventData.Id))
        {
            GD.PrintErr(
                "EventEditor: ya existe un evento con el ID '",
                eventData.Id,
                "'."
            );


            return;
        }


        bool saved =
            eventRepository.Save(
                eventData
            );


        if (!saved)
        {
            GD.PrintErr(
                "EventEditor: error guardando el evento."
            );


            return;
        }


        eventId =
            eventData.Id;


        creatingNewEvent = false;


        titleLabel.Text =
            "Editor de evento: " +
            eventData.Id;


        SaveOriginalState();


        GD.Print(
            "EventEditor: evento guardado correctamente."
        );


        EmitSignal(
            SignalName.EventSaved
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


    private void TestValidator()
    {
        if (eventData == null)
        {
            GD.PrintErr(
                "EventEditor: no hay evento cargado para validar."
            );


            return;
        }


        ValidationResult result =
            ValidateCurrentEvent();


        GD.Print(
            "========================================"
        );


        GD.Print(
            "VALIDACIÓN DEL EVENTO: ",
            eventData.Id
        );


        GD.Print(
            "========================================"
        );


        if (result.IsValid)
        {
            GD.Print(
                "✓ El evento no tiene errores."
            );
        }
        else
        {
            GD.Print(
                "✗ El evento contiene ",
                result.Errors.Count,
                " errores."
            );


            foreach (
                string error
                in result.Errors)
            {
                GD.PrintErr(
                    "ERROR: ",
                    error
                );
            }
        }


        if (result.Warnings.Count == 0)
        {
            GD.Print(
                "✓ El evento no tiene advertencias."
            );
        }
        else
        {
            GD.Print(
                "⚠ El evento contiene ",
                result.Warnings.Count,
                " advertencias."
            );


            foreach (
                string warning
                in result.Warnings)
            {
                GD.Print(
                    "ADVERTENCIA: ",
                    warning
                );
            }
        }


        GD.Print(
            "========================================"
        );
    }
}