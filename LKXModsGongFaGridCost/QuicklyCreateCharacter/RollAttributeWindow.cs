using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using TMPro;
using UICommon.Character.Elements;
using UICommon.Character;
using UnityEngine.Events;
using UnityEngine;
using ConvenienceFrontend.TaiwuBuildingManager;
using FrameWork;
using DG.Tweening;
using ConvenienceFrontend.CombatStrategy;
using FrameWork.ModSystem;

namespace ConvenienceFrontend.QuicklyCreateCharacter
{
    public class RollAttributeWindow : MonoBehaviour
    {
        // Token: 0x0600002C RID: 44 RVA: 0x00004314 File Offset: 0x00002514
        private void Awake()
        {
            this._initialPostion = this._windowSize / 2f + new Vector2(-this._windowSize.x, 0f) + new Vector2(this._windowPadding.x, -this._windowPadding.y);
            this._currentPostion = this._initialPostion;
        }

        // Token: 0x0600002D RID: 45 RVA: 0x00004380 File Offset: 0x00002580
        private void Start()
        {
            this.CreateUI();
        }

        // Token: 0x0600002E RID: 46 RVA: 0x0000438C File Offset: 0x0000258C
        private void OnDestroy()
        {
            bool flag = this._maskGo != null;
            if (flag)
            {
                Object.Destroy(this._maskGo);
            }
        }

