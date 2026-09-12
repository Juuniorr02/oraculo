using Godot;
using System.Collections.Generic;

public partial class RelationshipEditor : Control
{
    [Signal]
    public delegate void RelationshipSavedEventHandler();

    [Signal]
    public delegate void RelationshipCancelledEventHandler();


    private OptionButton characterOption;
    private SpinBox initialValueSpinBox;
    private LineEdit titleLineEdit;
    private ItemList titlesList;

    private Button addTitleButton;
    private Button removeTitleButton;
    private Button saveButton;
    private Button cancelButton;


    private CharacterDatabase characterDatabase;
    private RelationshipDatabase relationshipDatabase;


    private string projectPath = "";
    private string editingCharacterId = "";


    private EditorRelationshipDefinitionData relationshipData;

    private UnsavedChangesGuard<EditorRelationshipDefinitionData> unsavedChangesGuard;


    public override void _Ready()
    {
        characterOption =
            GetNode<OptionButton>(
                "MarginContainer/VBoxContainer/CharacterSection/CharacterOption"
            );

        initialValueSpinBox =
            GetNode<SpinBox>(
                "MarginContainer/VBoxContainer/InitialValueSection/InitialValueSpinBox"
            );

        titleLineEdit =
            GetNode<LineEdit>(
                "MarginContainer/VBoxContainer/TitlesSection/AddTitleContainer/TitleLineEdit"
            );

        titlesList =
            GetNode<ItemList>(
                "MarginContainer/VBoxContainer/TitlesSection/TitlesList"
            );

        addTitleButton =
            GetNode<Button>(
                "MarginContainer/VBoxContainer/TitlesSection/AddTitleContainer/AddTitleButton"
            );

        removeTitleButton =
            GetNode<Button>(
                "MarginContainer/VBoxContainer/TitlesSection/RemoveTitleButton"
            );

        saveButton =
            GetNode<Button>(
                "MarginContainer/VBoxContainer/Buttons/SaveButton"
            );

        cancelButton =
            GetNode<Button>(
                "MarginContainer/VBoxContainer/Buttons/CancelButton"
            );


        initialValueSpinBox.MinValue = -10;
        initialValueSpinBox.MaxValue = 10;
        initialValueSpinBox.Step = 1;


        characterOption.ItemSelected +=
            OnCharacterSelected;

        addTitleButton.Pressed +=
            OnAddTitlePressed;

        removeTitleButton.Pressed +=
            OnRemoveTitlePressed;

        saveButton.Pressed +=
            OnSavePressed;

        cancelButton.Pressed +=
            OnCancelPressed;


        unsavedChangesGuard =
            new UnsavedChangesGuard<EditorRelationshipDefinitionData>(
                this,
                GetCurrentRelationshipState,
                CloseEditor
            );


        GD.Print(
            "RelationshipEditor iniciado."
        );
    }


