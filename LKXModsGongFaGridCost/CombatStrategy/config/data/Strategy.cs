using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvenienceFrontend.CombatStrategy
{
    public class Strategy
    {
        /// <summary>
        /// 是否是完整可用的策略
        /// </summary>
        /// <returns></returns>
        public bool IsComplete()
        {
            bool result;
            if (this.skillId < 0 && this.changeTrickAction == null && this.switchWeaponAction == null && this.teammateCommandAction == null)
            {
                result = false;
            }
            else
            {
                for (int i = 0; i < this.conditions.Count; i++)
                {
                    if (!this.conditions[i].IsComplete())
                    {
                        return false;
                    }
                }
                result = true;
            }
            return result;
        }

        public void setAction(short skillId)
        { 
            this.skillId = skillId;
            changeTrickAction = null;
            switchWeaponAction = null;
            teammateCommandAction = null;
        }

        public void setAction(ChangeTrickAction changeTrickAction)
        {
            this.changeTrickAction = changeTrickAction;
            this.skillId = -1;
            switchWeaponAction = null;
            teammateCommandAction = null;
        }

        public void setAction(SwitchWeaponAction switchWeaponAction)
        {
            this.switchWeaponAction = switchWeaponAction;
            this.skillId = -1;
            changeTrickAction = null;
            teammateCommandAction = null;
        }

        public void setAction(TeammateCommandAction teammateCommandAction)
        { 
            this.teammateCommandAction = teammateCommandAction;
            this.skillId = -1;
            changeTrickAction = null;
            switchWeaponAction = null;
        }

        // 执行条件
        public List<Condition> conditions = new List<Condition>();

        // 技能templateId
        public short skillId = -1;

        /// <summary>
        /// 变招
        /// </summary>
        public ChangeTrickAction changeTrickAction = null;

        /// <summary>
        /// 切换武器
        /// </summary>
        public SwitchWeaponAction switchWeaponAction = null;

        /// <summary>
        /// 队友指令
        /// </summary>
        public TeammateCommandAction teammateCommandAction = null;

        // 类型
        // 0: 释放技能
        // 1: 变招
        // 2: 切换武器
        public short type = 0;

        // 释放启用
        public bool enabled = false;
    }
}
