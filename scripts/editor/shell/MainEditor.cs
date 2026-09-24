using System;
using System.Collections.Generic;
using Godot;


public partial class MainEditor : Control
{
    private Button eventsButton;
    private Button interludesButton;
    private Button chaptersButton;
    private Button attributesButton;
    private Button charactersButton;
    private Button relationshipsButton;
    private Button settingsButton;
    private Button exitButton;
    private Button validateButton;
    private Button saveButton;


    private Control workspaceContent;
    private Control emptyWorkspace;


    private PackedScene settingsDialogScene;
    private PackedScene eventListViewScene;
    private PackedScene eventEditorScene;
    private PackedScene interludeListViewScene;
    private PackedScene interludeEditorScene;
    private PackedScene chaptersListViewScene;
    private PackedScene chapterEditorScene;
    private PackedScene chapterPageEditorScene;
    private PackedScene attributeListViewScene;
    private PackedScene attributeEditorScene;
    private PackedScene characterListViewScene;
    private PackedScene characterEditorScene;
    private PackedScene relationshipListViewScene;
    private PackedScene relationshipEditorScene;
    private PackedScene validationPanelScene;


    private SettingsDialog settingsDialog;


    private Control currentView;


    private EventListView eventListView;
    private InterludeListView interludeListView;
    private ChaptersListView chaptersListView;
    private ChapterEditor chapterEditor;
    private ChapterPageEditor chapterPageEditor;
    private AttributeListView attributeListView;
    private AttributeEditor attributeEditor;
    private CharacterListView characterListView;
    private CharacterEditor characterEditor;
    private RelationshipListView relationshipListView;
    private RelationshipEditor relationshipEditor;
    private ValidationPanel validationPanel;


    private Button validationDetailsButton;
    private Label validationStatusLabel;


    private ProjectManager projectManager;
    private EventRepository eventRepository;
    private InterludeRepository interludeRepository;


    private bool returnToChapterEditor = false;
    private string chapterEditorId = "";


    private Action pendingNavigationAction;
    private ConfirmationDialog navigationDialog;


    private bool keepEditorOpenAfterGlobalSave = false;


    public override void _Ready()
    {
        ApplyOraculoTheme();


        projectManager =
            GetNode<ProjectManager>(
                "../ProjectManager"
            );


        if (projectManager.HasProject())
        {
            string projectPath =
                projectManager.GetProjectPath();


            eventRepository =
                new EventRepository(
                    projectPath
                );


            interludeRepository =
                new InterludeRepository(
                    projectPath
                );
        }


        eventsButton =
            GetNode<Button>(
                "MainLayout/MainArea/SideBar/MarginContainer/VBoxContainer/Navigation/EventsButton"
            );


        interludesButton =
            GetNode<Button>(
                "MainLayout/MainArea/SideBar/MarginContainer/VBoxContainer/Navigation/InterludesButton"
            );


        chaptersButton =
            GetNode<Button>(
                "MainLayout/MainArea/SideBar/MarginContainer/VBoxContainer/Navigation/ChaptersButton"
            );


        attributesButton =
            GetNode<Button>(
                "MainLayout/MainArea/SideBar/MarginContainer/VBoxContainer/Navigation/AttributesButton"
            );


        charactersButton =
            GetNode<Button>(
                "MainLayout/MainArea/SideBar/MarginContainer/VBoxContainer/Navigation/CharactersButton"
            );


        relationshipsButton =
            GetNode<Button>(
                "MainLayout/MainArea/SideBar/MarginContainer/VBoxContainer/Navigation/RelationshipsButton"
            );


        settingsButton =
            GetNode<Button>(
                "MainLayout/TopBar/MarginContainer/HBoxContainer/SettingsButton"
            );


        exitButton =
            GetNode<Button>(
                "MainLayout/TopBar/MarginContainer/HBoxContainer/ExitButton"
            );


        saveButton =
            GetNode<Button>(
                "MainLayout/TopBar/MarginContainer/HBoxContainer/SaveButton"
            );


        validateButton =
            GetNode<Button>(
                "MainLayout/TopBar/MarginContainer/HBoxContainer/ValidateButton"
            );


        workspaceContent =
            GetNode<Control>(
                "MainLayout/MainArea/WorkSpace/MarginContainer/VBoxContainer/WorkspaceContent"
            );


        emptyWorkspace =
            GetNode<Control>(
                "MainLayout/MainArea/WorkSpace/MarginContainer/VBoxContainer/WorkspaceContent/EmptyWorkspace"
            );


        validationStatusLabel =
            GetNode<Label>(
                "MainLayout/StatusBar/MarginContainer/HBoxContainer/StatusLabel"
            );


        validationDetailsButton =
            GetNode<Button>(
                "MainLayout/StatusBar/MarginContainer/HBoxContainer/DetailsButton"
            );


        settingsDialogScene =
            GD.Load<PackedScene>(
                "res://scenes/SettingsDialog.tscn"
            );


        eventListViewScene =
            GD.Load<PackedScene>(
                "res://scenes/EventListView.tscn"
            );


        eventEditorScene =
            GD.Load<PackedScene>(
                "res://scenes/EventEditor.tscn"
            );


        interludeListViewScene =
            GD.Load<PackedScene>(
                "res://scenes/InterludeListView.tscn"
            );


        interludeEditorScene =
            GD.Load<PackedScene>(
                "res://scenes/InterludeEditor.tscn"
            );


        chaptersListViewScene =
            GD.Load<PackedScene>(
                "res://scenes/ChaptersListView.tscn"
            );


        chapterEditorScene =
            GD.Load<PackedScene>(
                "res://scenes/ChapterEditor.tscn"
            );


        chapterPageEditorScene =
            GD.Load<PackedScene>(
                "res://scenes/ChapterPageEditor.tscn"
            );


        attributeListViewScene =
            GD.Load<PackedScene>(
                "res://scenes/AttributeListView.tscn"
            );


        attributeEditorScene =
            GD.Load<PackedScene>(
                "res://scenes/AttributeEditor.tscn"
            );


        characterListViewScene =
            GD.Load<PackedScene>(
                "res://scenes/CharacterListView.tscn"
            );


        characterEditorScene =
            GD.Load<PackedScene>(
                "res://scenes/CharacterEditor.tscn"
            );


        relationshipListViewScene =
            GD.Load<PackedScene>(
                "res://scenes/RelationshipListView.tscn"
            );


        relationshipEditorScene =
            GD.Load<PackedScene>(
                "res://scenes/RelationshipEditor.tscn"
            );


        validationPanelScene =
            GD.Load<PackedScene>(
                "res://scenes/ValidationPanel.tscn"
            );


        eventsButton.Pressed +=
            OnEventsPressed;


        interludesButton.Pressed +=
            OnInterludesPressed;


        chaptersButton.Pressed +=
            OnChaptersPressed;


        attributesButton.Pressed +=
            OnAttributesPressed;


        charactersButton.Pressed +=
            OnCharactersPressed;


        relationshipsButton.Pressed +=
            OnRelationshipsPressed;


        settingsButton.Pressed +=
            OnSettingsPressed;


        exitButton.Pressed +=
            OnExitPressed;


        saveButton.Pressed +=
            OnSavePressed;


        validateButton.Pressed +=
            OnValidatePressed;


        validationDetailsButton.Pressed +=
            OnValidationDetailsPressed;


        UpdateValidationStatus();


        GD.Print(
            "MainEditor iniciado."
        );
    }


