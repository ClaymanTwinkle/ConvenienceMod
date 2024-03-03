using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Character.Ai.PrioritizedAction;
using GameData.Domains.Extra;
using GameData.Domains.Item;
using GameData.Domains.LifeRecord;
using GameData.Domains.Map;
using GameData.Domains.SpecialEffect.LegendaryBook.NpcEffect;
using GameData.Domains.TaiwuEvent;
using GameData.Domains.TaiwuEvent.EventHelper;
using GameData.GameDataBridge;
using GameData.Utilities;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using NLog;
using Redzen.Random;
using TaiwuModdingLib.Core.Utils;

namespace ConvenienceBackend.CustomSteal
{
    internal class CustomStealBackendPatch: BaseBackendPatch
    {
        /// <summary>
        /// 偷窃
        /// </summary>
        private static int _stealValue;
        private static TryMode _stealMode = TryMode.SimulationMode;

        /// <summary>
        /// 唬骗
        /// </summary>
        private static int _scamValue;
        private static TryMode _scamMode = TryMode.SimulationMode;

        /// <summary>
        /// 暗害
        /// </summary>
        private static int _plotHarmValue;
        private static TryMode _plotHarmMode = TryMode.SimulationMode;

        /// <summary>
        /// 抢夺
        /// </summary>
        private static int _robValue;
        private static TryMode _robMode = TryMode.SimulationMode;
        private static int _retryCount = 0;

        private static TryMode[] _modeList = new TryMode[]
        {
            // 模拟
            TryMode.SimulationMode,
            // 成功率
            TryMode.SuccessRateMode,
            // 不修改
            TryMode.NoModifyMode
        };

        /// <summary>
        /// 月挖掘保底有一个
        /// </summary>
        private static bool _guaranteedExcavation = true;

        /// <summary>
        /// 开启幸运挖掘
        /// </summary>
        private static bool _enableExcavationLucky = false;

        /// <summary>
        /// 挖掘幸运值
        /// </summary>
        private static int _excavationLuckyValue = 0;

        public override void OnModSettingUpdate(string modIdStr)
        {
            int num = 0;
            DomainManager.Mod.GetSetting(modIdStr, "steal_mode", ref num);
            DomainManager.Mod.GetSetting(modIdStr, "steal_value", ref _stealValue);
            _stealMode = _modeList[num];

            num = 0;
            DomainManager.Mod.GetSetting(modIdStr, "scam_mode", ref num);
            DomainManager.Mod.GetSetting(modIdStr, "scam_value", ref _scamValue);
            _scamMode = _modeList[num];

            num = 0;
            DomainManager.Mod.GetSetting(modIdStr, "plot_harm_mode", ref num);
            DomainManager.Mod.GetSetting(modIdStr, "plot_harm_value", ref _plotHarmValue);
            _plotHarmMode = _modeList[num];

            num = 0;
            DomainManager.Mod.GetSetting(modIdStr, "rob_mode", ref num);
            DomainManager.Mod.GetSetting(modIdStr, "rob_value", ref _robValue);
            _robMode = _modeList[num];

            DomainManager.Mod.GetSetting(modIdStr, "guaranteed_excavation", ref _guaranteedExcavation);
            DomainManager.Mod.GetSetting(modIdStr, "excavation_lucky", ref _enableExcavationLucky);
        }

