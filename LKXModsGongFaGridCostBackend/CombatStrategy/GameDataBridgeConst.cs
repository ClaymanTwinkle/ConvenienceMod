using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvenienceBackend.CombatStrategy
{
    public class GameDataBridgeConst
    {
        public const int MethodId = 1957;

        public static class Flag
        {
            public const int Flag_SwitchAutoMove = 0;
            public const int Flag_SwitchAutoAttack = 1;
            public const int Flag_UpdateTargetDistance = 2;
            public const int Flag_UpdateSettingsJson = 3;
            public const int Flag_SwitchAutoCastSkill = 4;
            public const int Flag_UpdateStrategiesJson = 5;
        }
    }
}
