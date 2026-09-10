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
        HBoxContainer row =
            new HBoxContainer();

        row.CustomMinimumSize =
            new Vector2(0, 50);


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


        Label valueLabel =
            new Label();

        valueLabel.Text =
            $"Valor inicial: {relationship.InitialValue}";

        valueLabel.CustomMinimumSize =
            new Vector2(160, 0);

        valueLabel.VerticalAlignment =
            VerticalAlignment.Center;


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


        Button editButton =
            new Button();

        editButton.Text =
            "Editar";

        editButton.CustomMinimumSize =
            new Vector2(90, 40);


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


        relationshipList.AddChild(
            row
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