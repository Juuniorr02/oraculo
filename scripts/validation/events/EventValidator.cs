using System;
using System.Collections.Generic;

public class EventValidator
{
    private readonly ConditionValidator conditionValidator;
    private readonly EffectValidator effectValidator;

    private readonly EventRepository eventRepository;
    private readonly ChapterRepository chapterRepository;
    private readonly AttributeRepository attributeRepository;


    public EventValidator()
    {
        eventRepository =
            null;

        chapterRepository =
            null;

        attributeRepository =
            null;


        conditionValidator =
            new ConditionValidator();

        effectValidator =
            new EffectValidator();
    }


    public EventValidator(
        ChapterRepository chapterRepository,
        AttributeRepository attributeRepository)
    {
        eventRepository =
            null;

        this.chapterRepository =
            chapterRepository;

        this.attributeRepository =
            attributeRepository;


        conditionValidator =
            new ConditionValidator();

        effectValidator =
            new EffectValidator();
    }


    public EventValidator(
        EventRepository eventRepository,
        ChapterRepository chapterRepository,
        AttributeRepository attributeRepository)
    {
        this.eventRepository =
            eventRepository;

        this.chapterRepository =
            chapterRepository;

        this.attributeRepository =
            attributeRepository;


        DecisionDatabase decisionDatabase =
            new DecisionDatabase(
                eventRepository
            );


        conditionValidator =
            new ConditionValidator(
                decisionDatabase
            );

        effectValidator =
            new EffectValidator();
    }


    public ValidationResult Validate(
        EditorEventData eventData)
    {
        ValidationResult result =
            new ValidationResult();


        if (eventData == null)
        {
            result.AddError(
                "El evento es nulo."
            );

            return result;
        }


        ValidateBasicData(
            eventData,
            result
        );


        ValidateEventConditions(
            eventData,
            result
        );


        ValidatePages(
            eventData,
            result
        );


        ValidateCharacterAttributes(
            eventData,
            result
        );


        return result;
    }


    private void ValidateEventConditions(
        EditorEventData eventData,
        ValidationResult result)
    {
        if (eventData.Conditions == null)
        {
            return;
        }


        conditionValidator.Validate(
            eventData.Conditions,
            "Condiciones del evento",
            ValidationResourceType.Event,
            eventData.Id ?? "",
            -1,
            "",
            -1,
            "",
            result
        );
    }


    private void ValidateBasicData(
        EditorEventData eventData,
        ValidationResult result)
    {
        string eventId =
            eventData.Id ?? "";


        if (string.IsNullOrWhiteSpace(eventData.Id))
        {
            result.AddError(
                "El evento no tiene ID.",
                eventId
            );
        }


        if (string.IsNullOrWhiteSpace(eventData.Title))
        {
            result.AddWarning(
                "El evento no tiene título.",
                eventId
            );
        }


        if (eventData.Chapter < 1)
        {
            result.AddError(
                $"El capítulo del evento no es válido: {eventData.Chapter}.",
                eventId
            );
        }
        else
        {
            ValidateChapterExists(
                eventData,
                result
            );
        }


        if (eventData.Year < 1)
        {
            result.AddError(
                $"El año del evento no es válido: {eventData.Year}.",
                eventId
            );
        }


        if (eventData.WorldYear < 1)
        {
            result.AddError(
                $"El año mundial del evento no es válido: {eventData.WorldYear}.",
                eventId
            );
        }


        if (eventData.Pages == null)
        {
            result.AddError(
                "El evento no tiene una lista de páginas.",
                eventId
            );
        }
        else if (eventData.Pages.Count == 0)
        {
            result.AddError(
                "El evento no tiene ninguna página.",
                eventId
            );
        }
    }


    private void ValidateChapterExists(
        EditorEventData eventData,
        ValidationResult result)
    {
        if (chapterRepository == null)
        {
            return;
        }


        ChapterDefinitionData chapter =
            GetChapterByNumber(
                eventData.Chapter
            );


        if (chapter == null)
        {
            result.AddError(
                $"El evento referencia el capítulo {eventData.Chapter}, pero ese capítulo no existe.",
                eventData.Id ?? ""
            );
        }
    }


