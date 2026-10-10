using Godot;
using System;
using System.Collections.Generic;


internal static class EventMapLayout
{
    private const float TypeSpacingX = 520.0f;

    private const float LeftMargin = 80.0f;

    private const float TopMargin = 100.0f;

    private const float EventSpacingY = 320.0f;

    private const float InterludeSpacingY = 300.0f;

    private const float DecisionSpacingY = 320.0f;

    private const float FlagSpacingY = 270.0f;

    private const float EventGroupGap = 180.0f;

    private const float ChapterGap = 450.0f;


    // ============================================================
    // MAIN LAYOUT
    // ============================================================

    public static void PositionNodes(
        List<MapNodeData> visibleNodes,
        Dictionary<string, GraphNode> graphNodes)
    {
        if (
            visibleNodes == null ||
            graphNodes == null)
        {
            return;
        }


        Dictionary<int, int> eventCounters =
            new Dictionary<int, int>();

        Dictionary<int, int> interludeCounters =
            new Dictionary<int, int>();

        Dictionary<int, int> decisionCounters =
            new Dictionary<int, int>();

        Dictionary<int, int> flagCounters =
            new Dictionary<int, int>();


        Dictionary<int, int> eventCounts =
            new Dictionary<int, int>();

        Dictionary<int, int> interludeCounts =
            new Dictionary<int, int>();

        Dictionary<int, int> decisionCounts =
            new Dictionary<int, int>();

        Dictionary<int, int> flagCounts =
            new Dictionary<int, int>();


        HashSet<int> chapters =
            new HashSet<int>();


        foreach (
            MapNodeData node
            in visibleNodes)
        {
            if (node == null)
            {
                continue;
            }


            int chapter =
                NormalizeChapter(
                    node.Chapter
                );


            chapters.Add(
                chapter
            );


            switch (node.Type)
            {
                case MapNodeType.Event:

                    IncrementCounter(
                        eventCounts,
                        chapter
                    );

                    break;


                case MapNodeType.Interlude:

                    IncrementCounter(
                        interludeCounts,
                        chapter
                    );

                    break;


                case MapNodeType.Decision:

                    IncrementCounter(
                        decisionCounts,
                        chapter
                    );

                    break;


                case MapNodeType.PersistentDecision:

                    IncrementCounter(
                        flagCounts,
                        chapter
                    );

                    break;
            }
        }


        // ========================================================
        // ORDER CHAPTERS
        // ========================================================

        List<int> orderedChapters =
            new List<int>(
                chapters
            );


        orderedChapters.Sort();


        // ========================================================
        // ORDER EVENTS
        // ========================================================

        Dictionary<int, List<MapNodeData>> eventsByChapter =
            new Dictionary<int, List<MapNodeData>>();


        foreach (
            MapNodeData node
            in visibleNodes)
        {
            if (
                node == null ||
                node.Type != MapNodeType.Event)
            {
                continue;
            }


            int chapter =
                NormalizeChapter(
                    node.Chapter
                );


            if (
                !eventsByChapter.TryGetValue(
                    chapter,
                    out List<MapNodeData> chapterEvents))
            {
                chapterEvents =
                    new List<MapNodeData>();


                eventsByChapter[
                    chapter
                ] =
                    chapterEvents;
            }


            chapterEvents.Add(
                node
            );
        }


        foreach (
            List<MapNodeData> chapterEvents
            in eventsByChapter.Values)
        {
            chapterEvents.Sort(
                (left, right) =>
                {
                    int yearCompare =
                        left.Year.CompareTo(
                            right.Year
                        );


                    if (yearCompare != 0)
                    {
                        return yearCompare;
                    }


                    int titleCompare =
                        string.Compare(
                            left.Title,
                            right.Title,
                            StringComparison.OrdinalIgnoreCase
                        );


                    if (titleCompare != 0)
                    {
                        return titleCompare;
                    }


                    return string.Compare(
                        left.Id,
                        right.Id,
                        StringComparison.OrdinalIgnoreCase
                    );
                }
            );
        }


        // ========================================================
        // ORDER DECISIONS BY EVENT
        // ========================================================

        List<MapNodeData> orderedDecisions =
            OrderDecisionsByEvent(
                visibleNodes
            );


        // ========================================================
        // CALCULATE CHAPTER START Y
        // ========================================================

        Dictionary<int, float> chapterStartY =
            new Dictionary<int, float>();


        float currentChapterY =
            TopMargin;


        foreach (
            int chapter
            in orderedChapters)
        {
            chapterStartY[
                chapter
            ] =
                currentChapterY;


            // ----------------------------------------------------
            // EVENT LANE HEIGHT
            // ----------------------------------------------------

            float eventHeight =
                GetLaneHeight(
                    GetCounterValue(
                        eventCounts,
                        chapter
                    ),
                    EventSpacingY,
                    GetNodeHeight(
                        MapNodeType.Event
                    )
                );


            // ----------------------------------------------------
            // INTERLUDE LANE HEIGHT
            // ----------------------------------------------------

            float interludeHeight =
                GetLaneHeight(
                    GetCounterValue(
                        interludeCounts,
                        chapter
                    ),
                    InterludeSpacingY,
                    GetNodeHeight(
                        MapNodeType.Interlude
                    )
                );


            // ----------------------------------------------------
            // DECISION GROUP HEIGHT
            // ----------------------------------------------------

            float decisionHeight =
                GetDecisionGroupHeight(
                    chapter,
                    eventsByChapter,
                    orderedDecisions
                );


            // ----------------------------------------------------
            // PERSISTENT DECISION HEIGHT
            // ----------------------------------------------------

            float flagHeight =
                GetLaneHeight(
                    GetCounterValue(
                        flagCounts,
                        chapter
                    ),
                    FlagSpacingY,
                    GetNodeHeight(
                        MapNodeType.PersistentDecision
                    )
                );


            // ----------------------------------------------------
            // CHAPTER HEIGHT
            // ----------------------------------------------------

            float chapterHeight =
                Math.Max(
                    eventHeight,
                    Math.Max(
                        interludeHeight,
                        Math.Max(
                            decisionHeight,
                            flagHeight
                        )
                    )
                );


            currentChapterY +=
                chapterHeight +
                ChapterGap;
        }


        // ========================================================
        // ORDER ALL NODES
        // ========================================================

        List<MapNodeData> orderedNodes =
            new List<MapNodeData>(
                visibleNodes
            );


        orderedNodes.Sort(
            (left, right) =>
            {
                int chapterCompare =
                    NormalizeChapter(
                        left.Chapter
                    ).CompareTo(
                        NormalizeChapter(
                            right.Chapter
                        )
                    );


                if (chapterCompare != 0)
                {
                    return chapterCompare;
                }


                int typeCompare =
                    GetTypeOrder(
                        left.Type
                    ).CompareTo(
                        GetTypeOrder(
                            right.Type
                        )
                    );


                if (typeCompare != 0)
                {
                    return typeCompare;
                }


                if (
                    left.Type == MapNodeType.Decision &&
                    right.Type == MapNodeType.Decision
                )
                {
                    int leftOrder =
                        GetDecisionOrder(
                            left,
                            orderedDecisions
                        );


                    int rightOrder =
                        GetDecisionOrder(
                            right,
                            orderedDecisions
                        );


                    int decisionCompare =
                        leftOrder.CompareTo(
                            rightOrder
                        );


                    if (decisionCompare != 0)
                    {
                        return decisionCompare;
                    }
                }


                int yearCompare =
                    left.Year.CompareTo(
                        right.Year
                    );


                if (yearCompare != 0)
                {
                    return yearCompare;
                }


                int titleCompare =
                    string.Compare(
                        left.Title,
                        right.Title,
                        StringComparison.OrdinalIgnoreCase
                    );


                if (titleCompare != 0)
                {
                    return titleCompare;
                }


                return string.Compare(
                    left.Id,
                    right.Id,
                    StringComparison.OrdinalIgnoreCase
                );
            }
        );


        // ========================================================
        // POSITION ALL NODES
        // ========================================================

        Dictionary<int, int> eventRows =
            new Dictionary<int, int>();


        Dictionary<int, int> interludeRows =
            new Dictionary<int, int>();


        Dictionary<int, int> flagRows =
            new Dictionary<int, int>();


        Dictionary<string, float> decisionY =
            CalculateDecisionPositions(
                orderedChapters,
                eventsByChapter,
                orderedDecisions,
                chapterStartY
            );


        foreach (
            MapNodeData node
            in orderedNodes)
        {
            if (
                node == null)
            {
                continue;
            }


            if (
                !graphNodes.TryGetValue(
                    node.Key,
                    out GraphNode graphNode))
            {
                continue;
            }


            int chapter =
                NormalizeChapter(
                    node.Chapter
                );


            float baseY =
                chapterStartY.TryGetValue(
                    chapter,
                    out float chapterY)
                    ? chapterY
                    : TopMargin;


            float x =
                LeftMargin +
                GetTypeColumn(
                    node.Type
                ) *
                TypeSpacingX;


            float y =
                baseY;


            switch (node.Type)
            {
                case MapNodeType.Event:
                {
                    int row =
                        GetAndIncrementCounter(
                            eventRows,
                            chapter
                        );


                    y +=
                        row *
                        EventSpacingY;

                    break;
                }


                case MapNodeType.Interlude:
                {
                    int row =
                        GetAndIncrementCounter(
                            interludeRows,
                            chapter
                        );


                    y +=
                        row *
                        InterludeSpacingY;

                    break;
                }


                case MapNodeType.Decision:
                {
                    if (
                        decisionY.TryGetValue(
                            node.Id,
                            out float storedY
                        ))
                    {
                        y =
                            storedY;
                    }

                    break;
                }


                case MapNodeType.PersistentDecision:
                {
                    int row =
                        GetAndIncrementCounter(
                            flagRows,
                            chapter
                        );


                    y +=
                        row *
                        FlagSpacingY;

                    break;
                }
            }


            graphNode.PositionOffset =
                new Vector2(
                    x,
                    y
                );
        }
    }


