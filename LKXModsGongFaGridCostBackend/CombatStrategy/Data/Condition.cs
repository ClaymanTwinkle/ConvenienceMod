using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvenienceBackend.CombatStrategy.Data
{
    public class Condition
    {
        public bool isAlly { get; set; }

        public JudgeItem item { get; set; }

        public Judgement judge { get; set; }

        public int subType { get; set; }

        public int value { get; set; }

        public string valueStr { get; set; }
    }
}
