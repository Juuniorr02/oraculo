using Godot;
using System;
using System.Collections.Generic;


public partial class ChapterPageEditor : Control
{
    private const string DragDataPrefix =
        "chapter_page_editor:";


    [Signal]
    public delegate void PagesSavedEventHandler();


    [Signal]
    public delegate void PagesCancelledEventHandler();


    [Signal]
    public delegate void ApplicationCloseConfirmedEventHandler();


    private VBoxContainer pageList;
    private Button addPageButton;
    private Button deletePageButton;


    private LineEdit pageTitleEdit;
    private OptionButton pageTypeOption;
    private TextEdit textEdit;
    private LineEdit buttonTextEdit;


    private Button addIllustrationButton;
    private VBoxContainer illustrationSection;
    private TextureRect illustrationPreview;
    private Button changeIllustrationButton;
    private Button removeIllustrationButton;
    private FileDialog illustrationFileDialog;


    private Button saveButton;
    private Button cancelButton;


    private readonly List<ChapterPageDefinitionData> pages =
        new();


    private int selectedPage = -1;
    private bool isLoadingPage;


    private UnsavedChangesGuard<
        List<ChapterPageDefinitionData>
    > unsavedChangesGuard;


    private bool applicationCloseRequested = false;


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


        pageTitleEdit =
            GetNode<LineEdit>(
                "HBoxContainer/EditorPanel/MarginContainer/VBoxContainer/PageTitleEdit"
            );


        pageTypeOption =
            GetNode<OptionButton>(
                "HBoxContainer/EditorPanel/MarginContainer/VBoxContainer/PageTypeOption"
            );


        textEdit =
            GetNode<TextEdit>(
                "HBoxContainer/EditorPanel/MarginContainer/VBoxContainer/TextEdit"
            );


        buttonTextEdit =
            GetNode<LineEdit>(
                "HBoxContainer/EditorPanel/MarginContainer/VBoxContainer/ButtonTextEdit"
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


        illustrationFileDialog =
            GetNode<FileDialog>(
                "IllustrationFileDialog"
            );


        saveButton =
            GetNode<Button>(
                "HBoxContainer/EditorPanel/MarginContainer/VBoxContainer/PageButtons/SaveButton"
            );


        cancelButton =
            GetNode<Button>(
                "HBoxContainer/EditorPanel/MarginContainer/VBoxContainer/PageButtons/CancelButton"
            );


        addPageButton.Pressed +=
            OnAddPagePressed;


        deletePageButton.Pressed +=
            OnDeletePagePressed;


        pageTypeOption.ItemSelected +=
            OnPageTypeSelected;


        pageTitleEdit.TextChanged +=
            OnPageTitleChanged;


        textEdit.TextChanged +=
            OnTextChanged;


        buttonTextEdit.TextChanged +=
            OnButtonTextChanged;


        addIllustrationButton.Pressed +=
            OpenIllustrationPicker;


        changeIllustrationButton.Pressed +=
            OpenIllustrationPicker;


        removeIllustrationButton.Pressed +=
            OnRemoveIllustrationPressed;


        illustrationFileDialog.FileSelected +=
            OnIllustrationFileSelected;


        saveButton.Pressed +=
            OnSavePressed;


        cancelButton.Pressed +=
            OnCancelPressed;


        illustrationFileDialog.AddFilter(
            "*.png, *.jpg, *.jpeg, *.webp ; Archivos de imagen"
        );


        pageTypeOption.Clear();

        pageTypeOption.AddItem(
            "Información"
        );


        unsavedChangesGuard =
            new UnsavedChangesGuard<
                List<ChapterPageDefinitionData>
            >(
                this,
                GetCurrentPagesState,
                CloseEditor
            );


        CreateInitialPage();


        GD.Print(
            "ChapterPageEditor iniciado."
        );
    }


    public void SetPages(
        List<ChapterPageDefinitionData> newPages)
    {
        pages.Clear();


        if (newPages != null)
        {
            foreach (
                ChapterPageDefinitionData page
                in newPages)
            {
                if (page == null)
                {
                    continue;
                }


                pages.Add(
                    ClonePage(
                        page
                    )
                );
            }
        }


        if (pages.Count == 0)
        {
            CreateInitialPage();


            SaveOriginalState();


            return;
        }


        selectedPage =
            0;


        RefreshPageList();

        LoadSelectedPage();


        SaveOriginalState();
    }


