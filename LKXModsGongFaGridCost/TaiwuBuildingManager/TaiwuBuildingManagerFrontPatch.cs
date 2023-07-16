using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Game.Model;
using Config;
using Config.Common;
using ConvenienceFrontend.CustomWeapon;
using FrameWork;
using FrameWork.ModSystem;
using GameData.Domains.Building;
using GameData.Domains.Character.Display;
using GameData.GameDataBridge;
using GameData.Utilities;
using HarmonyLib;
using Newtonsoft.Json;
using TaiwuModdingLib.Core.Utils;
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
            UI_Heal.print("");


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

        /// <summary>
        /// 设置不可移动的资源
        /// </summary>
        /// <param name="__instance"></param>
        /// <returns></returns>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(UI_BuildingArea), "SetBlockCannotMove")]
        public static bool UI_BuildingArea_SetBlockCannotMove(UI_BuildingArea __instance)
        {
            if (!ConvenienceFrontend.Config.GetTypedValue<bool>("Toggle_EnableMoveResource")) return true;

            var traverse = Traverse.Create(__instance);
            Dictionary<short, BuildingBlockData> _buildingDict = (Dictionary<short, BuildingBlockData>)traverse.Field("_buildingDict").GetValue();
            Dictionary<short, Refers> _blockRefersDict = (Dictionary<short, Refers>)traverse.Field("_blockRefersDict").GetValue();
            Dictionary<short, Refers> _buildingCannotMoveDict = (Dictionary<short, Refers>)traverse.Field("_buildingCannotMoveDict").GetValue();
            PoolItem _buildingCannotMovePool = (PoolItem)traverse.Field("_buildingCannotMovePool").GetValue();

            foreach (KeyValuePair<short, BuildingBlockData> item2 in _buildingDict)
            {
                BuildingBlockData value = item2.Value;
                BuildingBlockItem item = BuildingBlock.Instance.GetItem(value.TemplateId);
                _blockRefersDict[item2.Key].CGet<RectTransform>("ShopEvent").gameObject.SetActive(value: false);
                _blockRefersDict[item2.Key].CGet<RectTransform>("ShopTipHolder").gameObject.SetActive(value: false);
                _blockRefersDict[item2.Key].CGet<RectTransform>("GetEarnHolder").gameObject.SetActive(value: false);
                if (/* 去掉是资源的判断 BuildingBlockData.IsResource(item.Type) || */ (value.Durability < item.MaxDurability && value.OperationType == -1) || item.OperationTotalProgress[2] == -1)
                {
                    GameObject gameObject = _blockRefersDict[item2.Key].CGet<RectTransform>("BuildingCannotMoveHolder").gameObject;
                    gameObject.SetActive(value: true);
                    __instance.CallMethod("ManagePoolItem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, _buildingCannotMovePool, gameObject, _buildingCannotMoveDict, item2.Key, true);
                    Refers refers = _buildingCannotMoveDict[item2.Key];
                    _blockRefersDict[item2.Key].CGet<CImage>("BuildingIcon").GetComponent<CButton>().interactable = false;
                    _blockRefersDict[item2.Key].CGet<CImage>("SelectTip").enabled = false;
                    _blockRefersDict[item2.Key].CGet<GameObject>("BuildingOperateBg").SetActive(value: true);
                    if (BuildingBlockData.IsResource(item.Type))
                    {
                        refers.CGet<MouseTipDisplayer>("BuildingCannotMoveContent").PresetParam[0] = LocalStringManager.Get(1100);
                    }

                    if (value.Durability < item.MaxDurability && value.OperationType == -1)
                    {
                        refers.CGet<MouseTipDisplayer>("BuildingCannotMoveContent").PresetParam[0] = LocalStringManager.Get(1101);
                    }

                    if (item.OperationTotalProgress[2] == -1)
                    {
                        refers.CGet<MouseTipDisplayer>("BuildingCannotMoveContent").PresetParam[0] = LocalStringManager.Get(1102);
                    }
                }
                else if (value.OperationType != -1)
                {
                    _blockRefersDict[item2.Key].CGet<CImage>("BuildingIcon").GetComponent<CButton>().interactable = false;
                    _blockRefersDict[item2.Key].CGet<CImage>("SelectTip").enabled = false;
                }
            }
            RectTransform _roadHolder = (RectTransform)traverse.Field("_roadHolder").GetValue();
            RectTransform _chickenHolder = (RectTransform)traverse.Field("_chickenHolder").GetValue();
            _roadHolder.gameObject.SetActive(value: false);
            _chickenHolder.gameObject.SetActive(value: false);

            return false;
        }

        /// <summary>
        /// 设置可以建造的资源
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_BuildingOverview), "OnClick")]
        public static void BuildingBlockData_IsResource_Patch(UI_BuildingOverview __instance, CButton btn)
        {
            if (btn.name == "AutoArrangeBtn")
            {
                var traverse = Traverse.Create(__instance);
                var _configData = (BuildingBlockItem)traverse.Field("_configData").GetValue();
                if (_configData == null) return;
                if (!(_configData.Type == EBuildingBlockType.NormalResource || _configData.Type == EBuildingBlockType.SpecialResource)) return;
                List<int> _availableWorkers = (List<int>)traverse.Field("_availableWorkers").GetValue();
                int[] _operatorList = (int[])traverse.Field("_operatorList").GetValue();

                if (Array.Exists(_operatorList, x => x > -1)) return;

                Dictionary<int, short> _propertyValueDict = (Dictionary<int, short>)traverse.Field("_propertyValueDict").GetValue();
                Dictionary<int, CharacterDisplayData> _charDisplayDataDict = (Dictionary<int, CharacterDisplayData>)traverse.Field("_charDisplayDataDict").GetValue();

                BuildingModel buildingModel = SingletonObject.getInstance<BuildingModel>();
                List<int> list = _availableWorkers.Where((int id) => !_operatorList.Contains(id) && !_charDisplayDataDict[id].CompletelyInfected && !buildingModel.VillagerWork.ContainsKey(id)).ToList();
                if (list.Count - 1 > 0)
                {
                    _operatorList[0] = list[0];
                }

                __instance.CallMethod("UpdateOperatorInfo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                __instance.CallMethod("UpdateConfirmButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            }
        }

        /// <summary>
        /// 设置可以建造的资源
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_BuildingOverview), "InitData")]
        public static void BuildingBlockData_IsResource_Patch(UI_BuildingOverview __instance)
        {
            if (!ConvenienceFrontend.Config.GetTypedValue<bool>("Toggle_EnableBuildResource")) return;

            Dictionary<EBuildingBlockClass, List<BuildingBlockItem>> _buildingMap = (Dictionary<EBuildingBlockClass, List<BuildingBlockItem>>)Traverse.Create(__instance).Field("_buildingMap").GetValue();
            BuildingBlock.Instance.Iterate(delegate (BuildingBlockItem item)
            {
                if (item.Class == EBuildingBlockClass.BornResource && item.Type != EBuildingBlockType.UselessResource)
                {
                    _buildingMap[EBuildingBlockClass.Resource].Add(item);
                }
                return true;
            });
        }
    }
}
