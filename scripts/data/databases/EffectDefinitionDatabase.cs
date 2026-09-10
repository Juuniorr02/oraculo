using System.Collections.Generic;

public class EffectDefinitionDatabase
{
    private readonly List<EffectTypeDefinition> definitions = new();


    public EffectDefinitionDatabase()
    {
        RegisterDefaults();
    }


    public IReadOnlyList<EffectTypeDefinition> GetDefinitions()
    {
        return definitions;
    }


    private void RegisterDefaults()
    {
        definitions.Clear();

        definitions.Add(
            new EffectTypeDefinition
            {
                Type = EditorEffectType.CharacterAttribute,
                Id = "characterattribute",
                DisplayName = "Atributo"
            }
        );

        definitions.Add(
            new EffectTypeDefinition
            {
                Type = EditorEffectType.Prestige,
                Id = "prestige",
                DisplayName = "Prestigio"
            }
        );

        definitions.Add(
            new EffectTypeDefinition
            {
                Type = EditorEffectType.Decision,
                Id = "decision",
                DisplayName = "Decisión"
            }
        );

        definitions.Add(
            new EffectTypeDefinition
            {
                Type = EditorEffectType.EmpireAttribute,
                Id = "empireattribute",
                DisplayName = "Atributo del imperio"
            }
        );

        definitions.Add(
            new EffectTypeDefinition
            {
                Type = EditorEffectType.Relationship,
                Id = "relationship",
                DisplayName = "Relación"
            }
        );

        definitions.Add(
            new EffectTypeDefinition
            {
                Type = EditorEffectType.RelationshipTitle,
                Id = "relationshiptitle",
                DisplayName = "Título de relación"
            }
        );
    }
}