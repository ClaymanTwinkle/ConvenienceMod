using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace ConvenienceFrontend.InscribeChar
{
    internal class InscribeCharFrontPatch : BaseFrontPatch
    {
        public override void OnModSettingUpdate(string modIdStr)
        {
            // UI_CharacterMenuInfo
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UI_CharacterMenuInfo), "UpdateBtnIconAndLabel")]
        public static void UI_CharacterMenuInfo_UpdateBtnIconAndLabel_Prefix(UI_CharacterMenuInfo __instance, CButton btn, string enableIcon, string disableIcon)
        {
            int taiwuCharId = SingletonObject.getInstance<BasicGameData>().TaiwuCharId;
            bool isTaiwu = __instance.CharacterMenu.CurCharacterId == taiwuCharId;

            if (btn == __instance.CGet<CButton>("InscribeBtn") && isTaiwu)
            {
                btn.interactable = true;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_CharacterMenuInfo), "OnCurrentCharacterChange")]
        public static void UI_CharacterMenuInfo_OnCurrentCharacterChange_Postfix(UI_CharacterMenuInfo __instance, int prevCharacterId)
        {
            int taiwuCharId = SingletonObject.getInstance<BasicGameData>().TaiwuCharId;
            bool isTaiwu = __instance.CharacterMenu.CurCharacterId == taiwuCharId;

            if (isTaiwu)
            {
                __instance.CGet<CButton>("InscribeBtn").gameObject.SetActive(true);
            }
        }
    }
}
