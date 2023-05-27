using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvenienceFrontend.CombatStrategy
{
    public class SettingsConst
    {
        // Token: 0x04000010 RID: 16
        public static readonly string[] ToggleParams = new string[]
        {
            "AutoMove",
            "JumpPassTargetDistance",
            "JumpOutOfAttackRange",
            "AllowOppositeMoveInJumpingSkill",
            "AutoAttack",
            "IgnoreRange",
            "AutoCastSkill",
            "ShowAutoAttackTips"
        };

        // Token: 0x04000011 RID: 17
        public static readonly string[] DistanceParams = new string[]
        {
            "TargetDistance",
            "TargetDistance2",
            "DistanceAllowJumpForward",
            "DistanceAllowJumpBackward",
            "MinJumpPosition",
            "MaxJumpPosition",
            "AttackBufferMin",
            "AttackBufferMax"
        };

        // Token: 0x04000012 RID: 18
        public static readonly string[] MobilityParams = new string[]
        {
            "MobilityAllowForward",
            "MobilityAllowBackward",
            "MobilityRecoverCap"
        };
    }
}
