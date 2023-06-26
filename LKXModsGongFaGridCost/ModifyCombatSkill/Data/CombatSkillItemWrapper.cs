using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using Config;
using HarmonyLib;
using TaiwuModdingLib.Core.Utils;

namespace ConvenienceFrontend.ModifyCombatSkill.Data
{
    internal class CombatSkillItemWrapper
    {
        public CombatSkillItem Item;
        private static Traverse _traverse;


        public CombatSkillItemWrapper(CombatSkillItem item)
        { 
            this.Item = item;
            _traverse = new Traverse(item);
        }

        /// <summary>
        /// 设置功法ID
        /// </summary>
        /// <param name="templateId"></param>
        public void SetTemplateId(short templateId) 
        {
            SetValue("TemplateId", templateId);
        }

        /// <summary>
        /// 设置功法名称
        /// </summary>
        /// <param name="name"></param>
        public void SetName(string name)
        {
            SetValue("Name", name);
        }


        /// <summary>
        /// 设置功法描述
        /// </summary>
        /// <param name="desc"></param>
        public void SetDesc(string desc)
        {
            SetValue("Desc", desc);
        }

        /// <summary>
        /// 设置门派id
        /// 0: 无门无派
        /// </summary>
        /// <param name="sectId"></param>
        public void SetSectId(sbyte sectId)
        {
            SetValue("SectId", sectId);
        }

        /// <summary>
        /// 设置功法书秘籍id
        /// </summary>
        /// <param name="bookId"></param>
        public void SetBookId(short bookId)
        {
            SetValue("BookId", bookId);
        }

        /// <summary>
        /// 正练特效
        /// </summary>
        /// <param name="directEffectID"></param>
        public void SetDirectEffectID(int directEffectID)
        {
            SetValue("DirectEffectID", directEffectID);
        }

        /// <summary>
        /// 逆练特效
        /// </summary>
        /// <param name="reverseEffectID"></param>
        public void SetReverseEffectID(int reverseEffectID)
        {
            SetValue("ReverseEffectID", reverseEffectID);
        }

        /// <summary>
        /// 施展动画
        /// </summary>
        public void SetCastAnimation(string CastAnimation)
        {
            SetValue("CastAnimation", CastAnimation);
        }

        /// <summary>
        /// 施展特效
        /// </summary>
        public void SetCastParticle(string CastParticle)
        {
            SetValue("CastParticle", CastParticle);
        }
        /// <summary>
        /// 施展音效
        /// </summary>
        public void SetCastSoundEffect(string CastSoundEffect)
        {
            SetValue("CastSoundEffect", CastSoundEffect);
        }

        public void SetPrepareAnimation(string PrepareAnimation)
        {
            SetValue("PrepareAnimation", PrepareAnimation);
        }

        private void SetValue<T>(string fieldName, T value)
        {
            _traverse.Field<T>(fieldName).Value = value;
        }
    }
}
