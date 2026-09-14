using Godot;
using System.Collections.Generic;

public partial class InterludeListView : Control
{
    [Signal]
    public delegate void InterludeSelectedEventHandler(
        string interludeId
    );


    private LineEdit searchBar;
    private Label interludeCountLabel;
    private VBoxContainer interludeList;
    private Button newInterludeButton;


    private PackedScene interludeListItemScene;


    private InterludeRepository interludeRepository;


    private List<EditorInterludeData> allInterludes =
        new List<EditorInterludeData>();


    private ConfirmationDialog deleteConfirmationDialog;

    private string pendingDeleteInterludeId = "";


    public override void _Ready()
    {
        searchBar =
            GetNode<LineEdit>(
                "VBoxContainer/SearchBar"
            );

        interludeCountLabel =
            GetNode<Label>(
                "VBoxContainer/Footer/InterludeCountLabel"
            );

        interludeList =
            GetNode<VBoxContainer>(
                "VBoxContainer/InterludeList"
            );

        newInterludeButton =
            GetNode<Button>(
                "VBoxContainer/Header/NewInterludeButton"
            );


        interludeListItemScene =
            GD.Load<PackedScene>(
                "res://scenes/InterludeListItem.tscn"
            );


        searchBar.TextChanged += OnSearchChanged;

        newInterludeButton.Pressed +=
            OnNewInterludePressed;


        CreateDeleteConfirmation();


        if (interludeRepository != null)
        {
            LoadInterludes();
        }
    }


    public void SetRepository(
        InterludeRepository repository)
    {
        interludeRepository =
            repository;


        if (IsNodeReady())
        {
            LoadInterludes();
        }
    }


    private void LoadInterludes()
    {
        if (interludeRepository == null)
        {
            return;
        }


        allInterludes =
            interludeRepository.LoadAll();


        RefreshInterludeList();
    }


    private void OnSearchChanged(
        string newText)
    {
        RefreshInterludeList();
    }


    private void OnNewInterludePressed()
    {
        EmitSignal(
            SignalName.InterludeSelected,
            ""
        );
    }


    private void RefreshInterludeList()
    {
        foreach (
            Node child
            in interludeList.GetChildren())
        {
            child.QueueFree();
        }


        string search =
            searchBar.Text.Trim();


        int visibleCount = 0;


        foreach (
            EditorInterludeData interlude
            in allInterludes)
        {
            if (interlude == null)
            {
                continue;
            }


            if (!MatchesSearch(
                interlude,
                search))
            {
                continue;
            }


            AddInterludeItem(
                interlude
            );


            visibleCount++;
        }


        interludeCountLabel.Text =
            $"{visibleCount} interludio(s)";
    }


    private bool MatchesSearch(
        EditorInterludeData interlude,
        string search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return true;
        }


        return
            (interlude.Id ?? "")
                .Contains(
                    search,
                    System.StringComparison.OrdinalIgnoreCase
                )
            ||
            (interlude.Title ?? "")
                .Contains(
                    search,
                    System.StringComparison.OrdinalIgnoreCase
                );
    }


    private void AddInterludeItem(
        EditorInterludeData interlude)
    {
        if (interludeListItemScene == null)
        {
            return;
        }


        InterludeListItem item =
            interludeListItemScene.Instantiate<
                InterludeListItem
            >();


        interludeList.AddChild(
            item
        );


        item.SetData(
            interlude.Id,
            interlude.Title,
            GetStyleDisplayName(
                interlude.Style
            )
        );


        item.InterludeSelected +=
            OnInterludeSelected;

        item.InterludeDeleteRequested +=
            OnInterludeDeleteRequested;
    }


    private string GetStyleDisplayName(
        InterludeStyle style)
    {
        return style switch
        {
            InterludeStyle.Letter =>
                "Carta",

            InterludeStyle.Newspaper =>
                "Periódico",

            _ =>
                style.ToString()
        };
    }


    private void OnInterludeSelected(
        string interludeId)
    {
        EmitSignal(
            SignalName.InterludeSelected,
            interludeId
        );
    }


    private void OnInterludeDeleteRequested(
        string interludeId)
    {
        if (string.IsNullOrWhiteSpace(
            interludeId))
        {
            return;
        }


        pendingDeleteInterludeId =
            interludeId;


        deleteConfirmationDialog.DialogText =
            $"¿Quieres eliminar el interludio '{interludeId}'?";


        deleteConfirmationDialog.PopupCentered();
    }


    private void CreateDeleteConfirmation()
    {
        deleteConfirmationDialog =
            new ConfirmationDialog();


        deleteConfirmationDialog.Title =
            "Eliminar interludio";


        deleteConfirmationDialog.Confirmed +=
            OnDeleteConfirmed;


        AddChild(
            deleteConfirmationDialog
        );
    }


    private void OnDeleteConfirmed()
    {
        if (
            interludeRepository == null ||
            string.IsNullOrWhiteSpace(
                pendingDeleteInterludeId))
        {
            return;
        }


        interludeRepository.Delete(
            pendingDeleteInterludeId
        );


        pendingDeleteInterludeId =
            "";


        LoadInterludes();
    }
}