    // ============================================================
    // DECISION GROUP HEIGHT
    // ============================================================

    private static float GetDecisionGroupHeight(
        int chapter,
        Dictionary<int, List<MapNodeData>> eventsByChapter,
        List<MapNodeData> orderedDecisions)
    {
        if (
            !eventsByChapter.TryGetValue(
                chapter,
                out List<MapNodeData> chapterEvents))
        {
            return 0.0f;
        }


        float totalHeight =
            0.0f;


        int groupsWithDecisions =
            0;


        foreach (
            MapNodeData eventNode
            in chapterEvents)
        {
            int decisionCount =
                GetDecisionCountForEvent(
                    eventNode,
                    orderedDecisions
                );


            if (decisionCount <= 0)
            {
                continue;
            }


            float groupHeight =
                GetLaneHeight(
                    decisionCount,
                    DecisionSpacingY,
                    GetNodeHeight(
                        MapNodeType.Decision
                    )
                );


            totalHeight +=
                groupHeight;


            groupsWithDecisions++;
        }


        if (groupsWithDecisions > 1)
        {
            totalHeight +=
                (groupsWithDecisions - 1) *
                EventGroupGap;
        }


        return totalHeight;
    }


    // ============================================================
    // DECISION POSITIONS
    // ============================================================

    private static Dictionary<string, float> CalculateDecisionPositions(
        List<int> orderedChapters,
        Dictionary<int, List<MapNodeData>> eventsByChapter,
        List<MapNodeData> orderedDecisions,
        Dictionary<int, float> chapterStartY)
    {
        Dictionary<string, float> result =
            new Dictionary<string, float>(
                StringComparer.OrdinalIgnoreCase
            );


        foreach (
            int chapter
            in orderedChapters)
        {
            if (
                !eventsByChapter.TryGetValue(
                    chapter,
                    out List<MapNodeData> chapterEvents))
            {
                continue;
            }


            float currentY =
                chapterStartY.TryGetValue(
                    chapter,
                    out float chapterY)
                    ? chapterY
                    : TopMargin;


            bool firstGroup =
                true;


            foreach (
                MapNodeData eventNode
                in chapterEvents)
            {
                List<MapNodeData> eventDecisions =
                    GetDecisionsForEvent(
                        eventNode,
                        orderedDecisions
                    );


                if (
                    eventDecisions.Count == 0)
                {
                    continue;
                }


                if (!firstGroup)
                {
                    currentY +=
                        EventGroupGap;
                }


                firstGroup =
                    false;


                foreach (
                    MapNodeData decision
                    in eventDecisions)
                {
                    if (
                        decision == null ||
                        string.IsNullOrWhiteSpace(
                            decision.Id
                        ))
                    {
                        continue;
                    }


                    result[
                        decision.Id
                    ] =
                        currentY;


                    currentY +=
                        DecisionSpacingY;
                }


                currentY -=
                    DecisionSpacingY -
                    GetNodeHeight(
                        MapNodeType.Decision
                    );
            }
        }


        return result;
    }


