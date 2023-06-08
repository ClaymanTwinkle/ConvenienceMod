using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Config.ConfigCells.Character;
using Config;
using UnityEngine;

namespace ConvenienceFrontend.QuicklyCreateCharacter
{
    internal static class CharacterDataTool
    {
        // Token: 0x0600001A RID: 26 RVA: 0x000031C4 File Offset: 0x000013C4
        public static Dictionary<CharacterDataType, List<string>> CharacterDataListToDataDict(List<string> argList)
        {
            Dictionary<CharacterDataType, List<string>> dictionary = new Dictionary<CharacterDataType, List<string>>();
            int i = 0;
            while (i < argList.Count)
            {
                string text = argList[i];
                switch (text)
                {
                    case "baseCombatSkillQualifications":
                        dictionary.Add(CharacterDataType.CombatSkillQualification, CharacterDataTool.StringArgToStringList(argList[i + 1]));
                        break;
                    case "atkHitAttribute":
                        dictionary.Add(CharacterDataType.AtkHitAttribute, CharacterDataTool.StringArgToStringList(argList[i + 1]));
                        break;
                    case "secondaryAttribute":
                        dictionary.Add(CharacterDataType.SecondaryAttribute, CharacterDataTool.StringArgToStringList(argList[i + 1]));
                        break;
                    case "combatSkillQualificationGrowthType":
                        dictionary.Add(CharacterDataType.CombatSkillGrowthType, new List<string>
                                    {
                                        argList[i + 1]
                                    });
                        break;
                    case "baseLifeSkillQualifications":
                        dictionary.Add(CharacterDataType.LifeSkillQualification, CharacterDataTool.StringArgToStringList(argList[i + 1]));
                        break;
                    case "atkPenetrability":
                        dictionary.Add(CharacterDataType.AtkPenetrability, CharacterDataTool.StringArgToStringList(argList[i + 1]));
                        break;
                    case "combatSkillBookPageTypes":
                        dictionary.Add(CharacterDataType.CombatSkillBookPageType, CharacterDataTool.NormalizeCombatSkillBookPageTypeList(CharacterDataTool.StringArgToStringList(argList[i + 1])));
                        break;
                    case "featureIds":
                        List<string> list = CharacterDataTool.DoHideFeatureIdList(CharacterDataTool.StringArgToStringList(argList[i + 1]));
                        dictionary.Add(CharacterDataType.FeatureIds, list);
                        dictionary.Add(CharacterDataType.FeatureMedalValue, CharacterDataTool.GetMedalsListByFeatureIdList(list));
                        break;
                    case "combatSkillBookName":
                        dictionary.Add(CharacterDataType.CombatSkillBookName, new List<string>
                            {
                                argList[i + 1]
                            });
                        break;
                    case "lifeSkillBookType":
                        dictionary.Add(CharacterDataType.LifeSkillBookType, new List<string>
                                {
                                    argList[i + 1]
                                });
                        break;
                    case "defHitAttribute":
                        dictionary.Add(CharacterDataType.DefHitAttribute, CharacterDataTool.StringArgToStringList(argList[i + 1]));
                        break;
                    case "lifeSkillBookName":
                        dictionary.Add(CharacterDataType.LifeSkillBookName, new List<string>
                                {
                                    argList[i + 1]
                                });
                        break;
                    case "mainAttributes":
                        dictionary.Add(CharacterDataType.MainAttribute, CharacterDataTool.StringArgToStringList(argList[i + 1]));
                        break;
                    case "lifeSkillQualificationGrowthType":
                        dictionary.Add(CharacterDataType.LifeSkillGrowthType, new List<string>
                            {
                                argList[i + 1]
                            });
                        break;
                    case "defPenetrability":
                        dictionary.Add(CharacterDataType.DefPenetrability, CharacterDataTool.StringArgToStringList(argList[i + 1]));
                        break;
                }
                i += 2;
            }
            return dictionary;
        }

