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

        eventData =
            new EditorEventData
            {
                Id = "",
                Title = "",
                Chapter = 1,
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

            eventData =
                new EditorEventData
                {
                    Id = id,
                    Title = "",
                    Chapter = 1,
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


    private void LoadChapter(
        int chapter)
    {
        if (chapter < 1)
        {
            chapter = 1;
        }

        int chapterIndex =
            chapter - 1;

        if (
            chapterIndex < 0 ||
            chapterIndex >= chapterOption.ItemCount)
        {
            chapterIndex = 0;
        }

        chapterOption.Select(
            chapterIndex
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
                    : 1
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

        if (!pageSelected &&
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

        eventData.Chapter =
            chapterOption.Selected + 1;

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

        if (creatingNewEvent &&
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

        EventValidator validator =
            new EventValidator();

        ValidationResult result =
            validator.Validate(
                eventData
            );

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