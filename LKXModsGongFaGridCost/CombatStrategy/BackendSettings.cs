using System;

namespace ConvenienceFrontend.CombatStrategy
{
    public class BackendSettings
    {
        public bool isEnable = true;

        public bool AutoMove = true;

        // Token: 0x04000014 RID: 20
        public int TargetDistance = 60;

        // Token: 0x04000015 RID: 21
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

        // Token: 0x04000022 RID: 34
        public bool IgnoreRange = false;

        // Token: 0x04000023 RID: 35
        public int AcceleratePercent = 0;

        // Token: 0x04000024 RID: 36
        public bool[] RemoveTeammateCommand = new bool[14];

        // 空A的式
        public bool[] RemoveTrick = new bool[22];

        // Token: 0x04000025 RID: 37
        public bool AutoCastSkill = true;

        /// <summary>
        /// 当前选中的策略序号
        /// </summary>
        public int SelectStrategyIndex = 0;
    }
}
