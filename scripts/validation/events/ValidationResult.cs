using System.Collections.Generic;

public class ValidationResult
{
    private readonly List<string> errors = new();
    private readonly List<string> warnings = new();


    public IReadOnlyList<string> Errors =>
        errors;


    public IReadOnlyList<string> Warnings =>
        warnings;


    public bool IsValid =>
        errors.Count == 0;


    public void AddError(
        string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        errors.Add(
            message
        );
    }


    public void AddWarning(
        string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        warnings.Add(
            message
        );
    }


    public void Merge(
        ValidationResult other)
    {
        if (other == null)
        {
            return;
        }


        foreach (
            string error
            in other.Errors)
        {
            AddError(
                error
            );
        }


        foreach (
            string warning
            in other.Warnings)
        {
            AddWarning(
                warning
            );
        }
    }
}