    public List<ChapterPageDefinitionData> GetPages()
    {
        SaveCurrentPage();


        List<ChapterPageDefinitionData> result =
            new();


        foreach (
            ChapterPageDefinitionData page
            in pages)
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


    private List<ChapterPageDefinitionData>
        GetCurrentPagesState()
    {
        SaveCurrentPage();


        List<ChapterPageDefinitionData> result =
            new();


        foreach (
            ChapterPageDefinitionData page
            in pages)
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


    private void SaveOriginalState()
    {
        if (unsavedChangesGuard == null)
        {
            return;
        }


        unsavedChangesGuard.SaveOriginalState(
            GetCurrentPagesState()
        );
    }
	public void SaveCurrentPages()
{
    OnSavePressed();
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
            SignalName.PagesCancelled
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


    private void CreateInitialPage()
    {
        pages.Clear();


        pages.Add(
            new ChapterPageDefinitionData()
        );


        selectedPage =
            0;


        RefreshPageList();

        LoadSelectedPage();
    }


    private void OnAddPagePressed()
    {
        SaveCurrentPage();


        ChapterPageDefinitionData newPage =
            new ChapterPageDefinitionData();


        pages.Add(
            newPage
        );


        RefreshPageList();


        SelectPage(
            pages.Count - 1
        );
    }


    private void OnDeletePagePressed()
    {
        if (
            pages.Count <= 1 ||
            selectedPage < 0 ||
            selectedPage >= pages.Count)
        {
            return;
        }


        SaveCurrentPage();


        int deletedPage =
            selectedPage;


        pages.RemoveAt(
            deletedPage
        );


        selectedPage =
            Math.Min(
                deletedPage,
                pages.Count - 1
            );


        RefreshPageList();

        LoadSelectedPage();
    }


    private void RefreshPageList()
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
            ChapterPageDefinitionData page =
                pages[i];


            string pageTitle =
                string.IsNullOrWhiteSpace(
                    page.Title)
                    ? "Sin título"
                    : page.Title.Trim();


            PageDragButton pageButton =
                new PageDragButton
                {
                    Text =
                        $"Página {i + 1}  ·  {pageTitle}",

                    PageIndex =
                        i,

                    CustomMinimumSize =
                        new Vector2(
                            0,
                            38
                        ),

                    Alignment =
                        HorizontalAlignment.Left,

                    ToggleMode =
                        true,

                    ButtonPressed =
                        i == selectedPage
                };


            pageButton.Pressed +=
                () =>
                {
                    SelectPage(
                        pageButton.PageIndex
                    );
                };


            pageButton.DragStarted +=
                OnPageDragStarted;


            pageButton.PageDropped +=
                OnPageDropped;


            pageList.AddChild(
                pageButton
            );
        }


        deletePageButton.Disabled =
            pages.Count <= 1;
    }


    private void SelectPage(
        int index)
    {
        if (
            index < 0 ||
            index >= pages.Count)
        {
            return;
        }


        SaveCurrentPage();


        selectedPage =
            index;


        RefreshPageList();

        LoadSelectedPage();
    }


    private void LoadSelectedPage()
    {
        if (
            selectedPage < 0 ||
            selectedPage >= pages.Count)
        {
            return;
        }


        ChapterPageDefinitionData page =
            pages[selectedPage];


        isLoadingPage =
            true;


        pageTitleEdit.Text =
            page.Title;


        textEdit.Text =
            page.Text;


        buttonTextEdit.Text =
            page.ButtonText;


        LoadPageType(
            page.Type
        );


        isLoadingPage =
            false;


        UpdateIllustrationUi(
            page
        );
    }


    private void LoadPageType(
        string type)
    {
        if (
            string.Equals(
                type,
                "Information",
                StringComparison.OrdinalIgnoreCase
            ))
        {
            pageTypeOption.Select(
                0
            );


            return;
        }


        pageTypeOption.Select(
            0
        );
    }


    private void OnPageTypeSelected(
        long index)
    {
        if (
            isLoadingPage ||
            selectedPage < 0 ||
            selectedPage >= pages.Count)
        {
            return;
        }


        ChapterPageDefinitionData page =
            pages[selectedPage];


        if (index == 0)
        {
            page.Type =
                "Information";
        }
    }


    private void SaveCurrentPage()
    {
        if (
            selectedPage < 0 ||
            selectedPage >= pages.Count)
        {
            return;
        }


        ChapterPageDefinitionData page =
            pages[selectedPage];


        page.Title =
            pageTitleEdit.Text;


        page.Text =
            textEdit.Text;


        page.ButtonText =
            buttonTextEdit.Text;
    }


    private void OnPageTitleChanged(
        string newText)
    {
        if (!isLoadingPage)
        {
            SaveCurrentPage();
        }
    }


    private void OnTextChanged()
    {
        if (!isLoadingPage)
        {
            SaveCurrentPage();
        }
    }


    private void OnButtonTextChanged(
        string newText)
    {
        if (!isLoadingPage)
        {
            SaveCurrentPage();
        }
    }


    private void OnPageDragStarted()
    {
        SaveCurrentPage();
    }


    private void OnPageDropped(
        int sourceIndex,
        int targetIndex)
    {
        if (
            sourceIndex == targetIndex ||
            sourceIndex < 0 ||
            targetIndex < 0 ||
            sourceIndex >= pages.Count ||
            targetIndex >= pages.Count)
        {
            return;
        }


        ChapterPageDefinitionData movedPage =
            pages[sourceIndex];


        pages.RemoveAt(
            sourceIndex
        );


        pages.Insert(
            targetIndex,
            movedPage
        );


        if (
            selectedPage ==
            sourceIndex)
        {
            selectedPage =
                targetIndex;
        }
        else if (
            sourceIndex < selectedPage &&
            selectedPage <= targetIndex)
        {
            selectedPage--;
        }
        else if (
            targetIndex <= selectedPage &&
            selectedPage < sourceIndex)
        {
            selectedPage++;
        }


        RefreshPageList();

        LoadSelectedPage();
    }


    private void OpenIllustrationPicker()
    {
        if (
            selectedPage >= 0 &&
            selectedPage < pages.Count)
        {
            illustrationFileDialog.PopupCenteredRatio(
                0.75f
            );
        }
    }


    private void OnIllustrationFileSelected(
        string path)
    {
        if (
            selectedPage < 0 ||
            selectedPage >= pages.Count)
        {
            return;
        }


        pages[selectedPage].ImagePath =
            path;


        UpdateIllustrationUi(
            pages[selectedPage]
        );
    }


    private void OnRemoveIllustrationPressed()
    {
        if (
            selectedPage < 0 ||
            selectedPage >= pages.Count)
        {
            return;
        }


        pages[selectedPage].ImagePath =
            string.Empty;


        UpdateIllustrationUi(
            pages[selectedPage]
        );
    }


    private void UpdateIllustrationUi(
        ChapterPageDefinitionData page)
    {
        bool hasIllustration =
            !string.IsNullOrEmpty(
                page.ImagePath
            );


        addIllustrationButton.Visible =
            !hasIllustration;


        illustrationSection.Visible =
            hasIllustration;


        illustrationPreview.Texture =
            hasIllustration
                ? LoadPreview(
                    page.ImagePath
                )
                : null;
    }


    private static Texture2D LoadPreview(
        string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }


        Image image =
            Image.LoadFromFile(
                path
            );


        if (
            image == null ||
            image.IsEmpty())
        {
            return null;
        }


        return ImageTexture.CreateFromImage(
            image
        );
    }


