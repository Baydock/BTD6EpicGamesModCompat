using BTD6EpicGamesModCompat;
using MelonLoader;
using System;
using System.IO;

[assembly: MelonInfo(typeof(Plugin), "BTD6 Epic Games Mod Compat", "1.0.5", "Baydock")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6-Epic")]

namespace BTD6EpicGamesModCompat;

public sealed class Plugin : MelonPlugin {
    private const string EOSBypasserModPath = "Mods/BTD6EOSBypasser.dll";
    private const string EOSBypasserResourcePath = "BTD6EOSBypasser.dll";
    public static MelonLogger.Instance Logger { get; private set; }

    // Runs before crash caused by EOSSDK thankfully
    public override void OnPreInitialization() {
        Logger = LoggerInstance;

        // Avoid crash
        EOSSDK.Remove();

        // Not fully tested to work for all close cases
        AppDomain.CurrentDomain.ProcessExit += (s, e) => EOSSDK.Restore();
        AppDomain.CurrentDomain.UnhandledException += (s, e) => EOSSDK.Restore();
    }

    public override void OnApplicationQuit() {
        EOSSDK.Restore();
    }

    public override void OnPreModsLoaded() {
        // Regenerate BTD6EOSBypasser mod
        File.WriteAllBytes(EOSBypasserModPath, Resources.GetResource(EOSBypasserResourcePath));

        // Retarget Mods
        BTD6Retargeter.Retarget();
    }
}
