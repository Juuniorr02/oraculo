using System;
using System.Collections.Generic;

public class EventValidator
{
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


        ValidatePages(
            eventData,
            result
        );


        return result;
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


            ValidateConditions(
                decision.Conditions,
                location,
                eventId,
                pageIndex,
                page.Id ?? "",
                decisionIndex,
                decisionId,
                result
            );


            ValidateEffects(
                decision.Effects,
                location,
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


    private void ValidateConditions(
        List<EditorConditionData> conditions,
        string location,
        string eventId,
        int pageIndex,
        string pageId,
        int decisionIndex,
        string decisionId,
        ValidationResult result)
    {
        if (conditions == null)
        {
            return;
        }


        for (
            int i = 0;
            i < conditions.Count;
            i++)
        {
            EditorConditionData condition =
                conditions[i];


            string conditionLocation =
                $"{location}, condición {i + 1}";


            if (condition == null)
            {
                result.AddError(
                    $"{conditionLocation}: la condición es nula.",
                    eventId,
                    pageIndex,
                    pageId,
                    decisionIndex,
                    decisionId
                );

                continue;
            }


            if (string.IsNullOrWhiteSpace(
                condition.TypeId))
            {
                result.AddError(
                    $"{conditionLocation}: no tiene tipo.",
                    eventId,
                    pageIndex,
                    pageId,
                    decisionIndex,
                    decisionId
                );

                continue;
            }


            if (
                condition.MinimumValue >
                condition.MaximumValue)
            {
                result.AddError(
                    $"{conditionLocation}: tiene un rango inválido.",
                    eventId,
                    pageIndex,
                    pageId,
                    decisionIndex,
                    decisionId
                );
            }


            switch (condition.TypeId)
            {
                case "characterattribute":

                    ValidateRequiredField(
                        condition.AttributeId,
                        "AttributeId",
                        conditionLocation,
                        eventId,
                        pageIndex,
                        pageId,
                        decisionIndex,
                        decisionId,
                        result
                    );

                    break;


                case "prestige":

                    break;


                case "year":

                    break;


                case "decision":

                    ValidateRequiredField(
                        condition.DecisionId,
                        "DecisionId",
                        conditionLocation,
                        eventId,
                        pageIndex,
                        pageId,
                        decisionIndex,
                        decisionId,
                        result
                    );

                    break;


                case "empireattribute":

                    ValidateRequiredField(
                        condition.AttributeId,
                        "AttributeId",
                        conditionLocation,
                        eventId,
                        pageIndex,
                        pageId,
                        decisionIndex,
                        decisionId,
                        result
                    );

                    break;


                case "relationship":

                    ValidateRequiredField(
                        condition.RelationshipCharacterId,
                        "RelationshipCharacterId",
                        conditionLocation,
                        eventId,
                        pageIndex,
                        pageId,
                        decisionIndex,
                        decisionId,
                        result
                    );

                    break;


                case "relationshiptitle":

                    ValidateRequiredField(
                        condition.RelationshipCharacterId,
                        "RelationshipCharacterId",
                        conditionLocation,
                        eventId,
                        pageIndex,
                        pageId,
                        decisionIndex,
                        decisionId,
                        result
                    );


                    ValidateRequiredField(
                        condition.RelationshipTitle,
                        "RelationshipTitle",
                        conditionLocation,
                        eventId,
                        pageIndex,
                        pageId,
                        decisionIndex,
                        decisionId,
                        result
                    );

                    break;


                default:

                    result.AddError(
                        $"{conditionLocation}: tipo de condición desconocido '{condition.TypeId}'.",
                        eventId,
                        pageIndex,
                        pageId,
                        decisionIndex,
                        decisionId
                    );

                    break;
            }
        }
    }


    private void ValidateEffects(
        List<EditorEffectData> effects,
        string location,
        string eventId,
        int pageIndex,
        string pageId,
        int decisionIndex,
        string decisionId,
        ValidationResult result)
    {
        if (effects == null)
        {
            return;
        }


        for (
            int i = 0;
            i < effects.Count;
            i++)
        {
            EditorEffectData effect =
                effects[i];


            string effectLocation =
                $"{location}, efecto {i + 1}";


            if (effect == null)
            {
                result.AddError(
                    $"{effectLocation}: el efecto es nulo.",
                    eventId,
                    pageIndex,
                    pageId,
                    decisionIndex,
                    decisionId
                );

                continue;
            }


            if (string.IsNullOrWhiteSpace(
                effect.TypeId))
            {
                result.AddError(
                    $"{effectLocation}: no tiene tipo.",
                    eventId,
                    pageIndex,
                    pageId,
                    decisionIndex,
                    decisionId
                );

                continue;
            }


            switch (effect.TypeId)
            {
                case "characterattribute":

                    ValidateRequiredField(
                        effect.AttributeId,
                        "AttributeId",
                        effectLocation,
                        eventId,
                        pageIndex,
                        pageId,
                        decisionIndex,
                        decisionId,
                        result
                    );

                    break;


                case "prestige":

                    break;


                case "decision":

                    ValidateRequiredField(
                        effect.DecisionId,
                        "DecisionId",
                        effectLocation,
                        eventId,
                        pageIndex,
                        pageId,
                        decisionIndex,
                        decisionId,
                        result
                    );

                    break;


                case "empireattribute":

                    ValidateRequiredField(
                        effect.AttributeId,
                        "AttributeId",
                        effectLocation,
                        eventId,
                        pageIndex,
                        pageId,
                        decisionIndex,
                        decisionId,
                        result
                    );

                    break;


                case "relationship":

                    ValidateRequiredField(
                        effect.RelationshipCharacterId,
                        "RelationshipCharacterId",
                        effectLocation,
                        eventId,
                        pageIndex,
                        pageId,
                        decisionIndex,
                        decisionId,
                        result
                    );

                    break;


                case "relationshiptitle":

                    ValidateRequiredField(
                        effect.RelationshipCharacterId,
                        "RelationshipCharacterId",
                        effectLocation,
                        eventId,
                        pageIndex,
                        pageId,
                        decisionIndex,
                        decisionId,
                        result
                    );


                    ValidateRequiredField(
                        effect.RelationshipTitle,
                        "RelationshipTitle",
                        effectLocation,
                        eventId,
                        pageIndex,
                        pageId,
                        decisionIndex,
                        decisionId,
                        result
                    );

                    break;


                default:

                    result.AddError(
                        $"{effectLocation}: tipo de efecto desconocido '{effect.TypeId}'.",
                        eventId,
                        pageIndex,
                        pageId,
                        decisionIndex,
                        decisionId
                    );

                    break;
            }
        }
    }


    private void ValidateRequiredField(
        string value,
        string fieldName,
        string location,
        string eventId,
        int pageIndex,
        string pageId,
        int decisionIndex,
        string decisionId,
        ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result.AddError(
                $"{location}: el campo '{fieldName}' es obligatorio.",
                eventId,
                pageIndex,
                pageId,
                decisionIndex,
                decisionId
            );
        }
    }
}