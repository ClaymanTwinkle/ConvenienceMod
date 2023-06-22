using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ConvenienceFrontend.CombatStrategy.config.data
{
    [Serializable]
    public class GlobalSettings
    {
        public KeyCode GetKey(string key)
        {
            KeyCode result;
            switch (key)
            {
                case "SwitchAutoMoveKey":
                    result = this.SwitchAutoMoveKey;
                    break;
                case "SwitchAutoAttackKey":
                    result = this.SwitchAutoAttackKey;
                    break;
                case "SwitchTargetDistanceKey":
                    result = this.SwitchTargetDistanceKey;
                    break;
                case "IncreaseDistanceKey":
                    result = this.IncreaseDistanceKey;
                    break;
                case "DecreaseDistanceKey":
                    result = this.DecreaseDistanceKey;
                    break;
                case "SwitchAutoCastSkillKey":
                    result = this.SwitchAutoCastSkillKey;
                    break;
                default:
                    result = 0;
                    break;
            }
            return result;
        }

        // Token: 0x06000013 RID: 19 RVA: 0x00002724 File Offset: 0x00000924
        public void SetKey(string key, KeyCode keyCode)
        {
            switch (key)
            {
                case "SwitchAutoMoveKey":
                    this.SwitchAutoMoveKey = keyCode;
                    break;
                case "SwitchAutoAttackKey":
                    this.SwitchAutoAttackKey = keyCode;
                    break;
                case "SwitchTargetDistanceKey":
                    this.SwitchTargetDistanceKey = keyCode;
                    break;
                case "IncreaseDistanceKey":
                    this.IncreaseDistanceKey = keyCode;
                    break;
                case "DecreaseDistanceKey":
                    this.DecreaseDistanceKey = keyCode;
                    break;
                case "SwitchAutoCastSkillKey":
                    this.SwitchAutoCastSkillKey = keyCode;
                    break;
                default:
                    break;
            }
        }

        // Token: 0x04000026 RID: 38
        // Token: 0x04000028 RID: 40
        public KeyCode SwitchAutoMoveKey = KeyCode.Tab; // 9;

        // Token: 0x04000029 RID: 41
        public KeyCode SwitchTargetDistanceKey = KeyCode.R;//114;

        // Token: 0x0400002A RID: 42
        public KeyCode IncreaseDistanceKey = KeyCode.Q; //113;

        // Token: 0x0400002B RID: 43
        public KeyCode DecreaseDistanceKey = KeyCode.E; // 101;

        // Token: 0x0400002D RID: 45
        public KeyCode SwitchAutoAttackKey = KeyCode.LeftControl; // 306;

        public KeyCode SwitchAutoCastSkillKey = KeyCode.RightControl;

        /// <summary>
        /// 当前选中的策略序号
        /// </summary>
        public int SelectStrategyIndex = 0;

        public bool isEnable = true;
    }
}
