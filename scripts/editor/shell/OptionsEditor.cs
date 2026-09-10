using Godot;
using System.Collections.Generic;

public partial class OptionsEditor : Control
{
    private OptionButton pageOption;

    private VBoxContainer optionList;
    private Button addOptionButton;
    private Button deleteOptionButton;

    private DecisionEditor decisionEditor;

    private readonly List<EditorDecisionData> decisions = new();

    private List<EditorPageData> pages =
        new List<EditorPageData>();

    private EditorPageData selectedPage;

    private int selectedDecision = -1;
    private int chapter = 1;

    private EventRepository eventRepository;
    private string projectPath = "";


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


        pageOption.ItemSelected +=
            OnPageSelected;

        addOptionButton.Pressed +=
            OnAddOptionPressed;

        deleteOptionButton.Pressed +=
            OnDeleteOptionPressed;


        GD.Print(
            "OptionsEditor iniciado."
        );
    }


    public void SetRepository(
        EventRepository repository)
    {
        eventRepository =
            repository;


        decisionEditor.SetRepository(
            eventRepository
        );
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


        GD.Print(
            "OptionsEditor: ruta del proyecto establecida: ",
            projectPath
        );
    }


    public void SetPages(
        List<EditorPageData> eventPages)
    {
        SaveCurrentPageDecisions();


        pages =
            eventPages ??
            new List<EditorPageData>();


        PopulateDecisionPages();
    }


    public void SetDecisions(
        List<EditorDecisionData> eventDecisions)
    {
        decisions.Clear();


        if (eventDecisions != null)
        {
            decisions.AddRange(
                eventDecisions
            );
        }


        if (decisions.Count == 0)
        {
            CreateInitialDecision();

            return;
        }


        selectedDecision =
            0;


        RefreshOptionList();

        LoadSelectedDecision();
    }


    public List<EditorDecisionData> GetDecisions()
    {
        SaveCurrentDecision();


        return new List<EditorDecisionData>(
            decisions
        );
    }


    public List<EditorPageData> GetPages()
    {
        SaveCurrentPageDecisions();


        return pages;
    }


    public void LoadPage(
        EditorPageData page)
    {
        if (page == null)
        {
            selectedPage = null;

            decisions.Clear();
            selectedDecision = -1;

            RefreshOptionList();

            decisionEditor.Visible = false;

            return;
        }


        SaveCurrentPageDecisions();


        selectedPage =
            page;


        LoadDecisionsFromPage();


        GD.Print(
            "OptionsEditor: cargada página de decisión."
        );

        GD.Print(
            "  Página ID: ",
            selectedPage.Id
        );

        GD.Print(
            "  Decisiones: ",
            selectedPage.Decisions.Count
        );
    }


    private void PopulateDecisionPages()
    {
        SaveCurrentPageDecisions();


        pageOption.Clear();


        int firstDecisionPageIndex = -1;


        for (
            int i = 0;
            i < pages.Count;
            i++)
        {
            EditorPageData page =
                pages[i];


            if (
                page.Type !=
                EditorPageType.Decision)
            {
                continue;
            }


            pageOption.AddItem(
                $"Página {i + 1}"
            );


            int itemIndex =
                pageOption.ItemCount - 1;


            pageOption.SetItemMetadata(
                itemIndex,
                page.Id
            );


            if (firstDecisionPageIndex < 0)
            {
                firstDecisionPageIndex =
                    itemIndex;
            }
        }


        if (firstDecisionPageIndex < 0)
        {
            selectedPage = null;

            decisions.Clear();
            selectedDecision = -1;

            RefreshOptionList();

            decisionEditor.Visible = false;


            GD.Print(
                "OptionsEditor: no hay páginas de decisión."
            );


            return;
        }


        pageOption.Select(
            firstDecisionPageIndex
        );


        LoadPageFromOption(
            firstDecisionPageIndex
        );
    }


    private void OnPageSelected(
        long index)
    {
        SaveCurrentPageDecisions();


        LoadPageFromOption(
            (int)index
        );
    }


    private void LoadPageFromOption(
        int index)
    {
        if (
            index < 0 ||
            index >= pageOption.ItemCount)
        {
            return;
        }


        Variant metadata =
            pageOption.GetItemMetadata(
                index
            );


        string pageId =
            metadata.AsString();


        EditorPageData page =
            FindPageById(
                pageId
            );


        if (page == null)
        {
            GD.PrintErr(
                "OptionsEditor: no se encontró la página con ID: ",
                pageId
            );

            return;
        }


        selectedPage =
            page;


        LoadDecisionsFromPage();
    }


    private EditorPageData FindPageById(
        string pageId)
    {
        foreach (
            EditorPageData page
            in pages)
        {
            if (
                page.Id ==
                pageId)
            {
                return page;
            }
        }


        return null;
    }


    private void LoadDecisionsFromPage()
    {
        decisions.Clear();


        if (
            selectedPage != null &&
            selectedPage.Decisions != null)
        {
            decisions.AddRange(
                selectedPage.Decisions
            );
        }


        if (decisions.Count == 0)
        {
            CreateInitialDecision();

            return;
        }


        selectedDecision =
            0;


        RefreshOptionList();

        LoadSelectedDecision();
    }


    private void SaveCurrentPageDecisions()
    {
        if (selectedPage == null)
        {
            return;
        }


        SaveCurrentDecision();


        selectedPage.Decisions.Clear();


        foreach (
            EditorDecisionData decision
            in decisions)
        {
            if (decision != null)
            {
                selectedPage.Decisions.Add(
                    decision
                );
            }
        }


        GD.Print(
            "OptionsEditor: guardadas ",
            selectedPage.Decisions.Count,
            " decisiones en página ",
            GetPageNumber(selectedPage)
        );
    }


    private int GetPageNumber(
        EditorPageData page)
    {
        if (page == null)
        {
            return -1;
        }


        for (
            int i = 0;
            i < pages.Count;
            i++)
        {
            if (
                pages[i].Id ==
                page.Id)
            {
                return i + 1;
            }
        }


        return -1;
    }


    private void CreateInitialDecision()
    {
        if (decisions.Count == 0)
        {
            decisions.Add(
                new EditorDecisionData
                {
                    Text =
                        "Nueva opción"
                }
            );
        }


        selectedDecision =
            0;


        RefreshOptionList();

        LoadSelectedDecision();
    }


    private void OnAddOptionPressed()
    {
        SaveCurrentDecision();


        EditorDecisionData newDecision =
            new EditorDecisionData
            {
                Text =
                    $"Nueva opción {decisions.Count + 1}"
            };


        decisions.Add(
            newDecision
        );


        selectedDecision =
            decisions.Count - 1;


        RefreshOptionList();

        LoadSelectedDecision();
    }


    private void OnDeleteOptionPressed()
    {
        if (
            selectedDecision < 0 ||
            selectedDecision >= decisions.Count)
        {
            return;
        }


        if (decisions.Count <= 1)
        {
            return;
        }


        SaveCurrentDecision();


        decisions.RemoveAt(
            selectedDecision
        );


        if (
            selectedDecision >=
            decisions.Count)
        {
            selectedDecision =
                decisions.Count - 1;
        }


        RefreshOptionList();

        LoadSelectedDecision();
    }


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
            int index =
                i;


            Button optionButton =
                new Button();


            optionButton.Text =
                $"Opción {index + 1}";


            optionButton.CustomMinimumSize =
                new Vector2(
                    0,
                    40
                );


            optionButton.Alignment =
                HorizontalAlignment.Left;


            optionButton.ToggleMode =
                true;


            optionButton.ButtonPressed =
                index == selectedDecision;


            optionButton.Pressed +=
                () =>
                {
                    SelectDecision(
                        index
                    );
                };


            optionList.AddChild(
                optionButton
            );
        }


        deleteOptionButton.Disabled =
            decisions.Count <= 1;
    }


    private void SelectDecision(
        int index)
    {
        if (
            index < 0 ||
            index >= decisions.Count)
        {
            return;
        }


        SaveCurrentDecision();


        selectedDecision =
            index;


        RefreshOptionList();

        LoadSelectedDecision();
    }


    private void LoadSelectedDecision()
    {
        if (
            selectedDecision < 0 ||
            selectedDecision >= decisions.Count)
        {
            decisionEditor.Visible =
                false;

            return;
        }


        decisionEditor.Visible =
            true;


        EditorDecisionData decision =
            decisions[selectedDecision];


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


        GD.Print(
            "OptionsEditor: cargada opción ",
            selectedDecision + 1,
            " de página ",
            GetPageNumber(selectedPage)
        );
    }


    private void SaveCurrentDecision()
    {
        if (
            selectedDecision < 0 ||
            selectedDecision >= decisions.Count)
        {
            return;
        }


        EditorDecisionData decision =
            decisions[selectedDecision];


        decisionEditor.SaveCurrentDecision();


        decision.Text =
            decisionEditor.GetDecisionText();


        decision.Description =
            decisionEditor.GetDecisionDescription();


        decision.NextPageId =
            decisionEditor.GetNextPageId();


        decision.Conditions =
            decisionEditor.GetConditions();


        decision.Effects =
            decisionEditor.GetEffects();


        GD.Print(
            "OptionsEditor: guardada opción ",
            selectedDecision + 1
        );

        GD.Print(
            "  Condiciones: ",
            decision.Conditions.Count
        );

        GD.Print(
            "  Efectos: ",
            decision.Effects.Count
        );

        GD.Print(
            "  NextPageId: ",
            decision.NextPageId
        );
    }


    public void SetChapter(
        int eventChapter)
    {
        chapter =
            Mathf.Clamp(
                eventChapter,
                1,
                7
            );


        decisionEditor.SetChapter(
            chapter
        );


        GD.Print(
            "OptionsEditor: capítulo establecido: ",
            chapter
        );
    }
}