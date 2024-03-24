using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ConvenienceFrontend.CombatStrategy;
using FrameWork;
using GameData.Common;
using GameData.Domains.Building;
using GameData.Domains.Character;
using GameData.Domains.Item;
using GameData.Domains.Item.Display;
using GameData.Domains.TaiwuEvent;
using GameData.GameDataBridge;
using GameData.Serializer;
using GameData.Utilities;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using static ConvenienceFrontend.CombatStrategy.StrategyConst;
using static UnityEngine.GraphicsBuffer;

namespace ConvenienceFrontend.BatchMakeItem
{
    internal class BatchMakeItemFrontPatch : BaseFrontPatch
    {
        private static bool _batchMakeItem = false;
        private static CToggle _batchMakeItemToggle = null;
        private static List<ItemDisplayData> _batchMakeItemList = new List<ItemDisplayData>();
        private static sbyte _itemGrade = 0;

        public override void OnModSettingUpdate(string modIdStr)
        {
            // UI_Make
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_Make), "OnInit")]
        public static void UI_Make_OnInit_Pro(UI_Make __instance, sbyte ____curLifeSkillType, CButton ____buttonConfirm)
        {

            UI_Make_OnSwitchBuildingMake_Pro(__instance, ____curLifeSkillType, ____buttonConfirm);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_Make), "OnSwitchBuildingMake")]
        public static void UI_Make_OnSwitchBuildingMake_Pro(UI_Make __instance, sbyte ____curLifeSkillType, CButton ____buttonConfirm)
        {
            _batchMakeItemList = new List<ItemDisplayData>();

            if (SingletonObject.getInstance<TutorialChapterModel>().InGuiding || ____curLifeSkillType != LifeSkillType.Cooking)
            {
                if (_batchMakeItemToggle != null)
                {
                    _batchMakeItemToggle.isOn = false;
                    _batchMakeItemToggle.transform.parent.gameObject.SetActive(false);
                }
                return;
            }

            if (_batchMakeItemToggle == null)
            {
                if (____buttonConfirm == null) return;
                _batchMakeItemToggle = UIUtils.CreateToggle(____buttonConfirm.transform.parent, "BatchMakeItemToggle", "批量制作");
                var localPosition = ____buttonConfirm.transform.localPosition;
                _batchMakeItemToggle.transform.parent.localPosition = new Vector3(localPosition.x + 170f, localPosition.y - 35, localPosition.z);
                _batchMakeItemToggle.onValueChanged.AddListener((bool value) =>
                {
                    _batchMakeItem = value;
                });
            }
            else
            {
                _batchMakeItemToggle.isOn = false;
                _batchMakeItemToggle.transform.parent.gameObject.SetActive(true);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UI_Make), "ConfirmMake")]
        public static bool UI_Make_ConfirmMake_Prefix(UI_Make __instance)
        {
            if (_batchMakeItemToggle == null) return true;
            if (!_batchMakeItemToggle.transform.parent.gameObject.active) return true;
            if (!_batchMakeItem) return true;

            var traverse = Traverse.Create(__instance);
            ItemDisplayData _currentTarget = traverse.Field<ItemDisplayData>("_currentTarget").Value;
            ItemDisplayData _currentTool = traverse.Field<ItemDisplayData>("_currentTool").Value;
            List<ItemDisplayData> _allItems = traverse.Field<List<ItemDisplayData>>("_allItems").Value;
            List<(short, short)> _makeDropdownDataList = traverse.Field<List<(short, short)>>("_makeDropdownDataList").Value;
            CDropdown _makeDropdown = traverse.Field<CDropdown>("_makeDropdown").Value;
            Dictionary<int, MakeResult> _makeResultDict = traverse.Field<Dictionary<int, MakeResult>>("_makeResultDict").Value;
            List<short> _makeItemSubtypeIdList = traverse.Field<List<short>>("_makeItemSubtypeIdList").Value;
            short _makeCount = traverse.Field<short>("_makeCount").Value;
            bool _makeIsManual = traverse.Field<bool>("_makeIsManual").Value;
            short _makeItemSubTypeId = traverse.Field<short>("_makeItemSubTypeId").Value;
            sbyte _curLifeSkillType = traverse.Field<sbyte>("_curLifeSkillType").Value;
            BuildingBlockKey _buildingBlockKey = traverse.Field<BuildingBlockKey>("_buildingBlockKey").Value;

            ItemDisplayData itemDisplayData = _allItems.Find((ItemDisplayData d) => d.Key.Equals(_currentTarget.Key));
            if (itemDisplayData != null)
            {
                itemDisplayData.Amount -= _makeCount;
            }

            ItemDisplayData itemDisplayData2 = _allItems.Find((ItemDisplayData d) => d.Key.Equals(_currentTool.Key));
            if (itemDisplayData2 != null)
            {
                itemDisplayData2.Durability -= _makeCount;
            }

            void showResult()
            {
                if (_batchMakeItemList.Count == 0) return;
                BuildingBlockData _blockData = traverse.Field<BuildingBlockData>("_blockData").Value;
                // 结束
                TaiwuEventDomainHelper.MethodCall.OnCollectedMakingSystemItem(_buildingBlockKey, _blockData.TemplateId, showingGetItem: true);
                ArgumentBox argumentBox = EasyPool.Get<ArgumentBox>();
                argumentBox.SetObject("DisplayData", _batchMakeItemList);
                argumentBox.Set("Title", LocalStringManager.Get(2725));
                argumentBox.Set("InWareHouse", !traverse.Field<bool>("_isSettlement").Value);
                UIElement.GetItem.SetOnInitArgs(argumentBox);
                UIManager.Instance.ShowUI(UIElement.GetItem);

                _batchMakeItemList = new List<ItemDisplayData>();
            }

            void DoConfirmMake(MakeResult CurMakeResult, List<short> resultTemplateIdList)
            {
                bool _makePerfect = traverse.Field<bool>("_makePerfect").Value;
                int _taiwuCharId = traverse.Field<int>("_taiwuCharId").Value;
                short _makeItemTypeId = traverse.Field<short>("_makeItemTypeId").Value;
                ResourceInts _curMakeResourceCountInts = traverse.Field<ResourceInts>("_curMakeResourceCountInts").Value;
                ResourceInts _makeRequiredResourceInts = traverse.Field<ResourceInts>("_makeRequiredResourceInts").Value;
                short _makeTime = traverse.Field<short>("_makeTime").Value;

                MakeConditionArguments makeConditionArguments = default(MakeConditionArguments);
                makeConditionArguments.BuildingBlockKey = _buildingBlockKey;
                makeConditionArguments.CharId = _taiwuCharId;
                makeConditionArguments.IsManual = _makeIsManual;
                makeConditionArguments.MakeCount = _makeCount;
                makeConditionArguments.MakeItemSubTypeId = _makeItemSubTypeId;
                makeConditionArguments.MakeItemTypeId = _makeItemTypeId;
                makeConditionArguments.MaterialKey = _currentTarget.Key;
                makeConditionArguments.ResourceCount = _curMakeResourceCountInts;
                makeConditionArguments.ToolKey = _currentTool.Key;
                makeConditionArguments.IsPerfect = _makePerfect;
                MakeConditionArguments makeConditionArguments2 = makeConditionArguments;
                BuildingDomainHelper.AsyncMethodCall.CheckMakeCondition(__instance, makeConditionArguments2, delegate (int offset, RawDataPool dataPool)
                {
                    bool item2 = false;
                    Serializer.Deserialize(dataPool, offset, ref item2);
                    if (!item2)
                    {
                        UIElement.FullScreenMask.Hide();
                        showResult();
                    }
                    else if (_makePerfect)
                    {
                        __instance.CGet<GameObject>("PerfectEffectConfirm").SetActive(_makePerfect);
                        traverse.Method("ShowPerfectEffectLoop", false, true).GetValue();
                        traverse.Method("ShowResultPreviewImage", false).GetValue();
                        traverse.Method("PlayCenterAnim", true, true, true).GetValue();
                        SingletonObject.getInstance<YieldHelper>().DelaySecondsDo(1.5f, Action);
                    }
                    else
                    {
                        Action();
                    }
                });
                void Action()
                {
                    StartMakeArguments startMakeArguments = default(StartMakeArguments);
                    startMakeArguments.CharId = _taiwuCharId;
                    startMakeArguments.BuildingBlockKey = _buildingBlockKey;
                    startMakeArguments.Tool = _currentTool;
                    startMakeArguments.Material = _currentTarget;
                    startMakeArguments.ItemList = resultTemplateIdList;
                    startMakeArguments.ItemType = CurMakeResult.TargetResultStage.ItemType;
                    startMakeArguments.MakeItemSubTypeId = _makeItemSubTypeId;
                    startMakeArguments.ResourceCount = _curMakeResourceCountInts;
                    startMakeArguments.NeedResource = _makeRequiredResourceInts;
                    startMakeArguments.IsPerfect = _makePerfect;
                    StartMakeArguments startMakeArguments2 = startMakeArguments;
                    BuildingDomainHelper.MethodCall.StartMakeItem(__instance.Element.GameDataListenerId, startMakeArguments2);
                    if (_makeTime == 0)
                    {
                        BuildingDomainHelper.AsyncMethodCall.GetMakeItems(null, _buildingBlockKey, delegate (int offset, RawDataPool dataPool)
                        {
                            List<ItemDisplayData> itemList = null;
                            Serializer.Deserialize(dataPool, offset, ref itemList);
                            if (itemList != null)
                            {
                                _batchMakeItemList.AddRange(itemList);
                            }

                            traverse.Field<MakeItemData>("_makeItemData").Value = null;
                            traverse.Method("UpdateMakeState", false).GetValue();

                            var currentTarget = traverse.Field<ItemDisplayData>("_currentTarget").Value;
                            var currentTool = traverse.Field<ItemDisplayData>("_currentTool").Value;

                            

                            if (currentTool == null)
                            {
                                // 没有工具
                                showResult();
                            }
                            else if (currentTarget != null)
                            {
                                // 还有材料制作
                                traverse.Method("ConfirmMake").GetValue();
                            }
                            else
                            {
                                var filterItems = _allItems.FindAll((x) => 
                                {
                                    return x.Key.ItemType == ItemType.Material && ItemTemplateHelper.GetCraftRequiredLifeSkillType(x.Key.ItemType, x.Key.TemplateId) == _curLifeSkillType &&  ItemTemplateHelper.GetGrade(x.Key.ItemType, x.Key.TemplateId) <= _itemGrade; 
                                }
                                );
                                filterItems.Sort((x, y) => {
                                    return ItemTemplateHelper.GetGrade(y.Key.ItemType, y.Key.TemplateId) - ItemTemplateHelper.GetGrade(x.Key.ItemType, x.Key.TemplateId);
                                });
                                var item = filterItems.FirstOrDefault();

                                if (item != null)
                                {
                                    traverse.Method("ChangeCurrentTarget", item, true).GetValue();
                                    traverse.Method("ConfirmMake").GetValue();
                                }
                                else 
                                {
                                    showResult();
                                }
                            }
                        });
                    }

                    traverse.Method("RefreshAllItems").GetValue();

                }
            }

            short makeItemSubtypeId = _makeIsManual ? _makeItemSubTypeId : (short)(-1);
            ItemKey toolKey = _currentTool?.Key ?? ItemKey.Invalid;
            var MakeOriginTemplateId = _currentTarget.Key.TemplateId;
            BuildingDomainHelper.AsyncMethodCall.GetMakeResult(null, MakeOriginTemplateId, toolKey, _buildingBlockKey, _curLifeSkillType, _makeItemSubtypeIdList, makeItemSubtypeId, delegate (int offset, RawDataPool pool)
            {
                MakeResult makeResultItem = default(MakeResult);
                Serializer.Deserialize(pool, offset, ref makeResultItem);
                _makeResultDict[makeItemSubtypeId] = makeResultItem;

                List<short> resultTemplateIdList = new List<short>(_makeCount);
                short item = (short)((_makeDropdown.value == 0) ? (-1) : _makeDropdownDataList[_makeDropdown.value].Item2);

                _makeResultDict.TryGetValue(_makeIsManual ? _makeItemSubTypeId : (-1), out MakeResult CurMakeResult);

                for (int i = 0; i < _makeCount; i++)
                {
                    if (_makeDropdown.value == 0)
                    {

                        item = CurMakeResult.TargetResultStage.GetGradeAndId().Item2;
                    }

                    resultTemplateIdList.Add(item);
                }

                DoConfirmMake(CurMakeResult, resultTemplateIdList);
            });

            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_Make), "ChangeCurrentTargetOnMake")]
        public static void UI_Make_ChangeCurrentTargetOnMake_Postfix(UI_Make __instance, ItemDisplayData target)
        {
            if (target != null)
            {
                _itemGrade = ItemTemplateHelper.GetGrade(target.Key.ItemType, target.Key.TemplateId);
            }
        }
    }
}
