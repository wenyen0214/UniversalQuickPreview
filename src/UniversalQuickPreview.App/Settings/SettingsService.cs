using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

namespace UniversalQuickPreview.App.Settings;

public sealed class SettingsService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _settingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "UniversalQuickPreview",
        "settings.json");

    public SettingsService()
    {
        Current = Load();
    }

    public event EventHandler? Changed;

    public UserSettings Current { get; private set; }

    public void Update(Func<UserSettings, UserSettings> update)
    {
        ArgumentNullException.ThrowIfNull(update);
        var updated = update(Current);
        if (updated == Current)
        {
            return;
        }

        Current = updated;
        Save();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private UserSettings Load()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                return new UserSettings();
            }

            using var stream = File.OpenRead(_settingsPath);
            return JsonSerializer.Deserialize<UserSettings>(stream, SerializerOptions) ?? new UserSettings();
        }
        catch (Exception exception) when (exception is IOException or JsonException or UnauthorizedAccessException)
        {
            return new UserSettings();
        }
    }

    private void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
            var temporaryPath = _settingsPath + ".tmp";
            using (var stream = File.Create(temporaryPath))
            {
                JsonSerializer.Serialize(stream, Current, SerializerOptions);
            }

            File.Move(temporaryPath, _settingsPath, overwrite: true);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            // Settings persistence must never prevent local previewing.
        }
    }
}