    private void ApplyOraculoTheme()
    {
        Theme theme =
            GD.Load<Theme>(
                "res://resources/OraculoTheme.tres"
            );


        Theme =
            theme;
    }


    private void OnSavePressed()
    {
        keepEditorOpenAfterGlobalSave =
            true;


        CallDeferred(
            nameof(ClearGlobalSaveFlag)
        );


        if (currentView is EventEditor eventEditor)
        {
            eventEditor.SaveCurrentEvent();

            return;
        }


        if (currentView is InterludeEditor interludeEditor)
        {
            interludeEditor.SaveCurrentInterlude();

            return;
        }


        if (currentView is ChapterEditor chapterEditorView)
        {
            chapterEditorView.SaveCurrentChapter();

            return;
        }


        if (currentView is ChapterPageEditor)
        {
            SaveChapterFromPageEditor();

            return;
        }


        if (currentView is AttributeEditor attributeEditorView)
        {
            attributeEditorView.SaveCurrentAttribute();

            return;
        }


        if (currentView is CharacterEditor characterEditorView)
        {
            characterEditorView.SaveCurrentCharacter();

            return;
        }


        if (currentView is RelationshipEditor relationshipEditorView)
        {
            relationshipEditorView.SaveCurrentRelationship();

            return;
        }


        keepEditorOpenAfterGlobalSave =
            false;


        GD.Print(
            "MainEditor: no hay nada que guardar en la vista actual."
        );
    }


    private void ClearGlobalSaveFlag()
    {
        keepEditorOpenAfterGlobalSave =
            false;
    }


    private void SaveChapterFromPageEditor()
    {
        if (chapterPageEditor == null)
        {
            GD.PrintErr(
                "MainEditor: ChapterPageEditor no está disponible."
            );

            return;
        }


        if (chapterEditor == null)
        {
            GD.PrintErr(
                "MainEditor: ChapterEditor no está disponible."
            );

            return;
        }


        List<ChapterPageDefinitionData> pages =
            chapterPageEditor.GetPages();


        chapterEditor.SetPages(
            pages
        );


        chapterEditor.SaveCurrentChapter();
    }


    private void OnValidatePressed()
    {
        GD.Print(
            "MainEditor: validación manual iniciada."
        );


        if (currentView is EventEditor eventEditor)
        {
            ValidationResult result =
                eventEditor.ValidateCurrentEvent();


            UpdateValidationStatus(
                result
            );


            GD.Print(
                $"MainEditor: validación del evento actual completada. " +
                $"{result.GetErrorCount()} errores, " +
                $"{result.GetWarningCount()} advertencias."
            );


            return;
        }


        UpdateValidationStatus();
    }


    private void UpdateValidationStatus()
    {
        if (validationStatusLabel == null)
        {
            return;
        }


        if (
            projectManager == null ||
            !projectManager.HasProject() ||
            eventRepository == null)
        {
            validationStatusLabel.Text =
                "✕ No hay proyecto cargado";


            return;
        }


        validationStatusLabel.Text =
            "⟳ Validando proyecto...";


        ProjectValidator projectValidator =
            new ProjectValidator(
                eventRepository,
                interludeRepository,
                projectManager.GetProjectPath()
            );


        ValidationResult result =
            projectValidator.ValidateProject();


        int errorCount =
            result.GetErrorCount();


        int warningCount =
            result.GetWarningCount();


        if (errorCount > 0)
        {
            validationStatusLabel.Text =
                $"✕ Proyecto con errores · " +
                $"{errorCount} errores · " +
                $"{warningCount} advertencias";


            GD.Print(
                $"MainEditor: validación completada. " +
                $"{errorCount} errores, {warningCount} advertencias."
            );


            return;
        }


        if (warningCount > 0)
        {
            validationStatusLabel.Text =
                $"⚠ Proyecto válido con advertencias · " +
                $"{warningCount} advertencias";


            GD.Print(
                $"MainEditor: validación completada. " +
                $"0 errores, {warningCount} advertencias."
            );


            return;
        }


        validationStatusLabel.Text =
            "✓ Proyecto válido · 0 errores · 0 advertencias";


        GD.Print(
            "MainEditor: validación completada. Proyecto válido."
        );
    }


