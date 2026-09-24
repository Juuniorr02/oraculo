using Godot;
using System;
using System.Collections.Generic;


public partial class OptionsEditor : Control
{
    private const string DragDataPrefix =
        "event_option_editor:";


    private OptionButton pageOption;
    private VBoxContainer optionList;

    private Button addOptionButton;
    private Button deleteOptionButton;
    private Button addPageToBranchButton;

    private DecisionEditor decisionEditor;


    private readonly List<EditorDecisionData> decisions =
        new();


    private List<EditorPageData> pages =
        new();


    private EditorPageData selectedPage;

    private int selectedDecision = -1;

    private int chapter = 1;

    private EventRepository eventRepository;

    private string projectPath = "";

    private bool isLoadingPage = false;


    // ============================================================
    // READY
    // ============================================================

    public override void _Ready()
    {
        pageOption =
            GetNode<OptionButton>(
                "HBoxContainer/OptionsPanel/MarginContainer/VBoxContainer/PageOption"
            );


        optionList =
            GetNode<VBoxContainer>(
                "HBoxContainer/OptionsPanel/MarginContainer/VBoxContainer/OptionList"
            );


        addOptionButton =
            GetNode<Button>(
                "HBoxContainer/OptionsPanel/MarginContainer/VBoxContainer/Buttons/AddOptionButton"
            );


        deleteOptionButton =
            GetNode<Button>(
                "HBoxContainer/OptionsPanel/MarginContainer/VBoxContainer/Buttons/DeleteOptionButton"
            );


        decisionEditor =
            GetNode<DecisionEditor>(
                "HBoxContainer/EditorPanel/ScrollContainer/MarginContainer/DecisionEditor"
            );


        // El botón no necesita existir en la escena.
        // Lo añadimos al contenedor de botones desde código.
        Node buttonsContainer =
            GetNode<Node>(
                "HBoxContainer/OptionsPanel/MarginContainer/VBoxContainer/Buttons"
            );


        addPageToBranchButton =
            new Button();


        addPageToBranchButton.Text =
            "+ Página en rama";


        addPageToBranchButton.TooltipText =
            "Añadir una nueva página dentro de la opción seleccionada.";


        addPageToBranchButton.CustomMinimumSize =
            new Vector2(
                150,
                38
            );


        buttonsContainer.AddChild(
            addPageToBranchButton
        );


        pageOption.ItemSelected +=
            OnPageSelected;


        addOptionButton.Pressed +=
            OnAddOptionPressed;


        deleteOptionButton.Pressed +=
            OnDeleteOptionPressed;


        addPageToBranchButton.Pressed +=
            OnAddPageToBranchPressed;


        GD.Print(
            "OptionsEditor iniciado."
        );
    }


    // ============================================================
    // CONFIGURATION
    // ============================================================

    public void SetRepository(
        EventRepository repository)
    {
        eventRepository =
            repository;


        if (decisionEditor != null)
        {
            decisionEditor.SetRepository(
                eventRepository
            );
        }
    }


    public void SetProjectPath(
        string path)
    {
        projectPath =
            path ?? "";


        if (decisionEditor != null)
        {
            decisionEditor.SetProjectPath(
                projectPath
            );
        }
    }


    public void SetChapter(
        int value)
    {
        chapter =
            value;


        if (decisionEditor != null)
        {
            decisionEditor.SetChapter(
                chapter
            );
        }
    }


    // ============================================================
    // PAGES
    // ============================================================

    public void SetPages(
        List<EditorPageData> newPages)
    {
        SaveCurrentPageDecisions();


        pages =
            newPages != null
                ? new List<EditorPageData>(
                    newPages
                )
                : new List<EditorPageData>();


        selectedPage =
            null;


        selectedDecision =
            -1;


        PopulateDecisionPages();


        if (pageOption.ItemCount > 0)
        {
            pageOption.Select(
                0
            );


            LoadPageByIndex(
                0
            );
        }
        else
        {
            ClearEditor();
        }
    }


    public List<EditorPageData> GetPages()
    {
        SaveCurrentPageDecisions();


        return pages;
    }


    public bool SelectPageById(
        string pageId)
    {
        if (string.IsNullOrWhiteSpace(
            pageId))
        {
            return false;
        }


        int index =
            FindDecisionPageIndexById(
                pageId
            );


        if (index < 0)
        {
            return false;
        }


        pageOption.Select(
            index
        );


        LoadPageByIndex(
            index
        );


        return true;
    }