    private void ValidatePages(
        EditorEventData eventData,
        ValidationResult result)
    {
        if (eventData.Pages == null)
        {
            return;
        }


        string eventId =
            eventData.Id ?? "";


        HashSet<string> pageIds =
            new HashSet<string>(
                StringComparer.Ordinal
            );


        for (
            int pageIndex = 0;
            pageIndex < eventData.Pages.Count;
            pageIndex++)
        {
            EditorPageData page =
                eventData.Pages[pageIndex];


            string pageLocation =
                $"Página {pageIndex + 1}";


            if (page == null)
            {
                result.AddError(
                    $"{pageLocation}: la página es nula.",
                    eventId,
                    pageIndex
                );

                continue;
            }


            ValidatePageId(
                page,
                pageIndex,
                eventId,
                pageIds,
                result
            );


            ValidatePageText(
                page,
                pageIndex,
                eventId,
                result
            );


            ValidatePageType(
                page,
                pageIndex,
                eventId,
                result
            );
        }


        ValidateDecisionTargets(
            eventData.Pages,
            eventId,
            pageIds,
            result
        );
    }


    private void ValidatePageId(
        EditorPageData page,
        int pageIndex,
        string eventId,
        HashSet<string> pageIds,
        ValidationResult result)
    {
        string pageId =
            page.Id ?? "";


        if (string.IsNullOrWhiteSpace(page.Id))
        {
            result.AddError(
                $"Página {pageIndex + 1}: la página no tiene ID.",
                eventId,
                pageIndex,
                pageId
            );

            return;
        }


        if (!pageIds.Add(page.Id))
        {
            result.AddError(
                $"Página {pageIndex + 1}: el ID '{page.Id}' está duplicado.",
                eventId,
                pageIndex,
                pageId
            );
        }
    }


