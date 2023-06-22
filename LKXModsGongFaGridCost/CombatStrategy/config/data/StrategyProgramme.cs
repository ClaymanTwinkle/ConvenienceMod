using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvenienceFrontend.CombatStrategy.config.data
{
    [Serializable]
    public class StrategyProgramme
    {
        public string name = "";
        public BackendSettings settings = null;
        public List<Strategy> strategies = new List<Strategy>();
    }
}