        // Token: 0x0600001B RID: 27 RVA: 0x00003630 File Offset: 0x00001830
        public static Dictionary<CharacterDataType, List<short>> CharacterDataDictToShortDataDict(Dictionary<CharacterDataType, List<string>> dataDict)
        {
            Dictionary<CharacterDataType, List<short>> dictionary = new Dictionary<CharacterDataType, List<short>>();
            foreach (CharacterDataType key in dataDict.Keys)
            {
                switch (key)
                {
                    case CharacterDataType.LifeSkillGrowthType:
                    case CharacterDataType.LifeSkillQualification:
                    case CharacterDataType.CombatSkillGrowthType:
                    case CharacterDataType.CombatSkillQualification:
                    case CharacterDataType.FeatureIds:
                    case CharacterDataType.FeatureMedalValue:
                    case CharacterDataType.MainAttribute:
                    case CharacterDataType.LifeSkillBookType:
                    case CharacterDataType.CombatSkillBookPageType:
                    case CharacterDataType.AtkHitAttribute:
                    case CharacterDataType.DefHitAttribute:
                    case CharacterDataType.AtkPenetrability:
                    case CharacterDataType.DefPenetrability:
                    case CharacterDataType.SecondaryAttribute:
                        {
                            List<short> list = new List<short>();
                            for (int i = 0; i < dataDict[key].Count; i++)
                            {
                                list.Add((short)int.Parse(dataDict[key][i]));
                            }
                            dictionary.Add(key, list);
                            break;
                        }
                }
            }
            return dictionary;
        }

        // Token: 0x0600001C RID: 28 RVA: 0x00003738 File Offset: 0x00001938
        public static Dictionary<CharacterDataType, List<string>> CharacterDataDictToNameDict(Dictionary<CharacterDataType, List<string>> dataDict)
        {
            Dictionary<CharacterDataType, List<string>> dictionary = new Dictionary<CharacterDataType, List<string>>();
            foreach (CharacterDataType key in dataDict.Keys)
            {
                switch (key)
                {
                    case CharacterDataType.LifeSkillGrowthType:
                    case CharacterDataType.CombatSkillGrowthType:
                        dictionary.Add(key, new List<string>
                    {
                        CharacterDataTool.QualificationGrowthTypeNameArray[int.Parse(dataDict[key][0])]
                    });
                        break;
                    case CharacterDataType.LifeSkillQualification:
                        dictionary.Add(key, new List<string>(CharacterDataTool.LifeSkillNameArray));
                        break;
                    case CharacterDataType.CombatSkillQualification:
                        dictionary.Add(key, new List<string>(CharacterDataTool.CombatSkillNameArray));
                        break;
                    case CharacterDataType.FeatureIds:
                        dictionary.Add(key, CharacterDataTool.GetNameListByFeatureIdList(dataDict[key]));
                        break;
                    case CharacterDataType.FeatureMedalValue:
                        dictionary.Add(key, new List<string>(CharacterDataTool.FeatureMedalNameArray));
                        break;
                    case CharacterDataType.MainAttribute:
                        dictionary.Add(key, new List<string>(CharacterDataTool.MainAttributeNameArray));
                        break;
                    case CharacterDataType.LifeSkillBookName:
                        dictionary.Add(key, dataDict[key]);
                        break;
                    case CharacterDataType.LifeSkillBookType:
                        dictionary.Add(key, new List<string>
                    {
                        CharacterDataTool.LifeSkillNameArray[int.Parse(dataDict[key][0])]
                    });
                        break;
                    case CharacterDataType.CombatSkillBookName:
                        dictionary.Add(key, dataDict[key]);
                        break;
                    case CharacterDataType.CombatSkillBookPageType:
                        dictionary.Add(key, CharacterDataTool.GetNameListByCombatSkillBookPageType(dataDict[key]));
                        break;
                    case CharacterDataType.AtkHitAttribute:
                        dictionary.Add(key, new List<string>(CharacterDataTool.AtkHitAttributeNameArray));
                        break;
                    case CharacterDataType.DefHitAttribute:
                        dictionary.Add(key, new List<string>(CharacterDataTool.DefHitAttributeNameArray));
                        break;
                }
            }
            return dictionary;
        }

