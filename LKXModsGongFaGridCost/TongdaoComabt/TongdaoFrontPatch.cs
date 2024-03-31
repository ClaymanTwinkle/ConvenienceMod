using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrameWork.ModSystem;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;
using HarmonyLib;
using UICommon.Character;
using UnityEngine;

namespace ConvenienceFrontend.TongdaoComabt
{
    internal class TongdaoFrontPatch : BaseFrontPatch
    {
        private static Dictionary<int, GameData.Utilities.ShortList> _tempCombatSkillOrderPlans = null;
        private static CButton[] _switchButton = null;

        public override void OnModSettingUpdate(string modIdStr)
        {
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UI_Combat), "HandlerMethodCombatDomain")]
        public static void UI_Combat_HandlerMethodCombatDomain_Prefix(List<int> ____selfTeam, int ____taiwuCharId, int ____selfCurrCharId, ref Dictionary<int, GameData.Utilities.ShortList> ____combatSkillOrderPlans, List<int> ____gettingProactiveSkillCharList)
        {
            if (____selfCurrCharId == ____selfTeam[0] && ____selfCurrCharId != ____taiwuCharId)
            {
                _tempCombatSkillOrderPlans = ____combatSkillOrderPlans;
                ____combatSkillOrderPlans = new Dictionary<int, GameData.Utilities.ShortList>();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_Combat), "HandlerMethodCombatDomain")]
        public static void UI_Combat_HandlerMethodCombatDomain_Postfix(List<int> ____selfTeam, int ____taiwuCharId, int ____selfCurrCharId, List<short> ____proactiveSkillList, List<int> ____gettingProactiveSkillCharList, ref Dictionary<int, GameData.Utilities.ShortList> ____combatSkillOrderPlans)
        {
            if (____combatSkillOrderPlans.Count == 0 && _tempCombatSkillOrderPlans != null)
            {
                ____combatSkillOrderPlans = _tempCombatSkillOrderPlans;
                _tempCombatSkillOrderPlans = null;
            }
        }



        // [HarmonyPostfix]
        // [HarmonyPatch(typeof(UI_Combat), "OnRenderProactiveSkill")]
        public static void UI_Combat_OnRenderProactiveSkill_Postfix(CombatSkillDisplayData skillData, CombatSkillView skillView)
        {
            Debug.Log("OnRenderProactiveSkill skillData = " + skillData.TemplateId);
        }

        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(UI_Combat), "UpdateAssistSkillList")]
        public static bool UI_Combat_UpdateAssistSkillList_Prefix(int ____selfCurrCharId, Dictionary<int, List<short>> ____assistSkillDict)
        {
            if (!____assistSkillDict.ContainsKey(____selfCurrCharId)) return false;

            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_CombatBegin), "Awake")]
        public static void UI_CombatBegin_Awake_Postfix(UI_CombatBegin __instance, CharacterAvatar[] ____selfAvatars, List<int> ____selfTeam)
        {
            Debug.Log("UI_CombatBegin Awake");

            // OnInit(ArgumentBox argsBox)
            if (_switchButton == null)
            {
                _switchButton = new CButton[____selfAvatars.Length - 1];
            }

            var _selfCharInfo = __instance.CGet<Refers>("SelfInfo");
            var _selfTeammateHolder = _selfCharInfo.CGet<RectTransform>("TeammateHolder");
            var position = _selfTeammateHolder.localPosition;

            for (var i = 0; i < _switchButton.Length; i++)
            {
                if (_switchButton[i] != null && _switchButton[i].gameObject != null) continue;
                _switchButton[i] = GameObjectCreationUtils.UGUICreateCButton(_selfTeammateHolder.parent, new Vector2(position.x - 100, position.y + 140 - (110 * i)), new Vector2(120, 50), 14, "同道代打");

                var index = i;
                _switchButton[i].ClearAndAddListener(delegate {
                    Debug.Log("UI_CombatBegin_Awake_Postfix click = " + index);
                    Debug.Log("UI_CombatBegin_Awake_Postfix ____selfTeam = " + String.Join(",", ____selfTeam));
                    var charId = ____selfTeam[index + 1];

                    (____selfTeam[index + 1], ____selfTeam[0]) = (____selfTeam[0], ____selfTeam[index + 1]);
                    Traverse.Create(__instance).Method("ClearElementAndMonitor").GetValue();
                    Traverse.Create(__instance).Method("InitElementAndMonitor").GetValue();
                    GameData.GameDataBridge.GameDataBridge.AddMethodCall(-1, 8, 24324, charId);
                });
            }
            // InitElementAndMonitor
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_CombatBegin), "InitElementAndMonitor")]
        public static void UI_CombatBegin_InitElementAndMonitor_Postfix(UI_CombatBegin __instance, CharacterAvatar[] ____selfAvatars)
        {
            Debug.Log("UI_CombatBegin InitElementAndMonitor");

            if (_switchButton != null)
            {
                for (var i = 0; i < _switchButton.Length; i++)
                {
                    _switchButton[i].gameObject.SetActive(____selfAvatars[i+1].CharacterId != -1);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_CombatBegin), "ClearElementAndMonitor")]
        public static void UI_CombatBegin_ClearElementAndMonitor_Postfix(UI_CombatBegin __instance, CharacterAvatar[] ____selfAvatars)
        {
            Debug.Log("UI_CombatBegin ClearElementAndMonitor");

            if (_switchButton != null)
            {
                for (var i = 0; i < _switchButton.Length; i++)
                {
                    _switchButton[i].gameObject.SetActive(false);
                }
            }
        }
    }
}
