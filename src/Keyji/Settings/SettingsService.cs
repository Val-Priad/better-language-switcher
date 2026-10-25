using System.IO;
using System.Text;
using System.Text.Json;
using Keyji.Layouts;

namespace Keyji.Settings;

/// <summary>
/// Stores settings in %AppData%\Keyji\settings.json with a last-good backup.
/// </summary>
public static class SettingsService
{
    private static readonly string DirPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Keyji");

    private static readonly string FilePath = Path.Combine(DirPath, "settings.json");

    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static bool Exists() => File.Exists(FilePath) || File.Exists(FilePath + ".bak");

    public static KeyjiSettings Load() => LoadFrom(FilePath);

    internal static KeyjiSettings LoadFrom(string path)
    {
        if (TryRead(path, out var settings))
            return settings!;

        if (TryRead(path + ".bak", out settings))
        {
            // Restore without replacing the backup with a broken primary.
            try { WriteAtomic(path, JsonSerializer.Serialize(settings, Options), keepPrevious: false); }
            catch (IOException) { /* The backup remains available on the next launch. */ }
            catch (UnauthorizedAccessException) { /* Read-only recovery is still useful. */ }
            return settings!;
        }

        return KeyjiSettings.CreateDefault();
    }

    private static bool TryRead(string path, out KeyjiSettings? settings)
    {
        settings = null;
        try
        {
            if (!File.Exists(path))
                return false;
            settings = JsonSerializer.Deserialize<KeyjiSettings>(File.ReadAllText(path), Options);
            return settings is not null && Valid(settings);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return false;
        }
    }

    private static bool Valid(KeyjiSettings settings) =>
        settings.ManagedLanguages is not null && settings.PermanentLanguages is not null &&
        settings.ManagedLanguages.All(ValidProfile) && settings.PermanentLanguages.All(ValidProfile);

    private static bool ValidProfile(LanguageProfile? profile) =>
        profile is not null && profile.TryGetInstallString(out _) &&
        !string.IsNullOrWhiteSpace(profile.DisplayName) &&
        !string.IsNullOrWhiteSpace(profile.BcpTag);

    public static void Save(KeyjiSettings settings) => SaveTo(FilePath, settings);

    internal static void SaveTo(string path, KeyjiSettings settings)
    {
        if (!Valid(settings))
            throw new ArgumentException("Settings contain an invalid language profile.", nameof(settings));
        WriteAtomic(path, JsonSerializer.Serialize(settings, Options), keepPrevious: true);
    }

    private static void WriteAtomic(string path, string json, bool keepPrevious)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
            {
                writer.Write(json);
                writer.Flush();
                stream.Flush(flushToDisk: true);
            }

            if (File.Exists(path))
                File.Replace(temporary, path, keepPrevious ? path + ".bak" : null);
            else
                File.Move(temporary, path);
        }
        finally
        {
            if (File.Exists(temporary))
                File.Delete(temporary);
        }
    }
}
