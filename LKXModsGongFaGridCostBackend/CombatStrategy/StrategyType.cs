using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvenienceBackend.CombatStrategy
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
    }
}
