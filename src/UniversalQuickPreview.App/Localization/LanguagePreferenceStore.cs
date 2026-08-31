using System.Text.Json;
using System.IO;

namespace UniversalQuickPreview.App.Localization;

internal sealed class LanguagePreferenceStore
{
    private readonly string _settingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "UniversalQuickPreview",
        "settings.json");

    internal string? ReadLanguage()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                return null;
            }

            using var stream = File.OpenRead(_settingsPath);
            return JsonSerializer.Deserialize<UserPreferences>(stream)?.Language;
        }
        catch (IOException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    internal void WriteLanguage(string language)
    {
        try
        {
            var directory = Path.GetDirectoryName(_settingsPath)!;
            Directory.CreateDirectory(directory);
            using var stream = File.Create(_settingsPath);
            JsonSerializer.Serialize(stream, new UserPreferences(language));
        }
        catch (IOException)
        {
            // A read-only settings location should not prevent previewing files.
        }
        catch (UnauthorizedAccessException)
        {
            // A read-only settings location should not prevent previewing files.
        }
    }

    private sealed record UserPreferences(string Language);
}
