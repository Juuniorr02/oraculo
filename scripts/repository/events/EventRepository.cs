using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

public class EventRepository
{
    private readonly string eventsFolder;

    private readonly JsonSerializerOptions jsonOptions =
        new JsonSerializerOptions
        {
            WriteIndented = true
        };


    public EventRepository(
        string projectFolder)
    {
        if (string.IsNullOrWhiteSpace(projectFolder))
        {
            throw new ArgumentException(
                "La ruta del proyecto está vacía.",
                nameof(projectFolder)
            );
        }


        eventsFolder =
            Path.Combine(
                projectFolder,
                "events"
            );


        Directory.CreateDirectory(
            eventsFolder
        );
    }


    public bool Save(
        EditorEventData eventData)
    {
        if (eventData == null)
        {
            return false;
        }


        if (string.IsNullOrWhiteSpace(eventData.Id))
        {
            return false;
        }


        try
        {
            string filePath =
                GetEventFilePath(
                    eventData.Id
                );


            string json =
                JsonSerializer.Serialize(
                    eventData,
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


    public EditorEventData Load(
        string eventId)
    {
        if (string.IsNullOrWhiteSpace(eventId))
        {
            return null;
        }


        string filePath =
            GetEventFilePath(
                eventId
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


            return JsonSerializer.Deserialize<EditorEventData>(
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
        string eventId)
    {
        if (string.IsNullOrWhiteSpace(eventId))
        {
            return false;
        }


        return File.Exists(
            GetEventFilePath(
                eventId
            )
        );
    }


    public bool Delete(
        string eventId)
    {
        if (string.IsNullOrWhiteSpace(eventId))
        {
            return false;
        }


        string filePath =
            GetEventFilePath(
                eventId
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


    public List<string> GetEventIds()
    {
        List<string> eventIds =
            new List<string>();


        if (!Directory.Exists(eventsFolder))
        {
            return eventIds;
        }


        string[] files =
            Directory.GetFiles(
                eventsFolder,
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
                eventIds.Add(
                    fileName
                );
            }
        }


        eventIds.Sort(
            StringComparer.OrdinalIgnoreCase
        );


        return eventIds;
    }


    public List<EditorEventData> LoadAll()
    {
        List<EditorEventData> events =
            new List<EditorEventData>();


        foreach (
            string eventId
            in GetEventIds())
        {
            EditorEventData eventData =
                Load(eventId);


            if (eventData != null)
            {
                events.Add(
                    eventData
                );
            }
        }


        return events;
    }


    private string GetEventFilePath(
        string eventId)
    {
        string safeFileName =
            Path.GetFileName(
                eventId
            );


        return Path.Combine(
            eventsFolder,
            safeFileName + ".json"
        );
    }
}