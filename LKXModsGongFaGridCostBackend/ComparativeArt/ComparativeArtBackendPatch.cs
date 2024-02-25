using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Taiwu;
using GameData.Domains.Taiwu.LifeSkillCombat;
using GameData.Domains.TaiwuEvent;
using GameData.Domains.TaiwuEvent.EventHelper;
using GameData.GameDataBridge;
using GameData.Serializer;
using GameData.Utilities;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Fluent;
using TaiwuModdingLib.Core.Utils;

namespace ConvenienceBackend.ComparativeArt
{
    internal class ComparativeArtBackendPatch : BaseBackendPatch
    {
        private static Logger _logger = LogManager.GetLogger("较艺优化");

        private static bool _artAlwaysWin = false;
        private static bool _artDirectvictory = false;
        private static bool _isStart = false;


        public override void OnModSettingUpdate(string modIdStr)
        {
            DomainManager.Mod.GetSetting(modIdStr, "Toggle_ArtAlwaysWin", ref _artAlwaysWin);
            DomainManager.Mod.GetSetting(modIdStr, "Toggle_ArtDirectvictory", ref _artDirectvictory);
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(TaiwuDomain), "LifeSkillCombatStart")]
        public static void TaiwuDoamin_LifeSkillCombatStart_Prefix() 
        {
            _isStart = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TaiwuDomain), "LifeSkillCombatStart")]
        public static void TaiwuDoamin_LifeSkillCombatStart_Postfix()
        {
            _isStart = false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Match), "CheckResult")]
        public static void Match_CheckResult_Postfix(Match __instance)
        {
            if (_isStart) return;
            if (!_artAlwaysWin) return;
            ReflectionExtensions.ModifyField(__instance, "_suiciderPlayerId", (sbyte?)1);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EventHelper), "StartLifeSkillCombat")]
        public static bool EventHelper_StartLifeSkillCombat_Prefix(int characterId, sbyte lifeSKillType, string onFinishEventId, EventArgBox argBox)
        {
            if (!_artDirectvictory) return true;

            // Events.RaiseLifeSkillCombatStarted(DomainManager.TaiwuEvent.MainThreadDataContext);
            DomainManager.TaiwuEvent.SetListenerWithActionName(onFinishEventId, argBox, "LifeSkillBattleComplete");
            // Events.RaiseLifeSkillCombatFinished(DomainManager.TaiwuEvent.MainThreadDataContext, true, characterId, 0, 0);

            // DomainManager.TaiwuEvent.SetListenerBoolArg("WinState", true);
            // DomainManager.TaiwuEvent.TriggerListener("LifeSkillBattleComplete", true);

            GameDataBridge.AddDisplayEvent<int, sbyte>(DisplayEventType.OpenLifeSkillCombat, characterId, lifeSKillType);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TaiwuEventDomain), "ToEvent")]
        public static void EventHelper_ToEvent_Prefix(string eventGuid)
        {
            // _logger.Info("ToEvent=" + eventGuid);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TaiwuEventDomain), "GetEventDisplayData")]
        public static void TaiwuEventDomain_GetEventDisplayData_Prefix(TaiwuEventDomain __instance)
        {
            // var showingEvent = Traverse.Create(__instance).Field<TaiwuEvent>("ShowingEvent").Value;
            // if (showingEvent == null) return;
            //_logger.Info("GetEventDisplayData=" + showingEvent.EventGuid);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TaiwuEventDomain), "set_ShowingEvent")]

        public static void EventSelect(TaiwuEvent value)
        {
            // _logger.Info("set_ShowingEvent=" + value.EventGuid);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TaiwuEventDomain), "CallMethod")]
        public static bool TaiwuEventDomain_CallMethod_Prefix(Operation operation, RawDataPool argDataPool, RawDataPool returnDataPool, DataContext context)
        {
            // _logger.Info("CallMethod=" + operation.MethodId);
            if (_artDirectvictory && operation.MethodId == 1970)
            {

                int num = operation.ArgsOffset;
                ushort flag = 0;
                int characterId = 0;
                num += Serializer.Deserialize(argDataPool, num, ref flag);
                num += Serializer.Deserialize(argDataPool, num, ref characterId);

                // 获取威望和历练
                var taiwu = DomainManager.Taiwu.GetTaiwu();
                int preAuthorityNum = taiwu.GetResource(ResourceType.Authority);
                int preExpNum = taiwu.GetExp();
                DomainManager.Taiwu.ApplyLifeSkillCombatResult(context, characterId, true);
                int authorityDiffNum = taiwu.GetResource(ResourceType.Authority) - preAuthorityNum;
                if (authorityDiffNum > 0)
                {
                    DomainManager.World.GetInstantNotificationCollection().AddResourceIncreased(taiwu.GetId(), ResourceType.Authority, authorityDiffNum);
                }

                int expDiffNum = taiwu.GetExp() - preExpNum;
                if (expDiffNum > 0)
                {
                    DomainManager.World.GetInstantNotificationCollection().AddExpIncreased(taiwu.GetId(), expDiffNum);
                }
                
                // 跳转到较艺成功对话
                DomainManager.TaiwuEvent.SetListenerBoolArg("WinState", true);
                DomainManager.TaiwuEvent.TriggerListener("LifeSkillBattleComplete", true);
                return false;
            }

            return true;
        }
    }
}
