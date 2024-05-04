using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;
using ACE.Server.Managers;
using ACE.Server.WorldObjects;

namespace DerptideHardcore;

[HarmonyPatchCategory(nameof(PlayerLuminancePatch))]
internal class PlayerLuminancePatch
{
    #region Settings
    public static Settings Settings = new();
    static string settingsPath => Path.Combine(Mod.ModPath, "Settings.json");
    private FileInfo settingsInfo = new(settingsPath);
    #endregion

    #region Patch
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.EarnLuminance), new Type[] { typeof(long), typeof(XpType), typeof(ShareType) })]
    public static bool PreEarnLuminance(long amount, XpType xpType, ShareType shareType, ref Player __instance)
    {
        if (__instance.GetProperty((PropertyBool)31000) == null)
        {
            __instance.SetProperty((PropertyBool)31000, false);
            return true;
        }
        if (__instance.GetProperty((PropertyBool)31000) != true)
            return true;
        if (__instance.GetProperty((PropertyBool)31000) == true)
        {
            if (__instance.IsOlthoiPlayer)
                return true;

            // following the same model as Player_Xp
            var questModifier = PropertyManager.GetDouble("quest_lum_modifier").Item;
            var modifier = PropertyManager.GetDouble("luminance_modifier").Item;
            if (xpType == XpType.Quest)
                modifier *= questModifier;

            // should this be passed upstream to fellowship?
            var enchantment = __instance.GetXPAndLuminanceModifier(xpType);

            amount = (long)(amount + (amount * Settings.HcLumXpMultiplier));

            var m_amount = (long)Math.Round(amount * enchantment * modifier);

            __instance.GrantLuminance(m_amount, xpType, shareType);
        }
        return false;
    }
    #endregion
}



