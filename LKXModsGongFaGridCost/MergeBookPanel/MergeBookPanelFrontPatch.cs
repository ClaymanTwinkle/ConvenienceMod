using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using TaiwuModdingLib.Core.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ConvenienceFrontend.MergeBookPanel
{
    internal class MergeBookPanelFrontPatch : BaseFrontPatch
    {
        public ModMono mono;

        public static bool EnableMergeBook;

        // Token: 0x04000004 RID: 4
        public static bool EnableOutlineTranform;

        // Token: 0x04000005 RID: 5
        public static bool EnableAllPagesTranform;

        // Token: 0x04000006 RID: 6
        public static bool EnableGenerateTwoBooks;

        // Token: 0x04000025 RID: 37
        public const int TogKeyItem = 3;

        // Token: 0x04000026 RID: 38
        public const int BookTogKey = 3;

        // Token: 0x04000027 RID: 39
        public const int TogKeyLifeSkill = 4;

        // Token: 0x04000028 RID: 40
        public const int LibararyTogKey = 3;

        public override void OnModSettingUpdate(string modIdStr)
        {
            ModManager.GetSetting(modIdStr, "EnableOutlineTranform", ref EnableOutlineTranform);
            ModManager.GetSetting(modIdStr, "EnableAllPagesTranform", ref EnableAllPagesTranform);
            // ModManager.GetSetting(modIdStr, "EnableGenerateTwoBooks", ref EnableGenerateTwoBooks);
            // ModManager.SaveModSettings();
        }

        public override void Initialize(Harmony harmony, string modIdStr)
        {
            base.Initialize(harmony, modIdStr);
            ModManager.GetSetting(modIdStr, "Toggle_MergeBook", ref EnableMergeBook);
            if (EnableMergeBook) 
            {
                this.mono = new GameObject("MergeBookPanel").AddComponent<ModMono>();
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            if (this.mono != null)
            {
                UnityEngine.Object.Destroy(this.mono.gameObject);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_CharacterMenu), "OnTabToggleChanged")]
        public static void OnTabToggleChanged_Patch(CToggle newTog, UI_CharacterMenu __instance)
        {
            if (!EnableMergeBook) return;

            GameLog.LogMessage("OnTabToggleChanged");
            if (!__instance.AllSubPages[newTog.Key].gameObject.activeSelf || __instance.CurCharacterId != SingletonObject.getInstance<BasicGameData>().TaiwuCharId)
            {
                return;
            }
            CToggleGroup subTogGroup = __instance.CGet<CToggleGroup>("SubTogGroup");
            RectTransform subTogHolder = subTogGroup.GetComponent<RectTransform>();
            CToggle subTogPrefab = subTogHolder.GetChild(0).GetComponent<CToggle>();
            if (newTog.Key == 3)
            {
                int newCount = 4 - subTogHolder.childCount;
                CToggle subTog = null;
                for (int i = 0; i < newCount; i++)
                {
                    subTog = UnityEngine.Object.Instantiate<CToggle>(subTogPrefab, subTogHolder, false);
                }
                if (subTog == null)
                {
                    subTog = subTogHolder.GetChild(3).GetComponent<CToggle>();
                }
                subTog.name = "BookSubTog";
                subTog.Key = 3;
                subTog.gameObject.SetActive(true);
                subTog.isOn = false;
                subTog.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "书籍";
                if (!subTogGroup.GetFieldValue<List<int>>("_keyList").Contains(3))
                {
                    try
                    {
                        subTogGroup.Remove(3);
                        subTogGroup.Add(subTog);
                    }
                    catch (Exception ex)
                    {
                        GameLog.LogMessage(ex.Message ?? "");
                    }
                    subTogGroup.InitPreOnToggle();
                }
                LayoutRebuilder.ForceRebuildLayoutImmediate(subTogGroup.GetComponent<RectTransform>());
                return;
            }
            int key = newTog.Key;
        }

        // Token: 0x06000027 RID: 39 RVA: 0x00004860 File Offset: 0x00002A60
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_CharacterMenuItems), "OnSwitchToSubpage")]
        public static void OnSwitchToSubpage_Items_Patch(int subPageIndex)
        {
            if (!EnableMergeBook) return;

            GameLog.LogMessage("OnSwitchToSubpage");
            ModMono.MergeBooks?.gameObject?.SetActive(subPageIndex == 3);
        }

        // Token: 0x06000028 RID: 40 RVA: 0x0000487F File Offset: 0x00002A7F
        [HarmonyPrefix]
        [HarmonyPatch(typeof(UI_CharacterMenuItems), "CanSubpageShow")]
        public static bool CanSubpageShow_Patch(int subPageIndex, ref bool __result, UI_CharacterMenuItems __instance, int ____taiwuCharId)
        {
            if (!EnableMergeBook) return true;

            GameLog.LogMessage("CanSubpageShow");
            if (subPageIndex == 3)
            {
                if (UIElement.CharacterMenu.UiBaseAs<UI_CharacterMenu>().CurCharacterId == ____taiwuCharId)
                {
                    __result = true;
                }
                __result = false;
                return false;
            }
            return true;
        }

        // Token: 0x06000029 RID: 41 RVA: 0x000048AC File Offset: 0x00002AAC
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_CharacterMenuItems), "OnCurrentCharacterChange")]
        public static void OnCurrentCharacterChange_Items_Patch(int prevCharacterId, UI_CharacterMenuItems __instance, int ____taiwuCharId)
        {
            if (!EnableMergeBook) return;

            GameLog.LogMessage("OnCurrentCharacterChange");
            UI_CharacterMenu charcterMenu = __instance.CharacterMenu;
            CToggleGroup subTogGroup = charcterMenu.CGet<CToggleGroup>("SubTogGroup");
            RectTransform subTogHolder = subTogGroup.GetComponent<RectTransform>();
            if (UI_CharacterMenu.CurSubPage == 3 && subTogHolder.childCount > 3)
            {
                if (subTogHolder.GetChild(3) == null)
                {
                    return;
                }
                CToggle mergeSubTog = subTogHolder.GetChild(3).GetComponent<CToggle>();
                CToggle itemSubTog = subTogHolder.GetChild(0).GetComponent<CToggle>();
                mergeSubTog.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "书籍";
                if (!subTogGroup.GetFieldValue<List<int>>("_keyList").Contains(3))
                {
                    subTogGroup.Add(mergeSubTog);
                }
                if (UIElement.CharacterMenuItems.UiBaseAs<UI_CharacterMenuItems>().CurTabIndex == 3)
                {
                    charcterMenu.CallPrivateMethod("OnSubToggleGroupChanged", new object[]
                    {
                        itemSubTog,
                        mergeSubTog
                    });
                }
                bool isTaiwu = charcterMenu.CurCharacterId == ____taiwuCharId;
                mergeSubTog.gameObject.SetActive(isTaiwu);
                ModMono.MergeBooks.gameObject.SetActive(false);
                mergeSubTog.isOn = false;
            }
        }

        // Token: 0x0600002A RID: 42 RVA: 0x000049A8 File Offset: 0x00002BA8
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MultiplyItemScrollView), "ShowMultiplySelectButton")]
        public static void ShowMultiplySelectButton_Patch(MultiplyItemScrollView __instance, CButton button)
        {
            var isTaiwu = __instance.CurCharId == SingletonObject.getInstance<BasicGameData>().TaiwuCharId;
            ModMono.mergeLifeBooksButton?.gameObject?.SetActive(isTaiwu);
            ModMono.mergeToolsButton?.gameObject?.SetActive(isTaiwu);
        }

        // Token: 0x0600002B RID: 43 RVA: 0x000049E4 File Offset: 0x00002BE4
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MultiplyItemScrollView), "HideMultiplySelectButton")]
        public static void HideMultiplySelectButton_Patch()
        {
            ModMono.mergeLifeBooksButton?.gameObject?.SetActive(false);
            ModMono.mergeToolsButton?.gameObject?.SetActive(false);
        }

    }
}
