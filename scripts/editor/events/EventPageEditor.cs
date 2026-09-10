using Godot;
using System;
using System.Collections.Generic;

public partial class EventPageEditor : Control
{
    private const string DragDataPrefix = "event_page_editor:";

    private VBoxContainer pageList;
    private Button addPageButton;
    private Button deletePageButton;

    private Label pageTitle;
    private OptionButton pageTypeOption;
    private TextEdit textEdit;

    private Button addIllustrationButton;
    private VBoxContainer illustrationSection;
    private TextureRect illustrationPreview;
    private Button changeIllustrationButton;
    private Button removeIllustrationButton;
    private FileDialog illustrationFileDialog;

    private readonly List<EditorPageData> pages = new();

    private int selectedPage = -1;
    private bool isLoadingPage;


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
            GetNode<Label>(
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

        illustrationFileDialog =
            GetNode<FileDialog>(
                "IllustrationFileDialog"
            );


        addPageButton.Pressed +=
            OnAddPagePressed;

        deletePageButton.Pressed +=
            OnDeletePagePressed;

        pageTypeOption.ItemSelected +=
            OnPageTypeSelected;

        textEdit.TextChanged +=
            OnTextChanged;

        addIllustrationButton.Pressed +=
            OpenIllustrationPicker;

        changeIllustrationButton.Pressed +=
            OpenIllustrationPicker;

        removeIllustrationButton.Pressed +=
            OnRemoveIllustrationPressed;

        illustrationFileDialog.FileSelected +=
            OnIllustrationFileSelected;

        illustrationFileDialog.AddFilter(
            "*.png, *.jpg, *.jpeg, *.webp ; Archivos de imagen"
        );


        CreateInitialPage();


        GD.Print(
            "EventPageEditor iniciado."
        );
    }


    public void SetPages(
        List<EditorPageData> newPages)
    {
        SaveCurrentPage();

        pages.Clear();


        if (newPages != null)
        {
            foreach (
                EditorPageData page
                in newPages)
            {
                if (page == null)
                {
                    continue;
                }

                pages.Add(
                    page
                );
            }
        }


        if (pages.Count == 0)
        {
            CreateInitialPage();

            GD.Print(
                "EventPageEditor: no había páginas. Se creó una página inicial."
            );

            return;
        }


        selectedPage =
            0;


        RefreshPageList();

        LoadSelectedPage();


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


        for (
            int i = 0;
            i < pages.Count;
            i++)
        {
            GD.Print(
                "Página ",
                i + 1,
                " | ID: ",
                pages[i].Id,
                " | Tipo: ",
                pages[i].Type
            );
        }


        return pages;
    }


    private void CreateInitialPage()
    {
        if (pages.Count == 0)
        {
            pages.Add(
                new EditorPageData()
            );
        }


        RefreshPageList();

        SelectPage(0);
    }


    private void OnAddPagePressed()
    {
        SaveCurrentPage();


        EditorPageData newPage =
            new EditorPageData();


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
            PageDragButton pageButton =
                new PageDragButton
                {
                    Text =
                        $"Página {i + 1}",

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


        EditorPageData page =
            pages[selectedPage];


        isLoadingPage =
            true;


        pageTitle.Text =
            $"Página {selectedPage + 1}";


        textEdit.Text =
            page.Text;


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
        EditorPageType type)
    {
        switch (type)
        {
            case EditorPageType.Normal:
                pageTypeOption.Select(0);
                break;

            case EditorPageType.Decision:
                pageTypeOption.Select(1);
                break;

            default:
                pageTypeOption.Select(0);
                break;
        }
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


        EditorPageData page =
            pages[selectedPage];


        switch (index)
        {
            case 0:
                page.Type =
                    EditorPageType.Normal;
                break;

            case 1:
                page.Type =
                    EditorPageType.Decision;
                break;
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


        EditorPageData page =
            pages[selectedPage];


        page.Text =
            textEdit.Text;
    }


    private void OnTextChanged()
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


        EditorPageData movedPage =
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


        pages[selectedPage].Illustration =
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


        pages[selectedPage].Illustration =
            string.Empty;


        UpdateIllustrationUi(
            pages[selectedPage]
        );
    }


    private void UpdateIllustrationUi(
        EditorPageData page)
    {
        bool hasIllustration =
            !string.IsNullOrEmpty(
                page.Illustration
            );


        addIllustrationButton.Visible =
            !hasIllustration;


        illustrationSection.Visible =
            hasIllustration;


        illustrationPreview.Texture =
            hasIllustration
                ? LoadPreview(
                    page.Illustration
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


    private sealed partial class PageDragButton : Button
    {
        public int PageIndex { get; set; }

        public event Action DragStarted;

        public event Action<int, int> PageDropped;


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


            SetDragPreview(
                preview
            );


            return
                EventPageEditor.DragDataPrefix +
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
                    EventPageEditor.DragDataPrefix
                ) &&
                int.TryParse(
                    value.Substring(
                        EventPageEditor.DragDataPrefix.Length
                    ),
                    out pageIndex
                );
        }
    }
}