using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
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
    [HarmonyPatch(typeof(Player), "Die", new Type[] { typeof(DamageHistoryInfo), typeof(DamageHistoryInfo) })]
    public static void PostDie(DamageHistoryInfo lastDamager, DamageHistoryInfo topDamager, ref Player __instance)
    {
        
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Player), nameof(Player.OnDeath), new Type[] { typeof(DamageHistoryInfo), typeof(DamageType), typeof(bool) })]
    public static void PostOnDeath(DamageHistoryInfo lastDamager, DamageType damageType, bool criticalHit, ref Player __instance, ref DeathMessage __result)
    {
        if (__instance.GetProperty((PropertyBool)31002) == null)
        {
            __instance.SetProperty((PropertyBool)31002, false);
        }

        if (__instance.GetProperty((PropertyBool)31000) == true && __instance.GetProperty((PropertyBool)31002) == false)
        {
            __instance.SendMessage("You have died in hardcore mode!", ChatMessageType.Broadcast);

            if (__instance.GetProperty((PropertyBool)31000) == true && Settings.DeleteHcToons && __instance.GetProperty((PropertyBool)31002) == false)
            {
                var halfLife = __instance.GetInventoryItemsOfWCID(Settings.HalfLifeWCID).FirstOrDefault();
                var killerName = lastDamager.TryGetPetOwnerOrAttacker()?.Name ?? "Unknown";
                var onlinePlayers = PlayerManager.GetAllOnline();

                if (__instance.IsPKDeath(lastDamager))
                {
                    __instance.Character.DeleteTime = (ulong)Time.GetUnixTime();
                    __instance.Character.IsDeleted = true;
                    __instance.CharacterChangesDetected = true;
                    __instance.Session.LogOffPlayer(true);
                    PlayerManager.HandlePlayerDelete(__instance.Character.Id);
                    foreach (var player in onlinePlayers)
                    {
                        if (player.Name != __instance.Name)
                            player.SendMessage($"{__instance.Name} has been killed by {killerName}. Thier light fades forever from this world.", ChatMessageType.WorldBroadcast);
                    }
                }
                else if (halfLife != null)
                {
                    __instance.SendMessage("You have lost 1 halflife token!", ChatMessageType.Broadcast);
                    __instance.TryConsumeFromInventoryWithNetworking(halfLife, 1);
                    foreach (var player in onlinePlayers)
                    {
                        var halfLifeCount = __instance.GetInventoryItemsOfWCID(Settings.HalfLifeWCID).Count;
                        if (player.Name != __instance.Name)
                            player.SendMessage($"{__instance.Name} has been killed by {killerName}. They have {halfLifeCount} Half Live(s) left.", ChatMessageType.WorldBroadcast);
                    }
                }
                else if (halfLife == null)
                {
                    __instance.Character.DeleteTime = (ulong)Time.GetUnixTime();
                    __instance.Character.IsDeleted = true;
                    __instance.CharacterChangesDetected = true;
                    __instance.Session.LogOffPlayer(true);
                    PlayerManager.HandlePlayerDelete(__instance.Character.Id);
                    foreach (var player in onlinePlayers)
                    {
                        if (player.Name != __instance.Name)
                            player.SendMessage($"{__instance.Name} has been killed by {killerName}. Thier light fades forever from this world.", ChatMessageType.WorldBroadcast);
                    }
                }

                __instance.SetProperty((PropertyBool)31002, true);
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Player), nameof(Player.ThreadSafeTeleportOnDeath))]
    public static void PostThreadSafeTeleportOnDeath(ref Player __instance)
    {
        if (__instance.GetProperty((PropertyBool)31002) == true)
        {
            __instance.SetProperty((PropertyBool)31002, false);
        }
    }


    #endregion
}

