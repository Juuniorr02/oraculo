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
        PanelContainer rowPanel =
            new PanelContainer();

        rowPanel.CustomMinimumSize =
            new Vector2(0, 50);


        StyleBoxFlat rowStyle =
            new StyleBoxFlat();

        rowStyle.BgColor =
            new Color("222730");

        rowStyle.BorderColor =
            new Color("313743");

        rowStyle.SetBorderWidthAll(
            1
        );

        rowStyle.CornerRadiusTopLeft = 3;
        rowStyle.CornerRadiusTopRight = 3;
        rowStyle.CornerRadiusBottomLeft = 3;
        rowStyle.CornerRadiusBottomRight = 3;


        rowPanel.AddThemeStyleboxOverride(
            "panel",
            rowStyle
        );


        MarginContainer margin =
            new MarginContainer();

        margin.AddThemeConstantOverride(
            "margin_left",
            12
        );

        margin.AddThemeConstantOverride(
            "margin_top",
            5
        );

        margin.AddThemeConstantOverride(
            "margin_right",
            8
        );

        margin.AddThemeConstantOverride(
            "margin_bottom",
            5
        );


        HBoxContainer row =
            new HBoxContainer();

        row.AddThemeConstantOverride(
            "separation",
            10
        );


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

        nameLabel.AddThemeColorOverride(
            "font_color",
            new Color("E7EAF0")
        );


        Label idLabel =
            new Label();

        idLabel.Text =
            $"ID: {character.Id}";

        idLabel.CustomMinimumSize =
            new Vector2(180, 0);

        idLabel.VerticalAlignment =
            VerticalAlignment.Center;

        idLabel.AddThemeColorOverride(
            "font_color",
            new Color("6F7785")
        );


        Label ageLabel =
            new Label();

        ageLabel.Text =
            $"Edad: {character.Age}";

        ageLabel.CustomMinimumSize =
            new Vector2(100, 0);

        ageLabel.VerticalAlignment =
            VerticalAlignment.Center;

        ageLabel.AddThemeColorOverride(
            "font_color",
            new Color("A8AFBC")
        );


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

        aliveLabel.AddThemeColorOverride(
            "font_color",
            character.IsAlive
                ? new Color("5FA878")
                : new Color("6F7785")
        );


        Button editButton =
            new Button();

        editButton.Text =
            "Editar";

        editButton.CustomMinimumSize =
            new Vector2(90, 40);

        editButton.AddThemeColorOverride(
            "font_color",
            new Color("A8AFBC")
        );

        editButton.AddThemeColorOverride(
            "font_hover_color",
            new Color("E7EAF0")
        );

        editButton.AddThemeColorOverride(
            "font_pressed_color",
            new Color("E7EAF0")
        );

        editButton.AddThemeColorOverride(
            "font_focus_color",
            new Color("E7EAF0")
        );


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


        margin.AddChild(
            row
        );

        rowPanel.AddChild(
            margin
        );


        characterList.AddChild(
            rowPanel
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