    private void UpdateValidationStatus(
        ValidationResult result)
    {
        if (validationStatusLabel == null)
        {
            return;
        }


        if (result == null)
        {
            UpdateValidationStatus();

            return;
        }


        int errorCount =
            result.GetErrorCount();


        int warningCount =
            result.GetWarningCount();


        if (errorCount > 0)
        {
            validationStatusLabel.Text =
                $"✕ Proyecto con errores · " +
                $"{errorCount} errores · " +
                $"{warningCount} advertencias";

            return;
        }


        if (warningCount > 0)
        {
            validationStatusLabel.Text =
                $"⚠ Proyecto válido con advertencias · " +
                $"{warningCount} advertencias";

            return;
        }


        validationStatusLabel.Text =
            "✓ Proyecto válido · 0 errores · 0 advertencias";
    }


    private void RequestNavigation(
        Action navigationAction)
    {
        if (navigationAction == null)
        {
            return;
        }


        if (!CurrentViewHasUnsavedChanges())
        {
            navigationAction.Invoke();

            return;
        }


        pendingNavigationAction =
            navigationAction;


        ShowUnsavedChangesDialog();
    }


    private bool CurrentViewHasUnsavedChanges()
    {
        if (currentView is EventEditor eventEditor)
        {
            return eventEditor.HasUnsavedChanges();
        }


        if (currentView is InterludeEditor interludeEditor)
        {
            return interludeEditor.HasUnsavedChanges();
        }


        if (currentView is ChapterEditor chapterEditorView)
        {
            return chapterEditorView.HasUnsavedChanges();
        }


        if (currentView is ChapterPageEditor chapterPageEditorView)
        {
            bool pageChanges =
                chapterPageEditorView.HasUnsavedChanges();


            bool chapterChanges =
                chapterEditor != null &&
                chapterEditor.HasUnsavedChanges();


            return pageChanges ||
                   chapterChanges;
        }


        if (currentView is AttributeEditor attributeEditorView)
        {
            return attributeEditorView.HasUnsavedChanges();
        }


        if (currentView is CharacterEditor characterEditorView)
        {
            return characterEditorView.HasUnsavedChanges();
        }


        if (currentView is RelationshipEditor relationshipEditorView)
        {
            return relationshipEditorView.HasUnsavedChanges();
        }


        return false;
    }


    private string GetCurrentEditorName()
    {
        if (currentView is EventEditor)
        {
            return "el evento actual";
        }


        if (currentView is InterludeEditor)
        {
            return "el interludio actual";
        }


        if (currentView is ChapterEditor ||
            currentView is ChapterPageEditor)
        {
            return "el capítulo actual";
        }


        if (currentView is AttributeEditor)
        {
            return "el atributo actual";
        }


        if (currentView is CharacterEditor)
        {
            return "el personaje actual";
        }


        if (currentView is RelationshipEditor)
        {
            return "la relación actual";
        }


        return "el elemento actual";
    }


    private void ShowUnsavedChangesDialog()
    {
        if (navigationDialog != null)
        {
            navigationDialog.QueueFree();

            navigationDialog = null;
        }


        navigationDialog =
            new ConfirmationDialog();


        navigationDialog.Title =
            "Cambios sin guardar";


        navigationDialog.DialogText =
            $"Hay cambios sin guardar en {GetCurrentEditorName()}.\n\n" +
            "¿Quieres guardarlos antes de continuar?";


        navigationDialog.OkButtonText =
            "Guardar";


        navigationDialog.CancelButtonText =
            "Cancelar";


        navigationDialog.AddButton(
            "No guardar",
            false,
            "discard"
        );


        navigationDialog.Confirmed +=
            OnNavigationSaveConfirmed;


        navigationDialog.Canceled +=
            OnNavigationCancelled;


        navigationDialog.CustomAction +=
            OnNavigationCustomAction;


        AddChild(
            navigationDialog
        );


        navigationDialog.PopupCentered();
    }


    private void OnNavigationSaveConfirmed()
    {
        if (navigationDialog != null)
        {
            navigationDialog.QueueFree();

            navigationDialog = null;
        }


        SaveCurrentEditorForNavigation();
    }


    private void OnNavigationCustomAction(
        StringName action)
    {
        if (action != "discard")
        {
            return;
        }


        if (navigationDialog != null)
        {
            navigationDialog.QueueFree();

            navigationDialog = null;
        }


        Action actionToExecute =
            pendingNavigationAction;


        pendingNavigationAction =
            null;


        CleanupCurrentEditorWithoutSaving();


        actionToExecute?.Invoke();
    }


    private void OnNavigationCancelled()
    {
        if (navigationDialog != null)
        {
            navigationDialog.QueueFree();

            navigationDialog = null;
        }


        pendingNavigationAction =
            null;
    }


    private void SaveCurrentEditorForNavigation()
    {
        keepEditorOpenAfterGlobalSave =
            false;


        if (currentView is EventEditor eventEditor)
        {
            eventEditor.SaveCurrentEvent();

            return;
        }


        if (currentView is InterludeEditor interludeEditor)
        {
            interludeEditor.SaveCurrentInterlude();

            return;
        }


        if (currentView is ChapterEditor chapterEditorView)
        {
            chapterEditorView.SaveCurrentChapter();

            return;
        }


        if (currentView is ChapterPageEditor)
        {
            SaveChapterFromPageEditor();

            return;
        }


        if (currentView is AttributeEditor attributeEditorView)
        {
            attributeEditorView.SaveCurrentAttribute();

            return;
        }


        if (currentView is CharacterEditor characterEditorView)
        {
            characterEditorView.SaveCurrentCharacter();

            return;
        }


        if (currentView is RelationshipEditor relationshipEditorView)
        {
            relationshipEditorView.SaveCurrentRelationship();

            return;
        }
    }


