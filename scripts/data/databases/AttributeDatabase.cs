using System.Collections.Generic;

public class AttributeDatabase
{
    private readonly List<AttributeDefinitionData> attributes =
        new List<AttributeDefinitionData>
        {
            // ================================================
            // CAPÍTULO I
            // ================================================

            new AttributeDefinitionData
            {
                Id = "confidence",
                DisplayName = "Confianza",
                Chapter = 1
            },

            new AttributeDefinitionData
            {
                Id = "obedience",
                DisplayName = "Obediencia",
                Chapter = 1
            },

            new AttributeDefinitionData
            {
                Id = "curiosity",
                DisplayName = "Curiosidad",
                Chapter = 1
            },


            // ================================================
            // CAPÍTULO II
            // ================================================

            new AttributeDefinitionData
            {
                Id = "sociability",
                DisplayName = "Sociabilidad",
                Chapter = 2
            },

            new AttributeDefinitionData
            {
                Id = "compassion",
                DisplayName = "Compasión",
                Chapter = 2
            },

            new AttributeDefinitionData
            {
                Id = "ambition",
                DisplayName = "Ambición",
                Chapter = 2
            },


            // ================================================
            // CAPÍTULO III
            // ================================================

            new AttributeDefinitionData
            {
                Id = "bravery",
                DisplayName = "Valentía",
                Chapter = 3
            },

            new AttributeDefinitionData
            {
                Id = "cunning",
                DisplayName = "Astucia",
                Chapter = 3
            },

            new AttributeDefinitionData
            {
                Id = "tradition",
                DisplayName = "Tradición",
                Chapter = 3
            },


            // ================================================
            // CAPÍTULO IV
            // ================================================

            new AttributeDefinitionData
            {
                Id = "discipline",
                DisplayName = "Disciplina",
                Chapter = 4
            },

            new AttributeDefinitionData
            {
                Id = "prudence",
                DisplayName = "Prudencia",
                Chapter = 4
            },

            new AttributeDefinitionData
            {
                Id = "idealism",
                DisplayName = "Idealismo",
                Chapter = 4
            },


            // ================================================
            // CAPÍTULO V
            // ================================================

            new AttributeDefinitionData
            {
                Id = "loyalty",
                DisplayName = "Lealtad",
                Chapter = 5
            },

            new AttributeDefinitionData
            {
                Id = "generosity",
                DisplayName = "Generosidad",
                Chapter = 5
            },

            new AttributeDefinitionData
            {
                Id = "justice",
                DisplayName = "Justicia",
                Chapter = 5
            },


            // ================================================
            // CAPÍTULO VI
            // ================================================

            new AttributeDefinitionData
            {
                Id = "authority",
                DisplayName = "Autoridad",
                Chapter = 6
            },

            new AttributeDefinitionData
            {
                Id = "diplomacy",
                DisplayName = "Diplomacia",
                Chapter = 6
            },

            new AttributeDefinitionData
            {
                Id = "perseverance",
                DisplayName = "Perseverancia",
                Chapter = 6
            },


            // ================================================
            // CAPÍTULO VII
            // ================================================

            new AttributeDefinitionData
            {
                Id = "devotion",
                DisplayName = "Devoción",
                Chapter = 7
            },

            new AttributeDefinitionData
            {
                Id = "sacrifice",
                DisplayName = "Sacrificio",
                Chapter = 7
            },

            new AttributeDefinitionData
            {
                Id = "hope",
                DisplayName = "Esperanza",
                Chapter = 7
            }
        };


    public List<AttributeDefinitionData> GetAllAttributes()
    {
        return new List<AttributeDefinitionData>(
            attributes
        );
    }


    public List<AttributeDefinitionData> GetAttributesForChapter(
        int chapter)
    {
        List<AttributeDefinitionData> result =
            new List<AttributeDefinitionData>();


        foreach (
            AttributeDefinitionData attribute
            in attributes)
        {
            if (
                attribute.Chapter ==
                chapter)
            {
                result.Add(
                    attribute
                );
            }
        }


        return result;
    }


    public AttributeDefinitionData GetAttribute(
        string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }


        foreach (
            AttributeDefinitionData attribute
            in attributes)
        {
            if (
                attribute.Id ==
                id)
            {
                return attribute;
            }
        }


        return null;
    }
}