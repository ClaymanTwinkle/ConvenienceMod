using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrameWork;
using FrameWork.ModSystem;
using GameData.GameDataBridge;
using GameData.Utilities;
using HarmonyLib;
using UnityEngine;

namespace ConvenienceFrontend.RollCreateRole
{
    internal class RollCreateRoleFrontPatch : BaseFrontPatch
    {
        private static GameObject _rollPanel = null;

        public override void OnModSettingUpdate(string modIdStr)
        {
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_NewGame), "Awake")]
        public static void UI_NewGame_Awake_PostPatch(UI_NewGame __instance)
        {
            CToggleGroup cToggleGroup = __instance.CGet<CToggleGroup>("SwitchMode");

            CButton rollButton = __instance.Names.Contains("RollButton") ? __instance.CGet<CButton>("RollButton") : null;
            if (rollButton == null)
            {
                rollButton = GameObjectCreationUtils.UGUICreateCButton(cToggleGroup.transform, new UnityEngine.Vector2(0, 0), new UnityEngine.Vector2(150, 60), 15, "Roll属性");
                __instance.AddMono(rollButton, "RollButton");
            }

            rollButton.ClearAndAddListener(delegate () {
                UI_CheckInscription.print("");

                if (_rollPanel == null)
                {
                    createRollPanel(__instance);
                }
                _rollPanel.SetActive(true);

            });
        }


        private static void createRollPanel(UI_NewGame __instance)
        {
            Refers refers = __instance.CGet<Refers>("InscriptionView");

            _rollPanel = GameObjectCreationUtils.CreatePopupWindow(__instance.transform, new UnityEngine.Vector2(0, 0), new UnityEngine.Vector2(1200, 1200), "Roll属性", new Action(() => {
                _rollPanel.SetActive(false);
            }), null);
            PopupWindow component2 = _rollPanel.GetComponent<PopupWindow>();
            foreach (var component in component2.GetComponentsInChildren<Component>())
            {
                if (component.name.Contains("ImgTitle"))
                {
                    component.gameObject.SetActive(false);
                }
            }
            component2.TitleLabel.gameObject.SetActive(false);
        }
    }
}
