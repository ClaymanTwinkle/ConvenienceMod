using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvenienceFrontend.CombatStrategy.config.data;

namespace ConvenienceFrontend.CombatStrategy
{
    [Serializable]
    public class Strategy
    {
        /// <summary>
        /// 是否是完整可用的策略
        /// </summary>
        /// <returns></returns>
        public bool IsComplete()
        {
            switch ((StrategyConst.StrategyType)type)
            {
                case StrategyConst.StrategyType.ReleaseSkill:
                case StrategyConst.StrategyType.InterruptSkill:
                    if (this.skillId < 0) return false;
                    break;
                case StrategyConst.StrategyType.ChangeTrick:
                    if (changeTrickAction ==null) return false;
                    break;
                case StrategyConst.StrategyType.SwitchWeapons:
                    if (switchWeaponAction == null) return false;
                    break;
                case StrategyConst.StrategyType.ExecTeammateCommand:
                    if (teammateCommandAction == null) return false;
                    break;
                case StrategyConst.StrategyType.AutoMove:
                    if (autoMoveAction == null) return false;
                    break;
                case StrategyConst.StrategyType.NormalAttack:
                    if (normalAttackAction == null) return false;
                    break;
                default: return false;
            }

            for (int i = 0; i < this.conditions.Count; i++)
            {
                if (!this.conditions[i].IsComplete())
                {
                    return false;
                }
            }

            return true;
        }

        public void SetAction(short skillId)
        { 
            this.skillId = skillId;
            changeTrickAction = null;
            switchWeaponAction = null;
            teammateCommandAction = null;
            autoMoveAction= null;
            normalAttackAction = null;
        }

        public void SetAction(ChangeTrickAction changeTrickAction)
        {
            this.changeTrickAction = changeTrickAction;
            this.skillId = -1;
            switchWeaponAction = null;
            teammateCommandAction = null;
            autoMoveAction = null;
            normalAttackAction = null;
        }

        public void SetAction(SwitchWeaponAction switchWeaponAction)
        {
            this.switchWeaponAction = switchWeaponAction;
            this.skillId = -1;
            changeTrickAction = null;
            teammateCommandAction = null;
            autoMoveAction = null;
            normalAttackAction = null;
        }

        public void SetAction(TeammateCommandAction teammateCommandAction)
        { 
            this.teammateCommandAction = teammateCommandAction;
            this.skillId = -1;
            changeTrickAction = null;
            switchWeaponAction = null;
            autoMoveAction = null;
            normalAttackAction = null;
        }

        public void SetAction(AutoMoveAction autoMoveAction)
        { 
            this.autoMoveAction = autoMoveAction;
            teammateCommandAction = null;
            skillId = -1;
            changeTrickAction = null;
            switchWeaponAction = null;
            normalAttackAction = null;
        }

        public void SetAction(NormalAttackAction normalAttackAction)
        {
            autoMoveAction = null;
            teammateCommandAction = null;
            skillId = -1;
            changeTrickAction = null;
            switchWeaponAction = null;
            this.normalAttackAction = normalAttackAction;
        }

        // 技能templateId
        public short skillId = -1;

        /// <summary>
        /// 变招
        /// </summary>
        public ChangeTrickAction changeTrickAction = null;

        /// <summary>
        /// 切换武器
        /// </summary>
        public SwitchWeaponAction switchWeaponAction = null;

        /// <summary>
        /// 队友指令
        /// </summary>
        public TeammateCommandAction teammateCommandAction = null;

        /// <summary>
        /// 自动移动
        /// </summary>
        public AutoMoveAction autoMoveAction = null;

        /// <summary>
        /// 普通攻击
        /// </summary>
        public NormalAttackAction normalAttackAction = null;

        // 类型
        // 0: 释放技能
        // 1: 变招
        // 2: 切换武器
        public short type = 0;

        // 释放启用
        public bool enabled = false;

        // 执行条件
        public List<Condition> conditions = new List<Condition>();
    }
}
