using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public class ChapterRepository
{
    private readonly string chaptersFolder;

    private readonly JsonSerializerOptions jsonOptions =
        new JsonSerializerOptions
        {
            WriteIndented = true
        };


    public ChapterRepository(
        string projectFolder)
    {
        if (string.IsNullOrWhiteSpace(projectFolder))
        {
            throw new ArgumentException(
                "La ruta del proyecto está vacía.",
                nameof(projectFolder)
            );
        }


        chaptersFolder =
            Path.Combine(
                projectFolder,
                "chapters"
            );


        Directory.CreateDirectory(
            chaptersFolder
        );
    }


    public bool Save(
        ChapterDefinitionData chapterData)
    {
        if (chapterData == null)
        {
            return false;
        }


        if (string.IsNullOrWhiteSpace(chapterData.Id))
        {
            return false;
        }


        try
        {
            string filePath =
                GetChapterFilePath(
                    chapterData.Id
                );


            string json =
                JsonSerializer.Serialize(
                    chapterData,
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


    public ChapterDefinitionData Load(
        string chapterId)
    {
        if (string.IsNullOrWhiteSpace(chapterId))
        {
            return null;
        }


        string filePath =
            GetChapterFilePath(
                chapterId
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


            return JsonSerializer.Deserialize<ChapterDefinitionData>(
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
        string chapterId)
    {
        if (string.IsNullOrWhiteSpace(chapterId))
        {
            return false;
        }


        return File.Exists(
            GetChapterFilePath(
                chapterId
            )
        );
    }


    public bool Delete(
        string chapterId)
    {
        if (string.IsNullOrWhiteSpace(chapterId))
        {
            return false;
        }


        string filePath =
            GetChapterFilePath(
                chapterId
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


    public List<string> GetChapterIds()
    {
        List<string> chapterIds =
            new List<string>();


        if (!Directory.Exists(chaptersFolder))
        {
            return chapterIds;
        }


        string[] files =
            Directory.GetFiles(
                chaptersFolder,
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
                chapterIds.Add(
                    fileName
                );
            }
        }


        chapterIds.Sort(
            StringComparer.OrdinalIgnoreCase
        );


        return chapterIds;
    }


    public List<ChapterDefinitionData> LoadAll()
    {
        List<ChapterDefinitionData> chapters =
            new List<ChapterDefinitionData>();


        foreach (
            string chapterId
            in GetChapterIds())
        {
            ChapterDefinitionData chapterData =
                Load(chapterId);


            if (chapterData != null)
            {
                chapters.Add(
                    chapterData
                );
            }
        }


        return chapters;
    }


    private string GetChapterFilePath(
        string chapterId)
    {
        string safeFileName =
            Path.GetFileName(
                chapterId
            );


        return Path.Combine(
            chaptersFolder,
            safeFileName + ".json"
        );
    }
}