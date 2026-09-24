using Godot;
using System;
using System.Collections.Generic;


public partial class EventPageEditor : Control
{
    private const string DragDataPrefix =
        "event_page_editor:";


    private VBoxContainer pageList;
    private Button addPageButton;
    private Button deletePageButton;

    private LineEdit pageTitle;
    private OptionButton pageTypeOption;
    private TextEdit textEdit;

    private Button addIllustrationButton;
    private VBoxContainer illustrationSection;

    private TextureRect illustrationPreview;
    private Button changeIllustrationButton;
    private Button removeIllustrationButton;

    private FileDialog illustrationFileDialog;

    private ScrollContainer pageScroll;
    private ScrollContainer editorScroll;

    private VBoxContainer editorContent;

    private PanelContainer pagesPanel;
    private PanelContainer editorPanel;


    private readonly List<EditorPageData> pages =
        new();


    private int selectedPage = -1;

    private EditorDecisionData selectedDecision;

    private List<EditorPageData> selectedPageOwnerList;

    private bool isLoadingPage = false;


    // ============================================================
    // READY
    // ============================================================

    public override void _Ready()
    {
        pageList =
            GetNode<VBoxContainer>(
                "HBoxContainer/PagesPanel/MarginContainer/VBoxContainer/PageList"
            );

        addPageButton =
            GetNode<Button>(
                "HBoxContainer/PagesPanel/MarginContainer/VBoxContainer/AddPageButton"
            );

        deletePageButton =
            GetNode<Button>(
                "HBoxContainer/PagesPanel/MarginContainer/VBoxContainer/DeletePageButton"
            );


        pageTitle =
            GetNode<LineEdit>(
                "HBoxContainer/EditorPanel/MarginContainer/VBoxContainer/PageTitle"
            );

        pageTypeOption =
            GetNode<OptionButton>(
                "HBoxContainer/EditorPanel/MarginContainer/VBoxContainer/PageTypeOption"
            );

        textEdit =
            GetNode<TextEdit>(
                "HBoxContainer/EditorPanel/MarginContainer/VBoxContainer/TextEdit"
            );


        addIllustrationButton =
            GetNode<Button>(
                "HBoxContainer/EditorPanel/MarginContainer/VBoxContainer/AddIllustrationButton"
            );

        illustrationSection =
            GetNode<VBoxContainer>(
                "HBoxContainer/EditorPanel/MarginContainer/VBoxContainer/IllustrationSection"
            );


        illustrationPreview =
            GetNode<TextureRect>(
                "HBoxContainer/EditorPanel/MarginContainer/VBoxContainer/IllustrationSection/PreviewPanel/Preview"
            );

        changeIllustrationButton =
            GetNode<Button>(
                "HBoxContainer/EditorPanel/MarginContainer/VBoxContainer/IllustrationSection/IllustrationButtons/ChangeIllustrationButton"
            );

        removeIllustrationButton =
            GetNode<Button>(
                "HBoxContainer/EditorPanel/MarginContainer/VBoxContainer/IllustrationSection/IllustrationButtons/RemoveIllustrationButton"
            );


        // El FileDialog está directamente bajo EventPageEditor.
        illustrationFileDialog =
            GetNode<FileDialog>(
                "IllustrationFileDialog"
            );


        pagesPanel =
            GetNode<PanelContainer>(
                "HBoxContainer/PagesPanel"
            );

        editorPanel =
            GetNode<PanelContainer>(
                "HBoxContainer/EditorPanel"
            );


        editorContent =
            GetNode<VBoxContainer>(
                "HBoxContainer/EditorPanel/MarginContainer/VBoxContainer"
            );


        addPageButton.Pressed +=
            OnAddPagePressed;

        deletePageButton.Pressed +=
            OnDeletePagePressed;

        pageTitle.TextChanged +=
            OnPageTitleChanged;

        pageTypeOption.ItemSelected +=
            OnPageTypeSelected;

        textEdit.TextChanged +=
            OnTextChanged;

        addIllustrationButton.Pressed +=
            OnAddIllustrationPressed;

        changeIllustrationButton.Pressed +=
            OnChangeIllustrationPressed;

        removeIllustrationButton.Pressed +=
            OnRemoveIllustrationPressed;

        illustrationFileDialog.FileSelected +=
            OnIllustrationSelected;


        PopulatePageTypes();

        selectedPageOwnerList =
            pages;


        SetupWorkspaceLayout();


        GD.Print(
            "EventPageEditor iniciado."
        );
    }


