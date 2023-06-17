using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData.Domains.Character;
using GameData.Utilities;

namespace ConvenienceBackend.QuicklyCreateCharacter
{
    internal class TempCharacterData
    {
        public TempCharacterData(Character characterValue, Character.ProtagonistFeatureRelatedStatus statusValue, sbyte lifeSkillQualificationGrowthTypeValue, sbyte combatSkillQualificationGrowthTypeValue, List<short> featureIdsValue, LifeSkillShorts lifeSkill__ForOverwrite, CombatSkillShorts combatSkill_ForOverwrite, MainAttributes attributes_ForOverwrite, LifeSkillShorts lifeSkill__ForDisplay, CombatSkillShorts combatSkill_ForDisplay, MainAttributes attributes_ForDisplay, TempIteamData itemDataValue)
        {
            this.character = characterValue;
            this.status = statusValue;
            this.lifeSkillQualificationGrowthType = lifeSkillQualificationGrowthTypeValue;
            this.combatSkillQualificationGrowthType = combatSkillQualificationGrowthTypeValue;
            this.featureIds = featureIdsValue;
            this.lifeSkillQualifications_ForOverwrite = lifeSkill__ForOverwrite;
            this.combatSkillQualifications_ForOverwrite = combatSkill_ForOverwrite;
            this.mainAttributes_ForOverwrite = attributes_ForOverwrite;
            this.lifeSkillQualifications_ForDisplay = lifeSkill__ForDisplay;
            this.combatSkillQualifications_ForDisplay = combatSkill_ForDisplay;
            this.mainAttributes__ForDisplay = attributes_ForDisplay;
            this.itemData = itemDataValue;
            this.UpdateTransferDisplayList();
        }

