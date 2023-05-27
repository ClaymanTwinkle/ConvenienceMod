using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CharacterDataMonitor;
using GameData.Domains.Extra;
using GameData.GameDataBridge;
using HarmonyLib;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ConvenienceFrontend.CombatStrategy
{
    // Token: 0x0200000E RID: 14
    public class UI_CombatBeginPatch
    {
        // Token: 0x0600003C RID: 60 RVA: 0x00004AA0 File Offset: 0x00002CA0
        private static bool CalSelectable(int index, bool isAlly, out int addNum)
        {
            bool flag = (isAlly ? UI_CombatBeginPatch._selfTeamToggles : UI_CombatBeginPatch._enemyTeamToggles)[index];
            addNum = ((CombatStrategyMod.Settings.CounterAccordance == 1) ? 1 : (isAlly ? UI_CombatBeginPatch._selfTeamConsumateLevel : UI_CombatBeginPatch._enemyTeamConsumateLevel)[index]);
            bool flag2 = (isAlly && flag) || (!isAlly && !flag);
            if (flag2)
            {
                addNum = -addNum;
            }
            return UI_CombatBeginPatch._count + addNum >= 0;
        }

        // Token: 0x0600003D RID: 61 RVA: 0x00004B14 File Offset: 0x00002D14
        private static void InitTeammateButton(GameObject teammate, bool[] toggles, int index, bool isAlly)
        {
            Refers refers = teammate.GetComponent<Refers>();
            teammate.transform.Find("Highlight").gameObject.SetActive(false);
            refers.CGet<TextMeshProUGUI>("Name").fontStyle = 0;
            CButton cbutton = refers.CGet<CButton>("Button");
            cbutton.enabled = true;
            cbutton.ClearAndAddListener(delegate ()
            {
                int num;
                bool flag2 = !UI_CombatBeginPatch.CalSelectable(index, isAlly, out num);
                if (!flag2)
                {
                    UI_CombatBeginPatch._count += num;
                    toggles[index] = !toggles[index];
                    refers.CGet<TextMeshProUGUI>("Name").fontStyle = (toggles[index] ? FontStyles.Strikethrough : 0);
                    teammate.transform.Find("Highlight").gameObject.SetActive(toggles[index]);
                }
            });
            bool flag = !refers.Names.Contains("ConsummateIcon");
            if (flag)
            {
                GameObject gameObject = Object.Instantiate<GameObject>(teammate.transform.parent.parent.Find("ConsummateIcon").gameObject, teammate.transform);
                gameObject.transform.localPosition = new Vector2((float)(isAlly ? -100 : 100), 0f);
                refers.AddMono(gameObject.GetComponent<CImage>(), "ConsummateIcon");
                refers.AddMono(gameObject.GetComponentInChildren<TextMeshProUGUI>(), "ConsummateLevel");
            }
            refers.CGet<CImage>("ConsummateIcon").gameObject.SetActive(CombatStrategyMod.Settings.CounterAccordance == 0);
        }

        // Token: 0x0600003E RID: 62 RVA: 0x00004C90 File Offset: 0x00002E90
        [HarmonyPrefix]
        [HarmonyPatch(typeof(UI_CombatBegin), "ShowCombatUi")]
        public static void UI_CombatBegin_ShowCombatUi_Prefix(List<int> ____selfTeam, List<int> ____enemyTeam)
        {
            List<int> list = new List<int>();
            for (int i = 1; i < ____selfTeam.Count; i++)
            {
                bool flag = UI_CombatBeginPatch._selfTeamToggles[i - 1];
                if (flag)
                {
                    list.Add(____selfTeam[i]);
                }
            }
            for (int j = 1; j < ____enemyTeam.Count; j++)
            {
                bool flag2 = UI_CombatBeginPatch._enemyTeamToggles[j - 1];
                if (flag2)
                {
                    list.Add(____enemyTeam[j]);
                }
            }
            bool flag3 = list.Count > 0;
            if (flag3)
            {
                GameDataBridge.AddMethodCall<ushort, List<int>>(-1, 8, CombatStrategyMod.MethodId, 4, list);
            }
            foreach (KeyValuePair<EquipCombatSkillMonitor, Action> keyValuePair in UI_CombatBeginPatch._monitorsDict)
            {
                keyValuePair.Key.RemoveConsummateLevelListener(keyValuePair.Value);
            }
            UI_CombatBeginPatch._monitorsDict.Clear();
        }

        // Token: 0x0600003F RID: 63 RVA: 0x00004D9C File Offset: 0x00002F9C
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_CombatBegin), "OnInit")]
        public static void UI_CombatBegin_OnInit_Postfix(UI_CombatBegin __instance)
        {
            Refers refers = __instance.CGet<Refers>("SelfInfo");
            Refers refers2 = __instance.CGet<Refers>("EnemyInfo");
            for (int i = 0; i < 3; i++)
            {
                UI_CombatBeginPatch._selfTeamToggles[i] = false;
                UI_CombatBeginPatch._enemyTeamToggles[i] = false;
            }
            UI_CombatBeginPatch._count = 0;
            UI_CombatBeginPatch._teammateHolder = refers.CGet<RectTransform>("TeammateHolder");
            UI_CombatBeginPatch._enemytTeammateHolder = refers2.CGet<RectTransform>("TeammateHolder");
            for (int j = 0; j < 3; j++)
            {
                GameObject gameObject = UI_CombatBeginPatch._teammateHolder.GetChild(j).gameObject;
                UI_CombatBeginPatch.InitTeammateButton(gameObject, UI_CombatBeginPatch._selfTeamToggles, j, true);
                GameObject gameObject2 = UI_CombatBeginPatch._enemytTeammateHolder.GetChild(j).gameObject;
                UI_CombatBeginPatch.InitTeammateButton(gameObject2, UI_CombatBeginPatch._enemyTeamToggles, j, false);
            }
        }

        // Token: 0x06000040 RID: 64 RVA: 0x00004E68 File Offset: 0x00003068
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_CombatBegin), "InitElementAndMonitor")]
        public static void UI_CombatBegin_InitElementAndMonitor_Postfix(List<int> ____selfTeam, List<int> ____enemyTeam)
        {
            bool flag = CombatStrategyMod.Settings.CounterAccordance == 1;
            if (!flag)
            {
                for (int i = 1; i < ____selfTeam.Count; i++)
                {
                    int index = i - 1;
                    EquipCombatSkillMonitor monitor = SingletonObject.getInstance<CharacterMonitorModel>().GetMonitorItem<EquipCombatSkillMonitor>(____selfTeam[i], 17, false);
                    Action action = delegate ()
                    {
                        UI_CombatBeginPatch.OnSelfConsummateLevelChange(index, true, monitor);
                    };
                    monitor.AddConsummateLevelListener(action);
                    UI_CombatBeginPatch._monitorsDict.Add(monitor, action);
                }
                for (int j = 1; j < ____enemyTeam.Count; j++)
                {
                    int index = j - 1;
                    EquipCombatSkillMonitor monitor = SingletonObject.getInstance<CharacterMonitorModel>().GetMonitorItem<EquipCombatSkillMonitor>(____enemyTeam[j], 17, false);
                    Action action2 = delegate ()
                    {
                        UI_CombatBeginPatch.OnSelfConsummateLevelChange(index, false, monitor);
                    };
                    monitor.AddConsummateLevelListener(action2);
                    UI_CombatBeginPatch._monitorsDict.Add(monitor, action2);
                }
            }
        }

        // Token: 0x06000041 RID: 65 RVA: 0x00004F78 File Offset: 0x00003178
        private static void OnSelfConsummateLevelChange(int index, bool isAlly, EquipCombatSkillMonitor monitor)
        {
            Refers component = (isAlly ? UI_CombatBeginPatch._teammateHolder : UI_CombatBeginPatch._enemytTeammateHolder).GetChild(index).gameObject.GetComponent<Refers>();
            int consummateLevel = (int)monitor.ConsummateLevel;
            (isAlly ? UI_CombatBeginPatch._selfTeamConsumateLevel : UI_CombatBeginPatch._enemyTeamConsumateLevel)[index] = ((consummateLevel == 0) ? 1 : (consummateLevel * 10));
            CImage cimage = component.CGet<CImage>("ConsummateIcon");
            component.CGet<TextMeshProUGUI>("ConsummateLevel").text = consummateLevel.ToString();
            cimage.SetSprite(string.Format("combat_icon_jingyuan_{0}", Mathf.Max(consummateLevel - 1, 0) / 2), false, null);
            cimage.GetComponent<MouseTipDisplayer>().PresetParam[0] = string.Format("{0}{1}{2}", LocalStringManager.Get(2126), LocalStringManager.Get(430), consummateLevel);
        }

        // Token: 0x0400005A RID: 90
        private static readonly bool[] _selfTeamToggles = new bool[3];

        // Token: 0x0400005B RID: 91
        private static readonly bool[] _enemyTeamToggles = new bool[3];

        // Token: 0x0400005C RID: 92
        private static readonly int[] _selfTeamConsumateLevel = new int[3];

        // Token: 0x0400005D RID: 93
        private static readonly int[] _enemyTeamConsumateLevel = new int[3];

        // Token: 0x0400005E RID: 94
        private static readonly Dictionary<EquipCombatSkillMonitor, Action> _monitorsDict = new Dictionary<EquipCombatSkillMonitor, Action>();

        // Token: 0x0400005F RID: 95
        private static RectTransform _teammateHolder;

        // Token: 0x04000060 RID: 96
        private static RectTransform _enemytTeammateHolder;

        // Token: 0x04000061 RID: 97
        private static int _count;
    }
}
