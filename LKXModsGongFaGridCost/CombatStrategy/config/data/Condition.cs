using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Config;

namespace ConvenienceFrontend.CombatStrategy
{
    [Serializable]
    public class Condition
    {
        // Token: 0x0600001D RID: 29 RVA: 0x000031AC File Offset: 0x000013AC
        public Condition()
        {
        }

        // Token: 0x0600001E RID: 30 RVA: 0x000031DC File Offset: 0x000013DC
        public Condition(bool isAlly, int item, int judge, int subType, int value)
        {
            this.isAlly = isAlly;
            this.item = (JudgeItem)item;
            this.judge = (Judgement)judge;
            this.subType = subType;
            this.value = value;
        }

        // Token: 0x0600001F RID: 31 RVA: 0x0000323C File Offset: 0x0000143C
        public bool IsComplete()
        {
            return this.item != JudgeItem.None;
        }

        public string GetShowDesc()
        {
            JudgeItem judgeItem = item;

            StringBuilder stringBuilder = new StringBuilder();
            if (judgeItem > JudgeItem.Distance)
            {
                stringBuilder.Append(StrategyConst.PlayerOptions[isAlly ? 0 : 1]).Append(' ');
            }
            if (judgeItem == JudgeItem.HasSkillEffect || judgeItem == JudgeItem.CanUseSkill || judgeItem == JudgeItem.AffectingSkill)
            {
                CombatSkillItem combatSkillItem = CombatSkill.Instance[subType];
                if (combatSkillItem != null)
                {
                    stringBuilder.Append(combatSkillItem.Name).Append(' ');
                }
            }
            else if (judgeItem == JudgeItem.HasTrick && subType >= 0)
            {
                stringBuilder.Append(StrategyConst.TrickTypeOptions[subType + 1]).Append(' ');
            }
            else if (judgeItem == JudgeItem.DefeatMarkCount)
            {
                stringBuilder.Append(StrategyConst.DefeatMarkOptions[subType]).Append(' ');
            }
            else if (judgeItem == JudgeItem.Buff || judgeItem == JudgeItem.Debuff)
            {
                stringBuilder.Append(StrategyConst.GetSpecialEffectNameById(subType)).Append(' ');
            }

            stringBuilder.Append(StrategyConst.ItemOptions[(int)judgeItem].Name).Append(' ');

            if (judgeItem == JudgeItem.WeaponType)
            {
                stringBuilder.Append(StrategyConst.WeaponTypeOptions[value]);
            }
            else if (judgeItem == JudgeItem.PreparingAction)
            {
                stringBuilder.Append(StrategyConst.SkillTypeOptions[value]);
            }
            else if (judgeItem == JudgeItem.CanUseSkill)
            {
                stringBuilder.Append(StrategyConst.SatisfiedorDissatisfied[value]);
            }
            else if (judgeItem == JudgeItem.AffectingSkill)
            {
                stringBuilder.Append(StrategyConst.YesOrNo[value]);
            }
            else
            {
                stringBuilder.Append(StrategyConst.JudgementOptions[(int)judge]).Append(' ');
                if (judgeItem == JudgeItem.CurrentTrick)
                {
                    stringBuilder.Append(valueStr);
                }
                else
                {
                    string format = (judgeItem == JudgeItem.Distance) ? "f1" : "f0";
                    float multiplyer = StrategyConst.ItemOptions[(int)judgeItem].Multiplyer;
                    stringBuilder.Append(((float)value / multiplyer).ToString(format));
                }
            }
            return stringBuilder.ToString();
        }

        // Token: 0x04000043 RID: 67
        public bool isAlly = true;

        // Token: 0x04000044 RID: 68
        public JudgeItem item = JudgeItem.None;

        // Token: 0x04000045 RID: 69
        public Judgement judge = Judgement.Equals;

        // Token: 0x04000046 RID: 70
        public int subType = -1;

        // Token: 0x04000047 RID: 71
        public int value = -1;

        public string valueStr = "";
    }
}
