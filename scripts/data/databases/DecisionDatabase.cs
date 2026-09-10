using System.Collections.Generic;

public class DecisionDatabase
{
    public sealed class DecisionReference
    {
        public string Id { get; set; } = "";

        public string Text { get; set; } = "";

        public string EventId { get; set; } = "";

        public string EventTitle { get; set; } = "";

        public string PageId { get; set; } = "";

        public int PageNumber { get; set; }
    }


    private readonly EventRepository eventRepository;

    private List<DecisionReference> cachedDecisions;

    private bool cacheBuilt = false;


    public DecisionDatabase(
        EventRepository repository)
    {
        eventRepository =
            repository;
    }


    public List<DecisionReference> GetAllDecisions()
    {
        if (cacheBuilt)
        {
            return cachedDecisions;
        }


        cachedDecisions =
            new List<DecisionReference>();


        if (eventRepository == null)
        {
            cacheBuilt = true;

            return cachedDecisions;
        }


        List<EditorEventData> events =
            eventRepository.LoadAll();


        if (events == null)
        {
            cacheBuilt = true;

            return cachedDecisions;
        }


        foreach (
            EditorEventData eventData
            in events)
        {
            if (eventData == null)
            {
                continue;
            }


            if (eventData.Pages == null)
            {
                continue;
            }


            for (
                int pageIndex = 0;
                pageIndex < eventData.Pages.Count;
                pageIndex++)
            {
                EditorPageData page =
                    eventData.Pages[pageIndex];


                if (
                    page == null ||
                    page.Decisions == null)
                {
                    continue;
                }


                foreach (
                    EditorDecisionData decision
                    in page.Decisions)
                {
                    if (decision == null)
                    {
                        continue;
                    }


                    if (
                        string.IsNullOrWhiteSpace(
                            decision.Id))
                    {
                        continue;
                    }


                    cachedDecisions.Add(
                        new DecisionReference
                        {
                            Id =
                                decision.Id,

                            Text =
                                decision.Text,

                            EventId =
                                eventData.Id,

                            EventTitle =
                                eventData.Title,

                            PageId =
                                page.Id,

                            PageNumber =
                                pageIndex + 1
                        }
                    );
                }
            }
        }


        cacheBuilt = true;

        return cachedDecisions;
    }


    public DecisionReference GetDecision(
        string decisionId)
    {
        if (
            string.IsNullOrWhiteSpace(
                decisionId))
        {
            return null;
        }


        List<DecisionReference> decisions =
            GetAllDecisions();


        foreach (
            DecisionReference decision
            in decisions)
        {
            if (
                decision.Id ==
                decisionId)
            {
                return decision;
            }
        }


        return null;
    }


    public string GetDecisionDisplayName(
        string decisionId)
    {
        DecisionReference decision =
            GetDecision(decisionId);


        if (decision == null)
        {
            return string.IsNullOrWhiteSpace(
                decisionId)
                ? "Decisión no encontrada"
                : $"Decisión no encontrada ({decisionId})";
        }


        string eventName =
            string.IsNullOrWhiteSpace(
                decision.EventTitle)
                ? decision.EventId
                : decision.EventTitle;


        string decisionText =
            string.IsNullOrWhiteSpace(
                decision.Text)
                ? "Decisión sin texto"
                : decision.Text;


        return
            $"{eventName} → Página {decision.PageNumber} → {decisionText}";
    }


    public void InvalidateCache()
    {
        cachedDecisions = null;

        cacheBuilt = false;
    }


    public void Refresh()
    {
        InvalidateCache();

        GetAllDecisions();
    }
}