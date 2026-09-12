using Godot;
using System.Collections.Generic;

public partial class ValidationPanel : Control
{
    [Signal]
    public delegate void ClosedEventHandler();


    [Signal]
    public delegate void IssueSelectedEventHandler(
        string eventId,
        string pageId,
        int decisionIndex
    );


    private Label errorsLabel;
    private Label warningsLabel;

    private Button validateButton;
    private Button closeButton;

    private Button allButton;
    private Button errorsButton;
    private Button warningsButton;

    private VBoxContainer issueList;


    private EventRepository eventRepository;
    private ProjectValidator projectValidator;

    private ValidationResult currentResult;

    private ValidationFilter currentFilter =
        ValidationFilter.All;


    private enum ValidationFilter
    {
        All,
        Errors,
        Warnings
    }


    public override void _Ready()
    {
        errorsLabel =
            GetNode<Label>(
                "VBoxContainer/Summary/ErrorsPanel/MarginContainer/ErrorsLabel"
            );

        warningsLabel =
            GetNode<Label>(
                "VBoxContainer/Summary/WarningsPanel/MarginContainer/WarningsLabel"
            );

        validateButton =
            GetNode<Button>(
                "VBoxContainer/Header/ValidateButton"
            );

        closeButton =
            GetNode<Button>(
                "VBoxContainer/Header/CloseButton"
            );

        allButton =
            GetNode<Button>(
                "VBoxContainer/FilterBar/AllButton"
            );

        errorsButton =
            GetNode<Button>(
                "VBoxContainer/FilterBar/ErrorsButton"
            );

        warningsButton =
            GetNode<Button>(
                "VBoxContainer/FilterBar/WarningsButton"
            );

        issueList =
            GetNode<VBoxContainer>(
                "VBoxContainer/ScrollContainer/IssueList"
            );


        validateButton.Pressed +=
            OnValidatePressed;

        closeButton.Pressed +=
            OnClosePressed;

        allButton.Pressed +=
            OnAllFilterPressed;

        errorsButton.Pressed +=
            OnErrorsFilterPressed;

        warningsButton.Pressed +=
            OnWarningsFilterPressed;


        GD.Print(
            "ValidationPanel iniciado."
        );
    }


    public void SetRepository(
        EventRepository repository)
    {
        eventRepository =
            repository;

        projectValidator =
            new ProjectValidator(
                eventRepository
            );
    }


    public void Validate()
    {
        if (projectValidator == null)
        {
            ShowNoRepositoryMessage();
            return;
        }


        currentResult =
            projectValidator.ValidateProject();


        UpdateSummary();
        RefreshIssueList();
    }


    private void OnValidatePressed()
    {
        Validate();
    }


    private void OnClosePressed()
    {
        EmitSignal(
            SignalName.Closed
        );
    }


    private void OnAllFilterPressed()
    {
        currentFilter =
            ValidationFilter.All;

        RefreshIssueList();
    }


    private void OnErrorsFilterPressed()
    {
        currentFilter =
            ValidationFilter.Errors;

        RefreshIssueList();
    }


    private void OnWarningsFilterPressed()
    {
        currentFilter =
            ValidationFilter.Warnings;

        RefreshIssueList();
    }


    private void UpdateSummary()
    {
        if (currentResult == null)
        {
            errorsLabel.Text =
                "✕ 0 ERRORES";

            warningsLabel.Text =
                "⚠ 0 ADVERTENCIAS";

            return;
        }


        errorsLabel.Text =
            $"✕ {currentResult.GetErrorCount()} ERRORES";

        warningsLabel.Text =
            $"⚠ {currentResult.GetWarningCount()} ADVERTENCIAS";
    }


    private void RefreshIssueList()
    {
        ClearIssueList();


        if (currentResult == null)
        {
            return;
        }


        foreach (
            ValidationIssue issue
            in currentResult.Issues)
        {
            if (issue == null)
            {
                continue;
            }


            if (!ShouldShowIssue(issue))
            {
                continue;
            }


            AddIssue(
                issue
            );
        }
    }


