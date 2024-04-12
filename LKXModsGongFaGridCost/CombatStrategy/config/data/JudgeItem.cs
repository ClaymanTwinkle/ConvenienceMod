using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvenienceFrontend.CombatStrategy
{
    public enum JudgeItem
    {
        // Token: 0x0400003C RID: 60
        None = -1,
        
        /// <summary>
        /// 距离
        /// </summary>
        Distance,
        
        /// <summary>
        /// 脚力
        /// </summary>
        Mobility,

        /// <summary>
        /// 武器类型
        /// </summary>
        WeaponType,

        /// <summary>
        /// 施展的动作
        /// </summary>
        PreparingAction,
        
        /// <summary>
        /// 身法值
        /// </summary>
        SkillMobility,
        
        /// <summary>
        /// 有式
        /// </summary>
        HasTrick,

        /// <summary>
        /// 功法效果层数
        /// </summary>
        HasSkillEffect,

        /// <summary>
        /// 架势值
        /// </summary>
        Stance,

        /// <summary>
        /// 气势值
        /// </summary>
        Breath,

        /// <summary>
        /// 当前招式
        /// </summary>
        CurrentTrick,

        /// <summary>
        /// 战败标记数量
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
        WugCount,

        /// <summary>
        /// 人物属性
        /// </summary>
        CharacterAttribute,

        /// <summary>
        /// 技能施展次数
        /// </summary>
        NumOfPrepareSkill,

        /// <summary>
        /// 变招数量
        /// </summary>
        NumOfChangeTrickCount,


        /// <summary>
        /// 是否有装备技能
        /// </summary>
        EquippingSkill
    }
}
