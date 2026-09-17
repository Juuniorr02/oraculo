using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public static class ChapterDataGenerator
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


        string chaptersFolder =
            Path.Combine(
                projectFolder,
                "chapters"
            );


        Directory.CreateDirectory(
            chaptersFolder
        );


        List<ChapterDefinitionData> chapters =
            CreateChapters();


        int generated = 0;


        foreach (
            ChapterDefinitionData chapter
            in chapters)
        {
            if (chapter == null ||
                string.IsNullOrWhiteSpace(chapter.Id))
            {
                continue;
            }


            string filePath =
                Path.Combine(
                    chaptersFolder,
                    chapter.Id + ".json"
                );


            string json =
                JsonSerializer.Serialize(
                    chapter,
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


    private static List<ChapterDefinitionData> CreateChapters()
    {
        return new List<ChapterDefinitionData>
        {
            new ChapterDefinitionData
            {
                Id = "chapter_01",
                Number = 1,
                Title = "El hijo sin nombre",
                StartYear = 0,
                StartMonth = 1,

                AttributeIds = new List<string>
                {
                    "confidence",
                    "obedience",
                    "curiosity"
                }
            },


            new ChapterDefinitionData
            {
                Id = "chapter_02",
                Number = 2,
                Title = "Dos hijos de la misma sangre",
                StartYear = 4,
                StartMonth = 1,

                AttributeIds = new List<string>
                {
                    "sociability",
                    "compassion",
                    "ambition"
                }
            },


            new ChapterDefinitionData
            {
                Id = "chapter_03",
                Number = 3,
                Title = "El mundo más allá de Vance",
                StartYear = 10,
                StartMonth = 1,

                AttributeIds = new List<string>
                {
                    "bravery",
                    "cunning",
                    "tradition"
                }
            },


            new ChapterDefinitionData
            {
                Id = "chapter_04",
                Number = 4,
                Title = "El camino",
                StartYear = 15,
                StartMonth = 1,

                AttributeIds = new List<string>
                {
                    "discipline",
                    "prudence",
                    "idealism"
                }
            },


            new ChapterDefinitionData
            {
                Id = "chapter_05",
                Number = 5,
                Title = "La mayoría de edad",
                StartYear = 18,
                StartMonth = 1,

                AttributeIds = new List<string>
                {
                    "loyalty",
                    "generosity",
                    "justice"
                }
            },


            new ChapterDefinitionData
            {
                Id = "chapter_06",
                Number = 6,
                Title = "Resurgir Vance",
                StartYear = 25,
                StartMonth = 1,

                AttributeIds = new List<string>
                {
                    "authority",
                    "diplomacy",
                    "perseverance"
                }
            },


            new ChapterDefinitionData
            {
                Id = "chapter_07",
                Number = 7,
                Title = "La Crisis",
                StartYear = 35,
                StartMonth = 1,

                AttributeIds = new List<string>
                {
                    "devotion",
                    "sacrifice",
                    "hope"
                }
            }
        };
    }
}