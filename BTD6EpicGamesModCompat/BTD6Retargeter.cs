using MelonLoader;
using MelonLoader.Utils;
using System;
using System.IO;
using System.Linq;

namespace BTD6EpicGamesModCompat;

internal class BTD6Retargeter {
    // Takes all mods that target BloonsTD6 and retarget them to BloonsTD6-Epic if they don't already
    public static void Retarget() {
        Plugin.Logger.WriteSpacer();
        Plugin.Logger.Msg($"Loading mods from {MelonEnvironment.ModsDirectory}...");
        Plugin.Logger.Msg(ConsoleColor.Magenta, "------------------------------");

        // Load all mod assemblies from file
        MelonAssembly[] modAssemblies = Directory.GetFiles(MelonEnvironment.ModsDirectory).Select(modFile => {
            if (!Path.HasExtension(modFile) || !Path.GetExtension(modFile).Equals(".dll"))
                return null;

            // LoadMelonAssembly already error checks
            return MelonAssembly.LoadMelonAssembly(modFile, false);
        }).Where(melon => melon is not null) // Remove all null assemblies
        .OrderBy(melon => MelonUtils.PullAttributeFromAssembly<MelonPriorityAttribute>(melon.Assembly)?.Priority ?? 0) // Sort by priority
        .ToArray();

        Plugin.Logger.WriteSpacer();
        Plugin.Logger.Msg("Retargeting mods...");
        Plugin.Logger.Msg(ConsoleColor.Magenta, "------------------------------");

        // Iterate over all assemblies
        foreach (MelonAssembly melonAssembly in modAssemblies) {
            melonAssembly.LoadMelons();
            // Iterate over all melons in each mod assembly
            foreach (MelonBase mod in melonAssembly.LoadedMelons) {
                // Probably will never happen, but nice to check for
                if (mod is null)
                    continue;

                // If the mod doesn't target a game, skip
                if (mod.Games.Length < 1)
                    continue;

                // If the mod targets the epic version already or doesn't target btd6 to begin with, skip
                if (!TargetsBloonsTD6(mod) || TargetsBloonsTD6Epic(mod))
                    continue;

                // Replaces BloonsTD6 with BloonsTD6-Epic
                var targetIndex = BloonsTd6TargetIndex(mod);
                mod.Games[targetIndex] = new MelonGameAttribute("Ninja Kiwi", "BloonsTD6-Epic");

                Plugin.Logger.Msg(
                    $"Retargeted [{mod.Info.Name} v{mod.Info.Version} by {mod.Info.Author}] to BloonsTD6-Epic");
            }
            // Reload the assembly
            melonAssembly.UnregisterMelons(silent: true);
            melonAssembly.LoadMelons();
        }
    }

    // Tests if the mod targets BloonsTD6
    private static bool TargetsBloonsTD6(MelonBase mod) {
        return mod.Games.Any(game => game.Universal || IsTargetTo(game, "Ninja Kiwi", "BloonsTD6"));
    }

    // Tests if the mod targets BloonsTD6-Epic
    private static bool TargetsBloonsTD6Epic(MelonBase mod) {
        return mod.Games.Any(game => game.Universal || IsTargetTo(game, "Ninja Kiwi", "BloonsTD6-Epic"));
    }

    // Finds the index that the mod targets BloonsTD6
    private static int BloonsTd6TargetIndex(MelonBase mod) {
        return Array.FindIndex(mod.Games, game => IsTargetTo(game, "Ninja Kiwi", "BloonsTD6"));
    }

    // Determines if the target is the the given dev and game name
    private static bool IsTargetTo(MelonGameAttribute game, string dev, string name) {
        return game.Developer.Equals(dev) && game.Name.Equals(name);
    }
}
