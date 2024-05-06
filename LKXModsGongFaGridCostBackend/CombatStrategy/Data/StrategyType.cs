using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvenienceBackend.CombatStrategy.Data
{
    public enum StrategyType
    {
        /// <summary>
        /// 释放技能
        /// </summary>
        ReleaseSkill = 0,
        /// <summary>
        /// 变招
        /// </summary>
        ChangeTrick = 1,

        /// <summary>
        /// 切换武器
        /// </summary>
        SwitchWeapons = 2,

        /// <summary>
        /// 执行队友指令
        /// </summary>
        ExecTeammateCommand = 3,

        /// <summary>
        /// 自动移动
        /// </summary>
        AutoMove = 4,

        /// <summary>
        /// 普通攻击
        /// </summary>
        NormalAttack = 5,

        /// <summary>
        /// 打断功法
        /// </summary>
        InterruptSkill = 6,
    }
}
