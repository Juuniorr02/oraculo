using System;
using System.Collections.Generic;

public class InterludeValidator
{
    private readonly ConditionValidator conditionValidator;
    private readonly EffectValidator effectValidator;


    public InterludeValidator()
    {
        conditionValidator =
            new ConditionValidator();

        effectValidator =
            new EffectValidator();
    }


    public ValidationResult Validate(
        EditorInterludeData interlude)
    {
        ValidationResult result =
            new ValidationResult();


        if (interlude == null)
        {
            result.AddError(
                "El interludio es nulo."
            );

            return result;
        }


        ValidateBasicData(
            interlude,
            result
        );


        ValidatePages(
            interlude,
            result
        );


        ValidateConditions(
            interlude,
            result
        );


        ValidateEffects(
            interlude,
            result
        );


        return result;
    }


    private void ValidateBasicData(
        EditorInterludeData interlude,
        ValidationResult result)
    {
        string interludeId =
            interlude.Id ?? "";


        if (string.IsNullOrWhiteSpace(
            interlude.Id))
        {
            result.AddError(
                "El interludio no tiene ID.",
                ValidationResourceType.Interlude,
                interludeId
            );
        }


        if (string.IsNullOrWhiteSpace(
            interlude.Title))
        {
            result.AddWarning(
                "El interludio no tiene título.",
                ValidationResourceType.Interlude,
                interludeId
            );
        }


        if (interlude.Chapter < 1)
        {
            result.AddError(
                $"El capítulo del interludio no es válido: {interlude.Chapter}.",
                ValidationResourceType.Interlude,
                interludeId
            );
        }
        else if (interlude.Chapter > 7)
        {
            result.AddError(
                $"El capítulo del interludio no es válido: {interlude.Chapter}.",
                ValidationResourceType.Interlude,
                interludeId
            );
        }


        if (!Enum.IsDefined(
            typeof(InterludeStyle),
            interlude.Style))
        {
            result.AddError(
                "El estilo del interludio no es válido.",
                ValidationResourceType.Interlude,
                interludeId
            );
        }


        if (interlude.Pages == null)
        {
            result.AddError(
                "El interludio no tiene una lista de páginas.",
                ValidationResourceType.Interlude,
                interludeId
            );
        }
        else if (interlude.Pages.Count == 0)
        {
            result.AddError(
                "El interludio no tiene ninguna página.",
                ValidationResourceType.Interlude,
                interludeId
            );
        }
    }


    private void ValidatePages(
        EditorInterludeData interlude,
        ValidationResult result)
    {
        if (interlude.Pages == null)
        {
            return;
        }


        string interludeId =
            interlude.Id ?? "";


        HashSet<string> pageIds =
            new HashSet<string>(
                StringComparer.Ordinal
            );


        for (
            int pageIndex = 0;
            pageIndex < interlude.Pages.Count;
            pageIndex++)
        {
            EditorInterludePageData page =
                interlude.Pages[pageIndex];


            string pageLocation =
                $"Página {pageIndex + 1}";


            if (page == null)
            {
                result.AddError(
                    $"{pageLocation}: la página es nula.",
                    ValidationResourceType.Interlude,
                    interludeId,
                    pageIndex
                );

                continue;
            }


            ValidatePageId(
                page,
                pageIndex,
                interludeId,
                pageIds,
                result
            );


            ValidatePageContent(
                page,
                pageIndex,
                interludeId,
                result
            );


            ValidateIllustrationPlacement(
                page,
                pageIndex,
                interludeId,
                result
            );
        }
    }


    private void ValidatePageId(
        EditorInterludePageData page,
        int pageIndex,
        string interludeId,
        HashSet<string> pageIds,
        ValidationResult result)
    {
        string pageId =
            page.Id ?? "";


        if (string.IsNullOrWhiteSpace(
            page.Id))
        {
            result.AddError(
                $"Página {pageIndex + 1}: la página no tiene ID.",
                ValidationResourceType.Interlude,
                interludeId,
                pageIndex,
                pageId
            );

            return;
        }


        if (!pageIds.Add(page.Id))
        {
            result.AddError(
                $"Página {pageIndex + 1}: el ID '{page.Id}' está duplicado.",
                ValidationResourceType.Interlude,
                interludeId,
                pageIndex,
                pageId
            );
        }
    }


    private void ValidatePageContent(
        EditorInterludePageData page,
        int pageIndex,
        string interludeId,
        ValidationResult result)
    {
        bool hasText =
            !string.IsNullOrWhiteSpace(
                page.Text
            );


        bool hasIllustration =
            !string.IsNullOrWhiteSpace(
                page.Illustration
            );


        if (!hasText && !hasIllustration)
        {
            result.AddWarning(
                $"Página {pageIndex + 1}: no tiene texto ni ilustración.",
                ValidationResourceType.Interlude,
                interludeId,
                pageIndex,
                page.Id ?? ""
            );
        }
    }


    private void ValidateIllustrationPlacement(
        EditorInterludePageData page,
        int pageIndex,
        string interludeId,
        ValidationResult result)
    {
        if (!Enum.IsDefined(
            typeof(EditorIllustrationPlacement),
            page.IllustrationPlacement))
        {
            result.AddError(
                $"Página {pageIndex + 1}: la posición de la ilustración no es válida.",
                ValidationResourceType.Interlude,
                interludeId,
                pageIndex,
                page.Id ?? ""
            );
        }
    }


    private void ValidateConditions(
        EditorInterludeData interlude,
        ValidationResult result)
    {
        if (interlude.Conditions == null)
        {
            return;
        }


        string interludeId =
            interlude.Id ?? "";


        conditionValidator.Validate(
            interlude.Conditions,
            "Condiciones del interludio",
            ValidationResourceType.Interlude,
            interludeId,
            -1,
            "",
            -1,
            "",
            result
        );
    }


    private void ValidateEffects(
        EditorInterludeData interlude,
        ValidationResult result)
    {
        if (interlude.Effects == null)
        {
            return;
        }


        string interludeId =
            interlude.Id ?? "";


        effectValidator.Validate(
            interlude.Effects,
            "Consecuencias del interludio",
            ValidationResourceType.Interlude,
            interludeId,
            -1,
            "",
            -1,
            "",
            result
        );
    }
}