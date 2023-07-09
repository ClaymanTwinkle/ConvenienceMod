using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tensorflow.Keras.Engine;

namespace ConvenienceBackend.CombatSimulator
{
    internal class PPOAgent
    {
        private PPOModel _model = null;

        public PPOAgent() 
        { 
        
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment">游戏环境</param>
        /// <param name="maxEpisodes">最大迭代次数</param>
        /// <param name="maxTimesteps">单次游戏内最大步数，反正单局游戏执行太久</param>
        public void Learn(GameEnvironment environment, int maxEpisodes, int maxTimesteps)
        {
            _model = new PPOModel();
            var totalReward = 0f;

            for (var i = 0; i < maxEpisodes; i++)
            {
                // 重置游戏状态，重新开始
                var state = environment.Reset();

                for (var j = 0; j < maxTimesteps; j++)
                {
                    // 计算下一个动作
                    var action = _model.GetAction(state);

                    // 执行动作，获取游戏状态、奖励、判断是否游戏结束
                    var (newState, reward, done) = environment.Step(action);
                    state = newState;

                    // TODO 更新模型
                    _model.Update();

                    totalReward += reward;

                    // 判定游戏结束
                    if (done) break;
                }

            }
        }

        /// <summary>
        /// 保存模型
        /// </summary>
        public void SaveModel(string filePath)
        { 
            
        }

        /// <summary>
        /// 加载模型
        /// </summary>
        /// <returns></returns>
        public PPOModel LoadModel(string filePath)
        {
            return new PPOModel(filePath);
        }
    }
}
