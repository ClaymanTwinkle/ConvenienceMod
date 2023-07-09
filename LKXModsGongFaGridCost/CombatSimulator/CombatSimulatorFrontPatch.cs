using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvenienceFrontend.Utils;
using FrameWork.ModSystem;
using HarmonyLib;
using UnityEngine;

namespace ConvenienceFrontend.CombatSimulator
{
    internal class CombatSimulatorFrontPatch : BaseFrontPatch
    {
        private static CButton _randomNPCCombatButton = null;

        public override void OnModSettingUpdate(string modIdStr)
        {
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_Bottom), "OnInit")]
        public static void UI_Bottom_OnInit_Postfix(UI_Bottom __instance)
        {
            if (!ConvenienceFrontend.IsLocalTest()) return;

            if (_randomNPCCombatButton != null)
            {
                _randomNPCCombatButton.gameObject.SetActive(ConvenienceFrontend.IsLocalTest());
                return;
            }

            Refers refers = __instance.CGet<Refers>("Minimap");
            var parent = refers.gameObject.transform;

            _randomNPCCombatButton = GameObjectCreationUtils.UGUICreateCButton(parent, new Vector2(-200, 265), new Vector2(120, 50), 16, "随机对战");
            _randomNPCCombatButton.ClearAndAddListener(delegate () {
                GameDataBridgeUtils.SendData<int, int>(8, 1970, 0, 0, null);
            });
            _randomNPCCombatButton.gameObject.SetActive(ConvenienceFrontend.IsLocalTest());
        }
    }
}
