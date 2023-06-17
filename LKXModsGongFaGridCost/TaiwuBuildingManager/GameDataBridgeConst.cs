using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvenienceFrontend.TaiwuBuildingManager
{
    internal class GameDataBridgeConst
    {
        public const int MethodId = 1958;

        public static class Flag
        {   
            /// <summary>
            /// 分配工作
            /// </summary>
            public const int Flag_Assign_Jobs = 0;

            /// <summary>
            /// 升级建筑
            /// </summary>
            public const int Flag_Upgrade_buildings = 1;
        }
    }
}
