using System.Diagnostics;
using ACE.Common;
using ACE.Entity.Enum.Properties;
using ACE.Server.Entity;
using ACE.Server.Factories;
using ACE.Server.WorldObjects;

namespace DerptideHardcore;

[HarmonyPatchCategory(nameof(LootPatch))]
public class LootPatch
{
    #region Settings
    public static Settings Settings = new();
    static string settingsPath => Path.Combine(Mod.ModPath, "Settings.json");
    private FileInfo settingsInfo = new(settingsPath);
    #endregion

    #region Patch
    // Patch for Creature.GenerateTreasure. Checks if the killer is a player and if the player has the property 31000.
    // And rolls to drop a token if they do.
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Creature), "GenerateTreasure", new Type[] { typeof(DamageHistoryInfo), typeof(Corpse) })]
    public static void PostGenerateTreasure(DamageHistoryInfo killer, Corpse corpse, Creature __instance, List<WorldObject> __result)
    {
        Debugger.Break();
        if (killer is null || killer.TryGetPetOwnerOrAttacker() is not Player player)
            return;
        if (player.GetProperty((PropertyBool)31000) == null)
        {
            player.SetProperty((PropertyBool)31000, false);
            return;
        }
        if (player.GetProperty((PropertyBool)31000) != true)
            return;

        var roll = ThreadSafeRandom.Next(0.0f, 1.0f);

        if (Settings.TokenDrop && player.GetProperty((PropertyBool)31000) == true && roll < Settings.TokenDropChance)
        {
            var tokenCount = corpse.GetInventoryItemsOfWCID(Settings.TokenWCID);
            var token = WorldObjectFactory.CreateNewWorldObject(Settings.TokenWCID);

            if (tokenCount.Count >= 1)
                return;
        
            corpse.TryAddToInventory(token);
            player.SendMessage($"{corpse.Name} Dropped a token.", ACE.Entity.Enum.ChatMessageType.Broadcast);
        }
    }

    #endregion
}
