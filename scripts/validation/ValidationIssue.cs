public class ValidationIssue
{
    public ValidationIssueSeverity Severity { get; }

    public string Message { get; }

    public ValidationResourceType ResourceType { get; }

    public string ResourceId { get; }

    public int PageIndex { get; }

    public string PageId { get; }

    public int DecisionIndex { get; }

    public string DecisionId { get; }

    public int ConditionIndex { get; }

    public int EffectIndex { get; }


    public ValidationIssue(
        ValidationIssueSeverity severity,
        string message,
        ValidationResourceType resourceType =
            ValidationResourceType.Unknown,
        string resourceId = "",
        int pageIndex = -1,
        string pageId = "",
        int decisionIndex = -1,
        string decisionId = "",
        int conditionIndex = -1,
        int effectIndex = -1)
    {
        Severity =
            severity;

        Message =
            message;

        ResourceType =
            resourceType;

        ResourceId =
            resourceId;

        PageIndex =
            pageIndex;

        PageId =
            pageId;

        DecisionIndex =
            decisionIndex;

        DecisionId =
            decisionId;

        ConditionIndex =
            conditionIndex;

        EffectIndex =
            effectIndex;
    }


    public bool IsError()
    {
        return Severity ==
            ValidationIssueSeverity.Error;
    }


    public bool IsWarning()
    {
        return Severity ==
            ValidationIssueSeverity.Warning;
    }
}


public enum ValidationIssueSeverity
{
    Error,
    Warning
}


public enum ValidationResourceType
{
    Unknown,
    Event,
    Interlude
}