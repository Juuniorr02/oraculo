using Godot;
using System.Collections.Generic;

public partial class RelationshipListView : Control
{
    [Signal]
    public delegate void RelationshipSelectedEventHandler(
        string characterId
    );

    private VBoxContainer relationshipList;

    private Button newRelationshipButton;

    private CharacterDatabase characterDatabase;
    private RelationshipDatabase relationshipDatabase;

    private string projectPath = "";


    public override void _Ready()
    {
        relationshipList =
            GetNode<VBoxContainer>(
                "MarginContainer/VBoxContainer/RelationshipList"
            );

        newRelationshipButton =
            GetNode<Button>(
                "MarginContainer/VBoxContainer/TopBar/NewRelationshipButton"
            );

        newRelationshipButton.Pressed +=
            OnNewRelationshipPressed;

        GD.Print(
            "RelationshipListView iniciado."
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
                "RelationshipListView: la ruta del proyecto está vacía."
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

        Refresh();
    }


    public void Refresh()
    {
        if (relationshipList == null)
        {
            return;
        }

        ClearList();

        if (characterDatabase == null ||
            relationshipDatabase == null)
        {
            return;
        }

        List<EditorRelationshipDefinitionData> relationships =
            relationshipDatabase.LoadAll();

        foreach (
            EditorRelationshipDefinitionData relationship
            in relationships)
        {
            if (relationship == null)
            {
                continue;
            }

            AddRelationshipRow(
                relationship
            );
        }
    }


    private void ClearList()
    {
        foreach (
            Node child
            in relationshipList.GetChildren())
        {
            child.QueueFree();
        }
    }


    private void AddRelationshipRow(
        EditorRelationshipDefinitionData relationship)
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


        EditorCharacterData character =
            characterDatabase.GetCharacter(
                relationship.CharacterId
            );


        string characterName =
            character != null &&
            !string.IsNullOrWhiteSpace(
                character.Name)
                ? character.Name
                : relationship.CharacterId;


        Label characterLabel =
            new Label();

        characterLabel.Text =
            characterName;

        characterLabel.SizeFlagsHorizontal =
            Control.SizeFlags.ExpandFill;

        characterLabel.VerticalAlignment =
            VerticalAlignment.Center;

        characterLabel.AddThemeColorOverride(
            "font_color",
            new Color("E7EAF0")
        );


        Label valueLabel =
            new Label();

        valueLabel.Text =
            $"Valor inicial: {relationship.InitialValue}";

        valueLabel.CustomMinimumSize =
            new Vector2(160, 0);

        valueLabel.VerticalAlignment =
            VerticalAlignment.Center;

        valueLabel.AddThemeColorOverride(
            "font_color",
            new Color("A8AFBC")
        );


        int titleCount =
            relationship.Titles == null
                ? 0
                : relationship.Titles.Count;


        Label titleLabel =
            new Label();

        titleLabel.Text =
            $"Títulos: {titleCount}";

        titleLabel.CustomMinimumSize =
            new Vector2(100, 0);

        titleLabel.VerticalAlignment =
            VerticalAlignment.Center;

        titleLabel.AddThemeColorOverride(
            "font_color",
            new Color("A8AFBC")
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
            relationship.CharacterId;

        editButton.Pressed += () =>
        {
            EmitSignal(
                SignalName.RelationshipSelected,
                characterId
            );
        };


        row.AddChild(
            characterLabel
        );

        row.AddChild(
            valueLabel
        );

        row.AddChild(
            titleLabel
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

        relationshipList.AddChild(
            rowPanel
        );
    }


    private void OnNewRelationshipPressed()
    {
        EmitSignal(
            SignalName.RelationshipSelected,
            ""
        );
    }
}