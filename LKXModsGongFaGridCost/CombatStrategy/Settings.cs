using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ConvenienceFrontend.CombatStrategy
{
    public class Settings : BackendSettings
    {
        // Token: 0x06000012 RID: 18 RVA: 0x00002694 File Offset: 0x00000894
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

        // Token: 0x06000014 RID: 20 RVA: 0x000027A8 File Offset: 0x000009A8
        public bool GetBool(string name)
        {
            bool result = false;
            switch (name) {
                case "AutoMove":
                    result = this.AutoMove;
                    break;
                case "IgnoreRange":
                    result = this.IgnoreRange;
                    break;
                case "ShowAutoAttackTips":
                    result = this.ShowAutoAttackTips;
                    break;
                case "AllowOppositeMoveInJumpingSkill":
                    result = this.AllowOppositeMoveInJumpingSkill;
                    break;
                case "AutoCastSkill":
                    result = this.AutoCastSkill;
                    break;
                case "AutoAttack":
                    result = this.AutoAttack;
                    break;
                case "JumpPassTargetDistance":
                    result = this.JumpPassTargetDistance;
                    break;
                case "JumpOutOfAttackRange":
                    result = this.JumpOutOfAttackRange;
                    break;
            }
            return result;
        }

        // Token: 0x06000015 RID: 21 RVA: 0x0000291C File Offset: 0x00000B1C
        public void SetValue(string name, bool value)
        {
            switch (name) {
                case "AutoMove":
                    this.AutoMove = value;
                    break;
                case "IgnoreRange":
                    this.IgnoreRange = value;
                    break;
                case "ShowAutoAttackTips":
                    this.ShowAutoAttackTips = value;
                    break;
                case "AllowOppositeMoveInJumpingSkill":
                    this.AllowOppositeMoveInJumpingSkill = value;
                    break;
                case "AutoCastSkill":
                    this.AutoCastSkill = value;
                    break;
                case "AutoAttack":
                    this.AutoAttack = value;
                    break;
                case "JumpPassTargetDistance":
                    this.JumpPassTargetDistance = value;
                    break;
                case "JumpOutOfAttackRange":
                    this.JumpOutOfAttackRange = value;
                    break;
            }
        }

        // Token: 0x06000016 RID: 22 RVA: 0x00002A84 File Offset: 0x00000C84
        public int GetInt(string name)
        {
            int result = 0;
            switch (name) {
                case "AttackBufferMin":
                    result = this.AttackBufferMin;
                    break;
                case "MobilityAllowBackward":
                    result = this.MobilityAllowBackward;
                    break;
                case "TargetDistance2":
                    result = this.TargetDistance2;
                    break;
                case "TargetDistance":
                    result = this.TargetDistance;
                    break;
                case "AttackBufferMax":
                    result = this.AttackBufferMax;
                    break;
                case "MinJumpPosition":
                    result = this.MinJumpPosition;
                    break;
                case "MobilityAllowForward":
                    result = this.MobilityAllowForward;
                    break;
                case "MobilityRecoverCap":
                    result = this.MobilityRecoverCap;
                    break;
                case "DistanceAllowJumpForward":
                    result = this.DistanceAllowJumpForward;
                    break;
                case "DistanceAllowJumpBackward":
                    result = this.DistanceAllowJumpBackward;
                    break;
                case "MaxJumpPosition":
                    result = this.MaxJumpPosition;
                    break;
            }
            return result;
        }

        // Token: 0x06000017 RID: 23 RVA: 0x00002CB4 File Offset: 0x00000EB4
        public void SetValue(string name, int value)
        {
            switch (name)
            {
                case "AttackBufferMin":
                    this.AttackBufferMin = value;
                    break;
                case "MobilityAllowBackward":
                    this.MobilityAllowBackward = value;
                    break;
                case "TargetDistance2":
                    this.TargetDistance2 = value;
                    break;
                case "TargetDistance":
                    this.TargetDistance = value;
                    break;
                case "AttackBufferMax":
                    this.AttackBufferMax = value;
                    break;
                case "MinJumpPosition":
                    this.MinJumpPosition = value;
                    break;
                case "MobilityAllowForward":
                    this.MobilityAllowForward = value;
                    break;
                case "MobilityRecoverCap":
                    this.MobilityRecoverCap = value;
                    break;
                case "DistanceAllowJumpForward":
                    this.DistanceAllowJumpForward = value;
                    break;
                case "DistanceAllowJumpBackward":
                    this.DistanceAllowJumpBackward = value;
                    break;
                case "MaxJumpPosition":
                    this.MaxJumpPosition = value;
                    break;

            }
        }

        // Token: 0x04000026 RID: 38
        public int TargetDistance2 = 80;

        // Token: 0x04000027 RID: 39
        public int DistanceChangeSpeed = 0;

        // Token: 0x04000028 RID: 40
        public KeyCode SwitchAutoMoveKey = KeyCode.Tab; // 9;

        // Token: 0x04000029 RID: 41
        public KeyCode SwitchTargetDistanceKey = KeyCode.R;//114;

        // Token: 0x0400002A RID: 42
        public KeyCode IncreaseDistanceKey = KeyCode.Q; //113;

        // Token: 0x0400002B RID: 43
        public KeyCode DecreaseDistanceKey = KeyCode.E; // 101;

        // Token: 0x0400002C RID: 44
        public bool ShowAutoAttackTips = true;

        // Token: 0x0400002D RID: 45
        public KeyCode SwitchAutoAttackKey = KeyCode.LeftControl; // 306;

        public KeyCode SwitchAutoCastSkillKey = KeyCode.RightControl;

        // Token: 0x0400002E RID: 46
        public int CounterAccordance = 0;
    }
}
