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
        if (__instance.GetProperty((PropertyBool)31000) == true)
        {
            var token = __instance.GetInventoryItemsOfWCID(Settings.TokenWCID).FirstOrDefault();

            if (__instance.IsPKDeath(topDamager))
            {
                __instance.Character.DeleteTime = (ulong)Time.GetUnixTime();
                __instance.Character.IsDeleted = true;
                __instance.CharacterChangesDetected = true;
                __instance.Session.LogOffPlayer(true);
                PlayerManager.HandlePlayerDelete(__instance.Character.Id);

                var success = PlayerManager.ProcessDeletedPlayer(__instance.Character.Id);
            }

            if (token != null)
            {
                __instance.SendMessage("You have lost 1 token!", ChatMessageType.Broadcast);
                __instance.TryRemoveFromInventoryWithNetworking(__instance.Guid.Full, out token, Player.RemoveFromInventoryAction.ConsumeItem);
                return;
            }
            else if (token == null)
            {
                __instance.Character.DeleteTime = (ulong)Time.GetUnixTime();
                __instance.Character.IsDeleted = true;
                __instance.CharacterChangesDetected = true;
                __instance.Session.LogOffPlayer(true);
                PlayerManager.HandlePlayerDelete(__instance.Character.Id);

                var success = PlayerManager.ProcessDeletedPlayer(__instance.Character.Id);
                __instance.SendMessage("You have no tokens to lose!", ChatMessageType.Broadcast);
                return;
            }
        }
    }

    #endregion
}

