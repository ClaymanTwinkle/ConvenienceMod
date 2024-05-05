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

        public bool UseAICombat { get; set; }

        public bool UseAIPractice { get; set; }

        public bool AutoMove { get; set; }

        public int TargetDistance { get; set; }

        public int MobilityAllowForward { get; set; }

        public int MobilityAllowBackward { get; set; }

        public int MobilityRecoverCap { get; set; }

        public int DistanceAllowJumpForward { get; set; }

        public int DistanceAllowJumpBackward { get; set; }

        public bool JumpPassTargetDistance { get; set; }

        public bool JumpOutOfAttackRange { get; set; }

        /// <summary>
        /// 如果向前跳跃落点会越过MinJumpPosition距离，则停止跳跃蓄力
        /// </summary>
        public int MinJumpPosition { get; set; }

        /// <summary>
        /// 如果向后跳跃落点会越过MaxJumpPosition距离，则停止跳跃蓄力
        /// </summary>
        public int MaxJumpPosition { get; set; }

        /// <summary>
        /// 向跳跃方向的相反方向移动
        /// </summary>
        public bool AllowOppositeMoveInJumpingSkill { get; set; }

        public bool AutoAttack { get; set; }

        public int AttackBufferMin { get; set; }

        public int AttackBufferMax { get; set; }

        public bool IgnoreRange { get; set; }

        public bool[] RemoveTrick { get; set; }

        public bool AutoCastSkill { get; set; }
    }
}