        // Token: 0x0600002F RID: 47 RVA: 0x000043B8 File Offset: 0x000025B8
        private void CreateUI()
        {
            GameObject newUIMaskGo = UIFactory.GetNewUIMaskGo(new Color(0f, 0f, 0f, 0.8f));
            newUIMaskGo.transform.SetParent(this._layer.transform, false);
            newUIMaskGo.name = "customUIMask";
            newUIMaskGo.SetActive(false);
            this._maskGo = newUIMaskGo;
            GameObject newBackgroundContainerGoA = UIFactory.GetNewBackgroundContainerGoA();
            this._backgroundGo = newBackgroundContainerGoA;
            newBackgroundContainerGoA.transform.SetParent(this._maskGo.transform, false);
            Vector2 sizeDelta = newBackgroundContainerGoA.transform.Find("CoverFrame2").gameObject.GetComponent<RectTransform>().sizeDelta;
            Vector2 sizeDelta2 = newBackgroundContainerGoA.transform.Find("CoverFrame").gameObject.GetComponent<RectTransform>().sizeDelta;
            Vector2 sizeDelta3 = this._windowSize * sizeDelta2 / sizeDelta;
            newBackgroundContainerGoA.transform.Find("CoverFrame2").gameObject.GetComponent<RectTransform>().sizeDelta = this._windowSize;
            newBackgroundContainerGoA.transform.Find("CoverFrame").gameObject.GetComponent<RectTransform>().sizeDelta = sizeDelta3;
            newBackgroundContainerGoA.transform.localPosition = this._backgroundOffset;
            newBackgroundContainerGoA.GetComponent<RectTransform>().sizeDelta = sizeDelta3;
            newBackgroundContainerGoA.name = "customBackground";
            GameObject totalTitleGo = UIFactory.GetTotalTitleGo(this._title);
            totalTitleGo.transform.SetParent(this._backgroundGo.transform, false);
            totalTitleGo.GetComponent<RectTransform>().sizeDelta = new Vector2(this._windowSize.x - this._windowPadding.x * 2f, 45f);
            totalTitleGo.transform.localPosition = new Vector2(0f, this._windowSize.y * 0.45f);
            this.ReSetPosition(totalTitleGo.GetComponent<RectTransform>().sizeDelta.y + this._titleUnderSpace - this._gameObjectMargin.y);
            this._closeButton = UIFactory.GetNewXCloseButtonGo(new UnityAction(this.Close));
            this._closeButton.transform.SetParent(newBackgroundContainerGoA.transform, false);
            this._closeButton.transform.localPosition = this._windowSize * 0.5f;
            this._closeButton.name = "customCloseBtn";
            this._rollButton = UIFactory.GetRollButtonGo("开始摇属性", new UnityAction(() => {
                if (characterDataController.IsRolling)
                {
                    this.characterDataController.StopRollCharacterData();
                }
                else
                {
                    this.characterDataController.StartRollCharacterData();
                }
            }));
            this._rollButton.transform.SetParent(newBackgroundContainerGoA.transform, false);
            this._rollButton.transform.localPosition = new Vector2(0f, -this._windowSize.y * 0.6f);
            this._rollButton.name = "customRollBtn";

            var settingButton = GameObjectCreationUtils.UGUICreateCButton(newBackgroundContainerGoA.transform, new Vector2(200f, -this._windowSize.y * 0.6f), new Vector2(100, 40), 18, "设置");
            settingButton.ClearAndAddListener(delegate() { 
                ShowConfigPanel();
            });
            this._settingsButton = settingButton.gameObject;

            this._rollingCountText = GameObjectCreationUtils.UGUICreateTMPText(newBackgroundContainerGoA.transform, new Vector2(0f, -this._windowSize.y * 0.6f - 80), new Vector2(500, 50), 26, "");

            GameObject newBackgroundContainerGoB = UIFactory.GetNewBackgroundContainerGoB();
            newBackgroundContainerGoB.transform.SetParent(this._maskGo.transform, false);
            newBackgroundContainerGoB.GetComponent<RectTransform>().sizeDelta = new Vector2(1000f, 400f);
            newBackgroundContainerGoB.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(1000f, 400f);
            newBackgroundContainerGoB.transform.localPosition = new Vector3(-250f, 310f, 0f);
            GameObject gameObject = this.CreateEmptyContainerGo(newBackgroundContainerGoB, new Vector3(-450f, 100f, 0f));

            GameObject subTitleGo = UIFactory.GetSubTitleGo("技艺资质");
            subTitleGo.transform.SetParent(gameObject.transform, false);
            subTitleGo.transform.localPosition = new Vector3(0f, 50f, 0f);
            GameObject blankLabelGo = UIFactory.GetBlankLabelGo();
            blankLabelGo.transform.SetParent(gameObject.transform, false);
            blankLabelGo.transform.localPosition = new Vector3(120f, 50f, 0f);
            this._lifeGrowthLabelGo = blankLabelGo;
            for (int i = 0; i < 8; i++)
            {
                GameObject lifeQulificationGo = UIFactory.GetLifeQulificationGo(i);
                lifeQulificationGo.transform.SetParent(gameObject.transform, false);
                this.lifeQulificationContainerDict.Add(i, new RollAttributeWindow.QulificationContainer(CharacterDataTool.LifeSkillNameArray[i].ToString(), lifeQulificationGo));
                if (i > 0)
                {
                    lifeQulificationGo.transform.localPosition = this.lifeQulificationContainerDict[i - 1].Go.transform.localPosition + this._horizontalOffset;
                }
                else
                {
                    lifeQulificationGo.transform.localPosition = new Vector2(40f, 0f);
                }
            }
            for (int j = 8; j < 16; j++)
            {
                GameObject lifeQulificationGo2 = UIFactory.GetLifeQulificationGo(j);
                lifeQulificationGo2.transform.SetParent(gameObject.transform, false);
                this.lifeQulificationContainerDict.Add(j, new RollAttributeWindow.QulificationContainer(CharacterDataTool.LifeSkillNameArray[j].ToString(), lifeQulificationGo2));
                bool flag2 = j > 8;
                if (flag2)
                {
                    lifeQulificationGo2.transform.localPosition = this.lifeQulificationContainerDict[j - 1].Go.transform.localPosition + this._horizontalOffset;
                }
                else
                {
                    lifeQulificationGo2.transform.localPosition = new Vector2(40f, -60f);
                }
            }
            GameObject gameObject2 = this.CreateEmptyContainerGo(newBackgroundContainerGoB, new Vector3(-450f, -70f, 0f));

            GameObject subTitleGo2 = UIFactory.GetSubTitleGo("功法资质");
            subTitleGo2.transform.SetParent(gameObject2.transform, false);
            subTitleGo2.transform.localPosition = new Vector3(0f, 50f, 0f);
            GameObject blankLabelGo2 = UIFactory.GetBlankLabelGo();
            blankLabelGo2.transform.SetParent(gameObject2.transform, false);
            blankLabelGo2.transform.localPosition = new Vector3(120f, 50f, 0f);
            this._combatGrowthLabelGo = blankLabelGo2;
            for (int k = 0; k < 7; k++)
            {
                GameObject combatQulificationGo = UIFactory.GetCombatQulificationGo(k);
                combatQulificationGo.transform.SetParent(gameObject2.transform, false);
                this.combatQulificationContainerDict.Add(k, new RollAttributeWindow.QulificationContainer(CharacterDataTool.CombatSkillNameArray[k].ToString(), combatQulificationGo));
                bool flag3 = k > 0;
                if (flag3)
                {
                    combatQulificationGo.transform.localPosition = this.combatQulificationContainerDict[k - 1].Go.transform.localPosition + this._horizontalOffset;
                }
                else
                {
                    combatQulificationGo.transform.localPosition = new Vector2(40f, 0f);
                }
            }
            for (int l = 7; l < 14; l++)
            {
                GameObject combatQulificationGo2 = UIFactory.GetCombatQulificationGo(l);
                combatQulificationGo2.transform.SetParent(gameObject2.transform, false);
                this.combatQulificationContainerDict.Add(l, new RollAttributeWindow.QulificationContainer(CharacterDataTool.CombatSkillNameArray[l].ToString(), combatQulificationGo2));
                bool flag4 = l > 7;
                if (flag4)
                {
                    combatQulificationGo2.transform.localPosition = this.combatQulificationContainerDict[l - 1].Go.transform.localPosition + this._horizontalOffset;
                }
                else
                {
                    combatQulificationGo2.transform.localPosition = new Vector3(40f, -60f, 0f);
                }
            }
            GameObject newBackgroundContainerGoB2 = UIFactory.GetNewBackgroundContainerGoB();
            newBackgroundContainerGoB2.transform.SetParent(this._maskGo.transform, false);
            newBackgroundContainerGoB2.GetComponent<RectTransform>().sizeDelta = new Vector2(1000f, 300f);
            newBackgroundContainerGoB2.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(1000f, 300f);
            newBackgroundContainerGoB2.transform.localPosition = new Vector3(-250f, -50f, 0f);
            GameObject gameObject3 = this.CreateEmptyContainerGo(newBackgroundContainerGoB2, new Vector3(-450f, 120f, 0f));

            GameObject subTitleGo3 = UIFactory.GetSubTitleGo("人物特性");
            subTitleGo3.transform.SetParent(gameObject3.transform, false);
            subTitleGo3.transform.localPosition = Vector3.zero;
            GameObject totalMedalGo = UIFactory.GetTotalMedalGo();
            totalMedalGo.transform.SetParent(gameObject3.transform, false);
            totalMedalGo.transform.localPosition = new Vector3(360f, 20, 0f);
            RectTransform component = totalMedalGo.GetComponent<RectTransform>();
            this.totalMedal = component;
            GameObject featureScrollGo = UIFactory.GetFeatureScrollGo();
            featureScrollGo.transform.SetParent(gameObject3.transform, false);
            featureScrollGo.transform.localPosition = new Vector3(450f, -170f, 0f);
            InfinityScroll component2 = featureScrollGo.GetComponent<InfinityScroll>();
            CharacterFeatureScroll characterFeatureScroll = new CharacterFeatureScroll(component2, component);
            this.characterFeatureScroll = characterFeatureScroll;
            GameObject newBackgroundContainerGoB3 = UIFactory.GetNewBackgroundContainerGoB();
            newBackgroundContainerGoB3.transform.SetParent(this._maskGo.transform, false);
            newBackgroundContainerGoB3.GetComponent<RectTransform>().sizeDelta = new Vector2(1000f, 150f);
            newBackgroundContainerGoB3.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(1000f, 150f);
            newBackgroundContainerGoB3.transform.localPosition = new Vector3(-250f, -285f, 0f);
            GameObject gameObject4 = this.CreateEmptyContainerGo(newBackgroundContainerGoB3, new Vector3(-450f, 50f, 0f));

            GameObject subTitleGo4 = UIFactory.GetSubTitleGo("古冢遗刻");
            subTitleGo4.transform.SetParent(gameObject4.transform, false);
            subTitleGo4.transform.localPosition = Vector3.zero;
            GameObject skillBookGo = UIFactory.GetSkillBookGo();
            skillBookGo.transform.SetParent(gameObject4.transform, false);
            skillBookGo.transform.localPosition = new Vector3(300f, -60f, 0f);
            this._lifeSkillBookGo = skillBookGo;
            GameObject skillBookGo2 = UIFactory.GetSkillBookGo();
            skillBookGo2.transform.SetParent(gameObject4.transform, false);
            skillBookGo2.transform.localPosition = new Vector3(600f, -60f, 0f);
            this._combatSkillBookGo = skillBookGo2;
            GameObject mainAttributeGo = UIFactory.GetMainAttributeGo();
            mainAttributeGo.transform.SetParent(this._maskGo.transform, false);
            mainAttributeGo.transform.localPosition = new Vector3(750f, 90f, 0f);
            CharacterAttributeDataView component3 = mainAttributeGo.GetComponent<CharacterAttributeDataView>();
            component3.IsTaiwuTeam = true;
            Traverse traverse = Traverse.Create(component3);
            this.characterMajorAttribute = (CharacterMajorAttribute)traverse.Field("_majorAttributeController").GetValue();
            this.characterSecondaryAttribute = (CharacterSecondaryAttribute)traverse.Field("_secondaryAttributeController").GetValue();

            GameObject subTitleGo5 = UIFactory.GetSubTitleGo("主要属性");
            subTitleGo5.transform.SetParent(mainAttributeGo.transform, false);
            subTitleGo5.transform.localPosition = new Vector3(-210f, 430f, 0f);
            this.characterDataController.updateDataEvent += this.UpdateData;

            SetSkillGrowthValue();
        }

