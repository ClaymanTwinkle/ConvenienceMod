using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvenienceFrontend.CustomWeapon;
using FrameWork;
using FrameWork.ModSystem;
using GameData.GameDataBridge;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace ConvenienceFrontend.TaiwuBuildingManager
{
    internal class TaiwuBuildingManagerFrontPatch : BaseFrontPatch
    {
        public static string ModIdStr = string.Empty;
        private static bool _enableMod = false;

        private static CButton _openTaiwuBuildingManagerButton = null;


        public override void OnModSettingUpdate(string modIdStr)
        {
            ModIdStr = modIdStr;

            ModManager.GetSetting(modIdStr, "Toggle_EnableBuildManager", ref _enableMod);

            if (_openTaiwuBuildingManagerButton != null)
            {
                _openTaiwuBuildingManagerButton.gameObject.SetActive(_enableMod);
            }

            UI_Bottom.print("");
            UI_AdvanceConfirm.print("");
            UI_BuildingArea.print("");
            UI_MapBlockCharList.print("");
            UI_Worldmap.print("");
            UI_BuildingManage.print("");
            UI_Reading.print("");
            UI_BuildingBlockList.print("");
            // Events.RaiseAdvanceMonthBegin
            MouseTipMapBlock.print("");
            UI_Reading.print("");
            // GameDataBridge.AddMethodCall(Element.GameDataListenerId, 1, 10);
            //GameDataBridge.AddMethodCall(Element.GameDataListenerId, 1, 9, leftDays);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_BuildingManage), "OnClick")]
        public static void UI_BuildingManage_OnClick_Pro(CButton btn)
        {
            // ExpandQuickSelectBtn
            // ConfirmBtn
            // ShopQuickSelectBtn
            Debug.Log("OnClick: " + btn.name);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_Bottom), "OnInit")]
        public static void UI_Bottom_OnInit_Postfix(UI_Bottom __instance)
        {
            if (_openTaiwuBuildingManagerButton != null) 
            {
                _openTaiwuBuildingManagerButton.gameObject.SetActive(_enableMod);
                return;
            }

            Refers refers = __instance.CGet<Refers>("Minimap");
            var parent = refers.gameObject.transform;

            _openTaiwuBuildingManagerButton = GameObjectCreationUtils.UGUICreateCButton(parent, new Vector2(-200, 210), new Vector2(120, 50), 16, "种田管家");
            _openTaiwuBuildingManagerButton.ClearAndAddListener(delegate () {
                var element = UI_TaiwuBuildingManager.GetUI();
                ArgumentBox box = EasyPool.Get<ArgumentBox>();
                element.SetOnInitArgs(box);
                UIManager.Instance.ShowUI(element);
            });
            _openTaiwuBuildingManagerButton.gameObject.SetActive(_enableMod);
        }
    }
}
