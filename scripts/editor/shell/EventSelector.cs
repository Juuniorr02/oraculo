using Godot;
using System;
using System.Collections.Generic;

public partial class EventSelector : VBoxContainer
{
    [Signal]
    public delegate void EventSelectedEventHandler(string eventId);

    private LineEdit searchEdit;
    private PopupMenu resultPopup;

    private EventRepository eventRepository;

    private List<EditorEventData> events =
        new List<EditorEventData>();

    private string selectedEventId = "";

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
        EventRepository repository)
    {
        eventRepository =
            repository;

        LoadEvents();
    }


    public string GetSelectedEventId()
    {
        return selectedEventId;
    }


    public void SetSelectedEventId(
        string eventId)
    {
        selectedEventId =
            eventId ?? "";

        UpdateDisplayedText();

        if (string.IsNullOrWhiteSpace(
            selectedEventId))
        {
            return;
        }

        EditorEventData selectedEvent =
            FindEventById(
                selectedEventId
            );

        if (selectedEvent == null)
        {
            return;
        }

        updatingText = true;

        searchEdit.Text =
            BuildEventDisplayText(
                selectedEvent
            );

        updatingText = false;
    }


    public void ClearSelection()
    {
        selectedEventId =
            "";

        updatingText = true;

        searchEdit.Text =
            "";

        updatingText = false;
    }


    private void LoadEvents()
    {
        events.Clear();

        if (eventRepository == null)
        {
            return;
        }

        List<EditorEventData> loadedEvents =
            eventRepository.LoadAll();

        if (loadedEvents == null)
        {
            return;
        }

        foreach (
            EditorEventData eventData
            in loadedEvents)
        {
            if (eventData == null)
            {
                continue;
            }

            events.Add(
                eventData
            );
        }

        SortEvents();
    }


    private void SortEvents()
    {
        events.Sort(
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
            EditorEventData eventData
            in events)
        {
            if (!MatchesFilter(
                eventData,
                filter))
            {
                continue;
            }

            string displayText =
                BuildEventDisplayText(
                    eventData
                );

            resultPopup.AddItem(
                displayText
            );

            int itemIndex =
                resultPopup.ItemCount - 1;

            resultPopup.SetItemMetadata(
                itemIndex,
                eventData.Id
            );
        }

        if (resultPopup.ItemCount == 0)
        {
            resultPopup.AddItem(
                "No se encontraron eventos."
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
        EditorEventData eventData,
        string filter)
    {
        if (string.IsNullOrWhiteSpace(
            filter))
        {
            return true;
        }

        string id =
            eventData.Id ?? "";

        string title =
            eventData.Title ?? "";

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


    private string BuildEventDisplayText(
        EditorEventData eventData)
    {
        string title =
            string.IsNullOrWhiteSpace(
                eventData.Title)
                ? "(Sin título)"
                : eventData.Title;

        string id =
            string.IsNullOrWhiteSpace(
                eventData.Id)
                ? "(Sin ID)"
                : eventData.Id;

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

        string eventId =
            metadata.AsString();

        if (string.IsNullOrWhiteSpace(
            eventId))
        {
            return;
        }

        selectedEventId =
            eventId;

        EditorEventData selectedEvent =
            FindEventById(
                eventId
            );

        updatingText = true;

        if (selectedEvent != null)
        {
            searchEdit.Text =
                BuildEventDisplayText(
                    selectedEvent
                );
        }
        else
        {
            searchEdit.Text =
                eventId;
        }

        updatingText = false;

        resultPopup.Hide();

        EmitSignal(
            SignalName.EventSelected,
            selectedEventId
        );
    }


    private EditorEventData FindEventById(
        string eventId)
    {
        foreach (
            EditorEventData eventData
            in events)
        {
            if (eventData == null)
            {
                continue;
            }

            if (eventData.Id == eventId)
            {
                return eventData;
            }
        }

        return null;
    }


    private void UpdateDisplayedText()
    {
        if (string.IsNullOrWhiteSpace(
            selectedEventId))
        {
            updatingText = true;

            searchEdit.Text =
                "";

            updatingText = false;

            return;
        }

        EditorEventData eventData =
            FindEventById(
                selectedEventId
            );

        updatingText = true;

        if (eventData != null)
        {
            searchEdit.Text =
                BuildEventDisplayText(
                    eventData
                );
        }
        else
        {
            searchEdit.Text =
                selectedEventId;
        }

        updatingText = false;
    }
}