    // ============================================================
    // LAYOUT
    // ============================================================

    private void SetupWorkspaceLayout()
    {
        pagesPanel.SizeFlagsHorizontal =
            Control.SizeFlags.Fill;

        pagesPanel.SizeFlagsVertical =
            Control.SizeFlags.ExpandFill;

        pagesPanel.CustomMinimumSize =
            new Vector2(
                360,
                0
            );


        editorPanel.SizeFlagsHorizontal =
            Control.SizeFlags.ExpandFill;

        editorPanel.SizeFlagsVertical =
            Control.SizeFlags.ExpandFill;


        // --------------------------------------------------------
        // ÁRBOL
        // --------------------------------------------------------

        if (
            pageList.GetParent()
            is VBoxContainer pageParent)
        {
            pageScroll =
                new ScrollContainer();


            pageScroll.Name =
                "PageScroll";


            pageScroll.SizeFlagsHorizontal =
                Control.SizeFlags.ExpandFill;

            pageScroll.SizeFlagsVertical =
                Control.SizeFlags.ExpandFill;


            pageScroll.HorizontalScrollMode =
                ScrollContainer.ScrollMode.Disabled;


            int index =
                pageList.GetIndex();


            pageParent.RemoveChild(
                pageList
            );


            pageParent.AddChild(
                pageScroll
            );


            pageParent.MoveChild(
                pageScroll,
                index
            );


            pageScroll.AddChild(
                pageList
            );


            pageList.SizeFlagsHorizontal =
                Control.SizeFlags.ExpandFill;

            pageList.SizeFlagsVertical =
                Control.SizeFlags.Fill;
        }


        // --------------------------------------------------------
        // EDITOR
        // --------------------------------------------------------

        if (
            editorContent.GetParent()
            is MarginContainer editorMargin)
        {
            editorScroll =
                new ScrollContainer();


            editorScroll.Name =
                "EditorScroll";


            editorScroll.SizeFlagsHorizontal =
                Control.SizeFlags.ExpandFill;

            editorScroll.SizeFlagsVertical =
                Control.SizeFlags.ExpandFill;


            editorScroll.HorizontalScrollMode =
                ScrollContainer.ScrollMode.Disabled;


            int index =
                editorContent.GetIndex();


            editorMargin.RemoveChild(
                editorContent
            );


            editorMargin.AddChild(
                editorScroll
            );


            editorMargin.MoveChild(
                editorScroll,
                index
            );


            editorScroll.AddChild(
                editorContent
            );


            editorContent.SizeFlagsHorizontal =
                Control.SizeFlags.ExpandFill;
        }


        textEdit.SizeFlagsHorizontal =
            Control.SizeFlags.ExpandFill;

        textEdit.SizeFlagsVertical =
            Control.SizeFlags.ExpandFill;
    }


    // ============================================================
    // TYPES
    // ============================================================

    private void PopulatePageTypes()
    {
        pageTypeOption.Clear();


        pageTypeOption.AddItem(
            "Normal"
        );

        pageTypeOption.AddItem(
            "Decisión"
        );
    }


    // ============================================================
    // DATA
    // ============================================================

    public void SetPages(
        List<EditorPageData> newPages)
    {
        SaveCurrentPage();


        List<EditorPageData> incomingPages =
            newPages != null
                ? new List<EditorPageData>(
                    newPages
                )
                : new List<EditorPageData>();


        pages.Clear();


        pages.AddRange(
            incomingPages
        );


        selectedPage =
            -1;

        selectedDecision =
            null;

        selectedPageOwnerList =
            pages;


        if (pages.Count == 0)
        {
            EditorPageData initialPage =
                new EditorPageData();


            pages.Add(
                initialPage
            );


            selectedPage =
                0;


            GD.Print(
                "EventPageEditor: no había páginas. Se creó una página inicial."
            );
        }


        RenderPageList();


        SelectPage(
            selectedPage,
            pages,
            null
        );


        GD.Print(
            "EventPageEditor: cargadas ",
            pages.Count,
            " páginas."
        );
    }


