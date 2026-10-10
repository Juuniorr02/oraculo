
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

public sealed class LocalizationExportResult
{
    public int ExtractedEntries { get; internal set; }
    public int AddedTranslations { get; internal set; }
    public int PreservedTranslations { get; internal set; }
    public bool Saved { get; internal set; }
    public bool ValidationOnly { get; internal set; }

    public List<LocalizationIssue> Issues { get; } = new();

    public bool HasErrors => Issues.Any(issue => issue.IsError);

    public override string ToString()
    {
        return
            $"Modo: {(ValidationOnly ? "Validación" : "Exportación")}\n" +
            $"Entradas extraídas: {ExtractedEntries}\n" +
            $"Traducciones nuevas: {AddedTranslations}\n" +
            $"Traducciones conservadas: {PreservedTranslations}\n" +
            $"Archivo guardado: {(Saved ? "Sí" : "No")}\n" +
            $"Errores: {Issues.Count(issue => issue.IsError)}\n" +
            $"Avisos: {Issues.Count(issue => !issue.IsError)}";
    }
}

public sealed class LocalizationExporter
{
    private const string SpanishLanguageCode = "es";

    private static readonly string[] RequiredFolders =
    {
        "events",
        "interludes",
        "chapters",
        "attributes",
        "characters"
    };

    private readonly string _projectFolder;
    private readonly LocalizationRepository _repository;
    private readonly LocalizationValidator _validator;

    private readonly List<LocalizationEntry> _entries = new();
    private readonly List<LocalizationIssue> _readIssues = new();

    private ChapterPageIdMigrationPlan? _chapterMigrationPlan;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public LocalizationExporter(string projectFolder)
    {
        if (string.IsNullOrWhiteSpace(projectFolder))
        {
            throw new ArgumentException(
                "La carpeta del proyecto no puede estar vacía.",
                nameof(projectFolder));
        }

        _projectFolder = Path.GetFullPath(projectFolder);
        _repository = new LocalizationRepository(_projectFolder);
        _validator = new LocalizationValidator();
    }

    /// <summary>
    /// Comprueba el contenido y el catálogo español sin modificar archivos.
    /// Las traducciones de las páginas de capítulos se normalizan en memoria.
    /// </summary>
    public LocalizationExportResult ValidateSpanish()
    {
        var result = new LocalizationExportResult
        {
            ValidationOnly = true
        };

        _chapterMigrationPlan =
            new ChapterPageIdMigrator(_projectFolder).Prepare();

        if (_chapterMigrationPlan.HasErrors)
        {
            AddChapterMigrationErrors(result, _chapterMigrationPlan);
            return result;
        }

        if (!ExtractContent(result))
            return result;

        if (!TryLoadSpanish(
            result,
            out Dictionary<string, string> translations))
        {
            return result;
        }

        // Trabajamos sobre una copia. Validar nunca escribe archivos.
        translations = RemoveChapterPageTranslations(
            translations,
            out int removedChapterPageKeys);

        // Simular el resultado que tendría una exportación correcta.
        RegenerateChapterPageTranslations(translations);

        result.Issues.AddRange(
            _validator.ValidateTranslations(
                _entries,
                translations));

        if (_chapterMigrationPlan.AssignedIds > 0 ||
            removedChapterPageKeys > 0)
        {
            result.Issues.Add(new LocalizationIssue(
                false,
                "CHAPTER_MIGRATION_PENDING",
                $"Hay una migración pendiente: " +
                $"{_chapterMigrationPlan.AssignedIds} IDs por asignar y " +
                $"{removedChapterPageKeys} claves antiguas de páginas " +
                "por regenerar. La validación no ha modificado ningún archivo."));
        }

        return result;
    }

