
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

public sealed class ChapterPageIdMigrationPlan
{
    // JSON actualizado en memoria, indexado por ruta del archivo.
    public Dictionary<string, string> UpdatedFiles { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    // IDs de las páginas en su orden original, agrupados por capítulo.
    public Dictionary<string, List<string>> PageIdsByChapter { get; } =
        new(StringComparer.Ordinal);

    public List<string> Errors { get; } = new();

    public int AssignedIds { get; internal set; }

    public int ChangedFiles => UpdatedFiles.Count;

    public bool HasErrors => Errors.Count > 0;

    public bool CanContinue => !HasErrors;
}

public sealed class ChapterPageIdMigrator
{
    private readonly string _chaptersFolder;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public ChapterPageIdMigrator(string projectFolder)
    {
        if (string.IsNullOrWhiteSpace(projectFolder))
        {
            throw new ArgumentException(
                "La ruta del proyecto está vacía.",
                nameof(projectFolder));
        }

        _chaptersFolder = Path.Combine(
            Path.GetFullPath(projectFolder),
            "chapters");
    }

    /// <summary>
    /// Prepara la migración de IDs sin modificar archivos.
    /// </summary>
    public ChapterPageIdMigrationPlan Prepare()
    {
        var plan = new ChapterPageIdMigrationPlan();

        if (!Directory.Exists(_chaptersFolder))
        {
            plan.Errors.Add(
                $"No existe la carpeta de capítulos: {_chaptersFolder}");

            return plan;
        }

        string[] files;

        try
        {
            files = Directory.GetFiles(
                _chaptersFolder,
                "*.json",
                SearchOption.TopDirectoryOnly);

            Array.Sort(files, StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            plan.Errors.Add(
                $"No se pudieron enumerar los archivos de capítulos: {ex.Message}");

            return plan;
        }

        var chapterIdsSeen = new HashSet<string>(
            StringComparer.Ordinal);

        foreach (string filePath in files)
        {
            JsonObject chapter;

            try
            {
                string json = File.ReadAllText(filePath);

                if (JsonNode.Parse(json) is not JsonObject chapterObject)
                {
                    plan.Errors.Add(
                        $"El archivo '{filePath}' no contiene un objeto JSON válido.");

                    continue;
                }

                chapter = chapterObject;
            }
            catch (Exception ex) when (
                ex is JsonException ||
                ex is IOException ||
                ex is UnauthorizedAccessException)
            {
                plan.Errors.Add(
                    $"No se pudo leer o analizar '{filePath}': {ex.Message}");

                continue;
            }

            string? chapterId = ReadStringProperty(
                chapter,
                "Id",
                out bool chapterIdExists);

            if (!chapterIdExists ||
                string.IsNullOrWhiteSpace(chapterId))
            {
                plan.Errors.Add(
                    $"El capítulo '{filePath}' no tiene un ID válido.");

                continue;
            }

            if (!chapterIdsSeen.Add(chapterId))
            {
                plan.Errors.Add(
                    $"El ID de capítulo '{chapterId}' aparece en más de un archivo.");

                continue;
            }

            string? pagesPropertyName = FindPropertyName(
                chapter,
                "Pages");

            var pageIds = new List<string>();
            var pageIdsSeen = new HashSet<string>(
                StringComparer.Ordinal);

            bool chapterChanged = false;

            // Pages ausente o null equivale a una lista vacía.
            if (pagesPropertyName == null ||
                chapter[pagesPropertyName] == null)
            {
                plan.PageIdsByChapter.Add(chapterId, pageIds);
                continue;
            }

            if (chapter[pagesPropertyName] is not JsonArray pages)
            {
                plan.Errors.Add(
                    $"La propiedad Pages del capítulo '{chapterId}' no es una lista.");

                continue;
            }

            for (int index = 0; index < pages.Count; index++)
            {
                if (pages[index] is not JsonObject page)
                {
                    plan.Errors.Add(
                        $"La página {index} del capítulo '{chapterId}' no es un objeto JSON.");

                    continue;
                }

                string? idPropertyName = FindPropertyName(page, "Id");

                string? pageId = idPropertyName == null
                    ? null
                    : ReadStringProperty(page, "Id", out _);

                if (idPropertyName != null &&
                    pageId == null &&
                    page[idPropertyName] != null)
                {
                    plan.Errors.Add(
                        $"El ID de la página {index} del capítulo '{chapterId}' no es texto.");

                    continue;
                }

                if (string.IsNullOrWhiteSpace(pageId))
                {
                    do
                    {
                        pageId = Guid.NewGuid().ToString("N");
                    }
                    while (pageIdsSeen.Contains(pageId));

                    page[idPropertyName ?? "Id"] =
                        JsonValue.Create(pageId);

                    chapterChanged = true;
                    plan.AssignedIds++;
                }

                // No añadir IDs duplicados al plan: hacerlo desplazaría
                // las correspondencias entre páginas e IDs.
                if (!pageIdsSeen.Add(pageId))
                {
                    plan.Errors.Add(
                        $"El ID de página '{pageId}' está duplicado " +
                        $"en el capítulo '{chapterId}'.");

                    continue;
                }

                pageIds.Add(pageId);
            }

            plan.PageIdsByChapter.Add(chapterId, pageIds);

            if (chapterChanged)
            {
                plan.UpdatedFiles[filePath] =
                    chapter.ToJsonString(JsonOptions) +
                    Environment.NewLine;
            }
        }

        // No se permite escribir parcialmente si algún archivo falla.
        if (plan.HasErrors)
            plan.UpdatedFiles.Clear();

        return plan;
    }

    private static string? FindPropertyName(
        JsonObject obj,
        string expectedName)
    {
        foreach (KeyValuePair<string, JsonNode?> property in obj)
        {
            if (string.Equals(
                property.Key,
                expectedName,
                StringComparison.OrdinalIgnoreCase))
            {
                return property.Key;
            }
        }

        return null;
    }

    private static string? ReadStringProperty(
        JsonObject obj,
        string propertyName,
        out bool exists)
    {
        string? actualName = FindPropertyName(obj, propertyName);

        exists = actualName != null;

        if (actualName == null)
            return null;

        JsonNode? value = obj[actualName];

        if (value is JsonValue jsonValue &&
            jsonValue.TryGetValue<string>(out string? text))
        {
            return text;
        }

        return null;
    }
}
