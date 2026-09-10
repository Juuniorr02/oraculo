using Godot;

public partial class EventListItem : PanelContainer
{
    [Signal]
    public delegate void EventSelectedEventHandler(
        string eventId
    );


    private Label idLabel;
    private Label titleLabel;
    private Label chapterLabel;

    private string eventId = "";


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

        chapterLabel =
            GetNode<Label>(
                "MarginContainer/HBoxContainer/ChapterLabel"
            );


        GuiInput +=
            OnGuiInput;
    }


    public void SetEventData(
        string id,
        string title,
        string chapter)
    {
        eventId =
            id;

        idLabel.Text =
            id;

        titleLabel.Text =
            title;

        chapterLabel.Text =
            $"Capítulo {chapter}";
    }


    private void OnGuiInput(
        InputEvent inputEvent)
    {
        if (inputEvent is InputEventMouseButton mouseButton)
        {
            if (mouseButton.Pressed &&
                mouseButton.ButtonIndex ==
                MouseButton.Left)
            {
                EmitSignal(
                    SignalName.EventSelected,
                    eventId
                );
            }
        }
    }
}