    public List<EditorPageData> GetPages()
    {
        SaveCurrentPage();


        GD.Print(
            "EventPageEditor: devolviendo ",
            pages.Count,
            " páginas."
        );


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


        PageLocation location =
            FindPageLocation(
                pages,
                pageId
            );


        if (location == null)
        {
            return false;
        }


        selectedPage =
            location.Index;


        selectedPageOwnerList =
            location.OwnerList;


        selectedDecision =
            location.ParentDecision;


        RenderPageList();


        SelectPage(
            selectedPage,
            selectedPageOwnerList,
            selectedDecision
        );


        return true;
    }


    // ============================================================
    // ADD PAGE
    // ============================================================

    private void OnAddPagePressed()
    {
        SaveCurrentPage();


        List<EditorPageData> targetList;


        if (selectedDecision != null)
        {
            if (selectedDecision.Pages == null)
            {
                selectedDecision.Pages =
                    new List<EditorPageData>();
            }


            targetList =
                selectedDecision.Pages;
        }
        else
        {
            targetList =
                selectedPageOwnerList ?? pages;
        }


        EditorPageData newPage =
            new EditorPageData();

if (selectedDecision != null)
{
    targetList.Add(
        newPage
    );
}
else
{
    int insertIndex =
        selectedPage >= 0 &&
        selectedPage < targetList.Count
            ? selectedPage + 1
            : targetList.Count;


    targetList.Insert(
        insertIndex,
        newPage
    );
}


newPage.Title =
    GetDefaultPageTitle(
        newPage
    );


        selectedPageOwnerList =
            targetList;

        selectedPage =
            targetList.IndexOf(
                newPage
            );

        selectedDecision =
            null;


        RenderPageList();


        SelectPage(
            selectedPage,
            selectedPageOwnerList,
            null
        );
    }


    // ============================================================
    // DELETE PAGE
    // ============================================================

    private void OnDeletePagePressed()
    {
        if (
            selectedPageOwnerList == null ||
            selectedPage < 0 ||
            selectedPage >= selectedPageOwnerList.Count)
        {
            return;
        }


        if (
            selectedPageOwnerList == pages &&
            pages.Count <= 1)
        {
            return;
        }


        SaveCurrentPage();


        selectedPageOwnerList.RemoveAt(
            selectedPage
        );


        if (selectedPageOwnerList.Count == 0)
        {
            selectedPage =
                -1;

            selectedDecision =
                null;


            RenderPageList();
            ClearPageEditor();


            return;
        }


        selectedPage =
            Mathf.Clamp(
                selectedPage,
                0,
                selectedPageOwnerList.Count - 1
            );


        selectedDecision =
            null;


        RenderPageList();


        SelectPage(
            selectedPage,
            selectedPageOwnerList,
            null
        );
    }


    // ============================================================
    // TREE
    // ============================================================

    private void RenderPageList()
    {
        foreach (
            Node child
            in pageList.GetChildren())
        {
            child.QueueFree();
        }


        for (
            int i = 0;
            i < pages.Count;
            i++)
        {
            RenderPageRecursive(
                pages[i],
                pages,
                i,
                0
            );
        }


        deletePageButton.Disabled =
            selectedPage < 0 ||
            selectedPageOwnerList == null ||
            (
                selectedPageOwnerList == pages &&
                pages.Count <= 1
            );
    }


    private void RenderPageRecursive(
        EditorPageData page,
        List<EditorPageData> ownerList,
        int index,
        int depth)
    {
        if (page == null)
        {
            return;
        }


        HBoxContainer row =
            CreateRow();


        Label prefix =
            new Label();


        prefix.Text =
            BuildRootPrefix(
                depth
            );


        prefix.VerticalAlignment =
            VerticalAlignment.Center;


        prefix.CustomMinimumSize =
            new Vector2(
                Mathf.Max(
                    0,
                    depth * 22
                ),
                36
            );


        row.AddChild(
            prefix
        );


        PageTreeButton pageButton =
            new PageTreeButton(
                page.Id
            );


        pageButton.Text =
            $"{BuildTreeNumber(ownerList, index)} · {GetPageDisplayName(page, ownerList, index)}";


        pageButton.Alignment =
            HorizontalAlignment.Left;


        pageButton.SizeFlagsHorizontal =
            Control.SizeFlags.ExpandFill;


        pageButton.CustomMinimumSize =
            new Vector2(
                0,
                36
            );


        pageButton.TooltipText =
            page.Type == EditorPageType.Decision
                ? $"Decisión · {page.Decisions?.Count ?? 0} opciones"
                : "Página narrativa";


        pageButton.Pressed += () =>
        {
            SaveCurrentPage();


            selectedPageOwnerList =
                ownerList;

            selectedPage =
                index;

            selectedDecision =
                null;


            LoadPageEditor(
                page
            );
        };


        pageButton.PageDropped +=
            OnPageDropped;


        row.AddChild(
            pageButton
        );


        pageList.AddChild(
            row
        );


        RenderDecisionTree(
            page,
            depth,
            new List<bool>()
        );
    }