        /// <summary>
        /// 偷窃物品
        /// </summary>
        /// <param name="selfCharId"></param>
        /// <param name="targetCharId"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        // Token: 0x06000001 RID: 1 RVA: 0x00002048 File Offset: 0x00000248
        [HarmonyPrefix]
        [HarmonyPatch(typeof(EventHelper), "GetStealActionPhase")]
        public static bool GetStealActionPhasePrefix(int selfCharId, int targetCharId, ref sbyte __result)
        {
            switch (_stealMode)
            {
                case TryMode.SuccessRateMode:
                    processSuccessRateMode(_stealValue, ref __result);
                    return false;
                case TryMode.SimulationMode:
                    processStealItemSimulationMode(selfCharId, targetCharId, ref __result);
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 哄骗物品或者功法
        /// </summary>
        /// <param name="selfCharId"></param>
        /// <param name="targetCharId"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(EventHelper), "GetScamActionPhase")]
        public static bool GetScamActionPhasePrefix(int selfCharId, int targetCharId, ref sbyte __result)
        {
            switch (_scamMode)
            {
                case TryMode.SuccessRateMode:
                    processSuccessRateMode(_scamValue, ref __result);
                    return false;
                case TryMode.SimulationMode:
                    processScamSimulationMode(selfCharId, targetCharId, ref __result);
                    return false;
            }
            return true;
        }


        /// <summary>
        /// 偷窃功法
        /// </summary>
        /// <param name="selfCharId"></param>
        /// <param name="targetCharId"></param>
        /// <param name="combatSkillTemplateId"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(EventHelper), "GetStealCombatSkillActionPhase")]
        public static bool GetStealCombatSkillActionPhasePrefix(int selfCharId, int targetCharId, short combatSkillTemplateId, ref sbyte __result)
        {
            if (selfCharId == DomainManager.Taiwu.GetTaiwuCharId()) 
            {
                switch (_stealMode) {
                    case TryMode.SuccessRateMode:
                        processSuccessRateMode(_stealValue, ref __result);
                        return false;
                    case TryMode.SimulationMode:
                        processStealCombatSkillSimulationMode(selfCharId, targetCharId, combatSkillTemplateId, ref __result);
                        return false;
                }
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EventHelper), "GetRobActionPhase")]
        public static bool GetRobActionPhasePrefix(int selfCharId, int targetCharId, ref sbyte __result)
        {
            if (selfCharId == DomainManager.Taiwu.GetTaiwuCharId())
            {
                switch (_robMode)
                {
                    case TryMode.SuccessRateMode:
                        processSuccessRateMode(_robValue, ref __result);
                        return false;
                    case TryMode.SimulationMode:
                        GetRobActionPhasePostfix(selfCharId, targetCharId, ref __result);
                        return false;
                }
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(EventHelper), "GetRobActionPhase")]
        public static void GetRobActionPhasePostfix(int selfCharId, int targetCharId, ref sbyte __result)
        {
            if (selfCharId != DomainManager.Taiwu.GetTaiwuCharId() || _robMode != TryMode.SimulationMode) 
            {
                stealLogger.Info("selfCharId != DomainManager.Taiwu.GetTaiwuCharId() || _robMode != TryMode.SimulationMode第" + (_retryCount + 1) + "次抢夺结果为：" + __result);
                _retryCount = 0;
                return;
            }

            if (__result >= 5)
            {
                stealLogger.Info("第" + (_retryCount + 1) + "次抢夺结果为：" + __result);
                _retryCount = 0;
                return;
            }

            if (_retryCount > _robValue)
            {
                _retryCount = 0;
                return;
            }

            if (_retryCount == 0)
            {
                stealLogger.Info("======================");
                stealLogger.Info(string.Concat(new string[]
                {
                "准备开始抢夺",
                "，模拟抢夺次数为：",
                _robValue.ToString(),
                    "次"
                }));
            }

            _retryCount++;
            stealLogger.Info("第" + _retryCount + "次抢夺结果为：" + __result);

            sbyte newResult = EventHelper.GetRobActionPhase(selfCharId, targetCharId);
            if (newResult > __result)
            {
                __result = newResult;
            }
        }

        /// <summary>
        /// 挖掘
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="context"></param>
        /// <param name="charId"></param>
        /// <param name="____brokenAreaMaterials"></param>
        /// <param name="____treasureMaterialFailedTimes"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ExtraDomain), "FindTreasure")]
        public static unsafe bool FindTreasurePostfix(ExtraDomain __instance, DataContext context, int charId, TreasureMaterialData[] ____brokenAreaMaterials, int ____treasureMaterialFailedTimes, ref TreasureFindResult __result)
        {
            if (!_guaranteedExcavation) return true;

            if (charId != DomainManager.Taiwu.GetTaiwuCharId())
            {
                return true;
            }
            if (DomainManager.World.GetLeftDaysInCurrMonth() < 3)
            {
                __result = TreasureFindResult.Invalid;
                return false;
            }

            if (!DomainManager.Character.TryGetElement_Objects(charId, out var element_Objects))
            {
                __result = TreasureFindResult.Invalid;
                return false;
            }

            Location location = element_Objects.GetLocation();
            if (!location.IsValid())
            {
                __result = TreasureFindResult.Invalid;
                return false;
            }

            DomainManager.World.AdvanceDaysInMonth(context, 3);
            MapBlockData block = DomainManager.Map.GetBlock(location);
            Personalities personalities = element_Objects.GetPersonalities();
            // 幸运值生效
            int b = personalities.Items[5] + (_enableExcavationLucky ? _excavationLuckyValue : 0);
            // 保底逻辑
            int percentProb = ((block.Items != null) ? (DomainManager.World.GetLeftDaysInCurrMonth() < 6 ? 100 : block.Items.Count * (100 + b * 5) / 100) : 0);
            TreasureFindResult treasureFindResult = new TreasureFindResult();
            treasureFindResult.Location = location;
            TreasureFindResult treasureFindResult2 = treasureFindResult;
            if (context.Random.CheckPercentProb(percentProb))
            {
                treasureFindResult2.ItemKeyAndDate = RandomUtils.GetRandomResult(((IEnumerable<ItemKeyAndDate>)block.Items.Keys).Select((Func<ItemKeyAndDate, (ItemKeyAndDate, short)>)((ItemKeyAndDate itemAndDate) => (itemAndDate, GlobalConfig.Instance.FindTreasureGradeRate[ItemTemplateHelper.GetGrade(itemAndDate.ItemKey.ItemType, itemAndDate.ItemKey.TemplateId)]))), context.Random);
                treasureFindResult2.ItemCount = (uint)block.Items[treasureFindResult2.ItemKeyAndDate];
            }

            if (treasureFindResult2.ItemCount < 1 && _enableExcavationLucky)
            {
                // 累加幸运值
                _excavationLuckyValue++;
            }

            bool TryGetLocationMaterials(Location location, out List<short> materials)
            {
                materials = null;
                short areaId = location.AreaId;
                if (areaId < 45 || areaId >= 135)
                {
                    return false;
                }
                TreasureMaterialData treasureMaterialData = ____brokenAreaMaterials[location.AreaId - 45];
                if (treasureMaterialData?.BlockMaterialTemplateIds == null)
                {
                    return false;
                }

                return treasureMaterialData.BlockMaterialTemplateIds.TryGetValue(location.BlockId, out materials);
            }

            if (TryGetLocationMaterials(location, out var materials) && materials.Count > 0 && context.Random.CheckPercentProb(20 + ____treasureMaterialFailedTimes * 10))
            {
                ReflectionExtensions.CallMethod(__instance, "SetTreasureMaterialFailedTimes", BindingFlags.NonPublic | BindingFlags.Instance, 0, context);
                treasureFindResult2.MaterialTemplateId = materials[context.Random.Next(materials.Count)];
            }
            else if (materials != null && materials.Count > 0)
            {
                ReflectionExtensions.CallMethod(__instance, "SetTreasureMaterialFailedTimes", BindingFlags.NonPublic | BindingFlags.Instance, ____treasureMaterialFailedTimes + 1, context);
                treasureFindResult2.ResourceType = 6;
                treasureFindResult2.ResourceCount = context.Random.Next(100, 300);
            }

            if (treasureFindResult2.AnyMaterial)
            {
                int arg = DomainManager.Map.QueryAreaBrokenLevel(location.AreaId);
                DomainManager.TaiwuEvent.OnEvent_TaiwuFindMaterial(arg, treasureFindResult2);
            }
            else
            {
                __instance.FindTreasureApplyResult(context, charId, treasureFindResult2);
                __instance.FindTreasureGenerateLifeRecord(charId, treasureFindResult2);
            }

            __result = treasureFindResult2;

            return false;
        }

        /// <summary>
        /// 暗害
        /// </summary>
        /// <param name="selfCharId"></param>
        /// <param name="targetCharId"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(EventHelper), "GetPlotHarmActionPhase")]
        public static bool GetPlotHarmActionPhasePrefix(int selfCharId, int targetCharId, ref sbyte __result)
        {
            if (selfCharId == DomainManager.Taiwu.GetTaiwuCharId())
            {
                switch (_plotHarmMode)
                {
                    case TryMode.SuccessRateMode:
                        processSuccessRateMode(_plotHarmValue, ref __result);
                        return false;
                    case TryMode.SimulationMode:
                        processPlotHarmSimulationMode(selfCharId, targetCharId, ref __result);
                        return false;
                }
            }
            return true;
        }

        private static void processSuccessRateMode(int value, ref sbyte __result)
        {
            IRandomSource random = DomainManager.TaiwuEvent.MainThreadDataContext.Random;
            if (random.Next(0, 99) < value)
            {
                __result = 5;
                return;
            }
            __result = (sbyte)random.Next(0, 3);
        }

        private static void processScamSimulationMode(int selfCharId, int targetCharId, ref sbyte __result)
        {
            int value = _scamValue;
            IRandomSource random = DomainManager.TaiwuEvent.MainThreadDataContext.Random;
            Character element_Objects = DomainManager.Character.GetElement_Objects(selfCharId);
            Character character = DomainManager.Character.GetElement_Objects(targetCharId);
            OrganizationInfo organizationInfo = character.GetOrganizationInfo();
            if (organizationInfo.SettlementId >= 0 && Config.Organization.Instance[organizationInfo.OrgTemplateId].IsCivilian && DomainManager.Character.HasGuard(targetCharId, character))
            {
                sbyte fameType = character.GetFameType();
                bool isHeretic = fameType == -2 ? random.NextBool() : fameType < 3;
                Location location = DomainManager.Organization.GetSettlement(organizationInfo.SettlementId).GetLocation();
                sbyte stateTemplateIdByAreaId = DomainManager.Map.GetStateTemplateIdByAreaId(location.AreaId);
                character = DomainManager.Character.GetPregeneratedCityTownGuard(stateTemplateIdByAreaId, isHeretic, organizationInfo.Grade);
            }
            string text = "";
            Character element_Objects3 = DomainManager.Character.GetElement_Objects(targetCharId);
            if (element_Objects3 != null)
            {
                ValueTuple<string, string> name = element_Objects3.GetFullName().GetName(character.GetGender(), DomainManager.World.GetCustomTexts());
                text = name.Item1 + name.Item2;
            }
            stealLogger.Info("======================");
            stealLogger.Info(string.Concat(new string[]
            {
                "准备开始哄骗",
                text,
                "，模拟哄骗次数为：",
                value.ToString(),
                "次"
            }));
            sbyte b = 0;
            for (int i = 1; i < value; i++)
            {
                int stealActionPhase = element_Objects.GetStealActionPhase(random, character);
                stealLogger.Info("第" + i.ToString() + "次哄骗结果为：" + stealActionPhase);
                b = (sbyte)Math.Max(b, stealActionPhase);
                if (b >= 5)
                {
                    break;
                }
            }
            stealLogger.Info("结束哄骗，最终结果为：" + b);
            stealLogger.Info("======================");
            __result = b;
        }

        // Token: 0x06000003 RID: 3 RVA: 0x000020C4 File Offset: 0x000002C4
        private static void processStealItemSimulationMode(int selfCharId, int targetCharId, ref sbyte __result)
        {
            int value = _stealValue;
            IRandomSource random = DomainManager.TaiwuEvent.MainThreadDataContext.Random;
            Character element_Objects = DomainManager.Character.GetElement_Objects(selfCharId);
            Character character = DomainManager.Character.GetElement_Objects(targetCharId);
            OrganizationInfo organizationInfo = character.GetOrganizationInfo();
            if (organizationInfo.SettlementId >= 0 && Config.Organization.Instance[organizationInfo.OrgTemplateId].IsCivilian && DomainManager.Character.HasGuard(targetCharId, character))
            {
                sbyte fameType = character.GetFameType();
                bool isHeretic = fameType == -2 ? random.NextBool() : fameType < 3;
                Location location = DomainManager.Organization.GetSettlement(organizationInfo.SettlementId).GetLocation();
                sbyte stateTemplateIdByAreaId = DomainManager.Map.GetStateTemplateIdByAreaId(location.AreaId);
                character = DomainManager.Character.GetPregeneratedCityTownGuard(stateTemplateIdByAreaId, isHeretic, organizationInfo.Grade);
            }
            string text = "";
            Character element_Objects3 = DomainManager.Character.GetElement_Objects(targetCharId);
            if (element_Objects3 != null)
            {
                ValueTuple<string, string> name = element_Objects3.GetFullName().GetName(character.GetGender(), DomainManager.World.GetCustomTexts());
                text = name.Item1 + name.Item2;
            }
            stealLogger.Info("======================");
            stealLogger.Info(string.Concat(new string[]
            {
                "准备开始偷窃",
                text,
                "，模拟偷窃次数为：",
                value.ToString(),
                "次"
            }));
            sbyte b = 0;
            for (int i = 1; i < value; i++)
            {
                int stealActionPhase = element_Objects.GetStealActionPhase(random, character);
                stealLogger.Info("第" + i.ToString() + "次偷窃结果为：" + stealResultDesc[stealActionPhase]);
                b = (sbyte)Math.Max(b, stealActionPhase);
                if (b >= 5)
                {
                    break;
                }
            }
            stealLogger.Info("结束偷窃，最终结果为：" + stealResultDesc[b]);
            stealLogger.Info("======================");
            __result = b;
        }

        private static void processStealCombatSkillSimulationMode(int selfCharId, int targetCharId, short combatSkillTemplateId, ref sbyte __result)
        {
            int value = _stealValue;
            IRandomSource random = DomainManager.TaiwuEvent.MainThreadDataContext.Random;
            GameData.Domains.Character.Character element_Objects = DomainManager.Character.GetElement_Objects(selfCharId);
            GameData.Domains.Character.Character character = DomainManager.Character.GetElement_Objects(targetCharId);
            sbyte type = Config.CombatSkill.Instance[combatSkillTemplateId].Type;
            sbyte grade = Config.CombatSkill.Instance[combatSkillTemplateId].Grade;
            OrganizationInfo organizationInfo = character.GetOrganizationInfo();
            if (organizationInfo.SettlementId >= 0 && Config.Organization.Instance[organizationInfo.OrgTemplateId].IsCivilian && DomainManager.Character.HasGuard(targetCharId, character))
            {
                sbyte fameType = character.GetFameType();
                bool isHeretic = ((fameType == -2) ? random.NextBool() : (fameType < 3));
                Location location = DomainManager.Organization.GetSettlement(organizationInfo.SettlementId).GetLocation();
                sbyte stateTemplateIdByAreaId = DomainManager.Map.GetStateTemplateIdByAreaId(location.AreaId);
                character = DomainManager.Character.GetPregeneratedCityTownGuard(stateTemplateIdByAreaId, isHeretic, organizationInfo.Grade);
            }
            string text = Config.CombatSkill.Instance[combatSkillTemplateId].Name;

            stealLogger.Info("======================");
            stealLogger.Info(string.Concat(new string[]
            {
                "准备开始偷窃功法",
                text,
                "，模拟偷窃次数为：",
                value.ToString(),
                "次"
            }));
            sbyte b = 0;
            for (int i = 1; i < value; i++)
            {
                int stealActionPhase = element_Objects.GetStealCombatSkillActionPhase(random, character, type, grade);
                stealLogger.Info("第" + i.ToString() + "次偷窃结果为：" + stealResultDesc[stealActionPhase]);
                b = (sbyte)Math.Max(b, stealActionPhase);
                if (b >= 5)
                {
                    break;
                }
            }
            stealLogger.Info("结束偷窃，最终结果为：" + stealResultDesc[b]);
            stealLogger.Info("======================");
            __result = b;
        }

        private static void processPlotHarmSimulationMode(int selfCharId, int targetCharId, ref sbyte __result) 
        {
            int value = _plotHarmValue;

            GameData.Domains.Character.Character element_Objects = DomainManager.Character.GetElement_Objects(selfCharId);
            GameData.Domains.Character.Character character = DomainManager.Character.GetElement_Objects(targetCharId);
            IRandomSource random = DomainManager.TaiwuEvent.MainThreadDataContext.Random;
            OrganizationInfo organizationInfo = character.GetOrganizationInfo();
            if (organizationInfo.SettlementId >= 0 && Config.Organization.Instance[organizationInfo.OrgTemplateId].IsCivilian && DomainManager.Character.HasGuard(targetCharId, character))
            {
                sbyte fameType = character.GetFameType();
                bool isHeretic = ((fameType == -2) ? random.NextBool() : (fameType < 3));
                Location location = DomainManager.Organization.GetSettlement(organizationInfo.SettlementId).GetLocation();
                sbyte stateTemplateIdByAreaId = DomainManager.Map.GetStateTemplateIdByAreaId(location.AreaId);
                character = DomainManager.Character.GetPregeneratedCityTownGuard(stateTemplateIdByAreaId, isHeretic, organizationInfo.Grade);
            }

            stealLogger.Info("======================");
            stealLogger.Info(string.Concat(new string[]
            {
                "准备开始暗害",
                character.GetGivenName(),
                "，模拟暗害次数为：",
                value.ToString(),
                "次"
            }));

            sbyte b = 0;
            for (int i = 1; i < value; i++)
            {
                int plotHarmActionPhase = element_Objects.GetPlotHarmActionPhase(random, character);
                stealLogger.Info("第" + i.ToString() + "次暗害结果为：" + stealResultDesc[plotHarmActionPhase]);
                b = (sbyte)Math.Max(b, plotHarmActionPhase);
                if (b >= 5)
                {
                    break;
                }
            }
            stealLogger.Info("结束暗害，最终结果为：" + stealResultDesc[b]);
            stealLogger.Info("======================");
            __result = b;
        }

        // Token: 0x04000001 RID: 1
        private static Logger stealLogger = LogManager.GetLogger("便利性模组合集");

        // Token: 0x04000002 RID: 2
        private static string[] stealResultDesc = new string[]
        {
            "失败-[0]寻物阶段失败",
            "失败-[1]藏匿阶段失败",
            "失败-[2]等待时机阶段失败",
            "失败-[3]行动阶段失败",
            "成功-[4]需要战斗",
            "成功-[5]成功逃走"
        };
    }

    public enum TryMode
    {
        SuccessRateMode,

        SimulationMode,

        NoModifyMode
    }
}
