using System;
using System.Collections.Generic;
using ConvenienceFrontend.Utils;
using DG.Tweening;
using FrameWork;
using FrameWork.ModSystem;
using GameData.Domains.Taiwu;
using GameData.Serializer;
using GameData.Utilities;
using HarmonyLib;
using TMPro;
using UISkillBreakPlate;
using UnityEngine;
using static GEvent;
using static MapBlockEffect;

namespace ConvenienceFrontend.AutoBreak
{
    internal class AutoBreakFrontPatch : BaseFrontPatch
    {
        private static CButton _drawBreakLineButton = null;
        private static CButton _autoBreakButton = null;
        private static TextMeshProUGUI _expectMaxPowerLabel = null;

        private static List<SkillBreakPlateIndex> breakPath = null;

        public override void OnModSettingUpdate(string modIdStr)
        {

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UISkillBreakPlate2), "InitRefers")]
        public static void UISkillBreakPlate2_InitRefers_Postfix(UISkillBreakPlate2 __instance)
        {
            if (_drawBreakLineButton != null)
            {
                _drawBreakLineButton.gameObject.SetActive(false);
            }
            else
            {
                Refers refers = __instance.CharacterAttributeDataView;
                var parent = refers.gameObject.transform;

                _drawBreakLineButton = GameObjectCreationUtils.UGUICreateCButton(parent, new Vector2(0, -550), new Vector2(150, 50), 16, "绘制突破路线");
                _drawBreakLineButton.ClearAndAddListener(delegate ()
                {
                    OnClickDrawBreakPath(__instance);
                });
                _drawBreakLineButton.gameObject.SetActive(false);
            }
            if (_autoBreakButton != null)
            {
                _autoBreakButton.gameObject.SetActive(false);
            }
            else
            {
                Refers refers = __instance.CharacterAttributeDataView;
                var parent = refers.gameObject.transform;

                _autoBreakButton = GameObjectCreationUtils.UGUICreateCButton(parent, new Vector2(0, -610), new Vector2(150, 50), 16, "自动突破");
                _autoBreakButton.ClearAndAddListener(delegate ()
                {
                    OnClickAutoBreakPath(__instance);
                });
                _autoBreakButton.gameObject.SetActive(false);
            }

            if (_expectMaxPowerLabel != null)
            {
                _expectMaxPowerLabel.gameObject.SetActive(false);
                _expectMaxPowerLabel.text = "理论最大威力上限：0";
            }
            else
            {
                var _maxPowerLabel = __instance.CGet<TextMeshProUGUI>("MaxPowerLabel");
                var parent = _maxPowerLabel.gameObject.transform;
                _expectMaxPowerLabel = GameObjectCreationUtils.UGUICreateTMPText(parent, "理论最大威力上限：0", 1000, 50, 500);
                _expectMaxPowerLabel.gameObject.SetActive(false);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UISkillBreakPlate2), "OnInit")]
        public static void UISkillBreakPlate2_OnInit_Postfix(UISkillBreakPlate2 __instance)
        {
            var _isReview = Traverse.Create(__instance).Field<bool>("_isReview").Value;
            _drawBreakLineButton?.gameObject?.SetActive(!_isReview);
            _autoBreakButton?.gameObject?.SetActive(!_isReview);
            _expectMaxPowerLabel?.gameObject?.SetActive(!_isReview);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UISkillBreakPlate2), "OnListenerIdReady")]
        public static void UISkillBreakPlate2_OnListenerIdReady_Postfix(UISkillBreakPlate2 __instance)
        {
            var traverse = Traverse.Create(__instance);
            var skillId = traverse.Field<short>("_skillId").Value;

            int callId = SingletonObject.getInstance<AsyncMethodDispatcher>().AsyncMethodCall<int>(19, 1333, skillId, delegate (int offset, RawDataPool dataPool)
            {
                int maxScore = 0;
                Serializer.Deserialize(dataPool, offset, ref maxScore);
                _expectMaxPowerLabel.text = $"理论最大威力上限：{maxScore}";
            });
            __instance?.RegisterAsyncMethodCall(callId);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UISkillBreakPlate2), "OnDisable")]
        public static void UISkillBreakPlate2_OnDisable_Postfix(UISkillBreakPlate2 __instance)
        {
            breakPath = null;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SkillBreakPlateRenderer), "RefreshCanSelectPath")]
        public static bool USkillBreakPlateRenderer_RefreshCanSelectPath_Prefix(SkillBreakPlateRenderer __instance)
        {
            return breakPath == null || breakPath.Count == 0;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SkillBreakPlateRenderer), "RefreshSelectedPath")]
        public static void USkillBreakPlateRenderer_RefreshSelectedPath_Postfix(SkillBreakPlateRenderer __instance)
        {
            if (breakPath == null) return;

            TriangleGrid ____triangleGrid = __instance.GetFieldValue<TriangleGrid>("_triangleGrid");
            RectTransform ____normalPathRoot = __instance.GetFieldValue<RectTransform>("_normalPathRoot");
            RectTransform ____normalPathTemplate = __instance.GetFieldValue<RectTransform>("_normalPathTemplate");
            RectTransform ____longPathRoot = __instance.GetFieldValue<RectTransform>("_longPathRoot");
            RectTransform ____longPathTemplate = __instance.GetFieldValue<RectTransform>("_longPathTemplate");

            HashSet<Segment> segments = EasyPool.Get<HashSet<Segment>>();
            segments.Clear();
            HashSet<Segment> longSegments = EasyPool.Get<HashSet<Segment>>();
            longSegments.Clear();
            SkillBreakPlateIndex start = SkillBreakPlateIndex.Invalid;
            if (breakPath != null)
            {
                foreach (SkillBreakPlateIndex item in breakPath)
                {
                    SkillBreakPlateGrid skillBreakPlateGrid = __instance.DisplayPlate[item];
                    if (start == SkillBreakPlateIndex.Invalid)
                    {
                        start = item;
                    }
                    else
                    {
                        Add(new Segment(start, item));
                        start = item;
                    }
                }
            }

            RenderPaths(__instance, ____triangleGrid, segments, ____normalPathTemplate, ____normalPathRoot, isParticle: true);
            RenderPaths(__instance, ____triangleGrid, longSegments, ____longPathTemplate, ____longPathRoot, isParticle: true);
            EasyPool.Free(segments);
            EasyPool.Free(longSegments);
            void Add(Segment segment)
            {
                if (__instance.CallPrivateMethod<bool>("IsLongConnection", segment.Start, segment.End))
                {
                    longSegments.Add(segment);
                }
                else
                {
                    segments.Add(segment);
                }
            }
        }

        private static void OnClickDrawBreakPath(UISkillBreakPlate2 __instance)
        {
            var traverse = Traverse.Create(__instance);
            var _skillId = traverse.Field<short>("_skillId").Value;

            ShowMask();
            DrawBreakPath(null, _skillId, delegate (int offset, RawDataPool dataPool)
            {
                breakPath = null;
                offset += Serializer.Deserialize(dataPool, offset, ref breakPath);
                int maxScore = 0;
                offset += Serializer.Deserialize(dataPool, offset, ref maxScore);

                SkillBreakPlateRenderer _gridArea = __instance.GetFieldValue<SkillBreakPlateRenderer>("_gridArea");
                if (_gridArea != null)
                {
                    try
                    {
                        _gridArea.CallPrivateMethod("RefreshSelectedPath");
                        _gridArea.CallPrivateMethod("RefreshCanSelectPath");
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
                HideMask();
                ShowDialog("结果", $"预计最大分数{maxScore}", delegate () { });
            });
        }

        private static void OnClickAutoBreakPath(UISkillBreakPlate2 __instance)
        {
            var traverse = Traverse.Create(__instance);
            var _skillId = traverse.Field<short>("_skillId").Value;

            ShowMask();
            AutoBreak(null, _skillId, delegate (int offset, RawDataPool dataPool)
            {
                SkillBreakPlate plate = null;
                offset += Serializer.Deserialize(dataPool, offset, ref plate);

                SkillBreakPlateRenderer _gridArea = __instance.GetFieldValue<SkillBreakPlateRenderer>("_gridArea");
                if (_gridArea != null)
                {
                    try
                    {
                        if (plate != null)
                        {
                            __instance.CallPrivateMethod("CheckFinish", plate);

                            SkillBreakPlate _lastPlate = __instance.GetFieldValue<SkillBreakPlate>("_lastPlate");
                            _gridArea.RefreshOnShotParticles(plate, _lastPlate, out var largestDuration);
                            __instance.CallPrivateMethod
                            (
                                "DelayCall",
                                new Action
                                (
                                delegate
                                {
                                    __instance.CallPrivateMethod("RefreshWithPlate", plate);
                                }
                                ),
                                largestDuration
                            );
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
                HideMask();
            });
        }

        public static void DrawBreakPath(IAsyncMethodRequestHandler requestHandler, short skillId, AsyncMethodCallbackDelegate callback)
        {
            int callId = SingletonObject.getInstance<AsyncMethodDispatcher>().AsyncMethodCall<int>(19, 1334, skillId, callback);
            requestHandler?.RegisterAsyncMethodCall(callId);
        }

        public static void AutoBreak(IAsyncMethodRequestHandler requestHandler, short skillId, AsyncMethodCallbackDelegate callback)
        {
            int callId = SingletonObject.getInstance<AsyncMethodDispatcher>().AsyncMethodCall<int>(19, 1335, skillId, callback);
            requestHandler?.RegisterAsyncMethodCall(callId);
        }

        private static void RenderPaths(SkillBreakPlateRenderer __instance, TriangleGrid _triangleGrid, HashSet<Segment> segments, RectTransform template, RectTransform root, bool isParticle = false, bool needGrowAnimation = false)
        {
            CommonUtils.PrepareEnoughChildren(root.transform, template.gameObject, segments.Count);
            int num = 0;
            foreach (Segment segment in segments)
            {
                SkillBreakPlateIndex start = segment.Start;
                SkillBreakPlateIndex end = segment.End;
                SkillBreakPlateIndex start2 = start;
                SkillBreakPlateIndex end2 = end;
                Transform child = root.GetChild(num);
                num++;
                child.gameObject.name = __instance.CallPrivateMethod<string>("GetPathName", start2, end2);
                Vector2 pointPosition = _triangleGrid.GetPointPosition(start2.X, start2.Y);
                Vector2 pointPosition2 = _triangleGrid.GetPointPosition(end2.X, end2.Y);
                Vector2 gridPosition = (pointPosition + pointPosition2) / 2f;
                Vector2 childPosition = __instance.CallPrivateMethod<Vector2>("GetChildPosition", types: new Type[] { typeof(Vector2) }, gridPosition);
                RectTransform component = child.GetComponent<RectTransform>();
                float realDistance = __instance.CallPrivateMethod<float>("GetRealDistance", start2, end2);
                // refreshSegmentStyleFunc?.Invoke(segment, child.gameObject);
                if (isParticle)
                {
                    __instance.CallPrivateMethod("SetSizeForUIParticle", realDistance, child, template);
                }
                else
                {
                    __instance.CallPrivateMethod("SetSizeForRectTransform", realDistance, child);
                    if (needGrowAnimation)
                    {
                        CImage component2 = component.GetComponent<CImage>();
                        component2.fillAmount = 0f;
                        component2.DOKill();
                        component2.DOFillAmount(1f, 0.3f);
                    }
                }

                component.anchoredPosition = childPosition;
                component.rotation = Quaternion.Euler(0f, 0f, _triangleGrid.GetRotationZ(start2.X, start2.Y, end2.X, end2.Y));
            }
        }

        private static void ShowMask()
        {
            ArgumentBox box = EasyPool.Get<ArgumentBox>();
            box.Set("ShowBlackMask", true);
            box.Set("ShowWaitAnimation", true);
            box.Set("Message", LocalStringManager.Get(7031));
            UIElement.FullScreenMask.SetOnInitArgs(box);
            UIElement.FullScreenMask.Show();
        }

        private static void HideMask()
        {
            UIElement.FullScreenMask.Hide(false);
        }

        private static void ShowDialog(string title, string message, Action onYes)
        {
            DialogCmd dialogCmd = new DialogCmd();
            dialogCmd.Type = 1;
            dialogCmd.Title = title;
            dialogCmd.Content = message;
            if (onYes != null)
            {
                dialogCmd.Yes = onYes;
            }
            if (onYes != null)
            {
                dialogCmd.No = onYes;
            }
            UIElement.Dialog.SetOnInitArgs(EasyPool.Get<ArgumentBox>().SetObject("Cmd", dialogCmd));
            UIManager.Instance.ShowUI(UIElement.Dialog);
        }

        private struct Segment : IEquatable<Segment>
        {
            public SkillBreakPlateIndex Start;

            public SkillBreakPlateIndex End;

            public Segment(SkillBreakPlateIndex start, SkillBreakPlateIndex end)
            {
                Start = start;
                End = end;
            }

            public bool Equals(Segment other)
            {
                return Start.Equals(other.Start) && End.Equals(other.End);
            }

            public override bool Equals(object obj)
            {
                int result;
                if (obj is Segment)
                {
                    Segment other = (Segment)obj;
                    result = (Equals(other) ? 1 : 0);
                }
                else
                {
                    result = 0;
                }

                return (byte)result != 0;
            }

            public override int GetHashCode()
            {
                int hashCode = -1676728671;
                hashCode = hashCode * -1521134295 + Start.GetHashCode();
                hashCode = hashCode * -1521134295 + End.GetHashCode();
                return hashCode;
            }
        }
    }
}