    /// <summary>
    /// Asigna IDs permanentes a las páginas de capítulos y regenera
    /// sus traducciones españolas. Conserva las demás traducciones.
    /// Solo guarda si la validación termina sin errores.
    /// </summary>
    public LocalizationExportResult ExportSpanish()
    {
        var result = new LocalizationExportResult();

        _chapterMigrationPlan =
            new ChapterPageIdMigrator(_projectFolder).Prepare();

        if (_chapterMigrationPlan.HasErrors)
        {
            AddChapterMigrationErrors(result, _chapterMigrationPlan);
            return result;
        }

        if (!ExtractContent(result))
            return result;

        if (!TryLoadSpanish(
            result,
            out Dictionary<string, string> translations))
        {
            return result;
        }

        // Las traducciones de las páginas de capítulos son de prueba:
        // eliminamos todas sus claves antiguas y actuales.
        translations = RemoveChapterPageTranslations(
            translations,
            out int removedChapterPageKeys);

        // Añadir las claves extraídas.
        // Las páginas de capítulos se añaden de nuevo desde su texto fuente.
        foreach (LocalizationEntry entry in _entries)
        {
            if (translations.ContainsKey(entry.Key))
            {
                result.PreservedTranslations++;
                continue;
            }

            translations.Add(entry.Key, entry.SourceText);
            result.AddedTranslations++;
        }

        result.Issues.AddRange(
            _validator.ValidateTranslations(_entries, translations));

        if (result.HasErrors)
            return result;

        if (!TryCommitMigration(
            _chapterMigrationPlan,
            translations,
            result))
        {
            return result;
        }

        result.Saved = true;

        if (_chapterMigrationPlan.AssignedIds > 0 ||
            removedChapterPageKeys > 0)
        {
            result.Issues.Add(new LocalizationIssue(
                false,
                "CHAPTER_MIGRATION_COMPLETED",
                $"Migración completada. " +
                $"IDs asignados: {_chapterMigrationPlan.AssignedIds}. " +
                $"Claves de páginas descartadas: {removedChapterPageKeys}. " +
                $"Traducciones nuevas: {result.AddedTranslations}."));
        }

        return result;
    }

    /// <summary>
    /// Elimina todas las claves de localización de páginas de capítulos.
    /// Devuelve un diccionario nuevo para no modificar el original.
    /// </summary>
    private static Dictionary<string, string> RemoveChapterPageTranslations(
        Dictionary<string, string> existingTranslations,
        out int removedKeys)
    {
        var translations = new Dictionary<string, string>(
            existingTranslations,
            StringComparer.Ordinal);

        List<string> chapterPageKeys = translations.Keys
            .Where(IsChapterPageLocalizationKey)
            .ToList();

        foreach (string key in chapterPageKeys)
            translations.Remove(key);

        removedKeys = chapterPageKeys.Count;

        return translations;
    }

    /// <summary>
    /// Reconstruye las claves de páginas de capítulos a partir del contenido
    /// extraído. Se utiliza durante la validación, sin guardar cambios.
    /// </summary>
    private void RegenerateChapterPageTranslations(
        Dictionary<string, string> translations)
    {
        foreach (LocalizationEntry entry in _entries)
        {
            if (!IsChapterPageLocalizationKey(entry.Key))
                continue;

            translations[entry.Key] = entry.SourceText;
        }
    }

    private static bool IsChapterPageLocalizationKey(string key)
    {
        return key.StartsWith("chapter.", StringComparison.Ordinal)
            && key.Contains(".page.", StringComparison.Ordinal);
    }

    private bool ExtractContent(LocalizationExportResult result)
    {
        _entries.Clear();
        _readIssues.Clear();

        if (!Directory.Exists(_projectFolder))
        {
            result.Issues.Add(new LocalizationIssue(
                true,
                "PROJECT_FOLDER_NOT_FOUND",
                $"No existe la carpeta del proyecto: {_projectFolder}"));

            return false;
        }

        CheckRequiredFolders();

        ExtractEvents();
        ExtractInterludes();
        ExtractChapters();
        ExtractAttributes();
        ExtractCharacters();
        ExtractRelationships();

        result.ExtractedEntries = _entries.Count;
        result.Issues.AddRange(_readIssues);
        result.Issues.AddRange(_validator.ValidateEntries(_entries));

        return !result.HasErrors;
    }

