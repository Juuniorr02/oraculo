
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class LocalizationEntry
{
    public string Key { get; }
    public string SourceText { get; }
    public string Location { get; }

    public LocalizationEntry(
        string key,
        string sourceText,
        string location)
    {
        Key = key;
        SourceText = sourceText;
        Location = location;
    }
}

public sealed class LocalizationIssue
{
    public bool IsError { get; }
    public string Code { get; }
    public string Message { get; }

    public LocalizationIssue(
        bool isError,
        string code,
        string message)
    {
        IsError = isError;
        Code = code;
        Message = message;
    }

    public override string ToString()
    {
        string severity = IsError ? "ERROR" : "AVISO";
        return $"[{severity}] [{Code}] {Message}";
    }
}

public sealed class LocalizationValidator
{
    public List<LocalizationIssue> ValidateEntries(
        IReadOnlyCollection<LocalizationEntry> entries)
    {
        var issues = new List<LocalizationIssue>();

        foreach (LocalizationEntry entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Key))
            {
                issues.Add(new LocalizationIssue(
                    true,
                    "EMPTY_KEY",
                    $"Hay una entrada sin clave. Origen: {entry.Location}"));
            }

            if (string.IsNullOrWhiteSpace(entry.SourceText))
            {
                issues.Add(new LocalizationIssue(
                    false,
                    "EMPTY_SOURCE",
                    $"La entrada '{entry.Key}' no tiene texto de origen."));
            }
        }

        var duplicateGroups = entries
            .Where(entry => !string.IsNullOrWhiteSpace(entry.Key))
            .GroupBy(entry => entry.Key, StringComparer.Ordinal)
            .Where(group => group.Count() > 1);

        foreach (var group in duplicateGroups)
        {
            string locations = string.Join(
                ", ",
                group.Select(entry => entry.Location));

            issues.Add(new LocalizationIssue(
                true,
                "DUPLICATE_KEY",
                $"La clave '{group.Key}' aparece varias veces: {locations}"));
        }

        return issues;
    }

    public List<LocalizationIssue> ValidateTranslations(
        IReadOnlyCollection<LocalizationEntry> entries,
        IReadOnlyDictionary<string, string> translations)
    {
        var issues = new List<LocalizationIssue>();

        var currentKeys = new HashSet<string>(
            entries.Select(entry => entry.Key),
            StringComparer.Ordinal);

        foreach (LocalizationEntry entry in entries)
        {
            if (!translations.TryGetValue(
                    entry.Key,
                    out string? translation))
            {
                issues.Add(new LocalizationIssue(
                    true,
                    "MISSING_TRANSLATION",
                    $"Falta la traducción de '{entry.Key}'."));

                continue;
            }

            if (string.IsNullOrWhiteSpace(translation))
            {
                issues.Add(new LocalizationIssue(
                    false,
                    "EMPTY_TRANSLATION",
                    $"La traducción de '{entry.Key}' está vacía."));
            }
        }

        foreach (string key in translations.Keys)
        {
            if (!currentKeys.Contains(key))
            {
                issues.Add(new LocalizationIssue(
                    false,
                    "OBSOLETE_KEY",
                    $"La clave '{key}' no aparece en el contenido actual. " +
                    "Se conserva para evitar perder traducciones."));
            }
        }

        return issues;
    }
}
