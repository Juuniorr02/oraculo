using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


public partial class AttributeListView : Control
{
    [Signal]
    public delegate void AttributeSelectedEventHandler(
        string attributeId
    );


    private AttributeRepository attributeRepository;


    private LineEdit searchEdit;
    private Button newAttributeButton;
    private VBoxContainer attributeList;


    private List<AttributeDefinitionData> allAttributes =
        new List<AttributeDefinitionData>();


    public override void _Ready()
    {
        searchEdit =
            GetNode<LineEdit>(
                "MarginContainer/VBoxContainer/Header/MarginContainer/HBoxContainer/SearchEdit"
            );


        newAttributeButton =
            GetNode<Button>(
                "MarginContainer/VBoxContainer/Header/MarginContainer/HBoxContainer/NewAttributeButton"
            );


        attributeList =
            GetNode<VBoxContainer>(
"MarginContainer/VBoxContainer/ScrollContainer/AttributeList"            );


        searchEdit.TextChanged +=
            OnSearchChanged;


        newAttributeButton.Pressed +=
            OnNewAttributePressed;
    }


    public void SetProjectPath(
        string projectPath)
    {
        if (string.IsNullOrWhiteSpace(projectPath))
        {
            GD.PrintErr(
                "AttributeListView: la ruta del proyecto está vacía."
            );


            return;
        }


        attributeRepository =
            new AttributeRepository(
                projectPath
            );


        Refresh();
    }


    private void OnNewAttributePressed()
    {
        EmitSignal(
            SignalName.AttributeSelected,
            ""
        );
    }


    public void Refresh()
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


        allAttributes.Clear();


        if (attributeRepository == null)
        {
            return;
        }


        allAttributes =
            attributeRepository
                .LoadAll()
                .Where(
                    attribute =>
                        attribute != null
                )
                .OrderBy(
                    attribute =>
                        GetDisplayName(attribute),
                    StringComparer.OrdinalIgnoreCase
                )
                .ToList();


        string searchText =
            searchEdit?.Text.Trim() ?? "";


        foreach (
            AttributeDefinitionData attribute
            in allAttributes)
        {
            if (!MatchesSearch(
                attribute,
                searchText))
            {
                continue;
            }


            CreateAttributeItem(
                attribute
            );
        }
    }


    private bool MatchesSearch(
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


    private void CreateAttributeItem(
        AttributeDefinitionData attribute)
    {
        PanelContainer panel =
            new PanelContainer();


        panel.CustomMinimumSize =
            new Vector2(
                0,
                78
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
            GetDisplayName(
                attribute
            );


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


        Label valuesLabel =
            new Label();


        valuesLabel.Text =
            BuildValuesText(
                attribute
            );


        valuesLabel.AddThemeColorOverride(
            "font_color",
            new Color(
                "#A8AFBC"
            )
        );


        valuesLabel.AddThemeFontSizeOverride(
            "font_size",
            11
        );


        information.AddChild(
            valuesLabel
        );


        string description =
            attribute.Description?.Trim() ?? "";


        if (!string.IsNullOrWhiteSpace(
            description))
        {
            Label descriptionLabel =
                new Label();


            descriptionLabel.Text =
                description;


            descriptionLabel.AutowrapMode =
                TextServer.AutowrapMode.WordSmart;


            descriptionLabel.AddThemeColorOverride(
                "font_color",
                new Color(
                    "#6F7785"
                )
            );


            descriptionLabel.AddThemeFontSizeOverride(
                "font_size",
                10
            );


            information.AddChild(
                descriptionLabel
            );
        }


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
                    SignalName.AttributeSelected,
                    attribute.Id
                );
            };


        content.AddChild(
            editButton
        );


        attributeList.AddChild(
            panel
        );
    }


    private string GetDisplayName(
        AttributeDefinitionData attribute)
    {
        if (attribute == null)
        {
            return "";
        }


        if (!string.IsNullOrWhiteSpace(
            attribute.DisplayName))
        {
            return attribute.DisplayName.Trim();
        }


        return attribute.Id ?? "";
    }


    private string BuildValuesText(
        AttributeDefinitionData attribute)
    {
        List<string> values =
            new List<string>();


        if (!string.IsNullOrWhiteSpace(
            attribute.NegativeName))
        {
            values.Add(
                attribute.NegativeName
            );
        }


        if (!string.IsNullOrWhiteSpace(
            attribute.LowName))
        {
            values.Add(
                attribute.LowName
            );
        }


        if (!string.IsNullOrWhiteSpace(
            attribute.NeutralName))
        {
            values.Add(
                attribute.NeutralName
            );
        }


        if (!string.IsNullOrWhiteSpace(
            attribute.HighName))
        {
            values.Add(
                attribute.HighName
            );
        }


        if (!string.IsNullOrWhiteSpace(
            attribute.PositiveName))
        {
            values.Add(
                attribute.PositiveName
            );
        }


        if (values.Count == 0)
        {
            return attribute.Id ?? "";
        }


        return
            string.Join(
                "  ·  ",
                values
            );
    }


    private void OnSearchChanged(
        string newText)
    {
        Refresh();
    }
}