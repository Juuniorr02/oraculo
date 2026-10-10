
using System;
using Godot;

public partial class LocalizationPanel : Control
{
    private string _projectPath = "";

    private Label _projectLabel = null!;
    private Label _summaryLabel = null!;
    private RichTextLabel _issuesOutput = null!;

    private Button _validateButton = null!;
    private Button _exportButton = null!;
    private Button _clearButton = null!;

    public override void _Ready()
    {
        BuildInterface();
        SetProjectPath(_projectPath);
    }

    public void SetProjectPath(string path)
    {
        _projectPath = path ?? "";

        if (_projectLabel == null)
            return;

        _projectLabel.Text = string.IsNullOrWhiteSpace(_projectPath)
            ? "No hay ningún proyecto seleccionado."
            : $"Proyecto: {_projectPath}";

        bool hasProject = !string.IsNullOrWhiteSpace(_projectPath);

        _validateButton.Disabled = !hasProject;
        _exportButton.Disabled = !hasProject;
    }

    private void BuildInterface()
    {
        var margin = new MarginContainer();
        margin.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

        margin.AddThemeConstantOverride("margin_left", 20);
        margin.AddThemeConstantOverride("margin_right", 20);
        margin.AddThemeConstantOverride("margin_top", 20);
        margin.AddThemeConstantOverride("margin_bottom", 20);

        AddChild(margin);

        var layout = new VBoxContainer();
        layout.AddThemeConstantOverride("separation", 12);
        margin.AddChild(layout);

        var title = new Label
        {
            Text = "Localización del proyecto"
        };

        title.AddThemeFontSizeOverride("font_size", 24);
        layout.AddChild(title);

        var description = new Label
        {
            Text =
                "Comprueba las claves del catálogo español o actualízalo " +
                "con los textos del proyecto. La validación no modifica archivos.",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };

        layout.AddChild(description);

        _projectLabel = new Label
        {
            Text = "No hay ningún proyecto seleccionado.",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };

        layout.AddChild(_projectLabel);

        var actions = new HBoxContainer();
        actions.AddThemeConstantOverride("separation", 8);
        layout.AddChild(actions);

        _validateButton = new Button
        {
            Text = "Validar catálogo",
            TooltipText = "Comprueba el contenido y es.json sin guardar cambios."
        };

        _validateButton.Pressed += OnValidatePressed;
        actions.AddChild(_validateButton);

        _exportButton = new Button
        {
            Text = "Extraer y actualizar español",
            TooltipText = "Añade las claves nuevas y conserva las traducciones existentes."
        };

        _exportButton.Pressed += OnExportPressed;
        actions.AddChild(_exportButton);

        _clearButton = new Button
        {
            Text = "Limpiar resultados"
        };

        _clearButton.Pressed += OnClearPressed;
        actions.AddChild(_clearButton);

        _summaryLabel = new Label
        {
            Text = "Todavía no se ha ejecutado ninguna operación.",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };

        layout.AddChild(_summaryLabel);

        layout.AddChild(new HSeparator());

        var issuesTitle = new Label
        {
            Text = "Incidencias y resultados"
        };

        layout.AddChild(issuesTitle);

        var scroll = new ScrollContainer
        {
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };

        layout.AddChild(scroll);

        _issuesOutput = new RichTextLabel
        {
            FitContent = false,
            ScrollFollowing = true,
            BbcodeEnabled = false,
            SelectionEnabled = true,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };

        scroll.AddChild(_issuesOutput);
    }

    private void OnValidatePressed()
    {
        RunOperation(validationOnly: true);
    }

    private void OnExportPressed()
    {
        RunOperation(validationOnly: false);
    }

    private void RunOperation(bool validationOnly)
    {
        if (string.IsNullOrWhiteSpace(_projectPath))
        {
            _summaryLabel.Text = "No hay ningún proyecto cargado.";
            _issuesOutput.Text = "Abre un proyecto antes de continuar.";
            return;
        }

        SetActionButtonsDisabled(true);

        _summaryLabel.Text = validationOnly
            ? "Validando contenido y catálogo..."
            : "Extrayendo textos y preparando el catálogo...";

        _issuesOutput.Text = "";

        try
        {
            var exporter = new LocalizationExporter(_projectPath);

            LocalizationExportResult result = validationOnly
                ? exporter.ValidateSpanish()
                : exporter.ExportSpanish();

            _summaryLabel.Text = result.ToString();
            ShowResults(result);
        }
        catch (Exception exception)
        {
            _summaryLabel.Text = "La operación ha fallado.";
            _issuesOutput.Text = exception.Message;
        }
        finally
        {
            SetActionButtonsDisabled(false);
        }
    }

    private void ShowResults(LocalizationExportResult result)
    {
        _issuesOutput.Clear();

        foreach (LocalizationIssue issue in result.Issues)
        {
            _issuesOutput.AppendText(issue + "\n");
        }

        if (result.HasErrors)
        {
            _issuesOutput.AppendText(
                "\nOperación incompleta. Corrige los errores indicados y vuelve a intentarlo."
            );

            return;
        }

        if (result.ValidationOnly)
        {
            if (result.Issues.Count == 0)
            {
                _issuesOutput.Text =
                    "Validación completada sin incidencias. No se ha modificado ningún archivo.";
            }
            else
            {
                _issuesOutput.AppendText(
                    "\nValidación completada. Revisa los avisos anteriores. No se ha modificado ningún archivo."
                );
            }

            return;
        }

        if (result.Saved)
        {
            _issuesOutput.AppendText(
                "\n\nCatálogo español actualizado correctamente.\n" +
                $"Archivo: {_projectPath}/localization/es.json"
            );
        }
        else
        {
            _issuesOutput.AppendText(
                "\nNo se ha guardado ningún archivo."
            );
        }
    }

    private void SetActionButtonsDisabled(bool disabled)
    {
        _validateButton.Disabled =
            disabled || string.IsNullOrWhiteSpace(_projectPath);

        _exportButton.Disabled =
            disabled || string.IsNullOrWhiteSpace(_projectPath);

        _clearButton.Disabled = disabled;
    }

    private void OnClearPressed()
    {
        _summaryLabel.Text =
            "Todavía no se ha ejecutado ninguna operación.";

        _issuesOutput.Clear();
    }
}