    // ============================================================
    // DECISION PAGES
    // ============================================================

    private void PopulateDecisionPages()
    {
        isLoadingPage =
            true;


        pageOption.Clear();


        List<PageEntry> entries =
            new();


        foreach (
            EditorPageData page
            in pages)
        {
            CollectDecisionPages(
                page,
                "",
                entries
            );
        }


        foreach (
            PageEntry entry
            in entries)
        {
            int index =
                pageOption.ItemCount;


            pageOption.AddItem(
                entry.Path
            );


            pageOption.SetItemMetadata(
                index,
                entry.Page.Id
            );
        }


        isLoadingPage =
            false;


        GD.Print(
            "OptionsEditor: páginas de decisión encontradas: ",
            pageOption.ItemCount
        );
    }


    private void CollectDecisionPages(
        EditorPageData page,
        string parentPath,
        List<PageEntry> result)
    {
        if (page == null)
        {
            return;
        }


        string pageName =
            GetPageDisplayName(
                page
            );


        string currentPath =
            string.IsNullOrWhiteSpace(
                parentPath)
                ? pageName
                : parentPath +
                  " / " +
                  pageName;


        // Una página marcada como Decisión aparece aunque
        // todavía tenga 0 opciones.
        if (
            page.Type ==
            EditorPageType.Decision)
        {
            result.Add(
                new PageEntry(
                    page,
                    currentPath
                )
            );
        }


        if (page.Decisions == null)
        {
            return;
        }


        for (
            int i = 0;
            i < page.Decisions.Count;
            i++)
        {
            EditorDecisionData decision =
                page.Decisions[i];


            if (decision == null)
            {
                continue;
            }


            if (decision.Pages == null)
            {
                decision.Pages =
                    new List<EditorPageData>();
            }


            string decisionText =
                string.IsNullOrWhiteSpace(
                    decision.Text)
                    ? $"Opción {GetDecisionLetter(i)}"
                    : decision.Text.Trim();


            string decisionPath =
                currentPath +
                " / " +
                GetDecisionLetter(i) +
                " · " +
                decisionText;


            foreach (
                EditorPageData branchPage
                in decision.Pages)
            {
                CollectDecisionPages(
                    branchPage,
                    decisionPath,
                    result
                );
            }
        }
    }


    private string GetPageDisplayName(
        EditorPageData page)
    {
        if (page == null)
        {
            return "Página";
        }


        if (!string.IsNullOrWhiteSpace(
            page.Title))
        {
            return page.Title.Trim();
        }


        return "Página";
    }


    private int FindDecisionPageIndexById(
        string pageId)
    {
        for (
            int i = 0;
            i < pageOption.ItemCount;
            i++)
        {
            Variant metadata =
                pageOption.GetItemMetadata(
                    i
                );


            if (
                metadata.VariantType !=
                Variant.Type.Nil &&
                metadata.AsString() == pageId)
            {
                return i;
            }
        }


        return -1;
    }


    private EditorPageData FindPageById(
        string pageId)
    {
        if (string.IsNullOrWhiteSpace(
            pageId))
        {
            return null;
        }


        foreach (
            EditorPageData page
            in pages)
        {
            EditorPageData found =
                FindPageByIdRecursive(
                    page,
                    pageId
                );


            if (found != null)
            {
                return found;
            }
        }


        return null;
    }


    private EditorPageData FindPageByIdRecursive(
        EditorPageData page,
        string pageId)
    {
        if (page == null)
        {
            return null;
        }


        if (page.Id == pageId)
        {
            return page;
        }


        if (page.Decisions == null)
        {
            return null;
        }


        foreach (
            EditorDecisionData decision
            in page.Decisions)
        {
            if (
                decision == null ||
                decision.Pages == null)
            {
                continue;
            }


            foreach (
                EditorPageData branchPage
                in decision.Pages)
            {
                EditorPageData found =
                    FindPageByIdRecursive(
                        branchPage,
                        pageId
                    );


                if (found != null)
                {
                    return found;
                }
            }
        }


        return null;
    }


    // ============================================================
    // SAVE CURRENT PAGE / DECISION
    // ============================================================

    private void SaveCurrentPageDecisions()
    {
        SaveCurrentDecision();
    }


