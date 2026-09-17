using Godot;

public partial class CharacterEditor : Control
{
    [Signal]
    public delegate void CharacterSavedEventHandler();

    [Signal]
    public delegate void CharacterCancelledEventHandler();


    private LineEdit nameLineEdit;
    private SpinBox ageSpinBox;
    private OptionButton genderOption;
    private CheckButton aliveCheckButton;

    private Label idValueLabel;

    private Button saveButton;
    private Button cancelButton;


    private CharacterDatabase characterDatabase;

    private string projectPath = "";
    private string editingCharacterId = "";

    private EditorCharacterData characterData;

    private UnsavedChangesGuard<EditorCharacterData> unsavedChangesGuard;


    public override void _Ready()
    {
        nameLineEdit =
            GetNode<LineEdit>(
                "MarginContainer/VBoxContainer/NameSection/NameLineEdit"
            );

        ageSpinBox =
            GetNode<SpinBox>(
                "MarginContainer/VBoxContainer/AgeSection/AgeSpinBox"
            );

        genderOption =
            GetNode<OptionButton>(
                "MarginContainer/VBoxContainer/GenderSection/GenderOption"
            );

        aliveCheckButton =
            GetNode<CheckButton>(
                "MarginContainer/VBoxContainer/AliveSection/AliveCheckButton"
            );

        idValueLabel =
            GetNode<Label>(
                "MarginContainer/VBoxContainer/IdSection/IdValueLabel"
            );

        saveButton =
            GetNode<Button>(
                "MarginContainer/VBoxContainer/Buttons/SaveButton"
            );

        cancelButton =
            GetNode<Button>(
                "MarginContainer/VBoxContainer/Buttons/CancelButton"
            );


        PopulateGenderOptions();


        ageSpinBox.MinValue = 0;
        ageSpinBox.MaxValue = 200;
        ageSpinBox.Step = 1;


        saveButton.Pressed +=
            OnSavePressed;

        cancelButton.Pressed +=
            OnCancelPressed;


        unsavedChangesGuard =
            new UnsavedChangesGuard<EditorCharacterData>(
                this,
                GetCurrentCharacterState,
                CloseEditor
            );


        GD.Print(
            "CharacterEditor iniciado."
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
                "CharacterEditor: la ruta del proyecto está vacía."
            );

            return;
        }


        characterDatabase =
            new CharacterDatabase(
                projectPath
            );
    }


    private void PopulateGenderOptions()
    {
        genderOption.Clear();


        genderOption.AddItem(
            "Sin especificar"
        );

        genderOption.AddItem(
            "Masculino"
        );

        genderOption.AddItem(
            "Femenino"
        );
    }


    public void CreateNewCharacter()
    {
        editingCharacterId = "";


        characterData =
            new EditorCharacterData();


        idValueLabel.Text =
            "Se generará automáticamente";


        nameLineEdit.Text = "";
        ageSpinBox.Value = 0;
        genderOption.Select(0);
        aliveCheckButton.ButtonPressed = true;


        SaveOriginalState();


        nameLineEdit.GrabFocus();
    }


    public void LoadCharacter(
        string characterId)
    {
        if (characterDatabase == null)
        {
            GD.PrintErr(
                "CharacterEditor: CharacterDatabase no está disponible."
            );

            return;
        }


        if (string.IsNullOrWhiteSpace(
            characterId))
        {
            CreateNewCharacter();
            return;
        }


        EditorCharacterData loadedCharacter =
            characterDatabase.GetCharacter(
                characterId
            );


        if (loadedCharacter == null)
        {
            GD.PrintErr(
                "No se encontró el personaje: ",
                characterId
            );

            CreateNewCharacter();
            return;
        }


        characterData =
            loadedCharacter;


        editingCharacterId =
            characterData.Id;


        idValueLabel.Text =
            characterData.Id;


        nameLineEdit.Text =
            characterData.Name;

        ageSpinBox.Value =
            characterData.Age;

        SelectGender(
            characterData.Gender
        );

        aliveCheckButton.ButtonPressed =
            characterData.IsAlive;


        SaveOriginalState();
    }


    private void SelectGender(
        string gender)
    {
        if (string.IsNullOrWhiteSpace(
            gender))
        {
            genderOption.Select(0);
            return;
        }


        for (
            int index = 0;
            index < genderOption.ItemCount;
            index++)
        {
            if (genderOption.GetItemText(index) ==
                gender)
            {
                genderOption.Select(index);
                return;
            }
        }


        genderOption.Select(0);
    }


    private string GetSelectedGender()
    {
        int selectedIndex =
            genderOption.Selected;


        if (selectedIndex < 0 ||
            selectedIndex >= genderOption.ItemCount)
        {
            return "";
        }


        if (selectedIndex == 0)
        {
            return "";
        }


        return genderOption.GetItemText(
            selectedIndex
        );
    }


    private EditorCharacterData GetCurrentCharacterState()
    {
        if (characterData == null)
        {
            characterData =
                new EditorCharacterData();
        }


        characterData.Name =
            nameLineEdit.Text.Trim();

        characterData.Age =
            Mathf.Clamp(
                (int)ageSpinBox.Value,
                0,
                200
            );

        characterData.Gender =
            GetSelectedGender();

        characterData.IsAlive =
            aliveCheckButton.ButtonPressed;


        if (!string.IsNullOrWhiteSpace(
            editingCharacterId))
        {
            characterData.Id =
                editingCharacterId;
        }


        return characterData;
    }


    private void SaveOriginalState()
    {
        if (unsavedChangesGuard == null)
        {
            return;
        }


        unsavedChangesGuard.SaveOriginalState(
            characterData
        );
    }
    public void SaveCurrentCharacter()
{
    OnSavePressed();
}


    private void OnSavePressed()
    {
        if (characterDatabase == null)
        {
            ShowError(
                "La base de personajes no está disponible."
            );

            return;
        }


        string name =
            nameLineEdit.Text.Trim();


        if (string.IsNullOrWhiteSpace(
            name))
        {
            ShowError(
                "El personaje necesita un nombre."
            );

            return;
        }


        bool creating =
            string.IsNullOrWhiteSpace(
                editingCharacterId
            );


        if (characterData == null)
        {
            characterData =
                new EditorCharacterData();
        }


        if (creating)
        {
            characterData.Id =
                characterDatabase.GenerateUniqueId(
                    name
                );
        }
        else
        {
            characterData.Id =
                editingCharacterId;
        }


        characterData.Name =
            name;

        characterData.Age =
            Mathf.Clamp(
                (int)ageSpinBox.Value,
                0,
                200
            );

        characterData.Gender =
            GetSelectedGender();

        characterData.IsAlive =
            aliveCheckButton.ButtonPressed;


        bool saved =
            characterDatabase.SaveCharacter(
                characterData
            );


        if (!saved)
        {
            ShowError(
                "No se pudo guardar el personaje."
            );

            return;
        }


        editingCharacterId =
            characterData.Id;


        SaveOriginalState();


        EmitSignal(
            SignalName.CharacterSaved
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
            SignalName.CharacterCancelled
        );
    }


    private void ShowError(
        string message)
    {
        AcceptDialog dialog =
            new AcceptDialog();


        dialog.Title =
            "Personajes";


        dialog.DialogText =
            message;


        AddChild(
            dialog
        );


        dialog.PopupCentered();
    }
}