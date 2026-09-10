using Godot;
using System.Collections.Generic;

public partial class DecisionEditor : VBoxContainer
{
    private TextEdit decisionTextEdit;
    private TextEdit descriptionTextEdit;
    private OptionButton nextPageOption;

    private ConditionListEditor conditionListEditor;
    private EffectListEditor effectListEditor;

    private int chapter = 1;

    private List<EditorPageData> pages =
        new List<EditorPageData>();

    private EditorDecisionData decisionData;

    private EventRepository eventRepository;
    private string projectPath = "";


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


        nextPageOption.ItemSelected +=
            OnNextPageSelected;


        GD.Print(
            "DecisionEditor iniciado."
        );
    }


    public void SetRepository(
        EventRepository repository)
    {
        eventRepository =
            repository;


        conditionListEditor.SetRepository(
            eventRepository
        );


        effectListEditor.SetRepository(
            eventRepository
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


    public void SetPages(
        List<EditorPageData> eventPages)
    {
        pages =
            eventPages ??
            new List<EditorPageData>();


        GD.Print(
            "DecisionEditor: recibidas ",
            pages.Count,
            " páginas."
        );


        for (
            int i = 0;
            i < pages.Count;
            i++)
        {
            if (pages[i] == null)
            {
                continue;
            }

            GD.Print(
                "Página ",
                i + 1,
                " | ID: ",
                pages[i].Id
            );
        }


        PopulateNextPages();
    }


    public void LoadDecision(
        int optionNumber,
        string text)
    {
        decisionTextEdit.Text =
            text;

        descriptionTextEdit.Text =
            "";

        GD.Print(
            "DecisionEditor cargando opción: ",
            optionNumber
        );
    }


    public void LoadDecisionData(
        EditorDecisionData data)
    {
        if (data == null)
        {
            GD.PrintErr(
                "DecisionEditor: se intentó cargar una decisión nula."
            );

            return;
        }


        decisionData =
            data;


        decisionTextEdit.Text =
            decisionData.Text;

        descriptionTextEdit.Text =
            decisionData.Description;


        PopulateNextPages();


        SelectNextPage(
            decisionData.NextPageId
        );


        conditionListEditor.SetConditions(
            decisionData.Conditions
        );


        effectListEditor.SetEffects(
            decisionData.Effects
        );


        GD.Print(
            "DecisionEditor: decisión cargada."
        );


        GD.Print(
            "  Texto: ",
            decisionData.Text
        );

        GD.Print(
            "  Descripción: ",
            decisionData.Description
        );

        GD.Print(
            "  Condiciones: ",
            decisionData.Conditions.Count
        );

        GD.Print(
            "  Efectos: ",
            decisionData.Effects.Count
        );

        GD.Print(
            "  NextPageId: ",
            decisionData.NextPageId
        );
    }


    public void SaveCurrentDecision()
    {
        if (decisionData == null)
        {
            return;
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


        GD.Print(
            "DecisionEditor: datos de decisión guardados."
        );


        GD.Print(
            "  Texto: ",
            decisionData.Text
        );

        GD.Print(
            "  Descripción: ",
            decisionData.Description
        );

        GD.Print(
            "  Condiciones: ",
            decisionData.Conditions.Count
        );

        GD.Print(
            "  Efectos: ",
            decisionData.Effects.Count
        );
    }


    public string GetDecisionText()
    {
        return decisionTextEdit.Text;
    }


    public string GetDecisionDescription()
    {
        if (decisionData == null)
        {
            return "";
        }


        return descriptionTextEdit.Text;
    }


    public string GetNextPageId()
    {
        if (decisionData == null)
        {
            return "";
        }


        if (nextPageOption.Selected < 0)
        {
            return "";
        }


        if (nextPageOption.Selected == 0)
        {
            return "";
        }


        Variant metadata =
            nextPageOption.GetItemMetadata(
                nextPageOption.Selected
            );


        return metadata.AsString();
    }


    public List<EditorConditionData> GetConditions()
    {
        return conditionListEditor.GetConditions();
    }


    public List<EditorEffectData> GetEffects()
    {
        return effectListEditor.GetEffects();
    }


    private void PopulateNextPages()
    {
        GD.Print(
            "DecisionEditor: rellenando Próxima página con ",
            pages.Count,
            " páginas."
        );


        nextPageOption.Clear();


        nextPageOption.AddItem(
            "Ninguna"
        );


        for (
            int i = 0;
            i < pages.Count;
            i++)
        {
            EditorPageData page =
                pages[i];


            if (page == null)
            {
                continue;
            }


            nextPageOption.AddItem(
                $"Página {i + 1}"
            );


            int itemIndex =
                nextPageOption.ItemCount - 1;


            nextPageOption.SetItemMetadata(
                itemIndex,
                page.Id
            );
        }


        nextPageOption.Select(
            0
        );
    }


    private void OnNextPageSelected(
        long index)
    {
        if (decisionData == null)
        {
            GD.Print(
                "DecisionEditor: se seleccionó una página pero no hay decisión cargada."
            );

            return;
        }


        if (index == 0)
        {
            decisionData.NextPageId =
                "";


            GD.Print(
                "DecisionEditor: próxima página establecida en Ninguna."
            );


            return;
        }


        Variant metadata =
            nextPageOption.GetItemMetadata(
                (int)index
            );


        string pageId =
            metadata.AsString();


        decisionData.NextPageId =
            pageId;


        GD.Print(
            "DecisionEditor: próxima página seleccionada."
        );


        GD.Print(
            "  Página: ",
            index,
            " | ID: ",
            pageId
        );
    }


    private void SelectNextPage(
        string pageId)
    {
        if (string.IsNullOrEmpty(pageId))
        {
            nextPageOption.Select(
                0
            );

            return;
        }


        for (
            int i = 1;
            i < nextPageOption.ItemCount;
            i++)
        {
            Variant metadata =
                nextPageOption.GetItemMetadata(
                    i
                );


            if (
                metadata.AsString() ==
                pageId)
            {
                nextPageOption.Select(
                    i
                );


                GD.Print(
                    "DecisionEditor: página previamente seleccionada encontrada: Página ",
                    i
                );


                return;
            }
        }


        GD.Print(
            "DecisionEditor: no se encontró la página con ID: ",
            pageId
        );


        nextPageOption.Select(
            0
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


        conditionListEditor.SetChapter(
            chapter
        );


        effectListEditor.SetChapter(
            chapter
        );


        GD.Print(
            "DecisionEditor: capítulo establecido: ",
            chapter
        );
    }
}