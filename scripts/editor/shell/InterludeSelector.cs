using Godot;
using System;
using System.Collections.Generic;

public partial class InterludeSelector : VBoxContainer
{
    [Signal]
    public delegate void InterludeSelectedEventHandler(
        string interludeId
    );

    private LineEdit searchEdit;
    private PopupMenu resultPopup;

    private InterludeRepository interludeRepository;

    private List<EditorInterludeData> interludes =
        new List<EditorInterludeData>();

    private string selectedInterludeId = "";

    private bool updatingText = false;


    public override void _Ready()
    {
        searchEdit =
            new LineEdit();

        searchEdit.CustomMinimumSize =
            new Vector2(0, 35);

        searchEdit.PlaceholderText =
            "Buscar por ID o título...";

        searchEdit.ClearButtonEnabled =
            true;

        AddChild(
            searchEdit
        );


        resultPopup =
            new PopupMenu();

        resultPopup.MaxSize =
            new Vector2I(
                700,
                400
            );

        AddChild(
            resultPopup
        );


        searchEdit.TextChanged +=
            OnSearchTextChanged;

        searchEdit.GuiInput +=
            OnSearchGuiInput;

        resultPopup.IndexPressed +=
            OnResultSelected;
    }


    public void SetRepository(
        InterludeRepository repository)
    {
        interludeRepository =
            repository;

        LoadInterludes();
    }


    public string GetSelectedInterludeId()
    {
        return selectedInterludeId;
    }


    public void SetSelectedInterludeId(
        string interludeId)
    {
        selectedInterludeId =
            interludeId ?? "";

        UpdateDisplayedText();

        if (string.IsNullOrWhiteSpace(
            selectedInterludeId))
        {
            return;
        }

        EditorInterludeData selectedInterlude =
            FindInterludeById(
                selectedInterludeId
            );

        if (selectedInterlude == null)
        {
            return;
        }

        updatingText = true;

        searchEdit.Text =
            BuildInterludeDisplayText(
                selectedInterlude
            );

        updatingText = false;
    }


    public void ClearSelection()
    {
        selectedInterludeId =
            "";

        updatingText = true;

        searchEdit.Text =
            "";

        updatingText = false;
    }


    private void LoadInterludes()
    {
        interludes.Clear();

        if (interludeRepository == null)
        {
            return;
        }

        List<EditorInterludeData> loadedInterludes =
            interludeRepository.LoadAll();

        if (loadedInterludes == null)
        {
            return;
        }

        foreach (
            EditorInterludeData interludeData
            in loadedInterludes)
        {
            if (interludeData == null)
            {
                continue;
            }

            interludes.Add(
                interludeData
            );
        }

        SortInterludes();
    }


    private void SortInterludes()
    {
        interludes.Sort(
            (a, b) =>
            {
                string titleA =
                    a.Title ?? "";

                string titleB =
                    b.Title ?? "";

                int titleComparison =
                    string.Compare(
                        titleA,
                        titleB,
                        StringComparison.OrdinalIgnoreCase
                    );

                if (titleComparison != 0)
                {
                    return titleComparison;
                }

                return string.Compare(
                    a.Id ?? "",
                    b.Id ?? "",
                    StringComparison.OrdinalIgnoreCase
                );
            }
        );
    }


    private void OnSearchTextChanged(
        string newText)
    {
        if (updatingText)
        {
            return;
        }

        ShowResults(
            newText
        );
    }


    private void OnSearchGuiInput(
        InputEvent inputEvent)
    {
        if (
            inputEvent is not InputEventMouseButton
            mouseButton)
        {
            return;
        }

        if (!mouseButton.Pressed)
        {
            return;
        }

        if (
            mouseButton.ButtonIndex !=
            MouseButton.Left)
        {
            return;
        }

        ShowResults(
            searchEdit.Text
        );
    }


