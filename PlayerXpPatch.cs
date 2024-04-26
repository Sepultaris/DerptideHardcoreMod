using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;
using ACE.Server.Managers;
using ACE.Server.WorldObjects;

namespace DerptideHardcore;

[HarmonyPatchCategory(nameof(PlayerXpPatch))]
internal class PlayerXpPatch
{
    #region Settings
    public static Settings Settings = new();
    static string settingsPath => Path.Combine(Mod.ModPath, "Settings.json");
    private FileInfo settingsInfo = new(settingsPath);
    #endregion

    #region Patch

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.EarnXP), new Type[] { typeof(long), typeof(XpType), typeof(ShareType) })]
    public static bool PreEarnXP(long amount, XpType xpType, ShareType shareType, ref Player __instance)
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
            //Console.WriteLine($"{Name}.EarnXP({amount}, {sharable}, {fixedAmount})");

            // apply xp modifiers.  Quest XP is multiplicative with general XP modification
            var questModifier = PropertyManager.GetDouble("quest_xp_modifier").Item;
            var modifier = PropertyManager.GetDouble("xp_modifier").Item;
            if (xpType == XpType.Quest)
                modifier *= questModifier;

            // should this be passed upstream to fellowship / allegiance?
            var enchantment = __instance.GetXPAndLuminanceModifier(xpType);

            var m_amount = (long)Math.Round(amount * enchantment * modifier * Settings.HcXpMultiplier);

            if (m_amount < 0)
            {
                return true;
            }

            __instance.GrantXP(m_amount, xpType, shareType);
        }
        return false;
    }



    #endregion
}