        // Token: 0x0600001D RID: 29 RVA: 0x00003930 File Offset: 0x00001B30
        public static Dictionary<CharacterDataType, List<string>> CharacterDataDictToColorDict(Dictionary<CharacterDataType, List<string>> dataDict)
        {
            Dictionary<CharacterDataType, List<string>> dictionary = new Dictionary<CharacterDataType, List<string>>();
            foreach (CharacterDataType key in dataDict.Keys)
            {
                switch (key)
                {
                    case CharacterDataType.LifeSkillGrowthType:
                    case CharacterDataType.CombatSkillGrowthType:
                        dictionary.Add(key, CharacterDataTool.GetColorList(dataDict[key], new CharacterDataTool.GetColorFunc(CharacterDataTool.GetColorWhite)));
                        break;
                    case CharacterDataType.LifeSkillQualification:
                    case CharacterDataType.CombatSkillQualification:
                        dictionary.Add(key, CharacterDataTool.GetColorList(dataDict[key], new CharacterDataTool.GetColorFunc(CharacterDataTool.GetColorByQualification)));
                        break;
                    case CharacterDataType.FeatureIds:
                        dictionary.Add(key, CharacterDataTool.GetColorList(dataDict[CharacterDataType.FeatureIds], new CharacterDataTool.GetColorFunc(CharacterDataTool.GetColorByFeatureId)));
                        break;
                    case CharacterDataType.FeatureMedalValue:
                        dictionary.Add(key, CharacterDataTool.GetColorList(dataDict[key], new CharacterDataTool.GetColorFunc(CharacterDataTool.GetColorByFeatureMedal)));
                        break;
                    case CharacterDataType.MainAttribute:
                        dictionary.Add(key, CharacterDataTool.GetColorList(dataDict[key], new CharacterDataTool.GetColorFunc(CharacterDataTool.GetColorWhite)));
                        break;
                    case CharacterDataType.LifeSkillBookName:
                        dictionary.Add(key, CharacterDataTool.GetColorList(dataDict[key], new CharacterDataTool.GetColorFunc(CharacterDataTool.GetColorYellow)));
                        break;
                    case CharacterDataType.LifeSkillBookType:
                        dictionary.Add(key, CharacterDataTool.GetColorList(dataDict[key], new CharacterDataTool.GetColorFunc(CharacterDataTool.GetColorWhite)));
                        break;
                    case CharacterDataType.CombatSkillBookName:
                        dictionary.Add(key, CharacterDataTool.GetColorList(dataDict[key], new CharacterDataTool.GetColorFunc(CharacterDataTool.GetColorYellow)));
                        break;
                    case CharacterDataType.CombatSkillBookPageType:
                        dictionary.Add(key, CharacterDataTool.GetColorList(dataDict[key], new CharacterDataTool.GetColorFunc(CharacterDataTool.GetColorByPageType)));
                        break;
                }
            }
            return dictionary;
        }

        // Token: 0x0600001E RID: 30 RVA: 0x00003B20 File Offset: 0x00001D20
        public static List<string> StringArgToStringList(string argString)
        {
            List<string> list = new List<string>();
            string[] array = argString.Split(new char[]
            {
                ','
            });
            for (int i = 0; i < array.Length; i++)
            {
                list.Add(array[i]);
            }
            return list;
        }

        // Token: 0x0600001F RID: 31 RVA: 0x00003B6C File Offset: 0x00001D6C
        public static List<string> DoHideFeatureIdList(List<string> argList)
        {
            List<string> list = new List<string>();
            for (int i = 0; i < argList.Count; i++)
            {
                CharacterFeature instance = CharacterFeature.Instance;
                int id = int.Parse(argList[i]);
                bool hidden = instance[id].Hidden;
                if (!hidden)
                {
                    list.Add(argList[i]);
                }
            }
            return list;
        }

        // Token: 0x06000020 RID: 32 RVA: 0x00003BD8 File Offset: 0x00001DD8
        public static List<string> NormalizeCombatSkillBookPageTypeList(List<string> argList)
        {
            List<string> list = new List<string>();
            list.Add((int.Parse(argList[0]) + int.Parse(argList[1]) * 2 + int.Parse(argList[2]) * 4).ToString());
            for (int i = 3; i < argList.Count; i++)
            {
                list.Add(argList[i]);
            }
            return list;
        }

