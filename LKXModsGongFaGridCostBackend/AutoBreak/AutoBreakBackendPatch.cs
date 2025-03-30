using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Config;
using ConvenienceBackend.MergeBookPanel;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.CombatSkill;
using GameData.Domains.Taiwu;
using GameData.GameDataBridge;
using GameData.Utilities;
using HarmonyLib;
using NLog;

namespace ConvenienceBackend.AutoBreak
{
    internal class AutoBreakBackendPatch : BaseBackendPatch
    {
        private static Logger _logger = LogManager.GetLogger("自动突破");

        public override void OnModSettingUpdate(string modIdStr)
        {
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TaiwuDomain), "EnterSkillBreakPlate")]
        public static void Taiwu_Init_PostPatch(TaiwuDomain __instance, ref GameData.Domains.Taiwu.SkillBreakPlate __result)
        {
            // if (!_isAutoBreak) return;

            for (int j = 0; j < __result.Width; j++)
            {
                for (int k = 0; k < __result.Height; k++)
                {
                    var grid = __result[j, k];
                    if (grid.State == ESkillBreakGridState.Invisible)
                    {
                        grid.State = ESkillBreakGridState.Showed; // 显示
                    }
                    // grid.SuccessRateFix = 100;
                }
            }

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameData.Domains.Taiwu.SkillBreakPlate), "CalcSuccessRate")]
        public static bool Taiwu_CalcSuccessRate_PostPatch(GameData.Domains.Taiwu.SkillBreakPlate __instance, ref short __result)
        {
            __result = 100;
            return false;
        }

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(GameData.Domains.Taiwu.SkillBreakPlate), "UpdateCanSelectGrids")]
        //public static void Taiwu_UpdateCanSelectGrids_PostPatch(GameData.Domains.Taiwu.SkillBreakPlate __instance)
        //{
        //    if (__instance.Finished)
        //    {
        //        return;
        //    }

        //    foreach (SkillBreakPlateIndex index in __instance.GetIndexes())
        //    {
        //        if (__instance[index].State == ESkillBreakGridState.Showed)
        //        {
        //            __instance[index].State = ESkillBreakGridState.CanSelect;
        //        }
        //    }
        //}
    }
}
