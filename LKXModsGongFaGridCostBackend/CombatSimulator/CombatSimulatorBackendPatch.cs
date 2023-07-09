using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BehTree;
using Config;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.Combat;
using GameData.Domains.Organization;
using GameData.GameDataBridge;
using GameData.Utilities;
using HarmonyLib;
using TaiwuModdingLib.Core.Utils;

namespace ConvenienceBackend.CombatSimulator
{
    internal class CombatSimulatorBackendPatch : BaseBackendPatch
    {
        public override void OnModSettingUpdate(string modIdStr)
        {
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CombatDomain), "SetTimeScale", new Type[] { typeof(float), typeof(DataContext) })]
        public static void CombatDomain_SetTimeScale_Prefix(ref float value)
        {
            if (value == 2f)
            {
                value = 10f;
            }
            AdaptableLog.Info("SetTimeScale " + value);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CombatDomain), "GetElement_CombatCharacterDict")]
        public static bool CombatDomain_GetElement_CombatCharacterDict_Prefix(Dictionary<int, CombatCharacter> ____combatCharacterDict, int objectId, ref CombatCharacter __result)
        {
            if (!____combatCharacterDict.ContainsKey(objectId))
            {
                __result = null;

                return false;
            }

            return true;
        }
            

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CombatDomain), "CallMethod")]
        public static bool CombatDomain_CallMethod_Prefix(CombatDomain __instance, Operation operation, RawDataPool argDataPool, RawDataPool returnDataPool, DataContext context, ref int __result)
        {
            if (operation.MethodId == 1970)
            {
                var consummateLevel = 2;

                var list = DomainManager.Character.GmCmd_GetAllCharacterName().FindAll(x=>DomainManager.Character.GetElement_Objects(x.CharId).GetConsummateLevel() == consummateLevel);

                var a = list.GetRandom(context.Random);

                var b = list.GetRandom(context.Random);

                AdaptableLog.Info("开始模拟NPC对战: " + a.Name + "(" +a.CharId +")" + " VS " + b.Name + "(" + b.CharId + ")");

                DomainManager.Combat.StartNpcCombat(context, new int[] { a.CharId, -1, -1, -1 }, new int[] { b.CharId, -1, -1, -1 });

                DomainManager.Combat.SetTimeScale(context, 100f);
                DomainManager.Combat.StartCombat(context);
                DomainManager.Combat.SetTimeScale(context, 100f);

                return false;
            }

            // 1. ProcessCombatTeammateBetray
            // 2. PrepareEnemyEquipments
            // 3. PrepareCombat
            // 4. SetAiOptions
            // 5. SetTimeScale
            // 6. GetProactiveSkillList
            // 7. GetCanHealInjuryCount
            // 8. GetCanHealPoisonCount
            // 9. StartCombat
            // 10. PlayMoveStepSound
            // 11. GetWeaponInnerRatio
            // 12. GetWeaponEffects
            // 13. GetCombatResultDisplayData
            // 14. SelectGetItem
            return true;
        }

        /// <summary>
        /// 结束战斗
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CombatDomain), "EndCombat")]
        public static void CombatDomain_EndCombat_Postfix(CombatDomain __instance, CombatCharacter failChar, bool flee = false)
        {
            var mainChar = __instance.GetCombatCharacter(true, true).GetCharacter();
            var enemyChar = __instance.GetCombatCharacter(false, true).GetCharacter();

            var mainCharName = DomainManager.Character.GetRealName(mainChar.GetId());
            var enemyCharName = DomainManager.Character.GetRealName(enemyChar.GetId());

            var isMainWin = mainChar.GetId() != failChar.GetId();

            AdaptableLog.Info("结束模拟NPC对战: " + (mainCharName.surname + mainCharName.givenName) + "(" + mainChar.GetId() + ")" + (isMainWin ? " 胜 " : " 负 ") + (enemyCharName.surname + enemyCharName.givenName) + "(" + enemyChar.GetId() + "), 原因是:" + (flee ? "一方逃跑":"一方战败"));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CombatCharacterStateIdle), "OnEnter")]
        [HarmonyPatch(typeof(CombatCharacterStateSelectChangeTrick), "OnEnter")]
        [HarmonyPatch(typeof(CombatCharacterStatePrepareAttack), "OnEnter")]
        [HarmonyPatch(typeof(CombatCharacterStateAttack), "OnEnter")]
        [HarmonyPatch(typeof(CombatCharacterStatePrepareSkill), "OnEnter")]
        [HarmonyPatch(typeof(CombatCharacterStateCastSkill), "OnEnter")]
        [HarmonyPatch(typeof(CombatCharacterStatePrepareOtherAction), "OnEnter")]
        [HarmonyPatch(typeof(CombatCharacterStatePrepareUseItem), "OnEnter")]
        [HarmonyPatch(typeof(CombatCharacterStateUseItem), "OnEnter")]
        [HarmonyPatch(typeof(CombatCharacterStateSelectMercy), "OnEnter")]
        [HarmonyPatch(typeof(CombatCharacterStateDelaySettlement), "OnEnter")]
        [HarmonyPatch(typeof(CombatCharacterStateChangeCharacter), "OnEnter")]
        [HarmonyPatch(typeof(CombatCharacterStateTeammateCommand), "OnEnter")]
        [HarmonyPatch(typeof(CombatCharacterStateChangeBossPhase), "OnEnter")]
        [HarmonyPatch(typeof(CombatCharacterStateAnimalAttack), "OnEnter")]
        [HarmonyPatch(typeof(CombatCharacterStateJumpMove), "OnEnter")]
        [HarmonyPatch(typeof(CombatCharacterStateSpecialShow), "OnEnter")]
        public static void CombatCharacterStateBase_OnEnter_Postfix(CombatCharacterStateBase __instance)
        {
            CombatCharacter CombatChar = (CombatCharacter)__instance.GetFieldValue("CombatChar", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            
            AdaptableLog.Info(CombatChar.GetCharacter().GetId() + "进入状态" + __instance.StateType);
        }
    }
}
