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

namespace ConvenienceFrontend.BatchMakeItem
{
    internal class BatchMakeItemFrontPatch : BaseFrontPatch
    {
        private static bool _batchMakeItem = false;
        private static CToggle _batchMakeItemToggle = null;
        private static List<ItemDisplayData> _batchMakeItemList = new List<ItemDisplayData>();
        private static sbyte _itemGrade = 0;
        private static int _curMakeItemSubToggleIndex = -1;
        private static bool _makeIsManual = false;
        private static bool _isFirstBatchMake = false;

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
            _itemGrade = 0;
            _isFirstBatchMake = false;
            _makeIsManual = false;
            _curMakeItemSubToggleIndex = -1;

            if (SingletonObject.getInstance<TutorialChapterModel>().InGuiding || (____curLifeSkillType != LifeSkillType.Cooking && ____curLifeSkillType != LifeSkillType.Medicine))
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
                _batchMakeItemToggle.transform.parent.localPosition = new Vector3(localPosition.x - 170f - 240f, localPosition.y - 35, localPosition.z);
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
        [HarmonyPatch(typeof(UI_Make), "OnClick")]
        public static void UI_Make_OnClick_Prefix(UI_Make __instance, CButton btn)
        {
            if (btn == null) return;
            if (btn.name == "ButtonConfirm")
            {
                _isFirstBatchMake = true;
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
            ItemDisplayData currentTarget = traverse.Field<ItemDisplayData>("_currentTarget").Value;
            ItemDisplayData currentTool = traverse.Field<ItemDisplayData>("_currentTool").Value;
            List<ItemDisplayData> allItems = traverse.Field<List<ItemDisplayData>>("_allItems").Value;
            List<(short, short)> makeDropdownDataList = traverse.Field<List<(short, short)>>("_makeDropdownDataList").Value;
            CDropdown makeDropdown = traverse.Field<CDropdown>("_makeDropdown").Value;
            Dictionary<int, MakeResult> makeResultDict = traverse.Field<Dictionary<int, MakeResult>>("_makeResultDict").Value;
            List<short> makeItemSubtypeIdList = traverse.Field<List<short>>("_makeItemSubtypeIdList").Value;
            short makeCount = traverse.Field<short>("_makeCount").Value;
            bool makeIsManual = traverse.Field<bool>("_makeIsManual").Value;
            int curMakeItemSubToggleIndex = traverse.Field<int>("_curMakeItemSubToggleIndex").Value;
            short makeItemSubTypeId = traverse.Field<short>("_makeItemSubTypeId").Value;
            sbyte curLifeSkillType = traverse.Field<sbyte>("_curLifeSkillType").Value;
            BuildingBlockKey buildingBlockKey = traverse.Field<BuildingBlockKey>("_buildingBlockKey").Value;

            ItemDisplayData itemDisplayData = allItems.Find((ItemDisplayData d) => d.Key.Equals(currentTarget.Key));
            if (itemDisplayData != null)
            {
                itemDisplayData.Amount -= makeCount;
            }

            ItemDisplayData itemDisplayData2 = allItems.Find((ItemDisplayData d) => d.Key.Equals(currentTool.Key));
            if (itemDisplayData2 != null)
            {
                itemDisplayData2.Durability -= makeCount;
            }

            if (_isFirstBatchMake)
            {
                _isFirstBatchMake = false;

                _itemGrade = ItemTemplateHelper.GetGrade(currentTarget.Key.ItemType, currentTarget.Key.TemplateId);
                _makeIsManual = makeIsManual;
                _curMakeItemSubToggleIndex = curMakeItemSubToggleIndex;
            }
            else
            {
                if (_makeIsManual && makeItemSubtypeIdList != null && _curMakeItemSubToggleIndex < makeItemSubtypeIdList.Count && _curMakeItemSubToggleIndex>-1)
                {
                    makeIsManual = _makeIsManual;
                    makeItemSubTypeId = makeItemSubtypeIdList[_curMakeItemSubToggleIndex];
                }
            }

            void showResult()
            {
                if (_batchMakeItemList.Count == 0) return;
                BuildingBlockData _blockData = traverse.Field<BuildingBlockData>("_blockData").Value;
                // 结束
                TaiwuEventDomainHelper.MethodCall.OnCollectedMakingSystemItem(buildingBlockKey, _blockData.TemplateId, showingGetItem: true);
                ArgumentBox argumentBox = EasyPool.Get<ArgumentBox>();
                argumentBox.SetObject("DisplayData", _batchMakeItemList);
                argumentBox.Set("Title", LocalStringManager.Get(2813));
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
                makeConditionArguments.BuildingBlockKey = buildingBlockKey;
                makeConditionArguments.CharId = _taiwuCharId;
                makeConditionArguments.IsManual = makeIsManual;
                makeConditionArguments.MakeCount = makeCount;
                makeConditionArguments.MakeItemSubTypeId = makeItemSubTypeId;
                makeConditionArguments.MakeItemTypeId = _makeItemTypeId;
                makeConditionArguments.MaterialKey = currentTarget.Key;
                makeConditionArguments.ResourceCount = _curMakeResourceCountInts;
                makeConditionArguments.ToolKey = currentTool.Key;
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
                    startMakeArguments.BuildingBlockKey = buildingBlockKey;
                    startMakeArguments.Tool = currentTool;
                    startMakeArguments.Material = currentTarget;
                    startMakeArguments.ItemList = resultTemplateIdList;
                    startMakeArguments.ItemType = CurMakeResult.TargetResultStage.ItemType;
                    startMakeArguments.MakeItemSubTypeId = makeItemSubTypeId;
                    startMakeArguments.ResourceCount = _curMakeResourceCountInts;
                    startMakeArguments.NeedResource = _makeRequiredResourceInts;
                    startMakeArguments.IsPerfect = _makePerfect;
                    StartMakeArguments startMakeArguments2 = startMakeArguments;
                    BuildingDomainHelper.MethodCall.StartMakeItem(__instance.Element.GameDataListenerId, startMakeArguments2);
                    if (_makeTime == 0)
                    {
                        BuildingDomainHelper.AsyncMethodCall.GetMakeItems(null, buildingBlockKey, delegate (int offset, RawDataPool dataPool)
                        {
                            List<ItemDisplayData> itemList = null;
                            Serializer.Deserialize(dataPool, offset, ref itemList);
                            if (itemList != null)
                            {
                                _batchMakeItemList.AddRange(itemList);
                            }

                            traverse.Field<MakeItemData>("_makeItemData").Value = null;
                            traverse.Method("UpdateMakeState", false).GetValue();

                            if (traverse.Field<ItemDisplayData>("_currentTool").Value == null)
                            {
                                // 没有工具
                                showResult();
                            }
                            else if (traverse.Field<ItemDisplayData>("_currentTarget").Value != null)
                            {
                                // 还有材料制作
                                traverse.Method("ConfirmMake").GetValue();
                            }
                            else
                            {
                                var filterItems = allItems.FindAll((x) => 
                                {
                                    return x.Key.ItemType == ItemType.Material && ItemTemplateHelper.GetCraftRequiredLifeSkillType(x.Key.ItemType, x.Key.TemplateId) == curLifeSkillType &&  ItemTemplateHelper.GetGrade(x.Key.ItemType, x.Key.TemplateId) <= _itemGrade; 
                                }
                                );
                                filterItems.Sort((x, y) => {
                                    return ItemTemplateHelper.GetGrade(y.Key.ItemType, y.Key.TemplateId) - ItemTemplateHelper.GetGrade(x.Key.ItemType, x.Key.TemplateId);
                                });
                                var item = filterItems.FirstOrDefault();

                                if (item != null)
                                {
                                    traverse.Method("ChangeCurrentTarget", item, true).GetValue();
                                    if (_makeIsManual)
                                    {
                                        traverse.Method("SelectMakeItemSubType", _curMakeItemSubToggleIndex, _makeIsManual).GetValue();
                                        traverse.Field<SubTogGroup>("_makeSubTogGroup").Value.ToggleGroup.Set(_curMakeItemSubToggleIndex, _makeIsManual);
                                    }
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

            short makeItemSubtypeId = makeIsManual ? makeItemSubTypeId : (short)(-1);
            ItemKey toolKey = currentTool?.Key ?? ItemKey.Invalid;
            var MakeOriginTemplateId = currentTarget.Key.TemplateId;
            BuildingDomainHelper.AsyncMethodCall.GetMakeResult(null, MakeOriginTemplateId, toolKey, buildingBlockKey, curLifeSkillType, makeItemSubtypeIdList, makeItemSubtypeId, delegate (int offset, RawDataPool pool)
            {
                MakeResult makeResultItem = default(MakeResult);
                Serializer.Deserialize(pool, offset, ref makeResultItem);
                makeResultDict[makeItemSubtypeId] = makeResultItem;

                List<short> resultTemplateIdList = new List<short>(makeCount);
                short item = (short)((makeDropdown.value == 0) ? (-1) : makeDropdownDataList[makeDropdown.value].Item2);

                makeResultDict.TryGetValue(makeIsManual ? makeItemSubTypeId : (-1), out MakeResult CurMakeResult);

                for (int i = 0; i < makeCount; i++)
                {
                    if (makeDropdown.value == 0)
                    {

                        item = CurMakeResult.TargetResultStage.GetGradeAndId().Item2;
                    }

                    resultTemplateIdList.Add(item);
                }

                DoConfirmMake(CurMakeResult, resultTemplateIdList);
            });

            return false;
        }
    }
}