    private void OnSavePressed()
    {
        SaveCurrentPage();


        SaveOriginalState();


        EmitSignal(
            SignalName.PagesSaved
        );
    }


    private void OnCancelPressed()
    {
        RequestClose();
    }


    private sealed partial class PageDragButton : Button
    {
        public int PageIndex { get; set; }

        public event Action DragStarted;

        public event Action<int, int> PageDropped;


        public override void _Ready()
        {
            MouseEntered +=
                OnMouseEntered;


            MouseExited +=
                OnMouseExited;


            Toggled +=
                OnToggled;


            ApplyStyle(
                ButtonPressed
            );
        }


        private void ApplyStyle(
            bool selected)
        {
            StyleBoxFlat normalStyle =
                new StyleBoxFlat
                {
                    BgColor =
                        new Color(
                            "222730"
                        ),

                    BorderWidthLeft = 1,
                    BorderWidthTop = 1,
                    BorderWidthRight = 1,
                    BorderWidthBottom = 1,

                    BorderColor =
                        new Color(
                            "313743"
                        ),

                    CornerRadiusTopLeft = 3,
                    CornerRadiusTopRight = 3,
                    CornerRadiusBottomLeft = 3,
                    CornerRadiusBottomRight = 3,

                    ContentMarginLeft = 12,
                    ContentMarginRight = 12,
                    ContentMarginTop = 8,
                    ContentMarginBottom = 8
                };


            StyleBoxFlat hoverStyle =
                new StyleBoxFlat
                {
                    BgColor =
                        new Color(
                            "252D35"
                        ),

                    BorderWidthLeft = 1,
                    BorderWidthTop = 1,
                    BorderWidthRight = 1,
                    BorderWidthBottom = 1,

                    BorderColor =
                        new Color(
                            "4FA6A6"
                        ),

                    CornerRadiusTopLeft = 3,
                    CornerRadiusTopRight = 3,
                    CornerRadiusBottomLeft = 3,
                    CornerRadiusBottomRight = 3,

                    ContentMarginLeft = 12,
                    ContentMarginRight = 12,
                    ContentMarginTop = 8,
                    ContentMarginBottom = 8
                };


            StyleBoxFlat pressedStyle =
                new StyleBoxFlat
                {
                    BgColor =
                        new Color(
                            "27343A"
                        ),

                    BorderWidthLeft = 2,
                    BorderWidthTop = 1,
                    BorderWidthRight = 1,
                    BorderWidthBottom = 1,

                    BorderColor =
                        new Color(
                            "4FA6A6"
                        ),

                    CornerRadiusTopLeft = 3,
                    CornerRadiusTopRight = 3,
                    CornerRadiusBottomLeft = 3,
                    CornerRadiusBottomRight = 3,

                    ContentMarginLeft = 11,
                    ContentMarginRight = 12,
                    ContentMarginTop = 8,
                    ContentMarginBottom = 8
                };


            AddThemeStyleboxOverride(
                "normal",
                normalStyle
            );


            AddThemeStyleboxOverride(
                "hover",
                hoverStyle
            );


            AddThemeStyleboxOverride(
                "pressed",
                pressedStyle
            );


            AddThemeColorOverride(
                "font_color",
                new Color(
                    "A8AFBC"
                )
            );


            AddThemeColorOverride(
                "font_hover_color",
                new Color(
                    "E7EAF0"
                )
            );


            AddThemeColorOverride(
                "font_pressed_color",
                new Color(
                    "E7EAF0"
                )
            );


            AddThemeColorOverride(
                "font_focus_color",
                new Color(
                    "E7EAF0"
                )
            );


            if (selected)
            {
                AddThemeStyleboxOverride(
                    "normal",
                    pressedStyle
                );
            }
        }


