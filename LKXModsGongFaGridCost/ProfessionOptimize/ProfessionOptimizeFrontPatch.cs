using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvenienceFrontend.CustomWeapon;
using FrameWork;
using GameData.Domains.Item.Display;
using GameData.Domains.Item;
using GameData.GameDataBridge;
using GameData.Serializer;
using GameData.Utilities;
using HarmonyLib;
using UnityEngine;
using GameData.Domains.Extra;
using GameData.Domains.Taiwu.Profession;
using UnityEngine.UI;
using GameData.Domains.World;
using ConvenienceFrontend.CombatStrategy;
using System.Reflection;

namespace ConvenienceFrontend.ProfessionOptimize
{
    internal class ProfessionOptimizeFrontPatch : BaseFrontPatch
    {
        public override void OnModSettingUpdate(string modIdStr)
        {
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_Bottom), "RefreshProfession")]
        public static void UI_Bottom_RefreshProfession_Postfix(UI_Bottom __instance, Refers ____groupChar)
        {
            RectTransform rectTransform = ____groupChar.CGet<RectTransform>("ProfessionSkillLayout");
            ProfessionData professionData = SingletonObject.getInstance<ProfessionModel>().GetCurrentProfessionData();
            int taiwuCurrProfessionId = SingletonObject.getInstance<ProfessionModel>().TaiwuCurrProfessionId;
            if (professionData == null)
            {
                return;
            }

            // 平民一技能可以主动使用
            if (professionData.TemplateId == Config.Profession.DefKey.Civilian)
            {
                Transform child = rectTransform.GetChild(0);
                CButton btn = child.GetComponent<CButton>();
                btn.ClearAndAddListener(delegate
                {
                    // 消耗3天
                    if (SingletonObject.getInstance<TimeManager>().GetLeftDaysInCurrMonth() > 2)
                    {
                        WorldDomainHelper.MethodCall.AdvanceDaysInMonth(3);
                        var seniority = SingletonObject.getInstance<ProfessionModel>().GetCurrentProfessionData().Seniority;
                        ExtraDomainHelper.MethodCall.SetProfessionSeniorityCurrent(seniority + 100);
                    }
                    else
                    {
                        UIUtils.ShowTips("天数不足", "使用技能至少需要3天");
                    }
                });
                ExtraDomainHelper.AsyncMethodCall.CanExecuteProfessionSkill(__instance, taiwuCurrProfessionId, 0, delegate (int offset, RawDataPool dataPool)
                {
                    bool item = false;
                    Serializer.Deserialize(dataPool, offset, ref item);
                    btn.interactable = true;
                });
                btn.interactable = true;
            }
        }
    }
}
