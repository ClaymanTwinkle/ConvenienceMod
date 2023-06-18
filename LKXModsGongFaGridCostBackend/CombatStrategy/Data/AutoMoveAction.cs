using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvenienceBackend.CombatStrategy.Data
{
    public class AutoMoveAction
    {
        /// <summary>
        /// 0: 向前
        /// 1: 向后
        /// 2: 不动
        /// </summary>
        public int type;

        public AutoMoveAction() { }

        public AutoMoveAction(int type)
        {
            this.type = type;
        }
    }
}
