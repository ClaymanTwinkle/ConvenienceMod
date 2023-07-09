using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvenienceBackend.CombatSimulator
{
    internal interface GameEnvironment
    {
        /// <summary>
        /// 初始化游戏，重新玩，返回初始状态
        /// </summary>
        public int Reset();

        /// <summary>
        /// 执行动作后获得游戏新的状态、奖励、是否执行完游戏
        /// </summary>
        /// <param name="action"></param>
        public (int, float, bool) Step(int action);
    }
}
