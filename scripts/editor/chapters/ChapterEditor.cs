using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


public partial class ChapterEditor : Control
{
    private enum PendingChapterAction
    {
        None,
        CreateNew,
        Delete
    }


    [Signal]
    public delegate void ChapterSavedEventHandler();


    [Signal]
    public delegate void ChapterCancelledEventHandler();


    [Signal]
    public delegate void EventSelectedEventHandler(
        string eventId
    );


    [Signal]
    public delegate void InterludeSelectedEventHandler(
        string interludeId
    );


    [Signal]
    public delegate void ChapterPagesSelectedEventHandler(
        string chapterId
    );


    [Signal]
    public delegate void ApplicationCloseConfirmedEventHandler();


    [Signal]
    public delegate void ChapterDeletedEventHandler();


    private string projectPath = "";


    private ChapterRepository chapterRepository;
    private EventRepository eventRepository;
    private InterludeRepository interludeRepository;
    private AttributeDatabase attributeDatabase;


    private ChapterDefinitionData currentChapter;


    private Label titleLabel;
    private Label idLabel;


    private LineEdit chapterTitleEdit;


    private SpinBox numberSpinBox;
    private SpinBox startYearSpinBox;
    private SpinBox startMonthSpinBox;


    private VBoxContainer selectedAttributeList;
    private LineEdit attributeSearchEdit;
    private VBoxContainer attributeList;


    private LineEdit contentSearchEdit;
    private OptionButton contentFilterButton;
    private VBoxContainer contentList;


    private Button editPagesButton;
    private Button newButton;
    private Button deleteButton;


    private Button saveButton;
    private Button cancelButton;
    private Button closeButton;


    private List<EditorEventData> chapterEvents =
        new List<EditorEventData>();


    private List<EditorInterludeData> chapterInterludes =
        new List<EditorInterludeData>();


    private List<AttributeDefinitionData> allAttributes =
        new List<AttributeDefinitionData>();


    private UnsavedChangesGuard<ChapterDefinitionData>
        unsavedChangesGuard;


    private bool applicationCloseRequested = false;


    private bool creatingNewChapter = false;


    private PendingChapterAction pendingChapterAction =
        PendingChapterAction.None;