    private void SaveCurrentDecision()
    {
        if (
            selectedDecision < 0 ||
            selectedDecision >= decisions.Count ||
            decisionEditor == null)
        {
            return;
        }


        EditorDecisionData decision =
            decisions[selectedDecision];


        EnsureDecisionStructure(
            decision
        );


        decisionEditor.SaveCurrentDecision();


        decision.Text =
            decisionEditor.GetDecisionText();


        decision.Description =
            decisionEditor.GetDecisionDescription();


        decision.Conditions =
            decisionEditor.GetConditions();


        decision.Effects =
            decisionEditor.GetEffects();


        decisions[selectedDecision] =
            decision;
    }


    // ============================================================
    // PAGE SELECTION
    // ============================================================

    private void OnPageSelected(
        long index)
    {
        if (isLoadingPage)
        {
            return;
        }


        if (
            index < 0 ||
            index >= pageOption.ItemCount)
        {
            return;
        }


        SaveCurrentDecision();


        LoadPageByIndex(
            (int)index
        );
    }


    private void LoadPageByIndex(
        int index)
    {
        if (
            index < 0 ||
            index >= pageOption.ItemCount)
        {
            ClearEditor();


            return;
        }


        Variant metadata =
            pageOption.GetItemMetadata(
                index
            );


        string pageId =
            metadata.AsString();


        selectedPage =
            FindPageById(
                pageId
            );


        if (selectedPage == null)
        {
            ClearEditor();


            return;
        }


        LoadDecisionsFromPage(
            selectedPage
        );
    }


    private void LoadDecisionsFromPage(
        EditorPageData page)
    {
        decisions.Clear();


        if (page.Decisions == null)
        {
            page.Decisions =
                new List<EditorDecisionData>();
        }


        foreach (
            EditorDecisionData decision
            in page.Decisions)
        {
            if (decision == null)
            {
                continue;
            }


            EnsureDecisionStructure(
                decision
            );


            decisions.Add(
                decision
            );
        }


        selectedDecision =
            decisions.Count > 0
                ? 0
                : -1;


        RefreshOptionList();


        if (selectedDecision >= 0)
        {
            LoadSelectedDecision();
        }
        else
        {
            decisionEditor.LoadDecision(
                -1,
                ""
            );
        }


        UpdateActionButtons();
    }


    // ============================================================
    // OPTION LIST
    // ============================================================

    private void RefreshOptionList()
    {
        foreach (
            Node child
            in optionList.GetChildren())
        {
            child.QueueFree();
        }


        for (
            int i = 0;
            i < decisions.Count;
            i++)
        {
            EditorDecisionData decision =
                decisions[i];


            EnsureDecisionStructure(
                decision
            );


            OptionDragButton optionButton =
                new OptionDragButton(
                    i
                );
                optionButton.ToggleMode =
    true;


optionButton.ButtonPressed =
    i == selectedDecision;


            string letter =
                GetDecisionLetter(i);


            string text =
                string.IsNullOrWhiteSpace(
                    decision.Text)
                    ? $"Opción {letter}"
                    : decision.Text.Trim();


            int branchCount =
                decision.Pages.Count;


            optionButton.Text =
                $"{letter} · {text}";


            optionButton.TooltipText =
                branchCount > 0
                    ? $"{branchCount} página(s) en esta rama."
                    : "Esta opción todavía no tiene páginas.";


            optionButton.SizeFlagsHorizontal =
                Control.SizeFlags.ExpandFill;


            optionButton.CustomMinimumSize =
                new Vector2(
                    0,
                    38
                );


            if (i == selectedDecision)
            {
                optionButton.ButtonPressed =
                    true;
            }


            int capturedIndex =
                i;


            optionButton.Pressed += () =>
            {
                SelectDecisionByIndex(
                    capturedIndex
                );
            };


            optionButton.OptionDropped +=
                OnOptionDropped;


            optionList.AddChild(
                optionButton
            );
        }


        UpdateActionButtons();
    }


    private void UpdateActionButtons()
    {
        deleteOptionButton.Disabled =
            selectedDecision < 0 ||
            selectedDecision >= decisions.Count;


        addPageToBranchButton.Disabled =
            selectedDecision < 0 ||
            selectedDecision >= decisions.Count;
    }


