using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public static class AttributeDataGenerator
{
    private static readonly JsonSerializerOptions JsonOptions =
        new JsonSerializerOptions
        {
            WriteIndented = true
        };

    public static int Generate(string projectFolder)
    {
        if (string.IsNullOrWhiteSpace(projectFolder))
        {
            return 0;
        }

        string attributesFolder =
            Path.Combine(projectFolder, "attributes");

        Directory.CreateDirectory(attributesFolder);

        List<AttributeDefinitionData> attributes =
            CreateAttributes();

        int generated = 0;

        foreach (AttributeDefinitionData attribute in attributes)
        {
            if (attribute == null ||
                string.IsNullOrWhiteSpace(attribute.Id))
            {
                continue;
            }

            string filePath =
                Path.Combine(
                    attributesFolder,
                    attribute.Id + ".json"
                );

            string json =
                JsonSerializer.Serialize(
                    attribute,
                    JsonOptions
                );

            File.WriteAllText(
                filePath,
                json
            );

            generated++;
        }

        return generated;
    }

    private static List<AttributeDefinitionData> CreateAttributes()
    {
        return new List<AttributeDefinitionData>
        {
            new AttributeDefinitionData
            {
                Id = "confidence",
                DisplayName = "Confianza",
                Description = "Disposición para confiar en los demás y en el mundo que le rodea.",
                NegativeName = "Desconfiado",
                LowName = "Suspicaz",
                NeutralName = "Neutral",
                HighName = "Confiado",
                PositiveName = "Ingenuo"
            },

            new AttributeDefinitionData
            {
                Id = "obedience",
                DisplayName = "Obediencia",
                Description = "Tendencia a aceptar las normas, órdenes y autoridad establecida.",
                NegativeName = "Rebelde",
                LowName = "Desobediente",
                NeutralName = "Neutral",
                HighName = "Obediente",
                PositiveName = "Sumiso"
            },

            new AttributeDefinitionData
            {
                Id = "curiosity",
                DisplayName = "Curiosidad",
                Description = "Deseo de descubrir, aprender y comprender aquello que le rodea.",
                NegativeName = "Indiferente",
                LowName = "Apático",
                NeutralName = "Neutral",
                HighName = "Curioso",
                PositiveName = "Inquisitivo"
            },

            new AttributeDefinitionData
            {
                Id = "sociability",
                DisplayName = "Sociabilidad",
                Description = "Facilidad para relacionarse con otras personas.",
                NegativeName = "Aislado",
                LowName = "Reservado",
                NeutralName = "Neutral",
                HighName = "Sociable",
                PositiveName = "Extrovertido"
            },

            new AttributeDefinitionData
            {
                Id = "compassion",
                DisplayName = "Compasión",
                Description = "Capacidad para comprender y sentir consideración por el sufrimiento ajeno.",
                NegativeName = "Cruel",
                LowName = "Insensible",
                NeutralName = "Neutral",
                HighName = "Compasivo",
                PositiveName = "Misericordioso"
            },

            new AttributeDefinitionData
            {
                Id = "ambition",
                DisplayName = "Ambición",
                Description = "Deseo de alcanzar una posición, objetivo o grandeza personal.",
                NegativeName = "Conformista",
                LowName = "Apocado",
                NeutralName = "Neutral",
                HighName = "Ambicioso",
                PositiveName = "Aspirante"
            },

            new AttributeDefinitionData
            {
                Id = "bravery",
                DisplayName = "Valentía",
                Description = "Capacidad para enfrentarse al peligro o a la adversidad.",
                NegativeName = "Cobarde",
                LowName = "Temeroso",
                NeutralName = "Neutral",
                HighName = "Valiente",
                PositiveName = "Intrépido"
            },

            new AttributeDefinitionData
            {
                Id = "cunning",
                DisplayName = "Astucia",
                Description = "Capacidad para desenvolverse mediante la inteligencia, el engaño o la estrategia.",
                NegativeName = "Franco",
                LowName = "Ingenuo",
                NeutralName = "Neutral",
                HighName = "Astuto",
                PositiveName = "Maquinador"
            },

            new AttributeDefinitionData
            {
                Id = "tradition",
                DisplayName = "Tradición",
                Description = "Valoración de las costumbres, estructuras y conocimientos heredados.",
                NegativeName = "Innovador",
                LowName = "Reformista",
                NeutralName = "Neutral",
                HighName = "Tradicionalista",
                PositiveName = "Conservador"
            },

            new AttributeDefinitionData
            {
                Id = "discipline",
                DisplayName = "Disciplina",
                Description = "Capacidad para controlar los propios actos y mantener una conducta constante.",
                NegativeName = "Impulsivo",
                LowName = "Irreflexivo",
                NeutralName = "Neutral",
                HighName = "Disciplinado",
                PositiveName = "Metódico"
            },

            new AttributeDefinitionData
            {
                Id = "prudence",
                DisplayName = "Prudencia",
                Description = "Capacidad para actuar con cautela y valorar las consecuencias de los propios actos.",
                NegativeName = "Temerario",
                LowName = "Imprudente",
                NeutralName = "Neutral",
                HighName = "Cauteloso",
                PositiveName = "Prudente"
            },

            new AttributeDefinitionData
            {
                Id = "idealism",
                DisplayName = "Idealismo",
                Description = "Tendencia a actuar de acuerdo con principios e ideales incluso frente a las dificultades.",
                NegativeName = "Pragmático",
                LowName = "Realista",
                NeutralName = "Neutral",
                HighName = "Idealista",
                PositiveName = "Idealista radical"
            },

            new AttributeDefinitionData
            {
                Id = "loyalty",
                DisplayName = "Lealtad",
                Description = "Compromiso firme con las personas, causas o principios a los que se ha jurado fidelidad.",
                NegativeName = "Oportunista",
                LowName = "Inconstante",
                NeutralName = "Neutral",
                HighName = "Leal",
                PositiveName = "Inquebrantable"
            },

            new AttributeDefinitionData
            {
                Id = "generosity",
                DisplayName = "Generosidad",
                Description = "Disposición para compartir recursos, tiempo o esfuerzo con los demás.",
                NegativeName = "Avaro",
                LowName = "Tacaño",
                NeutralName = "Neutral",
                HighName = "Generoso",
                PositiveName = "Altruista"
            },

            new AttributeDefinitionData
            {
                Id = "justice",
                DisplayName = "Justicia",
                Description = "Forma en que una persona entiende y aplica aquello que considera justo.",
                NegativeName = "Vengativo",
                LowName = "Rencoroso",
                NeutralName = "Neutral",
                HighName = "Justo",
                PositiveName = "Imparcial"
            },

            new AttributeDefinitionData
            {
                Id = "authority",
                DisplayName = "Autoridad",
                Description = "Tendencia a ejercer poder sobre otros y establecer el orden.",
                NegativeName = "Igualitario",
                LowName = "Tolerante",
                NeutralName = "Neutral",
                HighName = "Autoritario",
                PositiveName = "Dominante"
            },

            new AttributeDefinitionData
            {
                Id = "diplomacy",
                DisplayName = "Diplomacia",
                Description = "Capacidad para resolver conflictos y alcanzar objetivos mediante las relaciones y la negociación.",
                NegativeName = "Dominador",
                LowName = "Intimidante",
                NeutralName = "Neutral",
                HighName = "Diplomático",
                PositiveName = "Conciliador"
            },

            new AttributeDefinitionData
            {
                Id = "perseverance",
                DisplayName = "Perseverancia",
                Description = "Capacidad para mantener el esfuerzo y continuar avanzando pese a las dificultades.",
                NegativeName = "Adaptable",
                LowName = "Flexible",
                NeutralName = "Neutral",
                HighName = "Perseverante",
                PositiveName = "Inquebrantable"
            },

            new AttributeDefinitionData
            {
                Id = "devotion",
                DisplayName = "Devoción",
                Description = "Grado de entrega y fidelidad hacia aquello en lo que se cree.",
                NegativeName = "Escéptico",
                LowName = "Dudoso",
                NeutralName = "Neutral",
                HighName = "Devoto",
                PositiveName = "Fanático"
            },

            new AttributeDefinitionData
            {
                Id = "sacrifice",
                DisplayName = "Sacrificio",
                Description = "Disposición para renunciar al propio bienestar por aquello que se considera importante.",
                NegativeName = "Superviviente",
                LowName = "Conservador",
                NeutralName = "Neutral",
                HighName = "Sacrificado",
                PositiveName = "Mártir"
            },

            new AttributeDefinitionData
            {
                Id = "hope",
                DisplayName = "Esperanza",
                Description = "Capacidad para mantener la confianza en un futuro favorable pese a las dificultades.",
                NegativeName = "Fatalista",
                LowName = "Pesimista",
                NeutralName = "Neutral",
                HighName = "Esperanzado",
                PositiveName = "Optimista"
            }
        };
    }
}