    public override void _Ready()
    {
        titleLabel =
            GetNode<Label>(
                "VBoxContainer/Header/Title"
            );


        idLabel =
            GetNode<Label>(
                "VBoxContainer/ChapterInfo/IdContainer/Id"
            );


        chapterTitleEdit =
            GetNode<LineEdit>(
                "VBoxContainer/ChapterInfo/TitleContainer/TitleEdit"
            );


        numberSpinBox =
            GetNode<SpinBox>(
                "VBoxContainer/ChapterInfo/BasicInfo/NumberContainer/NumberSpinBox"
            );


        startYearSpinBox =
            GetNode<SpinBox>(
                "VBoxContainer/ChapterInfo/BasicInfo/StartYearContainer/StartYearSpinBox"
            );


        startMonthSpinBox =
            GetNode<SpinBox>(
                "VBoxContainer/ChapterInfo/BasicInfo/StartMonthContainer/StartMonthSpinBox"
            );


        selectedAttributeList =
            GetNode<VBoxContainer>(
                "VBoxContainer/Attributes/SelectedAttributes/SelectedList"
            );


        attributeSearchEdit =
            GetNode<LineEdit>(
                "VBoxContainer/Attributes/AttributeSearch/AttributeSearchEdit"
            );


        attributeList =
            GetNode<VBoxContainer>(
                "VBoxContainer/Attributes/AttributeSearch/ScrollContainer/AttributeList"
            );


        contentSearchEdit =
            GetNode<LineEdit>(
                "VBoxContainer/Content/VBoxContainer/ContentHeader/SearchEdit"
            );


        contentFilterButton =
            GetNode<OptionButton>(
                "VBoxContainer/Content/VBoxContainer/ContentHeader/FilterButton"
            );


        contentList =
            GetNode<VBoxContainer>(
                "VBoxContainer/Content/VBoxContainer/ContentBody/ScrollContainer/ContentList"
            );


        editPagesButton =
            GetNode<Button>(
                "VBoxContainer/EditPagesButton"
            );


        newButton =
            GetNode<Button>(
                "VBoxContainer/Header/NewButton"
            );


        deleteButton =
            GetNode<Button>(
                "VBoxContainer/Header/DeleteButton"
            );


        saveButton =
            GetNode<Button>(
                "VBoxContainer/Buttons/SaveButton"
            );


        cancelButton =
            GetNode<Button>(
                "VBoxContainer/Buttons/CancelButton"
            );


        closeButton =
            GetNode<Button>(
                "VBoxContainer/Header/CloseButton"
            );


        saveButton.Pressed +=
            OnSavePressed;


        cancelButton.Pressed +=
            OnCancelPressed;


        closeButton.Pressed +=
            OnCancelPressed;


        newButton.Pressed +=
            OnNewChapterPressed;


        deleteButton.Pressed +=
            OnDeleteChapterPressed;


        editPagesButton.Pressed +=
            OnEditPagesPressed;


        attributeSearchEdit.TextChanged +=
            OnAttributeSearchChanged;


        contentSearchEdit.TextChanged +=
            OnContentSearchChanged;


        contentFilterButton.ItemSelected +=
            OnContentFilterChanged;


        contentFilterButton.AddItem(
            "Todos"
        );


        contentFilterButton.AddItem(
            "Eventos"
        );


        contentFilterButton.AddItem(
            "Interludios"
        );


        contentFilterButton.Select(
            0
        );


        unsavedChangesGuard =
            new UnsavedChangesGuard<ChapterDefinitionData>(
                this,
                GetCurrentChapterState,
                CloseEditor
            );


        UpdateDeleteButtonState();
    }


    public void SetProjectPath(
        string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            GD.PrintErr(
                "ChapterEditor: la ruta del proyecto está vacía."
            );

            return;
        }


        projectPath =
            path;


        chapterRepository =
            new ChapterRepository(
                projectPath
            );


        eventRepository =
            new EventRepository(
                projectPath
            );


        interludeRepository =
            new InterludeRepository(
                projectPath
            );


        attributeDatabase =
            new AttributeDatabase(
                projectPath
            );