    private void CompletePendingNavigation()
    {
        Action action =
            pendingNavigationAction;


        pendingNavigationAction =
            null;


        CleanupCurrentEditorAfterNavigation();


        action?.Invoke();
    }


    private void CleanupCurrentEditorWithoutSaving()
    {
        if (currentView is ChapterPageEditor)
        {
            if (chapterPageEditor != null)
            {
                chapterPageEditor.QueueFree();

                chapterPageEditor =
                    null;
            }


            if (chapterEditor != null)
            {
                chapterEditor.QueueFree();

                chapterEditor =
                    null;
            }


            currentView =
                null;

            return;
        }
    }


    private void CleanupCurrentEditorAfterNavigation()
    {
        if (currentView is ChapterPageEditor)
        {
            if (chapterPageEditor != null)
            {
                chapterPageEditor.QueueFree();

                chapterPageEditor =
                    null;
            }


            if (chapterEditor != null)
            {
                chapterEditor.QueueFree();

                chapterEditor =
                    null;
            }


            currentView =
                null;
        }
    }


    private void OnEventsPressed()
    {
        RequestNavigation(
            () =>
            {
                returnToChapterEditor =
                    false;


                chapterEditorId =
                    "";


                ShowEventList();
            }
        );
    }


    private void ShowEventList()
    {
        EventListView view =
            eventListViewScene.Instantiate<EventListView>();


        eventListView =
            view;


        eventListView.EventSelected +=
            OnEventSelected;


        ShowView(
            view
        );


        if (
            projectManager == null ||
            !projectManager.HasProject())
        {
            GD.PrintErr(
                "MainEditor: no hay ningún proyecto cargado."
            );


            return;
        }


        view.SetRepository(
            eventRepository
        );


        UpdateValidationStatus();
    }


    private void OnInterludesPressed()
    {
        RequestNavigation(
            () =>
            {
                returnToChapterEditor =
                    false;


                chapterEditorId =
                    "";


                ShowInterludeList();
            }
        );
    }


    private void ShowInterludeList()
    {
        if (interludeListViewScene == null)
        {
            GD.PrintErr(
                "MainEditor: InterludeListView.tscn no está disponible."
            );


            return;
        }


        InterludeListView view =
            interludeListViewScene.Instantiate<InterludeListView>();


        interludeListView =
            view;


        interludeListView.InterludeSelected +=
            OnInterludeSelected;


        ShowView(
            view
        );


        if (
            projectManager == null ||
            !projectManager.HasProject())
        {
            GD.PrintErr(
                "MainEditor: no hay ningún proyecto cargado."
            );


            return;
        }


        view.SetRepository(
            interludeRepository
        );
    }


    private void OnInterludeSelected(
        string interludeId)
    {
        if (interludeRepository == null)
        {
            GD.PrintErr(
                "MainEditor: InterludeRepository no está disponible."
            );


            return;
        }


        if (interludeEditorScene == null)
        {
            GD.PrintErr(
                "MainEditor: InterludeEditor.tscn no está disponible."
            );


            return;
        }


        InterludeEditor editor =
            interludeEditorScene.Instantiate<InterludeEditor>();


        editor.InterludeSaved +=
            OnInterludeEditorSaved;


        editor.InterludeCancelled +=
            OnInterludeEditorCancelled;


        editor.ApplicationCloseConfirmed +=
            OnApplicationCloseConfirmed;


        ShowView(
            editor
        );


        editor.SetRepository(
            interludeRepository
        );


        editor.SetEventRepository(
            eventRepository
        );


        editor.SetProjectPath(
            projectManager.GetProjectPath()
        );


        if (string.IsNullOrWhiteSpace(
            interludeId))
        {
            editor.CreateNewInterlude();
        }
        else
        {
            editor.LoadInterlude(
                interludeId
            );
        }
    }


    private void OnInterludeEditorSaved()
    {
        GD.Print(
            "MainEditor: interludio guardado. Volviendo a la lista."
        );


        UpdateValidationStatus();


        if (keepEditorOpenAfterGlobalSave)
        {
            keepEditorOpenAfterGlobalSave =
                false;

            return;
        }


        if (pendingNavigationAction != null)
        {
            CompletePendingNavigation();

            return;
        }


        if (returnToChapterEditor)
        {
            string targetChapterId =
                chapterEditorId;


            returnToChapterEditor =
                false;


            chapterEditorId =
                "";


            ShowChapterEditor(
                targetChapterId
            );


            return;
        }


        ShowInterludeList();
    }


    private void OnInterludeEditorCancelled()
    {
        GD.Print(
            "MainEditor: edición de interludio cancelada."
        );


        if (returnToChapterEditor)
        {
            string targetChapterId =
                chapterEditorId;


            returnToChapterEditor =
                false;


            chapterEditorId =
                "";


            ShowChapterEditor(
                targetChapterId
            );


            return;
        }


        ShowInterludeList();
    }


    private void OnChaptersPressed()
    {
        RequestNavigation(
            () =>
            {
                returnToChapterEditor =
                    false;


                chapterEditorId =
                    "";


                ShowChapterList();
            }
        );
    }