        // Token: 0x06000030 RID: 48 RVA: 0x00004F14 File Offset: 0x00003114
        private GameObject CreateEmptyContainerGo(GameObject backGo, Vector3 localPositonValue)
        {
            GameObject gameObject = new GameObject();
            gameObject.transform.SetParent(backGo.transform, false);
            gameObject.transform.localPosition = localPositonValue;
            return gameObject;
        }

        // Token: 0x06000031 RID: 49 RVA: 0x00004F4D File Offset: 0x0000314D
        private void UpdateData()
        {
            this.UpdateRollButtonText();
            this.SetSkillGrowthValue();
            this.SetLifeQualificationValue();
            this.SetCombatQualificationValue();
            this.SetFeatureIdValue();
            this.SetTotalMedalValue();
            this.SetAttributeValue();
            this.SetLifeSkillBookValue();
            this.SetCombatSkillBookValue();
        }

        private void UpdateRollButtonText()
        {
            if (_rollButton != null)
            {
                var rollButtonText = _rollButton.GetComponentInChildren<TextMeshProUGUI>();
                if (rollButtonText != null)
                {
                    if (characterDataController.IsRolling)
                    {
                        rollButtonText.text = "停止摇属性";
                    }
                    else
                    {
                        rollButtonText.text = "开始摇属性";
                    }
                }
            }

            if (_settingsButton != null)
            {
                _settingsButton.SetActive(!characterDataController.IsRolling);
            }

            _rollingCountText.text = "当前已Roll了 <color=yellow>" + characterDataController.RollingCount +"</color> 次";
        }