    private void ValidatePageText(
        EditorPageData page,
        int pageIndex,
        string eventId,
        ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(page.Text))
        {
            result.AddWarning(
                $"Página {pageIndex + 1}: la página no tiene texto.",
                eventId,
                pageIndex,
                page.Id ?? ""
            );
        }
    }


    private void ValidatePageType(
        EditorPageData page,
        int pageIndex,
        string eventId,
        ValidationResult result)
    {
        switch (page.Type)
        {
            case EditorPageType.Normal:

                if (
                    page.Decisions != null &&
                    page.Decisions.Count > 0)
                {
                    result.AddError(
                        $"Página {pageIndex + 1}: una página Normal no puede contener decisiones.",
                        eventId,
                        pageIndex,
                        page.Id ?? ""
                    );
                }

                break;


            case EditorPageType.Decision:

                ValidateDecisions(
                    page,
                    pageIndex,
                    eventId,
                    result
                );

                break;


            default:

                result.AddError(
                    $"Página {pageIndex + 1}: tipo de página desconocido.",
                    eventId,
                    pageIndex,
                    page.Id ?? ""
                );

                break;
        }
    }


    private void ValidateDecisions(
        EditorPageData page,
        int pageIndex,
        string eventId,
        ValidationResult result)
    {
        if (
            page.Decisions == null ||
            page.Decisions.Count == 0)
        {
            result.AddWarning(
                $"Página {pageIndex + 1}: es una página de decisión pero no tiene opciones.",
                eventId,
                pageIndex,
                page.Id ?? ""
            );

            return;
        }


        for (
            int decisionIndex = 0;
            decisionIndex < page.Decisions.Count;
            decisionIndex++)
        {
            EditorDecisionData decision =
                page.Decisions[decisionIndex];


            string location =
                $"Página {pageIndex + 1}, opción {decisionIndex + 1}";


            if (decision == null)
            {
                result.AddError(
                    $"{location}: la decisión es nula.",
                    eventId,
                    pageIndex,
                    page.Id ?? "",
                    decisionIndex
                );

                continue;
            }


            string decisionId =
                decision.Id ?? "";


            if (string.IsNullOrWhiteSpace(decision.Id))
            {
                result.AddError(
                    $"{location}: la decisión no tiene ID.",
                    eventId,
                    pageIndex,
                    page.Id ?? "",
                    decisionIndex,
                    decisionId
                );
            }


            if (string.IsNullOrWhiteSpace(decision.Text))
            {
                result.AddError(
                    $"{location}: la opción no tiene texto.",
                    eventId,
                    pageIndex,
                    page.Id ?? "",
                    decisionIndex,
                    decisionId
                );
            }


            if (string.IsNullOrWhiteSpace(decision.NextPageId))
            {
                result.AddWarning(
                    $"{location}: no tiene próxima página. La decisión terminará el evento.",
                    eventId,
                    pageIndex,
                    page.Id ?? "",
                    decisionIndex,
                    decisionId
                );
            }


            conditionValidator.Validate(
                decision.Conditions,
                location,
                ValidationResourceType.Event,
                eventId,
                pageIndex,
                page.Id ?? "",
                decisionIndex,
                decisionId,
                result
            );


            effectValidator.Validate(
                decision.Effects,
                location,
                ValidationResourceType.Event,
                eventId,
                pageIndex,
                page.Id ?? "",
                decisionIndex,
                decisionId,
                result
            );
        }
    }


    private void ValidateDecisionTargets(
        List<EditorPageData> pages,
        string eventId,
        HashSet<string> pageIds,
        ValidationResult result)
    {
        if (pages == null)
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


            if (
                page == null ||
                page.Decisions == null)
            {
                continue;
            }


            for (
                int decisionIndex = 0;
                decisionIndex < page.Decisions.Count;
                decisionIndex++)
            {
                EditorDecisionData decision =
                    page.Decisions[decisionIndex];


                if (
                    decision == null ||
                    string.IsNullOrWhiteSpace(
                        decision.NextPageId))
                {
                    continue;
                }


                if (
                    !pageIds.Contains(
                        decision.NextPageId))
                {
                    result.AddError(
                        $"Página {pageIndex + 1}, opción {decisionIndex + 1}: la próxima página '{decision.NextPageId}' no existe.",
                        eventId,
                        pageIndex,
                        page.Id ?? "",
                        decisionIndex,
                        decision.Id ?? ""
                    );
                }
            }
        }
    }


    private void ValidateCharacterAttributes(
        EditorEventData eventData,
        ValidationResult result)
    {
        if (
            chapterRepository == null ||
            attributeRepository == null)
        {
            return;
        }


        ChapterDefinitionData chapter =
            GetChapterByNumber(
                eventData.Chapter
            );


        if (chapter == null)
        {
            return;
        }


        HashSet<string> chapterAttributeIds =
            new HashSet<string>(
                chapter.AttributeIds ??
                new List<string>(),
                StringComparer.OrdinalIgnoreCase
            );


        if (eventData.Pages == null)
        {
            return;
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


            for (
                int decisionIndex = 0;
                decisionIndex < page.Decisions.Count;
                decisionIndex++)
            {
                EditorDecisionData decision =
                    page.Decisions[decisionIndex];


                if (decision == null)
                {
                    continue;
                }


                ValidateConditionAttributes(
                    eventData,
                    page,
                    pageIndex,
                    decision,
                    decisionIndex,
                    chapterAttributeIds,
                    result
                );


                ValidateEffectAttributes(
                    eventData,
                    page,
                    pageIndex,
                    decision,
                    decisionIndex,
                    chapterAttributeIds,
                    result
                );
            }
        }
    }


    private void ValidateConditionAttributes(
        EditorEventData eventData,
        EditorPageData page,
        int pageIndex,
        EditorDecisionData decision,
        int decisionIndex,
        HashSet<string> chapterAttributeIds,
        ValidationResult result)
    {
        if (decision.Conditions == null)
        {
            return;
        }


        for (
            int conditionIndex = 0;
            conditionIndex < decision.Conditions.Count;
            conditionIndex++)
        {
            EditorConditionData condition =
                decision.Conditions[conditionIndex];


            if (condition == null)
            {
                continue;
            }


            if (!string.Equals(
                condition.TypeId,
                "characterattribute",
                StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }


            ValidateAttributeReference(
                condition.AttributeId,
                "condición",
                eventData,
                page,
                pageIndex,
                decision,
                decisionIndex,
                conditionIndex,
                -1,
                chapterAttributeIds,
                result
            );
        }
    }


    private void ValidateEffectAttributes(
        EditorEventData eventData,
        EditorPageData page,
        int pageIndex,
        EditorDecisionData decision,
        int decisionIndex,
        HashSet<string> chapterAttributeIds,
        ValidationResult result)
    {
        if (decision.Effects == null)
        {
            return;
        }


        for (
            int effectIndex = 0;
            effectIndex < decision.Effects.Count;
            effectIndex++)
        {
            EditorEffectData effect =
                decision.Effects[effectIndex];


            if (effect == null)
            {
                continue;
            }


            if (!string.Equals(
                effect.TypeId,
                "characterattribute",
                StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }


            ValidateAttributeReference(
                effect.AttributeId,
                "efecto",
                eventData,
                page,
                pageIndex,
                decision,
                decisionIndex,
                -1,
                effectIndex,
                chapterAttributeIds,
                result
            );
        }
    }


    private void ValidateAttributeReference(
        string attributeId,
        string sourceType,
        EditorEventData eventData,
        EditorPageData page,
        int pageIndex,
        EditorDecisionData decision,
        int decisionIndex,
        int conditionIndex,
        int effectIndex,
        HashSet<string> chapterAttributeIds,
        ValidationResult result)
    {
        string eventId =
            eventData.Id ?? "";


        string decisionId =
            decision?.Id ?? "";


        string safeAttributeId =
            attributeId ?? "";


        if (string.IsNullOrWhiteSpace(
            safeAttributeId))
        {
            string location;


            if (conditionIndex >= 0)
            {
                location =
                    $"Página {pageIndex + 1}, opción {decisionIndex + 1}, condición {conditionIndex + 1}";
            }
            else
            {
                location =
                    $"Página {pageIndex + 1}, opción {decisionIndex + 1}, efecto {effectIndex + 1}";
            }


            result.AddError(
                $"{location}: el {sourceType} de atributo de personaje no tiene un atributo seleccionado.",
                eventId,
                pageIndex,
                page?.Id ?? "",
                decisionIndex,
                decisionId,
                conditionIndex,
                effectIndex
            );

            return;
        }


        if (!attributeRepository.Exists(
            safeAttributeId))
        {
            string location;


            if (conditionIndex >= 0)
            {
                location =
                    $"Página {pageIndex + 1}, opción {decisionIndex + 1}, condición {conditionIndex + 1}";
            }
            else
            {
                location =
                    $"Página {pageIndex + 1}, opción {decisionIndex + 1}, efecto {effectIndex + 1}";
            }


            result.AddError(
                $"{location}: el atributo '{safeAttributeId}' no existe.",
                eventId,
                pageIndex,
                page?.Id ?? "",
                decisionIndex,
                decisionId,
                conditionIndex,
                effectIndex
            );

            return;
        }


        if (chapterAttributeIds.Contains(
            safeAttributeId))
        {
            return;
        }


        AttributeDefinitionData attribute =
            attributeRepository.Load(
                safeAttributeId
            );


        string attributeName =
            attribute != null &&
            !string.IsNullOrWhiteSpace(
                attribute.DisplayName)
                ? attribute.DisplayName
                : safeAttributeId;


        string usageLocation;


        if (conditionIndex >= 0)
        {
            usageLocation =
                $"Página {pageIndex + 1}, opción {decisionIndex + 1}, condición {conditionIndex + 1}";
        }
        else
        {
            usageLocation =
                $"Página {pageIndex + 1}, opción {decisionIndex + 1}, efecto {effectIndex + 1}";
        }


        result.AddError(
            $"{usageLocation}: el atributo '{attributeName}' no pertenece al capítulo {eventData.Chapter}.",
            eventId,
            pageIndex,
            page?.Id ?? "",
            decisionIndex,
            decisionId,
            conditionIndex,
            effectIndex
        );
    }


    private ChapterDefinitionData GetChapterByNumber(
        int chapterNumber)
    {
        if (chapterRepository == null)
        {
            return null;
        }


        List<ChapterDefinitionData> chapters =
            chapterRepository.LoadAll();


        foreach (
            ChapterDefinitionData chapter
            in chapters)
        {
            if (
                chapter != null &&
                chapter.Number == chapterNumber)
            {
                return chapter;
            }
        }


        return null;
    }
}