    private void ShowChapterList()
    {
        if (chaptersListViewScene == null)
        {
            GD.PrintErr(
                "MainEditor: ChaptersListView.tscn no está disponible."
            );


            return;
        }


        if (
            projectManager == null ||
            !projectManager.HasProject())
        {
            GD.PrintErr(
                "MainEditor: no hay ningún proyecto cargado."
            );


            return;
        }


        ChaptersListView view =
            chaptersListViewScene.Instantiate<ChaptersListView>();


        chaptersListView =
            view;


        chaptersListView.ChapterSelected +=
            OnChapterSelected;


        ShowView(
            view
        );


        view.SetProjectPath(
            projectManager.GetProjectPath()
        );
    }


    private void OnChapterSelected(
        string chapterId)
    {
        ShowChapterEditor(
            chapterId
        );
    }


    private void ShowChapterEditor(
        string chapterId)
    {
        if (chapterEditorScene == null)
        {
            GD.PrintErr(
                "MainEditor: ChapterEditor.tscn no está disponible."
            );


            return;
        }


        if (
            projectManager == null ||
            !projectManager.HasProject())
        {
            GD.PrintErr(
                "MainEditor: no hay ningún proyecto cargado."
            );


            return;
        }


        ChapterEditor editor =
            chapterEditorScene.Instantiate<ChapterEditor>();


        chapterEditor =
            editor;


        editor.ChapterSaved +=
            OnChapterEditorSaved;


        editor.ChapterCancelled +=
            OnChapterEditorCancelled;


        editor.ChapterDeleted +=
            OnChapterDeleted;


        editor.EventSelected +=
            OnChapterEditorEventSelected;


        editor.InterludeSelected +=
            OnChapterEditorInterludeSelected;


        editor.ChapterPagesSelected +=
            OnChapterPagesSelected;


        ShowView(
            editor
        );


        editor.SetProjectPath(
            projectManager.GetProjectPath()
        );


        if (string.IsNullOrWhiteSpace(
            chapterId))
        {
            editor.CreateNewChapter();
        }
        else
        {
            editor.LoadChapter(
                chapterId
            );
        }
    }


    private void OnChapterPagesSelected(
        string chapterId)
    {
        if (chapterEditor == null)
        {
            GD.PrintErr(
                "MainEditor: ChapterEditor no está disponible."
            );


            return;
        }


        if (chapterPageEditorScene == null)
        {
            GD.PrintErr(
                "MainEditor: ChapterPageEditor.tscn no está disponible."
            );


            return;
        }


        ChapterPageEditor pageEditor =
            chapterPageEditorScene.Instantiate<ChapterPageEditor>();


        chapterPageEditor =
            pageEditor;


        pageEditor.PagesSaved +=
            OnChapterPagesSaved;


        pageEditor.PagesCancelled +=
            OnChapterPagesCancelled;


        pageEditor.ApplicationCloseConfirmed +=
            OnApplicationCloseConfirmed;


        chapterEditor.Hide();


        workspaceContent.AddChild(
            pageEditor
        );


        pageEditor.SetAnchorsAndOffsetsPreset(
            LayoutPreset.FullRect
        );


        currentView =
            pageEditor;


        pageEditor.SetPages(
            chapterEditor.GetPages()
        );
    }


    private void OnChapterPagesSaved()
    {
        if (
            chapterPageEditor == null ||
            chapterEditor == null)
        {
            return;
        }


        chapterEditor.SetPages(
            chapterPageEditor.GetPages()
        );


        chapterPageEditor.QueueFree();


        chapterPageEditor =
            null;


        currentView =
            chapterEditor;


        chapterEditor.Show();
    }


    private void OnChapterPagesCancelled()
    {
        if (chapterPageEditor == null)
        {
            return;
        }


        chapterPageEditor.QueueFree();


        chapterPageEditor =
            null;


        currentView =
            chapterEditor;


        chapterEditor.Show();
    }


    private void OnChapterEditorEventSelected(
        string eventId)
    {
        if (chapterEditor == null)
        {
            GD.PrintErr(
                "MainEditor: ChapterEditor no está disponible."
            );


            return;
        }


        string targetChapterId =
            chapterEditor.GetChapterId();


        RequestNavigation(
            () =>
            {
                returnToChapterEditor =
                    true;


                chapterEditorId =
                    targetChapterId;


                OnEventSelected(
                    eventId
                );
            }
        );
    }


    private void OnChapterEditorInterludeSelected(
        string interludeId)
    {
        if (chapterEditor == null)
        {
            GD.PrintErr(
                "MainEditor: ChapterEditor no está disponible."
            );


            return;
        }


        string targetChapterId =
            chapterEditor.GetChapterId();


        RequestNavigation(
            () =>
            {
                returnToChapterEditor =
                    true;


                chapterEditorId =
                    targetChapterId;


                OnInterludeSelected(
                    interludeId
                );
            }
        );
    }


    private void OnChapterEditorSaved()
    {
        if (keepEditorOpenAfterGlobalSave)
        {
            keepEditorOpenAfterGlobalSave =
                false;

            UpdateValidationStatus();

            return;
        }


        returnToChapterEditor =
            false;


        chapterEditorId =
            "";


        UpdateValidationStatus();


        if (pendingNavigationAction != null)
        {
            CompletePendingNavigation();

            return;
        }


        chapterEditor =
            null;


        ShowChapterList();
    }


    private void OnChapterEditorCancelled()
    {
        returnToChapterEditor =
            false;


        chapterEditorId =
            "";


        chapterEditor =
            null;


        ShowChapterList();
    }


    private void OnChapterDeleted()
    {
        returnToChapterEditor =
            false;


        chapterEditorId =
            "";


        chapterEditor =
            null;


        ShowChapterList();
    }


    private void OnAttributesPressed()
    {
        RequestNavigation(
            () =>
            {
                returnToChapterEditor =
                    false;


                chapterEditorId =
                    "";


                ShowAttributeList();
            }
        );
    }


