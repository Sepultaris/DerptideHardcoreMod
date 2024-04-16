using ACE.Entity.Enum.Properties;
using ACE.Server.Entity;
using ACE.Server.WorldObjects;

namespace DerptideHardcore;

[HarmonyPatchCategory(nameof(DamageMod))]
internal class DamageMod
{
    #region Settings
    public static Settings Settings = new();
    static string settingsPath => Path.Combine(Mod.ModPath, "Settings.json");
    private FileInfo settingsInfo = new(settingsPath);
    #endregion

    #region Patch
    [HarmonyPostfix]
    [HarmonyPatch(typeof(DamageEvent), "DoCalculateDamage", new Type[] { typeof(Creature), typeof(Creature), typeof(WorldObject) })]
    public static void PostDoCalculateDamage(Creature attacker, Creature defender, WorldObject damageSource, ref DamageEvent __instance, ref float __result)
    {
        if (attacker is Player player && defender is Creature creature)
        {
            if (player.GetProperty((PropertyBool)31000) == null)
            {
                player.SetProperty((PropertyBool)31000, false);
                return;
            }
            if (player.GetProperty((PropertyBool)31000) != true)
                return;

            //player.SendMessage("You are in hardcore mode!", ACE.Entity.Enum.ChatMessageType.Broadcast);
            __result += 1 * Settings.HardcoreDamageBonus;
            return;
        }
        if (attacker is Creature creature1 && defender is Player player1)
        {
            if (player1.GetProperty((PropertyBool)31000) == null)
            {
                player1.SetProperty((PropertyBool)31000, false);
                return;
            }
            if (player1.GetProperty((PropertyBool)31000) != true)
                return;

            //player1.SendMessage("You are in hardcore mode!", ACE.Entity.Enum.ChatMessageType.Broadcast);
            __result -= 1 * Settings.HcMobDamagePenalty;
            return;
        }
    }

    #endregion
}

