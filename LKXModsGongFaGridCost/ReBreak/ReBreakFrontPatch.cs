using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Config;
using GameData.Domains.Character;
using GameData.Domains.Taiwu;
using HarmonyLib;

namespace ConvenienceFrontend.ReBreak
{
    internal class ReBreakFrontPatch : BaseFrontPatch
    {
        public override void OnModSettingUpdate(string modIdStr)
        {
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UI_CharacterMenuPractice), "UpdateReBreakBtn")]
        public static void UI_CharacterMenuPractice_UpdateReBreakBtn_PrePatch(UI_CharacterMenuPractice __instance, Dictionary<short, TaiwuCombatSkill> ____taiwuCombatSkill)
        {
            foreach(var combatSkill in ____taiwuCombatSkill.Values)
            {
                combatSkill.LastClearBreakPlateTime = 0;
            }
        }
    }
}