    private void CheckRequiredFolders()
    {
        foreach (string folderName in RequiredFolders)
        {
            string folderPath = Path.Combine(
                _projectFolder,
                folderName);

            if (!Directory.Exists(folderPath))
            {
                AddReadError(
                    "REQUIRED_FOLDER_NOT_FOUND",
                    $"Falta la carpeta de contenido '{folderName}'. " +
                    "Comprueba que la ruta del proyecto sea correcta.");
            }
        }
    }

    private bool TryLoadSpanish(
        LocalizationExportResult result,
        out Dictionary<string, string> translations)
    {
        translations = new Dictionary<string, string>(
            StringComparer.Ordinal);

        string languagePath;

        try
        {
            languagePath = _repository.GetLanguageFilePath(
                SpanishLanguageCode);
        }
        catch (Exception exception)
        {
            result.Issues.Add(new LocalizationIssue(
                true,
                "LANGUAGE_PATH_FAILED",
                $"No se pudo resolver la ruta de es.json: {exception.Message}"));

            return false;
        }

        if (!CheckDuplicateLanguageKeys(languagePath, result))
            return false;

        try
        {
            translations = _repository.LoadLanguage(
                SpanishLanguageCode);
        }
        catch (Exception exception)
        {
            result.Issues.Add(new LocalizationIssue(
                true,
                "LANGUAGE_READ_FAILED",
                $"No se pudo leer es.json: {exception.Message}"));

            return false;
        }

        return !result.HasErrors;
    }