        // Token: 0x06000032 RID: 50 RVA: 0x00004F88 File Offset: 0x00003188
        private void SetLifeQualificationValue()
        {
            if (this.lifeQulificationContainerDict.Count > 0)
            {
                foreach (int num in this.lifeQulificationContainerDict.Keys)
                {
                    this.lifeQulificationContainerDict[num].SetValue((int)this.characterDataController.characterDataShortDict[CharacterDataType.LifeSkillQualification][num]);
                }
            }
        }

        // Token: 0x06000033 RID: 51 RVA: 0x0000501C File Offset: 0x0000321C
        private void SetCombatQualificationValue()
        {
            bool flag = this.combatQulificationContainerDict.Count == 0;
            if (!flag)
            {
                foreach (int num in this.combatQulificationContainerDict.Keys)
                {
                    this.combatQulificationContainerDict[num].SetValue((int)this.characterDataController.characterDataShortDict[CharacterDataType.CombatSkillQualification][num]);
                }
            }
        }

        // Token: 0x06000034 RID: 52 RVA: 0x000050B0 File Offset: 0x000032B0
        private void SetSkillGrowthValue()
        {
            bool flag = this._combatGrowthLabelGo == null;
            if (!flag)
            {
                bool flag2 = this.characterDataController.characterDataNameDict.ContainsKey(CharacterDataType.CombatSkillGrowthType);
                if (flag2)
                {
                    string text = this.characterDataController.characterDataNameDict[CharacterDataType.LifeSkillGrowthType][0];
                    this._lifeGrowthLabelGo.transform.GetComponent<TextMeshProUGUI>().text = text;
                    string text2 = this.characterDataController.characterDataNameDict[CharacterDataType.CombatSkillGrowthType][0];
                    this._combatGrowthLabelGo.transform.GetComponent<TextMeshProUGUI>().text = text2;
                }
                else
                {
                    this._lifeGrowthLabelGo.transform.GetComponent<TextMeshProUGUI>().text = "无";
                    this._combatGrowthLabelGo.transform.GetComponent<TextMeshProUGUI>().text = "无";
                }
            }
        }

