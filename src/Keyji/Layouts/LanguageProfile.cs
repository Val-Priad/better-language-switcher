namespace Keyji.Layouts;

/// <summary>
/// Язык/раскладка, которым Keyji управляет в пользовательском списке.
/// Несёт идентификатор единообразно для обычной раскладки (KLID) и для TIP/IME
/// ({CLSID}{GUID}) — именно эта строка уходит в InstallLayoutOrTip.
/// </summary>
/// <param name="LangId">Language ID, напр. 0x0411 для японского.</param>
/// <param name="ProfileId">
/// Часть строки после "LangID:": KLID ("00000409") для обычной раскладки
/// либо "{CLSID}{GUID}" для TIP.
/// </param>
/// <param name="IsTip">true — text input processor (IME), false — обычная раскладка.</param>
/// <param name="DisplayName">Человекочитаемое имя для UI/трея.</param>
/// <param name="BcpTag">BCP-47 тег, напр. "ja-JP".</param>
public sealed record LanguageProfile(
    ushort LangId,
    string ProfileId,
    bool IsTip,
    string DisplayName,
    string BcpTag)
{
    /// <summary>Only one well-formed KLID or TIP may be passed to InstallLayoutOrTip.</summary>
    public bool TryGetInstallString(out string installString)
    {
        installString = string.Empty;
        if (LangId == 0 || ProfileId is null)
            return false;

        if (IsTip)
        {
            if (ProfileId.Length != 76 ||
                !Guid.TryParseExact(ProfileId[..38], "B", out _) ||
                !Guid.TryParseExact(ProfileId[38..], "B", out _))
                return false;
        }
        else if (ProfileId.Length != 8 || !ProfileId.All(Uri.IsHexDigit))
        {
            return false;
        }

        installString = ToInstallString();
        return true;
    }

    /// <summary>Строка для InstallLayoutOrTip: "LangID:ProfileId" (langid в hex, 4 знака).</summary>
    public string ToInstallString() => $"{LangId:x4}:{ProfileId}";
}
