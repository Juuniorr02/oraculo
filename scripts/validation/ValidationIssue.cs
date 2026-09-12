public class ValidationIssue
{
    public ValidationIssueSeverity Severity { get; }

    public string Message { get; }

    public string EventId { get; }

    public int PageIndex { get; }

    public string PageId { get; }

    public int DecisionIndex { get; }

    public string DecisionId { get; }


    public ValidationIssue(
        ValidationIssueSeverity severity,
        string message,
        string eventId = "",
        int pageIndex = -1,
        string pageId = "",
        int decisionIndex = -1,
        string decisionId = "")
    {
        Severity =
            severity;

        Message =
            message;

        EventId =
            eventId;

        PageIndex =
            pageIndex;

        PageId =
            pageId;

        DecisionIndex =
            decisionIndex;

        DecisionId =
            decisionId;
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