        // Token: 0x06000021 RID: 33 RVA: 0x00003C54 File Offset: 0x00001E54
        public static List<string> GetMedalsListByFeatureIdList(List<string> argList)
        {
            List<string> list = new List<string>();
            CharacterFeature instance = CharacterFeature.Instance;
            for (sbyte b = 0; b < 3; b += 1)
            {
                int num = 0;
                int num2 = 0;
                for (int i = 0; i < argList.Count; i++)
                {
                    int id = int.Parse(argList[i]);
                    FeatureMedals featureMedals = instance[id].FeatureMedals[(int)b];
                    List<sbyte> values = featureMedals.Values;
                    for (int j = 0; j < values.Count; j++)
                    {
                        switch (values[j])
                        {
                            case 0:
                                num++;
                                break;
                            case 1:
                                num--;
                                break;
                            case 2:
                                num2++;
                                break;
                            case 3:
                                num2 -= 3;
                                break;
                        }
                    }
                }
                bool flag = num > 0;
                if (flag)
                {
                    int a = num + num2;
                    list.Add(Mathf.Max(0, Mathf.Min(a, 8)).ToString());
                }
                else
                {
                    bool flag2 = num < 0;
                    if (flag2)
                    {
                        int a2 = num - num2;
                        list.Add(Mathf.Max(-8, Mathf.Min(a2, 0)).ToString());
                    }
                    else
                    {
                        list.Add(0.ToString());
                    }
                }
            }
            return list;
        }

        // Token: 0x06000022 RID: 34 RVA: 0x00003DBC File Offset: 0x00001FBC
        public static List<string> GetNameListByFeatureIdList(List<string> argList)
        {
            List<string> list = new List<string>();
            CharacterFeature instance = CharacterFeature.Instance;
            for (int i = 0; i < argList.Count; i++)
            {
                int id = int.Parse(argList[i]);
                CharacterFeatureItem characterFeatureItem = instance[id];
                list.Add(instance[id].Name);
            }
            return list;
        }

        // Token: 0x06000023 RID: 35 RVA: 0x00003E20 File Offset: 0x00002020
        public static List<string> GetNameListByCombatSkillBookPageType(List<string> argList)
        {
            List<string> list = new List<string>();
            bool flag = argList.Count != 6;
            List<string> result;
            if (flag)
            {
                result = null;
            }
            else
            {
                switch (int.Parse(argList[0]))
                {
                    case 0:
                        list.Add("承");
                        break;
                    case 1:
                        list.Add("合");
                        break;
                    case 2:
                        list.Add("解");
                        break;
                    case 3:
                        list.Add("异");
                        break;
                    case 4:
                        list.Add("独");
                        break;
                    default:
                        list.Add("Null");
                        break;
                }
                for (int i = 1; i < 6; i++)
                {
                    bool flag2 = int.Parse(argList[i]) == 0;
                    if (flag2)
                    {
                        list.Add("正");
                    }
                    else
                    {
                        list.Add("逆");
                    }
                }
                result = list;
            }
            return result;
        }

        // Token: 0x06000024 RID: 36 RVA: 0x00003F20 File Offset: 0x00002120
        public static List<string> GetColorList(List<string> argStringList, CharacterDataTool.GetColorFunc getColorFunc)
        {
            List<string> list = new List<string>();
            for (int i = 0; i < argStringList.Count; i++)
            {
                list.Add(getColorFunc(argStringList[i]));
            }
            return list;
        }

        // Token: 0x06000025 RID: 37 RVA: 0x00003F64 File Offset: 0x00002164
        public static string GetColorWhite(string argString)
        {
            return "#ffffffff";
        }

        // Token: 0x06000026 RID: 38 RVA: 0x00003F7C File Offset: 0x0000217C
        public static string GetColorYellow(string argString)
        {
            return "#ffff00ff";
        }