    private void ShowAttributeList()
    {
        if (attributeListViewScene == null)
        {
            GD.PrintErr(
                "MainEditor: AttributeListView.tscn no está disponible."
            );


            return;
        }


        if (
            projectManager == null ||
            !projectManager.HasProject())
        {
            GD.PrintErr(
                "MainEditor: no hay ningún proyecto cargado."
            );


            return;
        }


        AttributeListView view =
            attributeListViewScene.Instantiate<AttributeListView>();


        attributeListView =
            view;


        attributeListView.AttributeSelected +=
            OnAttributeSelected;


        ShowView(
            view
        );


        view.SetProjectPath(
            projectManager.GetProjectPath()
        );
    }


    private void OnAttributeSelected(
        string attributeId)
    {
        ShowAttributeEditor(
            attributeId
        );
    }


    private void ShowAttributeEditor(
        string attributeId)
    {
        if (attributeEditorScene == null)
        {
            GD.PrintErr(
                "MainEditor: AttributeEditor.tscn no está disponible."
            );


            return;
        }


        if (
            projectManager == null ||
            !projectManager.HasProject())
        {
            GD.PrintErr(
                "MainEditor: no hay ningún proyecto cargado."
            );


            return;
        }


        AttributeEditor editor =
            attributeEditorScene.Instantiate<AttributeEditor>();


        attributeEditor =
            editor;


        editor.AttributeSaved +=
            OnAttributeEditorSaved;


        editor.AttributeCancelled +=
            OnAttributeEditorCancelled;


        editor.AttributeDeleted +=
            OnAttributeEditorDeleted;


        editor.ApplicationCloseConfirmed +=
            OnApplicationCloseConfirmed;


        ShowView(
            editor
        );


        editor.SetProjectPath(
            projectManager.GetProjectPath()
        );


        if (string.IsNullOrWhiteSpace(
            attributeId))
        {
            editor.CreateNewAttribute();
        }
        else
        {
            editor.LoadAttribute(
                attributeId
            );
        }
    }


    private void OnAttributeEditorSaved()
    {
        if (keepEditorOpenAfterGlobalSave)
        {
            keepEditorOpenAfterGlobalSave =
                false;

            UpdateValidationStatus();

            return;
        }


        attributeEditor =
            null;


        UpdateValidationStatus();


        if (pendingNavigationAction != null)
        {
            CompletePendingNavigation();

            return;
        }


        ShowAttributeList();
    }


    private void OnAttributeEditorCancelled()
    {
        attributeEditor =
            null;


        ShowAttributeList();
    }


    private void OnAttributeEditorDeleted()
    {
        attributeEditor =
            null;


        UpdateValidationStatus();


        ShowAttributeList();
    }


    private void OnCharactersPressed()
    {
        RequestNavigation(
            () =>
            {
                returnToChapterEditor =
                    false;


                chapterEditorId =
                    "";


                ShowCharacterList();
            }
        );
    }


    private void ShowCharacterList()
    {
        if (characterListViewScene == null)
        {
            GD.PrintErr(
                "MainEditor: CharacterListView.tscn no está disponible."
            );


            return;
        }


        CharacterListView view =
            characterListViewScene.Instantiate<CharacterListView>();


        characterListView =
            view;


        characterListView.CharacterSelected +=
            OnCharacterSelected;


        ShowView(
            view
        );


        if (
            projectManager == null ||
            !projectManager.HasProject())
        {
            GD.PrintErr(
                "MainEditor: no hay ningún proyecto cargado."
            );


            return;
        }


        view.SetProjectPath(
            projectManager.GetProjectPath()
        );
    }


    private void OnCharacterSelected(
        string characterId)
    {
        ShowCharacterEditor(
            characterId
        );
    }


    private void ShowCharacterEditor(
        string characterId)
    {
        if (characterEditorScene == null)
        {
            GD.PrintErr(
                "MainEditor: CharacterEditor.tscn no está disponible."
            );


            return;
        }


        CharacterEditor editor =
            characterEditorScene.Instantiate<CharacterEditor>();


        characterEditor =
            editor;


        editor.CharacterSaved +=
            OnCharacterEditorSaved;


        editor.CharacterCancelled +=
            OnCharacterEditorCancelled;


        ShowView(
            editor
        );


        if (
            projectManager == null ||
            !projectManager.HasProject())
        {
            GD.PrintErr(
                "MainEditor: no hay ningún proyecto cargado."
            );


            return;
        }


        editor.SetProjectPath(
            projectManager.GetProjectPath()
        );


        if (string.IsNullOrWhiteSpace(
            characterId))
        {
            editor.CreateNewCharacter();
        }
        else
        {
            editor.LoadCharacter(
                characterId
            );
        }
    }


    private void OnCharacterEditorSaved()
    {
        if (keepEditorOpenAfterGlobalSave)
        {
            keepEditorOpenAfterGlobalSave =
                false;

            UpdateValidationStatus();

            return;
        }


        characterEditor =
            null;


        UpdateValidationStatus();


        if (pendingNavigationAction != null)
        {
            CompletePendingNavigation();

            return;
        }


        ShowCharacterList();
    }


    private void OnCharacterEditorCancelled()
    {
        characterEditor =
            null;


        ShowCharacterList();
    }


    private void OnRelationshipsPressed()
    {
        RequestNavigation(
            () =>
            {
                returnToChapterEditor =
                    false;


                chapterEditorId =
                    "";


                ShowRelationshipList();
            }
        );
    }


