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
        if (string.IsNullOrWhiteSpace(eventData.Id))
        {
            result.AddError(
                "El evento no tiene ID."
            );
        }


        if (string.IsNullOrWhiteSpace(eventData.Title))
        {
            result.AddWarning(
                "El evento no tiene título."
            );
        }


        if (eventData.Chapter < 1)
        {
            result.AddError(
                $"El capítulo del evento no es válido: {eventData.Chapter}."
            );
        }


        if (eventData.Year < 1)
        {
            result.AddError(
                $"El año del evento no es válido: {eventData.Year}."
            );
        }


        if (eventData.WorldYear < 1)
        {
            result.AddError(
                $"El año mundial del evento no es válido: {eventData.WorldYear}."
            );
        }


        if (eventData.Pages == null)
        {
            result.AddError(
                "El evento no tiene una lista de páginas."
            );
        }
        else if (eventData.Pages.Count == 0)
        {
            result.AddError(
                "El evento no tiene ninguna página."
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
                    $"{pageLocation}: la página es nula."
                );

                continue;
            }


            ValidatePageId(
                page,
                pageIndex,
                pageIds,
                result
            );


            ValidatePageText(
                page,
                pageIndex,
                result
            );


            ValidatePageType(
                page,
                pageIndex,
                result
            );
        }


        ValidateDecisionTargets(
            eventData.Pages,
            pageIds,
            result
        );
    }


    private void ValidatePageId(
        EditorPageData page,
        int pageIndex,
        HashSet<string> pageIds,
        ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(page.Id))
        {
            result.AddError(
                $"Página {pageIndex + 1}: la página no tiene ID."
            );

            return;
        }


        if (!pageIds.Add(page.Id))
        {
            result.AddError(
                $"Página {pageIndex + 1}: el ID '{page.Id}' está duplicado."
            );
        }
    }


    private void ValidatePageText(
        EditorPageData page,
        int pageIndex,
        ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(page.Text))
        {
            result.AddWarning(
                $"Página {pageIndex + 1}: la página no tiene texto."
            );
        }
    }


    private void ValidatePageType(
        EditorPageData page,
        int pageIndex,
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
                        $"Página {pageIndex + 1}: una página Normal no puede contener decisiones."
                    );
                }

                break;


            case EditorPageType.Decision:

                ValidateDecisions(
                    page,
                    pageIndex,
                    result
                );

                break;


            default:

                result.AddError(
                    $"Página {pageIndex + 1}: tipo de página desconocido."
                );

                break;
        }
    }


    private void ValidateDecisions(
        EditorPageData page,
        int pageIndex,
        ValidationResult result)
    {
        if (
            page.Decisions == null ||
            page.Decisions.Count == 0)
        {
            result.AddWarning(
                $"Página {pageIndex + 1}: es una página de decisión pero no tiene opciones."
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
                    $"{location}: la decisión es nula."
                );

                continue;
            }


            if (string.IsNullOrWhiteSpace(decision.Id))
            {
                result.AddError(
                    $"{location}: la decisión no tiene ID."
                );
            }


            if (string.IsNullOrWhiteSpace(decision.Text))
            {
                result.AddError(
                    $"{location}: la opción no tiene texto."
                );
            }


            if (string.IsNullOrWhiteSpace(decision.NextPageId))
            {
                result.AddWarning(
                    $"{location}: no tiene próxima página. La decisión terminará el evento."
                );
            }


            ValidateConditions(
                decision.Conditions,
                location,
                result
            );


            ValidateEffects(
                decision.Effects,
                location,
                result
            );
        }
    }


    private void ValidateDecisionTargets(
        List<EditorPageData> pages,
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
                        $"Página {pageIndex + 1}, opción {decisionIndex + 1}: la próxima página '{decision.NextPageId}' no existe."
                    );
                }
            }
        }
    }


    private void ValidateConditions(
        List<EditorConditionData> conditions,
        string location,
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
                    $"{conditionLocation}: la condición es nula."
                );

                continue;
            }


            if (string.IsNullOrWhiteSpace(
                condition.TypeId))
            {
                result.AddError(
                    $"{conditionLocation}: no tiene tipo."
                );

                continue;
            }


            if (
                condition.MinimumValue >
                condition.MaximumValue)
            {
                result.AddError(
                    $"{conditionLocation}: tiene un rango inválido."
                );
            }


            switch (condition.TypeId)
            {
                case "characterattribute":

                    ValidateRequiredField(
                        condition.AttributeId,
                        "AttributeId",
                        conditionLocation,
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
                        result
                    );

                    break;


                case "empireattribute":

                    ValidateRequiredField(
                        condition.AttributeId,
                        "AttributeId",
                        conditionLocation,
                        result
                    );

                    break;


                case "relationship":

                    ValidateRequiredField(
                        condition.RelationshipCharacterId,
                        "RelationshipCharacterId",
                        conditionLocation,
                        result
                    );

                    break;


                case "relationshiptitle":

                    ValidateRequiredField(
                        condition.RelationshipCharacterId,
                        "RelationshipCharacterId",
                        conditionLocation,
                        result
                    );

                    ValidateRequiredField(
                        condition.RelationshipTitle,
                        "RelationshipTitle",
                        conditionLocation,
                        result
                    );

                    break;


                default:

                    result.AddError(
                        $"{conditionLocation}: tipo de condición desconocido '{condition.TypeId}'."
                    );

                    break;
            }
        }
    }


    private void ValidateEffects(
        List<EditorEffectData> effects,
        string location,
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
                    $"{effectLocation}: el efecto es nulo."
                );

                continue;
            }


            if (string.IsNullOrWhiteSpace(
                effect.TypeId))
            {
                result.AddError(
                    $"{effectLocation}: no tiene tipo."
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
                        result
                    );

                    break;


                case "empireattribute":

                    ValidateRequiredField(
                        effect.AttributeId,
                        "AttributeId",
                        effectLocation,
                        result
                    );

                    break;


                case "relationship":

                    ValidateRequiredField(
                        effect.RelationshipCharacterId,
                        "RelationshipCharacterId",
                        effectLocation,
                        result
                    );

                    break;


                case "relationshiptitle":

                    ValidateRequiredField(
                        effect.RelationshipCharacterId,
                        "RelationshipCharacterId",
                        effectLocation,
                        result
                    );

                    ValidateRequiredField(
                        effect.RelationshipTitle,
                        "RelationshipTitle",
                        effectLocation,
                        result
                    );

                    break;


                default:

                    result.AddError(
                        $"{effectLocation}: tipo de efecto desconocido '{effect.TypeId}'."
                    );

                    break;
            }
        }
    }


    private void ValidateRequiredField(
        string value,
        string fieldName,
        string location,
        ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result.AddError(
                $"{location}: el campo '{fieldName}' es obligatorio."
            );
        }
    }
}