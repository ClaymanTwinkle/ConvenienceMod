using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvenienceBackend.CombatStrategy
{
    public enum JudgeItem
    {
        // Token: 0x04000006 RID: 6
        Distance,
        // Token: 0x04000007 RID: 7
        Mobility,
        // Token: 0x04000008 RID: 8
        WeaponType,
        // Token: 0x04000009 RID: 9
        PreparingSkillType,
        // Token: 0x0400000A RID: 10
        SkillMobility,
        // Token: 0x0400000B RID: 11
        HasTrick,

        /// <summary>
        /// 功法效果层数
        /// </summary>
        HasSkillEffect,

        /// <summary>
        /// 架势
        /// </summary>
        Stance,

        /// <summary>
        /// 气势
        /// </summary>
        Breath,

        /// <summary>
        /// 当前招式
        /// </summary>
        CurrentTrick,

        /// <summary>
        /// 战败标记
        /// </summary>
        DefeatMarkCount,

        /// <summary>
        /// 满足施展条件
        /// </summary>
        CanUseSkill,

        /// <summary>
        /// 运功中
        /// </summary>
        AffectingSkill,

        /// <summary>
        /// 增益效果
        /// </summary>
        Buff,

        /// <summary>
        /// 减益效果
        /// </summary>
        Debuff,

        /// <summary>
        /// 蛊引
        /// </summary>
        WugCount
    }
}
