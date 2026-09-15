using System.Collections.Generic;

public class ProjectValidator
{
    private readonly EventRepository eventRepository;
    private readonly InterludeRepository interludeRepository;

    private readonly EventValidator eventValidator;
    private readonly InterludeValidator interludeValidator;

    public ProjectValidator(
        EventRepository eventRepository,
        InterludeRepository interludeRepository)
    {
        this.eventRepository =
            eventRepository;

        this.interludeRepository =
            interludeRepository;

        eventValidator =
            new EventValidator();

        interludeValidator =
            new InterludeValidator();
    }

    public ValidationResult ValidateProject()
    {
        ValidationResult result =
            new ValidationResult();

        ValidateEvents(
            result
        );

        ValidateInterludes(
            result
        );

        return result;
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