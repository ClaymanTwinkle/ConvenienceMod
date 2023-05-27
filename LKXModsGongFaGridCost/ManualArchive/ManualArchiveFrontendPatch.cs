using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrameWork;
using GameData.Domains.Global;
using HarmonyLib;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ConvenienceFrontend.ManualArchive
{
    internal class ManualArchiveFrontendPatch : BaseFrontPatch
    {
        private static bool EnabledResortArchive = true;

        // Token: 0x0400000E RID: 14
        public static CButton BtnLoad;
        public static ModMono ModMono { get; set; }

        public override void OnModSettingUpdate(string modIdStr)
        {
            ModManager.GetSetting(modIdStr, "Bool_EnabledResortArchive", ref EnabledResortArchive);
        }

        public override void Initialize(Harmony harmony, string modIdStr)
        {
            base.Initialize(harmony, modIdStr);

            ManualArchiveFrontendPatch.ModMono = new GameObject("手动存档插件").AddComponent<ModMono>();

            GEvent.Add(UiEvents.OnUIElementShow, new GEvent.Callback(OnUI_SystemOptionElementShow));
            GEvent.Add(UiEvents.OnUIElementHide, new GEvent.Callback(OnUI_SystemSettingElementHide));
        }

        public override void Dispose()
        {
            base.Dispose();

            if (ModMono != null)
            {
                Object.Destroy(ModMono.gameObject);
            }

            GEvent.Remove(UiEvents.OnUIElementShow, new GEvent.Callback(OnUI_SystemOptionElementShow));
            GEvent.Remove(UiEvents.OnUIElementHide, new GEvent.Callback(OnUI_SystemSettingElementHide));
        }

        [HarmonyPatch(typeof(UI_RevertArchive), "OnArchiveItemRender")]
        [HarmonyPrefix]
        public static void ArchiveSortPrefix(UI_RevertArchive __instance, ref int index)
        {
            if (EnabledResortArchive)
            {
                ArchiveInfo value = Traverse.Create(__instance).Field("_archiveInfo").GetValue<ArchiveInfo>();
                index = value.BackupWorldsInfo.Count - index - 1;
            }
        }

        private static void OnUI_SystemOptionElementShow(ArgumentBox argbox)
        {
            UIElement uielement;
            if (argbox.Get<UIElement>("Element", out uielement))
            {
                if (uielement.Name == "UI_SystemOption")
                {
                    UI_SystemOption uiInstance = uielement.UiBaseAs<UI_SystemOption>();
                    if (uiInstance != null)
                    {
                        if (BtnLoad == null)
                        {
                            Debug.Log("开始创建存档按钮");
                            GameObject gameObject = uiInstance.transform.Find("MainWindow/ButtonHolder/SystemSetting").gameObject;
                            Transform parent = uiInstance.transform.Find("MainWindow");
                            BtnLoad = CreateBtn(gameObject, parent, "LoadGame", "快速读取", delegate
                            {
                                TryLoad();
                                uiInstance.QuickHide();
                            });
                            BtnLoad.transform.localPosition = new Vector3(21.9952f, -408f, 0f);
                        }
                    }
                }
            }
        }

        private static void OnUI_SystemSettingElementHide(ArgumentBox argbox)
        {

        }
        private static CButton CreateBtn(GameObject prefab, Transform parent, string name, string label, Action onClick)
        {
            GameObject gameObject = Object.Instantiate<GameObject>(prefab, parent, false);
            gameObject.name = name;
            TextMeshProUGUI component = gameObject.transform.Find("LabelBack/Label").GetComponent<TextMeshProUGUI>();
            component.text = label;
            CButton component2 = gameObject.GetComponent<CButton>();
            component2.onClick.RemoveAllListeners();
            component2.onClick.AddListener(delegate ()
            {
                onClick();
            });
            return component2;
        }

        private static bool CanQuickSaveOrLoad
        {
            get
            {
                bool flag = Game.Instance.GetCurrentGameStateName() == EGameState.InGame;
                bool flag2 = UIManager.Instance.IsInStack(UIElement.StateAdventure) || UIElement.Adventure.Exist || UIManager.Instance.IsInStack(UIElement.Combat) || UIElement.Combat.Exist;
                return flag && !flag2;
            }
        }

        private static void TryLoad()
        {
            bool flag = !CanQuickSaveOrLoad;
            if (!flag)
            {
                DialogCmd dialogCmd = new DialogCmd
                {
                    Title = "读取存档",
                    Content = "是否读取最新的存档？未保存的内容将会丢失。",
                    Type = 1,
                    Yes = new Action(QuickLoad)
                };
                UIElement.Dialog.SetOnInitArgs(EasyPool.Get<ArgumentBox>().SetObject("Cmd", dialogCmd));
                UIManager.Instance.ShowUI(UIElement.Dialog);
            }
        }

        private static void QuickLoad()
        {
            bool flag = !CanQuickSaveOrLoad;
            if (!flag)
            {
                UIManager.Instance.HideAll();
                LoadArchive(SingletonObject.getInstance<GlobalSettings>().LastEnterWorldIndex, -1L);
            }
        }

        public static void LoadArchive(sbyte index, long timestamp = -1L)
        {
            Action action = delegate ()
            {
                SingletonObject.getInstance<GlobalSettings>().HaveDoneSave = true;
                GameData.GameDataBridge.GlobalOperations.LoadWorld(index, timestamp);
                Game.Instance.ChangeGameState
                (
                    EGameState.Loading,
                    EasyPool.Get<ArgumentBox>().SetObject
                    (
                    "OnLoadingFinish",
                    new Action
                    (
                        () =>
                        {
                            Game.Instance.ChangeGameState(EGameState.InGame, null);
                            bool flag = SingletonObject.getInstance<BasicGameData>().CurrDate > 8;
                            bool flag2 = flag;
                            if (flag2)
                            {
                                UIElement.MonthNotify.SetOnInitArgs(EasyPool.Get<ArgumentBox>().Set("NeedSave", false));
                                UIManager.Instance.ShowUI(UIElement.MonthNotify);
                            }
                        }
                    )
                    )
                    .SetObject
                    (
                        "OnLoadingStart",
                        new Action
                        (
                            () =>
                            {
                                GEvent.OnEvent(UiEvents.OnUIElementHide, EasyPool.Get<ArgumentBox>().Set("Progress", 50));
                            }
                         )
                     )
                );
            };
            Game.ReturnToMainMenu(null, null, delegate ()
            {
                float f = 0.5f;

                ModMono.DelayInvoke(f, action);
            });
        }
    }
}
