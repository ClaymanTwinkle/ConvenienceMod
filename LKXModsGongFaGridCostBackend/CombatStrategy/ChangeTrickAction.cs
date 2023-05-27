using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvenienceBackend.CombatStrategy
{
    public class ChangeTrickAction
    {
        /// <summary>
        /// 招式
        /// </summary>
        public string trick;

        /// <summary>
        /// 打击部位
        /// </summary>
        public string body;

        public ChangeTrickAction()
        {
        }

        public ChangeTrickAction(string trick, string body)
        {
            this.trick = trick;
            this.body = body;
        }
    }
}
