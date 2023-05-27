using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using GameData.Domains.Combat;

namespace ConvenienceBackend.CombatStrategy
{
    // Token: 0x02000005 RID: 5
    public class Strategy
    {
        // 执行条件
        public List<Condition> conditions { get; set; }

        // 技能id，默认-1
        public short skillId { get; set; }

        /// <summary>
        /// 变招
        /// </summary>
        public ChangeTrickAction changeTrickAction { get; set; }

        /// <summary>
        /// 切换武器
        /// </summary>
        public SwitchWeaponAction switchWeaponAction { get; set; }

        /// <summary>
        /// 队友指令
        /// </summary>
        public TeammateCommandAction teammateCommandAction { get; set; }

        /// <summary>
        /// 自动移动
        /// </summary>
        public AutoMoveAction autoMoveAction { get; set; }

        /// <summary>
        /// 普通攻击
        /// </summary>
        public NormalAttackAction normalAttackAction { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public short type { get; set; }

        // 角色当前可用的技能
        [JsonIgnore]
        public CombatSkillData skillData;
    }
}