    private void ShowRelationshipList()
    {
        if (relationshipListViewScene == null)
        {
            GD.PrintErr(
                "MainEditor: RelationshipListView.tscn no está disponible."
            );


            return;
        }


        RelationshipListView view =
            relationshipListViewScene.Instantiate<RelationshipListView>();


        relationshipListView =
            view;


        relationshipListView.RelationshipSelected +=
            OnRelationshipSelected;


        ShowView(
            view
        );


        if (
            projectManager == null ||
            !projectManager.HasProject())
        {
            GD.PrintErr(
                "MainEditor: no hay ningún proyecto cargado."
            );


            return;
        }


        view.SetProjectPath(
            projectManager.GetProjectPath()
        );
    }


    private void OnRelationshipSelected(
        string characterId)
    {
        ShowRelationshipEditor(
            characterId
        );
    }


    private void ShowRelationshipEditor(
        string characterId)
    {
        if (relationshipEditorScene == null)
        {
            GD.PrintErr(
                "MainEditor: RelationshipEditor.tscn no está disponible."
            );


            return;
        }


        RelationshipEditor editor =
            relationshipEditorScene.Instantiate<RelationshipEditor>();


        relationshipEditor =
            editor;


        editor.RelationshipSaved +=
            OnRelationshipEditorSaved;


        editor.RelationshipCancelled +=
            OnRelationshipEditorCancelled;


        ShowView(
            editor
        );


        if (
            projectManager == null ||
            !projectManager.HasProject())
        {
            GD.PrintErr(
                "MainEditor: no hay ningún proyecto cargado."
            );


            return;
        }


        editor.SetProjectPath(
            projectManager.GetProjectPath()
        );


        if (string.IsNullOrWhiteSpace(
            characterId))
        {
            editor.CreateNewRelationship();
        }
        else
        {
            editor.LoadRelationship(
                characterId
            );
        }
    }


    private void OnRelationshipEditorSaved()
    {
        if (keepEditorOpenAfterGlobalSave)
        {
            keepEditorOpenAfterGlobalSave =
                false;

            UpdateValidationStatus();

            return;
        }


        relationshipEditor =
            null;


        UpdateValidationStatus();


        if (pendingNavigationAction != null)
        {
            CompletePendingNavigation();

            return;
        }


        ShowRelationshipList();
    }


    private void OnRelationshipEditorCancelled()
    {
        relationshipEditor =
            null;


        ShowRelationshipList();
    }


    private void OnValidationDetailsPressed()
    {
        RequestNavigation(
            () =>
            {
                returnToChapterEditor =
                    false;


                chapterEditorId =
                    "";


                OpenValidationPanel();
            }
        );
    }


    private void OpenValidationPanel()
    {
        if (validationPanelScene == null)
        {
            GD.PrintErr(
                "MainEditor: ValidationPanel.tscn no está disponible."
            );


            return;
        }


        if (
            projectManager == null ||
            !projectManager.HasProject())
        {
            GD.PrintErr(
                "MainEditor: no hay ningún proyecto cargado."
            );


            return;
        }


        ValidationPanel panel =
            validationPanelScene.Instantiate<ValidationPanel>();


        validationPanel =
            panel;


        validationPanel.Closed +=
            OnValidationPanelClosed;


        validationPanel.IssueSelected +=
            OnValidationIssueSelected;


        ShowView(
            panel
        );


        panel.SetRepository(
            eventRepository
        );


        panel.SetInterludeRepository(
            interludeRepository
        );


        panel.SetProjectPath(
            projectManager.GetProjectPath()
        );


        panel.Validate();
    }


    private void OnValidationIssueSelected(
        int resourceTypeValue,
        string resourceId,
        string pageId,
        int decisionIndex,
        int conditionIndex,
        int effectIndex)
    {
        if (string.IsNullOrWhiteSpace(
            resourceId))
        {
            GD.PrintErr(
                "MainEditor: la incidencia no tiene un recurso asociado."
            );


            return;
        }


        ValidationResourceType resourceType =
            (ValidationResourceType)
            resourceTypeValue;


        switch (resourceType)
        {
            case ValidationResourceType.Event:

                OpenEventFromValidation(
                    resourceId,
                    pageId,
                    decisionIndex
                );

                break;


            case ValidationResourceType.Interlude:

                OpenInterludeFromValidation(
                    resourceId,
                    pageId,
                    conditionIndex,
                    effectIndex
                );

                break;


            default:

                GD.PrintErr(
                    "MainEditor: tipo de recurso de validación desconocido: ",
                    resourceTypeValue
                );

                break;
        }
    }


    private void OpenEventFromValidation(
        string eventId,
        string pageId,
        int decisionIndex)
    {
        GD.Print(
            "MainEditor: incidencia seleccionada. Abriendo evento: ",
            eventId
        );


        EventEditor eventEditor =
            eventEditorScene.Instantiate<EventEditor>();


        eventEditor.EventSaved +=
            OnEventEditorSaved;


        eventEditor.EventCancelled +=
            OnEventEditorCancelled;


        eventEditor.ApplicationCloseConfirmed +=
            OnApplicationCloseConfirmed;


        ShowView(
            eventEditor
        );


        eventEditor.SetRepository(
            eventRepository
        );


        eventEditor.SetProjectPath(
            projectManager.GetProjectPath()
        );


        eventEditor.SetValidationTarget(
            pageId,
            decisionIndex
        );


        eventEditor.LoadEvent(
            eventId
        );
    }


