using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData.Domains.Character;
using GameData.Domains.Item;
using System.Windows;
using HarmonyLib;
using TMPro;
using ConvenienceFrontend.TaiwuBuildingManager;
using FrameWork.ModSystem;
using FrameWork;
using UnityEngine;
using GameData.Domains.Item.Display;
using DG.Tweening;
using GameData.Utilities;
using Spine.Unity;

namespace ConvenienceFrontend.CricketCombatOptimize
{
    /// <summary>
    /// 蛐蛐战斗优化
    /// </summary>
    internal class CricketCombatOptimizeFrontPatch : BaseFrontPatch
    {
        private static CButton _oneClickSetCricketButton = null;

        public override void OnModSettingUpdate(string modIdStr)
        {
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_CricketCombat), "OnInit")]
        public static void UI_Bottom_OnInit_Postfix(UI_CricketCombat __instance)
        {
            if (_oneClickSetCricketButton != null)
            {
                _oneClickSetCricketButton.gameObject.SetActive(true);
                return;
            }

            var parent = __instance.CGet<Refers>("SelfInfos").gameObject.transform.parent;

            _oneClickSetCricketButton = GameObjectCreationUtils.UGUICreateCButton(parent, new Vector2(-1150, 0), new Vector2(200, 70), 16, "一键放置促织");
            _oneClickSetCricketButton.ClearAndAddListener(delegate ()
            {
                _oneClickSetCricketButton.gameObject.SetActive(false);

                OnClickSetCrickets(__instance);
            });
        }

        private static void OnClickSetCrickets(UI_CricketCombat __instance)
        {
            var traverse = Traverse.Create(__instance);
            List<ItemDisplayData> _canUseCricketList = traverse.Field<List<ItemDisplayData>>("_canUseCricketList").Value;
            ItemKey[] _selfCricketKeys = traverse.Field<ItemKey[]>("_selfCricketKeys").Value;
            Wager _selfWager = traverse.Field<Wager>("_selfWager").Value;
            sbyte _minGrade = traverse.Field<sbyte>("_minGrade").Value;
            sbyte _maxGrade = traverse.Field<sbyte>("_maxGrade").Value;
            bool _onlyNoInjuryCricket = traverse.Field<bool>("_onlyNoInjuryCricket").Value;
            Dictionary<ItemKey, short[]> _cricketDataDict = traverse.Field<Dictionary<ItemKey, short[]>>("_cricketDataDict").Value;

            List<ItemDisplayData> list = new List<ItemDisplayData>();
            foreach (ItemDisplayData canUseCricket in _canUseCricketList)
            {
                if (_selfWager.Type == 1 && canUseCricket.Key.Equals(_selfWager.ItemKey))
                {
                    continue;
                }

                sbyte cricketGrade = ItemTemplateHelper.GetCricketGrade(canUseCricket.CricketColorId, canUseCricket.CricketPartId);
                if (cricketGrade < _minGrade || cricketGrade > _maxGrade)
                {
                    continue;
                }

                if (_onlyNoInjuryCricket)
                {
                    short[] array = _cricketDataDict[canUseCricket.Key];
                    if (array[0] > 0 || array[1] > 0 || array[2] > 0 || array[3] > 0 || array[4] > 0)
                    {
                        continue;
                    }
                }

                list.Add(canUseCricket);
            }

            list.Sort((x, y) =>
            {

                var result = ItemTemplateHelper.GetCricketGrade(y.CricketColorId, y.CricketPartId) - ItemTemplateHelper.GetCricketGrade(x.CricketColorId, x.CricketPartId);
                if (result == 0) return y.Durability - x.Durability;
                return result;
            });

            for (int i = 0; i < Math.Min(3, list.Count); i++)
            {
                traverse.Field<int>("_selectingJarIndex").Value = i;

                traverse.Method("OnClickSelectCricket", list[i].Key).GetValue();
            }
        }

        /// <summary>
        /// 跳过开头动画
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
        /// 对方的蛐蛐都可见
        /// </summary>
        /// <param name="visible"></param>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(UI_CricketCombat), "SetEnemyCricketsVisible")]
        public static void UI_CricketCombat_SetEnemyCricketsVisible_Prefix(ref bool visible)
        {
            visible = true;
        }

        private static bool randomFirstMove = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(UI_CricketCombat), "OnListenerIdReady")]
        public static void UI_CricketCombat_OnListenerIdReady_Prefix(UI_CricketCombat __instance)
        {
            randomFirstMove = true;
        }

        /// <summary>
        /// 我方先手
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="rate"></param>
        /// <param name="totalRate"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Utils_Random), "RandomCheck", new Type[] { typeof(int), typeof(int) })]
        public static bool Utils_Random_RandomCheck_Prefix(UI_CricketCombat __instance, int rate, int totalRate, ref bool __result)
        {
            if (rate == 50 && randomFirstMove)
            {
                randomFirstMove = false;

                __result = true;
                return false;
            }
            randomFirstMove = false;
            return true;
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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Sequence), "DoAppendInterval")]
        public static void Sequence_AppendInterval_Prefix(ref float interval)
        {
            if (_isInCricketCombat)
            {
                interval /= 10;
            }
        }
    }
}
