using Godot;
using System.Collections.Generic;

public partial class InterludePageEditor : Control
{
    private VBoxContainer pageList;
    private Button addPageButton;

    private Label pageTitle;

    private TextEdit textEdit;

    private Button addIllustrationButton;
    private VBoxContainer illustrationSection;

    private TextureRect illustrationPreview;

    private Button changeIllustrationButton;
    private Button removeIllustrationButton;

    private OptionButton illustrationPlacementOption;

    private FileDialog illustrationFileDialog;

    private List<EditorInterludePageData> pages =
        new List<EditorInterludePageData>();

    private int selectedPageIndex = -1;


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

        pageTitle =
            GetNode<Label>(
                "HBoxContainer/EditorPanel/MarginContainer/VBoxContainer/PageTitle"
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

        illustrationPlacementOption =
            GetNode<OptionButton>(
                "HBoxContainer/EditorPanel/MarginContainer/VBoxContainer/IllustrationSection/IllustrationPlacementOption"
            );

        illustrationFileDialog =
            GetNode<FileDialog>(
                "IllustrationFileDialog"
            );


        addPageButton.Pressed +=
            OnAddPagePressed;

        textEdit.TextChanged +=
            OnTextChanged;

        addIllustrationButton.Pressed +=
            OnAddIllustrationPressed;

        changeIllustrationButton.Pressed +=
            OnChangeIllustrationPressed;

        removeIllustrationButton.Pressed +=
            OnRemoveIllustrationPressed;

        illustrationPlacementOption.ItemSelected +=
            OnIllustrationPlacementSelected;

        illustrationFileDialog.FileSelected +=
            OnIllustrationFileSelected;


        RefreshPageList();
        ClearPageEditor();
    }


    public void SetPages(
        List<EditorInterludePageData> newPages)
    {
        pages =
            newPages != null
                ? newPages
                : new List<EditorInterludePageData>();

        selectedPageIndex =
            pages.Count > 0
                ? 0
                : -1;

        RefreshPageList();

        if (selectedPageIndex >= 0)
        {
            LoadSelectedPageIntoEditor();
        }
        else
        {
            ClearPageEditor();
        }
    }


    public List<EditorInterludePageData> GetPages()
    {
        UpdateCurrentPageFromEditor();

        return pages;
    }


    public bool SelectPageById(
        string pageId)
    {
        if (string.IsNullOrWhiteSpace(pageId))
        {
            return false;
        }

        for (
            int i = 0;
            i < pages.Count;
            i++)
        {
            EditorInterludePageData page =
                pages[i];

            if (page == null)
            {
                continue;
            }

            if (page.Id != pageId)
            {
                continue;
            }

            SelectPage(
                i
            );

            return true;
        }

        return false;
    }


    private void RefreshPageList()
    {
        foreach (Node child in pageList.GetChildren())
            child.QueueFree();

        for (int i = 0; i < pages.Count; i++)
        {
            int pageIndex = i;

            Button pageButton =
                new Button();

            pageButton.Text =
                $"Página {i + 1}";

            pageButton.CustomMinimumSize =
                new Vector2(0, 38);

            pageButton.SizeFlagsHorizontal =
                Control.SizeFlags.ExpandFill;

            pageButton.Pressed += () =>
                SelectPage(pageIndex);

            pageList.AddChild(pageButton);
        }
    }


    private void SelectPage(
        int pageIndex)
    {
        if (
            pageIndex < 0 ||
            pageIndex >= pages.Count)
        {
            return;
        }

        UpdateCurrentPageFromEditor();

        selectedPageIndex =
            pageIndex;

        LoadSelectedPageIntoEditor();
    }


    private void LoadSelectedPageIntoEditor()
    {
        if (
            selectedPageIndex < 0 ||
            selectedPageIndex >= pages.Count)
        {
            ClearPageEditor();
            return;
        }

        EditorInterludePageData page =
            pages[selectedPageIndex];


        pageTitle.Text =
            $"Página {selectedPageIndex + 1}";

        textEdit.Text =
            page.Text ?? "";


        illustrationPlacementOption.Select(
            (int)page.IllustrationPlacement
        );


        bool hasIllustration =
            !string.IsNullOrWhiteSpace(
                page.Illustration
            );


        if (hasIllustration)
        {
            ShowIllustration(
                page.Illustration
            );
        }
        else
        {
            HideIllustration();
        }
    }


    private void ClearPageEditor()
    {
        pageTitle.Text =
            "Página";

        textEdit.Text =
            "";

        illustrationPlacementOption.Select(
            (int)EditorIllustrationPlacement.Center
        );

        HideIllustration();
    }


    private void UpdateCurrentPageFromEditor()
    {
        if (
            selectedPageIndex < 0 ||
            selectedPageIndex >= pages.Count)
        {
            return;
        }

        EditorInterludePageData page =
            pages[selectedPageIndex];

        page.Text =
            textEdit.Text;

        page.IllustrationPlacement =
            (EditorIllustrationPlacement)
            illustrationPlacementOption.Selected;
    }


    private void OnAddPagePressed()
    {
        UpdateCurrentPageFromEditor();

        EditorInterludePageData newPage =
            new EditorInterludePageData();

        pages.Add(
            newPage
        );

        selectedPageIndex =
            pages.Count - 1;

        RefreshPageList();

        LoadSelectedPageIntoEditor();
    }


    private void OnTextChanged()
    {
        if (
            selectedPageIndex < 0 ||
            selectedPageIndex >= pages.Count)
        {
            return;
        }

        pages[selectedPageIndex].Text =
            textEdit.Text;
    }


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
            selectedPageIndex < 0 ||
            selectedPageIndex >= pages.Count)
        {
            return;
        }

        pages[selectedPageIndex].Illustration =
            "";

        HideIllustration();
    }


    private void OnIllustrationPlacementSelected(
        long index)
    {
        if (
            selectedPageIndex < 0 ||
            selectedPageIndex >= pages.Count)
        {
            return;
        }

        pages[selectedPageIndex].IllustrationPlacement =
            (EditorIllustrationPlacement)index;
    }


    private void OnIllustrationFileSelected(
        string path)
    {
        if (
            selectedPageIndex < 0 ||
            selectedPageIndex >= pages.Count)
        {
            return;
        }

        pages[selectedPageIndex].Illustration =
            path;

        ShowIllustration(
            path
        );
    }


    private void ShowIllustration(
        string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            HideIllustration();
            return;
        }

        Texture2D texture =
            GD.Load<Texture2D>(path);

        if (texture == null)
        {
            GD.PrintErr(
                $"InterludePageEditor: no se pudo cargar la ilustración '{path}'."
            );

            HideIllustration();
            return;
        }

        illustrationPreview.Texture =
            texture;

        illustrationSection.Visible =
            true;

        addIllustrationButton.Visible =
            false;
    }


    private void HideIllustration()
    {
        illustrationPreview.Texture =
            null;

        illustrationSection.Visible =
            false;

        addIllustrationButton.Visible =
            true;
    }
}