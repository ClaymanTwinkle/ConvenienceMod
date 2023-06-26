using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Config;
using ConvenienceFrontend.Utils;
using FrameWork.ModSystem;
using GameData.Domains.Organization;
using HarmonyLib;

namespace ConvenienceFrontend.ModifyCombatSkill.Data
{
    /// <summary>
    /// 功法拓展
    /// </summary>
    internal class CombatSkillExtraUtils
    {
        /// <summary>
        /// 创建新功法
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static CombatSkillItemWrapper CreateNewCombatSkillItem(CombatSkillItem item)
        {
            var _newCombatSkillItem = new CombatSkillItemWrapper((CombatSkillItem)item.CreateDeepCopy());
            _newCombatSkillItem.SetName("自创武学");
            _newCombatSkillItem.SetDesc("由" + item.Name + "演化而来的武学");
            _newCombatSkillItem.SetBookId(-1);
            _newCombatSkillItem.SetSectId(0);

            return _newCombatSkillItem;
        }

        /// <summary>
        /// 添加拓展功法
        /// </summary>
        public static void AddExtraCombatSkill(CombatSkillItem item)
        {
            short oldTemplateId = item.TemplateId;
            short newTemplateId = 30000;

            new Traverse(item).Field<short>("TemplateId").Value = newTemplateId;
            Config.CombatSkill.Instance.AddExtraItem(item.TemplateId.ToString(), item.Name, item);
            GameDataBridgeUtils.SendData<CombatSkillItem, int>(5, 2000, 0, item);

            var skillBreakGridListItem = (SkillBreakGridListItem)Config.SkillBreakGridList.Instance[oldTemplateId].CreateDeepCopy();
            new Traverse(skillBreakGridListItem).Field<short>("TemplateId").Value = newTemplateId;
            Config.SkillBreakGridList.Instance.AddExtraItem(skillBreakGridListItem.TemplateId.ToString(), skillBreakGridListItem.TemplateId.ToString(), skillBreakGridListItem);
            GameDataBridgeUtils.SendData<SkillBreakGridListItem, int>(5, 2000, 1, skillBreakGridListItem);
        }
    }
}
