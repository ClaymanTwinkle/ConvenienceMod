using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Config;
using FrameWork;
using GameData.Domains.Organization.Display;
using HarmonyLib;
using TaiwuModdingLib.Core.Utils;
using TMPro;
using UnityEngine;

namespace ConvenienceFrontend.ExchangeBook
{
    internal class ExchangeBookFrontPatch : BaseFrontPatch
    {

        // Token: 0x04000002 RID: 2
        private static short SettlementId;

        // Token: 0x04000003 RID: 3
        private static bool OnlyInSect;

        // Token: 0x04000004 RID: 4
        private static CButton exchangeCombatSkillBookBtn;

        // Token: 0x04000005 RID: 5
        private static CButton exchangeLifeSkillBookBtn;

        public override void Initialize(Harmony harmony, string modIdStr)
        {
            base.Initialize(harmony, modIdStr);

            UIBuilder.PrepareMaterial();
        }

        public override void OnModSettingUpdate(string modIdStr)
        {
        }

        // Token: 0x06000005 RID: 5 RVA: 0x000020A4 File Offset: 0x000002A4
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_SettlementInformation), "OnInit")]
        public static void UI_SettlementInformation_OnInit_Postfix(ArgumentBox argsBox, UI_SettlementInformation __instance)
        {
            bool flag = ExchangeBookFrontPatch.exchangeCombatSkillBookBtn == null;
            if (flag)
            {
                GameObject gameObject = __instance.transform.Find("AnimationRoot/BackGround/BackPanel/Supprot/ShowCombatSkillTree/").gameObject;
                GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject, gameObject.transform.parent);
                gameObject2.GetComponent<RectTransform>().anchoredPosition = new Vector2(300f, 16.8f);
                gameObject2.GetComponentInChildren<TextMeshProUGUI>().SetCharArray("门派换书".ToCharArray());
                gameObject2.GetComponentInChildren<TextMeshProUGUI>().SetAllDirty();
                ExchangeBookFrontPatch.exchangeCombatSkillBookBtn = gameObject2.GetComponent<CButton>();
                ExchangeBookFrontPatch.exchangeCombatSkillBookBtn.name = "ExchangeCombatSkillBook";
                ExchangeBookFrontPatch.exchangeCombatSkillBookBtn.ClearAndAddListener(delegate
                {
                    ExchangeBookFrontPatch.OnClick(__instance, true);
                });
                gameObject2.SetActive(!ExchangeBookFrontPatch.OnlyInSect);
            }
            bool flag2 = ExchangeBookFrontPatch.exchangeLifeSkillBookBtn == null;
            if (flag2)
            {
                GameObject gameObject3 = __instance.transform.Find("AnimationRoot/BackGround/BackPanel/Supprot/ShowCombatSkillTree/").gameObject;
                GameObject gameObject4 = UnityEngine.Object.Instantiate<GameObject>(gameObject3, gameObject3.transform.parent);
                gameObject4.GetComponent<RectTransform>().anchoredPosition = new Vector2(550f, 16.8f);
                gameObject4.GetComponentInChildren<TextMeshProUGUI>().SetCharArray("技艺换书".ToCharArray());
                gameObject4.GetComponentInChildren<TextMeshProUGUI>().SetAllDirty();
                ExchangeBookFrontPatch.exchangeLifeSkillBookBtn = gameObject4.GetComponent<CButton>();
                ExchangeBookFrontPatch.exchangeLifeSkillBookBtn.name = "ExchangeLifeSkillBook";
                ExchangeBookFrontPatch.exchangeLifeSkillBookBtn.ClearAndAddListener(delegate
                {
                    ExchangeBookFrontPatch.OnClick(__instance, false);
                });
                gameObject4.SetActive(!ExchangeBookFrontPatch.OnlyInSect);
            }
            bool onlyInSect = ExchangeBookFrontPatch.OnlyInSect;
            if (onlyInSect)
            {
                ExchangeBookFrontPatch.SettlementId = 0;
                if (argsBox != null)
                {
                    argsBox.Get("SettlementId", out ExchangeBookFrontPatch.SettlementId);
                }
                bool flag3 = ExchangeBookFrontPatch.SettlementId != 0;
                if (flag3)
                {
                    CButton cbutton = ExchangeBookFrontPatch.exchangeCombatSkillBookBtn;
                    if (cbutton != null)
                    {
                        cbutton.gameObject.SetActive(true);
                    }
                    CButton cbutton2 = ExchangeBookFrontPatch.exchangeLifeSkillBookBtn;
                    if (cbutton2 != null)
                    {
                        cbutton2.gameObject.SetActive(true);
                    }
                }
            }
        }

        // Token: 0x06000006 RID: 6 RVA: 0x000022B0 File Offset: 0x000004B0
        public static void OnClick(UI_SettlementInformation instance, bool isCombatSkill)
        {
            int curSettlementInDisplay = (int)instance.GetFieldValue("_curSettlementInDisplay");
            bool flag = curSettlementInDisplay != -1;
            if (flag)
            {
                List<SettlementDisplayData> enumerable = (List<SettlementDisplayData>)instance.GetFieldValue("_visitedSettlements");
                SettlementDisplayData settlementDisplayData = enumerable.First((SettlementDisplayData data) => data.SettlementId == curSettlementInDisplay);
                ArgumentBox argumentBox = EasyPool.Get<ArgumentBox>();
                argumentBox.Set("OrganizationId", curSettlementInDisplay);
                argumentBox.Set("OrganizationName", Organization.Instance[settlementDisplayData.OrgTemplateId].Name);
                argumentBox.Set("IsCombatSkill", isCombatSkill);
                UI_ExchangeBookPlus.GetUI().SetOnInitArgs(argumentBox);
                UIManager.Instance.ShowUI(UI_ExchangeBookPlus.GetUI());
            }
        }

        // Token: 0x06000007 RID: 7 RVA: 0x0000237C File Offset: 0x0000057C
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_SettlementInformation), "OnClickSettlement")]
        public static void UI_SettlementInformation_OnClickSettlement_Postfix(int ____curSettlementInDisplay)
        {
            bool onlyInSect = ExchangeBookFrontPatch.OnlyInSect;
            if (onlyInSect)
            {
                CButton cbutton = ExchangeBookFrontPatch.exchangeCombatSkillBookBtn;
                if (cbutton != null)
                {
                    cbutton.gameObject.SetActive(____curSettlementInDisplay == (int)ExchangeBookFrontPatch.SettlementId);
                }
                CButton cbutton2 = ExchangeBookFrontPatch.exchangeLifeSkillBookBtn;
                if (cbutton2 != null)
                {
                    cbutton2.gameObject.SetActive(____curSettlementInDisplay == (int)ExchangeBookFrontPatch.SettlementId);
                }
            }
        }
    }
}