        // Token: 0x06000010 RID: 16 RVA: 0x00002900 File Offset: 0x00000B00
        private unsafe void UpdateTransferDisplayList()
        {
            this.displayList = new List<string>();
            this.displayList.Add("lifeSkillQualificationGrowthType");
            this.displayList.Add(this.ListToString<sbyte>(new List<sbyte>
            {
                this.lifeSkillQualificationGrowthType
            }) ?? "");
            this.displayList.Add("baseLifeSkillQualifications");
            fixed (short* ptr = this.lifeSkillQualifications_ForDisplay.Items)
            {
                short* ptr2 = ptr;
                List<short> list = new List<short>();
                for (sbyte b = 0; b < 16; b += 1)
                {
                    short item = ptr2[b];
                    list.Add(item);
                }
                this.displayList.Add(this.ListToString<short>(list, ',') ?? "");
            }
            this.displayList.Add("combatSkillQualificationGrowthType");
            this.displayList.Add(this.ListToString<sbyte>(new List<sbyte>
            {
                this.combatSkillQualificationGrowthType
            }) ?? "");
            this.displayList.Add("baseCombatSkillQualifications");
            fixed (short* ptr3 = this.combatSkillQualifications_ForDisplay.Items)
            {
                short* ptr4 = ptr3;
                List<short> list2 = new List<short>();
                for (sbyte b2 = 0; b2 < 14; b2 += 1)
                {
                    short item2 = ptr4[b2];
                    list2.Add(item2);
                }
                this.displayList.Add(this.ListToString<short>(list2, ',') ?? "");
            }
            this.displayList.Add("featureIds");
            this.displayList.Add(this.ListToString<short>(this.featureIds, ',') ?? "");
            this.displayList.Add("mainAttributes");
            fixed (short* ptr5 = this.mainAttributes__ForDisplay.Items)
            {
                short* ptr6 = ptr5;
                List<short> list3 = new List<short>();
                for (sbyte b3 = 0; b3 < 6; b3 += 1)
                {
                    short item3 = ptr6[b3];
                    list3.Add(item3);
                }
                this.displayList.Add(this.ListToString<short>(list3, ',') ?? "");
            }
            bool flag2 = this.itemData != null && this.itemData.lifeSkillBook != null;
            if (flag2)
            {
                this.displayList.Add("lifeSkillBookName");
                this.displayList.Add(this.itemData.lifeSkillBook.GetName() ?? "");
                this.displayList.Add("lifeSkillBookType");
                this.displayList.Add(this.itemData.lifeSkillBook.GetLifeSkillType().ToString() ?? "");
            }
            bool flag3 = this.itemData != null && this.itemData.combatSkillBook != null;
            if (flag3)
            {
                this.displayList.Add("combatSkillBookName");
                this.displayList.Add(this.itemData.combatSkillBook.GetName() ?? "");
                this.displayList.Add("combatSkillBookPageTypes");
                this.displayList.Add(this.ListToString<int>(this.itemData.combatSkillBookPageTypes, ',') ?? "");
            }
            HitOrAvoidInts hitValues = this.character.GetHitValues();
            OuterAndInnerInts penetrations = this.character.GetPenetrations();
            HitOrAvoidInts avoidValues = this.character.GetAvoidValues();
            OuterAndInnerInts penetrationResists = this.character.GetPenetrationResists();
            OuterAndInnerShorts recoveryOfStanceAndBreath = this.character.GetRecoveryOfStanceAndBreath();
            short moveSpeed = this.character.GetMoveSpeed();
            short recoveryOfFlaw = this.character.GetRecoveryOfFlaw();
            short castSpeed = this.character.GetCastSpeed();
            short recoveryOfBlockedAcupoint = this.character.GetRecoveryOfBlockedAcupoint();
            short weaponSwitchSpeed = this.character.GetWeaponSwitchSpeed();
            short attackSpeed = this.character.GetAttackSpeed();
            short innerRatio = this.character.GetInnerRatio();
            short recoveryOfQiDisorder = this.character.GetRecoveryOfQiDisorder();
            this.displayList.Add("atkHitAttribute");
            List<int> list4 = new List<int>();
            for (sbyte b4 = 0; b4 < 4; b4 += 1)
            {
                int item4 = hitValues.Items[b4];
                list4.Add(item4);
            }
            this.displayList.Add(this.ListToString<int>(list4, ',') ?? "");
            this.displayList.Add("defHitAttribute");
            List<int> list5 = new List<int>();
            for (sbyte b5 = 0; b5 < 4; b5 += 1)
            {
                int item5 = avoidValues.Items[b5];
                list5.Add(item5);
            }
            this.displayList.Add(this.ListToString<int>(list5, ',') ?? "");
            this.displayList.Add("atkPenetrability");
            this.displayList.Add(this.ListToString<int>(new List<int>
            {
                penetrations.Outer,
                penetrations.Inner
            }, ',') ?? "");
            this.displayList.Add("defPenetrability");
            this.displayList.Add(this.ListToString<int>(new List<int>
            {
                penetrationResists.Outer,
                penetrationResists.Inner
            }, ',') ?? "");
            List<int> list6 = new List<int>();
            list6.Add((int)recoveryOfStanceAndBreath.Outer);
            list6.Add((int)recoveryOfStanceAndBreath.Inner);
            list6.Add((int)moveSpeed);
            list6.Add((int)recoveryOfFlaw);
            list6.Add((int)castSpeed);
            list6.Add((int)recoveryOfBlockedAcupoint);
            list6.Add((int)weaponSwitchSpeed);
            list6.Add((int)attackSpeed);
            list6.Add((int)innerRatio);
            list6.Add((int)recoveryOfQiDisorder);
            this.displayList.Add("secondaryAttribute");
            this.displayList.Add(this.ListToString<int>(list6, ',') ?? "");
        }

