using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


public partial class ChaptersListView : Control
{
    [Signal]
    public delegate void ChapterSelectedEventHandler(
        string chapterId
    );


    private ChapterRepository chapterRepository;
    private EventRepository eventRepository;
    private InterludeRepository interludeRepository;


    private Button newChapterButton;
    private VBoxContainer chapterList;


    public override void _Ready()
    {
        newChapterButton =
            GetNode<Button>(
                "MarginContainer/VBoxContainer/Header/NewChapterButton"
            );


        chapterList =
            GetNode<VBoxContainer>(
                "MarginContainer/VBoxContainer/ChapterList"
            );


        newChapterButton.Pressed +=
            OnNewChapterPressed;
    }


    public void SetProjectPath(
        string projectPath)
    {
        if (string.IsNullOrWhiteSpace(projectPath))
        {
            GD.PrintErr(
                "ChaptersListView: la ruta del proyecto está vacía."
            );


            return;
        }


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


        Refresh();
    }


    private void OnNewChapterPressed()
    {
        EmitSignal(
            SignalName.ChapterSelected,
            ""
        );
    }


    public void Refresh()
    {
        if (chapterList == null)
        {
            return;
        }


        foreach (
            Node child
            in chapterList.GetChildren())
        {
            child.QueueFree();
        }


        if (chapterRepository == null)
        {
            return;
        }


        List<ChapterDefinitionData> chapters =
            chapterRepository.LoadAll();


        chapters.Sort(
            (a, b) =>
                a.Number.CompareTo(
                    b.Number
                )
        );


        foreach (
            ChapterDefinitionData chapter
            in chapters)
        {
            if (chapter == null)
            {
                continue;
            }


            CreateChapterItem(
                chapter
            );
        }
    }


    private void CreateChapterItem(
        ChapterDefinitionData chapter)
    {
        PanelContainer panel =
            new PanelContainer();


        panel.CustomMinimumSize =
            new Vector2(
                0,
                70
            );


        StyleBoxFlat panelStyle =
            new StyleBoxFlat();


        panelStyle.BgColor =
            new Color(
                "#222730"
            );


        panelStyle.BorderColor =
            new Color(
                "#313743"
            );


        panelStyle.SetBorderWidthAll(
            1
        );


        panelStyle.CornerRadiusTopLeft = 3;
        panelStyle.CornerRadiusTopRight = 3;
        panelStyle.CornerRadiusBottomLeft = 3;
        panelStyle.CornerRadiusBottomRight = 3;


        panel.AddThemeStyleboxOverride(
            "panel",
            panelStyle
        );


        MarginContainer margin =
            new MarginContainer();


        margin.AddThemeConstantOverride(
            "margin_left",
            12
        );


        margin.AddThemeConstantOverride(
            "margin_right",
            12
        );


        margin.AddThemeConstantOverride(
            "margin_top",
            8
        );


        margin.AddThemeConstantOverride(
            "margin_bottom",
            8
        );


        panel.AddChild(
            margin
        );


        HBoxContainer content =
            new HBoxContainer();


        content.AddThemeConstantOverride(
            "separation",
            12
        );


        margin.AddChild(
            content
        );


        Label numberLabel =
            new Label();


        numberLabel.Text =
            $"CAPÍTULO {chapter.Number:D2}";


        numberLabel.CustomMinimumSize =
            new Vector2(
                110,
                0
            );


        numberLabel.VerticalAlignment =
            VerticalAlignment.Center;


        numberLabel.AddThemeColorOverride(
            "font_color",
            new Color(
                "#6F7785"
            )
        );


        numberLabel.AddThemeFontSizeOverride(
            "font_size",
            11
        );


        content.AddChild(
            numberLabel
        );


        VBoxContainer information =
            new VBoxContainer();


        information.SizeFlagsHorizontal =
            Control.SizeFlags.ExpandFill;


        information.AddThemeConstantOverride(
            "separation",
            2
        );


        content.AddChild(
            information
        );


        Label titleLabel =
            new Label();


        titleLabel.Text =
            chapter.Title;


        titleLabel.AddThemeColorOverride(
            "font_color",
            new Color(
                "#E7EAF0"
            )
        );


        titleLabel.AddThemeFontSizeOverride(
            "font_size",
            15
        );


        information.AddChild(
            titleLabel
        );


        int attributeCount =
            chapter.AttributeIds?.Count ?? 0;


        int eventCount =
            GetEventCount(
                chapter.Number
            );


        int interludeCount =
            GetInterludeCount(
                chapter.Number
            );


        Label detailsLabel =
            new Label();


        detailsLabel.Text =
            $"Inicio: año {chapter.StartYear}, " +
            $"mes {chapter.StartMonth}  ·  " +
            $"{attributeCount} atributos  ·  " +
            $"{eventCount} eventos  ·  " +
            $"{interludeCount} interludios";


        detailsLabel.AddThemeColorOverride(
            "font_color",
            new Color(
                "#A8AFBC"
            )
        );


        detailsLabel.AddThemeFontSizeOverride(
            "font_size",
            11
        );


        information.AddChild(
            detailsLabel
        );


        Button editButton =
            new Button();


        editButton.Text =
            "EDITAR";


        editButton.CustomMinimumSize =
            new Vector2(
                80,
                36
            );


        editButton.AddThemeColorOverride(
            "font_color",
            new Color(
                "#A8AFBC"
            )
        );


        editButton.AddThemeColorOverride(
            "font_hover_color",
            new Color(
                "#E7EAF0"
            )
        );


        editButton.Pressed +=
            () =>
            {
                EmitSignal(
                    SignalName.ChapterSelected,
                    chapter.Id
                );
            };


        content.AddChild(
            editButton
        );


        chapterList.AddChild(
            panel
        );
    }


    private int GetEventCount(
        int chapterNumber)
    {
        if (eventRepository == null)
        {
            return 0;
        }


        return eventRepository
            .LoadAll()
            .Count(
                eventData =>
                    eventData != null &&
                    eventData.Chapter ==
                    chapterNumber
            );
    }


    private int GetInterludeCount(
        int chapterNumber)
    {
        if (interludeRepository == null)
        {
            return 0;
        }


        return interludeRepository
            .LoadAll()
            .Count(
                interlude =>
                    interlude != null &&
                    interlude.Chapter ==
                    chapterNumber
            );
    }
}