    private void RenderDecisionTree(
        EditorPageData page,
        int depth,
        List<bool> ancestorHasNext)
    {
        if (page.Decisions == null)
        {
            return;
        }


        for (
            int d = 0;
            d < page.Decisions.Count;
            d++)
        {
            EditorDecisionData decision =
                page.Decisions[d];


            if (decision == null)
            {
                continue;
            }


            bool isLastDecision =
                d == page.Decisions.Count - 1;


            List<bool> currentPath =
                new List<bool>(
                    ancestorHasNext
                );


            currentPath.Add(
                !isLastDecision
            );


            HBoxContainer decisionRow =
                CreateRow();


            Label prefix =
                new Label();


            prefix.Text =
                BuildTreeBranchPrefix(
                    currentPath,
                    false
                );


            prefix.VerticalAlignment =
                VerticalAlignment.Center;


            prefix.CustomMinimumSize =
                new Vector2(
                    0,
                    32
                );


            decisionRow.AddChild(
                prefix
            );


            Button decisionButton =
                new Button();


            string decisionText =
                string.IsNullOrWhiteSpace(
                    decision.Text)
                    ? "Opción sin texto"
                    : decision.Text.Trim();


            decisionButton.Text =
                $"{GetDecisionLetter(d)} · {decisionText}";


            decisionButton.Alignment =
                HorizontalAlignment.Left;


            decisionButton.SizeFlagsHorizontal =
                Control.SizeFlags.ExpandFill;


            decisionButton.CustomMinimumSize =
                new Vector2(
                    0,
                    32
                );


            decisionButton.TooltipText =
                $"Opción {GetDecisionLetter(d)} · {decision.Pages?.Count ?? 0} páginas";


            int capturedDecisionIndex =
                d;


            EditorPageData capturedPage =
                page;


            decisionButton.Pressed += () =>
            {
                SaveCurrentPage();


                selectedPageOwnerList =
                    FindOwnerList(
                        capturedPage
                    );


                selectedPage =
                    selectedPageOwnerList != null
                        ? selectedPageOwnerList.IndexOf(
                            capturedPage
                        )
                        : -1;


                selectedDecision =
                    capturedPage.Decisions[
                        capturedDecisionIndex
                    ];


                LoadPageEditor(
                    capturedPage
                );
            };


            decisionRow.AddChild(
                decisionButton
            );


            pageList.AddChild(
                decisionRow
            );


            if (decision.Pages == null)
            {
                continue;
            }


            for (
                int p = 0;
                p < decision.Pages.Count;
                p++)
            {
                EditorPageData branchPage =
                    decision.Pages[p];


                if (branchPage == null)
                {
                    continue;
                }


                bool isLastPage =
                    p == decision.Pages.Count - 1;


                List<bool> branchPath =
                    new List<bool>(
                        currentPath
                    );


                branchPath.Add(
                    !isLastPage
                );


                RenderBranchPageRecursive(
                    branchPage,
                    decision.Pages,
                    p,
                    branchPath,
                    decision
                );
            }
        }
    }


