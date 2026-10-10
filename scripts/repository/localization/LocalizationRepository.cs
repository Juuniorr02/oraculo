
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public sealed class LocalizationRepository
{
    private readonly string _projectFolder;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public LocalizationRepository(string projectFolder)
    {
        if (string.IsNullOrWhiteSpace(projectFolder))
            throw new ArgumentException(
                "La carpeta del proyecto no puede estar vacía.",
                nameof(projectFolder));

        _projectFolder = Path.GetFullPath(projectFolder);
    }

    public string LocalizationFolder =>
        Path.Combine(_projectFolder, "localization");

    public string GetLanguageFilePath(string languageCode)
    {
        ValidateLanguageCode(languageCode);

        return Path.Combine(
            LocalizationFolder,
            $"{languageCode}.json");
    }

    public Dictionary<string, string> LoadLanguage(
        string languageCode)
    {
        string path = GetLanguageFilePath(languageCode);

        if (!File.Exists(path))
            return new Dictionary<string, string>(
                StringComparer.Ordinal);

        string json = File.ReadAllText(path);

        Dictionary<string, string>? translations =
            JsonSerializer.Deserialize<Dictionary<string, string>>(
                json,
                JsonOptions);

        if (translations == null)
        {
            throw new InvalidDataException(
                $"El archivo de idioma '{path}' no contiene " +
                "un diccionario válido de traducciones.");
        }

        return new Dictionary<string, string>(
            translations,
            StringComparer.Ordinal);
    }

    public void SaveLanguage(
        string languageCode,
        IReadOnlyDictionary<string, string> translations)
    {
        if (translations == null)
            throw new ArgumentNullException(nameof(translations));

        string path = GetLanguageFilePath(languageCode);

        Directory.CreateDirectory(LocalizationFolder);

        string json = JsonSerializer.Serialize(
            translations,
            JsonOptions);

        // No se modifica ningún JSON del contenido del juego.
        // Solo se escribe el archivo de idioma solicitado.
        File.WriteAllText(path, json);
    }

    public bool LanguageExists(string languageCode)
    {
        return File.Exists(GetLanguageFilePath(languageCode));
    }

    private static void ValidateLanguageCode(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
            throw new ArgumentException(
                "El código de idioma no puede estar vacío.",
                nameof(languageCode));

        foreach (char character in languageCode)
        {
            bool valid =
                character >= 'a' && character <= 'z' ||
                character >= 'A' && character <= 'Z' ||
                character >= '0' && character <= '9' ||
                character == '-' ||
                character == '_';

            if (!valid)
            {
                throw new ArgumentException(
                    "El código de idioma solo puede contener " +
                    "letras, números, guiones y guiones bajos.",
                    nameof(languageCode));
            }
        }
    }
}
