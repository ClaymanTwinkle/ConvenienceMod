using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Config;
using ConvenienceFrontend.CombatStrategy;
using ConvenienceFrontend.ModifyCombatSkill.Data;
using FrameWork;
using FrameWork.ModSystem;
using GameData.Domains.CombatSkill;
using GameData.Utilities;
using HarmonyLib;
using TaiwuModdingLib.Core.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ConvenienceFrontend.ModifyCombatSkill
{
    internal class ModifyCombatSkillFrontPatch : BaseFrontPatch
    {
        private static bool enableBladeAndSwordDoubleJue = false;

        private static CButton _fusionCombatSkillButton = null;

        public override void OnModSettingUpdate(string modIdStr)
        {
            ModManager.GetSetting(modIdStr, "enable_BladeAndSwordDoubleJue", ref enableBladeAndSwordDoubleJue);
            UpdateSkillEffectDesc();
        }


        private static void UpdateSkillEffectDesc()
        {
            var flag = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.SetField | BindingFlags.SetProperty | BindingFlags.DeclaredOnly;
            if (enableBladeAndSwordDoubleJue)
            {
                AdaptableLog.Info("真刀剑双绝");

                ReflectionExtensions.ModifyField(SpecialEffect.Instance[723], "Desc", new string[] { "此功法发挥十成威力时，运用者后退2距离，如果敌人仍在运用者的攻击范围内，运用者从施展进度50%开始立即无消耗施展另一个品阶不超过此功法的「剑法」攻击敌人" }, flag);
                ReflectionExtensions.ModifyField(SpecialEffect.Instance[1449], "Desc", new string[] { "此功法发挥十成威力时，运用者前进2距离，如果敌人仍在运用者的攻击范围内，运用者从施展进度50%开始立即无消耗施展另一个品阶不超过此功法的「剑法」攻击敌人" }, flag);

                ReflectionExtensions.ModifyField(SpecialEffect.Instance[715], "Desc", new string[] { "此功法发挥十成威力时，运用者前进2距离，如果敌人仍在运用者的攻击范围内，运用者从施展进度50%开始立即无消耗施展另一个品阶不超过此功法的「刀法」攻击敌人" }, flag);
                ReflectionExtensions.ModifyField(SpecialEffect.Instance[1441], "Desc", new string[] { "此功法发挥十成威力时，运用者后退2距离，如果敌人仍在运用者的攻击范围内，运用者从施展进度50%开始立即无消耗施展另一个品阶不超过此功法的「刀法」攻击敌人" }, flag);
            }
            else
            {
                ReflectionExtensions.ModifyField(SpecialEffect.Instance[723], "Desc", new string[] { "此功法未发挥十成威力时，运用者后退2距离，如果敌人仍在运用者的攻击范围内，运用者从施展进度50%开始立即无消耗施展另一个品阶不超过此功法的「剑法」攻击敌人" }, flag);
                ReflectionExtensions.ModifyField(SpecialEffect.Instance[1449], "Desc", new string[] { "此功法未发挥十成威力时，运用者前进2距离，如果敌人仍在运用者的攻击范围内，运用者从施展进度50%开始立即无消耗施展另一个品阶不超过此功法的「剑法」攻击敌人" }, flag);

                ReflectionExtensions.ModifyField(SpecialEffect.Instance[715], "Desc", new string[] { "此功法未发挥十成威力时，运用者前进2距离，如果敌人仍在运用者的攻击范围内，运用者从施展进度50%开始立即无消耗施展另一个品阶不超过此功法的「刀法」攻击敌人" }, flag);
                ReflectionExtensions.ModifyField(SpecialEffect.Instance[1441], "Desc", new string[] { "此功法未发挥十成威力时，运用者后退2距离，如果敌人仍在运用者的攻击范围内，运用者从施展进度50%开始立即无消耗施展另一个品阶不超过此功法的「刀法」攻击敌人" }, flag);
            }
        }

        ///// <summary>
        ///// 修习功法面板，增加融合功法入口
        ///// </summary>
        ///// <param name="__instance"></param>
        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(UI_CharacterMenuPractice), "OnInit")]
        //public static void UI_CharacterMenuPractice_OnInit_Postfix(UI_CharacterMenuPractice __instance)
        //{
        //    if (!ConvenienceFrontend.IsLocalTest()) return;
        //    if (_fusionCombatSkillButton != null) return;

        //    var _practiceRefers = __instance.CGet<Refers>("Practice");
        //    CButton cButton = _practiceRefers.CGet<CButton>("PracticeBtn");

        //    _fusionCombatSkillButton = GameObjectCreationUtils.UGUICreateCButton(cButton.gameObject.transform.parent, new Vector2(cButton.transform.position.x, -380), new Vector2(160, 50), 18f, "融合功法");
        //    _fusionCombatSkillButton.GetComponentInChildren<TextMeshProUGUI>().fontStyle = cButton.GetComponentInChildren<TextMeshProUGUI>().fontStyle;
        //    _fusionCombatSkillButton.GetComponentInChildren<TextMeshProUGUI>().fontSize = cButton.GetComponentInChildren<TextMeshProUGUI>().fontSize;
        //    _fusionCombatSkillButton.GetComponentInChildren<TextMeshProUGUI>().color = cButton.GetComponentInChildren<TextMeshProUGUI>().color;
        //    UIUtils.ShowMouseTipDisplayer(_fusionCombatSkillButton.gameObject, "以当前功法为主功法，选择副功法进行融合");
        //    _fusionCombatSkillButton.gameObject.SetActive(false);

        //    Traverse traverse = new Traverse(__instance);
        //    _fusionCombatSkillButton.ClearAndAddListener(delegate ()
        //    {
        //        var filterCombatSkillIdList = FilterAllCanFusionCombatSkillIdList(__instance);

        //        if (filterCombatSkillIdList.Count == 0) 
        //        {
        //            UIUtils.ShowTips("提示", "暂无可以融合的功法！");
        //            return;
        //        }

        //        var selectSkillArgBox = EasyPool.Get<ArgumentBox>();
        //        selectSkillArgBox.Set("ShowCombatSkill", true);
        //        selectSkillArgBox.Set("ShowLifeSkill", false);
        //        selectSkillArgBox.Set("CheckEquipRequirePracticeLevel", false);
        //        selectSkillArgBox.SetObject("UnselectableCombatSkillList", new List<short>());
        //        selectSkillArgBox.Set("ShowNone", false);

        //        var _onSelected = new Action<sbyte, short>((sbyte type, short skillId) => {
        //            if (skillId > -1 && type == 1)
        //            {
        //                var _currPracticeSelectedSkillId = traverse.Field<short>("_currPracticeSelectedSkillId").Value;
        //                UI_FusionCombatSkill.ShowUI(_currPracticeSelectedSkillId, skillId);
        //            }
        //        });

        //        selectSkillArgBox.Set("CharId", SingletonObject.getInstance<BasicGameData>().TaiwuCharId);
        //        selectSkillArgBox.SetObject("Callback", _onSelected);
        //        // selectSkillArgBox.Set("PrevCombatSkillId", selectedSkillId);
        //        selectSkillArgBox.SetObject("CombatSkillIdList", filterCombatSkillIdList);
        //        UIElement.SelectSkill.SetOnInitArgs(selectSkillArgBox);
        //        UIManager.Instance.ShowUI(UIElement.SelectSkill);
        //    });
        //}

        ///// <summary>
        ///// 用于刷新融合功法按钮是否显示
        ///// </summary>
        ///// <param name="__instance"></param>
        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(UI_CharacterMenuPractice), "UpdateCurrPracticeSkillInfo")]
        //public static void UI_CharacterMenuPractice_UpdateCurrPracticeSkillInfo_Postfix(UI_CharacterMenuPractice __instance)
        //{
        //    if (_fusionCombatSkillButton == null) return;

        //    Traverse traverse = new Traverse(__instance);
        //    var _currPracticeSelectedSkillId = traverse.Field<short>("_currPracticeSelectedSkillId").Value;

        //    if (_currPracticeSelectedSkillId == -1)
        //    {
        //        _fusionCombatSkillButton.gameObject.SetActive(false);
        //        return;
        //    }

        //    CombatSkillDisplayData combatSkillDisplayData = traverse.Field<Dictionary<short, CombatSkillDisplayData>>("_displayDataDict").Value[_currPracticeSelectedSkillId];
        //    var isBrokenOut = CombatSkillStateHelper.IsBrokenOut(combatSkillDisplayData.ActivationState);
        //    if (!isBrokenOut)
        //    {
        //        // 没突破的功法不行
        //        _fusionCombatSkillButton.gameObject.SetActive(false);
        //        return;
        //    }

        //    var combatSkillItem = Config.CombatSkill.Instance[_currPracticeSelectedSkillId];
        //    var isAttackSkill = combatSkillItem.EquipType == CombatSkillEquipType.Attack;
        //    if (!isAttackSkill)
        //    {
        //        // 只支持催迫
        //        _fusionCombatSkillButton.gameObject.SetActive(false);
        //        return;
        //    }

        //    _fusionCombatSkillButton.gameObject.SetActive(true);
        //}

        ///// <summary>
        ///// 过滤所有能融合的功法
        ///// </summary>
        ///// <param name="__instance"></param>
        ///// <returns></returns>
        //private static List<short> FilterAllCanFusionCombatSkillIdList(UI_CharacterMenuPractice __instance)
        //{
        //    Traverse traverse = new Traverse(__instance);
        //    var _currPracticeSelectedSkillId = traverse.Field<short>("_currPracticeSelectedSkillId").Value;
        //    var _allCombatSkillList = traverse.Field<List<CombatSkillDisplayData>>("_allCombatSkillList").Value;
        //    var combatSkillItem = Config.CombatSkill.Instance[_currPracticeSelectedSkillId];

        //    var filterCombatSkillList = _allCombatSkillList.FindAll(x => 
        //        x.TemplateId != _currPracticeSelectedSkillId && 
        //        CombatSkillStateHelper.IsBrokenOut(x.ActivationState) && 
        //        Config.CombatSkill.Instance[x.TemplateId].EquipType == CombatSkillEquipType.Attack &&
        //        Config.CombatSkill.Instance[x.TemplateId].Grade <= combatSkillItem.Grade &&
        //        Config.CombatSkill.Instance[x.TemplateId].Type == combatSkillItem.Type
        //    );

        //    return filterCombatSkillList.ConvertAll(x => x.TemplateId);
        //}
    }
}
