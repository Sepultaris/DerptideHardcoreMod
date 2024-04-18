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
    // Patch for DamageEvent.DoCalculateDamage. Checks if the attacker is a player and if the player has the property 31000.
    // And applies a damage bonus if they do.
    // It then Checks if the attacker is a creature and if the player has the property 31000.
    // And applies a damage boot to the mob if they do.
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

            if (player.GetProperty((PropertyBool)31000) == true)
            {
                //player.SendMessage("You are in hardcore mode!", ACE.Entity.Enum.ChatMessageType.Broadcast);
                var moddedDamage = __result + (__result * Settings.HardcoreDamageBonus);

                __result += 1 * moddedDamage;
                return;
            }
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

            if (player1.GetProperty((PropertyBool)31000) == true)
            {
                //player1.SendMessage("You are in hardcore mode!", ACE.Entity.Enum.ChatMessageType.Broadcast);
                var moddedDamage = __result + (__result * Settings.HcMobDamageBoost);

                __result += 1 * moddedDamage;
                return;
            }
        }
    }

    #endregion
}