    private void RenderBranchPageRecursive(
        EditorPageData page,
        List<EditorPageData> ownerList,
        int index,
        List<bool> path,
        EditorDecisionData parentDecision)
    {
        if (page == null)
        {
            return;
        }


        HBoxContainer row =
            CreateRow();


        Label prefix =
            new Label();


        prefix.Text =
            BuildTreeBranchPrefix(
                path,
                true
            );


        prefix.VerticalAlignment =
            VerticalAlignment.Center;


        prefix.CustomMinimumSize =
            new Vector2(
                0,
                36
            );


        row.AddChild(
            prefix
        );


        PageTreeButton pageButton =
            new PageTreeButton(
                page.Id
            );


        pageButton.Text =
            $"{BuildTreeNumber(ownerList, index)} · {GetPageDisplayName(page, ownerList, index)}";


        pageButton.Alignment =
            HorizontalAlignment.Left;


        pageButton.SizeFlagsHorizontal =
            Control.SizeFlags.ExpandFill;


        pageButton.CustomMinimumSize =
            new Vector2(
                0,
                36
            );


        pageButton.TooltipText =
            page.Type == EditorPageType.Decision
                ? $"Decisión · {page.Decisions?.Count ?? 0} opciones"
                : "Página narrativa";


        int capturedIndex =
            index;


        List<EditorPageData> capturedOwnerList =
            ownerList;


        EditorDecisionData capturedParentDecision =
            parentDecision;


        pageButton.Pressed += () =>
        {
            SaveCurrentPage();


            selectedPageOwnerList =
                capturedOwnerList;


            selectedPage =
                capturedIndex;


            selectedDecision =
                null;


            LoadPageEditor(
                page
            );
        };


        pageButton.PageDropped +=
            OnPageDropped;


        row.AddChild(
            pageButton
        );


        pageList.AddChild(
            row
        );


        RenderNestedDecisions(
            page,
            path
        );
    }


    private void RenderNestedDecisions(
        EditorPageData page,
        List<bool> parentPath)
    {
        if (page.Decisions == null)
        {
            return;
        }


        for (
            int d = 0;
            d < page.Decisions.Count;
            d++)
        {
            EditorDecisionData decision =
                page.Decisions[d];


            if (decision == null)
            {
                continue;
            }


            bool isLastDecision =
                d == page.Decisions.Count - 1;


            List<bool> decisionPath =
                new List<bool>(
                    parentPath
                );


            decisionPath.Add(
                !isLastDecision
            );


            HBoxContainer row =
                CreateRow();


            Label prefix =
                new Label();


            prefix.Text =
                BuildTreeBranchPrefix(
                    decisionPath,
                    false
                );


            prefix.VerticalAlignment =
                VerticalAlignment.Center;


            row.AddChild(
                prefix
            );


            Button decisionButton =
                new Button();


            string decisionText =
                string.IsNullOrWhiteSpace(
                    decision.Text)
                    ? "Opción sin texto"
                    : decision.Text.Trim();


            decisionButton.Text =
                $"{GetDecisionLetter(d)} · {decisionText}";


            decisionButton.Alignment =
                HorizontalAlignment.Left;


            decisionButton.SizeFlagsHorizontal =
                Control.SizeFlags.ExpandFill;


            decisionButton.CustomMinimumSize =
                new Vector2(
                    0,
                    32
                );


            row.AddChild(
                decisionButton
            );


            EditorPageData capturedPage =
                page;


            int capturedDecisionIndex =
                d;


            decisionButton.Pressed += () =>
            {
                SaveCurrentPage();


                selectedPageOwnerList =
                    FindOwnerList(
                        capturedPage
                    );


                selectedPage =
                    selectedPageOwnerList?.IndexOf(
                        capturedPage
                    ) ?? -1;


                selectedDecision =
                    capturedPage.Decisions[
                        capturedDecisionIndex
                    ];


                LoadPageEditor(
                    capturedPage
                );
            };


            pageList.AddChild(
                row
            );


            if (decision.Pages == null)
            {
                continue;
            }


            for (
                int p = 0;
                p < decision.Pages.Count;
                p++)
            {
                EditorPageData branchPage =
                    decision.Pages[p];


                bool isLastPage =
                    p == decision.Pages.Count - 1;


                List<bool> branchPath =
                    new List<bool>(
                        decisionPath
                    );


                branchPath.Add(
                    !isLastPage
                );


                RenderBranchPageRecursive(
                    branchPage,
                    decision.Pages,
                    p,
                    branchPath,
                    decision
                );
            }
        }
    }


    private HBoxContainer CreateRow()
    {
        HBoxContainer row =
            new HBoxContainer();


        row.SizeFlagsHorizontal =
            Control.SizeFlags.ExpandFill;


        row.AddThemeConstantOverride(
            "separation",
            0
        );


        return row;
    }


    private string BuildRootPrefix(
        int depth)
    {
        if (depth <= 0)
        {
            return "";
        }


        return new string(
            ' ',
            depth * 2
        );
    }


    private string BuildTreeBranchPrefix(
        List<bool> path,
        bool page)
    {
        string result = "";


        for (
            int i = 0;
            i < path.Count - 1;
            i++)
        {
            result +=
                path[i]
                    ? "│   "
                    : "    ";
        }


        if (path.Count > 0)
        {
            result +=
                page
                    ? (path[^1]
                        ? "├─ "
                        : "└─ ")
                    : (path[^1]
                        ? "├─ "
                        : "└─ ");
        }


        return result;
    }


