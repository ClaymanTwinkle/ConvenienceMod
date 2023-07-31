using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using ConvnetSharp;
using DeepQLearning.DRLAgent;
using GameData.Domains;

namespace ConvenienceBackend.CombatSimulator
{
    public class DeepQLearnManager
    {
        public static void SaveLearning(DeepQLearn Brain)
        {
            if (Brain == null) return;

            var netFile = GetNetFilePath();

            using (FileStream fstream = new(netFile, FileMode.Create))
            {
                new BinaryFormatter().Serialize(fstream, Brain);
            }
        }

        public static DeepQLearn LoadOrCreateDeepQLearn()
        {
            var netFile = GetNetFilePath();

            if (File.Exists(netFile))
            {
                using (FileStream fstream = new FileStream(netFile, FileMode.Open))
                {
                    return new BinaryFormatter().Deserialize(fstream) as DeepQLearn;
                }
            }

            return BuildDeepQLearn();
        }

        private static DeepQLearn BuildDeepQLearn()
        {
            var num_inputs = GameState.MAX_STATE_COUNT; // 9 eyes, each sees 3 numbers (wall, green, red thing proximity)
            var num_actions = GameEnvironment.MAX_ACTION_COUNT; // 5 possible angles agent can turn
            var temporal_window = 4; // amount of temporal memory. 0 = agent lives in-the-moment :)
            var network_size = num_inputs * temporal_window + num_actions * temporal_window + num_inputs;

            var layer_defs = new List<LayerDefinition>
            {
                // the value function network computes a value of taking any of the possible actions
                // given an input state. Here we specify one explicitly the hard way
                // but user could also equivalently instead use opt.hidden_layer_sizes = [20,20]
                // to just insert simple relu hidden layers.
                new LayerDefinition { type = "input", out_sx = 1, out_sy = 1, out_depth = network_size },
                new LayerDefinition { type = "fc", num_neurons = 96, activation = "relu" },
                new LayerDefinition { type = "fc", num_neurons = 96, activation = "relu" },
                new LayerDefinition { type = "fc", num_neurons = 96, activation = "relu" },
                new LayerDefinition { type = "regression", num_neurons = num_actions }
            };

            // options for the Temporal Difference learner that trains the above net
            // by backpropping the temporal difference learning rule.
            //var opt = new Options { method="sgd", learning_rate=0.01, l2_decay=0.001, momentum=0.9, batch_size=10, l1_decay=0.001 };
            var opt = new Options { method = "adadelta", l2_decay = 0.001, batch_size = 10 };

            var tdtrainer_options = new TrainingOptions
            {
                temporal_window = temporal_window,
                experience_size = 300000,
                start_learn_threshold = 10000,
                gamma = 0.7,
                learning_steps_total = 10000000,
                learning_steps_burnin = 300000,
                epsilon_min = 0.05,
                epsilon_test_time = 0.00,
                layer_defs = layer_defs,
                options = opt
            };

            return new DeepQLearn(GameState.MAX_STATE_COUNT, GameEnvironment.MAX_ACTION_COUNT, tdtrainer_options);
        }

        private static String GetNetFilePath()
        {
            return Path.Combine(ConvenienceBackend.GetModDirectory(), "learn_data");
        }

        private static String GetAnalysisFilePath()
        {
            return Path.Combine(ConvenienceBackend.GetModDirectory(), "analysis_data.txt");
        }

        public static void SaveAnalysisData(DeepQLearn Brain)
        {
            if (Brain == null) return;

            var analysisFile = GetAnalysisFilePath();
            var currentAnalysisInfo = Brain.GetLineData();

            if (!System.IO.File.Exists(analysisFile))
            {
                //没有则创建这个文件
                FileStream fs1 = new(analysisFile, FileMode.Create, FileAccess.Write);//创建写入文件               
                StreamWriter sw = new(fs1);

                sw.WriteLine(currentAnalysisInfo);//开始写入值
                sw.Close();
                fs1.Close();
            }
            else
            {
                using (FileStream fileStream = new FileStream(analysisFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (StreamWriter writer = new StreamWriter(fileStream))
                    {
                        writer.WriteLine(currentAnalysisInfo);
                    }
                }
            }
        }
    }
}
