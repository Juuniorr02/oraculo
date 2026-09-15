using Godot;
using System;
using System.Collections.Generic;

public partial class InterludeEditor : Control
{
    [Signal]
    public delegate void InterludeSavedEventHandler();

    [Signal]
    public delegate void InterludeCancelledEventHandler();

    [Signal]
    public delegate void ApplicationCloseConfirmedEventHandler();

    private InterludeRepository interludeRepository;
    private EventRepository eventRepository;

    private EditorInterludeData currentInterlude;

    private LineEdit idEdit;
    private LineEdit titleEdit;

    private OptionButton chapterOption;
    private OptionButton styleOption;

    private EventSelector eventSelector;
    private OptionButton nextChapterOption;

    private InterludePageEditor pageEditor;
    private ConditionListEditor conditionEditor;
    private EffectListEditor effectEditor;

    private Button closeButton;
    private Button cancelButton;
    private Button saveButton;

    private bool isNewInterlude = false;

    private string pendingPageId = "";

    private int pendingConditionIndex = -1;

    private int pendingEffectIndex = -1;

    private string projectPath = "";

    private UnsavedChangesGuard<EditorInterludeData> unsavedChangesGuard;

    private bool applicationCloseRequested = false;


    public override void _Ready()
    {
        idEdit =
            GetNode<LineEdit>(
                "VBoxContainer/InterludeInfo/BasicInfo/IdContainer/IdEdit"
            );

        titleEdit =
            GetNode<LineEdit>(
                "VBoxContainer/InterludeInfo/BasicInfo/TitleContainer/TitleEdit"
            );

        chapterOption =
            GetNode<OptionButton>(
                "VBoxContainer/InterludeInfo/BasicInfo/ChapterContainer/ChapterOption"
            );

        styleOption =
            GetNode<OptionButton>(
                "VBoxContainer/InterludeInfo/BasicInfo/StyleContainer/StyleOption"
            );

        eventSelector =
            GetNode<EventSelector>(
                "VBoxContainer/InterludeInfo/BasicInfo/NextEventContainer/EventSelector"
            );

        nextChapterOption =
            GetNode<OptionButton>(
                "VBoxContainer/InterludeInfo/BasicInfo/NextChapterContainer/NextChapterOption"
            );

        pageEditor =
            GetNode<InterludePageEditor>(
                "VBoxContainer/Content/TabContainer/Narrativa/InterludePageEditor"
            );

        conditionEditor =
            GetNode<ConditionListEditor>(
                "VBoxContainer/Content/TabContainer/Condiciones/ConditionListEditor"
            );

        effectEditor =
            GetNode<EffectListEditor>(
                "VBoxContainer/Content/TabContainer/Consecuencias/EffectListEditor"
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

        chapterOption.ItemSelected +=
            OnChapterChanged;


        unsavedChangesGuard =
            new UnsavedChangesGuard<EditorInterludeData>(
                this,
                GetCurrentInterludeState,
                CloseEditor
            );


        ApplyChapterToEditors();
    }


    public void SetProjectPath(
        string path)
    {
        projectPath =
            path ?? "";

        if (conditionEditor != null)
        {
            conditionEditor.SetProjectPath(
                projectPath
            );
        }

        if (effectEditor != null)
        {
            effectEditor.SetProjectPath(
                projectPath
            );
        }
    }


    public void SetInterludeRepository(
        InterludeRepository repository)
    {
        interludeRepository = repository;
    }


    public void SetEventRepository(
        EventRepository repository)
    {
        eventRepository = repository;

        if (eventSelector != null)
        {
            eventSelector.SetRepository(
                eventRepository
            );
        }

        if (conditionEditor != null)
        {
            conditionEditor.SetRepository(
                eventRepository
            );
        }

        if (effectEditor != null)
        {
            effectEditor.SetRepository(
                eventRepository
            );
        }
    }


    public void SetValidationTarget(
        string pageId,
        int conditionIndex = -1,
        int effectIndex = -1)
    {
        pendingPageId =
            pageId ?? "";

        pendingConditionIndex =
            conditionIndex;

        pendingEffectIndex =
            effectIndex;
    }


    public void CreateNewInterlude()
    {
        currentInterlude =
            new EditorInterludeData();

        isNewInterlude = true;

        idEdit.Text = "";
        titleEdit.Text = "";

        chapterOption.Select(0);

        styleOption.Select(
            (int)InterludeStyle.Letter
        );

        eventSelector.ClearSelection();

        nextChapterOption.Select(0);

        ApplyChapterToEditors();

        pageEditor.SetPages(
            currentInterlude.Pages
        );

        conditionEditor.SetConditions(
            currentInterlude.Conditions
        );

        effectEditor.SetEffects(
            currentInterlude.Effects
        );

        pendingPageId = "";

        pendingConditionIndex = -1;

        pendingEffectIndex = -1;

        SaveOriginalState();
    }


    public void LoadInterlude(
        string interludeId)
    {
        if (interludeRepository == null)
        {
            GD.PrintErr(
                "InterludeEditor: no hay repositorio de interludios."
            );

            return;
        }

        EditorInterludeData interlude =
            interludeRepository.Load(
                interludeId
            );

        if (interlude == null)
        {
            GD.PrintErr(
                "InterludeEditor: no se pudo cargar el interludio: ",
                interludeId
            );

            return;
        }

        currentInterlude =
            interlude;

        isNewInterlude = false;

        LoadInterludeIntoEditor(
            interlude
        );

        SaveOriginalState();
    }


    private void LoadInterludeIntoEditor(
        EditorInterludeData interlude)
    {
        idEdit.Text =
            interlude.Id ?? "";

        titleEdit.Text =
            interlude.Title ?? "";

        LoadChapter(
            interlude.Chapter
        );

        if (Enum.IsDefined(
            typeof(InterludeStyle),
            interlude.Style))
        {
            styleOption.Select(
                (int)interlude.Style
            );
        }
        else
        {
            styleOption.Select(
                (int)InterludeStyle.Letter
            );
        }


        eventSelector.SetSelectedEventId(
            interlude.NextEventId
        );


        int nextChapter =
            interlude.NextChapter;

        if (nextChapter < 0 ||
            nextChapter > 7)
        {
            nextChapter = 0;
        }

        nextChapterOption.Select(
            nextChapter
        );


        ApplyChapterToEditors();


        if (interlude.Pages == null)
        {
            interlude.Pages =
                new List<EditorInterludePageData>();
        }

        if (interlude.Conditions == null)
        {
            interlude.Conditions =
                new List<EditorConditionData>();
        }

        if (interlude.Effects == null)
        {
            interlude.Effects =
                new List<EditorEffectData>();
        }


        pageEditor.SetPages(
            interlude.Pages
        );

        conditionEditor.SetConditions(
            interlude.Conditions
        );

        effectEditor.SetEffects(
            interlude.Effects
        );


        SelectValidationTarget();
    }


    public void SetRepository(
        InterludeRepository repository)
    {
        interludeRepository = repository;
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


    public bool HasUnsavedChanges()
    {
        if (unsavedChangesGuard == null)
        {
            return false;
        }

        return unsavedChangesGuard.HasUnsavedChanges();
    }


    private EditorInterludeData GetCurrentInterludeState()
    {
        UpdateInterludeDataFromEditor();

        return currentInterlude;
    }


    private void SaveOriginalState()
    {
        if (unsavedChangesGuard == null)
        {
            return;
        }

        unsavedChangesGuard.SaveOriginalState(
            currentInterlude
        );
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
            SignalName.InterludeCancelled
        );
    }


    private void UpdateInterludeDataFromEditor()
    {
        if (currentInterlude == null)
        {
            return;
        }


        currentInterlude.Id =
            idEdit.Text.Trim();

        currentInterlude.Title =
            titleEdit.Text.Trim();


        currentInterlude.Chapter =
            chapterOption.Selected + 1;


        currentInterlude.Style =
            (InterludeStyle)
            styleOption.Selected;


        currentInterlude.NextEventId =
            eventSelector.GetSelectedEventId();


        currentInterlude.NextChapter =
            nextChapterOption.Selected;


        currentInterlude.Pages =
            pageEditor.GetPages();

        currentInterlude.Conditions =
            conditionEditor.GetConditions();

        currentInterlude.Effects =
            effectEditor.GetEffects();
    }


    private void LoadChapter(
        int chapter)
    {
        if (chapter < 1 ||
            chapter > 7)
        {
            chapter = 1;
        }

        chapterOption.Select(
            chapter - 1
        );
    }


    private void ApplyChapterToEditors()
    {
        if (chapterOption == null)
        {
            return;
        }

        if (conditionEditor != null)
        {
            conditionEditor.SetChapter(
                chapterOption.Selected + 1
            );
        }

        if (effectEditor != null)
        {
            effectEditor.SetChapter(
                chapterOption.Selected + 1
            );
        }
    }


    private void OnChapterChanged(
        long index)
    {
        ApplyChapterToEditors();
    }


    private void SelectValidationTarget()
    {
        if (pendingConditionIndex >= 0)
        {
            conditionEditor.SelectCondition(
                pendingConditionIndex
            );

            pendingPageId = "";
            pendingConditionIndex = -1;
            pendingEffectIndex = -1;

            return;
        }


        if (pendingEffectIndex >= 0)
        {
            effectEditor.SelectEffect(
                pendingEffectIndex
            );

            pendingPageId = "";
            pendingConditionIndex = -1;
            pendingEffectIndex = -1;

            return;
        }


        if (string.IsNullOrWhiteSpace(
            pendingPageId))
        {
            return;
        }

        bool pageSelected =
            pageEditor.SelectPageById(
                pendingPageId
            );

        if (!pageSelected)
        {
            GD.PrintErr(
                "InterludeEditor: no se encontró la página de validación: ",
                pendingPageId
            );

            return;
        }

        pendingPageId = "";
    }


    private void OnSavePressed()
    {
        if (currentInterlude == null)
        {
            return;
        }

        UpdateInterludeDataFromEditor();

        if (interludeRepository == null)
        {
            GD.PrintErr(
                "InterludeEditor: no hay repositorio de interludios."
            );

            return;
        }

        interludeRepository.Save(
            currentInterlude
        );

        isNewInterlude = false;

        SaveOriginalState();

        EmitSignal(
            SignalName.InterludeSaved
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


    private void OnApplicationCloseRequested()
    {
        RequestApplicationClose();
    }
}