    // ============================================================
    // DECISIONS FOR EVENT
    // ============================================================

    private static List<MapNodeData> GetDecisionsForEvent(
        MapNodeData eventNode,
        List<MapNodeData> orderedDecisions)
    {
        List<MapNodeData> result =
            new List<MapNodeData>();


        if (
            eventNode == null ||
            string.IsNullOrWhiteSpace(
                eventNode.Id
            ))
        {
            return result;
        }


        foreach (
            MapNodeData decision
            in orderedDecisions)
        {
            if (
                decision == null)
            {
                continue;
            }


            if (
                string.Equals(
                    decision.SourceEventId,
                    eventNode.Id,
                    StringComparison.OrdinalIgnoreCase
                ))
            {
                result.Add(
                    decision
                );
            }
        }


        return result;
    }


    // ============================================================
    // DECISION COUNT
    // ============================================================

    private static int GetDecisionCountForEvent(
        MapNodeData eventNode,
        List<MapNodeData> orderedDecisions)
    {
        return GetDecisionsForEvent(
            eventNode,
            orderedDecisions
        ).Count;
    }


    // ============================================================
    // DECISION ORDER
    // ============================================================

    private static int GetDecisionOrder(
        MapNodeData decision,
        List<MapNodeData> orderedDecisions)
    {
        if (
            decision == null ||
            string.IsNullOrWhiteSpace(
                decision.Id
            ))
        {
            return int.MaxValue;
        }


        for (
            int i = 0;
            i < orderedDecisions.Count;
            i++)
        {
            if (
                string.Equals(
                    orderedDecisions[i].Id,
                    decision.Id,
                    StringComparison.OrdinalIgnoreCase
                ))
            {
                return i;
            }
        }


        return int.MaxValue;
    }


