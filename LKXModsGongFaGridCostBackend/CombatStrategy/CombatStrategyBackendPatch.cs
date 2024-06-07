using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using Config;
using ConvenienceBackend.CombatStrategy.Data;
using ConvenienceBackend.CombatStrategy.Opt;
using ConvenienceBackend.CombatStrategy.Utils;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;
using GameData.Domains.Item;
using GameData.GameDataBridge;
using GameData.Serializer;
using GameData.Utilities;
using HarmonyLib;
using Newtonsoft.Json;
using NLog;
using TaiwuModdingLib.Core.Utils;

namespace ConvenienceBackend.CombatStrategy
{
    internal class CombatStrategyBackendPatch: BaseBackendPatch
    {
        private const int MOD_MAX_MOBILITY = 1000;

        private static bool _startCombatCalled = false;
        private static Logger _logger = LogManager.GetLogger("战斗策略");


        private static Dictionary<short, int> _prepareSkillCountMap = new();

        public override void OnModSettingUpdate(string modIdStr)
        {
            DomainManager.Mod.GetSetting(modIdStr, "ReplaceAI", ref _replaceAi);
        }

        // Token: 0x0600001D RID: 29 RVA: 0x00002E98 File Offset: 0x00001098
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CombatCharacter), "InitTeammateCommand")]
        public static void CombatCharacter_InitTeammateCommand_Postfix(DataContext context, CombatCharacter __instance)
        {
            if (IsEnable()) return;
            if (!__instance.IsAlly) return;
            _logger.Info("CombatCharacter::InitTeammateCommand");
            var currTeammateCommands = __instance.GetCurrTeammateCommands();
            _logger.Info("角色[" + __instance.GetId() + "]有" + String.Join(",", currTeammateCommands));
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CombatDomain), "SetPlayerAutoCombat")]
        public static void CombatDomain_SetPlayerAutoCombat_Prefix(DataContext context, ref bool autoCombat)
        {
            if (IsEnable() && autoCombat)
            {
                autoCombat = false;
                _logger.Info("系统设置打开原版自动战斗，但是因为战斗策略打开，所有禁用原版的自动战斗");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CombatDomain), "StartCombat")]
        public static void CombatDomain_StartCombat_Postfix()
        {
            _logger.Info("开始战斗");
            _startCombatCalled = true;
            AICombatManager.StartCombat();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CombatDomain), "SetCombatStatus")]
        public static void CombatDomain_SetCombatStatus_Postfix(CombatDomain __instance)
        {
            if (CombatStatusType.NotInCombat == __instance.GetCombatStatus())
            {
                _logger.Info("重置战斗");
                _startCombatCalled = false;
                _switchWeaponsCD = 0;
                _prepareSkillCountMap.Clear();

                AICombatManager.ResetCombat();
            }
        }

        // Token: 0x06000018 RID: 24 RVA: 0x00002858 File Offset: 0x00000A58
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CombatDomain), "OnUpdate")]
        public static void CombatDomain_OnUpdate_Prefix(CombatDomain __instance, DataContext context)
        {
            // 没有开启战斗策略
            if (!IsEnable()) return;

            // 还没开始，不能执行
            if (__instance.GetTimeScale() <= 0f || !__instance.IsInCombat() || !_startCombatCalled) return;

            // 没有配置
            if (_settings == null) return;

            // 系统自动战斗打开了
            if (__instance.GetAutoCombat()) return;

            // 关闭了战斗策略
            if (!_settings.isEnable) return;

            // 不是玩家
            CombatCharacter combatCharacter = __instance.GetCombatCharacter(true, false);
            if (!__instance.IsMainCharacter(combatCharacter)) return;


            //if (_settings.UseAICombat) 
            //{
            //    AICombatManager.HandleCombatUpdate(__instance, context, combatCharacter);
            //    return;
            //}

            if (_settings.UseAIPractice)
            {
                if (AIPracticeManager.HandleCombatUpdate(__instance, context, combatCharacter)) return;
            }

            List<Strategy> execStrategy = new List<Strategy>();

            // 自动执行策略
            if (_settings.AutoCastSkill)
            {
                execStrategy = AutoCastSkill(__instance, context, combatCharacter);
            }
            // 自动攻击
            if (_settings.AutoAttack && combatCharacter.MoveData.JumpPreparedFrame == 0 && combatCharacter.GetJumpPreparedDistance() == 0)
            {
                AutoAttack(__instance, context, combatCharacter);
            }
            // 自动移动
            if (_settings.AutoMove)
            {
                if (execStrategy.Find(x => (StrategyType)x.type == StrategyType.AutoMove) == null)
                {
                    AutoMove(__instance, combatCharacter);
                }
            }
        }

        /// <summary>
        /// 与前台程序通讯
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="operation"></param>
        /// <param name="argDataPool"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CombatDomain), "CallMethod")]
        public static bool CombatDomain_CallMethod_Prefix(CombatDomain __instance, Operation operation, RawDataPool argDataPool, RawDataPool returnDataPool, ref int __result)
        {
            bool result;
            if (operation.MethodId == GameDataBridgeConst.MethodId)
            {
                __result = -1;

                int num = operation.ArgsOffset;
                ushort num2 = 0;
                num += Serializer.Deserialize(argDataPool, num, ref num2);
                if (operation.ArgsCount == 2)
                {
                    switch (num2)
                    {
                        case GameDataBridgeConst.Flag.Flag_SwitchAutoMove:
                            {
                                if (_settings != null)
                                {
                                    bool autoMove = true;
                                    Serializer.Deserialize(argDataPool, num, ref autoMove);
                                    _settings.AutoMove = autoMove;
                                    bool flag4 = !_settings.AutoMove;
                                    if (flag4)
                                    {
                                        __instance.SetMoveState(0, true);
                                    }
                                }
                                break;
                            }
                        case GameDataBridgeConst.Flag.Flag_SwitchAutoAttack:
                            {
                                if (_settings != null)
                                {
                                    bool autoAttack = true;
                                    Serializer.Deserialize(argDataPool, num, ref autoAttack);
                                    _settings.AutoAttack = autoAttack;
                                }
                                break;
                            }
                        case GameDataBridgeConst.Flag.Flag_UpdateTargetDistance:
                            {
                                if (_settings != null)
                                {
                                    int targetDistance = 0;
                                    Serializer.Deserialize(argDataPool, num, ref targetDistance);
                                    _settings.TargetDistance = targetDistance;
                                }
                                break;
                            }
                        case GameDataBridgeConst.Flag.Flag_UpdateSettingsJson:
                            {
                                string json = null;
                                Serializer.Deserialize(argDataPool, num, ref json);
                                try
                                {
                                    _logger.Info("Flag_UpdateSettingsJson");
                                    _settings = JsonConvert.DeserializeObject<Settings>(json);
                                    _needRemoveTrick = _settings.RemoveTrick.Contains(true);
                                    if (!_settings.AutoMove || !_settings.isEnable)
                                    {
                                        __instance.SetMoveState(0, true);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.Warn("CombatStrategy Backend: Deserialize settings Json Failed:" + ex.Message, false);
                                    if (ConvenienceBackend.IsLocalTest()) throw;
                                }
                                break;
                            }
                        case GameDataBridgeConst.Flag.Flag_SwitchAutoCastSkill:
                            {
                                if (_settings != null)
                                {
                                    bool autoCastSkill = _settings.AutoCastSkill;
                                    Serializer.Deserialize(argDataPool, num, ref autoCastSkill);
                                    _settings.AutoCastSkill = autoCastSkill;
                                }
                                break;
                            }
                        case GameDataBridgeConst.Flag.Flag_UpdateStrategiesJson:
                            {
                                string json = null;
                                Serializer.Deserialize(argDataPool, num, ref json);
                                _logger.Info("Flag_UpdateStrategiesJson");

                                UpdateStrategies(JsonConvert.DeserializeObject<List<Strategy>>(json), __instance);
                                break;
                            }
                        case GameDataBridgeConst.Flag.Flag_AutoGenerateStrategy:
                            {
                                string json = null;
                                Serializer.Deserialize(argDataPool, num, ref json);
                                List<Strategy> strategies = JsonConvert.DeserializeObject<List<Strategy>>(json);
                                __result = Serializer.SerializeDefault<KeyValuePair<int, string>>(new KeyValuePair<int, string>(num2, JsonConvert.SerializeObject(AutoHelper.AutoGenerateStrategies())), returnDataPool);
                            }
                            break;
                    }
                }
                result = false;
            }
            else
            {
                // _logger.Info("CallMethod MethodId=" + operation.MethodId);
                result = true;
            }
            return result;
        }

        /// <summary>
        /// 指定式，空A
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="___CombatChar"></param>
        /// <param name="____trickType"></param>
        /// <param name="____inAttackRange"></param>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CombatCharacterStateAttack), "OnEnter")]
        public static void CombatCharacterStateAttack_OnEnter_Postfix(CombatCharacterStateAttack __instance, CombatCharacter ___CombatChar, sbyte ____trickType, ref bool ____inAttackRange)
        {
            if (!IsEnable()) return;
            if (!___CombatChar.IsAlly) return;

            if (_settings!= null && _settings.isEnable && _settings.RemoveTrick[____trickType]) 
            {
                ____inAttackRange = false;

                _logger.Info("我方空A " + Config.TrickType.Instance[____trickType].Name);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CombatCharacterStatePrepareSkill), "OnEnter")]
        public static void CombatCharacterStatePrepareSkill_OnEnter_Postfix(CombatCharacterStatePrepareSkill __instance, CombatCharacter ___CombatChar)
        {
            if (!IsEnable()) return;
            if (!___CombatChar.IsAlly) return;

            var skillId = ___CombatChar.GetPreparingSkillId();
            if (skillId == -1) return;

            if (_prepareSkillCountMap.ContainsKey(skillId))
            {
                _prepareSkillCountMap[skillId] = _prepareSkillCountMap[skillId] + 1;
            }
            else
            {
                _prepareSkillCountMap[skillId] = 1;
            }
        }

        /// <summary>
        /// 自动平A
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="context"></param>
        /// <param name="selfChar"></param>
        private static void AutoAttack(CombatDomain instance, DataContext context, CombatCharacter selfChar)
        {
            bool ignoreRange = _settings.IgnoreRange;
            if (ignoreRange)
            {
                instance.NormalAttack(context, true);
            }
            else
            {
                ValueTuple<byte, byte> distanceRange = instance.GetDistanceRange();
                byte item = distanceRange.Item1;
                byte item2 = distanceRange.Item2;
                int num = (int)selfChar.GetAttackRange().Inner;
                bool flag = num < (int)item2;
                if (flag)
                {
                    num -= _settings.AttackBufferMax;
                }
                int num2 = (int)selfChar.GetAttackRange().Outer;
                if ((int)selfChar.GetAttackRange().Outer > (int)item)
                {
                    num2 += _settings.AttackBufferMin;
                }
                if ((int)instance.GetCurrentDistance() <= num && (int)instance.GetCurrentDistance() >= num2)
                {
                    instance.NormalAttack(context, true);
                }
            }
        }

        /// <summary>
        /// 检查跳跃
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="selfChar"></param>
        private static void CheckJumping(CombatDomain instance, CombatCharacter selfChar)
        {
            short currentDistance = instance.GetCurrentDistance();
            ValueTuple<byte, byte> distanceRange = instance.GetDistanceRange();
            byte min = distanceRange.Item1;
            byte max = distanceRange.Item2;
            if (selfChar.MoveData.MaxJumpForwardDist > 0)
            {
                if (_settings.TargetDistance > (int)currentDistance)
                {
                    instance.SetMoveState(_settings.AllowOppositeMoveInJumpingSkill ? (byte)2 : (byte)0, true, true);
                }
                else
                {
                    int num = (int)(selfChar.MoveData.CanPartlyJump ? (selfChar.GetJumpPreparedDistance() + 10) : selfChar.MoveData.MaxJumpForwardDist);
                    int num2 = Math.Max((int)currentDistance - num, (int)min);
                    int minJumpPosition = _settings.MinJumpPosition;
                    if (!_settings.JumpPassTargetDistance && minJumpPosition < _settings.TargetDistance)
                    {
                        minJumpPosition = _settings.TargetDistance;
                    }
                    if (!_settings.JumpOutOfAttackRange && minJumpPosition < (int)selfChar.GetAttackRange().Outer && currentDistance > selfChar.GetAttackRange().Outer)
                    {
                        minJumpPosition = (int)selfChar.GetAttackRange().Outer;
                    }
                    int preparedDistance = (int)(currentDistance - selfChar.GetJumpPreparedDistance()); // 以当前蓄力准备要跳到的位置
                    if ((minJumpPosition == (int)min && minJumpPosition>0 && preparedDistance <= minJumpPosition) || 
                        num2 < minJumpPosition || 
                        (int)currentDistance < _settings.TargetDistance + _settings.DistanceAllowJumpForward
                        )
                    {
                        //if ((minJumpPosition == (int)min && num4 <= minJumpPosition))
                        //{
                        //    _logger.Info("停止移动因为minJumpPosition == (int)min && num4 <= minJumpPosition, minJumpPosition="+ minJumpPosition+", min="+min+ ", num4="+ num4);
                        //}
                        //else if (num2 < minJumpPosition)
                        //{
                        //    _logger.Info("停止移动因为num2 < minJumpPosition, minJumpPosition=" + minJumpPosition + ", num2=" + num2);

                        //}
                        //else
                        //{
                        //    _logger.Info("停止移动因为(int)currentDistance < _settings.TargetDistance + _settings.DistanceAllowJumpForward, currentDistance=" + currentDistance + ", _settings.TargetDistance=" + _settings.TargetDistance + ", _settings.DistanceAllowJumpForward="+ _settings.DistanceAllowJumpForward);
                        //}

                        instance.SetMoveState(0, true);
                    }
                    else
                    {
                        instance.SetMoveState(1, true, true);
                    }
                }
            }
            else
            {
                if (_settings.TargetDistance < (int)currentDistance)
                {
                    instance.SetMoveState(_settings.AllowOppositeMoveInJumpingSkill ? (byte)1 : (byte)0, true, true);
                }
                else
                {
                    int dist = (int)(selfChar.MoveData.CanPartlyJump ? (selfChar.GetJumpPreparedDistance() + 10) : selfChar.MoveData.MaxJumpBackwardDist);
                    int expectJumpPosition = Math.Min((int)currentDistance + dist, (int)max); // 预期能跳到的最大位置
                    int maxJumpPosition = _settings.MaxJumpPosition; // 预期最大能跳到的位置
                    if (!_settings.JumpPassTargetDistance && _settings.MaxJumpPosition > _settings.TargetDistance)
                    {
                        // 允许跳跃越过目标位置，但也只是到达目标位置
                        maxJumpPosition = _settings.TargetDistance;
                    }
                    if (!_settings.JumpOutOfAttackRange && maxJumpPosition > (int)selfChar.GetAttackRange().Inner && currentDistance < selfChar.GetAttackRange().Inner)
                    {
                        // 不允许跳过攻击距离范围
                        maxJumpPosition = (int)selfChar.GetAttackRange().Inner;
                    }
                    int preparedDistance = (int)(currentDistance + selfChar.GetJumpPreparedDistance()); // 以当前蓄力准备要跳到的位置
                    if (
                        (maxJumpPosition == (int)max && preparedDistance >= maxJumpPosition) ||
                        expectJumpPosition > maxJumpPosition ||  // 预期跳到的位置超过最大距离，则不跳
                        (int)currentDistance > _settings.TargetDistance - _settings.DistanceAllowJumpBackward 
                        )
                    {
                        instance.SetMoveState(0, true);
                    }
                    else
                    {
                        instance.SetMoveState(2, true, true);
                    }
                }
            }
        }

        /// <summary>
        /// 自动移动
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="selfChar"></param>
        private static void AutoMove(CombatDomain instance, CombatCharacter selfChar)
        {
            short currentDistance = instance.GetCurrentDistance();
            if ((int)currentDistance == _settings.TargetDistance)
            {
                instance.SetMoveState(0, true);
            }
            else
            {
                bool isMoveForward = (int)currentDistance > _settings.TargetDistance;
                byte moveState = isMoveForward ? (byte)1 : (byte)2;
                if (selfChar.MoveData.JumpMoveSkillId > -1)
                {
                    CheckJumping(instance, selfChar);
                }
                else
                {
                    short mobilityValue = (short)(selfChar.GetMobilityValue() * MOD_MAX_MOBILITY / selfChar.GetMaxMobility());
                    int maxMobility = selfChar.GetMaxMobility();
                    short mobilityRecoverPrepareValue = selfChar.GetMobilityRecoverPrepareValue();
                    if (selfChar.GetAffectingMoveSkillId() < 0)
                    {
                        if ((int)mobilityValue < _settings.MobilityRecoverCap)
                        {
                            instance.SetMoveState(0, true);
                        }
                        else
                        {
                            if ((int)mobilityValue >= (isMoveForward ? _settings.MobilityAllowForward : _settings.MobilityAllowBackward))
                            {
                                instance.SetMoveState(moveState, true, true);
                            }
                            else
                            {
                                instance.SetMoveState(0, true);
                            }
                        }
                    }
                    else
                    {
                        // 有身法就无需判断脚力
                        instance.SetMoveState(moveState, true, true);
                    }
                }
            }
        }

        private static int _switchWeaponsCD = 0;

        /// <summary>
        /// 自动施展
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="context"></param>
        /// <param name="selfChar"></param>
        private static List<Strategy> AutoCastSkill(CombatDomain instance, DataContext context, CombatCharacter selfChar)
        {
            var execedStrategyList = new List<Strategy>();

            foreach (Strategy strategy in _strategies)
            {
                // 条件不满足
                if (!CheckCondition(instance, selfChar, strategy.conditions)) continue;
                if ((StrategyType)strategy.type != StrategyType.AutoMove && (selfChar.MoveData.JumpPreparedFrame != 0 || selfChar.GetJumpPreparedDistance() != 0)) continue;

                switch (strategy.type)
                {
                    case (short)StrategyType.ReleaseSkill:
                        {
                            var skillId = strategy.skillId;
                            var skillItem = Config.CombatSkill.Instance[skillId];

                            if (skillItem.EquipType == CombatSkillEquipType.Neigong)
                            {
                                if (!OptCharacterHelper.CastCombatSkill(instance, context, selfChar, skillId)) break;

                                execedStrategyList.Add(strategy);
                                break;
                            }

                            CombatSkillData skillData = SkillUtils.GetCombatSkillData(instance, selfChar.GetId(), skillId);

                            // 无装备该功法
                            if (skillData == null) break;

                            // 检查功法施展范围
                            bool beyondRange = strategy.conditions.Find((Data.Condition condition) => condition.item == JudgeItem.Distance) == null;
                            if (beyondRange && skillData != null && skillItem.EquipType == CombatSkillEquipType.Attack)
                            {
                                if (540 == skillId || 410 == skillId)
                                {
                                    // 光相指、无极剑式无视攻击范围
                                    if (!skillData.GetCanUse())
                                    {
                                        break;// 不可用，直接break
                                    }
                                }
                                else
                                {
                                    // 检查催破功法是否在施法范围中
                                    OuterAndInnerInts skillAttackRange = instance.GetSkillAttackRange(selfChar, skillId);

                                    if ((int)instance.GetCurrentDistance() < skillAttackRange.Outer || (int)instance.GetCurrentDistance() > skillAttackRange.Inner)
                                    {
                                        // 不在施法范围中，跳过
                                        break;
                                    }
                                }
                            }

                            if (!OptCharacterHelper.CastCombatSkill(instance, context, selfChar, skillId)) break;
                            execedStrategyList.Add(strategy);
                            break;
                        }
                    case (short)StrategyType.ChangeTrick:
                        {
                            if (!selfChar.GetCanChangeTrick()) break;
                            if (selfChar.StateMachine.GetCurrentState().StateType == CombatCharacterStateType.Attack) break;
                            if (strategy.changeTrickAction == null) break;
                            if (strategy.changeTrickAction.trick == null) break;
                            var trickId = Config.TrickType.Instance[strategy.changeTrickAction.trick]?.TemplateId;
                            if (trickId == null) break;
                            // 当前没有这个式
                            if (selfChar.GetWeaponTricks().IndexOf((sbyte)trickId) < 0) break;

                            if (strategy.changeTrickAction.body == null) break;
                            var bodyId = Config.BodyPart.Instance[strategy.changeTrickAction.body]?.TemplateId;
                            if (bodyId == null) break;
                            if (selfChar.GetChangeTrickAttack()) break;
                            _logger.Info("变招[" + strategy.changeTrickAction.trick + "]" + "击打["+ strategy.changeTrickAction.body + "]");
                            instance.SelectChangeTrick(context, (sbyte)trickId, (sbyte)bodyId, true);
                            execedStrategyList.Add(strategy);
                            break;
                        }
                    case (short)StrategyType.SwitchWeapons:
                        {
                            if (strategy.switchWeaponAction == null) break;
                            var weaponIndex = strategy.switchWeaponAction.weaponIndex;
                            if (weaponIndex < 0) break;
                            if (weaponIndex == selfChar.GetUsingWeaponIndex()) break;
                            ItemKey weaponItemKey = selfChar.GetWeapons()[weaponIndex];
                            if (!weaponItemKey.IsValid()) break;
                            if (!instance.GetWeaponData(true, weaponItemKey).GetCanChangeTo()) break;
                            if (execedStrategyList.Find(x => x.type == strategy.type) != null) break;
                            if (selfChar.StateMachine.GetCurrentState().StateType != CombatCharacterStateType.Idle) break;
                            if (_switchWeaponsCD > 0)
                            {
                                _switchWeaponsCD = Math.Max(0, _switchWeaponsCD - 1);
                                break;
                            }
                            _switchWeaponsCD = 4;
                            instance.ChangeWeapon(context, weaponIndex, true);
                            _logger.Info("切换武器[" + (weaponIndex + 1) + "]");
                            execedStrategyList.Add(strategy);
                            break;
                        }
                    case (short)StrategyType.ExecTeammateCommand:
                        {
                            if (strategy.teammateCommandAction == null) break;

                            var comId = (sbyte)strategy.teammateCommandAction.id;
                            if (comId < 0) break;
                            
                            int[] selfTeam = instance.GetSelfTeam();
                            for (int i = 1; i < selfTeam.Length; i++)
                            {
                                var charId = selfTeam[i];
                                if (charId <= 0) continue;
                                instance.TryGetElement_CombatCharacterDict(charId, out CombatCharacter ally);
                                if (ally == null) continue;
                                var index = ally.GetCurrTeammateCommands().IndexOf(comId);
                                if (index < 0) continue;
                                if (!ally.GetTeammateCommandCanUse()[index]) continue;

                                instance.ExecuteTeammateCommand(context, selfChar.IsAlly, index, charId);
                                execedStrategyList.Add(strategy);
                                break;
                            }

                            break;
                        }
                    case (short)StrategyType.AutoMove:
                        {
                            if (strategy.autoMoveAction == null) break;

                            instance.SetMoveState((byte)strategy.autoMoveAction.type, true, true);
                            execedStrategyList.Add(strategy);
                            break;
                        }
                    case (short)StrategyType.NormalAttack:
                        {
                            if (strategy.normalAttackAction == null) break;

                            instance.NormalAttack(context, true);
                            execedStrategyList.Add(strategy);
                            break;
                        }
                    case (short)StrategyType.InterruptSkill:
                        {
                            // 打断功法
                            var skillId = strategy.skillId;
                            var skillItem = Config.CombatSkill.Instance[skillId];

                            CombatSkillData skillData = SkillUtils.GetCombatSkillData(instance, selfChar.GetId(), skillId);

                            // 无装备该功法
                            if (skillData == null) break;

                            if (selfChar.GetPreparingSkillId() == skillId)
                            {
                                instance.InterruptSkillManual(context, true);
                            }
                            else if (selfChar.GetAffectingDefendSkillId() == skillId)
                            {
                                instance.ClearAffectingDefenseSkillManual(context, true);
                            }

                            break;
                        }
                    default:
                        break;
                }
            }

            return execedStrategyList;
        }

        /// <summary>
        /// 检查策略执行条件
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="selfChar"></param>
        /// <param name="conditions"></param>
        /// <returns>true可用执行，false不能执行</returns>
        private static unsafe bool CheckCondition(CombatDomain instance, CombatCharacter selfChar, List<Data.Condition> conditions) 
        {
            bool canExecute = true;
            for (int i = 0; i < conditions.Count; i++)
            {
                Data.Condition condition = conditions[i];
                CombatCharacter combatCharacter = condition.isAlly ? selfChar : instance.GetCombatCharacter(false, false);
                JudgeItem item = condition.item;

                bool meetTheConditions = false;
                switch (item)
                {
                    case JudgeItem.Distance:
                        meetTheConditions = CheckCondition((int)instance.GetCurrentDistance(), condition);
                        break;
                    case JudgeItem.Mobility:
                        meetTheConditions = CheckCondition((int)(combatCharacter.GetMobilityValue() * MOD_MAX_MOBILITY / combatCharacter.GetMaxMobility()), condition);
                        break;
                    case JudgeItem.WeaponType:
                        meetTheConditions = CheckCondition((int)Config.Weapon.Instance[DomainManager.Item.GetElement_Weapons(instance.GetUsingWeaponKey(combatCharacter).Id).GetTemplateId()].ItemSubType, condition);
                        break;
                    case JudgeItem.PreparingAction:
                        if (condition.value < 3)
                        {
                            var value = -1;
                            if (combatCharacter.GetPreparingSkillId() >= 0)
                            {
                                value = (int)(Config.CombatSkill.Instance[combatCharacter.GetPreparingSkillId()].EquipType - 1);
                            }
                            else if (combatCharacter.NeedUseSkillId >= 0)
                            {
                                value = (int)(Config.CombatSkill.Instance[combatCharacter.NeedUseSkillId].EquipType - 1);
                            }
                            meetTheConditions = (condition.value == value && condition.judge == Judgement.Equals) || (condition.value != value && condition.judge != Judgement.Equals);
                        }
                        else
                        {
                            switch (condition.value)
                            {
                                case 3:
                                    // 治疗
                                    meetTheConditions = (combatCharacter.GetPreparingOtherAction() == 0 && condition.judge == Judgement.Equals) || (combatCharacter.GetPreparingOtherAction() != 0 && condition.judge != Judgement.Equals);
                                    break;
                                case 4:
                                    // 解毒
                                    meetTheConditions = (combatCharacter.GetPreparingOtherAction() == 1 && condition.judge == Judgement.Equals) || (combatCharacter.GetPreparingOtherAction() != 1 && condition.judge != Judgement.Equals);
                                    break;
                                case 5:
                                    // 逃跑
                                    meetTheConditions = (combatCharacter.GetPreparingOtherAction() == 2 && condition.judge == Judgement.Equals) || (combatCharacter.GetPreparingOtherAction() != 2 && condition.judge != Judgement.Equals);
                                    break;
                            }
                        }
                        break;
                    case JudgeItem.SkillMobility:
                        meetTheConditions = CheckCondition((int)(combatCharacter.GetSkillMobility() * 1000 / GlobalConfig.Instance.AgileSkillMobility), condition);
                        break;
                    case JudgeItem.HasTrick:
                        meetTheConditions = ((condition.subType < 0) ? CheckCondition(combatCharacter.GetTricks().Tricks.Count, condition) : CheckCondition((int)combatCharacter.GetTrickCount((sbyte)condition.subType), condition));
                        break;
                    case JudgeItem.HasSkillEffect:
                        var findSkill = false;
                        var effectDict = combatCharacter.GetSkillEffectCollection().EffectDict;
                        if (effectDict != null)
                        {
                            foreach (KeyValuePair<SkillEffectKey, short> kvp in effectDict)
                            {
                                // _logger.Info("有功法效果: " + kvp.Key.SkillId + " " + kvp.Value);
                                if (kvp.Key.SkillId == condition.subType)
                                {
                                    findSkill = true;
                                    if (CheckCondition(kvp.Value, condition))
                                    {
                                        meetTheConditions = true;
                                        break;
                                    }
                                }
                            }
                        }

                        combatCharacter.GetBuffCombatStateCollection();
                        if (meetTheConditions) break;
                        meetTheConditions = !findSkill && CheckCondition(0, condition);
                        break;
                    case JudgeItem.Stance:
                        meetTheConditions = CheckCondition(combatCharacter.GetStanceValue() * 100 / combatCharacter.GetMaxStanceValue(), condition);
                        break;
                    case JudgeItem.Breath:
                        meetTheConditions = CheckCondition(combatCharacter.GetBreathValue() * 100 / combatCharacter.GetMaxBreathValue(), condition);
                        break;
                    case JudgeItem.CurrentTrick:
                        var weaponTricks = combatCharacter.GetWeaponTricks();
                        var currentTrickIndex = combatCharacter.GetWeaponTrickIndex();
                        var trick = weaponTricks[currentTrickIndex];
                        _logger.Info("weaponTricks = " + trick + ", condition.value = " + condition.value);
                        if (trick == condition.value && condition.judge == Judgement.Equals)
                        {
                            meetTheConditions = true;
                        }
                        else if (trick != condition.value && condition.judge != Judgement.Equals)
                        {
                            meetTheConditions = true;
                        }
                        break;
                    case JudgeItem.DefeatMarkCount:
                        var defeatMarkCollection = combatCharacter.GetDefeatMarkCollection();
                        switch (condition.subType)
                        {
                            case 0:
                                // 总战败标记数量
                                meetTheConditions = CheckCondition(defeatMarkCollection.GetTotalCount(), condition);
                                break;
                            case 1:
                                // 破绽标记
                                meetTheConditions = CheckCondition(defeatMarkCollection.GetTotalFlawCount(), condition);
                                break;
                            case 2:
                                // 点穴标记
                                meetTheConditions = CheckCondition(defeatMarkCollection.GetTotalAcupointCount(), condition);
                                break;
                            case 3:
                                // 心神
                                meetTheConditions = CheckCondition(defeatMarkCollection.MindMarkList.Count, condition);
                                break;
                            case 4:
                                // 外伤
                                meetTheConditions = CheckCondition(defeatMarkCollection.OuterInjuryMarkList.Sum(), condition);
                                break;
                            case 5:
                                // 内伤
                                meetTheConditions = CheckCondition(defeatMarkCollection.InnerInjuryMarkList.Sum(), condition);
                                break;
                            case 6:
                                // 中毒
                                meetTheConditions = CheckCondition(defeatMarkCollection.PoisonMarkList.Sum(), condition);
                                break;
                            case 7:
                                // 重创
                                meetTheConditions = CheckCondition(defeatMarkCollection.FatalDamageMarkCount, condition);
                                break;
                        }
                        break;
                    case JudgeItem.CanUseSkill:
                        {
                            var skillId = (short)condition.subType;
                            CombatSkillKey combatSkillKey = new CombatSkillKey(combatCharacter.GetId(), skillId);
                            CombatSkillData skillData = null;
                            instance.TryGetElement_SelfSkillDataDict(combatSkillKey, out skillData);

                            if (skillData != null)
                            {
                                if (skillData.GetCanUse())
                                {
                                    meetTheConditions = condition.value == 0;
                                }
                                else
                                {
                                    meetTheConditions = condition.value == 1;
                                }
                            }
                            else if (condition.value == 1)
                            {
                                meetTheConditions = true;
                            }
                            else
                            {
                                meetTheConditions = false;
                            }
                            break;
                        }
                    case JudgeItem.AffectingSkill:
                        {
                            var skillId = (short)condition.subType;
                            if (condition.value == 0 && (combatCharacter.GetAffectingDefendSkillId() == skillId || combatCharacter.GetAffectingMoveSkillId() == skillId))
                            {
                                meetTheConditions = true;
                            }
                            else if (condition.value == 1 && combatCharacter.GetAffectingDefendSkillId() != skillId && combatCharacter.GetAffectingMoveSkillId() != skillId)
                            {
                                meetTheConditions = true;
                            }
                            else
                            {
                                meetTheConditions = false;
                            }
                            break;
                        }
                    case JudgeItem.Buff:
                    case JudgeItem.Debuff:
                        {
                            var totalPower = 0;

                            var specialEffectDataId = (short)condition.subType;

                            var combatStateCollection = item == JudgeItem.Buff ? combatCharacter.GetBuffCombatStateCollection() : combatCharacter.GetDebuffCombatStateCollection();
                            foreach (KeyValuePair<short, ValueTuple<short, bool, int>> buff in combatStateCollection.StateDict)
                            {
                                var stateId = buff.Key;
                                CombatStateItem configData = CombatState.Instance[stateId];
                                if (configData == null) continue;

                                var property = configData.PropertyList.Find(x => {
                                    // _logger.Info("stateId = " + stateId + ", SpecialEffectDataId = " + x.SpecialEffectDataId);
                                    return x.SpecialEffectDataId == specialEffectDataId;
                                });
                                if (property.SpecialEffectDataId != specialEffectDataId) continue;

                                short power = buff.Value.Item1;
                                bool reverse = buff.Value.Item2;
                                int srcCharId = buff.Value.Item3;

                                totalPower += (int)(property.Value * power / 100);
                            }

                            totalPower = Math.Abs(totalPower);
                            meetTheConditions = CheckCondition(totalPower, condition);
                            break;
                        }
                    case JudgeItem.WugCount:
                        meetTheConditions = CheckCondition(combatCharacter.GetWugCount(), condition);
                        break;
                    case JudgeItem.CharacterAttribute:
                        {
                            var subType = condition.subType;
                            if (subType == 0)
                            {
                                // 精纯
                                meetTheConditions = CheckCondition(combatCharacter.GetCharacter().GetConsummateLevel(), condition);
                            }
                            else if (subType < 5)
                            {
                                // 催破1 轻灵2 护体3 奇窍4
                                var neiliAllocation = combatCharacter.GetNeiliAllocation();
                                meetTheConditions = CheckCondition(neiliAllocation.Items[subType - 1], condition);
                            }
                            else if (subType < 11)
                            {
                                // 烈毒5 郁毒6 赤毒7 寒毒8 腐毒9 幻毒10
                                var poison = combatCharacter.GetPoison();
                                meetTheConditions = CheckCondition(poison.Items[subType - 5], condition);
                            }
                            else if (subType == 11)
                            {
                                // 内息
                                meetTheConditions = CheckCondition(combatCharacter.GetCharacter().GetDisorderOfQi() / 10, condition);
                            }
                        }
                        break;
                    case JudgeItem.NumOfPrepareSkill:
                        {
                            var count = 0;

                            var skillId = (short)condition.subType;

                            if (_prepareSkillCountMap.ContainsKey(skillId))
                            {
                                count = _prepareSkillCountMap[skillId];
                            }
                            meetTheConditions = CheckCondition(count, condition);
                        }
                        break;
                    case JudgeItem.NumOfChangeTrickCount:
                        {
                            meetTheConditions = CheckCondition(combatCharacter.GetChangeTrickCount(), condition);
                        }
                        break;
                    case JudgeItem.EquippingSkill:
                        {
                            var skillId = (short)condition.subType;
                            CombatSkillData skillData = SkillUtils.GetCombatSkillData(instance, selfChar.GetId(), skillId);

                            if (condition.value == 0 && skillData != null)
                            {
                                meetTheConditions = true;
                            }
                            else if (condition.value == 1 && skillData == null)
                            {
                                meetTheConditions = true;
                            }
                            else
                            {
                                meetTheConditions = false;
                            }
                        }
                        break;
                    case JudgeItem.DirectionSkill:
                        {
                            var skillId = (short)condition.subType;
                            GameData.Domains.CombatSkill.CombatSkill skill = SkillUtils.GetCombatSkill(selfChar.GetId(), skillId);

                            meetTheConditions = condition.value == skill.GetDirection();
                        }
                        break;
                    case JudgeItem.SkillBanState:
                        {
                            var skillId = (short)condition.subType;

                            if (combatCharacter.GetBannedSkillIds().Contains(skillId))
                            {
                                meetTheConditions = condition.value == 0;
                            }
                            else
                            {
                                meetTheConditions = condition.value == 1;
                            }
                        }
                        break;
                    case JudgeItem.SkillProgress:
                        {
                            var skillId = (short)condition.subType;

                            var skillIdNotEquals = combatCharacter.GetPreparingSkillId() != skillId;

                            if (condition.value <= 0)
                            {
                                // 判断技能不在施展就可以了
                                meetTheConditions = skillIdNotEquals;
                            }
                            else if (!skillIdNotEquals)
                            {
                                meetTheConditions = CheckCondition(combatCharacter.GetSkillPreparePercent(), condition);
                            }
                        }
                        break;
                    case JudgeItem.SkillProficiency:
                        {
                            var skillId = (short)condition.subType;
                            GameData.Domains.CombatSkill.CombatSkill skill = SkillUtils.GetCombatSkill(selfChar.GetId(), skillId);
                            meetTheConditions = CheckCondition(skill.GetPracticeLevel(), condition);
                        }
                        break;
                    default:
                        meetTheConditions = false;
                        break;
                }
                canExecute = meetTheConditions;
                if (!canExecute)
                {
                    // 只要不满足条件则跳过
                    break;
                }
            }
            return canExecute;
        }

        /// <summary>
        /// 检查条件
        /// </summary>
        /// <param name="checkNum"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        private static bool CheckCondition(int checkNum, Data.Condition condition)
        {
            Judgement judge = condition.judge;
            bool result;
            if (judge != Judgement.Equals)
            {
                if (judge != Judgement.Greater)
                {
                    result = (checkNum < condition.value);
                }
                else
                {
                    result = (checkNum > condition.value);
                }
            }
            else
            {
                result = (checkNum == condition.value);
            }
            return result;
        }

        // 更新策略
        private static void UpdateStrategies(List<Strategy> strategies, CombatDomain instance)
        {
            lock (_strategies)
            {
                strategies?.ForEach(delegate (Strategy strategy)
                    {
                        switch (strategy.type)
                        {
                            case (short)StrategyType.ReleaseSkill:
                                break;
                            case (short)StrategyType.ChangeTrick:
                                break;
                            case (short)StrategyType.SwitchWeapons:
                                break;
                        }
                    });
                _strategies = strategies;
            }
        }

        private static bool IsEnable()
        {
            return _replaceAi;
        }

        // Token: 0x04000014 RID: 20
        private static Settings _settings;

        // Token: 0x04000016 RID: 22
        private static List<Strategy> _strategies = new List<Strategy>();

        private static bool _needRemoveTrick;

        // Token: 0x0400001A RID: 26
        private static bool _replaceAi;
    }
}
