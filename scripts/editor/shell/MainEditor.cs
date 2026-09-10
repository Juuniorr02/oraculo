using Godot;

public partial class MainEditor : Control
{
    private Button eventsButton;
    private Button charactersButton;
    private Button relationshipsButton;
    private Button settingsButton;

    private Control workspaceContent;

    private PackedScene settingsDialogScene;
    private PackedScene eventListViewScene;
    private PackedScene eventEditorScene;
    private PackedScene characterListViewScene;
    private PackedScene characterEditorScene;
    private PackedScene relationshipListViewScene;
    private PackedScene relationshipEditorScene;

    private SettingsDialog settingsDialog;

    private Control currentView;

    private EventListView eventListView;
    private CharacterListView characterListView;
    private CharacterEditor characterEditor;
    private RelationshipListView relationshipListView;
    private RelationshipEditor relationshipEditor;

    private ProjectManager projectManager;
    private EventRepository eventRepository;


    public override void _Ready()
    {
        ApplyOraculoTheme();


        projectManager =
            GetNode<ProjectManager>(
                "../ProjectManager"
            );


        if (projectManager.HasProject())
        {
            eventRepository =
                new EventRepository(
                    projectManager.GetProjectPath()
                );
        }


        eventsButton =
            GetNode<Button>(
                "MainLayout/MainArea/SideBar/MarginContainer/VBoxContainer/Navigation/EventsButton"
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


        workspaceContent =
            GetNode<Control>(
                "MainLayout/MainArea/WorkSpace/MarginContainer/VBoxContainer/WorkspaceContent"
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


        eventsButton.Pressed +=
            OnEventsPressed;


        charactersButton.Pressed +=
            OnCharactersPressed;


        relationshipsButton.Pressed +=
            OnRelationshipsPressed;


        settingsButton.Pressed +=
            OnSettingsPressed;


        GD.Print(
            "MainEditor iniciado."
        );
    }


    private void ApplyOraculoTheme()
    {
        Theme theme =
            OraculoThemeBuilder.CreateTheme();

        Theme = theme;
    }


    private void OnEventsPressed()
    {
        ShowEventList();
    }


    private void ShowEventList()
    {
        EventListView view =
            eventListViewScene.Instantiate<EventListView>();


        eventListView = view;


        eventListView.EventSelected +=
            OnEventSelected;


        ShowView(
            view
        );


        if (projectManager == null ||
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


        characterListView = view;


        characterListView.CharacterSelected +=
            OnCharacterSelected;


        ShowView(
            view
        );


        if (projectManager == null ||
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


        characterEditor = editor;


        editor.CharacterSaved +=
            OnCharacterEditorSaved;


        editor.CharacterCancelled +=
            OnCharacterEditorCancelled;


        ShowView(
            editor
        );


        if (projectManager == null ||
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


        relationshipListView = view;


        relationshipListView.RelationshipSelected +=
            OnRelationshipSelected;


        ShowView(
            view
        );


        if (projectManager == null ||
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


        relationshipEditor = editor;


        editor.RelationshipSaved +=
            OnRelationshipEditorSaved;


        editor.RelationshipCancelled +=
            OnRelationshipEditorCancelled;


        ShowView(
            editor
        );


        if (projectManager == null ||
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


        currentView = view;


        workspaceContent.AddChild(
            currentView
        );


        currentView.SetAnchorsAndOffsetsPreset(
            LayoutPreset.FullRect
        );
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
}