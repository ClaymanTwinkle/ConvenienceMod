using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvenienceBackend.CombatStrategy;
using ConvenienceBackend.CombatStrategy.Data;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.Combat;
using GameData.GameDataBridge;
using GameData.Serializer;
using GameData.Utilities;
using HarmonyLib;
using Newtonsoft.Json;
using NLog;

namespace ConvenienceBackend.TongdaoCombat
{
    internal class TongdaoCombatBackendPatch : BaseBackendPatch
    {
        private static Logger _logger = LogManager.GetLogger("同道战斗");
        private static int _switchCharId = -1;

        public override void OnModSettingUpdate(string modIdStr)
        {
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CombatDomain), "StartCombat")]
        public static void CombatDomain_StartCombat_Postfix()
        {
            // ProcessCombatTeam(short combatConfigId, int[] enemyTeam, out int[] selfTeam)
            // OnRenderProactiveSkill
            // CallMethod(Operation operation, RawDataPool argDataPool, RawDataPool returnDataPool, DataContext context);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CombatDomain), "PrepareCombat", argumentTypes: new Type[] {
            typeof(DataContext),
            typeof(short),
            typeof(int[]),
            typeof(int[]),
        })]
        public static void CombatDomain_PrepareCombat_Postfix(CombatDomain __instance, DataContext context, short combatConfigId, ref int[] selfTeam, int[] enemyTeam)
        {
            if (_switchCharId != -1 && selfTeam != null && selfTeam.Length > 1 && selfTeam.Contains(DomainManager.Taiwu.GetTaiwuCharId()))
            {
                for (int i = 1; i < selfTeam.Length; i++)
                {
                    if (selfTeam[i] == _switchCharId)
                    {
                        (selfTeam[i], selfTeam[0]) = (selfTeam[0], selfTeam[i]);
                        break;
                    }
                }
            }

            _switchCharId = -1;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CombatDomain), "CallMethod")]
        public static bool CombatDomain_CallMethod_Prefix(Operation operation, RawDataPool argDataPool, RawDataPool returnDataPool, DataContext context, ref int __result)
        {
            bool result;
            if (operation.MethodId == 24324)
            {
                __result = -1;

                int num = operation.ArgsOffset;
                Serializer.Deserialize(argDataPool, num, ref _switchCharId);
                result = false;
            }
            else
            {
                result = true;
            }
            return result;
        }

        // [HarmonyPrefix]
        // [HarmonyPatch(typeof(CombatDomain), "SetCombatCharacter")]
        public static void CombatDomain_SetCombatCharacter_Prefix(DataContext context, bool isAlly, ref int charId, CombatCharacter ____selfChar, int[] ____selfTeam)
        {
            if (isAlly && ____selfChar == null)
            {
                var selfTeam = ____selfTeam;
                if (selfTeam.Length > 1 && selfTeam[0] == DomainManager.Taiwu.GetTaiwuCharId())
                {
                    for (int i = 1; i < selfTeam.Length; i++)
                    {
                        if (selfTeam[i] > -1)
                        {
                            charId = selfTeam[i];
                            break;
                        }
                    }
                }
            }
        }
    }
}
