using System;
using System.Collections.Generic;

public class ProjectValidator
{
    private readonly EventRepository eventRepository;
    private readonly InterludeRepository interludeRepository;

    private readonly ChapterRepository chapterRepository;
    private readonly AttributeRepository attributeRepository;

    private readonly EventValidator eventValidator;
    private readonly InterludeValidator interludeValidator;


    public ProjectValidator(
        EventRepository eventRepository,
        InterludeRepository interludeRepository,
        string projectPath)
    {
        this.eventRepository =
            eventRepository;

        this.interludeRepository =
            interludeRepository;


        if (!string.IsNullOrWhiteSpace(
            projectPath))
        {
            chapterRepository =
                new ChapterRepository(
                    projectPath
                );

            attributeRepository =
                new AttributeRepository(
                    projectPath
                );
        }


eventValidator =
    new EventValidator(
        eventRepository,
        chapterRepository,
        attributeRepository
    );


interludeValidator =
    new InterludeValidator(
        chapterRepository,
        attributeRepository
    );
    }


    public ValidationResult ValidateProject()
    {
        ValidationResult result =
            new ValidationResult();


        ValidateChapters(
            result
        );


        ValidateEvents(
            result
        );


        ValidateInterludes(
            result
        );


        return result;
    }


    private void ValidateChapters(
        ValidationResult result)
    {
        if (chapterRepository == null)
        {
            result.AddError(
                "No hay un repositorio de capítulos disponible."
            );

            return;
        }


        List<ChapterDefinitionData> chapters =
            chapterRepository.LoadAll();


        if (chapters == null)
        {
            result.AddError(
                "No se pudieron cargar los capítulos."
            );

            return;
        }


        HashSet<int> chapterNumbers =
            new HashSet<int>();


        HashSet<string> chapterIds =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase
            );


        foreach (
            ChapterDefinitionData chapter
            in chapters)
        {
            if (chapter == null)
            {
                result.AddError(
                    "Se encontró un capítulo nulo."
                );

                continue;
            }


            if (chapter.Number < 1)
            {
                result.AddError(
                    $"El capítulo '{chapter.Id}' tiene un número no válido: {chapter.Number}."
                );
            }
            else if (!chapterNumbers.Add(
                chapter.Number))
            {
                result.AddError(
                    $"El número de capítulo {chapter.Number} está duplicado."
                );
            }


            if (string.IsNullOrWhiteSpace(
                chapter.Id))
            {
                result.AddError(
                    $"El capítulo número {chapter.Number} no tiene ID."
                );
            }
            else if (!chapterIds.Add(
                chapter.Id))
            {
                result.AddError(
                    $"El ID de capítulo '{chapter.Id}' está duplicado."
                );
            }


            if (string.IsNullOrWhiteSpace(
                chapter.Title))
            {
                result.AddWarning(
                    $"El capítulo {chapter.Number} no tiene título."
                );
            }


            ValidateChapterAttributes(
                chapter,
                result
            );
        }
    }


    private void ValidateChapterAttributes(
        ChapterDefinitionData chapter,
        ValidationResult result)
    {
        if (chapter.AttributeIds == null)
        {
            return;
        }


        HashSet<string> attributeIds =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase
            );


        for (
            int index = 0;
            index < chapter.AttributeIds.Count;
            index++)
        {
            string attributeId =
                chapter.AttributeIds[index] ?? "";


            if (string.IsNullOrWhiteSpace(
                attributeId))
            {
                result.AddError(
                    $"El capítulo {chapter.Number} contiene un atributo vacío."
                );

                continue;
            }


            if (!attributeIds.Add(
                attributeId))
            {
                result.AddError(
                    $"El capítulo {chapter.Number} tiene el atributo '{attributeId}' repetido."
                );

                continue;
            }


            if (
                attributeRepository == null ||
                !attributeRepository.Exists(
                    attributeId))
            {
                result.AddError(
                    $"El capítulo {chapter.Number} referencia el atributo '{attributeId}', pero ese atributo no existe."
                );
            }
        }
    }


    private void ValidateEvents(
        ValidationResult result)
    {
        if (eventRepository == null)
        {
            result.AddError(
                "No hay un repositorio de eventos disponible."
            );

            return;
        }


        List<EditorEventData> events =
            eventRepository.LoadAll();


        foreach (
            EditorEventData eventData
            in events)
        {
            if (eventData == null)
            {
                result.AddError(
                    "Se encontró un evento nulo."
                );

                continue;
            }


            ValidationResult eventResult =
                eventValidator.Validate(
                    eventData
                );


            result.Merge(
                eventResult
            );
        }
    }


    private void ValidateInterludes(
        ValidationResult result)
    {
        if (interludeRepository == null)
        {
            result.AddError(
                "No hay un repositorio de interludios disponible."
            );

            return;
        }


        List<EditorInterludeData> interludes =
            interludeRepository.LoadAll();


        foreach (
            EditorInterludeData interludeData
            in interludes)
        {
            if (interludeData == null)
            {
                result.AddError(
                    "Se encontró un interludio nulo."
                );

                continue;
            }


            ValidationResult interludeResult =
                interludeValidator.Validate(
                    interludeData
                );


            result.Merge(
                interludeResult
            );
        }
    }
}