    private bool ShouldShowIssue(
        ValidationIssue issue)
    {
        if (currentFilter ==
            ValidationFilter.Errors)
        {
            return issue.IsError();
        }


        if (currentFilter ==
            ValidationFilter.Warnings)
        {
            return issue.IsWarning();
        }


        return true;
    }


    private void AddIssue(
        ValidationIssue issue)
    {
        PanelContainer panel =
            new PanelContainer();


        panel.CustomMinimumSize =
            new Vector2(
                0,
                68
            );


        if (!string.IsNullOrWhiteSpace(
            issue.EventId))
        {
            panel.MouseDefaultCursorShape =
                Control.CursorShape.PointingHand;

            panel.GuiInput +=
                (InputEvent inputEvent) =>
                {
                    OnIssueGuiInput(
                        inputEvent,
                        issue
                    );
                };
        }


        VBoxContainer content =
            new VBoxContainer();


        content.AddThemeConstantOverride(
            "separation",
            3
        );


        MarginContainer margin =
            new MarginContainer();


        margin.AddThemeConstantOverride(
            "margin_left",
            10
        );

        margin.AddThemeConstantOverride(
            "margin_right",
            10
        );

        margin.AddThemeConstantOverride(
            "margin_top",
            7
        );

        margin.AddThemeConstantOverride(
            "margin_bottom",
            7
        );


        HBoxContainer header =
            new HBoxContainer();


        header.AddThemeConstantOverride(
            "separation",
            8
        );


        Label icon =
            new Label();


        icon.Text =
            issue.IsError()
                ? "✕"
                : "⚠";


        icon.CustomMinimumSize =
            new Vector2(
                24,
                0
            );


        icon.VerticalAlignment =
            VerticalAlignment.Center;


        Label locationLabel =
            new Label();


        locationLabel.Text =
            BuildLocationText(
                issue
            );


        locationLabel.SizeFlagsHorizontal =
            Control.SizeFlags.ExpandFill;


        locationLabel.VerticalAlignment =
            VerticalAlignment.Center;


        header.AddChild(
            icon
        );


        header.AddChild(
            locationLabel
        );


        Label messageLabel =
            new Label();


        messageLabel.Text =
            issue.Message;


        messageLabel.AutowrapMode =
            TextServer.AutowrapMode.WordSmart;


        messageLabel.SizeFlagsHorizontal =
            Control.SizeFlags.ExpandFill;


        messageLabel.VerticalAlignment =
            VerticalAlignment.Center;


        content.AddChild(
            header
        );


        content.AddChild(
            messageLabel
        );


        margin.AddChild(
            content
        );


        panel.AddChild(
            margin
        );


        issueList.AddChild(
            panel
        );
    }


    private void OnIssueGuiInput(
        InputEvent inputEvent,
        ValidationIssue issue)
    {
        if (inputEvent is not InputEventMouseButton mouseButton)
        {
            return;
        }


        if (!mouseButton.Pressed)
        {
            return;
        }


        if (mouseButton.ButtonIndex !=
            MouseButton.Left)
        {
            return;
        }


        EmitSignal(
            SignalName.IssueSelected,
            issue.EventId,
            issue.PageId,
            issue.DecisionIndex
        );
    }


    private string BuildLocationText(
        ValidationIssue issue)
    {
        string eventText =
            string.IsNullOrWhiteSpace(
                issue.EventId)
                ? "Evento desconocido"
                : issue.EventId;


        List<string> parts =
            new List<string>();


        if (issue.PageIndex >= 0)
        {
            parts.Add(
                $"Página {issue.PageIndex + 1}"
            );
        }


        if (issue.DecisionIndex >= 0)
        {
            parts.Add(
                $"Opción {issue.DecisionIndex + 1}"
            );
        }


        if (parts.Count == 0)
        {
            return eventText;
        }


        return
            $"{eventText}  ·  " +
            string.Join(
                "  ·  ",
                parts
            );
    }


    private void ClearIssueList()
    {
        foreach (
            Node child
            in issueList.GetChildren())
        {
            child.QueueFree();
        }
    }


    private void ShowNoRepositoryMessage()
    {
        currentResult =
            new ValidationResult();


        currentResult.AddError(
            "No hay un repositorio de eventos disponible."
        );


        UpdateSummary();
        RefreshIssueList();
    }
}