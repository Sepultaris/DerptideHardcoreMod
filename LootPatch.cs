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
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Creature), "GenerateTreasure", new Type[] { typeof(DamageHistoryInfo), typeof(Corpse) })]
    public static void PostGenerateTreasure(DamageHistoryInfo killer, Corpse corpse, Creature __instance, List<WorldObject> __result)
    {
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

        var tokenCount = corpse.GetInventoryItemsOfWCID(Settings.TokenWCID);
        var token = WorldObjectFactory.CreateNewWorldObject(Settings.TokenWCID);

        if (tokenCount.Count >= 1)
            return;
        if (roll > Settings.TokenDropChance)
            return;

        if (Settings.TokenDrop)
        {
            corpse.TryAddToInventory(token);
            player.SendMessage($"{corpse.Name} Dropped a token.", ACE.Entity.Enum.ChatMessageType.Broadcast);
        }
    }

    #endregion
}