        // Token: 0x06000035 RID: 53 RVA: 0x00005184 File Offset: 0x00003384
        private void SetFeatureIdValue()
        {
            this.characterFeatureScroll.ResetToEmpty();
            bool flag = this.characterDataController.characterDataShortDict.ContainsKey(CharacterDataType.FeatureIds);
            if (flag)
            {
                this.characterFeatureScroll.SetShowFeatureListFromOutside(this.characterDataController.characterDataShortDict[CharacterDataType.FeatureIds]);
            }
            else
            {
                this.characterFeatureScroll.SetShowFeatureListFromOutside(new List<short>
                {
                    1
                });
            }
        }

        // Token: 0x06000036 RID: 54 RVA: 0x000051EC File Offset: 0x000033EC
        private void SetTotalMedalValue()
        {
            bool flag = this.characterDataController.characterDataShortDict.ContainsKey(CharacterDataType.FeatureMedalValue);
            List<short> list;
            if (flag)
            {
                list = this.characterDataController.characterDataShortDict[CharacterDataType.FeatureMedalValue];
            }
            else
            {
                list = new List<short>
                {
                    0,
                    0,
                    0
                };
            }
            short num = list[0];
            short num2 = list[1];
            short num3 = list[2];
            Refers component = this.totalMedal.transform.GetChild(0).GetComponent<Refers>();
            Refers component2 = this.totalMedal.transform.GetChild(1).GetComponent<Refers>();
            Refers component3 = this.totalMedal.transform.GetChild(2).GetComponent<Refers>();
            component.CGet<CImage>("Icon").SetSprite((num > 0) ? "sp_icon_renwutexing_10" : ((num < 0) ? "sp_icon_renwutexing_4" : "sp_icon_renwutexing_7"), false, null);
            component.CGet<TextMeshProUGUI>("Value").text = string.Format(" x{0}", Mathf.Abs((int)num));
            component2.CGet<CImage>("Icon").SetSprite((num2 > 0) ? "sp_icon_renwutexing_9" : ((num2 < 0) ? "sp_icon_renwutexing_3" : "sp_icon_renwutexing_6"), false, null);
            component2.CGet<TextMeshProUGUI>("Value").text = string.Format(" x{0}", Mathf.Abs((int)num2));
            component3.CGet<CImage>("Icon").SetSprite((num3 > 0) ? "sp_icon_renwutexing_11" : ((num3 < 0) ? "sp_icon_renwutexing_5" : "sp_icon_renwutexing_8"), false, null);
            component3.CGet<TextMeshProUGUI>("Value").text = string.Format(" x{0}", Mathf.Abs((int)num3));
        }

