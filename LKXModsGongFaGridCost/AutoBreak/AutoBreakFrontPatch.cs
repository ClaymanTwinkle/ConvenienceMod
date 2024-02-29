using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Config;
using ConvenienceFrontend.CombatStrategy;
using FrameWork;
using FrameWork.ModSystem;
using GameData.Domains.CombatSkill;
using GameData.Domains.Taiwu;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace ConvenienceFrontend.AutoBreak
{
    internal class AutoBreakFrontPatch : BaseFrontPatch
    {
        private static bool _enableMod = true;
        private static CButton _autoBreakButton = null;

        public override void OnModSettingUpdate(string modIdStr)
        {

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_SkillBreakPlate), "OnInit")]
        public static void UI_Bottom_OnInit_Postfix(UI_SkillBreakPlate __instance)
        {
            var _isReview = Traverse.Create(__instance).Field<bool>("_isReview").Value;

            if (_autoBreakButton != null)
            {
                _autoBreakButton.gameObject.SetActive(!_isReview && _enableMod);
                return;
            }

            Refers refers = __instance.CGet<CharacterAttributeDataView>("CharacterAttributeView");
            var parent = refers.gameObject.transform;

            _autoBreakButton = GameObjectCreationUtils.UGUICreateCButton(parent, new Vector2(0, -550), new Vector2(120, 50), 16, "自动突破");
            _autoBreakButton.ClearAndAddListener(delegate ()
            {
                var traverse = Traverse.Create(__instance);
                var DaysInCurrMonth = 30 - SingletonObject.getInstance<BasicGameData>().DaysInCurrMonth;
                if (DaysInCurrMonth < 10)
                {
                    UIUtils.ShowTips("本月天数不足", "自动突破需要消耗10天，本月天数（"+ DaysInCurrMonth + "）不足，不能自动突破");
                    return;
                }
                var _skillId = traverse.Field<short>("_skillId").Value;
                CombatSkillDisplayData skillData = SingletonObject.getInstance<CombatSkillModel>().Get(SingletonObject.getInstance<BasicGameData>().TaiwuCharId, _skillId);
                var _currentExp = traverse.Field<int>("_currentExp").Value;
                if (_currentExp < skillData.NewUnderstandingNeedExp)
                {
                    UIUtils.ShowTips("历练不足", "自动突破需要消耗"+ skillData.NewUnderstandingNeedExp + "，当前历练（"+ _currentExp +"）不足，不能自动突破");
                }

                var _selectedPages = traverse.Field<ushort>("_selectedPages").Value;

                // GameData.GameDataBridge.GameDataBridge.AddMethodCall(-1, 5, 2024);
                TaiwuDomainHelper.MethodCall.AutoBreakOut(__instance.Element.GameDataListenerId, _skillId, _selectedPages);
            });
            _autoBreakButton.gameObject.SetActive(!_isReview && _enableMod);
        }
    }
}
