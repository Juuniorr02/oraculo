using Godot;
using System.Collections.Generic;

public partial class CharacterListView : Control
{
    [Signal]
    public delegate void CharacterSelectedEventHandler(
        string characterId
    );

    private VBoxContainer characterList;

    private Button newCharacterButton;

    private CharacterDatabase characterDatabase;

    private string projectPath = "";


    public override void _Ready()
    {
        characterList =
            GetNode<VBoxContainer>(
                "MarginContainer/VBoxContainer/CharacterList"
            );

        newCharacterButton =
            GetNode<Button>(
                "MarginContainer/VBoxContainer/TopBar/NewCharacterButton"
            );


        newCharacterButton.Pressed +=
            OnNewCharacterPressed;


        GD.Print(
            "CharacterListView iniciado."
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
                "CharacterListView: la ruta del proyecto está vacía."
            );

            return;
        }


        characterDatabase =
            new CharacterDatabase(
                projectPath
            );


        Refresh();
    }


    public void Refresh()
    {
        if (characterList == null)
        {
            return;
        }


        ClearList();


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


            AddCharacterRow(
                character
            );
        }
    }


    private void ClearList()
    {
        foreach (
            Node child
            in characterList.GetChildren())
        {
            child.QueueFree();
        }
    }


    private void AddCharacterRow(
        EditorCharacterData character)
    {
        HBoxContainer row =
            new HBoxContainer();


        row.CustomMinimumSize =
            new Vector2(0, 50);


        Label nameLabel =
            new Label();

        nameLabel.Text =
            string.IsNullOrWhiteSpace(
                character.Name)
                ? character.Id
                : character.Name;

        nameLabel.SizeFlagsHorizontal =
            Control.SizeFlags.ExpandFill;

        nameLabel.VerticalAlignment =
            VerticalAlignment.Center;


        Label idLabel =
            new Label();

        idLabel.Text =
            $"ID: {character.Id}";

        idLabel.CustomMinimumSize =
            new Vector2(180, 0);

        idLabel.VerticalAlignment =
            VerticalAlignment.Center;


        Label ageLabel =
            new Label();

        ageLabel.Text =
            $"Edad: {character.Age}";

        ageLabel.CustomMinimumSize =
            new Vector2(100, 0);

        ageLabel.VerticalAlignment =
            VerticalAlignment.Center;


        Label aliveLabel =
            new Label();

        aliveLabel.Text =
            character.IsAlive
                ? "Vivo"
                : "Fallecido";

        aliveLabel.CustomMinimumSize =
            new Vector2(100, 0);

        aliveLabel.VerticalAlignment =
            VerticalAlignment.Center;


        Button editButton =
            new Button();

        editButton.Text =
            "Editar";

        editButton.CustomMinimumSize =
            new Vector2(90, 40);


        string characterId =
            character.Id;


        editButton.Pressed += () =>
        {
            EmitSignal(
                SignalName.CharacterSelected,
                characterId
            );
        };


        row.AddChild(
            nameLabel
        );

        row.AddChild(
            idLabel
        );

        row.AddChild(
            ageLabel
        );

        row.AddChild(
            aliveLabel
        );

        row.AddChild(
            editButton
        );


        characterList.AddChild(
            row
        );
    }


    private void OnNewCharacterPressed()
    {
        EmitSignal(
            SignalName.CharacterSelected,
            ""
        );
    }
}