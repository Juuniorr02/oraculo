using System.Collections.Generic;

public class ValidationResult
{
    private readonly List<ValidationIssue> issues =
        new List<ValidationIssue>();


    public IReadOnlyList<ValidationIssue> Issues =>
        issues;


    public IReadOnlyList<string> Errors
    {
        get
        {
            List<string> errors =
                new List<string>();

            foreach (
                ValidationIssue issue
                in issues)
            {
                if (issue.IsError())
                {
                    errors.Add(
                        issue.Message
                    );
                }
            }

            return errors;
        }
    }


    public IReadOnlyList<string> Warnings
    {
        get
        {
            List<string> warnings =
                new List<string>();

            foreach (
                ValidationIssue issue
                in issues)
            {
                if (issue.IsWarning())
                {
                    warnings.Add(
                        issue.Message
                    );
                }
            }

            return warnings;
        }
    }


    public bool IsValid =>
        GetErrorCount() == 0;


    // --------------------------------------------------
    // COMPATIBILIDAD CON EL SISTEMA ACTUAL
    // --------------------------------------------------

    public void AddError(
        string message,
        string eventId = "",
        int pageIndex = -1,
        string pageId = "",
        int decisionIndex = -1,
        string decisionId = "",
        int conditionIndex = -1,
        int effectIndex = -1)
    {
        AddIssue(
            new ValidationIssue(
                ValidationIssueSeverity.Error,
                message,
                string.IsNullOrWhiteSpace(eventId)
                    ? ValidationResourceType.Unknown
                    : ValidationResourceType.Event,
                eventId,
                pageIndex,
                pageId,
                decisionIndex,
                decisionId,
                conditionIndex,
                effectIndex
            )
        );
    }


    public void AddWarning(
        string message,
        string eventId = "",
        int pageIndex = -1,
        string pageId = "",
        int decisionIndex = -1,
        string decisionId = "",
        int conditionIndex = -1,
        int effectIndex = -1)
    {
        AddIssue(
            new ValidationIssue(
                ValidationIssueSeverity.Warning,
                message,
                string.IsNullOrWhiteSpace(eventId)
                    ? ValidationResourceType.Unknown
                    : ValidationResourceType.Event,
                eventId,
                pageIndex,
                pageId,
                decisionIndex,
                decisionId,
                conditionIndex,
                effectIndex
            )
        );
    }


    // --------------------------------------------------
    // NUEVA API GENÉRICA
    // --------------------------------------------------

    public void AddError(
        string message,
        ValidationResourceType resourceType,
        string resourceId = "",
        int pageIndex = -1,
        string pageId = "",
        int decisionIndex = -1,
        string decisionId = "",
        int conditionIndex = -1,
        int effectIndex = -1)
    {
        AddIssue(
            new ValidationIssue(
                ValidationIssueSeverity.Error,
                message,
                resourceType,
                resourceId,
                pageIndex,
                pageId,
                decisionIndex,
                decisionId,
                conditionIndex,
                effectIndex
            )
        );
    }


    public void AddWarning(
        string message,
        ValidationResourceType resourceType,
        string resourceId = "",
        int pageIndex = -1,
        string pageId = "",
        int decisionIndex = -1,
        string decisionId = "",
        int conditionIndex = -1,
        int effectIndex = -1)
    {
        AddIssue(
            new ValidationIssue(
                ValidationIssueSeverity.Warning,
                message,
                resourceType,
                resourceId,
                pageIndex,
                pageId,
                decisionIndex,
                decisionId,
                conditionIndex,
                effectIndex
            )
        );
    }


    public void AddIssue(
        ValidationIssue issue)
    {
        if (issue == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(
            issue.Message))
        {
            return;
        }

        issues.Add(
            issue
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
            ValidationIssue issue
            in other.Issues)
        {
            AddIssue(
                issue
            );
        }
    }


    public int GetErrorCount()
    {
        int count = 0;

        foreach (
            ValidationIssue issue
            in issues)
        {
            if (issue.IsError())
            {
                count++;
            }
        }

        return count;
    }


    public int GetWarningCount()
    {
        int count = 0;

        foreach (
            ValidationIssue issue
            in issues)
        {
            if (issue.IsWarning())
            {
                count++;
            }
        }

        return count;
    }
}