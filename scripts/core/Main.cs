using Godot;

public partial class Main : Control
{
    private ProjectManager projectManager;
    private Control startScreen;
    private MainEditor mainEditor;


    public override void _Ready()
    {
        projectManager =
            GetNode<ProjectManager>(
                "ProjectManager"
            );

        startScreen =
            GetNode<Control>(
                "StartScreen"
            );

        projectManager.ProjectOpened +=
            OnProjectOpened;
    }


    public override void _Notification(
        int what)
    {
        if (
            what ==
            NotificationWMCloseRequest)
        {
            RequestApplicationClose();
        }
    }


    private void OnProjectOpened()
    {
        GD.Print(
            "Main: proyecto abierto."
        );

        startScreen.Hide();


        if (mainEditor != null)
        {
            mainEditor.QueueFree();
        }


        PackedScene mainEditorScene =
            GD.Load<PackedScene>(
                "res://scenes/MainEditor.tscn"
            );


        mainEditor =
            mainEditorScene.Instantiate<MainEditor>();


        AddChild(
            mainEditor
        );
    }


    private void RequestApplicationClose()
    {
        if (mainEditor == null)
        {
            GetTree().Quit();
            return;
        }


        mainEditor.RequestApplicationClose();
    }


    public void ConfirmApplicationClose()
    {
        GetTree().Quit();
    }
}