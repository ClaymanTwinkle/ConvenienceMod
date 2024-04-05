
using FrameWork;
using GameData.GameDataBridge;
using HarmonyLib;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using GameData.Domains.Taiwu;
using UnityEngine.Events;
using System.Collections.Generic;
using GameData.Utilities;
using Config;
using ConvenienceFrontend.MergeBookPanel;
using System.Linq;

namespace ConvenienceFrontend.BetterReading
{
    internal class BetterReadingFrontPatch : BaseFrontPatch
    {
        private static bool _isInitUI = false;

        private static List<GameObject> gameObjects = new List<GameObject>();

        public override void OnModSettingUpdate(string modIdStr)
        {
        }

        public override void Initialize(Harmony harmony, string modIdStr)
        {
            base.Initialize(harmony, modIdStr);
            _isInitUI = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_ReadingEvent), "OnInit")]
        public static void UI_ReadingEvent_OnInit_Postfix(UI_ReadingEvent __instance)
        {
            if (_isInitUI)
            {
                if (gameObjects.All(x => x != null))
                {
                    return;
                }
            }
            _isInitUI = true;
            gameObjects.Clear();
            gameObjects.Add(
                        CreateRandomButtn(__instance, "1", "灵光\n再闪", new Vector3(0f, 100f, 0f), delegate ()
                        {
                            TaiwuDomainHelper.MethodCall.GetRandomSelectableStrategies(__instance.Element.GameDataListenerId, Traverse.Create(__instance).Field("_curPage").GetValue<byte>());
                        })
                );

            gameObjects.Add(
                CreateRandomButtn(__instance, "2", "寻寻\n猫", new Vector3(100f, 100f, 0f), delegate ()
                {
                    var traverse = Traverse.Create(__instance);
                    List<byte> oldSelectableStrategies = traverse.Field<List<byte>>("_selectableStrategies").Value;
                    if (oldSelectableStrategies == null) return;
                    // 4 照猫画虎
                    // 6 寻章摘句
                    // 10 枕经席文
                    List<byte> newSelectableStrategies = new List<byte> { 6, 6, 4 };
                    for (int i = 0; i < oldSelectableStrategies.Count; i++)
                    {
                        if (i >= newSelectableStrategies.Count)
                        {
                            oldSelectableStrategies[i] = 10;
                        }
                        else
                        {
                            oldSelectableStrategies[i] = newSelectableStrategies[i];
                        }
                    }

                    traverse.Field<List<byte>>("_selectableStrategies").Value = oldSelectableStrategies;

                    traverse.Method("RenderSelectableStrategies").GetValue();
                })
                );
        }

        private static GameObject CreateRandomButtn(UI_ReadingEvent __instance, string tag, string text, Vector3 localPosition, UnityAction onClick)
        {
            GameObject gameObject = Object.Instantiate<GameObject>(__instance.transform.Find("MainWindow/StrategyHolder/StrategyToggle_0/Bg").gameObject);
            gameObject.name = "RandomBtn_" + tag;
            Image component = gameObject.GetComponent<Image>();
            component.raycastTarget = true;
            CButton cbutton = gameObject.AddComponent<CButton>();
            GameObject gameObject2 = Object.Instantiate<GameObject>(__instance.transform.Find("MainWindow/StrategyHolder/StrategyToggle_0/Name").gameObject);
            gameObject2.name = "Name_" + tag;
            gameObject2.transform.SetParent(gameObject.transform, false);
            TextMeshProUGUI component2 = gameObject2.GetComponent<TextMeshProUGUI>();
            component2.text = text;
            gameObject.transform.SetParent(__instance.transform.Find("MainWindow/StrategyHolder"), false);
            gameObject.transform.localPosition = localPosition;
            cbutton.onClick.AddListener(onClick);

            return gameObject;
        }
    }
}
