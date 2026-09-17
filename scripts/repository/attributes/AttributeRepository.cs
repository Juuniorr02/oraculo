using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public class AttributeRepository
{
    private readonly string attributesFolder;

    private readonly JsonSerializerOptions jsonOptions =
        new JsonSerializerOptions
        {
            WriteIndented = true
        };


    public AttributeRepository(
        string projectFolder)
    {
        if (string.IsNullOrWhiteSpace(projectFolder))
        {
            throw new ArgumentException(
                "La ruta del proyecto está vacía.",
                nameof(projectFolder)
            );
        }


        attributesFolder =
            Path.Combine(
                projectFolder,
                "attributes"
            );


        Directory.CreateDirectory(
            attributesFolder
        );
    }


    public bool Save(
        AttributeDefinitionData attributeData)
    {
        if (attributeData == null)
        {
            return false;
        }


        if (string.IsNullOrWhiteSpace(attributeData.Id))
        {
            return false;
        }


        try
        {
            string filePath =
                GetAttributeFilePath(
                    attributeData.Id
                );


            string json =
                JsonSerializer.Serialize(
                    attributeData,
                    jsonOptions
                );


            File.WriteAllText(
                filePath,
                json
            );


            return true;
        }
        catch
        {
            return false;
        }
    }


    public AttributeDefinitionData Load(
        string attributeId)
    {
        if (string.IsNullOrWhiteSpace(attributeId))
        {
            return null;
        }


        string filePath =
            GetAttributeFilePath(
                attributeId
            );


        if (!File.Exists(filePath))
        {
            return null;
        }


        try
        {
            string json =
                File.ReadAllText(
                    filePath
                );


            return JsonSerializer.Deserialize<AttributeDefinitionData>(
                json,
                jsonOptions
            );
        }
        catch
        {
            return null;
        }
    }


    public bool Exists(
        string attributeId)
    {
        if (string.IsNullOrWhiteSpace(attributeId))
        {
            return false;
        }


        return File.Exists(
            GetAttributeFilePath(
                attributeId
            )
        );
    }


    public bool Delete(
        string attributeId)
    {
        if (string.IsNullOrWhiteSpace(attributeId))
        {
            return false;
        }


        string filePath =
            GetAttributeFilePath(
                attributeId
            );


        if (!File.Exists(filePath))
        {
            return false;
        }


        try
        {
            File.Delete(
                filePath
            );

            return true;
        }
        catch
        {
            return false;
        }
    }


    public List<string> GetAttributeIds()
    {
        List<string> attributeIds =
            new List<string>();


        if (!Directory.Exists(attributesFolder))
        {
            return attributeIds;
        }


        string[] files =
            Directory.GetFiles(
                attributesFolder,
                "*.json"
            );


        foreach (
            string filePath
            in files)
        {
            string fileName =
                Path.GetFileNameWithoutExtension(
                    filePath
                );


            if (
                !string.IsNullOrWhiteSpace(
                    fileName
                ))
            {
                attributeIds.Add(
                    fileName
                );
            }
        }


        attributeIds.Sort(
            StringComparer.OrdinalIgnoreCase
        );


        return attributeIds;
    }


    public List<AttributeDefinitionData> LoadAll()
    {
        List<AttributeDefinitionData> attributes =
            new List<AttributeDefinitionData>();


        foreach (
            string attributeId
            in GetAttributeIds())
        {
            AttributeDefinitionData attributeData =
                Load(attributeId);


            if (attributeData != null)
            {
                attributes.Add(
                    attributeData
                );
            }
        }


        return attributes;
    }


    private string GetAttributeFilePath(
        string attributeId)
    {
        string safeFileName =
            Path.GetFileName(
                attributeId
            );


        return Path.Combine(
            attributesFolder,
            safeFileName + ".json"
        );
    }
}