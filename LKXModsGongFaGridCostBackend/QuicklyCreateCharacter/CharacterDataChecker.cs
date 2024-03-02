using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData.Utilities;
using HarmonyLib;
using Newtonsoft.Json.Linq;

namespace ConvenienceBackend.QuicklyCreateCharacter
{
    internal class CharacterDataChecker
    {
        public unsafe static int CheckCharacterDataScore(TempCharacterData characterData, Dictionary<string, System.Object> _config)
        {
            // 古冢遗刻-功法书-书名
            if (!CheckCombatSkillBookName(characterData, _config)) return 0;

            // 全蓝特性
            if (!CheckAllBlueFeatures(characterData, _config)) return 1;

            // 特性
            if (!CheckFeatureNames(characterData, _config)) return 2;

            // 古冢遗刻-功法书-总纲
            if (!CheckCombatSkillGeneralPrinciples(characterData, _config)) return 3;

            // 成长类型
            if (!CheckQualificationGrowthType(characterData, _config)) return 4;

            // 主要属性
            if (!CheckMainAttributeValue(characterData, _config)) return 5;

            // 资质
            if (!CheckQualificationsValue(characterData, _config)) return 6;

            // 古冢遗刻-技艺书
            if (!CheckLifeSkillBookName(characterData, _config)) return 7;

            // 古冢遗刻-功法书-正逆练
            if (!CheckCombatSkillDirectAndReverse(characterData, _config)) return 8;

            return 100;
        }

        public unsafe static bool CheckCharacterDataValue(TempCharacterData characterData, Dictionary<string, System.Object> _config)
        {
            // 全蓝特性
            if (!CheckAllBlueFeatures(characterData, _config)) return false;

            // 特性
            if (!CheckFeatureNames(characterData, _config)) return false;

            // 成长类型
            if (!CheckQualificationGrowthType(characterData, _config)) return false;

            // 资质
            if (!CheckQualificationsValue(characterData, _config)) return false;

            // 主要属性
            if (!CheckMainAttributeValue(characterData, _config)) return false;

            // 古冢遗刻-技艺书
            if (!CheckLifeSkillBookName(characterData, _config)) return false;

            // 古冢遗刻-功法书
            // 书名
            if (!CheckCombatSkillBookName(characterData, _config)) return false;
            // 正逆练
            if (!CheckCombatSkillDirectAndReverse(characterData, _config)) return false;
            // 总纲
            if (!CheckCombatSkillGeneralPrinciples(characterData, _config)) return false;

            return true;
        }

