using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


public partial class AttributeEditor : Control
{
    [Signal]
    public delegate void AttributeSavedEventHandler();


    [Signal]
    public delegate void AttributeCancelledEventHandler();


    [Signal]
    public delegate void AttributeDeletedEventHandler();


    [Signal]
    public delegate void ApplicationCloseConfirmedEventHandler();


    private enum PendingAction
    {
        None,
        Delete
    }


    private AttributeRepository attributeRepository;
    private ChapterRepository chapterRepository;


    private string projectPath = "";
    private string attributeId = "";


    private bool creatingNewAttribute = false;
    private bool applicationCloseRequested = false;


    private PendingAction pendingAction =
        PendingAction.None;


    private AttributeDefinitionData attributeData;


    private Label titleLabel;
    private Label idLabel;


    private LineEdit displayNameEdit;
    private TextEdit descriptionEdit;


    private LineEdit negativeNameEdit;
    private LineEdit lowNameEdit;
    private LineEdit neutralNameEdit;
    private LineEdit highNameEdit;
    private LineEdit positiveNameEdit;


    private Button deleteButton;
    private Button cancelButton;
    private Button saveButton;
    private Button closeButton;


    private UnsavedChangesGuard<AttributeDefinitionData>
        unsavedChangesGuard;


    public override void _Ready()
    {
        titleLabel =
            GetNode<Label>(
                "VBoxContainer/Header/Title"
            );


        idLabel =
            GetNode<Label>(
                "VBoxContainer/AttributeInfo/IdContainer/Id"
            );


        displayNameEdit =
            GetNode<LineEdit>(
                "VBoxContainer/AttributeInfo/DisplayNameContainer/DisplayNameEdit"
            );


        descriptionEdit =
            GetNode<TextEdit>(
                "VBoxContainer/AttributeInfo/DescriptionContainer/DescriptionEdit"
            );


        negativeNameEdit =
            GetNode<LineEdit>(
                "VBoxContainer/Values/NegativeContainer/NegativeEdit"
            );


        lowNameEdit =
            GetNode<LineEdit>(
                "VBoxContainer/Values/LowContainer/LowEdit"
            );


        neutralNameEdit =
            GetNode<LineEdit>(
                "VBoxContainer/Values/NeutralContainer/NeutralEdit"
            );


        highNameEdit =
            GetNode<LineEdit>(
                "VBoxContainer/Values/HighContainer/HighEdit"
            );


        positiveNameEdit =
            GetNode<LineEdit>(
                "VBoxContainer/Values/PositiveContainer/PositiveEdit"
            );


        deleteButton =
            GetNode<Button>(
                "VBoxContainer/Header/DeleteButton"
            );


        cancelButton =
            GetNode<Button>(
                "VBoxContainer/Buttons/CancelButton"
            );


        saveButton =
            GetNode<Button>(
                "VBoxContainer/Buttons/SaveButton"
            );


        closeButton =
            GetNode<Button>(
                "VBoxContainer/Header/CloseButton"
            );


        deleteButton.Pressed +=
            OnDeletePressed;


        cancelButton.Pressed +=
            OnCancelPressed;


        saveButton.Pressed +=
            OnSavePressed;


        closeButton.Pressed +=
            OnCancelPressed;


        unsavedChangesGuard =
            new UnsavedChangesGuard<AttributeDefinitionData>(
                this,
                GetCurrentAttributeState,
                CloseEditor
            );
    }


    public void SetProjectPath(
        string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            GD.PrintErr(
                "AttributeEditor: la ruta del proyecto está vacía."
            );


            return;
        }


        projectPath =
            path;


        attributeRepository =
            new AttributeRepository(
                projectPath
            );