    // ============================================================
    // SORT
    // ============================================================

    public static int CompareNodes(
        MapNodeData left,
        MapNodeData right)
    {
        int chapterCompare =
            left.Chapter.CompareTo(
                right.Chapter
            );


        if (chapterCompare != 0)
        {
            return chapterCompare;
        }


        int yearCompare =
            left.Year.CompareTo(
                right.Year
            );


        if (yearCompare != 0)
        {
            return yearCompare;
        }


        int typeCompare =
            GetTypeOrder(
                left.Type
            ).CompareTo(
                GetTypeOrder(
                    right.Type
                )
            );


        if (typeCompare != 0)
        {
            return typeCompare;
        }


        return string.Compare(
            left.Title,
            right.Title,
            StringComparison.OrdinalIgnoreCase
        );
    }


    private static int GetTypeOrder(
        MapNodeType type)
    {
        return type switch
        {
            MapNodeType.Event => 0,
            MapNodeType.Interlude => 1,
            MapNodeType.Decision => 2,
            MapNodeType.PersistentDecision => 3,
            _ => 4
        };
    }


    private static int GetTypeColumn(
        MapNodeType type)
    {
        return type switch
        {
            MapNodeType.Event => 0,
            MapNodeType.Interlude => 1,
            MapNodeType.Decision => 2,
            MapNodeType.PersistentDecision => 3,
            _ => 0
        };
    }


