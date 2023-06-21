using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using BehTree;
using Config;
using ConvenienceBackend.CombatStrategy.Data;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;
using GameData.GameDataBridge;
using GameData.Serializer;
using GameData.Utilities;
using HarmonyLib;
using Newtonsoft.Json;

namespace ConvenienceBackend.CombatStrategy
{
    internal class CombatStrategyBackendPatch: BaseBackendPatch
    {
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
            AdaptableLog.Info("CombatCharacter::InitTeammateCommand");
            var currTeammateCommands = __instance.GetCurrTeammateCommands();
            AdaptableLog.Info("角色[" + __instance.GetId() + "]有" + String.Join(",", currTeammateCommands));
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CombatDomain), "SetPlayerAutoCombat")]
        public static void CombatDomain_SetPlayerAutoCombat_Prefix(DataContext context, ref bool autoCombat)
        {
            if (IsEnable() && autoCombat)
            {
                autoCombat = false;
                AdaptableLog.Info("系统设置打开原版自动战斗，但是因为战斗策略打开，所有禁用原版的自动战斗");
            }
        }

        // Token: 0x06000018 RID: 24 RVA: 0x00002858 File Offset: 0x00000A58
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CombatDomain), "OnUpdate")]
        public static void CombatDomain_OnUpdate_Postfix(CombatDomain __instance, DataContext context)
        {
            // 没有开启战斗策略
            if (!IsEnable()) return;

            // 没有配置
            if (_settings == null) return;

            // 系统自动战斗打开了
            if (__instance.GetAutoCombat()) return;

            // 关闭了战斗策略
            if (!_settings.isEnable) return;

            // 不是玩家
            CombatCharacter combatCharacter = __instance.GetCombatCharacter(true, false);
            if (!__instance.IsMainCharacter(combatCharacter)) return;

            List<Strategy> execStrategy = new List<Strategy>();

            // 自动施法
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
                                    _settings = JsonConvert.DeserializeObject<Settings>(json);
                                    _needRemoveTrick = _settings.RemoveTrick.Contains(true);
                                    if (!_settings.AutoMove || !_settings.isEnable)
                                    {
                                        __instance.SetMoveState(0, true);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    AdaptableLog.Warning("CombatStrategy Backend: Deserialize settings Json Failed:" + ex.Message, false);
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
        public static void CCombatCharacterStateAttack_OnEnter_Postfix(CombatCharacterStateAttack __instance, CombatCharacter ___CombatChar, sbyte ____trickType, ref bool ____inAttackRange)
        {
            if (!IsEnable()) return;
            if (!___CombatChar.IsAlly) return;

            if (_settings!= null && _settings.isEnable && _settings.RemoveTrick[____trickType]) 
            {
                ____inAttackRange = false;

                AdaptableLog.Info("我方空A " + Config.TrickType.Instance[____trickType].Name);
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
            byte item = distanceRange.Item1;
            byte item2 = distanceRange.Item2;
            bool flag = selfChar.MoveData.MaxJumpForwardDist > 0;
            if (flag)
            {
                if (_settings.TargetDistance > (int)currentDistance)
                {
                    instance.SetMoveState(_settings.AllowOppositeMoveInJumpingSkill ? (byte)2 : (byte)0, true);
                }
                else
                {
                    int num = (int)(selfChar.MoveData.CanPartlyJump ? (selfChar.GetJumpPreparedDistance() + 10) : selfChar.MoveData.MaxJumpForwardDist);
                    int num2 = Math.Max((int)currentDistance - num, (int)item);
                    int num3 = _settings.MinJumpPosition;
                    bool flag3 = !_settings.JumpPassTargetDistance && num3 < _settings.TargetDistance;
                    if (flag3)
                    {
                        num3 = _settings.TargetDistance;
                    }
                    bool flag4 = !_settings.JumpOutOfAttackRange && num3 < (int)selfChar.GetAttackRange().Outer && currentDistance > selfChar.GetAttackRange().Outer;
                    if (flag4)
                    {
                        num3 = (int)selfChar.GetAttackRange().Outer;
                    }
                    int num4 = (int)(currentDistance - selfChar.GetJumpPreparedDistance());
                    bool flag5 = (num3 == (int)item && num4 <= num3) || num2 < num3 || (int)currentDistance < _settings.TargetDistance + _settings.DistanceAllowJumpForward;
                    if (flag5)
                    {
                        instance.SetMoveState(0, true);
                    }
                    else
                    {
                        instance.SetMoveState(1, true);
                    }
                }
            }
            else
            {
                bool flag6 = _settings.TargetDistance < (int)currentDistance;
                if (flag6)
                {
                    instance.SetMoveState(_settings.AllowOppositeMoveInJumpingSkill ? (byte)1 : (byte)0, true);
                }
                else
                {
                    int num5 = (int)(selfChar.MoveData.CanPartlyJump ? (selfChar.GetJumpPreparedDistance() + 10) : selfChar.MoveData.MaxJumpBackwardDist);
                    int num6 = Math.Min((int)currentDistance + num5, (int)item2);
                    int num7 = _settings.MaxJumpPosition;
                    bool flag7 = _settings.JumpPassTargetDistance && num7 > _settings.TargetDistance;
                    if (flag7)
                    {
                        num7 = _settings.TargetDistance;
                    }
                    bool flag8 = !_settings.JumpOutOfAttackRange && num7 > (int)selfChar.GetAttackRange().Inner && currentDistance < selfChar.GetAttackRange().Inner;
                    if (flag8)
                    {
                        num7 = (int)selfChar.GetAttackRange().Inner;
                    }
                    int num8 = (int)(currentDistance + selfChar.GetJumpPreparedDistance());
                    bool flag9 = (num7 == (int)item2 && num8 >= num7) || num6 > num7 || (int)currentDistance > _settings.TargetDistance - _settings.DistanceAllowJumpBackward;
                    if (flag9)
                    {
                        instance.SetMoveState(0, true);
                    }
                    else
                    {
                        instance.SetMoveState(2, true);
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
                    short mobilityValue = selfChar.GetMobilityValue();
                    int maxMobility = selfChar.GetMaxMobility();
                    short mobilityRecoverPrepareValue = selfChar.GetMobilityRecoverPrepareValue();
                    if (selfChar.GetAffectingMoveSkillId() < 0)
                    {
                        if ((int)mobilityRecoverPrepareValue >= maxMobility && (int)mobilityValue < _settings.MobilityRecoverCap)
                        {
                            instance.SetMoveState(0, true);
                        }
                        else
                        {
                            if ((int)mobilityValue > (isMoveForward ? _settings.MobilityAllowForward : _settings.MobilityAllowBackward))
                            {
                                instance.SetMoveState(moveState, true);
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
                        instance.SetMoveState(moveState, true);
                    }
                }
            }
        }

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
                            // 无装备该功法
                            if (strategy.skillData == null) break;
                            // 功法目前不可使用
                            if (!strategy.skillData.GetCanUse()) break;
                            // 检查功法施展范围
                            bool beyondRange = strategy.conditions.Find((Data.Condition condition) => condition.item == JudgeItem.Distance) == null;
                            if (beyondRange && strategy.skillData != null && Config.CombatSkill.Instance[strategy.skillId].EquipType == 1)
                            {
                                // 检查催破功法是否在施法范围中
                                OuterAndInnerInts skillAttackRange = instance.GetSkillAttackRange(selfChar, strategy.skillId);
                                if ((int)instance.GetCurrentDistance() < skillAttackRange.Outer || (int)instance.GetCurrentDistance() > skillAttackRange.Inner)
                                {
                                    // 不在施法范围中，跳过
                                    break;
                                }
                            }
                            // 同一个身法没必要重复施展
                            if (selfChar.GetAffectingMoveSkillId() == strategy.skillId) break;
                            instance.StartPrepareSkill(context, strategy.skillId, true);
                            execedStrategyList.Add(strategy);
                            break;
                        }
                    case (short)StrategyType.ChangeTrick:
                        {
                            if (!selfChar.GetCanChangeTrick()) break;
                            if (strategy.changeTrickAction == null) break;
                            if (strategy.changeTrickAction.trick == null) break;
                            var trickId = Config.TrickType.Instance[strategy.changeTrickAction.trick]?.TemplateId;
                            if (trickId == null) break;
                            // 当前没有这个式
                            if (selfChar.GetWeaponTricks().IndexOf((sbyte)trickId) < 0) break;

                            if (strategy.changeTrickAction.body == null) break;
                            var bodyId = Config.BodyPart.Instance[strategy.changeTrickAction.body]?.TemplateId;
                            if (bodyId == null) break;

                            instance.SelectChangeTrick(context, (sbyte)trickId, (sbyte)bodyId, true);
                            execedStrategyList.Add(strategy);
                            break;
                        }
                    case (short)StrategyType.SwitchWeapons:
                        {
                            if (strategy.switchWeaponAction == null) break;
                            if (strategy.switchWeaponAction.weaponIndex < 0) break;
                            if (strategy.switchWeaponAction.weaponIndex == selfChar.GetUsingWeaponIndex()) break;

                            instance.ChangeWeapon(context, strategy.switchWeaponAction.weaponIndex, true);
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

                            instance.SetMoveState((byte)strategy.autoMoveAction.type, true);
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
        private static bool CheckCondition(CombatDomain instance, CombatCharacter selfChar, List<Data.Condition> conditions) 
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
                        meetTheConditions = CheckCondition((int)combatCharacter.GetMobilityValue(), condition);
                        break;
                    case JudgeItem.WeaponType:
                        meetTheConditions = CheckCondition((int)Config.Weapon.Instance[DomainManager.Item.GetElement_Weapons(instance.GetUsingWeaponKey(combatCharacter).Id).GetTemplateId()].ItemSubType, condition);
                        break;
                    case JudgeItem.PreparingAction:
                        if (condition.value < 3)
                        {
                            meetTheConditions = (combatCharacter.GetPreparingSkillId() >= 0 && CheckCondition((int)(Config.CombatSkill.Instance[combatCharacter.GetPreparingSkillId()].EquipType - 1), condition));
                        }
                        else
                        {
                            switch (condition.value)
                            {
                                case 3:
                                    // 治疗
                                    meetTheConditions = combatCharacter.GetPreparingOtherAction() == 0;
                                    break;
                                case 4:
                                    // 解毒
                                    meetTheConditions = combatCharacter.GetPreparingOtherAction() == 1;
                                    break;
                                case 5:
                                    // 逃跑
                                    meetTheConditions = combatCharacter.GetPreparingOtherAction() == 2;
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
                                AdaptableLog.Info("有功法效果: " + kvp.Key.SkillId + " " + kvp.Value);
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
                        var trickName = Config.TrickType.Instance[trick].Name;
                        if (trickName == condition.valueStr && condition.judge == Judgement.Equals)
                        {
                            meetTheConditions = true;
                        }
                        else if (trickName != condition.valueStr && condition.judge != Judgement.Equals)
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

                                var property = configData.PropertyList.Find(x => x.SpecialEffectDataId == specialEffectDataId);
                                if (property.SpecialEffectDataId != specialEffectDataId) continue;

                                short power = buff.Value.Item1;
                                bool reverse = buff.Value.Item2;
                                int srcCharId = buff.Value.Item3;

                                totalPower = (int)(property.Value * power / 100);
                            }

                            meetTheConditions = CheckCondition(totalPower, condition);

                            break;
                        }
                    case JudgeItem.WugCount:
                        meetTheConditions = CheckCondition(combatCharacter.GetWugCount(), condition);
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
                                CombatSkillKey combatSkillKey = new CombatSkillKey(instance.GetSelfTeam()[0], strategy.skillId);
                                CombatSkillData skillData = null;
                                instance.TryGetElement_SelfSkillDataDict(combatSkillKey, out skillData);
                                strategy.skillData = skillData;
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
