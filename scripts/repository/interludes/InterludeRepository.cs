using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

public class InterludeRepository
{
    private readonly string interludesFolder;

    private readonly JsonSerializerOptions jsonOptions =
        new JsonSerializerOptions
        {
            WriteIndented = true
        };


    public InterludeRepository(string projectFolder)
    {
        if (string.IsNullOrWhiteSpace(projectFolder))
        {
            throw new ArgumentException(
                "La ruta del proyecto no puede estar vacía.",
                nameof(projectFolder)
            );
        }


        interludesFolder =
            Path.Combine(
                projectFolder,
                "interludes"
            );


        Directory.CreateDirectory(
            interludesFolder
        );
    }


    public void Save(
        EditorInterludeData interludeData)
    {
        if (interludeData == null)
        {
            throw new ArgumentNullException(
                nameof(interludeData)
            );
        }


        if (string.IsNullOrWhiteSpace(
            interludeData.Id))
        {
            throw new ArgumentException(
                "El interludio debe tener un ID.",
                nameof(interludeData)
            );
        }


        string safeId =
            Path.GetFileName(
                interludeData.Id
            );


        string filePath =
            Path.Combine(
                interludesFolder,
                safeId + ".json"
            );


        string json =
            JsonSerializer.Serialize(
                interludeData,
                jsonOptions
            );


        File.WriteAllText(
            filePath,
            json
        );
    }


    public EditorInterludeData Load(
        string interludeId)
    {
        if (string.IsNullOrWhiteSpace(
            interludeId))
        {
            return null;
        }


        string safeId =
            Path.GetFileName(
                interludeId
            );


        string filePath =
            Path.Combine(
                interludesFolder,
                safeId + ".json"
            );


        if (!File.Exists(filePath))
        {
            return null;
        }


        string json =
            File.ReadAllText(
                filePath
            );


        return JsonSerializer.Deserialize<
            EditorInterludeData
        >(json);
    }


    public bool Exists(
        string interludeId)
    {
        if (string.IsNullOrWhiteSpace(
            interludeId))
        {
            return false;
        }


        string safeId =
            Path.GetFileName(
                interludeId
            );


        string filePath =
            Path.Combine(
                interludesFolder,
                safeId + ".json"
            );


        return File.Exists(
            filePath
        );
    }


    public bool Delete(
        string interludeId)
    {
        if (string.IsNullOrWhiteSpace(
            interludeId))
        {
            return false;
        }


        string safeId =
            Path.GetFileName(
                interludeId
            );


        string filePath =
            Path.Combine(
                interludesFolder,
                safeId + ".json"
            );


        if (!File.Exists(filePath))
        {
            return false;
        }


        File.Delete(
            filePath
        );


        return true;
    }


    public List<string> GetInterludeIds()
    {
        if (!Directory.Exists(
            interludesFolder))
        {
            return new List<string>();
        }


        return Directory
            .GetFiles(
                interludesFolder,
                "*.json"
            )
            .Select(
                Path.GetFileNameWithoutExtension
            )
            .Where(
                id => !string.IsNullOrWhiteSpace(id)
            )
            .OrderBy(
                id => id,
                StringComparer.Ordinal
            )
            .ToList();
    }


    public List<EditorInterludeData> LoadAll()
    {
        List<EditorInterludeData> interludes =
            new List<EditorInterludeData>();


        foreach (
            string interludeId
            in GetInterludeIds())
        {
            EditorInterludeData interlude =
                Load(interludeId);


            if (interlude != null)
            {
                interludes.Add(
                    interlude
                );
            }
        }


        return interludes;
    }
}