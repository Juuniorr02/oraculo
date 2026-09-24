using Godot;
using System;
using System.Collections.Generic;


public partial class DecisionEditor : Control
{
    private TextEdit decisionTextEdit;
    private TextEdit descriptionTextEdit;

    private OptionButton nextPageOption;

    private ConditionListEditor conditionListEditor;
    private EffectListEditor effectListEditor;


    private int chapter = 1;

    private List<EditorPageData> pages =
        new();


    private EditorDecisionData decisionData;


    private EventRepository eventRepository;

    private string projectPath = "";


    // ============================================================
    // READY
    // ============================================================

    public override void _Ready()
    {
        decisionTextEdit =
            GetNode<TextEdit>(
                "VBoxContainer/DecisionTextEdit"
            );

        descriptionTextEdit =
            GetNode<TextEdit>(
                "VBoxContainer/DescriptionTextEdit"
            );

        nextPageOption =
            GetNode<OptionButton>(
                "VBoxContainer/NextPageOption"
            );

        conditionListEditor =
            GetNode<ConditionListEditor>(
                "VBoxContainer/ConditionListEditor"
            );

        effectListEditor =
            GetNode<EffectListEditor>(
                "VBoxContainer/EffectListEditor"
            );


        // La opción vuelve a estar activa.
        nextPageOption.Visible =
            true;

        nextPageOption.Disabled =
            false;


        nextPageOption.ItemSelected +=
            OnNextPageSelected;


        GD.Print(
            "DecisionEditor iniciado."
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


    conditionListEditor.SetRepository(
        repository
    );
}


    public void SetProjectPath(
        string path)
    {
        projectPath =
            path ?? "";


        if (conditionListEditor != null)
        {
            conditionListEditor.SetProjectPath(
                projectPath
            );
        }


        if (effectListEditor != null)
        {
            effectListEditor.SetProjectPath(
                projectPath
            );
        }


        GD.Print(
            "DecisionEditor: ruta del proyecto establecida: ",
            projectPath
        );
    }


    public void SetChapter(
        int value)
    {
        chapter =
            value;


        if (conditionListEditor != null)
        {
            conditionListEditor.SetChapter(
                chapter
            );
        }


        if (effectListEditor != null)
        {
            effectListEditor.SetChapter(
                chapter
            );
        }


        GD.Print(
            "DecisionEditor: capítulo establecido: ",
            chapter
        );
    }


    // ============================================================
    // PAGES
    // ============================================================

    public void SetPages(
        List<EditorPageData> newPages)
    {
        pages =
            newPages != null
                ? new List<EditorPageData>(
                    newPages
                )
                : new List<EditorPageData>();


        PopulateNextPages();
    }


    // ============================================================
    // LOAD
    // ============================================================

    public void LoadDecision(
    int index,
    string text)
{
    decisionData =
        new EditorDecisionData
        {
            Text =
                text ?? "",

            Description =
                "",

            Conditions =
                new List<EditorConditionData>(),

            Effects =
                new List<EditorEffectData>(),

            Pages =
                new List<EditorPageData>(),

            NextPageId =
                ""
        };


    decisionTextEdit.Text =
        decisionData.Text;


    descriptionTextEdit.Text =
        decisionData.Description;


    conditionListEditor.SetConditions(
        decisionData.Conditions
    );


    effectListEditor.SetEffects(
        decisionData.Effects
    );


    // Aquí no intentamos seleccionar nada si todavía
    // no se han cargado las páginas.
    if (nextPageOption.ItemCount > 0)
    {
        SelectNextPage(
            ""
        );
    }


    GD.Print(
        "DecisionEditor cargando opción: ",
        index
    );
}


    public void LoadDecisionData(
        EditorDecisionData data)
    {
        if (data == null)
        {
            LoadDecision(
                -1,
                ""
            );


            return;
        }


        decisionData =
            data;


        if (decisionData.Conditions == null)
        {
            decisionData.Conditions =
                new List<EditorConditionData>();
        }


        if (decisionData.Effects == null)
        {
            decisionData.Effects =
                new List<EditorEffectData>();
        }


        if (decisionData.Pages == null)
        {
            decisionData.Pages =
                new List<EditorPageData>();
        }


        decisionTextEdit.Text =
            decisionData.Text ?? "";


        descriptionTextEdit.Text =
            decisionData.Description ?? "";


        conditionListEditor.SetConditions(
            decisionData.Conditions
        );


        effectListEditor.SetEffects(
            decisionData.Effects
        );


        PopulateNextPages();


        SelectNextPage(
            decisionData.NextPageId
        );


        GD.Print(
            "DecisionEditor cargando opción: ",
            decisionData.NextPageId
        );
    }


    // ============================================================
    // NEXT PAGE
    // ============================================================

    private void PopulateNextPages()
    {
        if (nextPageOption == null)
        {
            return;
        }


        nextPageOption.Clear();


        nextPageOption.AddItem(
            "Sin página de destino"
        );


        nextPageOption.SetItemMetadata(
            0,
            ""
        );


        List<PageEntry> entries =
            new();


        foreach (
            EditorPageData page
            in pages)
        {
            CollectPages(
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
                nextPageOption.ItemCount;


            nextPageOption.AddItem(
                entry.Path
            );


            nextPageOption.SetItemMetadata(
                index,
                entry.Page.Id
            );
        }


        nextPageOption.Disabled =
            false;


        GD.Print(
            "DecisionEditor: páginas disponibles como destino: ",
            entries.Count
        );
    }


    private void CollectPages(
        EditorPageData page,
        string parentPath,
        List<PageEntry> result)
    {
        if (page == null)
        {
            return;
        }


        string pageName =
            GetPageName(
                page
            );


        string currentPath =
            string.IsNullOrWhiteSpace(
                parentPath)
                ? pageName
                : parentPath +
                  " / " +
                  pageName;


        result.Add(
            new PageEntry(
                page,
                currentPath
            )
        );


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


            string decisionText =
                string.IsNullOrWhiteSpace(
                    decision.Text)
                    ? $"Opción {GetDecisionLetter(d)}"
                    : decision.Text.Trim();


            string decisionPath =
                currentPath +
                " / " +
                GetDecisionLetter(d) +
                " · " +
                decisionText;


            if (decision.Pages == null)
            {
                continue;
            }


            foreach (
                EditorPageData branchPage
                in decision.Pages)
            {
                CollectPages(
                    branchPage,
                    decisionPath,
                    result
                );
            }
        }
    }


    private string GetPageName(
        EditorPageData page)
    {
        if (
            page != null &&
            !string.IsNullOrWhiteSpace(
                page.Title))
        {
            return page.Title.Trim();
        }


        return "Página";
    }


    private void OnNextPageSelected(
        long index)
    {
        if (
            index < 0 ||
            index >= nextPageOption.ItemCount)
        {
            return;
        }


        Variant metadata =
            nextPageOption.GetItemMetadata(
                (int)index
            );


        if (decisionData != null)
        {
            decisionData.NextPageId =
                metadata.AsString();
        }
    }


    private string GetNextPageId()
    {
        if (
            nextPageOption == null ||
            nextPageOption.Selected < 0)
        {
            return
                decisionData?.NextPageId ?? "";
        }


        Variant metadata =
            nextPageOption.GetItemMetadata(
                nextPageOption.Selected
            );


        return metadata.AsString();
    }


    private void SelectNextPage(
    string pageId)
{
    if (nextPageOption == null)
    {
        return;
    }


    if (nextPageOption.ItemCount == 0)
    {
        // Todavía no se han cargado las páginas.
        // No intentamos seleccionar nada.
        return;
    }


    string targetId =
        pageId ?? "";


    for (
        int i = 0;
        i < nextPageOption.ItemCount;
        i++)
    {
        Variant metadata =
            nextPageOption.GetItemMetadata(
                i
            );


        if (
            metadata.VariantType !=
            Variant.Type.Nil &&
            metadata.AsString() == targetId)
        {
            nextPageOption.Select(
                i
            );


            return;
        }
    }


    // Si el destino ya no existe, usamos
    // "Sin página de destino".
    nextPageOption.Select(
        0
    );
}


    // Compatibilidad con código anterior.
    private void OnNextPageSelectedLegacy(
        long index)
    {
        OnNextPageSelected(
            index
        );
    }


    // ============================================================
    // SAVE
    // ============================================================

    public void SaveCurrentDecision()
    {
        if (decisionData == null)
        {
            decisionData =
                new EditorDecisionData();
        }


        if (decisionData.Conditions == null)
        {
            decisionData.Conditions =
                new List<EditorConditionData>();
        }


        if (decisionData.Effects == null)
        {
            decisionData.Effects =
                new List<EditorEffectData>();
        }


        if (decisionData.Pages == null)
        {
            decisionData.Pages =
                new List<EditorPageData>();
        }


        decisionData.Text =
            decisionTextEdit.Text;


        decisionData.Description =
            descriptionTextEdit.Text;


        decisionData.NextPageId =
            GetNextPageId();


        decisionData.Conditions =
            conditionListEditor.GetConditions();


        decisionData.Effects =
            effectListEditor.GetEffects();
    }


    // ============================================================
    // GETTERS
    // ============================================================

    public string GetDecisionText()
    {
        return
            decisionTextEdit?.Text ?? "";
    }


    public string GetDecisionDescription()
    {
        return
            descriptionTextEdit?.Text ?? "";
    }


    public List<EditorConditionData> GetConditions()
    {
        return
            conditionListEditor?.GetConditions()
            ?? new List<EditorConditionData>();
    }


    public List<EditorEffectData> GetEffects()
    {
        return
            effectListEditor?.GetEffects()
            ?? new List<EditorEffectData>();
    }


    public string GetNextPageIdValue()
    {
        return
            GetNextPageId();
    }


    public List<EditorPageData> GetBranchPages()
    {
        if (decisionData == null)
        {
            return
                new List<EditorPageData>();
        }


        if (decisionData.Pages == null)
        {
            decisionData.Pages =
                new List<EditorPageData>();
        }


        return decisionData.Pages;
    }


    // ============================================================
    // HELPERS
    // ============================================================

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
}