    private void ShowResults(
        string searchText)
    {
        resultPopup.Clear();

        string filter =
            searchText?.Trim() ?? "";

        foreach (
            EditorInterludeData interludeData
            in interludes)
        {
            if (!MatchesFilter(
                interludeData,
                filter))
            {
                continue;
            }

            string displayText =
                BuildInterludeDisplayText(
                    interludeData
                );

            resultPopup.AddItem(
                displayText
            );

            int itemIndex =
                resultPopup.ItemCount - 1;

            resultPopup.SetItemMetadata(
                itemIndex,
                interludeData.Id
            );
        }

        if (resultPopup.ItemCount == 0)
        {
            resultPopup.AddItem(
                "No se encontraron interludios."
            );

            resultPopup.SetItemDisabled(
                0,
                true
            );
        }

        Vector2 popupPosition =
            searchEdit.GlobalPosition;

        popupPosition.Y +=
            searchEdit.Size.Y;

        resultPopup.Position =
            new Vector2I(
                (int)popupPosition.X,
                (int)popupPosition.Y
            );

        resultPopup.Size =
            new Vector2I(
                Mathf.Max(
                    300,
                    (int)searchEdit.Size.X
                ),
                Mathf.Min(
                    400,
                    resultPopup.ItemCount * 32 + 10
                )
            );

        resultPopup.Popup();
    }


    private bool MatchesFilter(
        EditorInterludeData interludeData,
        string filter)
    {
        if (string.IsNullOrWhiteSpace(
            filter))
        {
            return true;
        }

        string id =
            interludeData.Id ?? "";

        string title =
            interludeData.Title ?? "";

        return
            id.Contains(
                filter,
                StringComparison.OrdinalIgnoreCase
            )
            ||
            title.Contains(
                filter,
                StringComparison.OrdinalIgnoreCase
            );
    }


    private string BuildInterludeDisplayText(
        EditorInterludeData interludeData)
    {
        string title =
            string.IsNullOrWhiteSpace(
                interludeData.Title)
                ? "(Sin título)"
                : interludeData.Title;

        string id =
            string.IsNullOrWhiteSpace(
                interludeData.Id)
                ? "(Sin ID)"
                : interludeData.Id;

        return
            $"{title}  ·  {id}";
    }


    private void OnResultSelected(
        long index)
    {
        if (index < 0 ||
            index >= resultPopup.ItemCount)
        {
            return;
        }

        Variant metadata =
            resultPopup.GetItemMetadata(
                (int)index
            );

        string interludeId =
            metadata.AsString();

        if (string.IsNullOrWhiteSpace(
            interludeId))
        {
            return;
        }

        selectedInterludeId =
            interludeId;

        EditorInterludeData selectedInterlude =
            FindInterludeById(
                interludeId
            );

        updatingText = true;

        if (selectedInterlude != null)
        {
            searchEdit.Text =
                BuildInterludeDisplayText(
                    selectedInterlude
                );
        }
        else
        {
            searchEdit.Text =
                interludeId;
        }

        updatingText = false;

        resultPopup.Hide();

        EmitSignal(
            SignalName.InterludeSelected,
            selectedInterludeId
        );
    }


    private EditorInterludeData FindInterludeById(
        string interludeId)
    {
        foreach (
            EditorInterludeData interludeData
            in interludes)
        {
            if (interludeData == null)
            {
                continue;
            }

            if (interludeData.Id == interludeId)
            {
                return interludeData;
            }
        }

        return null;
    }


    private void UpdateDisplayedText()
    {
        if (string.IsNullOrWhiteSpace(
            selectedInterludeId))
        {
            updatingText = true;

            searchEdit.Text =
                "";

            updatingText = false;

            return;
        }

        EditorInterludeData interludeData =
            FindInterludeById(
                selectedInterludeId
            );

        updatingText = true;

        if (interludeData != null)
        {
            searchEdit.Text =
                BuildInterludeDisplayText(
                    interludeData
                );
        }
        else
        {
            searchEdit.Text =
                selectedInterludeId;
        }

        updatingText = false;
    }
}