    private static int NormalizeChapter(
        int chapter)
    {
        return chapter > 0
            ? chapter
            : 0;
    }


    // ============================================================
    // COUNTERS
    // ============================================================

    private static void IncrementCounter(
        Dictionary<int, int> counters,
        int chapter)
    {
        if (!counters.ContainsKey(
            chapter))
        {
            counters[
                chapter
            ] = 0;
        }


        counters[
            chapter
        ]++;
    }


    private static int GetAndIncrementCounter(
        Dictionary<int, int> counters,
        int chapter)
    {
        if (!counters.TryGetValue(
            chapter,
            out int current))
        {
            current = 0;
        }


        counters[
            chapter
        ] =
            current + 1;


        return current;
    }


    private static int GetCounterValue(
        Dictionary<int, int> counters,
        int chapter)
    {
        return counters.TryGetValue(
            chapter,
            out int value)
            ? value
            : 0;
    }


    private static float GetLaneHeight(
        int nodeCount,
        float spacing,
        float nodeHeight)
    {
        if (nodeCount <= 0)
        {
            return 0.0f;
        }


        return
            nodeHeight +
            Math.Max(
                0,
                nodeCount - 1
            ) *
            spacing;
    }


    private static float GetNodeHeight(
        MapNodeType type)
    {
        return EventMapNodeFactory.GetNodeHeight(
            type
        );
    }


    // ============================================================
    // ORDER DECISIONS BY EVENT
    // ============================================================

    private static List<MapNodeData> OrderDecisionsByEvent(
        List<MapNodeData> nodes
    )
    {
        List<MapNodeData> events =
            nodes.FindAll(
                node =>
                    node != null &&
                    node.Type == MapNodeType.Event
            );


        List<MapNodeData> decisions =
            nodes.FindAll(
                node =>
                    node != null &&
                    node.Type == MapNodeType.Decision
            );


        Dictionary<string, int> eventOrder =
            new Dictionary<string, int>(
                StringComparer.OrdinalIgnoreCase
            );


        int order = 0;


        foreach (
            MapNodeData eventNode
            in events)
        {
            if (
                eventNode == null ||
                string.IsNullOrWhiteSpace(
                    eventNode.Id
                ))
            {
                continue;
            }


            if (
                !eventOrder.ContainsKey(
                    eventNode.Id
                ))
            {
                eventOrder[
                    eventNode.Id
                ] =
                    order;


                order++;
            }
        }


        decisions.Sort(
            (left, right) =>
            {
                int leftOrder =
                    eventOrder.TryGetValue(
                        left.SourceEventId ?? "",
                        out int foundLeft
                    )
                        ? foundLeft
                        : int.MaxValue;


                int rightOrder =
                    eventOrder.TryGetValue(
                        right.SourceEventId ?? "",
                        out int foundRight
                    )
                        ? foundRight
                        : int.MaxValue;


                int result =
                    leftOrder.CompareTo(
                        rightOrder
                    );


                if (result != 0)
                {
                    return result;
                }


                return string.Compare(
                    left.Title,
                    right.Title,
                    StringComparison.OrdinalIgnoreCase
                );
            }
        );


        return decisions;
    }
}