        LoadAllAttributes();
    }


    public string GetChapterId()
    {
        return currentChapter?.Id ?? "";
    }


    public List<ChapterPageDefinitionData> GetPages()
    {
        if (currentChapter == null)
        {
            return new List<ChapterPageDefinitionData>();
        }


        List<ChapterPageDefinitionData> result =
            new List<ChapterPageDefinitionData>();


        if (currentChapter.Pages == null)
        {
            return result;
        }


        foreach (
            ChapterPageDefinitionData page
            in currentChapter.Pages)
        {
            if (page == null)
            {
                continue;
            }


            result.Add(
                ClonePage(
                    page
                )
            );
        }


        return result;
    }


    public void SetPages(
        List<ChapterPageDefinitionData> pages)
    {
        if (currentChapter == null)
        {
            return;
        }


        currentChapter.Pages =
            new List<ChapterPageDefinitionData>();


        if (pages == null)
        {
            return;
        }


        foreach (
            ChapterPageDefinitionData page
            in pages)
        {
            if (page == null)
            {
                continue;
            }


            currentChapter.Pages.Add(
                ClonePage(
                    page
                )
            );
        }
    }


    private static ChapterPageDefinitionData ClonePage(
        ChapterPageDefinitionData page)
    {
        if (page == null)
        {
            return null;
        }


        return new ChapterPageDefinitionData
        {
            Type =
                page.Type,

            Title =
                page.Title,

            Text =
                page.Text,

            ImagePath =
                page.ImagePath,

            ButtonText =
                page.ButtonText
        };
    }


    public void LoadChapter(
        string chapterId)
    {
        if (string.IsNullOrWhiteSpace(chapterId))
        {
            GD.PrintErr(
                "ChapterEditor: el ID del capítulo está vacío."
            );

            return;
        }


        if (chapterRepository == null)
        {
            GD.PrintErr(
                "ChapterEditor: ChapterRepository no está disponible."
            );

            return;
        }


        currentChapter =
            chapterRepository.Load(
                chapterId
            );


        if (currentChapter == null)
        {
            GD.PrintErr(
                "ChapterEditor: no se encontró el capítulo: ",
                chapterId
            );

            return;
        }


        creatingNewChapter =
            false;


        pendingChapterAction =
            PendingChapterAction.None;


        if (currentChapter.AttributeIds == null)
        {
            currentChapter.AttributeIds =
                new List<string>();
        }


        if (currentChapter.Pages == null)
        {
            currentChapter.Pages =
                new List<ChapterPageDefinitionData>();
        }


        PopulateChapterFields();


        LoadChapterContent();


        RefreshSelectedAttributes();


        RefreshAttributeList();


        RefreshContentList();


        SaveOriginalState();


        UpdateDeleteButtonState();
    }


    public void CreateNewChapter()
    {
        if (chapterRepository == null)
        {
            GD.PrintErr(
                "ChapterEditor: ChapterRepository no está disponible."
            );

            return;
        }


        int nextNumber =
            GetNextChapterNumber();


        string newId =
            GenerateChapterId(
                nextNumber
            );


        creatingNewChapter =
            true;


        pendingChapterAction =
            PendingChapterAction.None;


        currentChapter =
            new ChapterDefinitionData
            {
                Id = newId,
                Number = nextNumber,
                Title = "",
                StartYear = 1,
                StartMonth = 1,
                AttributeIds = new List<string>(),
                Pages = new List<ChapterPageDefinitionData>()
            };


        PopulateChapterFields();


        LoadChapterContent();


        RefreshSelectedAttributes();


        RefreshAttributeList();


        RefreshContentList();


        titleLabel.Text =
            "Nuevo capítulo";


        SaveOriginalState();


        UpdateDeleteButtonState();
    }


    private int GetNextChapterNumber()
    {
        if (chapterRepository == null)
        {
            return 1;
        }


        List<ChapterDefinitionData> chapters =
            chapterRepository.LoadAll();


        int maxNumber =
            0;


        foreach (
            ChapterDefinitionData chapter
            in chapters)
        {
            if (chapter == null)
            {
                continue;
            }


            if (chapter.Number > maxNumber)
            {
                maxNumber =
                    chapter.Number;
            }
        }


        return maxNumber + 1;
    }


    private string GenerateChapterId(
        int chapterNumber)
    {
        string baseId =
            $"chapter_{chapterNumber:D2}";


        string candidate =
            baseId;


        int suffix =
            2;


        while (
            chapterRepository != null &&
            chapterRepository.Exists(
                candidate))
        {
            candidate =
                $"{baseId}_{suffix}";


            suffix++;
        }


        return candidate;
    }


    private void PopulateChapterFields()
    {
        if (currentChapter == null)
        {
            return;
        }


        titleLabel.Text =
            creatingNewChapter
                ? "Nuevo capítulo"
                : $"Editor de capítulo · " +
                  $"{currentChapter.Number:D2}";


        idLabel.Text =
            currentChapter.Id;


        chapterTitleEdit.Text =
            currentChapter.Title;


        numberSpinBox.Value =
            currentChapter.Number;


        startYearSpinBox.Value =
            currentChapter.StartYear;


        startMonthSpinBox.Value =
            currentChapter.StartMonth;
    }


    private void SavePendingFields()
    {
        if (currentChapter == null)
        {
            return;
        }


        currentChapter.Title =
            chapterTitleEdit.Text.Trim();


        currentChapter.Number =
            (int)numberSpinBox.Value;


        currentChapter.StartYear =
            (int)startYearSpinBox.Value;


        currentChapter.StartMonth =
            (int)startMonthSpinBox.Value;
    }


    private ChapterDefinitionData
        GetCurrentChapterState()
    {
        SavePendingFields();


        return currentChapter;
    }


    private void SaveOriginalState()
    {
        if (
            unsavedChangesGuard == null ||
            currentChapter == null)
        {
            return;
        }


        unsavedChangesGuard.SaveOriginalState(
            currentChapter
        );
    }


    public bool HasUnsavedChanges()
    {
        if (unsavedChangesGuard == null)
        {
            return false;
        }


        return unsavedChangesGuard.HasUnsavedChanges();
    }


    public void RequestApplicationClose()
    {
        applicationCloseRequested =
            true;


        pendingChapterAction =
            PendingChapterAction.None;


        if (unsavedChangesGuard == null)
        {
            EmitSignal(
                SignalName.ApplicationCloseConfirmed
            );

            return;
        }


        unsavedChangesGuard.RequestClose();
    }


    private void CloseEditor()
    {
        if (
            pendingChapterAction ==
            PendingChapterAction.CreateNew)
        {
            pendingChapterAction =
                PendingChapterAction.None;


            CreateNewChapter();

            return;
        }


        if (
            pendingChapterAction ==
            PendingChapterAction.Delete)
        {
            pendingChapterAction =
                PendingChapterAction.None;


            DeleteCurrentChapter();

            return;
        }


        if (applicationCloseRequested)
        {
            applicationCloseRequested =
                false;


            EmitSignal(
                SignalName.ApplicationCloseConfirmed
            );


            return;
        }


        EmitSignal(
            SignalName.ChapterCancelled
        );
    }


    private void RequestClose()
    {
        if (unsavedChangesGuard == null)
        {
            CloseEditor();

            return;
        }


        unsavedChangesGuard.RequestClose();
    }


    private void OnNewChapterPressed()
    {
        if (currentChapter == null)
        {
            CreateNewChapter();

            return;
        }


        pendingChapterAction =
            PendingChapterAction.CreateNew;


        RequestClose();
    }


    private void OnDeleteChapterPressed()
    {
        if (
            currentChapter == null ||
            chapterRepository == null)
        {
            return;
        }


        ConfirmationDialog confirmation =
            new ConfirmationDialog();


        confirmation.Title =
            "Eliminar capítulo";


        confirmation.DialogText =
            $"¿Seguro que quieres eliminar el capítulo " +
            $"{currentChapter.Number:D2} · " +
            $"{currentChapter.Title}" +
            "?\n\nEsta acción no se puede deshacer.";


        confirmation.OkButtonText =
            "Eliminar";


        confirmation.CancelButtonText =
            "Cancelar";


        confirmation.Confirmed +=
            () =>
            {
                confirmation.QueueFree();


                pendingChapterAction =
                    PendingChapterAction.Delete;


                RequestClose();
            };


        confirmation.Canceled +=
            () =>
            {
                confirmation.QueueFree();
            };


        AddChild(
            confirmation
        );


        confirmation.PopupCentered();
    }


    private void DeleteCurrentChapter()
    {
        if (
            currentChapter == null ||
            chapterRepository == null)
        {
            return;
        }


        string chapterId =
            currentChapter.Id;


        if (
            creatingNewChapter ||
            !chapterRepository.Exists(
                chapterId))
        {
            EmitSignal(
                SignalName.ChapterCancelled
            );

            return;
        }


        bool deleted =
            chapterRepository.Delete(
                chapterId
            );


        if (!deleted)
        {
            GD.PrintErr(
                "ChapterEditor: no se pudo eliminar el capítulo: ",
                chapterId
            );

            return;
        }


        GD.Print(
            "ChapterEditor: capítulo eliminado: ",
            chapterId
        );


        currentChapter =
            null;


        creatingNewChapter =
            false;


        pendingChapterAction =
            PendingChapterAction.None;


        EmitSignal(
            SignalName.ChapterDeleted
        );
    }


    private void UpdateDeleteButtonState()
    {
        if (deleteButton == null)
        {
            return;
        }


        deleteButton.Disabled =
            currentChapter == null ||
            creatingNewChapter;
    }


    private void LoadChapterContent()
    {
        chapterEvents.Clear();
        chapterInterludes.Clear();


        if (currentChapter == null)
        {
            return;
        }


        if (eventRepository != null)
        {
            chapterEvents =
                eventRepository
                    .LoadAll()
                    .Where(
                        eventData =>
                            eventData != null &&
                            eventData.Chapter ==
                            currentChapter.Number
                    )
                    .OrderBy(
                        eventData =>
                            eventData.Year
                    )
                    .ThenBy(
                        eventData =>
                            eventData.Id,
                        StringComparer.OrdinalIgnoreCase
                    )
                    .ToList();
        }


        if (interludeRepository != null)
        {
            chapterInterludes =
                interludeRepository
                    .LoadAll()
                    .Where(
                        interlude =>
                            interlude != null &&
                            interlude.Chapter ==
                            currentChapter.Number
                    )
                    .OrderBy(
                        interlude =>
                            interlude.Id,
                        StringComparer.OrdinalIgnoreCase
                    )
                    .ToList();
        }
    }


    private void LoadAllAttributes()
    {
        allAttributes.Clear();


        if (attributeDatabase == null)
        {
            return;
        }


        allAttributes =
            attributeDatabase
                .GetAllAttributes()
                .Where(
                    attribute =>
                        attribute != null
                )
                .OrderBy(
                    attribute =>
                        attribute.DisplayName,
                    StringComparer.OrdinalIgnoreCase
                )
                .ToList();
    }


    private void RefreshSelectedAttributes()
    {
        if (selectedAttributeList == null)
        {
            return;
        }


        foreach (
            Node child
            in selectedAttributeList.GetChildren())
        {
            child.QueueFree();
        }


        if (
            currentChapter == null ||
            currentChapter.AttributeIds == null)
        {
            return;
        }


        foreach (
            string attributeId
            in currentChapter.AttributeIds)
        {
            AttributeDefinitionData attribute =
                FindAttribute(
                    attributeId
                );


            if (attribute == null)
            {
                continue;
            }


            CreateSelectedAttributeItem(
                attribute
            );
        }
    }
	public void SaveCurrentChapter()
{
    OnSavePressed();
}


    private void RefreshAttributeList()
    {
        if (attributeList == null)
        {
            return;
        }


        foreach (
            Node child
            in attributeList.GetChildren())
        {
            child.QueueFree();
        }


        if (currentChapter == null)
        {
            return;
        }


        string searchText =
            attributeSearchEdit?.Text.Trim() ?? "";


        foreach (
            AttributeDefinitionData attribute
            in allAttributes)
        {
            if (IsAttributeSelected(
                attribute.Id))
            {
                continue;
            }


            if (!MatchesAttributeSearch(
                attribute,
                searchText))
            {
                continue;
            }


            CreateAvailableAttributeItem(
                attribute
            );
        }
    }


    private bool IsAttributeSelected(
        string attributeId)
    {
        if (
            currentChapter == null ||
            currentChapter.AttributeIds == null)
        {
            return false;
        }


        return currentChapter.AttributeIds.Any(
            id =>
                string.Equals(
                    id,
                    attributeId,
                    StringComparison.OrdinalIgnoreCase
                )
        );
    }


    private bool MatchesAttributeSearch(
        AttributeDefinitionData attribute,
        string searchText)
    {
        if (attribute == null)
        {
            return false;
        }


        if (string.IsNullOrWhiteSpace(searchText))
        {
            return true;
        }


        string[] values =
        {
            attribute.Id ?? "",
            attribute.DisplayName ?? "",
            attribute.Description ?? "",
            attribute.NegativeName ?? "",
            attribute.LowName ?? "",
            attribute.NeutralName ?? "",
            attribute.HighName ?? "",
            attribute.PositiveName ?? ""
        };


        return values.Any(
            value =>
                value.Contains(
                    searchText,
                    StringComparison.OrdinalIgnoreCase
                )
        );
    }


    private AttributeDefinitionData FindAttribute(
        string attributeId)
    {
        if (string.IsNullOrWhiteSpace(attributeId))
        {
            return null;
        }


        return allAttributes.FirstOrDefault(
            attribute =>
                string.Equals(
                    attribute.Id,
                    attributeId,
                    StringComparison.OrdinalIgnoreCase
                )
        );
    }


    private void CreateSelectedAttributeItem(
        AttributeDefinitionData attribute)
    {
        HBoxContainer row =
            new HBoxContainer();


        row.CustomMinimumSize =
            new Vector2(
                0,
                32
            );


        row.AddThemeConstantOverride(
            "separation",
            8
        );


        Label label =
            new Label();


        label.Text =
            GetAttributeDisplayName(
                attribute
            );


        label.SizeFlagsHorizontal =
            Control.SizeFlags.ExpandFill;


        label.VerticalAlignment =
            VerticalAlignment.Center;


        row.AddChild(
            label
        );


        Button removeButton =
            new Button();


        removeButton.Text =
            "×";


        removeButton.CustomMinimumSize =
            new Vector2(
                32,
                30
            );


        removeButton.TooltipText =
            "Quitar atributo";


        removeButton.Pressed +=
            () =>
            {
                RemoveAttribute(
                    attribute.Id
                );
            };


        row.AddChild(
            removeButton
        );


        selectedAttributeList.AddChild(
            row
        );
    }


    private void CreateAvailableAttributeItem(
        AttributeDefinitionData attribute)
    {
        Button button =
            new Button();


        button.CustomMinimumSize =
            new Vector2(
                0,
                48
            );


        button.Alignment =
            HorizontalAlignment.Left;


        button.Text =
            BuildAttributeText(
                attribute
            );


        button.TooltipText =
            attribute.Description ?? "";


        button.Pressed +=
            () =>
            {
                AddAttribute(
                    attribute.Id
                );
            };


        attributeList.AddChild(
            button
        );
    }


    private string BuildAttributeText(
        AttributeDefinitionData attribute)
    {
        string extremes =
            "";


        if (
            !string.IsNullOrWhiteSpace(
                attribute.NegativeName)
            &&
            !string.IsNullOrWhiteSpace(
                attribute.PositiveName))
        {
            extremes =
                $" · {attribute.NegativeName} ↔ " +
                $"{attribute.PositiveName}";
        }


        return
            $"{GetAttributeDisplayName(attribute)}" +
            extremes;
    }


    private string GetAttributeDisplayName(
        AttributeDefinitionData attribute)
    {
        if (attribute == null)
        {
            return "";
        }


        if (!string.IsNullOrWhiteSpace(
            attribute.DisplayName))
        {
            return attribute.DisplayName;
        }


        return attribute.Id;
    }


    private void AddAttribute(
        string attributeId)
    {
        if (currentChapter == null)
        {
            return;
        }


        if (currentChapter.AttributeIds == null)
        {
            currentChapter.AttributeIds =
                new List<string>();
        }


        if (string.IsNullOrWhiteSpace(attributeId))
        {
            return;
        }


        if (IsAttributeSelected(attributeId))
        {
            return;
        }


        currentChapter.AttributeIds.Add(
            attributeId
        );


        RefreshSelectedAttributes();


        RefreshAttributeList();
    }


    private void RemoveAttribute(
        string attributeId)
    {
        if (
            currentChapter == null ||
            currentChapter.AttributeIds == null)
        {
            return;
        }


        currentChapter.AttributeIds.RemoveAll(
            id =>
                string.Equals(
                    id,
                    attributeId,
                    StringComparison.OrdinalIgnoreCase
                )
        );


        RefreshSelectedAttributes();


        RefreshAttributeList();
    }


    private void RefreshContentList()
    {
        if (contentList == null)
        {
            return;
        }


        foreach (
            Node child
            in contentList.GetChildren())
        {
            child.QueueFree();
        }


        string searchText =
            contentSearchEdit?.Text.Trim() ?? "";


        int filter =
            contentFilterButton?.Selected ?? 0;


        if (
            filter == 0 ||
            filter == 1)
        {
            foreach (
                EditorEventData eventData
                in chapterEvents)
            {
                if (!MatchesSearch(
                    eventData.Id,
                    eventData.Title,
                    searchText))
                {
                    continue;
                }


                CreateContentItem(
                    "EVENTO",
                    eventData.Id,
                    eventData.Title,
                    () =>
                    {
                        EmitSignal(
                            SignalName.EventSelected,
                            eventData.Id
                        );
                    }
                );
            }
        }


        if (
            filter == 0 ||
            filter == 2)
        {
            foreach (
                EditorInterludeData interlude
                in chapterInterludes)
            {
                if (!MatchesSearch(
                    interlude.Id,
                    interlude.Title,
                    searchText))
                {
                    continue;
                }


                CreateContentItem(
                    "INTERLUDIO",
                    interlude.Id,
                    interlude.Title,
                    () =>
                    {
                        EmitSignal(
                            SignalName.InterludeSelected,
                            interlude.Id
                        );
                    }
                );
            }
        }
    }


    private bool MatchesSearch(
        string id,
        string title,
        string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return true;
        }


        return
            (id ?? "").Contains(
                searchText,
                StringComparison.OrdinalIgnoreCase
            )
            ||
            (title ?? "").Contains(
                searchText,
                StringComparison.OrdinalIgnoreCase
            );
    }


    private void CreateContentItem(
        string type,
        string id,
        string title,
        Action onPressed)
    {
        Button button =
            new Button();


        button.CustomMinimumSize =
            new Vector2(
                0,
                44
            );


        button.Alignment =
            HorizontalAlignment.Left;


        button.Text =
            $"{type}  ·  {id}    {title}";


        button.TooltipText =
            $"Abrir {type.ToLowerInvariant()}";


        button.Pressed +=
            onPressed;


        contentList.AddChild(
            button
        );
    }


    private void OnAttributeSearchChanged(
        string newText)
    {
        RefreshAttributeList();
    }


    private void OnContentSearchChanged(
        string newText)
    {
        RefreshContentList();
    }


    private void OnContentFilterChanged(
        long index)
    {
        RefreshContentList();
    }


    private void OnEditPagesPressed()
    {
        SavePendingFields();


        EmitSignal(
            SignalName.ChapterPagesSelected,
            GetChapterId()
        );
    }


    private void OnSavePressed()
    {
        if (currentChapter == null)
        {
            GD.PrintErr(
                "ChapterEditor: no hay ningún capítulo cargado."
            );

            return;
        }


        if (chapterRepository == null)
        {
            GD.PrintErr(
                "ChapterEditor: ChapterRepository no está disponible."
            );

            return;
        }


        SavePendingFields();


        bool saved =
            chapterRepository.Save(
                currentChapter
            );


        if (!saved)
        {
            GD.PrintErr(
                "ChapterEditor: no se pudo guardar el capítulo."
            );

            return;
        }


        creatingNewChapter =
            false;


        titleLabel.Text =
            $"Editor de capítulo · " +
            $"{currentChapter.Number:D2}";


        SaveOriginalState();


        UpdateDeleteButtonState();


        EmitSignal(
            SignalName.ChapterSaved
        );
    }


    private void OnCancelPressed()
    {
        RequestClose();
    }
}