        // Token: 0x06000037 RID: 55 RVA: 0x000053AC File Offset: 0x000035AC
        private void SetAttributeValue()
        {
            Traverse traverse = Traverse.Create(this.characterMajorAttribute);
            Traverse traverse2 = Traverse.Create(this.characterSecondaryAttribute);
            AttributeItem[] array = (AttributeItem[])traverse.Field("_mainAttributeItems").GetValue();
            AttributeItem[] array2 = (AttributeItem[])traverse.Field("_atkHitAttributeItems").GetValue();
            AttributeItem[] array3 = (AttributeItem[])traverse.Field("_atkPenetrabilityItems").GetValue();
            AttributeItem[] array4 = (AttributeItem[])traverse.Field("_defHitAttributeItems").GetValue();
            AttributeItem[] array5 = (AttributeItem[])traverse.Field("_defPenetrabilityItems").GetValue();
            AttributeSlider[] array6 = (AttributeSlider[])traverse2.Field("_attributeSliders").GetValue();
            short[] array7;
            short[] array8;
            short[] array9;
            short[] array10;
            short[] array11;
            short[] array12;
            if (this.characterDataController.characterDataShortDict.ContainsKey(CharacterDataType.MainAttribute))
            {
                array7 = this.characterDataController.characterDataShortDict[CharacterDataType.MainAttribute].ToArray();
                array8 = this.characterDataController.characterDataShortDict[CharacterDataType.AtkHitAttribute].ToArray();
                array9 = this.characterDataController.characterDataShortDict[CharacterDataType.DefHitAttribute].ToArray();
                array10 = this.characterDataController.characterDataShortDict[CharacterDataType.AtkPenetrability].ToArray();
                array11 = this.characterDataController.characterDataShortDict[CharacterDataType.DefPenetrability].ToArray();
                array12 = this.characterDataController.characterDataShortDict[CharacterDataType.SecondaryAttribute].ToArray();
            }
            else
            {
                array7 = new short[6];
                array9 = (array8 = new short[4]);
                array11 = (array10 = new short[2]);
                array12 = new short[10];
            }
            for (int i = 0; i < 6; i++)
            {
                array[i].UpdateValue((int)array7[i], 0);
            }
            for (int j = 0; j < 4; j++)
            {
                array2[j].UpdateValue((int)array8[j], 0);
                array4[j].UpdateValue((int)array9[j], 0);
            }
            for (int k = 0; k < 2; k++)
            {
                array3[k].UpdateValue((int)array10[k], 0);
                array5[k].UpdateValue((int)array11[k], 0);
            }
            for (int l = 0; l < 10; l++)
            {
                array6[l].Value = (float)array12[l];
            }
        }

        // Token: 0x06000038 RID: 56 RVA: 0x000055FC File Offset: 0x000037FC
        private void SetLifeSkillBookValue()
        {
            bool flag = this._lifeSkillBookGo == null;
            if (!flag)
            {
                bool flag2 = this.characterDataController.characterDataNameDict.ContainsKey(CharacterDataType.LifeSkillBookName);
                if (flag2)
                {
                    string text = "<color=yellow>" + this.characterDataController.characterDataNameDict[CharacterDataType.LifeSkillBookName][0] + "</color>";
                    string text2 = this.characterDataController.characterDataNameDict[CharacterDataType.LifeSkillBookType][0];

                    this._lifeSkillBookGo.transform.GetComponent<TextMeshProUGUI>().text = text + "\n" + text2;
                }
                else
                {
                    this._lifeSkillBookGo.transform.GetComponent<TextMeshProUGUI>().text = "无";
                }
            }
        }