    // ============================================================
    // SELECTION / EDITOR
    // ============================================================

    private void SelectPage(
        int index,
        List<EditorPageData> ownerList,
        EditorDecisionData parentDecision)
    {
        if (
            ownerList == null ||
            index < 0 ||
            index >= ownerList.Count)
        {
            DisablePageEditor();


            return;
        }


        selectedPageOwnerList =
            ownerList;

        selectedPage =
            index;

        selectedDecision =
            parentDecision;


        LoadPageEditor(
            ownerList[index]
        );
    }


    private void LoadPageEditor(
    EditorPageData page)
{
    if (page == null)
    {
        DisablePageEditor();
        return;
    }


    isLoadingPage =
        true;


    pageTitle.Editable =
        true;

    pageTypeOption.Disabled =
        false;

    textEdit.Editable =
        true;

    addIllustrationButton.Disabled =
        false;

    changeIllustrationButton.Disabled =
        false;

    removeIllustrationButton.Disabled =
        false;


    // Si una página antigua no tiene nombre, le asignamos
    // inmediatamente uno para que nunca aparezca vacía.
    if (string.IsNullOrWhiteSpace(page.Title))
    {
        page.Title =
            GetDefaultPageTitle(page);
    }


    pageTitle.Text =
        page.Title;


    pageTypeOption.Select(
        page.Type == EditorPageType.Decision
            ? 1
            : 0
    );


    textEdit.Text =
        page.Text ?? "";


    UpdateIllustrationUI(
        page
    );


    isLoadingPage =
        false;


    deletePageButton.Disabled =
        selectedPageOwnerList == null ||
        (
            selectedPageOwnerList == pages &&
            pages.Count <= 1
        );
}
private string GetDefaultPageTitle(
    EditorPageData page)
{
    if (page == null)
    {
        return "Página";
    }


    int index =
        selectedPageOwnerList != null
            ? selectedPageOwnerList.IndexOf(page)
            : -1;


    if (
        selectedPageOwnerList == pages &&
        index == 0)
    {
        return "Introducción";
    }


    if (index >= 0)
    {
        return $"Página {index + 1}";
    }


    return "Página";
}


    private void ClearPageEditor()
    {
        isLoadingPage =
            true;


        pageTitle.Text =
            "";

        pageTypeOption.Select(
            0
        );

        textEdit.Text =
            "";

        illustrationPreview.Texture =
            null;

        illustrationSection.Visible =
            false;


        isLoadingPage =
            false;


        deletePageButton.Disabled =
            true;
    }


    private void DisablePageEditor()
    {
        ClearPageEditor();


        pageTitle.Editable =
            false;

        pageTypeOption.Disabled =
            true;

        textEdit.Editable =
            false;

        addIllustrationButton.Disabled =
            true;

        changeIllustrationButton.Disabled =
            true;

        removeIllustrationButton.Disabled =
            true;
    }


    // ============================================================
    // SAVE
    // ============================================================

    private void SaveCurrentPage()
    {
        if (isLoadingPage)
        {
            return;
        }


        if (
            selectedPageOwnerList == null ||
            selectedPage < 0 ||
            selectedPage >= selectedPageOwnerList.Count)
        {
            return;
        }


        EditorPageData page =
            selectedPageOwnerList[
                selectedPage
            ];


        if (page == null)
        {
            return;
        }


        page.Title =
            pageTitle.Text;

        page.Text =
            textEdit.Text;


        page.Type =
            pageTypeOption.Selected == 1
                ? EditorPageType.Decision
                : EditorPageType.Normal;


        if (page.Decisions == null)
        {
            page.Decisions =
                new List<EditorDecisionData>();
        }
    }


    // ============================================================
    // CHANGES
    // ============================================================

    private void OnPageTitleChanged(
        string newText)
    {
        if (isLoadingPage)
        {
            return;
        }


        if (
            selectedPageOwnerList == null ||
            selectedPage < 0 ||
            selectedPage >= selectedPageOwnerList.Count)
        {
            return;
        }


        EditorPageData page =
            selectedPageOwnerList[
                selectedPage
            ];


        if (page == null)
        {
            return;
        }


        page.Title =
            newText;


        RenderPageList();
    }


