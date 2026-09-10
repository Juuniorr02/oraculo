using System.Collections.Generic;

public class ConditionDefinitionDatabase
{


    private readonly List<ConditionTypeDefinition> definitions =
        new();


    public ConditionDefinitionDatabase()
    {
        RegisterDefaults();
    }


    public IReadOnlyList<ConditionTypeDefinition> GetDefinitions()
    {
        return definitions;
    }


    private void RegisterDefaults()
    {
        definitions.Clear();


        definitions.Add(
            new ConditionTypeDefinition
            {
                Id =
                    "characterattribute",

                DisplayName =
                    "Atributo"
            }
        );


        definitions.Add(
            new ConditionTypeDefinition
            {
                Id =
                    "prestige",

                DisplayName =
                    "Prestigio"
            }
        );


        definitions.Add(
            new ConditionTypeDefinition
            {
                Id =
                    "year",

                DisplayName =
                    "Año"
            }
        );


        definitions.Add(
            new ConditionTypeDefinition
            {
                Id =
                    "decision",

                DisplayName =
                    "Decisión"
            }
        );


        definitions.Add(
            new ConditionTypeDefinition
            {
                Id =
                    "empireattribute",

                DisplayName =
                    "Atributo del imperio"
            }
        );


        definitions.Add(
            new ConditionTypeDefinition
            {
                Id =
                    "relationship",

                DisplayName =
                    "Relación"
            }
        );


        definitions.Add(
            new ConditionTypeDefinition
            {
                Id =
                    "relationshiptitle",

                DisplayName =
                    "Título de relación"
            }
        );
    }
}