        // Token: 0x06000039 RID: 57 RVA: 0x000056F8 File Offset: 0x000038F8
        private void SetCombatSkillBookValue()
        {
            bool flag = this._combatSkillBookGo == null;
            if (!flag)
            {
                bool flag2 = this.characterDataController.characterDataNameDict.ContainsKey(CharacterDataType.CombatSkillBookName);
                if (flag2)
                {
                    string text = "<color=yellow>" + this.characterDataController.characterDataNameDict[CharacterDataType.CombatSkillBookName][0] + "</color>";
                    string text2 = this.characterDataController.characterDataNameDict[CharacterDataType.CombatSkillBookPageType][0];
                    string text3 = text2;
                    for (int i = 1; i < 6; i++)
                    {
                        string str = (this.characterDataController.characterDataNameDict[CharacterDataType.CombatSkillBookPageType][i] == "正") ? "<color=#00ffffff>正</color>" : "<color=#ff0000ff>逆</color>";
                        text3 = text3 + " " + str;
                    }
                    this._combatSkillBookGo.transform.GetComponent<TextMeshProUGUI>().text = text + "\n" + text3;
                }
                else
                {
                    this._combatSkillBookGo.transform.GetComponent<TextMeshProUGUI>().text = "无";
                }
            }
        }

        private void ShowConfigPanel()
        {
            var element = UI_RollFilter.GetUI();
            ArgumentBox box = EasyPool.Get<ArgumentBox>();
            element.SetOnInitArgs(box);
            UIManager.Instance.ShowUI(element);
        }

        // Token: 0x0600003A RID: 58 RVA: 0x00005858 File Offset: 0x00003A58
        private void ReSetPosition(float hight)
        {
            this._currentPostion = new Vector2(this._initialPostion.x, this._currentPostion.y) - new Vector2(0f, hight) - new Vector2(0f, this._gameObjectMargin.y);
        }

        // Token: 0x0600003B RID: 59 RVA: 0x000058B1 File Offset: 0x00003AB1
        public void Open()
        {
            Debug.Log("Open RollAttributeWindow");
            this._maskGo.SetActive(true);
            if (characterDataController.protagonistCreationInfo == null)
            {
                characterDataController.DoRollCharacterData();
            }
            else 
            {
                this.UpdateData();
            }
        }

        // Token: 0x0600003C RID: 60 RVA: 0x000058C8 File Offset: 0x00003AC8
        public void Close()
        {
            Debug.Log("Close RollAttributeWindow");

            this.characterDataController.StopRollCharacterData();
            this._maskGo.SetActive(false);
        }

        // Token: 0x0600003D RID: 61 RVA: 0x000058D8 File Offset: 0x00003AD8
        public void SetRootCanvas(Canvas canvas)
        {
            this._rootCanvas = canvas;
            this._layer = canvas.gameObject;
        }

        // Token: 0x0400002F RID: 47
        public CharacterDataController characterDataController = new CharacterDataController();

        // Token: 0x04000030 RID: 48
        private CharacterFeatureScroll characterFeatureScroll;

        // Token: 0x04000031 RID: 49
        private RectTransform totalMedal;

        // Token: 0x04000032 RID: 50
        private CharacterMajorAttribute characterMajorAttribute;

        // Token: 0x04000033 RID: 51
        private CharacterSecondaryAttribute characterSecondaryAttribute;

        // Token: 0x04000034 RID: 52
        private Dictionary<int, RollAttributeWindow.QulificationContainer> lifeQulificationContainerDict = new Dictionary<int, RollAttributeWindow.QulificationContainer>();

        // Token: 0x04000035 RID: 53
        private Dictionary<int, RollAttributeWindow.QulificationContainer> combatQulificationContainerDict = new Dictionary<int, RollAttributeWindow.QulificationContainer>();

