using Keyji.Layouts;
using Keyji.Native;
using Keyji.Settings;
using Keyji.Startup;

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

var japanese = LanguagePresets.Japanese;
Check(japanese.TryGetInstallString(out _), "Valid Japanese TIP rejected");
Check(new LanguageProfile(0x0409, "00000409", false, "English", "en-US")
    .TryGetInstallString(out _), "Valid KLID rejected");
foreach (string bad in new[] { "00000409;0419:00000419", "0000040G", "00000409\\other", "", "000004090" })
{
    var profile = new LanguageProfile(0x0409, bad, false, "bad", "en-US");
    Check(!profile.TryGetInstallString(out _), $"Malformed KLID accepted: {bad}");
    Check(!new LayoutService().EnableLanguage(profile), "Malformed profile reached native install");
    Check(!new LayoutService().DisableLanguage(profile), "Malformed profile reached native uninstall");
}
Check(!new LanguageProfile(0, "00000409", false, "bad", "en-US")
    .TryGetInstallString(out _), "Zero language ID accepted");
foreach (string bad in new[] { japanese.ProfileId + ";0419:00000419", japanese.ProfileId[..75],
    japanese.ProfileId.Replace('B', 'Z') })
    Check(!new LanguageProfile(0x0411, bad, true, "bad", "ja")
        .TryGetInstallString(out _), "Malformed TIP accepted");
Check(NativeMethods.Installs == 0, "Invalid identifier reached native API");

var service = new LayoutService();
service.SwitchTo(japanese, new IntPtr(1), forceHiragana: false);
Check(NativeMethods.ImeSets == 0, "Disabled hiragana preference changed IME");
service.SwitchTo(japanese, new IntPtr(1), forceHiragana: true);
Check(NativeMethods.ImeSets == 2, "Enabled hiragana preference did not set IME mode");

string folder = Path.Combine(Path.GetTempPath(), "keyji-security-tests-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(folder);
try
{
    string path = Path.Combine(folder, "settings.json");
    var settings = KeyjiSettings.CreateDefault();
    Check(settings.ManagedLanguages.Count == 0 && settings.PermanentLanguages.Count == 0 &&
          settings.PinnedKey is null, "New settings contain a preset language");
    settings.ManagedLanguages.Add(japanese);
    SettingsService.SaveTo(path, settings);
    settings.ForceHiraganaOnJapanese = false;
    SettingsService.SaveTo(path, settings);
    Check(File.Exists(path + ".bak"), "Backup was not created");
    File.WriteAllText(path, "{ broken JSON");
    var recovered = SettingsService.LoadFrom(path);
    Check(recovered.ForceHiraganaOnJapanese, "Last good backup was not recovered");
    Check(SettingsService.LoadFrom(path).ForceHiraganaOnJapanese, "Primary was not repaired");
    File.WriteAllText(path, "{ broken again");
    Check(SettingsService.LoadFrom(path).ForceHiraganaOnJapanese, "Recovery overwrote the backup");
    settings.ManagedLanguages[0] = new LanguageProfile(0x0411, "bad;value", true, "bad", "ja");
    bool rejected = false;
    try { SettingsService.SaveTo(path, settings); }
    catch (ArgumentException) { rejected = true; }
    Check(rejected, "Invalid profile was saved");
    settings.ManagedLanguages[0] = japanese;
    settings.ManagedLanguages.Clear();
    settings.PinnedKey = null;
    SettingsService.SaveTo(path, settings);
    Check(SettingsService.LoadFrom(path).ManagedLanguages.Count == 0, "Empty managed set rejected");

    string migratedPath = Path.Combine(folder, "new", "settings.json");
    var migrated = SettingsService.LoadFromPaths(migratedPath, path);
    Check(migrated.ManagedLanguages.Count == 0 && File.Exists(migratedPath),
        "Legacy settings were not migrated");
    Check(File.Exists(path), "Migration removed the legacy settings");
}
finally
{
    Directory.Delete(folder, recursive: true);
}

Check(AutostartService.IsOwnedValue("\"C:\\Keyji.exe\" --tray", "C:\\Keyji.exe"), "Owned Run entry rejected");
Check(!AutostartService.IsOwnedValue("\"C:\\Keyji.exe\" --tray & evil.exe", "C:\\Keyji.exe"),
    "Extra startup command accepted");
Check(AutostartService.IsLegacyOwnedValue("\"C:\\Apps\\Keyji.exe\" --tray"),
    "Owned legacy Run entry rejected");
Check(!AutostartService.IsLegacyOwnedValue("\"C:\\Apps\\Keyji.exe\" --tray & evil.exe"),
    "Injected legacy Run entry accepted");
Console.WriteLine("Security behavior tests passed.");
