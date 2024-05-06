using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ConvenienceFrontend.CombatStrategy;
using DG.Tweening;
using FrameWork;
using FrameWork.ModSystem;
using GameData.Domains.Item;
using GameData.Domains.TaiwuEvent.DisplayEvent;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using Spine.Unity;
using TMPro;
using UnityEngine;

namespace ConvenienceFrontend.FastTaiwu
{
    internal class FastTaiwuFrontPatch : BaseFrontPatch
    {
        private static CToggle _markAutoSelectCToggle = null;

        private static bool AllowAccelerate => UIElement.CricketCombat.Ready || UIElement.CombatResult.Ready || UIElement.CricketCombatResult.Ready;

        public override void OnModSettingUpdate(string modIdStr)
        {
            // UI_CombatResult
        }

        /// <summary>
        /// 奇遇加速
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="____curCarrierTravelTimeReduction"></param>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(UI_AdventureInfo), "SetCarrierAnimation")]
        public static void UI_AdventureInfo_SetCarrierAnimation_Prefix(UI_AdventureInfo __instance, ref sbyte ____curCarrierTravelTimeReduction)
        {
            ____curCarrierTravelTimeReduction = SByte.MaxValue;
        }

        /// <summary>
        /// 跳过斗蛐蛐开头动画
        /// </summary>
        /// <param name="visible"></param>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(UI_CricketCombat), "RandomFirstMove")]
        public static bool UI_CricketCombat_RandomFirstMove_Prefix(UI_CricketCombat __instance)
        {
            CricketView firstMoveJudger = __instance.CGet<CricketView>("FirstMoveJudger");
            firstMoveJudger.gameObject.SetActive(value: false);

            __instance.CGet<RectTransform>("BattleInfo").gameObject.SetActive(value: true);
            var traverse = Traverse.Create(__instance);
            bool _selfFirstMove = traverse.Field<bool>("_selfFirstMove").Value;
            traverse.Method("CheckCanStartBattle").GetValue();
            if (_selfFirstMove)
            {
                traverse.Method("ShowCombatStateInfo", LocalStringManager.Get(1520), -1f, 5f).GetValue();
                SingletonObject.getInstance<YieldHelper>().DelaySecondsDo(0.5f, delegate
                {
                    ItemDomainHelper.MethodCall.GetWagerValueRange(__instance.Element.GameDataListenerId);
                });
            }
            else
            {
                traverse.Method("ShowCombatStateInfo", LocalStringManager.Get(1521), -1f, 5f).GetValue();
                SingletonObject.getInstance<YieldHelper>().DelaySecondsDo(0.5f, delegate
                {
                    traverse.Method("ShowCombatStateInfo", LocalStringManager.Get(1523), -1f, 5f).GetValue();
                    __instance.CGet<Refers>("EnemyInfos").CGet<GameObject>("WagerState").SetActive(value: true);
                    __instance.CGet<Refers>("EnemyInfos").CGet<TextMeshProUGUI>("WagerStateLabel").text = LocalStringManager.Get(1524);
                    SingletonObject.getInstance<YieldHelper>().DelaySecondsDo(1f, delegate
                    {
                        ItemDomainHelper.MethodCall.CalcEnemyWager(__instance.Element.GameDataListenerId);
                    });
                });
            }


            return false;
        }

        /// <summary>
        /// 回合结束后，马上执行回合二、三
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="button"></param>
        /// <param name="interactable"></param>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_CricketCombat), "SetButtonInteractable")]
        public static void UI_CricketCombat_SetButtonInteractable_Postfix(UI_CricketCombat __instance, CButton button, bool interactable)
        {
            if (__instance.CGet<CButton>("BtnStartCombat") == button && interactable)
            {
                var traverse = Traverse.Create(__instance);
                var _currRound = traverse.Field<int>("_currRound").Value;
                if (_currRound > 0)
                {
                    traverse.Method("OnClick", button).GetValue();
                }
            }
        }

        private static bool _isInCricketCombat = false;
        private static GEvent.Callback onConfirmQuitGameStateCallback = null;

        /// <summary>
        /// 促织战斗开始动画加速
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="____wagerTypeTogGroup"></param>
        /// <returns></returns>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(UI_CricketCombat), "OnEnable")]
        public static bool UI_CricketCombat_OnEnable_Postfix(UI_CricketCombat __instance, CToggleGroup ____wagerTypeTogGroup)
        {
            _isInCricketCombat = true;
            __instance.Element.ShowAfterRefresh();
            onConfirmQuitGameStateCallback = (ArgumentBox argBox) =>
            {
                Traverse.Create(__instance).Method("OnConfirmQuitGameState", argBox).GetValue();
            };
            GEvent.Add(EEvents.OnConfirmQuitGameState, onConfirmQuitGameStateCallback);
            CanvasGroup canvasGroup = __instance.CGet<CanvasGroup>("StartAnim");
            canvasGroup.DOFade(1f, 0.2f);
            canvasGroup.DOFade(0f, 0.2f).SetDelay(0.3f).OnComplete(delegate
            {
                ____wagerTypeTogGroup.Set(0, value: true, forceRaiseEvent: true);
            });
            CImage startBG = __instance.CGet<CImage>("StartBG");
            startBG.DOFade(1f, 0f).OnStart(delegate
            {
                startBG.raycastTarget = true;
            });
            startBG.DOFade(0f, 0.2f).SetDelay(0.3f).OnComplete(delegate
            {
                startBG.raycastTarget = false;
            });
            canvasGroup.GetComponentsInChildren<SkeletonGraphic>().ForEach(delegate (int i, SkeletonGraphic graphic)
            {
                graphic.AnimationState.SetAnimation(0, "animation", false);
                return false;
            });

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UI_CricketCombat), "OnDisable")]
        public static void UI_CricketCombat_OnDisable_Postfix(UI_CricketCombat __instance, CToggleGroup ____wagerTypeTogGroup)
        {
            _isInCricketCombat = false;
            if (onConfirmQuitGameStateCallback != null)
            {
                GEvent.Remove(EEvents.OnConfirmQuitGameState, onConfirmQuitGameStateCallback);
                onConfirmQuitGameStateCallback = null;
            }
        }

        /// <summary>
        /// 对话UI动画
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_EventWindow), "AnimEventWindowIn")]
        public static void UI_EventWindow_AnimEventWindowIn_Postfix(UI_EventWindow __instance)
        {
            __instance.WindowAnimDuration = 0;
        }

        /// <summary>
        /// 对话UI动画
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_EventWindow), "AnimEventWindowOut")]
        public static void UI_EventWindow_AnimEventWindowOut_Postfix(UI_EventWindow __instance)
        {
            __instance.WindowAnimDuration = 0;
        }

        private static JObject AutoSelectOptions => (JObject)ConvenienceFrontend.Config.GetValueSafe("AutoSelectOptions") ?? new JObject();
        private static bool _isSelectOption = false;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_EventWindow), "Update")]
        public static void UI_EventWindow_Update_Postfix(UI_EventWindow __instance)
        {
            if (_isSelectOption) return;
            var displayingEventData = SingletonObject.getInstance<EventModel>().DisplayingEventData;
            if (displayingEventData == null) return;
            var eventGuid = displayingEventData.EventGuid;

            if (AutoSelectOptions.ContainsKey(eventGuid))
            {
                // 自动点击
                if (displayingEventData.EventOptionInfos == null) return;

                // Debug.Log("自动选 " + AutoSelectOptions[eventGuid]);

                Traverse.Create(__instance).Method("SelectOptionByOptionKey", new object[]
                {
                   AutoSelectOptions[eventGuid].ToString()
                }).GetValue();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_EventWindow), "UpdateOptionScroll")]
        public static void UI_EventWindow_UpdateOptionScroll_Postfix(UI_EventWindow __instance)
        {
            if (_markAutoSelectCToggle != null && _markAutoSelectCToggle.gameObject != null)
            {
                var parent = _markAutoSelectCToggle.transform.parent.gameObject;
                UnityEngine.Object.Destroy(_markAutoSelectCToggle.gameObject);
                UnityEngine.Object.Destroy(parent);
                _markAutoSelectCToggle = null;
            }

            var displayingEventData = SingletonObject.getInstance<EventModel>().DisplayingEventData;
            if (displayingEventData == null) return;
            var eventGuid = displayingEventData.EventGuid;
            Debug.Log("UI_EventWindow " + eventGuid);

            if (AutoSelectOptions.ContainsKey(eventGuid)) return;

            TextMeshProUGUI contentTxt = __instance.CGet<TextMeshProUGUI>("EventContent");

            _markAutoSelectCToggle = UIUtils.CreateToggle(contentTxt.transform.parent, "markAutoSelectCToggle", "自动选", "打开后自己手动选择选项后，下次将自动帮你做出一样的选择");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UI_EventWindow), "SelectOption")]
        public static void UI_EventWindow_SelectOption_Prefix(UI_EventWindow __instance, EventOptionInfo optionInfo)
        {
            _isSelectOption = true;
            var displayingEventData = SingletonObject.getInstance<EventModel>().DisplayingEventData;
            if (displayingEventData == null) return;
            var eventGuid = displayingEventData.EventGuid;

            if (_markAutoSelectCToggle != null)
            {
                if (_markAutoSelectCToggle.isOn)
                {
                    // 标记
                    AutoSelectOptions[eventGuid] = optionInfo.OptionKey;
                    Debug.Log("标记 " + eventGuid + "=" + optionInfo.OptionKey);

                    ConvenienceFrontend.Config["AutoSelectOptions"] = AutoSelectOptions;
                    ConvenienceFrontend.SaveConfig(false);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_EventWindow), "SelectOption")]
        public static void UI_EventWindow_SelectOption_Postfix(UI_EventWindow __instance, EventOptionInfo optionInfo)
        {
            _isSelectOption = false;
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_CombatResult), "Update")]
        public static void UI_CombatResult_Update_Postfix(UI_CombatResult __instance)
        {
            __instance.QuickHide();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_CombatResult), "OnInit")]
        public static void UI_CombatResult_OnInit_Postfix(UI_CombatResult __instance)
        {
            UIElement element = __instance.Element;
            SkeletonGraphic resultAni = __instance.CGet<SkeletonGraphic>("ResultAni");
            CanvasGroup mainWindow = __instance.CGet<CanvasGroup>("MainWindow");
            CanvasGroup btnCanvas = __instance.CGet<CButton>("Close").GetComponent<CanvasGroup>();

            element.OnShowed = (Action)Delegate.Combine(element.OnShowed, new Action(delegate () {
                resultAni.DOComplete(true);
                mainWindow.DOComplete(true);
                btnCanvas.DOComplete(true);
            }));
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Sequence), "DoAppendInterval")]
        public static void Sequence_AppendInterval_Prefix(ref float interval)
        {
            if (AllowAccelerate)
            {
                interval /= 10;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Sequence), "DoPrependInterval")]
        public static void Sequence_DoPrependInterval_Prefix(ref float interval)
        {
            if (AllowAccelerate)
            {
                interval /= 10;
            }
        }
    }
}
