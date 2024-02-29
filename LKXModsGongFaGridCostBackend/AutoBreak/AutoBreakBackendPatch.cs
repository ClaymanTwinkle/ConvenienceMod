using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Config;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.CombatSkill;
using GameData.Domains.Taiwu;
using GameData.GameDataBridge;
using GameData.Utilities;
using HarmonyLib;

namespace ConvenienceBackend.AutoBreak
{
    internal class AutoBreakBackendPatch : BaseBackendPatch
    {
        private static bool _isAutoBreak = false;

        public override void OnModSettingUpdate(string modIdStr)
        {
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TaiwuDomain), "InitSkillBreakPlate")]
        public static void Taiwu_Init_PostPatch(DataContext context, GameData.Domains.Taiwu.SkillBreakPlate plate, short skillId, TaiwuDomain __instance)
        {
            if (!_isAutoBreak) return;

            var totalStepCount = plate.TotalStepCount;
            var pageIndex = CombatSkillStateHelper.GetNormalPageInternalIndex(1, 4);
            bool beSpellbound = CombatSkillStateHelper.IsPageActive(plate.SelectedPages, pageIndex); // 是否走火入魔

            sbyte activeOutlinePageType = CombatSkillStateHelper.GetActiveOutlinePageType(plate.SelectedPages);
            List<BreakGrid> bonusBreakGrids = CombatSkillDomain.GetBonusBreakGrids(skillId, activeOutlinePageType);
            List<short> bonusGridslist = new List<short>();
            foreach (BreakGrid breakGrid in bonusBreakGrids)
            {
                for (int i = 0; i < (int)breakGrid.GridCount; i++)
                {
                    bonusGridslist.Add(breakGrid.BonusType);
                }
            }

            // 清除所有黄点
            SkillBreakPlateGrid[][] grids = plate.Grids;
            Point startPoint = Point.Empty;
            Point endPoint = Point.Empty;

            if (grids[plate.StartPoint.Item1][plate.StartPoint.Item2].TemplateId == SkillBreakGridType.DefKey.StartPoint)
            {
                startPoint = new Point(plate.StartPoint.Item1, plate.StartPoint.Item2);
            }

            for (int j = 0; j < grids.Length; j++)
            {
                for (int k = 0; k < grids[j].Length; k++)
                {
                    var grid = grids[j][k];
                    grid.State = 0; // 显示

                    switch (grid.TemplateId)
                    {
                        case SkillBreakGridType.DefKey.Bonus:
                            grid.TemplateId = SkillBreakGridType.DefKey.Normal;
                            grid.BonusType = -1;
                            break;
                        case SkillBreakGridType.DefKey.StartPoint:
                            if (startPoint == Point.Empty)
                            {
                                startPoint = new Point(j, k);
                            }
                            break;
                        case SkillBreakGridType.DefKey.EndPoint:
                            if (endPoint == Point.Empty)
                            {
                                endPoint = new Point(j, k);
                            }
                            break;
                    }
                }
            }

            // 绘制黄点
            var isXPlus = endPoint.X > startPoint.X;
            var isYPlus = endPoint.Y > startPoint.Y;
            for (var i = startPoint.X += (isXPlus ? 1 : -1); isXPlus ? i <= endPoint.X : i >= endPoint.X; i += (isXPlus ? 1 : -1))
            {
                var grid = plate.Grids[i][startPoint.Y];

                if (bonusGridslist.Count > 0)
                {
                    grid.TemplateId = SkillBreakGridType.DefKey.Bonus;
                    grid.BonusType = bonusGridslist[0];
                    bonusGridslist.RemoveAt(0);
                }
            }

            for (var j = startPoint.Y += (isYPlus ? 1 : -1); isYPlus ? j < endPoint.Y : j > endPoint.Y; j += (isYPlus ? 1 : -1))
            {
                var grid = plate.Grids[endPoint.X][j];
                if (bonusGridslist.Count > 0)
                {
                    grid.TemplateId = SkillBreakGridType.DefKey.Bonus;
                    grid.BonusType = bonusGridslist[0];
                    bonusGridslist.RemoveAt(0);
                }
                else
                {
                    grid.TemplateId = SkillBreakGridType.DefKey.Normal;
                    grid.SuccessRateFix = 100;
                }
            }

            if (bonusGridslist.Count > 0)
            {
                var nearX = endPoint.X + 1 == plate.Grids.Length ? endPoint.X-1 : endPoint.X + 1;
                for (var j = startPoint.Y += (isYPlus ? 1 : -1); isYPlus ? j < endPoint.Y : j > endPoint.Y; j += (isYPlus ? 1 : -1))
                {
                    var grid = plate.Grids[nearX][j];
                    if (bonusGridslist.Count > 0)
                    {
                        grid.TemplateId = SkillBreakGridType.DefKey.Bonus;
                        grid.BonusType = bonusGridslist[0];
                        bonusGridslist.RemoveAt(0);
                    }
                }
            }
            
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TaiwuDomain), "AutoBreak_Process")]
        public static void Taiwu_AutoBreak_Process_PostPatch(TaiwuDomain __instance, DataContext context, short skillTemplateId)
        {
            var traverse = Traverse.Create(__instance);
            List<ValueTuple<byte, byte>> _autoBreakSolutionPath = traverse.Field<List<ValueTuple<byte, byte>>>("_autoBreakSolutionPath").Value;
            var plate = __instance.GetElement_SkillBreakPlateDict(skillTemplateId);
            foreach (ValueTuple<byte, byte> pos in _autoBreakSolutionPath)
            {
                var grid = plate.Grids[pos.Item1][pos.Item2];
                if (grid.TemplateId != SkillBreakGridType.DefKey.Bonus && grid.TemplateId != SkillBreakGridType.DefKey.StartPoint && grid.TemplateId != SkillBreakGridType.DefKey.EndPoint)
                {
                    grid.TemplateId = SkillBreakGridType.DefKey.Normal;
                    grid.SuccessRateFix = 100;
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TaiwuDomain), "AutoBreakOut")]
        public static void Taiwu_AutoBreakOute_PrePatch(TaiwuDomain __instance, DataContext context, short skillTemplateId, ushort selectedPages, ref SkillBreakBonusCollection __result)
        {
            _isAutoBreak = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TaiwuDomain), "AutoBreakOut")]
        public static void Taiwu_AutoBreakOute_PostPatch(TaiwuDomain __instance, DataContext context, short skillTemplateId, ushort selectedPages, ref SkillBreakBonusCollection __result)
        {
            _isAutoBreak = false;
        }

        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(TaiwuDomain), "AutoBreakOut")]
        //public static bool Taiwu_AutoBreakOute_PostPatch(TaiwuDomain __instance, DataContext context, short skillTemplateId, ushort selectedPages, ref SkillBreakBonusCollection __result)
        //{
        //    var plate = __instance.GetElement_SkillBreakPlateDict(skillTemplateId);

        //    var traverse = Traverse.Create(__instance);
        //    traverse.Method("AutoBreak_Init", context, plate, skillTemplateId, null).GetValue();
        //    GameData.Domains.Character.Character taiwu = DomainManager.Taiwu.GetTaiwu();
        //    int newUnderstandingNeedExp = DomainManager.CombatSkill.GetNewUnderstandingNeedExp(taiwu.GetId(), skillTemplateId);
        //    taiwu.ChangeExp(context, -newUnderstandingNeedExp);
        //    DomainManager.World.AdvanceDaysInMonth(context, 10);

        //    Dictionary<short, SkillBreakBonusCollection> _skillBreakBonusDict = traverse.Field<Dictionary<short, SkillBreakBonusCollection>>("_skillBreakBonusDict").Value;

        //    __result = _skillBreakBonusDict[skillTemplateId];

        //    return false;
        //}
    }
}
