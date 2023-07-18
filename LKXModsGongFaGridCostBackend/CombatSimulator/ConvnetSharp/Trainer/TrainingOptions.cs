using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvnetSharp
{
    [Serializable]
    public class TrainingOptions
    {
        public int temporal_window = int.MinValue;
        /// <summary>
        /// 经验回放空间大小
        /// </summary>
        public int experience_size = int.MinValue;
        /// <summary>
        /// 经过多少次预填充步骤后，才开始进行学习
        /// 越大越好
        /// </summary>
        public int start_learn_threshold = int.MinValue;
        /// <summary>
        /// 多少步后开始学习
        /// </summary>
        public int learning_steps_total = int.MinValue;
        /// <summary>
        /// 一开始执行随机操作的步骤数
        /// </summary>
        public int learning_steps_burnin = int.MinValue;
        public int[] hidden_layer_sizes;

        public double gamma = double.MinValue;
        public double learning_rate = double.MinValue;
        public double epsilon_min = double.MinValue;
        public double epsilon_test_time = double.MinValue;

        public Options options;
        public List<LayerDefinition> layer_defs;
        public List<double> random_action_distribution;
    }
}