    private void OnPageTypeSelected(
        long index)
    {
        if (isLoadingPage)
        {
            return;
        }


        if (
            selectedPageOwnerList == null ||
            selectedPage < 0 ||
            selectedPage >= selectedPageOwnerList.Count)
        {
            return;
        }


        EditorPageData page =
            selectedPageOwnerList[
                selectedPage
            ];


        if (page == null)
        {
            return;
        }


        page.Type =
            index == 1
                ? EditorPageType.Decision
                : EditorPageType.Normal;


        if (page.Decisions == null)
        {
            page.Decisions =
                new List<EditorDecisionData>();
        }


        RenderPageList();
    }


    private void OnTextChanged()
    {
        if (isLoadingPage)
        {
            return;
        }


        if (
            selectedPageOwnerList == null ||
            selectedPage < 0 ||
            selectedPage >= selectedPageOwnerList.Count)
        {
            return;
        }


        EditorPageData page =
            selectedPageOwnerList[
                selectedPage
            ];


        if (page != null)
        {
            page.Text =
                textEdit.Text;
        }
    }


    // ============================================================
    // ILLUSTRATION
    // ============================================================

    private void OnAddIllustrationPressed()
    {
        illustrationFileDialog.PopupCentered();
    }


    private void OnChangeIllustrationPressed()
    {
        illustrationFileDialog.PopupCentered();
    }


    private void OnRemoveIllustrationPressed()
    {
        if (
            selectedPageOwnerList == null ||
            selectedPage < 0 ||
            selectedPage >= selectedPageOwnerList.Count)
        {
            return;
        }


        EditorPageData page =
            selectedPageOwnerList[
                selectedPage
            ];


        if (page == null)
        {
            return;
        }


        page.Illustration =
            "";


        illustrationPreview.Texture =
            null;

        illustrationSection.Visible =
            false;
    }


    private void OnIllustrationSelected(
        string path)
    {
        if (
            selectedPageOwnerList == null ||
            selectedPage < 0 ||
            selectedPage >= selectedPageOwnerList.Count)
        {
            return;
        }


        EditorPageData page =
            selectedPageOwnerList[
                selectedPage
            ];


        if (page == null)
        {
            return;
        }


        page.Illustration =
            path;


        UpdateIllustrationUI(
            page
        );
    }


    private void UpdateIllustrationUI(
        EditorPageData page)
    {
        if (
            page == null ||
            string.IsNullOrWhiteSpace(
                page.Illustration))
        {
            illustrationPreview.Texture =
                null;

            illustrationSection.Visible =
                false;


            return;
        }


        illustrationSection.Visible =
            true;


        Image image =
            Image.LoadFromFile(
                page.Illustration
            );


        if (image == null)
        {
            illustrationPreview.Texture =
                null;


            return;
        }


        illustrationPreview.Texture =
            ImageTexture.CreateFromImage(
                image
            );
    }


    // ============================================================
    // DRAG / DROP
    // ============================================================

    private void OnPageDropped(
        string draggedPageId,
        string targetPageId)
    {
        if (
            string.IsNullOrWhiteSpace(
                draggedPageId) ||
            string.IsNullOrWhiteSpace(
                targetPageId) ||
            draggedPageId == targetPageId)
        {
            return;
        }


        PageLocation dragged =
            FindPageLocation(
                pages,
                draggedPageId
            );


        PageLocation target =
            FindPageLocation(
                pages,
                targetPageId
            );


        if (
            dragged == null ||
            target == null ||
            dragged.OwnerList != target.OwnerList)
        {
            return;
        }


        SaveCurrentPage();


        EditorPageData draggedPage =
            dragged.Page;


        int targetIndex =
            target.Index;


        dragged.OwnerList.Remove(
            draggedPage
        );


        if (dragged.Index < targetIndex)
        {
            targetIndex--;
        }


        targetIndex =
            Mathf.Clamp(
                targetIndex,
                0,
                dragged.OwnerList.Count
            );


        dragged.OwnerList.Insert(
            targetIndex,
            draggedPage
        );


        selectedPageOwnerList =
            dragged.OwnerList;


        selectedPage =
            targetIndex;


        selectedDecision =
            null;


        RenderPageList();


        LoadPageEditor(
            draggedPage
        );
    }


    // ============================================================
    // RENAME
    // ============================================================

