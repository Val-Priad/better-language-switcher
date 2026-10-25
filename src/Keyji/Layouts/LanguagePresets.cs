namespace Keyji.Layouts;

/// <summary>
/// Известный профиль японского IME для проверки и специальных функций.
/// В стартовый набор и каталог добавления пресеты не подставляются.
/// </summary>
public static class LanguagePresets
{
    /// <summary>Microsoft Japanese IME (TIP). LangID 0x0411.</summary>
    public static readonly LanguageProfile Japanese = new(
        LangId: 0x0411,
        ProfileId: "{03B5835F-F03C-411B-9CE2-AA23E1171E36}{A76C93D9-5523-4E90-AAFA-4DB112F9AC76}",
        IsTip: true,
        DisplayName: "日本語 — Microsoft IME",
        // Windows кладёт ветку User Profile как нейтральный "ja" (не "ja-JP") —
        // verified write-экспериментом. BcpTag не входит в
        // install-строку, но join «установленное ↔ пресет» и показ тега сломались бы.
        BcpTag: "ja");

}