    public bool SelectDecisionByIndex(
        int index)
    {
        if (
            index < 0 ||
            index >= decisions.Count)
        {
            return false;
        }


        SaveCurrentDecision();


        selectedDecision =
            index;


        RefreshOptionList();


        LoadSelectedDecision();


        return true;
    }


    private void LoadSelectedDecision()
    {
        if (
            selectedDecision < 0 ||
            selectedDecision >= decisions.Count)
        {
            return;
        }


        EditorDecisionData decision =
            decisions[selectedDecision];


        EnsureDecisionStructure(
            decision
        );


        decisionEditor.SetPages(
            pages
        );


        decisionEditor.SetChapter(
            chapter
        );


        decisionEditor.SetRepository(
            eventRepository
        );


        decisionEditor.SetProjectPath(
            projectPath
        );


        decisionEditor.LoadDecisionData(
            decision
        );
    }


    // ============================================================
    // ADD OPTION
    // ============================================================

    private void OnAddOptionPressed()
    {
        if (selectedPage == null)
        {
            GD.PrintErr(
                "OptionsEditor: no hay página de decisión seleccionada."
            );


            return;
        }


        SaveCurrentDecision();


        if (selectedPage.Decisions == null)
        {
            selectedPage.Decisions =
                new List<EditorDecisionData>();
        }


        EditorDecisionData newDecision =
            new EditorDecisionData
            {
                Text = "",
                Description = "",
                Conditions =
                    new List<EditorConditionData>(),
                Effects =
                    new List<EditorEffectData>(),
                Pages =
                    new List<EditorPageData>(),
                NextPageId = ""
            };


        selectedPage.Decisions.Add(
            newDecision
        );


        decisions.Add(
            newDecision
        );


        selectedDecision =
            decisions.Count - 1;


        RefreshOptionList();


        LoadSelectedDecision();


        GD.Print(
            "OptionsEditor: nueva opción añadida."
        );
    }


    // ============================================================
    // ADD PAGE TO BRANCH
    // ============================================================

    private void OnAddPageToBranchPressed()
    {
        if (
            selectedDecision < 0 ||
            selectedDecision >= decisions.Count)
        {
            return;
        }


        SaveCurrentDecision();


        EditorDecisionData decision =
            decisions[selectedDecision];


        EnsureDecisionStructure(
            decision
        );


        EditorPageData newPage =
            new EditorPageData();


        decision.Pages.Add(
            newPage
        );


        // La primera página de la rama es automáticamente
        // el destino inicial de la opción.
        if (string.IsNullOrWhiteSpace(
            decision.NextPageId))
        {
            decision.NextPageId =
                newPage.Id;
        }


        decisions[selectedDecision] =
            decision;


        RefreshOptionList();


        LoadSelectedDecision();


        GD.Print(
            "OptionsEditor: página añadida a la rama de ",
            GetDecisionLetter(selectedDecision),
            "."
        );
    }


    // ============================================================
    // DELETE OPTION
    // ============================================================

    private void OnDeleteOptionPressed()
    {
        if (
            selectedPage == null ||
            selectedDecision < 0 ||
            selectedDecision >= decisions.Count)
        {
            return;
        }


        SaveCurrentDecision();


        EditorDecisionData decision =
            decisions[selectedDecision];


        selectedPage.Decisions.Remove(
            decision
        );


        decisions.RemoveAt(
            selectedDecision
        );


        if (decisions.Count == 0)
        {
            selectedDecision =
                -1;


            RefreshOptionList();


            decisionEditor.LoadDecision(
                -1,
                ""
            );


            UpdateActionButtons();


            GD.Print(
                "OptionsEditor: se ha eliminado la última opción."
            );


            return;
        }


        if (
            selectedDecision >=
            decisions.Count)
        {
            selectedDecision =
                decisions.Count - 1;
        }


        RefreshOptionList();


        LoadSelectedDecision();


        GD.Print(
            "OptionsEditor: opción eliminada."
        );
    }


    // ============================================================
    // REORDER OPTIONS
    // ============================================================

