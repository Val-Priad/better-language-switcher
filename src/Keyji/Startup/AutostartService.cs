using Microsoft.Win32;

namespace Keyji.Startup;

/// <summary>
/// Автозапуск через запись в HKCU\...\Run (unelevated, per-user).
/// v0.1 распространяется одним .exe без установщика — поэтому в UI обязательна
/// подпись-предупреждение: сначала выключить автозапуск, потом удалять .exe.
/// </summary>
public static class AutostartService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "BetLaSwi";
    private const string LegacyValueName = "Keyji";

    /// <summary>Путь к текущему .exe (для single-file — сам исполняемый файл).</summary>
    private static string ExecutablePath =>
        Environment.ProcessPath ?? throw new InvalidOperationException("Не удалось определить путь к процессу.");

    /// <summary>Включён ли автозапуск и указывает ли он на текущий .exe.</summary>
    public static bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath);
        // Match the whole command; a substring can hide extra startup commands.
        return key?.GetValue(ValueName) is string value
            && IsOwnedValue(value, ExecutablePath);
    }

    internal static bool IsOwnedValue(string value, string executablePath) =>
        string.Equals(value, $"\"{executablePath}\" {App.TrayArg}", StringComparison.OrdinalIgnoreCase);

    internal static bool IsLegacyOwnedValue(string value)
    {
        string suffix = $"\" {App.TrayArg}";
        if (!value.StartsWith('"') || !value.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            return false;

        string path = value[1..^suffix.Length];
        bool absolute = path.Length >= 3 && char.IsLetter(path[0]) &&
                        path[1] == ':' && path[2] == '\\';
        return absolute && !path.Contains('"') &&
               path.EndsWith("\\Keyji.exe", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Carry over an owned Keyji startup entry to the renamed executable.</summary>
    public static void MigrateLegacyEntry()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
        if (key is null)
            return;

        if (key.GetValue(LegacyValueName) is not string legacy ||
            !IsLegacyOwnedValue(legacy))
            return;

        object? current = key.GetValue(ValueName);
        if (current is not null &&
            (current is not string command || !IsOwnedValue(command, ExecutablePath)))
            return;

        if (current is null)
            key.SetValue(ValueName, $"\"{ExecutablePath}\" {App.TrayArg}");
        key.DeleteValue(LegacyValueName, throwOnMissingValue: false);
    }

    public static void Enable()
    {
        using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath);
        object? existing = key.GetValue(ValueName);
        if (existing is not null && existing is not string)
            throw new InvalidOperationException("The BetLaSwi startup entry has an unexpected type.");
        if (existing is string command && !IsOwnedValue(command, ExecutablePath))
            throw new InvalidOperationException("The BetLaSwi startup entry is owned by another command.");
        // Автозапуск при входе — свёрнутым в трей, без окна онбординга.
        key.SetValue(ValueName, $"\"{ExecutablePath}\" {App.TrayArg}");
    }

    public static void Disable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
        if (key?.GetValue(ValueName) is string value && IsOwnedValue(value, ExecutablePath))
            key.DeleteValue(ValueName, throwOnMissingValue: false);
    }
}
