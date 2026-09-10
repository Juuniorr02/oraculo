using Godot;
using System.Collections.Generic;

public partial class EventListView : Control
{
    [Signal]
    public delegate void EventSelectedEventHandler(
        string eventId
    );


    private LineEdit searchBar;
    private Label eventCountLabel;
    private VBoxContainer eventList;
    private Button newEventButton;

    private PackedScene eventListItemScene;

    private EventRepository eventRepository;

    private List<EditorEventData> allEvents =
        new List<EditorEventData>();


    public override void _Ready()
    {
        searchBar =
            GetNode<LineEdit>(
                "VBoxContainer/SearchBar"
            );

        eventCountLabel =
            GetNode<Label>(
                "VBoxContainer/Footer/EventCountLabel"
            );

        eventList =
            GetNode<VBoxContainer>(
                "VBoxContainer/EventList"
            );

        newEventButton =
            GetNode<Button>(
                "VBoxContainer/Header/NewEventButton"
            );

        eventListItemScene =
            GD.Load<PackedScene>(
                "res://scenes/EventListItem.tscn"
            );


        searchBar.TextChanged +=
            OnSearchTextChanged;

        newEventButton.Pressed +=
            OnNewEventPressed;


        GD.Print(
            "EventListView iniciado."
        );


        if (eventRepository != null)
        {
            LoadEvents();
        }
    }


    public void SetRepository(
        EventRepository repository)
    {
        eventRepository =
            repository;

        if (IsNodeReady())
        {
            LoadEvents();
        }
    }


    private void LoadEvents()
    {
        if (eventRepository == null)
        {
            GD.PrintErr(
                "EventListView: no hay EventRepository disponible."
            );

            return;
        }


        allEvents =
            eventRepository.LoadAll();


        RefreshEventList();
    }


    private void OnNewEventPressed()
    {
        GD.Print(
            "EventListView: creando nuevo evento."
        );

        EmitSignal(
            SignalName.EventSelected,
            ""
        );
    }


    private void OnSearchTextChanged(
        string searchText)
    {
        RefreshEventList();
    }


    private void RefreshEventList()
    {
        ClearEventList();


        string searchText =
            searchBar.Text.Trim();


        int visibleEventCount = 0;


        foreach (
            EditorEventData eventData
            in allEvents)
        {
            if (eventData == null)
            {
                continue;
            }


            if (!MatchesSearch(
                eventData,
                searchText))
            {
                continue;
            }


            AddEventItem(
                eventData
            );


            visibleEventCount++;
        }


        UpdateEventCount(
            visibleEventCount
        );
    }


    private bool MatchesSearch(
        EditorEventData eventData,
        string searchText)
    {
        if (string.IsNullOrWhiteSpace(
            searchText))
        {
            return true;
        }


        string id =
            eventData.Id ?? "";


        string title =
            eventData.Title ?? "";


        return id.Contains(
                   searchText,
                   System.StringComparison.OrdinalIgnoreCase
               )
               ||
               title.Contains(
                   searchText,
                   System.StringComparison.OrdinalIgnoreCase
               );
    }


    private void AddEventItem(
        EditorEventData eventData)
    {
        EventListItem item =
            eventListItemScene.Instantiate<EventListItem>();


        eventList.AddChild(
            item
        );


        item.SetEventData(
            eventData.Id,
            eventData.Title,
            GetChapterDisplayName(
                eventData.Chapter
            )
        );


        item.EventSelected +=
            OnEventSelected;
    }


    private string GetChapterDisplayName(
        int chapter)
    {
        return chapter.ToString();
    }


    private void ClearEventList()
    {
        foreach (
            Node child
            in eventList.GetChildren())
        {
            child.QueueFree();
        }
    }


    private void UpdateEventCount(
        int count)
    {
        if (searchBar.Text.Trim().Length > 0)
        {
            eventCountLabel.Text =
                $"{count} eventos encontrados";
        }
        else
        {
            eventCountLabel.Text =
                $"{count} eventos";
        }
    }


    private void OnEventSelected(
        string eventId)
    {
        GD.Print(
            "Evento seleccionado: ",
            eventId
        );


        EmitSignal(
            SignalName.EventSelected,
            eventId
        );
    }
}