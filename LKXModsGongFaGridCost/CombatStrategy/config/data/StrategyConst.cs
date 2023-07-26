using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Config;
using TaiwuModdingLib.Core.Utils;
using UnityEngine;

namespace ConvenienceFrontend.CombatStrategy
{
    public class StrategyConst
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
        }

        // Token: 0x04000032 RID: 50
        public static readonly StrategyConst.Item[] ItemOptions = new StrategyConst.Item[]
        {
            new StrategyConst.Item("距离", true, 10f, -1, false, 0),              // 0
            new StrategyConst.Item("脚力", true, 10f, -1, false, 0),              // 1  
            new StrategyConst.Item("装备", false, 1f, 0, false, -1),               // 2
            new StrategyConst.Item("正在施展", false, 1f, 1, false, 1),           // 3
            new StrategyConst.Item("身法值", true, 10f, -1, false, 0),            // 4
            new StrategyConst.Item("式的数量", true, 1f, 2, false, 0),            // 5
            new StrategyConst.Item("功法效果层数", true, 1f, -1, true, 0),         // 6
            new StrategyConst.Item("架势", true, 1f, -1, false, 0),               // 7
            new StrategyConst.Item("提气", true, 1f, -1, false, 0),               // 8
            new StrategyConst.Item("当前式", false, 1f, 3, false, -1),              // 9
            new StrategyConst.Item("战败标记", true, 1f, 4, false, 0),             // 10
            new StrategyConst.Item("施展条件", false, 1f, 6, true, -1),             // 11
            new StrategyConst.Item("运功中", false, 1f, 5, true, -1),               // 12
            new StrategyConst.Item("增益效果", true, 1f, 7, false, 0),             // 13
            new StrategyConst.Item("减益效果", true, 1f, 7, false, 0),             // 14
            new StrategyConst.Item("蛊引数", true, 1f, -1, false, 0),              // 15
            new StrategyConst.Item("属性", true, 1f, 8, false, 0)                  // 16
        };

        // Token: 0x04000031 RID: 49
        public static readonly string[] PlayerOptions = new string[]
        {
            "自己",
            "敌人"
        };

        // Token: 0x04000030 RID: 48
        public static readonly string[] JudgementOptions = new string[]
        {
            "等于",
            "大于",
            "小于"
        };
        public static readonly string[] YesOrNo = new string[]
        {
            "是",
            "否"
        };


        // Token: 0x04000033 RID: 51
        public static readonly string[] SkillTypeOptions = new string[]
        {
            "摧破功法",
            "轻灵功法",
            "护体功法",
            "治疗",
            "解毒",
            "逃跑"
        };

        // Token: 0x04000034 RID: 52
        public static readonly string[] WeaponTypeOptions = new string[]
        {
            "针匣",
            "对刺",
            "暗器",
            "萧笛",
            "掌套",
            "短杵",
            "拂尘",
            "长鞭",
            "剑",
            "刀",
            "长兵",
            "瑶琴",
            "机关",
            "令符",
            "毒霜",
            "毒砂"
        };

        // Token: 0x04000035 RID: 53
        public static readonly string[] TrickTypeOptions = new string[]
        {
            "不限类型",
            "掷",
            "弹",
            "御",
            "劈",
            "刺",
            "撩",
            "崩",
            "点",
            "拿",
            "音",
            "缠",
            "咒",
            "机",
            "药",
            "毒",
            "扫",
            "撞",
            "抓",
            "噬",
            "杀",
            "无",
            "神"
        };

        public static readonly string[] DefeatMarkOptions = new string[]
        {
            "不限类型",
            "破绽",
            "点穴",
            "心神",
            "外伤",
            "内伤",
            "中毒",
            "重创"
        };


        /// <summary>
        /// 式
        /// </summary>
        /// <returns></returns>
        public static List<string> GetTrickNameList()
        {
            return Config.TrickType.Instance.ToList().ConvertAll(x => x.Name);
        }

        /// <summary>
        /// 部位
        /// </summary>
        /// <returns></returns>
        public static List<string> GetBodyPartList()
        {
            return Config.BodyPart.Instance.ToList().ConvertAll(x => x.Name);
        }

        /// <summary>
        /// 队友指令
        /// </summary>
        /// <returns></returns>
        public static List<string> GetTeammateCommandList()
        {
            return Config.TeammateCommand.Instance.ToList().ConvertAll(x => x.Name);
        }

        /// <summary>
        /// 增减益效果
        /// </summary>
        /// <returns></returns>
        public static List<string> GetSpecialEffectNameList()
        {
            return SpecialEffectDataField.Instance.ToList().ConvertAll(x => x.Name);
        }

        public static String GetSpecialEffectNameById(int id)
        { 
            return SpecialEffectDataField.Instance[id].Name;
        }

        public static int GetSpecialEffectIdByName(string name)
        {
            return SpecialEffectDataField.Instance.ToList().Find(x => x.Name.Equals(name))?.TemplateId ?? 0;
        }

        /// <summary>
        /// 武器
        /// </summary>
        public static readonly string[] WeaponIndexOptions = new string[] 
        {
            "武器1",
            "武器2",
            "武器3",
            "空手",
            "树枝",
            "石子",
            "喉声"
        };

        public static readonly string[] MoveActionOptions = new string[] 
        {
            "不移动",
            "向前移动",
            "向后移动",
        };


        public static readonly string[] SatisfiedorDissatisfied = new string[]
        {
            "满足",
            "不满足"
        };

        public static readonly string[] CharacterAttribute = new string[]
        {
            "精纯",
            "催迫",
            "轻灵",
            "护体",
            "奇窍",
            "烈毒",
            "郁毒",
            "赤毒",
            "寒毒",
            "腐毒",
            "幻毒",
            "内息"
        };

        public static readonly List<string[]> JudgementList = new List<string[]>
        {
            JudgementOptions,
            YesOrNo
        };

        public static readonly List<string[]> OptionsList = new List<string[]>
        {
            StrategyConst.WeaponTypeOptions, // 0
            StrategyConst.SkillTypeOptions,  // 1
            StrategyConst.TrickTypeOptions,  // 2
            GetTrickNameList().ToArray(),    // 3
            DefeatMarkOptions,               // 4
            YesOrNo,                         // 5
            SatisfiedorDissatisfied,         // 6
            GetSpecialEffectNameList().ToArray(),  // 7
            CharacterAttribute
        };

        // Token: 0x02000014 RID: 20
        public struct Item
        {
            public Item(string name, bool showNumSetter, float multiplyer, int optionIndex, bool showSelectSkillBtn = false, int judgementIndex = -1)
            {
                this.Name = name;
                this.ShowNumSetter = showNumSetter;
                this.Multiplyer = multiplyer;
                this.OptionIndex = optionIndex;
                this.ShowSelectSkillBtn = showSelectSkillBtn;
                this.judgementIndex = judgementIndex;
            }

            public string Name;

            public bool ShowNumSetter;

            public float Multiplyer;

            public int OptionIndex;

            public bool ShowSelectSkillBtn;

            public int judgementIndex;
        }
    }
}