    private void OpenInterludeFromValidation(
        string interludeId,
        string pageId,
        int conditionIndex,
        int effectIndex)
    {
        GD.Print(
            "MainEditor: incidencia seleccionada. Abriendo interludio: ",
            interludeId
        );


        if (interludeEditorScene == null)
        {
            GD.PrintErr(
                "MainEditor: InterludeEditor.tscn no está disponible."
            );


            return;
        }


        InterludeEditor interludeEditor =
            interludeEditorScene.Instantiate<InterludeEditor>();


        interludeEditor.InterludeSaved +=
            OnInterludeEditorSaved;


        interludeEditor.InterludeCancelled +=
            OnInterludeEditorCancelled;


        interludeEditor.ApplicationCloseConfirmed +=
            OnApplicationCloseConfirmed;


        ShowView(
            interludeEditor
        );


        interludeEditor.SetRepository(
            interludeRepository
        );


        interludeEditor.SetEventRepository(
            eventRepository
        );


        interludeEditor.SetProjectPath(
            projectManager.GetProjectPath()
        );


        interludeEditor.SetValidationTarget(
            pageId,
            conditionIndex,
            effectIndex
        );


        interludeEditor.LoadInterlude(
            interludeId
        );
    }


    private void OnValidationPanelClosed()
    {
        validationPanel =
            null;


        UpdateValidationStatus();


        ShowEventList();
    }


    private void OnEventSelected(
        string eventId)
    {
        GD.Print(
            "MainEditor recibió evento: ",
            string.IsNullOrWhiteSpace(eventId)
                ? "<nuevo evento>"
                : eventId
        );


        if (eventRepository == null)
        {
            GD.PrintErr(
                "MainEditor: EventRepository no está disponible."
            );


            return;
        }


        EventEditor eventEditor =
            eventEditorScene.Instantiate<EventEditor>();


        eventEditor.EventSaved +=
            OnEventEditorSaved;


        eventEditor.EventCancelled +=
            OnEventEditorCancelled;


        eventEditor.ApplicationCloseConfirmed +=
            OnApplicationCloseConfirmed;


        ShowView(
            eventEditor
        );


        eventEditor.SetRepository(
            eventRepository
        );


        eventEditor.SetProjectPath(
            projectManager.GetProjectPath()
        );


        if (string.IsNullOrWhiteSpace(
            eventId))
        {
            eventEditor.CreateNewEvent();
        }
        else
        {
            eventEditor.LoadEvent(
                eventId
            );
        }
    }


    private void OnEventEditorSaved()
    {
        GD.Print(
            "MainEditor: evento guardado. Volviendo a la lista."
        );


        UpdateValidationStatus();


        if (keepEditorOpenAfterGlobalSave)
        {
            keepEditorOpenAfterGlobalSave =
                false;

            return;
        }


        if (pendingNavigationAction != null)
        {
            CompletePendingNavigation();

            return;
        }


        if (returnToChapterEditor)
        {
            string targetChapterId =
                chapterEditorId;


            returnToChapterEditor =
                false;


            chapterEditorId =
                "";


            ShowChapterEditor(
                targetChapterId
            );


            return;
        }


        ShowEventList();
    }


    private void OnEventEditorCancelled()
    {
        GD.Print(
            "MainEditor: edición cancelada. Volviendo a la lista."
        );


        if (returnToChapterEditor)
        {
            string targetChapterId =
                chapterEditorId;


            returnToChapterEditor =
                false;


            chapterEditorId =
                "";


            ShowChapterEditor(
                targetChapterId
            );


            return;
        }


        ShowEventList();
    }


    private void OnSettingsPressed()
    {
        if (settingsDialog == null)
        {
            settingsDialog =
                settingsDialogScene.Instantiate<SettingsDialog>();


            AddChild(
                settingsDialog
            );
        }


        settingsDialog.PopupCentered();
    }


    private void OnExitPressed()
    {
        RequestApplicationClose();
    }


    public void RequestApplicationClose()
    {
        EventEditor eventEditor =
            currentView as EventEditor;


        if (eventEditor != null)
        {
            eventEditor.RequestApplicationClose();

            return;
        }


        InterludeEditor interludeEditor =
            currentView as InterludeEditor;


        if (interludeEditor != null)
        {
            interludeEditor.RequestApplicationClose();

            return;
        }


        ChapterPageEditor chapterPageEditorView =
            currentView as ChapterPageEditor;


        if (chapterPageEditorView != null)
        {
            chapterPageEditorView.RequestApplicationClose();

            return;
        }


        ChapterEditor chapterEditorView =
            currentView as ChapterEditor;


        if (chapterEditorView != null)
        {
            chapterEditorView.RequestApplicationClose();

            return;
        }


        AttributeEditor attributeEditorView =
            currentView as AttributeEditor;


        if (attributeEditorView != null)
        {
            attributeEditorView.RequestApplicationClose();

            return;
        }


        CharacterEditor characterEditorView =
            currentView as CharacterEditor;


        if (characterEditorView != null)
        {
            characterEditorView.RequestApplicationClose();

            return;
        }


        RelationshipEditor relationshipEditorView =
            currentView as RelationshipEditor;


        if (relationshipEditorView != null)
        {
            relationshipEditorView.RequestApplicationClose();

            return;
        }


        OnApplicationCloseConfirmed();
    }


    private void OnApplicationCloseConfirmed()
    {
        Main main =
            GetParent<Main>();


        if (main == null)
        {
            GD.PrintErr(
                "MainEditor: no se encontró el nodo Main."
            );


            return;
        }


        main.ConfirmApplicationClose();
    }


    private void ShowView(
        Control view)
    {
        if (view == null)
        {
            GD.PrintErr(
                "MainEditor: la vista es nula."
            );


            return;
        }


        if (currentView != null)
        {
            currentView.QueueFree();


            currentView =
                null;
        }


        currentView =
            view;


        workspaceContent.AddChild(
            currentView
        );


        currentView.SetAnchorsAndOffsetsPreset(
            LayoutPreset.FullRect
        );


        if (emptyWorkspace != null)
        {
            emptyWorkspace.Hide();
        }
    }
}