        // Token: 0x06000027 RID: 39 RVA: 0x00003F94 File Offset: 0x00002194
        public static string GetColorByQualification(string argString)
        {
            int num = int.Parse(argString);
            bool flag = num >= 90;
            string result;
            if (flag)
            {
                result = "#ff0000ff";
            }
            else
            {
                bool flag2 = num >= 80;
                if (flag2)
                {
                    result = "#ffa500ff";
                }
                else
                {
                    bool flag3 = num >= 70;
                    if (flag3)
                    {
                        result = "#ff00ffff";
                    }
                    else
                    {
                        bool flag4 = num >= 60;
                        if (flag4)
                        {
                            result = "#00ffffff";
                        }
                        else
                        {
                            bool flag5 = num >= 40;
                            if (flag5)
                            {
                                result = "#ffffffff";
                            }
                            else
                            {
                                result = "#c0c0c0ff";
                            }
                        }
                    }
                }
            }
            return result;
        }

        // Token: 0x06000028 RID: 40 RVA: 0x00004024 File Offset: 0x00002224
        public static string GetColorByFeatureId(string argString)
        {
            CharacterFeature instance = CharacterFeature.Instance;
            int id = int.Parse(argString);
            CharacterFeatureItem characterFeatureItem = instance[id];
            string result;
            switch (instance[id].CandidateGroupId)
            {
                case -1:
                    result = "#ffffffff";
                    break;
                case 0:
                    result = "#00ffffff";
                    break;
                case 1:
                    result = "#ff0000ff";
                    break;
                default:
                    result = "#ffffffff";
                    break;
            }
            return result;
        }

        // Token: 0x06000029 RID: 41 RVA: 0x00004094 File Offset: 0x00002294
        public static string GetColorByFeatureMedal(string argString)
        {
            int num = int.Parse(argString);
            bool flag = num > 0;
            string result;
            if (flag)
            {
                result = "#00ffffff";
            }
            else
            {
                bool flag2 = num < 0;
                if (flag2)
                {
                    result = "#ff0000ff";
                }
                else
                {
                    result = "#c0c0c0ff";
                }
            }
            return result;
        }

        // Token: 0x0600002A RID: 42 RVA: 0x000040D4 File Offset: 0x000022D4
        public static string GetColorByPageType(string argString)
        {
            string result;
            if (!(argString == "0"))
            {
                if (!(argString == "1"))
                {
                    result = "#ffffffff";
                }
                else
                {
                    result = "#ff0000ff";
                }
            }
            else
            {
                result = "#00ffffff";
            }
            return result;
        }

        // Token: 0x04000017 RID: 23
        public static string[] QualificationGrowthTypeNameArray = new string[]
        {
            "均衡",
            "早熟",
            "晚成"
        };

        // Token: 0x04000018 RID: 24
        public static string[] LifeSkillNameArray = new string[]
        {
            "音律",
            "弈棋",
            "诗书",
            "绘画",
            "术数",
            "品鉴",
            "锻造",
            "制木",
            "医术",
            "毒术",
            "织锦",
            "巧匠",
            "道法",
            "佛学",
            "厨艺",
            "杂学"
        };

        // Token: 0x04000019 RID: 25
        public static string[] CombatSkillNameArray = new string[]
        {
            "内功",
            "身法",
            "绝技",
            "拳掌",
            "指法",
            "腿法",
            "暗器",
            "剑法",
            "刀法",
            "长兵",
            "奇门",
            "软兵",
            "御射",
            "乐器"
        };

        // Token: 0x0400001A RID: 26
        public static string[] MainAttributeNameArray = new string[]
        {
            "膂力",
            "灵敏",
            "定力",
            "体质",
            "根骨",
            "悟性"
        };

        // Token: 0x0400001B RID: 27
        public static string[] FeatureMedalNameArray = new string[]
        {
            "进攻",
            "守御",
            "机略"
        };

        // Token: 0x0400001C RID: 28
        public static string[] AtkHitAttributeNameArray = new string[]
        {
            "力道",
            "迅疾",
            "卸力",
            "闪避"
        };

        // Token: 0x0400001D RID: 29
        public static string[] DefHitAttributeNameArray = new string[]
        {
            "精妙",
            "动心",
            "拆招",
            "守心"
        };

        // Token: 0x0200000C RID: 12
        // (Invoke) Token: 0x0600005B RID: 91
        public delegate string GetColorFunc(string argString);
    }
}
