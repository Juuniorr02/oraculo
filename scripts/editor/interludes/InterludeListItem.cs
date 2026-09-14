using Godot;

public partial class InterludeListItem : PanelContainer
{
    [Signal]
    public delegate void InterludeSelectedEventHandler(
        string interludeId
    );

    [Signal]
    public delegate void InterludeDeleteRequestedEventHandler(
        string interludeId
    );


    private Label idLabel;
    private Label titleLabel;
    private Label styleLabel;
    private Button deleteButton;


    private string interludeId = "";


    public override void _Ready()
    {
        idLabel =
            GetNode<Label>(
                "MarginContainer/HBoxContainer/IdLabel"
            );

        titleLabel =
            GetNode<Label>(
                "MarginContainer/HBoxContainer/TitleLabel"
            );

        styleLabel =
            GetNode<Label>(
                "MarginContainer/HBoxContainer/StyleLabel"
            );

        deleteButton =
            GetNode<Button>(
                "MarginContainer/HBoxContainer/DeleteButton"
            );


        GuiInput += OnGuiInput;

        deleteButton.Pressed +=
            OnDeletePressed;
    }


    public void SetData(
        string id,
        string title,
        string style)
    {
        interludeId = id ?? "";


        idLabel.Text =
            interludeId;


        titleLabel.Text =
            string.IsNullOrWhiteSpace(title)
                ? "(Sin título)"
                : title;


        styleLabel.Text =
            style ?? "";
    }


    private void OnGuiInput(
        InputEvent @event)
    {
        if (
            @event is InputEventMouseButton mouseButton &&
            mouseButton.Pressed &&
            mouseButton.ButtonIndex ==
            MouseButton.Left)
        {
            EmitSignal(
                SignalName.InterludeSelected,
                interludeId
            );
        }
    }


    private void OnDeletePressed()
    {
        EmitSignal(
            SignalName.InterludeDeleteRequested,
            interludeId
        );
    }
}