        private unsafe static bool CheckAllBlueFeatures(TempCharacterData characterData, Dictionary<string, System.Object> _config)
        {
            var enableAllBlue = RollAttributeConfigReader.EnableAllBlueFeature(_config);
            if (enableAllBlue)
            {
                foreach (var featureId in characterData.featureIds)
                {
                    if (Config.CharacterFeature.Instance[featureId].Level < 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private unsafe static bool CheckFeatureNames(TempCharacterData characterData, Dictionary<string, System.Object> _config)
        {
            string[] filterFeatureList = (_config.GetTypedValue<string>("InputField_FilterAllFeatures") ?? "").Split(' ');
            if (filterFeatureList.Length > 0)
            {
                var featureNameList = characterData.featureIds.ConvertAll<string>(x => Config.CharacterFeature.Instance[x].Name);
                foreach (var featureName in filterFeatureList)
                {
                    if (featureName.Trim().Length > 0 && !featureNameList.Contains(featureName)) { return false; }
                }
            }
            return true;
        }

        private unsafe static bool CheckQualificationGrowthType(TempCharacterData characterData, Dictionary<string, System.Object> _config)
        {
            // 技艺成长
            JArray lifeSkillGrowthTypeJArray = (JArray)_config.GetTypedValue<JArray>("ToggleGroup_FilterLifeSkillQualificationGrowthType") ?? new JArray();
            if (lifeSkillGrowthTypeJArray.Count > 0)
            {
                if (!(bool)lifeSkillGrowthTypeJArray[characterData.lifeSkillQualificationGrowthType])
                {
                    foreach (bool item in lifeSkillGrowthTypeJArray)
                    {
                        if (item)
                        {
                            return false;
                        }
                    }
                }
            }

            // 功法成长
            JArray combatSkillGrowthTypeJArray = (JArray)_config.GetTypedValue<JArray>("ToggleGroup_FilterCombatSkillQualificationGrowthType") ?? new JArray();
            var combatSkillGrowthType = characterData.combatSkillQualificationGrowthType;
            if (combatSkillGrowthTypeJArray.Count > combatSkillGrowthType)
            {
                if (!(bool)combatSkillGrowthTypeJArray[combatSkillGrowthType])
                {
                    foreach (bool item in combatSkillGrowthTypeJArray)
                    {
                        if (item)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private unsafe static bool CheckQualificationsValue(TempCharacterData characterData, Dictionary<string, System.Object> _config)
        {
            // 技艺资质
            JArray lifeSkillQualificationsTypesJArray = (JArray)_config.GetTypedValue<JArray>("ToggleGroup_FilterLifeSkillQualificationsTypes") ?? new JArray();
            if (lifeSkillQualificationsTypesJArray.Count > 0)
            {
                var lifeSkillQualificationsValue = (float)_config.GetTypedValue<Double>("SliderBar_LifeSkillQualificationsValue");

                fixed (short* ptr = characterData.lifeSkillQualifications_ForDisplay.Items)
                {
                    short* ptr2 = ptr;
                    for (sbyte i = 0; i < 16; i += 1)
                    {
                        if ((bool)lifeSkillQualificationsTypesJArray[i])
                        {
                            short item = ptr2[i];
                            if (item < lifeSkillQualificationsValue)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            //功法资质
            JArray combatSkillQualificationsTypesJArray = (JArray)_config.GetTypedValue<JArray>("ToggleGroup_FilterCombatSkillQualificationsTypes") ?? new JArray();
            if (combatSkillQualificationsTypesJArray.Count > 0)
            {
                var combatSkillQualificationsValue = (float)_config.GetTypedValue<Double>("SliderBar_FilterCombatSkillQualificationsValue");

                fixed (short* ptr3 = characterData.combatSkillQualifications_ForDisplay.Items)
                {
                    short* ptr4 = ptr3;
                    List<short> list2 = new List<short>();
                    for (sbyte b2 = 0; b2 < 14; b2 += 1)
                    {
                        if ((bool)combatSkillQualificationsTypesJArray[b2])
                        {
                            short item2 = ptr4[b2];
                            if (item2 < combatSkillQualificationsValue)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        private unsafe static bool CheckMainAttributeValue(TempCharacterData characterData, Dictionary<string, System.Object> _config)
        {
            JArray mainAttributeTypesJArray = (JArray)_config.GetTypedValue<JArray>("ToggleGroup_FilterMainAttributeTypes") ?? new JArray();
            if (mainAttributeTypesJArray.Count > 0)
            {
                var minMainAttributeValue = (float)_config.GetTypedValue<Double>("SliderBar_FilterMainAttributeValue");

                fixed (short* ptr5 = characterData.mainAttributes__ForDisplay.Items)
                {
                    short* ptr6 = ptr5;
                    List<short> list3 = new List<short>();
                    for (sbyte b3 = 0; b3 < 6; b3 += 1)
                    {
                        if ((bool)mainAttributeTypesJArray[b3])
                        {
                            short item3 = ptr6[b3];
                            if (item3 < minMainAttributeValue)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        private static bool CheckLifeSkillBookName(TempCharacterData characterData, Dictionary<string, System.Object> _config)
        {
            JArray lifeSkillBookTypeJArray = RollAttributeConfigReader.GetLifeSkillBookTypes(_config);
            if (lifeSkillBookTypeJArray.Count > 0)
            {
                if (characterData.itemData != null && characterData.itemData.lifeSkillBook != null)
                {
                    var index = characterData.itemData.lifeSkillBook.GetLifeSkillType();
                    if (index > -1)
                    {
                        if (!(bool)lifeSkillBookTypeJArray[index])
                        {
                            foreach (bool item in lifeSkillBookTypeJArray)
                            {
                                if (item)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }

        private static bool CheckCombatSkillBookName(TempCharacterData characterData, Dictionary<string, System.Object> _config)
        {
            if (characterData.itemData != null && characterData.itemData.combatSkillBook != null)
            {
                // 书名
                string filterCombatSkillBookName = RollAttributeConfigReader.GetCombatSkillBookName(_config);
                if (!String.IsNullOrEmpty(filterCombatSkillBookName))
                {
                    var combatSkillBookName = characterData.itemData.combatSkillBook.GetName();
                    if (!combatSkillBookName.Contains(filterCombatSkillBookName))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool CheckCombatSkillDirectAndReverse(TempCharacterData characterData, Dictionary<string, System.Object> _config)
        {
            if (characterData.itemData != null && characterData.itemData.combatSkillBook != null)
            {
                // 正逆练
                var filterDirectAndReverse = _config.GetTypedValue<string>("InputField_FilterDirectAndReverse");
                if (filterDirectAndReverse != null)
                {
                    filterDirectAndReverse = filterDirectAndReverse.Trim();

                    if (filterDirectAndReverse.Length == 5)
                    {
                        string directAndReverse = "";

                        List<int> list = new List<int>();
                        list.Add(characterData.itemData.combatSkillBookPageTypes[0] + characterData.itemData.combatSkillBookPageTypes[1] * 2 + characterData.itemData.combatSkillBookPageTypes[2] * 4);
                        for (int i = 3; i < characterData.itemData.combatSkillBookPageTypes.Count; i++)
                        {
                            list.Add(characterData.itemData.combatSkillBookPageTypes[i]);
                        }

                        for (int i = 1; i < 6; i++)
                        {
                            if (list[i] == 0)
                            {
                                directAndReverse += "正";
                            }
                            else
                            {
                                directAndReverse += "逆";
                            }
                        }
                        if (!directAndReverse.Contains(filterDirectAndReverse))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private static bool CheckCombatSkillGeneralPrinciples(TempCharacterData characterData, Dictionary<string, System.Object> _config)
        {
            if (characterData.itemData != null && characterData.itemData.combatSkillBook != null)
            {
                // 总纲
                JArray generalPrinciplesJArray = (JArray)_config.GetTypedValue<JArray>("ToggleGroup_FilterGeneralPrinciples") ?? new JArray();
                if (generalPrinciplesJArray.Count > 0)
                {

                    List<int> list = new List<int>();
                    list.Add(characterData.itemData.combatSkillBookPageTypes[0] + characterData.itemData.combatSkillBookPageTypes[1] * 2 + characterData.itemData.combatSkillBookPageTypes[2] * 4);
                    for (int i = 3; i < characterData.itemData.combatSkillBookPageTypes.Count; i++)
                    {
                        list.Add(characterData.itemData.combatSkillBookPageTypes[i]);
                    }

                    var index = list[0];
                    if (index > -1)
                    {
                        if (!(bool)generalPrinciplesJArray[index])
                        {
                            foreach (bool item in generalPrinciplesJArray)
                            {
                                if (item)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }
    }
}
