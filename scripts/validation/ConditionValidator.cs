using System.Collections.Generic;

public class ConditionValidator
{
    private readonly DecisionDatabase decisionDatabase;


    public ConditionValidator()
    {
        decisionDatabase =
            null;
    }


    public ConditionValidator(
        DecisionDatabase decisionDatabase)
    {
        this.decisionDatabase =
            decisionDatabase;
    }


    public void Validate(
        List<EditorConditionData> conditions,
        string location,
        ValidationResourceType resourceType,
        string resourceId,
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
                    resourceType,
                    resourceId,
                    pageIndex,
                    pageId,
                    decisionIndex,
                    decisionId,
                    i,
                    -1
                );

                continue;
            }

            if (string.IsNullOrWhiteSpace(
                condition.TypeId))
            {
                result.AddError(
                    $"{conditionLocation}: no tiene tipo.",
                    resourceType,
                    resourceId,
                    pageIndex,
                    pageId,
                    decisionIndex,
                    decisionId,
                    i,
                    -1
                );

                continue;
            }

            if (
                condition.MinimumValue >
                condition.MaximumValue)
            {
                result.AddError(
                    $"{conditionLocation}: tiene un rango inválido.",
                    resourceType,
                    resourceId,
                    pageIndex,
                    pageId,
                    decisionIndex,
                    decisionId,
                    i,
                    -1
                );
            }

            switch (condition.TypeId)
            {
                case "characterattribute":

                    ValidateRequiredField(
                        condition.AttributeId,
                        "AttributeId",
                        conditionLocation,
                        resourceType,
                        resourceId,
                        pageIndex,
                        pageId,
                        decisionIndex,
                        decisionId,
                        i,
                        result
                    );

                    break;


                case "prestige":
                    break;


                case "year":
                    break;


                case "decision":

                    ValidateDecisionReference(
                        condition.DecisionId,
                        conditionLocation,
                        resourceType,
                        resourceId,
                        pageIndex,
                        pageId,
                        decisionIndex,
                        decisionId,
                        i,
                        result
                    );

                    break;


                case "empireattribute":

                    ValidateRequiredField(
                        condition.AttributeId,
                        "AttributeId",
                        conditionLocation,
                        resourceType,
                        resourceId,
                        pageIndex,
                        pageId,
                        decisionIndex,
                        decisionId,
                        i,
                        result
                    );

                    break;


                case "relationship":

                    ValidateRequiredField(
                        condition.RelationshipCharacterId,
                        "RelationshipCharacterId",
                        conditionLocation,
                        resourceType,
                        resourceId,
                        pageIndex,
                        pageId,
                        decisionIndex,
                        decisionId,
                        i,
                        result
                    );

                    break;


                case "relationshiptitle":

                    ValidateRequiredField(
                        condition.RelationshipCharacterId,
                        "RelationshipCharacterId",
                        conditionLocation,
                        resourceType,
                        resourceId,
                        pageIndex,
                        pageId,
                        decisionIndex,
                        decisionId,
                        i,
                        result
                    );

                    ValidateRequiredField(
                        condition.RelationshipTitle,
                        "RelationshipTitle",
                        conditionLocation,
                        resourceType,
                        resourceId,
                        pageIndex,
                        pageId,
                        decisionIndex,
                        decisionId,
                        i,
                        result
                    );

                    break;


                default:

                    result.AddError(
                        $"{conditionLocation}: tipo de condición desconocido '{condition.TypeId}'.",
                        resourceType,
                        resourceId,
                        pageIndex,
                        pageId,
                        decisionIndex,
                        decisionId,
                        i,
                        -1
                    );

                    break;
            }
        }
    }


    private void ValidateDecisionReference(
        string value,
        string location,
        ValidationResourceType resourceType,
        string resourceId,
        int pageIndex,
        string pageId,
        int decisionIndex,
        string decisionId,
        int conditionIndex,
        ValidationResult result)
    {
        ValidateRequiredField(
            value,
            "DecisionId",
            location,
            resourceType,
            resourceId,
            pageIndex,
            pageId,
            decisionIndex,
            decisionId,
            conditionIndex,
            result
        );


        if (string.IsNullOrWhiteSpace(
            value))
        {
            return;
        }


        if (decisionDatabase == null)
        {
            return;
        }


        if (decisionDatabase.GetDecision(
            value.Trim()) == null)
        {
            result.AddError(
                $"{location}: la decisión persistente '{value.Trim()}' no existe.",
                resourceType,
                resourceId,
                pageIndex,
                pageId,
                decisionIndex,
                decisionId,
                conditionIndex,
                -1
            );
        }
    }


    private void ValidateRequiredField(
        string value,
        string fieldName,
        string location,
        ValidationResourceType resourceType,
        string resourceId,
        int pageIndex,
        string pageId,
        int decisionIndex,
        string decisionId,
        int conditionIndex,
        ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result.AddError(
                $"{location}: el campo '{fieldName}' es obligatorio.",
                resourceType,
                resourceId,
                pageIndex,
                pageId,
                decisionIndex,
                decisionId,
                conditionIndex,
                -1
            );
        }
    }
}