using System.Collections.Generic;

public class EffectValidator
{
    public void Validate(
        List<EditorEffectData> effects,
        string location,
        ValidationResourceType resourceType,
        string resourceId,
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
                    resourceType,
                    resourceId,
                    pageIndex,
                    pageId,
                    decisionIndex,
                    decisionId,
                    -1,
                    i
                );

                continue;
            }

            if (string.IsNullOrWhiteSpace(
                effect.TypeId))
            {
                result.AddError(
                    $"{effectLocation}: no tiene tipo.",
                    resourceType,
                    resourceId,
                    pageIndex,
                    pageId,
                    decisionIndex,
                    decisionId,
                    -1,
                    i
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


                case "decision":

                    ValidateRequiredField(
                        effect.DecisionId,
                        "DecisionId",
                        effectLocation,
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
                        effect.AttributeId,
                        "AttributeId",
                        effectLocation,
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
                        effect.RelationshipCharacterId,
                        "RelationshipCharacterId",
                        effectLocation,
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
                        effect.RelationshipCharacterId,
                        "RelationshipCharacterId",
                        effectLocation,
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
                        effect.RelationshipTitle,
                        "RelationshipTitle",
                        effectLocation,
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
                        $"{effectLocation}: tipo de efecto desconocido '{effect.TypeId}'.",
                        resourceType,
                        resourceId,
                        pageIndex,
                        pageId,
                        decisionIndex,
                        decisionId,
                        -1,
                        i
                    );

                    break;
            }
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
        int effectIndex,
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
                -1,
                effectIndex
            );
        }
    }
}