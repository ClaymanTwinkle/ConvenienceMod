using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace ConvenienceFrontend.CombatSkillSpecialBreakOptimize
{
    internal class CombatSkillSpecialBreakOptimizeFrontPatch : BaseFrontPatch
    {
        public override void OnModSettingUpdate(string modIdStr)
        {
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_CombatSkillSpecialBreak), "OnClick")]
        public static void UI_CombatSkillSpecialBreak_OnClick_Pro(UI_CombatSkillSpecialBreak __instance, CButton btn)
        {
            // var traverse = Traverse.Create(__instance);

            // ExpandQuickSelectBtn
            // ConfirmBtn
            // ShopQuickSelectBtn
            // Debug.Log(traverse.Field("_isResultFirst").GetValue<bool>());
            // Debug.Log(traverse.Field("_blockData").GetValue<BuildingBlockData>().TemplateId + "OnClick: " + btn.name);

        }
    }
}
