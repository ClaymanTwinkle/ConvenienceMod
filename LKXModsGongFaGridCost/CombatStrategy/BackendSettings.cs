using System;

namespace ConvenienceFrontend.CombatStrategy
{
    [Serializable]
    public class BackendSettings
    {
        public bool isEnable = true;

        // 使用AI战斗
        public bool UseAI = false;

        // 自动移动
        public bool AutoMove = true;

        // 主要距离
        public int TargetDistance = 60;

        // 备用距离
        public int TargetDistance2 = 80;

        public int DistanceChangeSpeed = 0;

        public int MobilityAllowForward = 500;

        // Token: 0x04000016 RID: 22
        public int MobilityAllowBackward = 500;

        // Token: 0x04000017 RID: 23
        public int MobilityRecoverCap = 1000;

        // Token: 0x04000018 RID: 24
        public int DistanceAllowJumpForward = 5;

        // Token: 0x04000019 RID: 25
        public int DistanceAllowJumpBackward = 5;

        // Token: 0x0400001A RID: 26
        public bool JumpPassTargetDistance = true;

        // Token: 0x0400001B RID: 27
        public bool JumpOutOfAttackRange = true;

        // Token: 0x0400001C RID: 28
        public int MinJumpPosition = 20;

        // Token: 0x0400001D RID: 29
        public int MaxJumpPosition = 120;

        // Token: 0x0400001E RID: 30
        public bool AllowOppositeMoveInJumpingSkill = true;

        // Token: 0x0400001F RID: 31
        public bool AutoAttack = true;

        // Token: 0x04000020 RID: 32
        public int AttackBufferMin = 0;

        // Token: 0x04000021 RID: 33
        public int AttackBufferMax = 0;

        // Token: 0x0400002C RID: 44
        public bool ShowAutoAttackTips = true;

        // Token: 0x04000022 RID: 34
        public bool IgnoreRange = false;

        // Token: 0x04000024 RID: 36
        public bool[] RemoveTeammateCommand = new bool[14];

        // 空A的式
        public bool[] RemoveTrick = new bool[22];

        // 执行策略开关
        public bool AutoCastSkill = true;

        public bool GetBool(string name)
        {
            bool result = false;
            switch (name)
            {
                case "UseAI":
                    result = this.UseAI;
                    break;

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
            switch (name)
            {
                case "UseAI":
                    this.UseAI = value;
                    break;
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
            switch (name)
            {
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
    }
}
