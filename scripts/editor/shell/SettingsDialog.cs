using Godot;

public partial class SettingsDialog : Window
{
    private CheckButton fullscreenCheckButton;
    private Button closeButton;


    public override void _Ready()
    {
        fullscreenCheckButton =
            GetNode<CheckButton>(
                "MarginContainer/VBoxContainer/FullscreenCheckButton"
            );


        closeButton =
            GetNode<Button>(
                "MarginContainer/VBoxContainer/CloseButton"
            );


        fullscreenCheckButton.Toggled +=
            OnFullscreenToggled;


        closeButton.Pressed +=
            OnClosePressed;


        CloseRequested +=
            OnClosePressed;


        UpdateFullscreenState();
    }


    private void OnFullscreenToggled(
        bool enabled)
    {
        if (enabled)
        {
            DisplayServer.WindowSetMode(
                DisplayServer.WindowMode.Fullscreen
            );
        }
        else
        {
            DisplayServer.WindowSetMode(
                DisplayServer.WindowMode.Windowed
            );
        }
    }


    private void UpdateFullscreenState()
    {
        fullscreenCheckButton.ButtonPressed =
            DisplayServer.WindowGetMode() ==
            DisplayServer.WindowMode.Fullscreen;
    }


    private void OnClosePressed()
    {
        Hide();
    }
}