        private void OnMouseEntered()
        {
            if (!ButtonPressed)
            {
                ApplyStyle(false);
            }
        }


        private void OnMouseExited()
        {
            ApplyStyle(
                ButtonPressed
            );
        }


        private void OnToggled(
            bool pressed)
        {
            ApplyStyle(
                pressed
            );
        }


        public override Variant _GetDragData(
            Vector2 atPosition)
        {
            DragStarted?.Invoke();


            Label preview =
                new Label
                {
                    Text =
                        Text
                };


            preview.AddThemeColorOverride(
                "font_color",
                new Color(
                    "E7EAF0"
                )
            );


            preview.AddThemeFontSizeOverride(
                "font_size",
                14
            );


            SetDragPreview(
                preview
            );


            return
                ChapterPageEditor.DragDataPrefix +
                PageIndex;
        }


        public override bool _CanDropData(
            Vector2 atPosition,
            Variant data)
        {
            return
                TryGetDraggedPageIndex(
                    data,
                    out int sourceIndex
                ) &&
                sourceIndex != PageIndex;
        }


        public override void _DropData(
            Vector2 atPosition,
            Variant data)
        {
            if (
                TryGetDraggedPageIndex(
                    data,
                    out int sourceIndex
                ))
            {
                PageDropped?.Invoke(
                    sourceIndex,
                    PageIndex
                );
            }
        }


        private static bool TryGetDraggedPageIndex(
            Variant data,
            out int pageIndex)
        {
            pageIndex =
                -1;


            if (
                data.VariantType !=
                Variant.Type.String)
            {
                return false;
            }


            string value =
                data.AsString();


            return
                value.StartsWith(
                    ChapterPageEditor.DragDataPrefix
                ) &&
                int.TryParse(
                    value.Substring(
                        ChapterPageEditor.DragDataPrefix.Length
                    ),
                    out pageIndex
                );
        }
    }
}