        // Token: 0x04000036 RID: 54
        private Vector3 _horizontalOffset = new Vector3(115f, 0f, 0f);

        // Token: 0x04000037 RID: 55
        private float _titleUnderSpace = 40f;

        // Token: 0x04000038 RID: 56
        private Vector2 _backgroundOffset = new Vector2(100f, 100f);

        // Token: 0x04000039 RID: 57
        private Vector2 _windowPadding = new Vector2(40f, 40f) + new Vector2(60f, 0f);

        // Token: 0x0400003A RID: 58
        private Vector2 _gameObjectMargin = new Vector2(20f, 20f);

        // Token: 0x0400003B RID: 59
        private Vector2 _windowSize = new Vector2(1800f, 1100f);

        // Token: 0x0400003C RID: 60
        private Vector2 _initialPostion;

        // Token: 0x0400003D RID: 61
        private Vector2 _currentPostion;

        // Token: 0x0400003E RID: 62
        private string _title = "随机人物属性";

        // Token: 0x0400003F RID: 63
        private Canvas _rootCanvas;

        // Token: 0x04000040 RID: 64
        private GameObject _layer;

        // Token: 0x04000041 RID: 65
        private GameObject _maskGo;

        // Token: 0x04000042 RID: 66
        public GameObject _backgroundGo;

        // Token: 0x04000043 RID: 67
        private GameObject _closeButton;

        // Token: 0x04000044 RID: 68
        private GameObject _rollButton;

        private GameObject _settingsButton;

        private TextMeshProUGUI _rollingCountText;

        // Token: 0x04000045 RID: 69
        private GameObject _combatGrowthLabelGo;

        // Token: 0x04000046 RID: 70
        private GameObject _lifeGrowthLabelGo;

        // Token: 0x04000047 RID: 71
        private GameObject _lifeSkillBookGo;

        // Token: 0x04000048 RID: 72
        private GameObject _combatSkillBookGo;

        // Token: 0x0200000D RID: 13
        private class QulificationContainer
        {
            // Token: 0x0600005E RID: 94 RVA: 0x00006C28 File Offset: 0x00004E28
            public QulificationContainer(string nameText, GameObject gameObject)
            {
                this.Go = gameObject;
                this.NameText = nameText;
                this.ValueTextMesh = gameObject.GetComponent<TextMeshProUGUI>();
            }

            // Token: 0x0600005F RID: 95 RVA: 0x00006C7C File Offset: 0x00004E7C
            public void SetColor(Color color)
            {
                this.ValueTextMesh.color = color;
            }

            // Token: 0x06000060 RID: 96 RVA: 0x00006C9C File Offset: 0x00004E9C
            public void SetValue(int value)
            {
                this.ValueTextMesh.text = "<b>" + NameText + " " + value.ToString() + "</b>";
                bool flag = value >= 90;
                if (flag)
                {
                    this.SetColor(Color.red);
                }
                else
                {
                    bool flag2 = value >= 80;
                    if (flag2)
                    {
                        this.SetColor(Color.yellow);
                    }
                    else
                    {
                        bool flag3 = value >= 70;
                        if (flag3)
                        {
                            this.SetColor(new Color(0.8f, 0.2f, 0.8f));
                        }
                        else
                        {
                            bool flag4 = value >= 60;
                            if (flag4)
                            {
                                this.SetColor(new Color(0f, 0.8f, 0.8f));
                            }
                            else
                            {
                                bool flag5 = value >= 40;
                                if (flag5)
                                {
                                    this.SetColor(Color.white);
                                }
                                else
                                {
                                    this.SetColor(new Color(0.6f, 0.6f, 0.6f));
                                }
                            }
                        }
                    }
                }
            }

            // Token: 0x04000054 RID: 84
            public GameObject Go;

            // Token: 0x04000055 RID: 85
            private string NameText;

            // Token: 0x04000056 RID: 86
            public TextMeshProUGUI ValueTextMesh;
        }
    }
}
