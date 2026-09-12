using System.Collections.Generic;

public class ProjectValidator
{
    private readonly EventRepository eventRepository;
    private readonly EventValidator eventValidator;


    public ProjectValidator(
        EventRepository eventRepository)
    {
        this.eventRepository =
            eventRepository;

        eventValidator =
            new EventValidator();
    }


    public ValidationResult ValidateProject()
    {
        ValidationResult result =
            new ValidationResult();


        if (eventRepository == null)
        {
            result.AddError(
                "No hay un repositorio de eventos disponible."
            );

            return result;
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


        return result;
    }
}