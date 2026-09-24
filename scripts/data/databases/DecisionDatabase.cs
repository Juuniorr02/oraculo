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
            cacheBuilt =
                true;

            return cachedDecisions;
        }


        List<EditorEventData> events =
            eventRepository.LoadAll();


        if (events == null)
        {
            cacheBuilt =
                true;

            return cachedDecisions;
        }


        HashSet<string> registeredIds =
            new HashSet<string>(
                System.StringComparer.OrdinalIgnoreCase
            );


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


            CollectDecisionEffects(
                eventData,
                eventData.Pages,
                registeredIds
            );
        }


        cacheBuilt =
            true;


        return cachedDecisions;
    }


    private void CollectDecisionEffects(
        EditorEventData eventData,
        List<EditorPageData> pages,
        HashSet<string> registeredIds)
    {
        if (
            eventData == null ||
            pages == null)
        {
            return;
        }


        for (
            int pageIndex = 0;
            pageIndex < pages.Count;
            pageIndex++)
        {
            EditorPageData page =
                pages[pageIndex];


            if (page == null)
            {
                continue;
            }


            if (page.Decisions == null)
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


                if (decision.Effects != null)
                {
                    foreach (
                        EditorEffectData effect
                        in decision.Effects)
                    {
                        if (effect == null)
                        {
                            continue;
                        }


                        if (
                            !string.Equals(
                                effect.TypeId,
                                "decision",
                                System.StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }


                        string decisionId =
                            effect.DecisionId?.Trim() ?? "";


                        if (string.IsNullOrWhiteSpace(
                            decisionId))
                        {
                            continue;
                        }


                        if (!registeredIds.Add(
                            decisionId))
                        {
                            continue;
                        }


                        cachedDecisions.Add(
                            new DecisionReference
                            {
                                Id =
                                    decisionId,

                                Text =
                                    decisionId,

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


                if (decision.Pages == null)
                {
                    continue;
                }


                CollectDecisionEffects(
                    eventData,
                    decision.Pages,
                    registeredIds
                );
            }
        }
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
                string.Equals(
                    decision.Id,
                    decisionId,
                    System.StringComparison.OrdinalIgnoreCase))
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
            GetDecision(
                decisionId
            );


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


        return
            $"{eventName} → Decisión → {decision.Id}";
    }


    public void InvalidateCache()
    {
        cachedDecisions =
            null;

        cacheBuilt =
            false;
    }


    public void Refresh()
    {
        InvalidateCache();

        GetAllDecisions();
    }
}