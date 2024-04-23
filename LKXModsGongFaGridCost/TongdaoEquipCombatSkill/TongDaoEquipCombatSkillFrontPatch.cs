using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CharacterDataMonitor;
using Config;
using ConvenienceFrontend.MergeBookPanel;
using FrameWork;
using GameData.Common;
using GameData.Domains.Character;
using GameData.Domains.Taiwu;
using GameData.GameDataBridge;
using GameData.Serializer;
using GameData.Utilities;
using HarmonyLib;
using TaiwuModdingLib.Core.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

namespace ConvenienceFrontend.TongdaoEquipCombatSkill
{
    internal class TongDaoEquipCombatSkillFrontPatch : BaseFrontPatch
    {
        public override void OnModSettingUpdate(string modIdStr)
        {
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_CharacterMenuEquipCombatSkill), "UpdateCharacter")]
        public static void UI_CharacterMenuEquipCombatSkill_UpdateCharacter_Postfix(UI_CharacterMenuEquipCombatSkill __instance, RectTransform ____neiliAllocationHolder)
        {
            var _equipSkillRefers = __instance.CGet<Refers>("EquipSkill");

            if (DisablePatch(__instance))
            {
                _equipSkillRefers.CGet<CToggleGroup>("PlanHolder").transform.parent.gameObject.SetActive(__instance.CharacterMenu.CanOperate);
                return;
            }

            _equipSkillRefers.CGet<CToggleGroup>("PlanHolder").transform.parent.gameObject.SetActive(false);
            __instance.CGet<Refers>("CombatSkill").gameObject.SetActive(false);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_CharacterMenuEquipCombatSkill), "TaiwuCharId", MethodType.Getter)]
        public static void UI_CharacterMenuEquipCombatSkill_TaiwuCharId_Postfix(UI_CharacterMenuEquipCombatSkill __instance, ref int __result)
        {
            if (DisablePatch(__instance)) return;
            if (__instance.CharacterMenu.CurCharacterId > 0) { __result = __instance.CharacterMenu.CurCharacterId; }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_CharacterMenuEquipCombatSkill), "OnCurrentCharacterChange")]
        public static void UI_CharacterMenuEquipCombatSkill_OnCurrentCharacterChange_Postfix(UI_CharacterMenuEquipCombatSkill __instance, int prevCharacterId)
        {
            if (DisablePatch(__instance)) return;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UI_CharacterMenuEquipCombatSkill), "RequireGridCounts")]
        public static bool UI_CharacterMenuEquipCombatSkill_RequireGridCounts_Prefix(UI_CharacterMenuEquipCombatSkill __instance, EquipCombatSkillMonitor ____dataMonitor, sbyte[] ____specificGridCount, byte[] ____genericGridAllocation)
        {
            if (DisablePatch(__instance)) return true;

            CharacterDomainHelper.AsyncMethodCall.GetCombatSkillSlotCounts(null, __instance.CharacterMenu.CurCharacterId, delegate (int offset, RawDataPool pool) {
                sbyte[] item = null;
                Serializer.Deserialize(pool, offset, ref item);
                for (sbyte b = 0; b < 5; b = (sbyte)(b + 1))
                {
                    ____specificGridCount[b] = item[b];
                }
                var _totalGenericGrid = (sbyte)item[5];
                __instance.SetPrivateField("_totalGenericGrid", _totalGenericGrid);

                bool flag = ____genericGridAllocation[0] == byte.MaxValue;
                FakeGetGenericGridAllocation(__instance, ____dataMonitor, ____specificGridCount, ____genericGridAllocation, _totalGenericGrid);
                if (flag)
                {
                    __instance.CallPrivateMethod("UpdateEquippedSkills");
                }
                else
                {
                    __instance.CallPrivateMethod("UpdateGenericGrid");
                }

            });
            // TaiwuDomainHelper.MethodCall.GetGenericGridAllocation(__instance.Element.GameDataListenerId);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UI_CharacterMenuEquipCombatSkill), "UnequipAllCombatSkills")]
        public static bool UI_CharacterMenuEquipCombatSkill_UnequipAllCombatSkills_Prefix(UI_CharacterMenuEquipCombatSkill __instance)
        {
            if (DisablePatch(__instance)) return true;

            CharacterDomainHelper.MethodCall.UnequipAllCombatSkills(__instance.CharacterMenu.CurCharacterId);
            
            __instance.CallPrivateMethod("RequireGridCounts");
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UI_CharacterMenuEquipCombatSkill), "AutoLoadCombatSkills")]
        public static bool UI_CharacterMenuEquipCombatSkill_AutoLoadCombatSkills_Prefix(UI_CharacterMenuEquipCombatSkill __instance)
        {
            if (DisablePatch(__instance)) return true;

            CharacterDomainHelper.MethodCall.AutoEquipCombatSkills(__instance.CharacterMenu.CurCharacterId);
            __instance.CallPrivateMethod("RequireGridCounts");
            return false;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(UI_CharacterMenuEquipCombatSkill), "UpdateGenericGrid")]
        public static void UI_CharacterMenuEquipCombatSkill_UpdateGenericGrid_Prefix(UI_CharacterMenuEquipCombatSkill __instance, EquipCombatSkillMonitor ____dataMonitor, sbyte[] ____specificGridCount, byte[] ____genericGridAllocation, sbyte ____totalGenericGrid, RectTransform ____slotTypeHolder)
        {
            if (DisablePatch(__instance))
            {
                for (sbyte b = 0; b < 5; b = (sbyte)(b + 1))
                {
                    sbyte type = b;

                    Refers component = ____slotTypeHolder.GetChild(b).GetComponent<Refers>();
                    RectTransform rectTransform = component.CGet<RectTransform>("SlotHolder");
                    CButton cButton = component.CGet<CButton>("AddGenericGrid");
                    CButton cButton2 = component.CGet<CButton>("ReduceGenericGrid");

                    component.CGet<CButton>("AddGenericGrid").ClearAndAddListener(delegate
                    {
                        TaiwuDomainHelper.MethodCall.AllocateGenericGrid(type);
                        TaiwuDomainHelper.MethodCall.GetGenericGridAllocation(__instance.Element.GameDataListenerId);
                        __instance.CallPrivateMethod("RequestLearnedSkillDisplayData");
                    });
                    component.CGet<CButton>("ReduceGenericGrid").ClearAndAddListener(delegate
                    {
                        TaiwuDomainHelper.MethodCall.DeallocateGenericGrid(type);
                        TaiwuDomainHelper.MethodCall.GetGenericGridAllocation(__instance.Element.GameDataListenerId);
                        __instance.CallPrivateMethod("RequestLearnedSkillDisplayData");

                    });
                }
                return;
            }

            for (sbyte b = 0; b < 5; b = (sbyte)(b + 1))
            {
                sbyte type = b;

                Refers component = ____slotTypeHolder.GetChild(b).GetComponent<Refers>();
                RectTransform rectTransform = component.CGet<RectTransform>("SlotHolder");
                CButton cButton = component.CGet<CButton>("AddGenericGrid");
                CButton cButton2 = component.CGet<CButton>("ReduceGenericGrid");

                component.CGet<CButton>("AddGenericGrid").ClearAndAddListener(delegate
                {
                    __instance.CallPrivateMethod("RequestLearnedSkillDisplayData");
                    ____genericGridAllocation[type - 1] = (byte)(____genericGridAllocation[type - 1] + 1);
                    __instance.CallPrivateMethod("UpdateGenericGrid");
                });
                component.CGet<CButton>("ReduceGenericGrid").ClearAndAddListener(delegate
                {
                    __instance.CallPrivateMethod("RequestLearnedSkillDisplayData");
                    ____genericGridAllocation[type - 1] = (byte)(____genericGridAllocation[type - 1] - 1);
                    __instance.CallPrivateMethod("UpdateGenericGrid");
                });
            }
        }


        private static void FakeGetGenericGridAllocation(
            UI_CharacterMenuEquipCombatSkill __instance, 
            EquipCombatSkillMonitor ____dataMonitor, 
            sbyte[] ____specificGridCount, 
            byte[] ____genericGridAllocation, 
            sbyte ____totalGenericGrid
        )
        {
            var equippedSkillGridCount = new sbyte[5];
            // 重新生成分配
            for (sbyte b = 0; b < 5; b = (sbyte)(b + 1))
            {
                int num2 = GameData.Domains.Character.CombatSkillHelper.MaxSlotCounts[b];
                int num3 = ____specificGridCount[b];

                for (sbyte b2 = 0; b2 < num2; b2 = (sbyte)(b2 + 1))
                {
                    short equippedSkill = GameData.Domains.Character.CombatSkillHelper.GetEquippedSkill(____dataMonitor.EquippedCombatSkills, b, b2);
                    if (equippedSkill >= 0)
                    {
                        var gridCount = SingletonObject.getInstance<CombatSkillModel>().Get(__instance.CharacterMenu.CurCharacterId, equippedSkill).GridCount;
                        for (int j = 0; j < gridCount - 1; j++)
                        {
                            b2 = (sbyte)(b2 + 1);
                        }
                        equippedSkillGridCount[b] += gridCount;
                    }
                }

                if (b > 0)
                {
                    sbyte diff = (sbyte)(____specificGridCount[b] - equippedSkillGridCount[b]);
                    if (diff < 0)
                    {
                        if (____totalGenericGrid + diff >= 0)
                        {
                            ____genericGridAllocation[b - 1] = (byte)-diff;
                            ____totalGenericGrid += diff;
                        }
                        else
                        {
                            ____genericGridAllocation[b - 1] = (byte)____totalGenericGrid;
                            ____totalGenericGrid = 0;
                        }
                    }
                    else
                    {
                        ____genericGridAllocation[b - 1] = 0;
                    }
                }
            }
        }


        private static bool DisablePatch(UI_CharacterMenuEquipCombatSkill __instance) 
        {
            return !__instance.CharacterMenu.IsTaiwuTeam || __instance.CharacterMenu.CurCharacterId == SingletonObject.getInstance<BasicGameData>().TaiwuCharId;
        }
    }
}