    private void OnOptionDropped(
        int draggedIndex,
        int targetIndex)
    {
        if (
            draggedIndex < 0 ||
            draggedIndex >= decisions.Count ||
            targetIndex < 0 ||
            targetIndex >= decisions.Count ||
            draggedIndex == targetIndex)
        {
            return;
        }


        SaveCurrentDecision();


        EditorDecisionData draggedDecision =
            decisions[draggedIndex];


        decisions.RemoveAt(
            draggedIndex
        );


        if (draggedIndex < targetIndex)
        {
            targetIndex--;
        }


        targetIndex =
            Mathf.Clamp(
                targetIndex,
                0,
                decisions.Count
            );


        decisions.Insert(
            targetIndex,
            draggedDecision
        );


        if (selectedPage != null)
        {
            selectedPage.Decisions =
                new List<EditorDecisionData>(
                    decisions
                );
        }


        selectedDecision =
            targetIndex;


        RefreshOptionList();


        LoadSelectedDecision();


        GD.Print(
            "OptionsEditor: opciones reordenadas."
        );
    }


    // ============================================================
    // CLEAR
    // ============================================================

    private void ClearEditor()
    {
        decisions.Clear();


        selectedPage =
            null;


        selectedDecision =
            -1;


        RefreshOptionList();


        decisionEditor.LoadDecision(
            -1,
            ""
        );


        UpdateActionButtons();
    }


    // ============================================================
    // HELPERS
    // ============================================================

    private void EnsureDecisionStructure(
        EditorDecisionData decision)
    {
        if (decision == null)
        {
            return;
        }


        if (decision.Conditions == null)
        {
            decision.Conditions =
                new List<EditorConditionData>();
        }


        if (decision.Effects == null)
        {
            decision.Effects =
                new List<EditorEffectData>();
        }


        if (decision.Pages == null)
        {
            decision.Pages =
                new List<EditorPageData>();
        }
    }


    private string GetDecisionLetter(
        int index)
    {
        if (index < 26)
        {
            return (
                (char)('A' + index)
            ).ToString();
        }


        int first =
            index / 26;

        int second =
            index % 26;


        return
            ((char)('A' + first - 1)).ToString() +
            ((char)('A' + second)).ToString();
    }


    // ============================================================
    // INTERNAL ENTRY
    // ============================================================

    private sealed class PageEntry
    {
        public EditorPageData Page { get; }

        public string Path { get; }


        public PageEntry(
            EditorPageData page,
            string path)
        {
            Page =
                page;

            Path =
                path;
        }
    }


    // ============================================================
    // DRAG BUTTON
    // ============================================================

    private sealed partial class OptionDragButton : Button
    {
        public event Action<int, int> OptionDropped;


        private readonly int optionIndex;


        private bool pointerDown = false;
        private Vector2 pointerStart;


        public OptionDragButton(
            int index)
        {
            optionIndex =
                index;


            MouseDefaultCursorShape =
                CursorShape.Drag;
        }


        public override void _GuiInput(
            InputEvent @event)
        {
            if (
                @event is InputEventMouseButton mouseButton &&
                mouseButton.ButtonIndex ==
                MouseButton.Left)
            {
                if (mouseButton.Pressed)
                {
                    pointerDown =
                        true;

                    pointerStart =
                        mouseButton.Position;
                }
                else
                {
                    pointerDown =
                        false;
                }
            }


            if (
                @event is InputEventMouseMotion motion &&
                pointerDown)
            {
                float distance =
                    pointerStart.DistanceTo(
                        motion.Position
                    );


                if (distance >= 8.0f)
                {
                    pointerDown =
                        false;


                    Label preview =
                        new Label();


                    preview.Text =
                        Text;


                    preview.CustomMinimumSize =
                        new Vector2(
                            220,
                            36
                        );


                    ForceDrag(
                        DragDataPrefix +
                        optionIndex,
                        preview
                    );
                }
            }
        }


        public override bool _CanDropData(
            Vector2 atPosition,
            Variant data)
        {
            if (
                data.VariantType !=
                Variant.Type.String)
            {
                return false;
            }


            return
                data.AsString().StartsWith(
                    DragDataPrefix
                );
        }


        public override void _DropData(
            Vector2 atPosition,
            Variant data)
        {
            if (
                data.VariantType !=
                Variant.Type.String)
            {
                return;
            }


            string value =
                data.AsString();


            if (!value.StartsWith(
                DragDataPrefix))
            {
                return;
            }


            string indexText =
                value.Substring(
                    DragDataPrefix.Length
                );


            if (!int.TryParse(
                indexText,
                out int draggedIndex))
            {
                return;
            }


            OptionDropped?.Invoke(
                draggedIndex,
                optionIndex
            );
        }
    }
}