    // Ya no existe botón de lápiz.
    // El nombre se edita directamente mediante pageTitle.
    

    // ============================================================
    // HELPERS
    // ============================================================

    private string GetPageDisplayName(
        EditorPageData page,
        List<EditorPageData> ownerList,
        int index)
    {
        if (
            page != null &&
            !string.IsNullOrWhiteSpace(
                page.Title))
        {
            return page.Title.Trim();
        }


        if (
            ownerList == pages &&
            index == 0)
        {
            return "Introducción";
        }


        return $"Página {index + 1}";
    }


    private string BuildTreeNumber(
        List<EditorPageData> ownerList,
        int index)
    {
        if (ownerList == null)
        {
            return "??";
        }


        return (
            index + 1
        ).ToString(
            "00"
        );
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


    private List<EditorPageData> FindOwnerList(
        EditorPageData target)
    {
        if (target == null)
        {
            return null;
        }


        if (pages.Contains(target))
        {
            return pages;
        }


        foreach (
            EditorPageData root
            in pages)
        {
            List<EditorPageData> result =
                FindOwnerListRecursive(
                    root,
                    target
                );


            if (result != null)
            {
                return result;
            }
        }


        return null;
    }


    private List<EditorPageData> FindOwnerListRecursive(
        EditorPageData page,
        EditorPageData target)
    {
        if (
            page == null ||
            page.Decisions == null)
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


            if (decision.Pages.Contains(
                target))
            {
                return decision.Pages;
            }


            foreach (
                EditorPageData child
                in decision.Pages)
            {
                List<EditorPageData> result =
                    FindOwnerListRecursive(
                        child,
                        target
                    );


                if (result != null)
                {
                    return result;
                }
            }
        }


        return null;
    }


    private PageLocation FindPageLocation(
        List<EditorPageData> collection,
        string pageId)
    {
        if (collection == null)
        {
            return null;
        }


        for (
            int i = 0;
            i < collection.Count;
            i++)
        {
            EditorPageData page =
                collection[i];


            if (page == null)
            {
                continue;
            }


            if (page.Id == pageId)
            {
                return new PageLocation
                {
                    Page = page,
                    OwnerList = collection,
                    Index = i,
                    ParentDecision = null
                };
            }


            PageLocation nested =
                FindPageLocationRecursive(
                    page,
                    pageId
                );


            if (nested != null)
            {
                return nested;
            }
        }


        return null;
    }


    private PageLocation FindPageLocationRecursive(
        EditorPageData page,
        string pageId)
    {
        if (
            page == null ||
            page.Decisions == null)
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


            for (
                int i = 0;
                i < decision.Pages.Count;
                i++)
            {
                EditorPageData child =
                    decision.Pages[i];


                if (child == null)
                {
                    continue;
                }


                if (child.Id == pageId)
                {
                    return new PageLocation
                    {
                        Page = child,
                        OwnerList = decision.Pages,
                        Index = i,
                        ParentDecision = decision
                    };
                }


                PageLocation nested =
                    FindPageLocationRecursive(
                        child,
                        pageId
                    );


                if (nested != null)
                {
                    return nested;
                }
            }
        }


        return null;
    }


    private sealed class PageLocation
    {
        public EditorPageData Page;
        public List<EditorPageData> OwnerList;
        public int Index;
        public EditorDecisionData ParentDecision;
    }


    // ============================================================
    // DRAG BUTTON
    // ============================================================

    private sealed partial class PageTreeButton : Button
    {
        public event Action<string, string> PageDropped;


        private readonly string pageId;

        private bool pointerDown;
        private Vector2 pointerStart;


        public PageTreeButton(
            string id)
        {
            pageId =
                id;


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
                @event is InputEventMouseMotion mouseMotion &&
                pointerDown)
            {
                float distance =
                    pointerStart.DistanceTo(
                        mouseMotion.Position
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
                        DragDataPrefix + pageId,
                        preview
                    );
                }
            }
        }


        public override Variant _GetDragData(
            Vector2 atPosition)
        {
            Label preview =
                new Label();


            preview.Text =
                Text;


            return
                DragDataPrefix +
                pageId;
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


            string value =
                data.AsString();


            return value.StartsWith(
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


            string draggedPageId =
                value.Substring(
                    DragDataPrefix.Length
                );


            PageDropped?.Invoke(
                draggedPageId,
                pageId
            );
        }
    }
}