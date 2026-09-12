using Godot;
using System;
using System.Text.Json;

public class UnsavedChangesGuard<T>
{
    private readonly Control owner;
    private readonly Func<T> getCurrentState;
    private readonly Action closeAction;

    private readonly ConfirmationDialog confirmationDialog;

    private T originalState;

    private Action pendingConfirmedAction;

    private readonly JsonSerializerOptions jsonOptions =
        new JsonSerializerOptions();


    public UnsavedChangesGuard(
        Control owner,
        Func<T> getCurrentState,
        Action closeAction)
    {
        this.owner =
            owner;

        this.getCurrentState =
            getCurrentState;

        this.closeAction =
            closeAction;


        confirmationDialog =
            new ConfirmationDialog();

        confirmationDialog.Title =
            "Cambios sin guardar";

        confirmationDialog.OkButtonText =
            "Descartar cambios";

        confirmationDialog.CancelButtonText =
            "Seguir editando";

        confirmationDialog.Confirmed +=
            OnDiscardChangesConfirmed;


        owner.AddChild(
            confirmationDialog
        );
    }


    public void SaveOriginalState(
        T state)
    {
        originalState =
            DeepClone(
                state
            );
    }


    public bool HasUnsavedChanges()
    {
        T currentState =
            getCurrentState();

        return !StatesAreEqual(
            currentState,
            originalState
        );
    }


    public void RequestClose()
    {
        RequestClose(
            "Hay cambios sin guardar.\n\n" +
            "¿Quieres descartarlos y cerrar el editor?",
            closeAction
        );
    }


    public void RequestClose(
        string message,
        Action confirmedAction)
    {
        if (!HasUnsavedChanges())
        {
            confirmedAction();
            return;
        }


        pendingConfirmedAction =
            confirmedAction;


        confirmationDialog.DialogText =
            message;


        confirmationDialog.PopupCentered();
    }


    private void OnDiscardChangesConfirmed()
    {
        Action action =
            pendingConfirmedAction;


        pendingConfirmedAction =
            null;


        if (action != null)
        {
            action();
            return;
        }


        closeAction();
    }


    private bool StatesAreEqual(
        T first,
        T second)
    {
        if (first == null &&
            second == null)
        {
            return true;
        }

        if (first == null ||
            second == null)
        {
            return false;
        }


        string firstJson =
            JsonSerializer.Serialize(
                first,
                jsonOptions
            );


        string secondJson =
            JsonSerializer.Serialize(
                second,
                jsonOptions
            );


        return firstJson == secondJson;
    }


    private T DeepClone(
        T source)
    {
        if (source == null)
        {
            return default;
        }


        string json =
            JsonSerializer.Serialize(
                source,
                jsonOptions
            );


        return JsonSerializer.Deserialize<T>(
            json,
            jsonOptions
        );
    }
}