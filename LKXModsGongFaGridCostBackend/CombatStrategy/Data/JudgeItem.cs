using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvenienceBackend.CombatStrategy.Data
{
    public enum JudgeItem
    {
        /// <summary>
        /// 距离
        /// </summary>
        Distance,
        /// <summary>
        /// 脚力
        /// </summary>
        Mobility,
        /// <summary>
        /// 装备
        /// </summary>
        WeaponType,
        /// <summary>
        /// 正在施展
        /// </summary>
        PreparingAction,
        /// <summary>
        /// 身法值
        /// </summary>
        SkillMobility,
        /// <summary>
        /// 式的数量
        /// </summary>
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
    }
}
