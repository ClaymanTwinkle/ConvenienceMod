using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvenienceFrontend.CombatStrategy
{
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

        public string getShowDesc()
        {
            return "";
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
