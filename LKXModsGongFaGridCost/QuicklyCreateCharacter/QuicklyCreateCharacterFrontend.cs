using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using static LifeSkillCombatLogic;
using TaiwuModdingLib.Core.Plugin;
using UnityEngine.Events;
using UnityEngine;
using GameData.Domains.Character;
using GameData.GameDataBridge;
using GameData.Serializer;
using Newtonsoft.Json;

namespace ConvenienceFrontend.QuicklyCreateCharacter
{
    internal class QuicklyCreateCharacterFrontend : BaseFrontPatch
    {
        public override void OnModSettingUpdate(string modIdStr)
        {
            ModManager.GetSetting(modIdStr, "Toggle_SLRole", ref QuicklyCreateCharacterFrontend.bool_Toggle_Total);
        }

        public override void Initialize(Harmony harmony, string modIdStr)
        {
            base.Initialize(harmony, modIdStr);
            QuicklyCreateCharacterFrontend.bool_IsEnterNewGame = false;
        }

        public override void OnEnterNewWorld()
        {
            base.OnEnterNewWorld();
        }

        // Token: 0x06000003 RID: 3 RVA: 0x00002088 File Offset: 0x00000288
        public override void Dispose()
        {
            if (QuicklyCreateCharacterFrontend.emptyGo != null)
            {
                Object.Destroy(QuicklyCreateCharacterFrontend.emptyGo);
            }
            if (QuicklyCreateCharacterFrontend.UGUIGo != null)
            {
                Object.Destroy(QuicklyCreateCharacterFrontend.UGUIGo);
            }
            QuicklyCreateCharacterFrontend.bool_IsEnterNewGame = false;
        }

        // Token: 0x06000004 RID: 4 RVA: 0x000020F0 File Offset: 0x000002F0
        public override void OnLoadedArchiveData()
        {
            if (QuicklyCreateCharacterFrontend.emptyGo != null)
            {
                Object.Destroy(QuicklyCreateCharacterFrontend.emptyGo);
            }
            if (QuicklyCreateCharacterFrontend.UGUIGo != null)
            {
                Object.Destroy(QuicklyCreateCharacterFrontend.UGUIGo);
            }
            QuicklyCreateCharacterFrontend.bool_IsEnterNewGame = false;
        }

        // Token: 0x06000005 RID: 5 RVA: 0x0000213C File Offset: 0x0000033C
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_NewGame), "Awake")]
        public static void UI_NewGame_Awake_PostPatch(UI_NewGame __instance)
        {
            if (!QuicklyCreateCharacterFrontend.bool_Toggle_Total) return;

            QuicklyCreateCharacterFrontend.bool_IsEnterNewGame = true;

            CToggleGroup ctoggleGroup = __instance.CGet<CToggleGroup>("SwitchMode");
            Canvas componentInParent = ctoggleGroup.transform.GetComponentInParent<Canvas>();
            QuicklyCreateCharacterFrontend.UGUIGo = new GameObject("mainWindowGoForQCCF");
            QuicklyCreateCharacterFrontend.dataController_Instance = QuicklyCreateCharacterFrontend.UGUIGo.AddComponent<CharacterDataController>();
            QuicklyCreateCharacterFrontend.dataController_Instance.UI_NewGame_Member = __instance;
            RollAttributeWindow rollAttributeWindow = QuicklyCreateCharacterFrontend.UGUIGo.AddComponent<RollAttributeWindow>();
            rollAttributeWindow.characterDataController = QuicklyCreateCharacterFrontend.dataController_Instance;
            rollAttributeWindow.SetRootCanvas(componentInParent);

            QuicklyCreateCharacterFrontend.guideGo = UIFactory.GetCommonButtonGo("人物属性", new UnityAction(rollAttributeWindow.Open), false);
            QuicklyCreateCharacterFrontend.guideGo.transform.SetParent(ctoggleGroup.transform, false);
            Vector2 sizeDelta = QuicklyCreateCharacterFrontend.guideGo.transform.GetComponent<RectTransform>().sizeDelta;
            Vector2 vector = new Vector2(500f, 500f) + sizeDelta / 2f - new Vector2(0f, sizeDelta.y);
            QuicklyCreateCharacterFrontend.guideGo.transform.localPosition = Vector3.zero;
            QuicklyCreateCharacterFrontend.guideGo.name = "guideGoForQCCF";
        }

        // Token: 0x06000006 RID: 6 RVA: 0x0000226C File Offset: 0x0000046C
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_NewGame), "UpdateScrolls")]
        public static void UI_NewGame_UpdateScrolls_PostPatch(UI_NewGame __instance)
        {
            if (!QuicklyCreateCharacterFrontend.bool_Toggle_Total) return;

            if (QuicklyCreateCharacterFrontend.bool_IsEnterNewGame)
            {
                try
                {
                    QuicklyCreateCharacterFrontend.dataController_Instance.DoRollCharacterData();
                }
                catch (System.Exception ex)
                { 
                }
            }
        }

        // Token: 0x06000007 RID: 7 RVA: 0x000022A4 File Offset: 0x000004A4
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_NewGame), "SetUseInscribedCharacter")]
        public static void UI_NewGame_SetUseInscribedCharacter_PostPatch(UI_NewGame __instance, bool useInscribedChar)
        {
            if (!QuicklyCreateCharacterFrontend.bool_Toggle_Total) return;

            if (QuicklyCreateCharacterFrontend.guideGo != null)
            {
                QuicklyCreateCharacterFrontend.guideGo.SetActive(!useInscribedChar);
            }

            //if (QuicklyCreateCharacterFrontend.bool_IsEnterNewGame)
            //{
            //    if (QuicklyCreateCharacterFrontend.emptyGo != null)
            //    {
            //        Object.Destroy(QuicklyCreateCharacterFrontend.emptyGo);
            //    }
            //    if (QuicklyCreateCharacterFrontend.UGUIGo != null)
            //    {
            //        Object.Destroy(QuicklyCreateCharacterFrontend.UGUIGo);
            //    }
            //}
        }

        // Token: 0x06000008 RID: 8 RVA: 0x00002308 File Offset: 0x00000508
        [HarmonyPrefix]
        [HarmonyPatch(typeof(UI_NewGame), "OnDestroy")]
        public static bool UI_NewGame_OnLoadFinish_PrePatch(UI_NewGame __instance)
        {
            if (!QuicklyCreateCharacterFrontend.bool_Toggle_Total) return true;
            if (!QuicklyCreateCharacterFrontend.bool_IsEnterNewGame) return true;


            if (QuicklyCreateCharacterFrontend.emptyGo != null)
            {
                Object.Destroy(QuicklyCreateCharacterFrontend.emptyGo);
            }
            if (QuicklyCreateCharacterFrontend.UGUIGo != null)
            {
                Object.Destroy(QuicklyCreateCharacterFrontend.UGUIGo);
            }
            return true;
        }

        // Token: 0x04000002 RID: 2
        private static bool bool_Toggle_Total;

        // Token: 0x04000003 RID: 3
        private static bool bool_IsEnterNewGame;

        // Token: 0x04000004 RID: 4
        private static GameObject guideGo;

        // Token: 0x04000005 RID: 5
        private static GameObject UGUIGo;

        // Token: 0x04000006 RID: 6
        private static GameObject emptyGo;

        // Token: 0x04000007 RID: 7
        private static CharacterDataController dataController_Instance;

        // Token: 0x04000008 RID: 8
        private static UIController uiController_Instance;
    }
}
