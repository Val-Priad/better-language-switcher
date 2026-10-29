<div align="center">

<img src="img/betlaswi.svg" alt="BetLaSwi" width="200" />

**A tiny bird. A world of languages.**

BetLaSwi keeps a rarely-needed layout one click away — and out of your way the rest of the time.

<a href="#"><img src="https://img.shields.io/badge/platform-Windows-0078D6?style=flat&logo=windows&logoColor=white" alt="Windows"></a>
<a href="https://github.com/Wel-lz/Keyji/stargazers"><img src="https://img.shields.io/github/stars/Wel-lz/Keyji?style=flat&color=yellow&cacheSeconds=300" alt="Stars"></a>
<a href="LICENSE"><img src="https://img.shields.io/badge/license-MIT-brightgreen?style=flat" alt="License: MIT"></a>

**Personal source build** · Windows x64 · .NET 10 LTS · [build instructions](#build-from-source)

</div>

---

## The problem

Windows gives you two awkward options for an input profile you only need *occasionally* — a learner's Japanese, an emigrant's Cyrillic, or an Arabic script you touch once a week:

- **Keep it installed** — and it sits forever in your `Win+Space` cycle, so you tab past it dozens of times a day to reach the two layouts you actually use.
- **Remove its input profile** — and every time you need it, you have to enable it again through Windows Settings.

Neither fits a layout you need rarely but genuinely. That's the gap BetLaSwi fills.

## How it works

BetLaSwi lives in the system tray. Open its panel to choose which installed input profiles it may manage, then toggle those profiles in or out of the `Win+Space` cycle. You can pin one managed profile for a left-click tray toggle.

- **Tray controls** — open the panel to manage profiles; left-click the tray icon to toggle a pinned profile. The icon shows that profile's enabled state.
- **Clean start** — on first run, profiles already installed in your Windows user profile are listed as permanent, so BetLaSwi does not disable them until you choose to manage them. No language is added or pinned automatically.
- **Installed profiles only** — the Add list contains profiles already installed for your Windows user and registered on the machine. Install a new language through Windows Settings first, then reopen BetLaSwi.
- **Native switching** — uses Windows APIs and TSF to enable, disable, and switch input profiles.
- **Optional hiragana mode** — when Japanese IME is active, BetLaSwi can request hiragana mode. This preference is on by default and can be changed in the Japanese profile's menu. It does not install Japanese.
- **Single-file publish** — build a self-contained Windows x64 `.exe` from this source; no separate .NET runtime is required on the target PC.

BetLaSwi changes the enabled input profiles in your Windows user session. Removing a profile from the **BetLaSwi set** stops BetLaSwi managing it; it does not uninstall the language from Windows. Use Windows Settings to remove a language pack.

The clean-start defaults apply only when BetLaSwi has no saved settings. Existing settings in `%APPDATA%\Keyji\settings.json` (or its `.bak` recovery file) are migrated to `%APPDATA%\BetLaSwi\settings.json` on first launch.

## Who it's for

- **Language learners** who need a script for study but not for daily typing.
- **Multilingual users** who switch scripts occasionally, not constantly.
- **Anyone** tired of a rarely-used layout crowding their `Win+Space` cycle.

## What BetLaSwi is *not*

`Win+Space` cycles through enabled input profiles. BetLaSwi lets you choose which installed profiles are enabled. It does not download language packs.

## How it compares

| | BetLaSwi | `Win+Space` | Windows Settings | PowerToys |
|---|:---:|:---:|:---:|:---:|
| Toggle a rare layout in/out on demand | ✅ | ❌ | 🐢 manual | ❌ |
| One-click toggle | ✅ | ✅ | ❌ | — |
| Keeps rare layouts out of the daily cycle | ✅ | ❌ | ❌ | ❌ |
| Self-contained single-file build | ✅ | — | — | — |
| Purpose-built, no bloat | ✅ | — | — | ❌ |

## Build from source

BetLaSwi is a C# WPF application. On Windows x64, install the .NET SDK version specified in [global.json](global.json), then run these commands from the repository root:

```powershell
dotnet restore tests/Keyji.SecurityTests/Keyji.SecurityTests.csproj --locked-mode
dotnet run --project tests/Keyji.SecurityTests/Keyji.SecurityTests.csproj --no-restore
dotnet restore src/Keyji/BetLaSwi.csproj -p:Configuration=Release -p:RuntimeIdentifier=win-x64 --locked-mode
dotnet publish src/Keyji/BetLaSwi.csproj -c Release -r win-x64 --no-restore -o build/win-x64
```

The executable is `build/win-x64/BetLaSwi.exe`. Release builds pin the .NET runtime version in [BetLaSwi.csproj](src/Keyji/BetLaSwi.csproj) and use the committed NuGet lock files. Review and update both pins when applying SDK, runtime, or package security updates. The automated tests cover selected settings and switching logic; they do not exercise the WPF UI or Windows input APIs on a live PC.

## License

[MIT](LICENSE) — minimal friction for users and contributors.