    private bool CheckDuplicateLanguageKeys(
        string languagePath,
        LocalizationExportResult result)
    {
        if (!File.Exists(languagePath))
            return true;

        try
        {
            using JsonDocument document =
                JsonDocument.Parse(File.ReadAllText(languagePath));

            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                result.Issues.Add(new LocalizationIssue(
                    true,
                    "LANGUAGE_ROOT_INVALID",
                    "es.json debe contener un objeto JSON de claves y textos."));

                return false;
            }

            var seen = new HashSet<string>(StringComparer.Ordinal);
            bool hasDuplicates = false;

            foreach (JsonProperty property in
                     document.RootElement.EnumerateObject())
            {
                if (seen.Add(property.Name))
                    continue;

                hasDuplicates = true;

                result.Issues.Add(new LocalizationIssue(
                    true,
                    "DUPLICATE_TRANSLATION_KEY",
                    $"La clave '{property.Name}' aparece más de una vez en es.json."));
            }

            return !hasDuplicates;
        }
        catch (Exception exception)
        {
            result.Issues.Add(new LocalizationIssue(
                true,
                "LANGUAGE_JSON_INVALID",
                $"No se pudo analizar es.json: {exception.Message}"));

            return false;
        }
    }

    private void ExtractEvents()
    {
        foreach (EditorEventData data in
                 LoadFolder<EditorEventData>("events"))
        {
            if (string.IsNullOrWhiteSpace(data.Id))
            {
                AddReadError(
                    "EVENT_WITHOUT_ID",
                    "Hay un evento sin identificador.");
                continue;
            }

            string root = $"event.{data.Id}";

            Add(
                $"{root}.title",
                data.Title,
                $"{data.Id}: título");

            ExtractPages(
                data.Pages ?? new List<EditorPageData>(),
                root,
                $"{data.Id}: páginas");
        }
    }

    private void ExtractInterludes()
    {
        foreach (EditorInterludeData data in
                 LoadFolder<EditorInterludeData>("interludes"))
        {
            if (string.IsNullOrWhiteSpace(data.Id))
            {
                AddReadError(
                    "INTERLUDE_WITHOUT_ID",
                    "Hay un interludio sin identificador.");
                continue;
            }

            string root = $"interlude.{data.Id}";

            Add(
                $"{root}.title",
                data.Title,
                $"{data.Id}: título");

            foreach (EditorInterludePageData page in
                     data.Pages ?? new List<EditorInterludePageData>())
            {
                string? pageId = GetStableId(page.Id);

                if (pageId == null)
                {
                    AddReadError(
                        "INTERLUDE_PAGE_WITHOUT_ID",
                        $"Hay una página sin ID en el interludio '{data.Id}'.");
                    continue;
                }

                Add(
                    $"{root}.page.{pageId}.text",
                    page.Text,
                    $"{data.Id}, página {pageId}");
            }
        }
    }

    private void ExtractChapters()
    {
        foreach (ChapterDefinitionData data in
                 LoadFolder<ChapterDefinitionData>("chapters"))
        {
            if (string.IsNullOrWhiteSpace(data.Id))
            {
                AddReadError(
                    "CHAPTER_WITHOUT_ID",
                    "Hay un capítulo sin identificador.");
                continue;
            }

            string root = $"chapter.{data.Id}";

            Add(
                $"{root}.title",
                data.Title,
                $"{data.Id}: título");

            List<ChapterPageDefinitionData> pages =
                data.Pages ?? new List<ChapterPageDefinitionData>();

            if (_chapterMigrationPlan == null ||
                !_chapterMigrationPlan.PageIdsByChapter.TryGetValue(
                    data.Id,
                    out List<string>? plannedPageIds))
            {
                AddReadError(
                    "CHAPTER_PAGE_MIGRATION_PLAN_MISSING",
                    $"No existe un plan de IDs para el capítulo '{data.Id}'.");
                continue;
            }

            if (plannedPageIds.Count != pages.Count)
            {
                AddReadError(
                    "CHAPTER_PAGE_MIGRATION_COUNT_MISMATCH",
                    $"El número de páginas del capítulo '{data.Id}' " +
                    "no coincide con el plan de migración.");
                continue;
            }

            var seenPageIds = new HashSet<string>(
                StringComparer.Ordinal);

            for (int index = 0; index < pages.Count; index++)
            {
                ChapterPageDefinitionData page = pages[index];

                if (page == null)
                {
                    AddReadError(
                        "CHAPTER_PAGE_NULL",
                        $"Hay una página nula en el capítulo '{data.Id}'.");
                    continue;
                }

                // Usar los IDs preparados a partir del JSON original.
                // No utilizar los GUID temporales de la deserialización.
                page.Id = plannedPageIds[index];

                if (string.IsNullOrWhiteSpace(page.Id))
                {
                    AddReadError(
                        "CHAPTER_PAGE_WITHOUT_ID",
                        $"Hay una página sin ID en el capítulo '{data.Id}'.");
                    continue;
                }

                if (!seenPageIds.Add(page.Id))
                {
                    AddReadError(
                        "DUPLICATE_CHAPTER_PAGE_ID",
                        $"El ID '{page.Id}' está repetido en el capítulo '{data.Id}'.");
                    continue;
                }

                string pageRoot = $"{root}.page.{page.Id}";
                string location = $"{data.Id}, página {page.Id}";

                Add(
                    $"{pageRoot}.title",
                    page.Title,
                    $"{location}: título");

                Add(
                    $"{pageRoot}.text",
                    page.Text,
                    $"{location}: texto");

                Add(
                    $"{pageRoot}.button_text",
                    page.ButtonText,
                    $"{location}: botón");
            }
        }
    }

    private void ExtractAttributes()
    {
        foreach (AttributeDefinitionData data in
                 LoadFolder<AttributeDefinitionData>("attributes"))
        {
            if (string.IsNullOrWhiteSpace(data.Id))
            {
                AddReadError(
                    "ATTRIBUTE_WITHOUT_ID",
                    "Hay un atributo sin identificador.");
                continue;
            }

            string root = $"attribute.{data.Id}";

            Add($"{root}.display_name", data.DisplayName, $"{data.Id}: nombre");
            Add($"{root}.description", data.Description, $"{data.Id}: descripción");
            Add($"{root}.negative_name", data.NegativeName, $"{data.Id}: nombre negativo");
            Add($"{root}.low_name", data.LowName, $"{data.Id}: nombre bajo");
            Add($"{root}.neutral_name", data.NeutralName, $"{data.Id}: nombre neutral");
            Add($"{root}.high_name", data.HighName, $"{data.Id}: nombre alto");
            Add($"{root}.positive_name", data.PositiveName, $"{data.Id}: nombre positivo");
        }
    }

    private void ExtractCharacters()
    {
        foreach (EditorCharacterData data in
                 LoadFolder<EditorCharacterData>("characters"))
        {
            if (string.IsNullOrWhiteSpace(data.Id))
            {
                AddReadError(
                    "CHARACTER_WITHOUT_ID",
                    "Hay un personaje sin identificador.");
                continue;
            }

            Add(
                $"character.{data.Id}.name",
                data.Name,
                $"{data.Id}: nombre");
        }
    }

    private void ExtractRelationships()
    {
        string folder = Path.Combine(
            _projectFolder,
            "relationships");

        // Las relaciones son opcionales en algunos proyectos.
        if (!Directory.Exists(folder))
            return;

        string[] files;

        try
        {
            files = Directory.GetFiles(
                    folder,
                    "*.json",
                    SearchOption.TopDirectoryOnly)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
        }
        catch (Exception exception)
        {
            AddReadError(
                "RELATIONSHIP_FOLDER_READ_FAILED",
                $"No se pudo leer la carpeta de relaciones: {exception.Message}");
            return;
        }

        foreach (string file in files)
        {
            try
            {
                using JsonDocument document =
                    JsonDocument.Parse(File.ReadAllText(file));

                if (document.RootElement.ValueKind == JsonValueKind.Array)
                {
                    List<EditorRelationshipDefinitionData>? definitions =
                        JsonSerializer.Deserialize<
                            List<EditorRelationshipDefinitionData>>(
                            document.RootElement.GetRawText(),
                            JsonOptions);

                    if (definitions == null)
                    {
                        throw new InvalidDataException(
                            "La lista de relaciones no es válida.");
                    }

                    foreach (EditorRelationshipDefinitionData definition
                             in definitions)
                    {
                        if (definition != null)
                            ExtractRelationshipTitles(definition, file);
                    }
                }
                else if (document.RootElement.ValueKind == JsonValueKind.Object)
                {
                    EditorRelationshipDefinitionData? definition =
                        JsonSerializer.Deserialize<
                            EditorRelationshipDefinitionData>(
                            document.RootElement.GetRawText(),
                            JsonOptions);

                    if (definition == null)
                    {
                        throw new InvalidDataException(
                            "La definición de relación no es válida.");
                    }

                    ExtractRelationshipTitles(definition, file);
                }
                else
                {
                    throw new InvalidDataException(
                        "Se esperaba un objeto o una lista de relaciones.");
                }
            }
            catch (Exception exception)
            {
                AddReadError(
                    "RELATIONSHIP_READ_FAILED",
                    $"No se pudo procesar '{file}': {exception.Message}");
            }
        }
    }

    private void ExtractRelationshipTitles(
        EditorRelationshipDefinitionData data,
        string file)
    {
        if (string.IsNullOrWhiteSpace(data.CharacterId))
        {
            AddReadError(
                "RELATIONSHIP_WITHOUT_CHARACTER",
                $"Hay una relación sin CharacterId en '{file}'.");
            return;
        }

        List<string> titles = data.Titles ?? new List<string>();

        for (int index = 0; index < titles.Count; index++)
        {
            Add(
                $"relationship.{data.CharacterId}.title.{index}",
                titles[index],
                $"{Path.GetFileName(file)}: título {index}");
        }
    }

    private void ExtractPages(
        List<EditorPageData> pages,
        string parentKey,
        string location)
    {
        foreach (EditorPageData page in pages)
        {
            string? pageId = GetStableId(page.Id);

            if (pageId == null)
            {
                AddReadError(
                    "PAGE_WITHOUT_ID",
                    $"Hay una página sin ID en '{location}'.");
                continue;
            }

            string pageRoot = $"{parentKey}.page.{pageId}";
            string pageLocation = $"{location}, página {pageId}";

            Add(
                $"{pageRoot}.title",
                page.Title,
                $"{pageLocation}: título");

            Add(
                $"{pageRoot}.text",
                page.Text,
                $"{pageLocation}: texto");

            foreach (EditorDecisionData decision in
                     page.Decisions ?? new List<EditorDecisionData>())
            {
                string? decisionId = GetStableId(decision.Id);

                if (decisionId == null)
                {
                    AddReadError(
                        "DECISION_WITHOUT_ID",
                        $"Hay una decisión sin ID en '{pageLocation}'.");
                    continue;
                }

                string decisionRoot =
                    $"{pageRoot}.decision.{decisionId}";

                Add(
                    $"{decisionRoot}.text",
                    decision.Text,
                    $"{pageLocation}, decisión {decisionId}: texto");

                Add(
                    $"{decisionRoot}.description",
                    decision.Description,
                    $"{pageLocation}, decisión {decisionId}: descripción");

                ExtractPages(
                    decision.Pages ?? new List<EditorPageData>(),
                    decisionRoot,
                    $"{pageLocation}, decisión {decisionId}");
            }
        }
    }

    private List<T> LoadFolder<T>(string relativeFolder)
    {
        var results = new List<T>();
        string folder = Path.Combine(_projectFolder, relativeFolder);

        if (!Directory.Exists(folder))
            return results;

        string[] files;

        try
        {
            files = Directory.GetFiles(
                    folder,
                    "*.json",
                    SearchOption.TopDirectoryOnly)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
        }
        catch (Exception exception)
        {
            AddReadError(
                "FOLDER_READ_FAILED",
                $"No se pudo leer '{folder}': {exception.Message}");
            return results;
        }

        foreach (string file in files)
        {
            try
            {
                string json = File.ReadAllText(file);

                T? data = JsonSerializer.Deserialize<T>(
                    json,
                    JsonOptions);

                if (data == null)
                {
                    AddReadError(
                        "EMPTY_JSON",
                        $"El archivo '{file}' no contiene datos válidos.");
                    continue;
                }

                results.Add(data);
            }
            catch (Exception exception)
            {
                AddReadError(
                    "JSON_READ_FAILED",
                    $"No se pudo procesar '{file}': {exception.Message}");
            }
        }

        return results;
    }

    private void Add(
        string key,
        string? sourceText,
        string location)
    {
        if (string.IsNullOrWhiteSpace(sourceText))
            return;

        _entries.Add(
            new LocalizationEntry(key, sourceText, location));
    }

    private void AddReadError(string code, string message)
    {
        _readIssues.Add(
            new LocalizationIssue(true, code, message));
    }

    private static string? GetStableId(string? id)
    {
        return string.IsNullOrWhiteSpace(id) ? null : id;
    }

    private void AddChapterMigrationErrors(
        LocalizationExportResult result,
        ChapterPageIdMigrationPlan plan)
    {
        foreach (string error in plan.Errors)
        {
            result.Issues.Add(new LocalizationIssue(
                true,
                "CHAPTER_PAGE_ID_MIGRATION_FAILED",
                error));
        }
    }

    private bool TryCommitMigration(
        ChapterPageIdMigrationPlan chapterPlan,
        Dictionary<string, string> translations,
        LocalizationExportResult result)
    {
        string languagePath;

        try
        {
            languagePath = Path.GetFullPath(
                _repository.GetLanguageFilePath(SpanishLanguageCode));
        }
        catch (Exception exception)
        {
            result.Issues.Add(new LocalizationIssue(
                true,
                "LANGUAGE_PATH_FAILED",
                $"No se pudo resolver la ruta de es.json: {exception.Message}"));

            return false;
        }

        var chapterPaths = chapterPlan.UpdatedFiles.Keys
            .Select(Path.GetFullPath)
            .ToList();

        if (chapterPaths.Contains(
            languagePath,
            StringComparer.OrdinalIgnoreCase))
        {
            result.Issues.Add(new LocalizationIssue(
                true,
                "MIGRATION_PATH_COLLISION",
                "La ruta de es.json coincide con la de un archivo de capítulo."));

            return false;
        }

        var targets = new List<string>(chapterPaths)
        {
            languagePath
        };

        // Ruta original -> copia de seguridad.
        // null significa que el archivo no existía previamente.
        var backups = new Dictionary<string, string?>(
            StringComparer.OrdinalIgnoreCase);

        var temporaryFiles = new List<string>();

        // Respaldar todos los archivos antes de empezar a escribir.
        try
        {
            foreach (string target in targets)
            {
                if (!File.Exists(target))
                {
                    backups.Add(target, null);
                    continue;
                }

                string backupPath = CreateBackupPath(target);

                File.Copy(target, backupPath, false);
                backups.Add(target, backupPath);
            }
        }
        catch (Exception exception)
        {
            result.Issues.Add(new LocalizationIssue(
                true,
                "MIGRATION_BACKUP_FAILED",
                $"No se pudieron preparar todas las copias de seguridad: " +
                exception.Message));

            return false;
        }

        try
        {
            foreach (KeyValuePair<string, string> entry
                     in chapterPlan.UpdatedFiles)
            {
                string target = Path.GetFullPath(entry.Key);

                string tempPath = target + "." +
                                  Guid.NewGuid().ToString("N") +
                                  ".migration.tmp";

                temporaryFiles.Add(tempPath);

                File.WriteAllText(tempPath, entry.Value);
                File.Move(tempPath, target, true);
            }

            // Guardar es.json después de los capítulos.
            // Si falla, se intentará restaurar el estado anterior.
            _repository.SaveLanguage(
                SpanishLanguageCode,
                translations);

            return true;
        }
        catch (Exception exception)
        {
            var rollbackErrors = new List<string>();

            foreach (KeyValuePair<string, string?> backup in backups)
            {
                try
                {
                    if (backup.Value == null)
                    {
                        if (File.Exists(backup.Key))
                            File.Delete(backup.Key);
                    }
                    else
                    {
                        File.Copy(backup.Value, backup.Key, true);
                    }
                }
                catch (Exception rollbackException)
                {
                    rollbackErrors.Add(
                        $"{backup.Key}: {rollbackException.Message}");
                }
            }

            string message =
                $"Falló el guardado de la migración: {exception.Message}";

            if (rollbackErrors.Count > 0)
            {
                message +=
                    " También hubo errores al restaurar archivos: " +
                    string.Join(" | ", rollbackErrors);
            }

            result.Issues.Add(new LocalizationIssue(
                true,
                rollbackErrors.Count > 0
                    ? "MIGRATION_ROLLBACK_FAILED"
                    : "MIGRATION_SAVE_FAILED",
                message));

            return false;
        }
        finally
        {
            foreach (string tempPath in temporaryFiles)
            {
                try
                {
                    if (File.Exists(tempPath))
                        File.Delete(tempPath);
                }
                catch
                {
                    // No ocultar el resultado por un fallo de limpieza.
                }
            }
        }
    }

    private static string CreateBackupPath(string originalPath)
    {
        string backupPath;

        do
        {
            backupPath = originalPath + "." +
                         DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") +
                         "." +
                         Guid.NewGuid().ToString("N") +
                         ".bak";
        }
        while (File.Exists(backupPath));

        return backupPath;
    }
}