        chapterRepository =
            new ChapterRepository(
                projectPath
            );
    }


    public void CreateNewAttribute()
    {
        if (attributeRepository == null)
        {
            GD.PrintErr(
                "AttributeEditor: AttributeRepository no está disponible."
            );


            return;
        }


        int nextNumber =
            GetNextAttributeNumber();


        string newId =
            GenerateAttributeId(
                nextNumber
            );


        creatingNewAttribute =
            true;


        attributeId =
            newId;


        attributeData =
            new AttributeDefinitionData
            {
                Id = newId,
                DisplayName = "",
                Description = "",
                NegativeName = "",
                LowName = "",
                NeutralName = "Neutral",
                HighName = "",
                PositiveName = ""
            };


        PopulateFields();


        titleLabel.Text =
            "Nuevo atributo";


        deleteButton.Disabled =
            true;


        SaveOriginalState();
    }


    public void LoadAttribute(
        string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            GD.PrintErr(
                "AttributeEditor: el ID del atributo está vacío."
            );


            return;
        }


        if (attributeRepository == null)
        {
            GD.PrintErr(
                "AttributeEditor: AttributeRepository no está disponible."
            );


            return;
        }


        AttributeDefinitionData loadedAttribute =
            attributeRepository.Load(
                id
            );


        if (loadedAttribute == null)
        {
            GD.PrintErr(
                "AttributeEditor: no se encontró el atributo: ",
                id
            );


            return;
        }


        if (string.IsNullOrWhiteSpace(
            loadedAttribute.NeutralName))
        {
            loadedAttribute.NeutralName =
                "Neutral";
        }


        creatingNewAttribute =
            false;


        attributeId =
            loadedAttribute.Id;


        attributeData =
            loadedAttribute;


        PopulateFields();


        titleLabel.Text =
            $"Editor de atributo · " +
            $"{attributeData.Id}";


        deleteButton.Disabled =
            false;


        SaveOriginalState();
    }


    private void PopulateFields()
    {
        if (attributeData == null)
        {
            return;
        }


        idLabel.Text =
            attributeData.Id;


        displayNameEdit.Text =
            attributeData.DisplayName;


        descriptionEdit.Text =
            attributeData.Description;


        negativeNameEdit.Text =
            attributeData.NegativeName;


        lowNameEdit.Text =
            attributeData.LowName;


        neutralNameEdit.Text =
            attributeData.NeutralName;


        highNameEdit.Text =
            attributeData.HighName;


        positiveNameEdit.Text =
            attributeData.PositiveName;
    }


    private void SavePendingFields()
    {
        if (attributeData == null)
        {
            return;
        }


        attributeData.DisplayName =
            displayNameEdit.Text.Trim();


        attributeData.Description =
            descriptionEdit.Text.Trim();


        attributeData.NegativeName =
            negativeNameEdit.Text.Trim();


        attributeData.LowName =
            lowNameEdit.Text.Trim();


        attributeData.NeutralName =
            neutralNameEdit.Text.Trim();


        attributeData.HighName =
            highNameEdit.Text.Trim();


        attributeData.PositiveName =
            positiveNameEdit.Text.Trim();
    }


    private AttributeDefinitionData
        GetCurrentAttributeState()
    {
        SavePendingFields();


        return attributeData;
    }


    private void SaveOriginalState()
    {
        if (
            unsavedChangesGuard == null ||
            attributeData == null)
        {
            return;
        }


        unsavedChangesGuard.SaveOriginalState(
            attributeData
        );
    }
	public void SaveCurrentAttribute()
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


        pendingAction =
            PendingAction.None;


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
        if (
            pendingAction ==
            PendingAction.Delete)
        {
            pendingAction =
                PendingAction.None;


            DeleteCurrentAttribute();


            return;
        }


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
            SignalName.AttributeCancelled
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


    private void OnSavePressed()
    {
        if (
            attributeData == null ||
            attributeRepository == null)
        {
            GD.PrintErr(
                "AttributeEditor: no se puede guardar el atributo."
            );


            return;
        }


        SavePendingFields();


        if (string.IsNullOrWhiteSpace(
            attributeData.Id))
        {
            GD.PrintErr(
                "AttributeEditor: el atributo no tiene ID."
            );


            return;
        }


        if (string.IsNullOrWhiteSpace(
            attributeData.DisplayName))
        {
            GD.PrintErr(
                "AttributeEditor: el atributo necesita un nombre."
            );


            return;
        }


        if (
            creatingNewAttribute &&
            attributeRepository.Exists(
                attributeData.Id))
        {
            GD.PrintErr(
                "AttributeEditor: ya existe un atributo con el ID '",
                attributeData.Id,
                "'."
            );


            return;
        }


        bool saved =
            attributeRepository.Save(
                attributeData
            );


        if (!saved)
        {
            GD.PrintErr(
                "AttributeEditor: no se pudo guardar el atributo."
            );


            return;
        }


        creatingNewAttribute =
            false;


        attributeId =
            attributeData.Id;


        titleLabel.Text =
            $"Editor de atributo · " +
            $"{attributeData.Id}";


        deleteButton.Disabled =
            false;


        SaveOriginalState();


        EmitSignal(
            SignalName.AttributeSaved
        );
    }


    private void OnCancelPressed()
    {
        RequestClose();
    }


    private void OnDeletePressed()
    {
        if (
            attributeData == null ||
            attributeRepository == null ||
            creatingNewAttribute)
        {
            return;
        }


        int usedByChapters =
            CountChaptersUsingAttribute(
                attributeData.Id
            );


        ConfirmationDialog confirmation =
            new ConfirmationDialog();


        confirmation.Title =
            "Eliminar atributo";


        if (usedByChapters > 0)
        {
            confirmation.DialogText =
                $"El atributo \"{GetAttributeName()}\" " +
                $"está siendo usado por {usedByChapters} " +
                $"{(usedByChapters == 1 ? "capítulo" : "capítulos")}.\n\n" +
                "Si lo eliminas, esos capítulos conservarán la referencia " +
                "al atributo, pero el atributo dejará de existir.\n\n" +
                "¿Quieres continuar?";
        }
        else
        {
            confirmation.DialogText =
                $"¿Seguro que quieres eliminar el atributo " +
                $"\"{GetAttributeName()}\"?\n\n" +
                "Esta acción no se puede deshacer.";
        }


        confirmation.OkButtonText =
            "Eliminar";


        confirmation.CancelButtonText =
            "Cancelar";


        confirmation.Confirmed +=
            () =>
            {
                confirmation.QueueFree();


                pendingAction =
                    PendingAction.Delete;


                RequestClose();
            };


        confirmation.Canceled +=
            () =>
            {
                confirmation.QueueFree();
            };


        AddChild(
            confirmation
        );


        confirmation.PopupCentered();
    }


    private string GetAttributeName()
    {
        if (attributeData == null)
        {
            return "";
        }


        if (!string.IsNullOrWhiteSpace(
            attributeData.DisplayName))
        {
            return attributeData.DisplayName.Trim();
        }


        return attributeData.Id;
    }


    private int CountChaptersUsingAttribute(
        string id)
    {
        if (
            chapterRepository == null ||
            string.IsNullOrWhiteSpace(id))
        {
            return 0;
        }


        int count =
            0;


        foreach (
            ChapterDefinitionData chapter
            in chapterRepository.LoadAll())
        {
            if (
                chapter == null ||
                chapter.AttributeIds == null)
            {
                continue;
            }


            bool found =
                chapter.AttributeIds.Any(
                    attributeId =>
                        string.Equals(
                            attributeId,
                            id,
                            StringComparison.OrdinalIgnoreCase
                        )
                );


            if (found)
            {
                count++;
            }
        }


        return count;
    }


    private void DeleteCurrentAttribute()
    {
        if (
            attributeData == null ||
            attributeRepository == null)
        {
            return;
        }


        string id =
            attributeData.Id;


        if (!attributeRepository.Exists(id))
        {
            EmitSignal(
                SignalName.AttributeCancelled
            );


            return;
        }


        bool deleted =
            attributeRepository.Delete(
                id
            );


        if (!deleted)
        {
            GD.PrintErr(
                "AttributeEditor: no se pudo eliminar el atributo: ",
                id
            );


            return;
        }


        GD.Print(
            "AttributeEditor: atributo eliminado: ",
            id
        );


        attributeData =
            null;


        attributeId =
            "";


        creatingNewAttribute =
            false;


        EmitSignal(
            SignalName.AttributeDeleted
        );
    }


    private int GetNextAttributeNumber()
    {
        if (attributeRepository == null)
        {
            return 1;
        }


        int maxNumber =
            0;


        foreach (
            string id
            in attributeRepository.GetAttributeIds())
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                continue;
            }


            if (!id.StartsWith(
                "attribute_",
                StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }


            string numberText =
                id.Substring(
                    "attribute_".Length
                );


            if (numberText.Contains("_"))
            {
                numberText =
                    numberText.Split('_')[0];
            }


            if (int.TryParse(
                numberText,
                out int number))
            {
                maxNumber =
                    Math.Max(
                        maxNumber,
                        number
                    );
            }
        }


        return maxNumber + 1;
    }


    private string GenerateAttributeId(
        int number)
    {
        string baseId =
            $"attribute_{number:D2}";


        string candidate =
            baseId;


        int suffix =
            2;


        while (
            attributeRepository != null &&
            attributeRepository.Exists(
                candidate))
        {
            candidate =
                $"{baseId}_{suffix}";


            suffix++;
        }


        return candidate;
    }
}