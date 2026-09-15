using Godot;

public partial class MainEditor : Control
{
    private Button eventsButton;
    private Button interludesButton;
    private Button charactersButton;
    private Button relationshipsButton;
    private Button settingsButton;
    private Button exitButton;

    private Control workspaceContent;
    private Control emptyWorkspace;

    private PackedScene settingsDialogScene;
    private PackedScene eventListViewScene;
    private PackedScene eventEditorScene;
    private PackedScene interludeListViewScene;
    private PackedScene interludeEditorScene;
    private PackedScene characterListViewScene;
    private PackedScene characterEditorScene;
    private PackedScene relationshipListViewScene;
    private PackedScene relationshipEditorScene;
    private PackedScene validationPanelScene;

    private SettingsDialog settingsDialog;

    private Control currentView;

    private EventListView eventListView;
    private InterludeListView interludeListView;
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


        charactersButton.Pressed +=
            OnCharactersPressed;


        relationshipsButton.Pressed +=
            OnRelationshipsPressed;


        settingsButton.Pressed +=
            OnSettingsPressed;


        exitButton.Pressed +=
            OnExitPressed;


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


    private void UpdateValidationStatus()
    {
        if (validationStatusLabel == null)
        {
            return;
        }


        if (eventRepository == null)
        {
            validationStatusLabel.Text =
                "✕ No hay proyecto cargado";

            return;
        }


        ProjectValidator projectValidator =
            new ProjectValidator(
                eventRepository,
                interludeRepository
            );


        ValidationResult result =
            projectValidator.ValidateProject();


        int errorCount =
            result.Errors.Count;


        int warningCount =
            result.Warnings.Count;


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


    private void OnEventsPressed()
    {
        ShowEventList();
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
        ShowInterludeList();
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


        ShowInterludeList();
    }


    private void OnInterludeEditorCancelled()
    {
        GD.Print(
            "MainEditor: edición de interludio cancelada."
        );


        ShowInterludeList();
    }


    private void OnCharactersPressed()
    {
        ShowCharacterList();
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
        ShowCharacterList();
    }


    private void OnCharacterEditorCancelled()
    {
        ShowCharacterList();
    }


    private void OnRelationshipsPressed()
    {
        ShowRelationshipList();
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
        ShowRelationshipList();
    }


    private void OnRelationshipEditorCancelled()
    {
        ShowRelationshipList();
    }


    private void OnValidationDetailsPressed()
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


        ShowEventList();
    }


    private void OnEventEditorCancelled()
    {
        GD.Print(
            "MainEditor: edición cancelada. Volviendo a la lista."
        );


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
            currentView = null;
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