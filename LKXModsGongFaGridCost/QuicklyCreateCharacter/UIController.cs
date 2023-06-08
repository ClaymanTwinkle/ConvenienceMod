using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ConvenienceFrontend.QuicklyCreateCharacter
{
    public class UIController : MonoBehaviour
    {
        // Token: 0x0600000A RID: 10 RVA: 0x00002380 File Offset: 0x00000580
        private void Awake()
        {
            this._winRect = new Rect(new Vector2(50f, 400f), new Vector2(300f, 600f));
            this.UpdateButtonRect();
        }

        // Token: 0x0600000B RID: 11 RVA: 0x000023B4 File Offset: 0x000005B4
        private void UpdateButtonRect()
        {
            this._buttonRect = new Rect(new Vector2(this._winRect.xMax - 150f - 50f, this._winRect.yMax + 10f), new Vector2(100f, 40f));
        }

        // Token: 0x0600000C RID: 12 RVA: 0x0000240C File Offset: 0x0000060C
        private void OnGUI()
        {
            bool bool_WindowShow = this._bool_WindowShow;
            if (bool_WindowShow)
            {
                this._winRect = GUILayout.Window(1, this._winRect, new GUI.WindowFunction(this.WindowFunc), "<b><color=#ffffffff><size=16>随机人物属性</size></color></b>", Array.Empty<GUILayoutOption>());
                this.UpdateButtonRect();
                bool flag = GUI.Button(this._buttonRect, "<b><size=16>随机属性</size></b>");
                if (flag)
                {
                    this.DoClickRoll();
                }
            }
        }

        // Token: 0x0600000D RID: 13 RVA: 0x00002474 File Offset: 0x00000674
        private void WindowFunc(int winId)
        {
            GUI.DragWindow();
            bool flag = this.dataController.characterDataList.Count > 0;
            if (flag)
            {
                string text = this.dataController.characterDataDict[CharacterDataType.LifeSkillGrowthType][0];
                string text2 = this.dataController.characterDataNameDict[CharacterDataType.LifeSkillGrowthType][0];
                string text3 = this.dataController.characterDataColorDict[CharacterDataType.LifeSkillGrowthType][0];
                GUILayout.Space(10f);
                GUILayout.BeginVertical(GUI.skin.box, Array.Empty<GUILayoutOption>());
                GUILayout.Label(string.Concat(new string[]
                {
                    "<b><color=",
                    text3,
                    "><size=16>技艺资质（",
                    text2,
                    "）</size></color></b>"
                }), Array.Empty<GUILayoutOption>());
                for (int i = 0; i < 16; i += 4)
                {
                    GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                    for (int j = 0; j < 4; j++)
                    {
                        bool flag2 = i + j >= 16;
                        if (flag2)
                        {
                            break;
                        }
                        string text4 = this.dataController.characterDataDict[CharacterDataType.LifeSkillQualification][i + j];
                        string text5 = this.dataController.characterDataNameDict[CharacterDataType.LifeSkillQualification][i + j];
                        string text6 = this.dataController.characterDataColorDict[CharacterDataType.LifeSkillQualification][i + j];
                        GUILayout.Label(string.Concat(new string[]
                        {
                            "<b><color=",
                            text6,
                            "><size=14>",
                            text5,
                            "  ",
                            text4,
                            "</size></color></b>"
                        }), Array.Empty<GUILayoutOption>());
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
                string text7 = this.dataController.characterDataDict[CharacterDataType.CombatSkillGrowthType][0];
                string text8 = this.dataController.characterDataNameDict[CharacterDataType.CombatSkillGrowthType][0];
                string text9 = this.dataController.characterDataColorDict[CharacterDataType.CombatSkillGrowthType][0];
                GUILayout.BeginVertical(GUI.skin.box, Array.Empty<GUILayoutOption>());
                GUILayout.Label(string.Concat(new string[]
                {
                    "<b><color=",
                    text9,
                    "><size=16>功法资质（",
                    text8,
                    "）</size></color></b>"
                }), Array.Empty<GUILayoutOption>());
                for (int k = 0; k < 14; k += 4)
                {
                    GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                    for (int l = 0; l < 4; l++)
                    {
                        bool flag3 = k + l >= 14;
                        if (flag3)
                        {
                            break;
                        }
                        string text10 = this.dataController.characterDataDict[CharacterDataType.CombatSkillQualification][k + l];
                        string text11 = this.dataController.characterDataNameDict[CharacterDataType.CombatSkillQualification][k + l];
                        string text12 = this.dataController.characterDataColorDict[CharacterDataType.CombatSkillQualification][k + l];
                        GUILayout.Label(string.Concat(new string[]
                        {
                            "<b><color=",
                            text12,
                            "><size=14>",
                            text11,
                            "  ",
                            text10,
                            "</size></color></b>"
                        }), Array.Empty<GUILayoutOption>());
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
                GUILayout.Space(10f);
                GUILayout.BeginVertical(GUI.skin.box, Array.Empty<GUILayoutOption>());
                GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                GUILayout.Label("<b><size=16>特性</size></b>", Array.Empty<GUILayoutOption>());
                for (int m = 0; m < 3; m++)
                {
                    string text13 = this.dataController.characterDataDict[CharacterDataType.FeatureMedalValue][m];
                    string text14 = this.dataController.characterDataNameDict[CharacterDataType.FeatureMedalValue][m];
                    string text15 = this.dataController.characterDataColorDict[CharacterDataType.FeatureMedalValue][m];
                    GUILayout.Label(string.Concat(new string[]
                    {
                        "<b><color=",
                        text15,
                        "><size=14>",
                        text14,
                        "×",
                        text13.TrimStart(new char[]
                        {
                            '-'
                        }),
                        "</size></color></b>"
                    }), Array.Empty<GUILayoutOption>());
                }
                GUILayout.EndHorizontal();
                for (int n = 0; n < this.dataController.characterDataNameDict[CharacterDataType.FeatureIds].Count; n++)
                {
                    GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                    for (int num = 0; num < 4; num++)
                    {
                        bool flag4 = n + num >= this.dataController.characterDataNameDict[CharacterDataType.FeatureIds].Count;
                        if (flag4)
                        {
                            break;
                        }
                        string text16 = this.dataController.characterDataNameDict[CharacterDataType.FeatureIds][n + num];
                        string text17 = this.dataController.characterDataColorDict[CharacterDataType.FeatureIds][n + num];
                        GUILayout.Label(string.Concat(new string[]
                        {
                            "<b><color=",
                            text17,
                            "><size=14>",
                            text16,
                            "</size></color></b>"
                        }), Array.Empty<GUILayoutOption>());
                    }
                    GUILayout.EndHorizontal();
                    n += 4;
                }
                GUILayout.EndVertical();
                GUILayout.Space(10f);
                GUILayout.BeginVertical(GUI.skin.box, Array.Empty<GUILayoutOption>());
                GUILayout.Label("<b><size=16>主属性</size></b>", Array.Empty<GUILayoutOption>());
                for (int num2 = 0; num2 < 6; num2 += 3)
                {
                    GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                    for (int num3 = 0; num3 < 3; num3++)
                    {
                        bool flag5 = num2 + num3 >= 6;
                        if (flag5)
                        {
                            break;
                        }
                        string text18 = this.dataController.characterDataDict[CharacterDataType.MainAttribute][num2 + num3];
                        string text19 = this.dataController.characterDataNameDict[CharacterDataType.MainAttribute][num2 + num3];
                        string text20 = this.dataController.characterDataColorDict[CharacterDataType.MainAttribute][num2 + num3];
                        GUILayout.Label(string.Concat(new string[]
                        {
                            "<b><color=",
                            text20,
                            "><size=14>",
                            text19,
                            " ",
                            text18,
                            "</size></color></b>"
                        }), Array.Empty<GUILayoutOption>());
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
                bool flag6 = this.dataController.characterDataDict.ContainsKey(CharacterDataType.LifeSkillBookName);
                if (flag6)
                {
                    string text21 = this.dataController.characterDataNameDict[CharacterDataType.LifeSkillBookName][0];
                    string text22 = this.dataController.characterDataColorDict[CharacterDataType.LifeSkillBookName][0];
                    string str = this.dataController.characterDataNameDict[CharacterDataType.LifeSkillBookType][0];
                    string text23 = this.dataController.characterDataNameDict[CharacterDataType.CombatSkillBookName][0];
                    string text24 = this.dataController.characterDataColorDict[CharacterDataType.CombatSkillBookName][0];
                    GUILayout.Space(10f);
                    GUILayout.BeginVertical(GUI.skin.box, Array.Empty<GUILayoutOption>());
                    GUILayout.Label("<b><size=16>古冢遗刻</size></b>", Array.Empty<GUILayoutOption>());
                    GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                    GUILayout.Label(string.Concat(new string[]
                    {
                        "<b><color=",
                        text22,
                        "><size=16>",
                        text21,
                        "</size></color></b>"
                    }), Array.Empty<GUILayoutOption>());
                    GUILayout.Label("<b><size=14>" + str + "</size></b>", Array.Empty<GUILayoutOption>());
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                    GUILayout.Label(string.Concat(new string[]
                    {
                        "<b><color=",
                        text24,
                        "><size=16>",
                        text23,
                        "</size></color></b>"
                    }), Array.Empty<GUILayoutOption>());
                    for (int num4 = 0; num4 < this.dataController.characterDataNameDict[CharacterDataType.CombatSkillBookPageType].Count; num4++)
                    {
                        string text25 = this.dataController.characterDataNameDict[CharacterDataType.CombatSkillBookPageType][num4];
                        string text26 = (num4 > 0) ? this.dataController.characterDataColorDict[CharacterDataType.CombatSkillBookPageType][num4] : CharacterDataTool.GetColorWhite("");
                        GUILayout.Label(string.Concat(new string[]
                        {
                            "<b><color=",
                            text26,
                            "><size=14>",
                            text25,
                            "</size></color></b>"
                        }), Array.Empty<GUILayoutOption>());
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                }
            }
            else
            {
                GUILayout.Label("<b><size=30>Loading</size></b>", Array.Empty<GUILayoutOption>());
            }
        }

        // Token: 0x0600000E RID: 14 RVA: 0x00002D74 File Offset: 0x00000F74
        public void ShowUI()
        {
            this._bool_WindowShow = true;
        }

        // Token: 0x0600000F RID: 15 RVA: 0x00002D7E File Offset: 0x00000F7E
        public void CloseUI()
        {
            this._bool_WindowShow = false;
        }

        // Token: 0x06000010 RID: 16 RVA: 0x00002D88 File Offset: 0x00000F88
        public void DoClickRoll()
        {
            this.dataController.DoRollCharacterData();
        }

        // Token: 0x04000009 RID: 9
        public CharacterDataController dataController;

        // Token: 0x0400000A RID: 10
        private bool _bool_WindowShow;

        // Token: 0x0400000B RID: 11
        private Rect _winRect;

        // Token: 0x0400000C RID: 12
        private Rect _buttonRect;
    }
}
