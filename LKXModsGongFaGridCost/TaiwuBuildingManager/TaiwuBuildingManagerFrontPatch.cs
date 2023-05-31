using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace ConvenienceFrontend.TaiwuBuildingManager
{
    internal class TaiwuBuildingManagerFrontPatch : BaseFrontPatch
    {
        public override void OnModSettingUpdate(string modIdStr)
        {
            UI_Bottom.print("");
            UI_AdvanceConfirm.print("");
            UI_BuildingArea.print("");
            UI_MapBlockCharList.print("");
            UI_Worldmap.print("");
            UI_BuildingManage.print("");
            // Events.RaiseAdvanceMonthBegin
            MouseTipMapBlock.print("");
            // GameDataBridge.AddMethodCall(Element.GameDataListenerId, 1, 10);
            //GameDataBridge.AddMethodCall(Element.GameDataListenerId, 1, 9, leftDays);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_BuildingManage), "OnClick")]
        public static void UI_BuildingManage_OnClick_Pro(CButton btn)
        {
            // ExpandQuickSelectBtn
            // ConfirmBtn
            Debug.Log("OnClick: " + btn.name);
        }
    }
}