    public void SetProjectPath(
        string path)
    {
        projectPath = path;


        if (string.IsNullOrWhiteSpace(
            projectPath))
        {
            GD.PrintErr(
                "RelationshipEditor: la ruta del proyecto está vacía."
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


        PopulateCharacters();
    }


    private void PopulateCharacters()
    {
        characterOption.Clear();


        if (characterDatabase == null)
        {
            return;
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


        characterOption.Select(-1);
    }


    public void CreateNewRelationship()
    {
        editingCharacterId = "";


        relationshipData =
            new EditorRelationshipDefinitionData();


        characterOption.Disabled = false;
        initialValueSpinBox.Editable = true;


        characterOption.Select(-1);


        initialValueSpinBox.Value = 0;


        ClearTitles();


        SaveOriginalState();
    }


    public void LoadRelationship(
        string characterId)
    {
        if (relationshipDatabase == null)
        {
            GD.PrintErr(
                "RelationshipEditor: RelationshipDatabase no está disponible."
            );

            return;
        }


        if (string.IsNullOrWhiteSpace(
            characterId))
        {
            CreateNewRelationship();
            return;
        }


        EditorRelationshipDefinitionData loadedRelationship =
            relationshipDatabase.GetRelationship(
                characterId
            );


        if (loadedRelationship == null)
        {
            GD.PrintErr(
                "No se encontró la relación del personaje: ",
                characterId
            );

            CreateNewRelationship();
            return;
        }


        relationshipData =
            loadedRelationship;


        editingCharacterId =
            relationshipData.CharacterId;


        SelectCharacter(
            relationshipData.CharacterId
        );


        characterOption.Disabled = true;


        initialValueSpinBox.Value =
            relationshipData.InitialValue;


        PopulateTitles();


        SaveOriginalState();
    }


    private void SelectCharacter(
        string characterId)
    {
        for (
            int index = 0;
            index < characterOption.ItemCount;
            index++)
        {
            Variant metadata =
                characterOption.GetItemMetadata(
                    index
                );


            if (metadata.AsString() ==
                characterId)
            {
                characterOption.Select(
                    index
                );

                return;
            }
        }


        characterOption.Select(-1);
    }


    private void OnCharacterSelected(
        long index)
    {
        if (relationshipData == null)
        {
            return;
        }


        if (index < 0 ||
            index >= characterOption.ItemCount)
        {
            return;
        }


        Variant metadata =
            characterOption.GetItemMetadata(
                (int)index
            );


        string characterId =
            metadata.AsString();


        if (string.IsNullOrWhiteSpace(
            characterId))
        {
            return;
        }


        relationshipData.CharacterId =
            characterId;
    }


    private void OnAddTitlePressed()
    {
        string title =
            titleLineEdit.Text.Trim();


        if (string.IsNullOrWhiteSpace(
            title))
        {
            return;
        }


        if (relationshipData == null)
        {
            relationshipData =
                new EditorRelationshipDefinitionData();
        }


        if (relationshipData.Titles == null)
        {
            relationshipData.Titles =
                new List<string>();
        }


        if (relationshipData.Titles.Contains(
            title))
        {
            titleLineEdit.Clear();
            return;
        }


        relationshipData.Titles.Add(
            title
        );


        titlesList.AddItem(
            title
        );


        titleLineEdit.Clear();
        titleLineEdit.GrabFocus();
    }


    private void OnRemoveTitlePressed()
    {
        int selectedIndex =
            titlesList.GetSelectedItems()
                .Length > 0
                ? titlesList.GetSelectedItems()[0]
                : -1;


        if (selectedIndex < 0)
        {
            return;
        }


        if (relationshipData == null ||
            relationshipData.Titles == null)
        {
            return;
        }


        if (selectedIndex >=
            relationshipData.Titles.Count)
        {
            return;
        }


        relationshipData.Titles.RemoveAt(
            selectedIndex
        );


        titlesList.RemoveItem(
            selectedIndex
        );
    }


    private void PopulateTitles()
    {
        ClearTitles();


        if (relationshipData == null ||
            relationshipData.Titles == null)
        {
            return;
        }


        foreach (
            string title
            in relationshipData.Titles)
        {
            if (string.IsNullOrWhiteSpace(
                title))
            {
                continue;
            }


            titlesList.AddItem(
                title
            );
        }
    }


    private void ClearTitles()
    {
        titlesList.Clear();
    }


    private EditorRelationshipDefinitionData GetCurrentRelationshipState()
    {
        if (relationshipData == null)
        {
            relationshipData =
                new EditorRelationshipDefinitionData();
        }


        relationshipData.InitialValue =
            Mathf.Clamp(
                (int)initialValueSpinBox.Value,
                -10,
                10
            );


        if (relationshipData.Titles == null)
        {
            relationshipData.Titles =
                new List<string>();
        }


        return relationshipData;
    }


    private void SaveOriginalState()
    {
        if (unsavedChangesGuard == null)
        {
            return;
        }


        unsavedChangesGuard.SaveOriginalState(
            relationshipData
        );
    }


    private void OnSavePressed()
    {
        if (relationshipDatabase == null)
        {
            ShowError(
                "RelationshipDatabase no está disponible."
            );

            return;
        }


        GetCurrentRelationshipState();


        if (string.IsNullOrWhiteSpace(
            relationshipData.CharacterId))
        {
            ShowError(
                "Debes seleccionar un personaje."
            );

            return;
        }


        bool alreadyExists =
            relationshipDatabase.HasRelationship(
                relationshipData.CharacterId
            );


        if (alreadyExists &&
            relationshipData.CharacterId !=
            editingCharacterId)
        {
            ShowError(
                "Ese personaje ya tiene una relación definida."
            );

            return;
        }


        bool saved =
            relationshipDatabase.SaveRelationship(
                relationshipData
            );


        if (!saved)
        {
            ShowError(
                "No se pudo guardar la relación."
            );

            return;
        }


        editingCharacterId =
            relationshipData.CharacterId;


        SaveOriginalState();


        EmitSignal(
            SignalName.RelationshipSaved
        );
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


    private void CloseEditor()
    {
        EmitSignal(
            SignalName.RelationshipCancelled
        );
    }


    private void ShowError(
        string message)
    {
        AcceptDialog dialog =
            new AcceptDialog();

        dialog.Title =
            "Relaciones";

        dialog.DialogText =
            message;

        AddChild(
            dialog
        );

        dialog.PopupCentered();
    }
}