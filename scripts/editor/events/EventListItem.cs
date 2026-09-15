using Godot;

public partial class EventListItem : PanelContainer
{
    [Signal]
    public delegate void EventSelectedEventHandler(
        string eventId
    );

    [Signal]
    public delegate void EventDeleteRequestedEventHandler(
        string eventId
    );


    private Label idLabel;
    private Label titleLabel;
    private Label chapterLabel;
    private Button deleteButton;

    private string eventId = "";

    private StyleBoxFlat normalStyle;
    private StyleBoxFlat hoverStyle;
    private StyleBoxFlat selectedStyle;

    private bool isSelected = false;


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

        deleteButton =
            GetNode<Button>(
                "MarginContainer/HBoxContainer/DeleteButton"
            );


        CreateStyles();

        ApplyNormalStyle();


        GuiInput +=
            OnGuiInput;

        MouseEntered +=
            OnMouseEntered;

        MouseExited +=
            OnMouseExited;

        deleteButton.Pressed +=
            OnDeleteButtonPressed;
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
    public string GetEventId()
{
    return eventId;
}


    public void SetSelected(
        bool selected)
    {
        isSelected =
            selected;

        if (isSelected)
        {
            ApplySelectedStyle();
        }
        else
        {
            ApplyNormalStyle();
        }
    }


    private void CreateStyles()
    {
        normalStyle =
            new StyleBoxFlat();

        normalStyle.BgColor =
            new Color(
                "#222730"
            );

        normalStyle.BorderWidthLeft = 1;
        normalStyle.BorderWidthTop = 1;
        normalStyle.BorderWidthRight = 1;
        normalStyle.BorderWidthBottom = 1;

        normalStyle.BorderColor =
            new Color(
                "#313743"
            );

        normalStyle.CornerRadiusTopLeft = 3;
        normalStyle.CornerRadiusTopRight = 3;
        normalStyle.CornerRadiusBottomRight = 3;
        normalStyle.CornerRadiusBottomLeft = 3;


        hoverStyle =
            new StyleBoxFlat();

        hoverStyle.BgColor =
            new Color(
                "#252D35"
            );

        hoverStyle.BorderWidthLeft = 1;
        hoverStyle.BorderWidthTop = 1;
        hoverStyle.BorderWidthRight = 1;
        hoverStyle.BorderWidthBottom = 1;

        hoverStyle.BorderColor =
            new Color(
                "#4FA6A6"
            );

        hoverStyle.CornerRadiusTopLeft = 3;
        hoverStyle.CornerRadiusTopRight = 3;
        hoverStyle.CornerRadiusBottomRight = 3;
        hoverStyle.CornerRadiusBottomLeft = 3;


        selectedStyle =
            new StyleBoxFlat();

        selectedStyle.BgColor =
            new Color(
                "#27343A"
            );

        selectedStyle.BorderWidthLeft = 2;
        selectedStyle.BorderWidthTop = 1;
        selectedStyle.BorderWidthRight = 1;
        selectedStyle.BorderWidthBottom = 1;

        selectedStyle.BorderColor =
            new Color(
                "#4FA6A6"
            );

        selectedStyle.CornerRadiusTopLeft = 3;
        selectedStyle.CornerRadiusTopRight = 3;
        selectedStyle.CornerRadiusBottomRight = 3;
        selectedStyle.CornerRadiusBottomLeft = 3;
    }


    private void ApplyNormalStyle()
    {
        AddThemeStyleboxOverride(
            "panel",
            normalStyle
        );
    }


    private void ApplyHoverStyle()
    {
        AddThemeStyleboxOverride(
            "panel",
            hoverStyle
        );
    }


    private void ApplySelectedStyle()
    {
        AddThemeStyleboxOverride(
            "panel",
            selectedStyle
        );
    }


    private void OnMouseEntered()
    {
        if (!isSelected)
        {
            ApplyHoverStyle();
        }
    }


    private void OnMouseExited()
    {
        if (!isSelected)
        {
            ApplyNormalStyle();
        }
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
                SetSelected(true);

                EmitSignal(
                    SignalName.EventSelected,
                    eventId
                );
            }
        }
    }


    private void OnDeleteButtonPressed()
    {
        GD.Print(
            "EventListItem: eliminación solicitada para: ",
            eventId
        );

        EmitSignal(
            SignalName.EventDeleteRequested,
            eventId
        );
    }
}