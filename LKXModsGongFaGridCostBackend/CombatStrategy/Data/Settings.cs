using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvenienceBackend.CombatStrategy.Data
{
    internal class Settings
    {
        public bool isEnable { get; set; }

        public bool AutoMove { get; set; }

        public int TargetDistance { get; set; }

        public int MobilityAllowForward { get; set; }

        public int MobilityAllowBackward { get; set; }

        public int MobilityRecoverCap { get; set; }

        public int DistanceAllowJumpForward { get; set; }

        public int DistanceAllowJumpBackward { get; set; }

        public bool JumpPassTargetDistance { get; set; }

        public bool JumpOutOfAttackRange { get; set; }

        public int MinJumpPosition { get; set; }

        public int MaxJumpPosition { get; set; }

        public bool AllowOppositeMoveInJumpingSkill { get; set; }

        public bool AutoAttack { get; set; }

        public int AttackBufferMin { get; set; }

        public int AttackBufferMax { get; set; }

        public bool IgnoreRange { get; set; }

        public bool[] RemoveTrick { get; set; }

        public bool AutoCastSkill { get; set; }
    }
}
