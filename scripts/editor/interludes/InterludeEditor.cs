using Godot;

public partial class InterludeEditor : Control
{
    [Signal]
    public delegate void InterludeSavedEventHandler();

    [Signal]
    public delegate void InterludeCancelledEventHandler();

    [Signal]
    public delegate void ApplicationCloseConfirmedEventHandler();

    private InterludeRepository interludeRepository;

    private EditorInterludeData currentInterlude;

    private LineEdit idEdit;
    private LineEdit titleEdit;
    private OptionButton styleOption;

    private Button closeButton;
    private Button cancelButton;
    private Button saveButton;

    private bool isNewInterlude = false;


    public override void _Ready()
    {
        idEdit =
            GetNode<LineEdit>(
                "VBoxContainer/InterludeInfo/IdContainer/IdEdit"
            );


        titleEdit =
            GetNode<LineEdit>(
                "VBoxContainer/InterludeInfo/TitleContainer/TitleEdit"
            );


        styleOption =
            GetNode<OptionButton>(
                "VBoxContainer/InterludeInfo/StyleContainer/StyleOption"
            );


        closeButton =
            GetNode<Button>(
                "VBoxContainer/Header/CloseButton"
            );


        cancelButton =
            GetNode<Button>(
                "VBoxContainer/Buttons/CancelButton"
            );


        saveButton =
            GetNode<Button>(
                "VBoxContainer/Buttons/SaveButton"
            );


        closeButton.Pressed +=
            OnClosePressed;


        cancelButton.Pressed +=
            OnCancelPressed;


        saveButton.Pressed +=
            OnSavePressed;
    }


    public void SetRepository(
        InterludeRepository repository)
    {
        interludeRepository =
            repository;
    }


    public void CreateNewInterlude()
    {
        currentInterlude =
            new EditorInterludeData();


        isNewInterlude =
            true;


        idEdit.Text = "";
        titleEdit.Text = "";


        styleOption.Select(
            (int)InterludeStyle.Letter
        );
    }


    public void LoadInterlude(
        string interludeId)
    {
        if (interludeRepository == null)
        {
            GD.PrintErr(
                "InterludeEditor: InterludeRepository no está disponible."
            );

            return;
        }


        if (string.IsNullOrWhiteSpace(
            interludeId))
        {
            GD.PrintErr(
                "InterludeEditor: el ID del interludio está vacío."
            );

            return;
        }


        EditorInterludeData interlude =
            interludeRepository.Load(
                interludeId
            );


        if (interlude == null)
        {
            GD.PrintErr(
                $"InterludeEditor: no se pudo cargar el interludio '{interludeId}'."
            );

            return;
        }


        currentInterlude =
            interlude;


        isNewInterlude =
            false;


        LoadInterludeIntoEditor(
            currentInterlude
        );
    }


    private void LoadInterludeIntoEditor(
        EditorInterludeData interlude)
    {
        if (interlude == null)
        {
            return;
        }


        idEdit.Text =
            interlude.Id ?? "";


        titleEdit.Text =
            interlude.Title ?? "";


        styleOption.Select(
            (int)interlude.Style
        );
    }


    private void UpdateInterludeDataFromEditor()
    {
        if (currentInterlude == null)
        {
            currentInterlude =
                new EditorInterludeData();
        }


        currentInterlude.Id =
            idEdit.Text.Trim();


        currentInterlude.Title =
            titleEdit.Text.Trim();


        currentInterlude.Style =
            (InterludeStyle)styleOption.Selected;
    }


    private void OnSavePressed()
    {
        if (interludeRepository == null)
        {
            GD.PrintErr(
                "InterludeEditor: InterludeRepository no está disponible."
            );

            return;
        }


        UpdateInterludeDataFromEditor();


        if (string.IsNullOrWhiteSpace(
            currentInterlude.Id))
        {
            GD.PrintErr(
                "InterludeEditor: el ID no puede estar vacío."
            );

            return;
        }


        if (
            isNewInterlude &&
            interludeRepository.Exists(
                currentInterlude.Id
            ))
        {
            GD.PrintErr(
                $"InterludeEditor: ya existe un interludio con ID '{currentInterlude.Id}'."
            );

            return;
        }


        try
        {
            interludeRepository.Save(
                currentInterlude
            );
        }
        catch (System.Exception exception)
        {
            GD.PrintErr(
                "InterludeEditor: error al guardar: ",
                exception.Message
            );

            return;
        }


        GD.Print(
            "InterludeEditor: interludio guardado: ",
            currentInterlude.Id
        );


        EmitSignal(
            SignalName.InterludeSaved
        );
    }


    private void OnCancelPressed()
    {
        EmitSignal(
            SignalName.InterludeCancelled
        );
    }


    private void OnClosePressed()
    {
        EmitSignal(
            SignalName.InterludeCancelled
        );
    }


    public void RequestApplicationClose()
    {
        EmitSignal(
            SignalName.ApplicationCloseConfirmed
        );
    }
}