using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public class RelationshipDatabase
{
    private readonly string relationshipsFolderPath;


    public RelationshipDatabase(
        string projectPath)
    {
        relationshipsFolderPath =
            Path.Combine(
                projectPath,
                "relationships"
            );
    }


    public List<EditorRelationshipDefinitionData> LoadAll()
    {
        List<EditorRelationshipDefinitionData> relationships =
            new List<EditorRelationshipDefinitionData>();


        if (!Directory.Exists(
            relationshipsFolderPath))
        {
            Directory.CreateDirectory(
                relationshipsFolderPath
            );

            return relationships;
        }


        string[] files =
            Directory.GetFiles(
                relationshipsFolderPath,
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


                EditorRelationshipDefinitionData relationship =
                    JsonSerializer.Deserialize<
                        EditorRelationshipDefinitionData>(
                            json,
                            options
                        );


                if (relationship == null)
                {
                    continue;
                }


                if (string.IsNullOrWhiteSpace(
                    relationship.CharacterId))
                {
                    continue;
                }


                if (relationship.Titles == null)
                {
                    relationship.Titles =
                        new List<string>();
                }


                relationship.InitialValue =
                    Math.Clamp(
                        relationship.InitialValue,
                        -10,
                        10
                    );


                relationships.Add(
                    relationship
                );
            }
            catch (Exception)
            {
                // Un archivo corrupto no debe impedir
                // cargar las demás relaciones.
            }
        }


        return relationships;
    }


    public EditorRelationshipDefinitionData GetRelationship(
        string characterId)
    {
        if (string.IsNullOrWhiteSpace(
            characterId))
        {
            return null;
        }


        List<EditorRelationshipDefinitionData> relationships =
            LoadAll();


        foreach (
            EditorRelationshipDefinitionData relationship
            in relationships)
        {
            if (relationship == null)
            {
                continue;
            }


            if (relationship.CharacterId ==
                characterId)
            {
                return relationship;
            }
        }


        return null;
    }


    public bool HasRelationship(
        string characterId)
    {
        return
            GetRelationship(
                characterId
            ) != null;
    }


    public bool SaveRelationship(
        EditorRelationshipDefinitionData relationship)
    {
        if (relationship == null)
        {
            return false;
        }


        if (string.IsNullOrWhiteSpace(
            relationship.CharacterId))
        {
            return false;
        }


        if (!Directory.Exists(
            relationshipsFolderPath))
        {
            Directory.CreateDirectory(
                relationshipsFolderPath
            );
        }


        relationship.InitialValue =
            Math.Clamp(
                relationship.InitialValue,
                -10,
                10
            );


        if (relationship.Titles == null)
        {
            relationship.Titles =
                new List<string>();
        }


        string filePath =
            Path.Combine(
                relationshipsFolderPath,
                relationship.CharacterId + ".json"
            );


        JsonSerializerOptions options =
            new JsonSerializerOptions
            {
                WriteIndented = true
            };


        string json =
            JsonSerializer.Serialize(
                relationship,
                options
            );


        File.WriteAllText(
            filePath,
            json
        );


        return true;
    }


    public bool DeleteRelationship(
        string characterId)
    {
        if (string.IsNullOrWhiteSpace(
            characterId))
        {
            return false;
        }


        string filePath =
            Path.Combine(
                relationshipsFolderPath,
                characterId + ".json"
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


    public List<string> GetTitles(
        string characterId)
    {
        EditorRelationshipDefinitionData relationship =
            GetRelationship(
                characterId
            );


        if (relationship == null ||
            relationship.Titles == null)
        {
            return new List<string>();
        }


        return new List<string>(
            relationship.Titles
        );
    }


    public bool HasTitle(
        string characterId,
        string title)
    {
        if (string.IsNullOrWhiteSpace(
            title))
        {
            return false;
        }


        EditorRelationshipDefinitionData relationship =
            GetRelationship(
                characterId
            );


        if (relationship == null ||
            relationship.Titles == null)
        {
            return false;
        }


        return relationship.Titles.Contains(
            title
        );
    }
}