        public unsafe int CalcScope()
        {
            int scope = 0;

            // 功法资质
            fixed (short* ptr3 = this.combatSkillQualifications_ForDisplay.Items)
            {
                short* ptr4 = ptr3;
                for (sbyte b2 = 0; b2 < 14; b2 += 1)
                {
                    short item2 = ptr4[b2];
                    scope += item2;
                }
            }

            // 生活资质
            fixed (short* ptr = this.lifeSkillQualifications_ForDisplay.Items)
            {
                short* ptr2 = ptr;
                for (sbyte b = 0; b < 16; b += 1)
                {
                    short item = ptr2[b];
                    scope += item;
                }
            }

            // 主要属性
            fixed (short* ptr5 = this.mainAttributes__ForDisplay.Items)
            {
                short* ptr6 = ptr5;
                for (sbyte b3 = 0; b3 < 6; b3 += 1)
                {
                    short item3 = ptr6[b3];
                    scope += item3;
                }
            }

            HitOrAvoidInts hitValues = this.character.GetHitValues();
            OuterAndInnerInts penetrations = this.character.GetPenetrations();
            HitOrAvoidInts avoidValues = this.character.GetAvoidValues();
            OuterAndInnerInts penetrationResists = this.character.GetPenetrationResists();
            OuterAndInnerShorts recoveryOfStanceAndBreath = this.character.GetRecoveryOfStanceAndBreath();
            short moveSpeed = this.character.GetMoveSpeed();
            short recoveryOfFlaw = this.character.GetRecoveryOfFlaw();
            short castSpeed = this.character.GetCastSpeed();
            short recoveryOfBlockedAcupoint = this.character.GetRecoveryOfBlockedAcupoint();
            short weaponSwitchSpeed = this.character.GetWeaponSwitchSpeed();
            short attackSpeed = this.character.GetAttackSpeed();
            short innerRatio = this.character.GetInnerRatio();
            short recoveryOfQiDisorder = this.character.GetRecoveryOfQiDisorder();

            // 攻击属性
            for (sbyte b4 = 0; b4 < 4; b4 += 1)
            {
                int item4 = hitValues.Items[b4];
                scope += item4;
            }

            // 防御属性
            for (sbyte b5 = 0; b5 < 4; b5 += 1)
            {
                int item5 = avoidValues.Items[b5];
                scope += item5;
            }

            scope += penetrations.Outer;
            scope += penetrations.Inner;
            scope += penetrationResists.Outer;
            scope += penetrationResists.Inner;

            // 次要属性
            if (recoveryOfStanceAndBreath.Outer < 100) return 0;
            scope += (int)recoveryOfStanceAndBreath.Outer;
            if (recoveryOfStanceAndBreath.Inner < 100) return 0;
            scope += (int)recoveryOfStanceAndBreath.Inner;
            if (moveSpeed < 100) return 0;
            scope += (int)moveSpeed;
            if (recoveryOfFlaw < 100) return 0;
            scope += (int)recoveryOfFlaw;
            if (castSpeed < 100) return 0;
            scope += (int)castSpeed;
            if (recoveryOfBlockedAcupoint < 100) return 0;
            scope += (int)recoveryOfBlockedAcupoint;
            if (weaponSwitchSpeed < 100) return 0;
            scope += (int)weaponSwitchSpeed;
            if (attackSpeed < 100) return 0;
            scope += (int)attackSpeed;
            if (innerRatio < 100) return 0;
            scope += (int)innerRatio;
            if (recoveryOfQiDisorder < 100) return 0;
            scope += (int)recoveryOfQiDisorder;

            return scope;
        }

        // Token: 0x06000011 RID: 17 RVA: 0x00002F54 File Offset: 0x00001154
        private string ListToString<T>(List<T> list, char charPstr)
        {
            string text = "";
            bool flag = list.Count == 1;
            string result;
            if (flag)
            {
                string str = text;
                T t = list[0];
                text = str + t.ToString();
                result = text;
            }
            else
            {
                for (int i = 0; i < list.Count; i++)
                {
                    string str2 = text;
                    T t = list[i];
                    text = str2 + t.ToString();
                    text += charPstr.ToString();
                }
                text = text.TrimEnd(charPstr);
                result = text;
            }
            return result;
        }

        // Token: 0x06000012 RID: 18 RVA: 0x00002FEC File Offset: 0x000011EC
        private string ListToString<T>(List<T> list)
        {
            string text = "";
            bool flag = list.Count == 1;
            string result;
            if (flag)
            {
                string str = text;
                T t = list[0];
                text = str + t.ToString();
                result = text;
            }
            else
            {
                for (int i = 0; i < list.Count; i++)
                {
                    string str2 = text;
                    T t = list[i];
                    text = str2 + t.ToString();
                }
                result = text;
            }
            return result;
        }

        // Token: 0x0400000A RID: 10
        public Character character;

        // Token: 0x0400000B RID: 11
        public Character.ProtagonistFeatureRelatedStatus status;

        // Token: 0x0400000C RID: 12
        public sbyte lifeSkillQualificationGrowthType;

        // Token: 0x0400000D RID: 13
        public sbyte combatSkillQualificationGrowthType;

        // Token: 0x0400000E RID: 14
        public List<short> featureIds;

        // Token: 0x0400000F RID: 15
        public LifeSkillShorts lifeSkillQualifications_ForOverwrite;

        // Token: 0x04000010 RID: 16
        public CombatSkillShorts combatSkillQualifications_ForOverwrite;

        // Token: 0x04000011 RID: 17
        public MainAttributes mainAttributes_ForOverwrite;

        // Token: 0x04000012 RID: 18
        public LifeSkillShorts lifeSkillQualifications_ForDisplay;

        // Token: 0x04000013 RID: 19
        public CombatSkillShorts combatSkillQualifications_ForDisplay;

        // Token: 0x04000014 RID: 20
        public MainAttributes mainAttributes__ForDisplay;

        // Token: 0x04000015 RID: 21
        public TempIteamData itemData;

        // Token: 0x04000016 RID: 22
        public List<string> displayList;
    }
}
