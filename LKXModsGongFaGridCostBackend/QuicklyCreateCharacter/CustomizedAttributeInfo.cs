using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData.Domains.Character;
using HarmonyLib;

namespace ConvenienceBackend.QuicklyCreateCharacter
{
    internal class CustomizedAttributeInfo
    {
        public CustomizedAttributeInfo(int Dropdown_LifeSkillGrowthType, int Dropdown_CombatSkillGrowthType, int Dropdown_LifeSkillType, int Slider_LifeSkillQualification, int Dropdown_CombatSkillType, int Slider_CombatSkillQualification, int Dropdown_MainAttributeType, int Slider_MainAttribute, int Slider_RollCountLimit)
        {
            this.bool_LifeGrowthType = (Dropdown_LifeSkillGrowthType > 0);
            this.value_LifeGrowthType = (sbyte)(Dropdown_LifeSkillGrowthType - 1);
            this.bool_CombatGrowthType = (Dropdown_CombatSkillGrowthType > 0);
            this.value_CombatGrowthType = (sbyte)(Dropdown_CombatSkillGrowthType - 1);
            this.bool_LifeQulification = (Dropdown_LifeSkillType > 0);
            this.type_LifeQulification = Dropdown_LifeSkillType - 1;
            this.value_LifeQulification = Slider_LifeSkillQualification;
            this.bool_CombatQulification = (Dropdown_CombatSkillType > 0);
            this.type_CombatQulification = Dropdown_CombatSkillType - 1;
            this.value_CombatQulification = Slider_CombatSkillQualification;
            this.bool_MainAttribute = (Dropdown_MainAttributeType > 0);
            this.type_MainAttribute = Dropdown_MainAttributeType - 1;
            this.value_MainAttribute = Slider_MainAttribute;
            this.rollCountLimit = Slider_RollCountLimit;
        }

        // Token: 0x06000015 RID: 21 RVA: 0x00003300 File Offset: 0x00001500
        public CustomizedAttributeInfo(bool bool_LifeGrowthType_Value, sbyte value_LifeGrowthType_Value, bool bool_CombatGrowthType_Value, sbyte value_CombatGrowthType_Value, bool bool_LifeQulification_Value, int type_LifeQulification_Value, int value_LifeQulification_Value, bool bool_CombatQulification_Value, int type_CombatQulification_Value, int value_CombatQulification_Value, bool bool_MainAttribute_Value, int type_MainAttribute_Value, int value_MainAttribute_Value, int Slider_RollCountLimit)
        {
            this.bool_LifeGrowthType = bool_LifeGrowthType_Value;
            this.value_LifeGrowthType = value_LifeGrowthType_Value;
            this.bool_CombatGrowthType = bool_CombatGrowthType_Value;
            this.value_CombatGrowthType = value_CombatGrowthType_Value;
            this.bool_LifeQulification = bool_LifeQulification_Value;
            this.type_LifeQulification = type_LifeQulification_Value;
            this.value_LifeQulification = value_LifeQulification_Value;
            this.bool_CombatQulification = bool_CombatQulification_Value;
            this.type_CombatQulification = type_CombatQulification_Value;
            this.value_CombatQulification = value_CombatQulification_Value;
            this.bool_MainAttribute = bool_MainAttribute_Value;
            this.type_MainAttribute = type_MainAttribute_Value;
            this.value_MainAttribute = value_MainAttribute_Value;
            this.rollCountLimit = Slider_RollCountLimit;
        }

        // Token: 0x06000016 RID: 22 RVA: 0x00003384 File Offset: 0x00001584
        public unsafe bool CheckIsPassed(Character charater)
        {
            Traverse traverse = Traverse.Create(charater);
            sbyte lifeSkillQualificationGrowthType = charater.GetLifeSkillQualificationGrowthType();
            sbyte combatSkillQualificationGrowthType = charater.GetCombatSkillQualificationGrowthType();
            traverse.Field("_lifeSkillQualificationGrowthType").SetValue(0);
            traverse.Field("_combatSkillQualificationGrowthType").SetValue(0);
            bool flag = this.bool_LifeQulification;
            if (flag)
            {
                var lifeSkillShorts = (LifeSkillShorts)traverse.Method("CalcLifeSkillQualifications").GetValue();
                short num = lifeSkillShorts.Items[this.type_LifeQulification];
                bool flag2 = (int)num < this.value_LifeQulification;
                if (flag2)
                {
                    return false;
                }
            }
            bool flag3 = this.bool_CombatQulification;
            if (flag3)
            {
                var combatSkillShorts = (CombatSkillShorts)traverse.Method("CalcCombatSkillQualifications", Array.Empty<object>()).GetValue();
                short num2 = combatSkillShorts.Items[this.type_CombatQulification];
                bool flag4 = (int)num2 < this.value_CombatQulification;
                if (flag4)
                {
                    return false;
                }
            }
            bool flag5 = this.bool_MainAttribute;
            if (flag5)
            {
                var mainAttributes = charater.GetMaxMainAttributes();
                short num3 = mainAttributes.Items[this.type_MainAttribute];
                bool flag6 = (int)num3 < this.value_MainAttribute;
                if (flag6)
                {
                    return false;
                }
            }
            sbyte b = this.bool_LifeGrowthType ? this.value_LifeGrowthType : lifeSkillQualificationGrowthType;
            sbyte b2 = this.bool_CombatGrowthType ? this.value_CombatGrowthType : lifeSkillQualificationGrowthType;
            traverse.Field("_lifeSkillQualificationGrowthType").SetValue(b);
            traverse.Field("_combatSkillQualificationGrowthType").SetValue(b2);
            return true;
        }

        // Token: 0x0400001C RID: 28
        public bool bool_LifeGrowthType;

        // Token: 0x0400001D RID: 29
        public sbyte value_LifeGrowthType;

        // Token: 0x0400001E RID: 30
        public bool bool_CombatGrowthType;

        // Token: 0x0400001F RID: 31
        public sbyte value_CombatGrowthType;

        // Token: 0x04000020 RID: 32
        public bool bool_LifeQulification;

        // Token: 0x04000021 RID: 33
        public int type_LifeQulification;

        // Token: 0x04000022 RID: 34
        public int value_LifeQulification;

        // Token: 0x04000023 RID: 35
        public bool bool_CombatQulification;

        // Token: 0x04000024 RID: 36
        public int type_CombatQulification;

        // Token: 0x04000025 RID: 37
        public int value_CombatQulification;

        // Token: 0x04000026 RID: 38
        public bool bool_MainAttribute;

        // Token: 0x04000027 RID: 39
        public int type_MainAttribute;

        // Token: 0x04000028 RID: 40
        public int value_MainAttribute;

        // Token: 0x04000029 RID: 41
        public int rollCountLimit;
    }
}
