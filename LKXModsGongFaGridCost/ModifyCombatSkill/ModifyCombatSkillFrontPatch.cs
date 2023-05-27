using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Config;
using GameData.Utilities;
using TaiwuModdingLib.Core.Utils;

namespace ConvenienceFrontend.ModifyCombatSkill
{
    internal class ModifyCombatSkillFrontPatch : BaseFrontPatch
    {
        private static bool enableBladeAndSwordDoubleJue = false;

        public override void OnModSettingUpdate(string modIdStr)
        {
            ModManager.GetSetting(modIdStr, "enable_BladeAndSwordDoubleJue", ref enableBladeAndSwordDoubleJue);
            UpdateSkillEffectDesc();
        }


        private static void UpdateSkillEffectDesc()
        {
            var flag = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.SetField | BindingFlags.SetProperty | BindingFlags.DeclaredOnly;
            if (enableBladeAndSwordDoubleJue)
            {
                AdaptableLog.Info("真刀剑双绝");

                ReflectionExtensions.ModifyField(SpecialEffect.Instance[723], "Desc", new string[] { "此功法发挥十成威力时，运用者后退2距离，如果敌人仍在运用者的攻击范围内，运用者从施展进度50%开始立即无消耗施展另一个品阶不超过此功法的「剑法」攻击敌人" }, flag);
                ReflectionExtensions.ModifyField(SpecialEffect.Instance[1449], "Desc", new string[] { "此功法发挥十成威力时，运用者前进2距离，如果敌人仍在运用者的攻击范围内，运用者从施展进度50%开始立即无消耗施展另一个品阶不超过此功法的「剑法」攻击敌人" }, flag);

                ReflectionExtensions.ModifyField(SpecialEffect.Instance[715], "Desc", new string[] { "此功法发挥十成威力时，运用者前进2距离，如果敌人仍在运用者的攻击范围内，运用者从施展进度50%开始立即无消耗施展另一个品阶不超过此功法的「刀法」攻击敌人" }, flag);
                ReflectionExtensions.ModifyField(SpecialEffect.Instance[1441], "Desc", new string[] { "此功法发挥十成威力时，运用者后退2距离，如果敌人仍在运用者的攻击范围内，运用者从施展进度50%开始立即无消耗施展另一个品阶不超过此功法的「刀法」攻击敌人" }, flag);
            }
            else
            {
                ReflectionExtensions.ModifyField(SpecialEffect.Instance[723], "Desc", new string[] { "此功法未发挥十成威力时，运用者后退2距离，如果敌人仍在运用者的攻击范围内，运用者从施展进度50%开始立即无消耗施展另一个品阶不超过此功法的「剑法」攻击敌人" }, flag);
                ReflectionExtensions.ModifyField(SpecialEffect.Instance[1449], "Desc", new string[] { "此功法未发挥十成威力时，运用者前进2距离，如果敌人仍在运用者的攻击范围内，运用者从施展进度50%开始立即无消耗施展另一个品阶不超过此功法的「剑法」攻击敌人" }, flag);

                ReflectionExtensions.ModifyField(SpecialEffect.Instance[715], "Desc", new string[] { "此功法未发挥十成威力时，运用者前进2距离，如果敌人仍在运用者的攻击范围内，运用者从施展进度50%开始立即无消耗施展另一个品阶不超过此功法的「刀法」攻击敌人" }, flag);
                ReflectionExtensions.ModifyField(SpecialEffect.Instance[1441], "Desc", new string[] { "此功法未发挥十成威力时，运用者后退2距离，如果敌人仍在运用者的攻击范围内，运用者从施展进度50%开始立即无消耗施展另一个品阶不超过此功法的「刀法」攻击敌人" }, flag);
            }
        }

    }
}
