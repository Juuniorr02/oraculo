using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

public class CharacterDatabase
{
    private readonly string charactersFolderPath;


    public CharacterDatabase(
        string projectPath)
    {
        charactersFolderPath =
            Path.Combine(
                projectPath,
                "characters"
            );
    }


    public List<EditorCharacterData> LoadAll()
    {
        List<EditorCharacterData> characters =
            new List<EditorCharacterData>();


        if (!Directory.Exists(
            charactersFolderPath))
        {
            Directory.CreateDirectory(
                charactersFolderPath
            );

            return characters;
        }


        string[] files =
            Directory.GetFiles(
                charactersFolderPath,
                "*.json"
            );


        JsonSerializerOptions options =
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };


        foreach (
            string filePath
            in files)
        {
            try
            {
                string json =
                    File.ReadAllText(
                        filePath
                    );


                EditorCharacterData character =
                    JsonSerializer.Deserialize<EditorCharacterData>(
                        json,
                        options
                    );


                if (character == null)
                {
                    continue;
                }


                if (string.IsNullOrWhiteSpace(
                    character.Id))
                {
                    continue;
                }


                characters.Add(
                    character
                );
            }
            catch (Exception)
            {
                // Un archivo de personaje corrupto
                // no debe impedir cargar los demás.
            }
        }


        return characters;
    }


    public EditorCharacterData GetCharacter(
        string id)
    {
        if (string.IsNullOrWhiteSpace(
            id))
        {
            return null;
        }


        List<EditorCharacterData> characters =
            LoadAll();


        foreach (
            EditorCharacterData character
            in characters)
        {
            if (character == null)
            {
                continue;
            }


            if (character.Id ==
                id)
            {
                return character;
            }
        }


        return null;
    }


    public bool HasCharacter(
        string id)
    {
        return
            GetCharacter(
                id
            ) != null;
    }


    public string GenerateUniqueId(
        string name)
    {
        string baseId =
            GenerateBaseId(
                name
            );


        if (string.IsNullOrWhiteSpace(
            baseId))
        {
            baseId = "character";
        }


        string candidate =
            baseId;


        int number = 2;


        while (
            HasCharacter(candidate))
        {
            candidate =
                baseId +
                "_" +
                number;


            number++;
        }


        return candidate;
    }


    private string GenerateBaseId(
        string name)
    {
        if (string.IsNullOrWhiteSpace(
            name))
        {
            return "";
        }


        string normalized =
            name.Trim()
                .ToLowerInvariant()
                .Normalize(
                    NormalizationForm.FormD
                );


        StringBuilder builder =
            new StringBuilder();


        foreach (
            char character
            in normalized)
        {
            System.Globalization.UnicodeCategory category =
                System.Globalization.CharUnicodeInfo.GetUnicodeCategory(
                    character
                );


            if (
                category ==
                System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                continue;
            }


            if (char.IsLetterOrDigit(
                character))
            {
                builder.Append(
                    character
                );

                continue;
            }


            if (
                character == ' ' ||
                character == '_' ||
                character == '-')
            {
                if (
                    builder.Length > 0 &&
                    builder[^1] != '_')
                {
                    builder.Append('_');
                }
            }
        }


        string result =
            builder.ToString().Trim('_');


        return result;
    }


    public bool SaveCharacter(
        EditorCharacterData character)
    {
        if (character == null)
        {
            return false;
        }


        if (string.IsNullOrWhiteSpace(
            character.Id))
        {
            return false;
        }


        if (!Directory.Exists(
            charactersFolderPath))
        {
            Directory.CreateDirectory(
                charactersFolderPath
            );
        }


        string filePath =
            Path.Combine(
                charactersFolderPath,
                character.Id + ".json"
            );


        JsonSerializerOptions options =
            new JsonSerializerOptions
            {
                WriteIndented = true
            };


        string json =
            JsonSerializer.Serialize(
                character,
                options
            );


        File.WriteAllText(
            filePath,
            json
        );


        return true;
    }


    public bool DeleteCharacter(
        string id)
    {
        if (string.IsNullOrWhiteSpace(
            id))
        {
            return false;
        }


        string filePath =
            Path.Combine(
                charactersFolderPath,
                id + ".json"
            );


        if (!File.Exists(
            filePath))
        {
            return false;
        }


        File.Delete(
            filePath
        );


        return true;
    }
}