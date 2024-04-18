using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACE.Common;
using ACE.Database.Models.Shard;
using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;
using ACE.Server.Entity;
using ACE.Server.Managers;
using ACE.Server.Network;
using ACE.Server.WorldObjects;

namespace DerptideHardcore;

[HarmonyPatchCategory(nameof(DeathPatch))]
internal class DeathPatch
{
    #region Settings
    public static Settings Settings = new();
    static string settingsPath => Path.Combine(Mod.ModPath, "Settings.json");
    private FileInfo settingsInfo = new(settingsPath);
    #endregion

    #region Patch
    // Patch for Player.OnDeath. Checks if the player has the property 31000 and if they do, sends a message when they die.
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Player), nameof(Player.OnDeath), new Type[] { typeof(DamageHistoryInfo), typeof(DamageType), typeof(bool) })]
    public static void PostOnDeath(DamageHistoryInfo lastDamager, DamageType damageType, bool criticalHit, ref Player __instance, ref DeathMessage __result)
    {
        if (__instance.GetProperty((PropertyBool)31000) == null)
        {
            __instance.SetProperty((PropertyBool)31000, false);
            return;
        }
        if (__instance.GetProperty((PropertyBool)31000) != true)
            return;
        if (__instance.GetProperty((PropertyBool)31000) == true)
        {
            __instance.SendMessage("You have died in hardcore mode!", ChatMessageType.Broadcast);
            
        }
    }

    // Patch for Player.Die. Checks if the player has the property 31000 and if they do, deletes the player if they die to a player killer or if they do not posses a halflife token.
    // If they do have a halflife token, it removes one from their inventory and the character is not deleted.
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Player), "Die", new Type[] { typeof(DamageHistoryInfo), typeof(DamageHistoryInfo) })]
    public static void PostDie(DamageHistoryInfo lastDamager, DamageHistoryInfo topDamager, ref Player __instance)
    {
        if (__instance.GetProperty((PropertyBool)31000) == null)
        {
            __instance.SetProperty((PropertyBool)31000, false);
            return;
        }
        if (__instance.GetProperty((PropertyBool)31000) != true)
            return;
        if (__instance.GetProperty((PropertyBool)31000) == true && Settings.DeleteHcToons)
        {
            var halfLife = __instance.GetInventoryItemsOfWCID(Settings.HalfLifeWCID).FirstOrDefault();

            if (__instance.IsPKDeath(topDamager))
            {
                __instance.Character.DeleteTime = (ulong)Time.GetUnixTime();
                __instance.Character.IsDeleted = true;
                __instance.CharacterChangesDetected = true;
                __instance.Session.LogOffPlayer(true);
                PlayerManager.HandlePlayerDelete(__instance.Character.Id);

                var success = PlayerManager.ProcessDeletedPlayer(__instance.Character.Id);
            }

            if (halfLife != null)
            {
                __instance.SendMessage("You have lost 1 halflife token!", ChatMessageType.Broadcast);
                __instance.TryRemoveFromInventoryWithNetworking(__instance.Guid.Full, out halfLife, Player.RemoveFromInventoryAction.ConsumeItem);
                return;
            }
            else if (halfLife == null)
            {
                __instance.Character.DeleteTime = (ulong)Time.GetUnixTime();
                __instance.Character.IsDeleted = true;
                __instance.CharacterChangesDetected = true;
                __instance.Session.LogOffPlayer(true);
                PlayerManager.HandlePlayerDelete(__instance.Character.Id);

                var success = PlayerManager.ProcessDeletedPlayer(__instance.Character.Id);
                return;
            }
        }
    }

    #endregion
}

