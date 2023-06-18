
namespace ConvenienceFrontend.CombatStrategy
{
    public class GameDataBridgeConst
    {
        public const int MethodId = 1957;

        public static class Flag
        { 
            /// <summary>
            /// 切换自动移动
            /// </summary>
            public const int Flag_SwitchAutoMove = 0;
            /// <summary>
            /// 切换自动攻击
            /// </summary>
            public const int Flag_SwitchAutoAttack = 1;
            /// <summary>
            /// 更新目标距离
            /// </summary>
            public const int Flag_UpdateTargetDistance = 2;
            /// <summary>
            /// 更新settings json
            /// </summary>
            public const int Flag_UpdateSettingsJson = 3;
            /// <summary>
            /// 切换自动执行策略开关
            /// </summary>
            public const int Flag_SwitchAutoCastSkill = 4;
            /// <summary>
            /// 更新策略json
            /// </summary>
            public const int Flag_UpdateStrategiesJson = 5;

            /// <summary>
            /// 自动生成策略
            /// </summary>
            public const int Flag_AutoGenerateStrategy = 6;
        }
    }
}
