using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CharacterDataMonitor;
using Config;
using FrameWork;
using GameData.Domains.Taiwu;
using TMPro;
using UnityEngine;

namespace ConvenienceFrontend.MergeBookPanel
{
    public class UI_Library : UIBase
    {
        // Token: 0x17000001 RID: 1
        // (get) Token: 0x0600002D RID: 45 RVA: 0x00004A0E File Offset: 0x00002C0E
        public UI_CharacterMenu CharacterMenu
        {
            get
            {
                return UIElement.CharacterMenu.UiBaseAs<UI_CharacterMenu>();
            }
        }

        // Token: 0x0600002E RID: 46 RVA: 0x00004A1A File Offset: 0x00002C1A
        public override void OnInit(ArgumentBox argsBox)
        {
        }

        // Token: 0x0600002F RID: 47 RVA: 0x00004A1C File Offset: 0x00002C1C
        private void Awake()
        {
            GameLog.LogMessage("OnAwake");
            Component lifSkillMenu = base.gameObject.GetComponentInParent(typeof(UI_CharacterMenuLifeSkill));
            GameLog.LogMessage(string.Format("lifSkillMenu: {0}", lifSkillMenu));
            this._dataMonitor = lifSkillMenu.GetFieldValue<LifeSkillMonitor>("_dataMonitor");
            this._taiwuLifeSkills = lifSkillMenu.GetFieldValue<Dictionary<short, TaiwuLifeSkill>>("_taiwuLifeSkills");
            this._taiwuNotLearnLifeSkills = lifSkillMenu.GetFieldValue<Dictionary<short, TaiwuLifeSkill>>("_taiwuNotLearnLifeSkills");
            this.skillHolder = base.CGet<RectTransform>("SkillHolder");
            this.detailTypeTogGroup = base.CGet<CToggleGroup>("SkillTypeTogGroup");
            this.InitToggleGroups();
        }

        // Token: 0x06000030 RID: 48 RVA: 0x00004AB4 File Offset: 0x00002CB4
        private void OnEnable()
        {
            GameLog.LogMessage("Libaray OnEnable");
            GameLog.LogMessage(string.Format("_dataMonitor: {0}", this._dataMonitor));
            GameLog.LogMessage(string.Format("_taiwuLifeSkills: {0}", this._taiwuLifeSkills));
            GameLog.LogMessage(string.Format("_taiwuNotLearnLifeSkills: {0}", this._taiwuNotLearnLifeSkills));
        }

        // Token: 0x06000031 RID: 49 RVA: 0x00004B0C File Offset: 0x00002D0C
        public void Clone(Refers refers)
        {
            this.Objects = refers.Objects;
            this.Names = refers.Names;
            this.UserFloat = refers.UserFloat;
            this.UserInt = refers.UserInt;
            this.UserString = refers.UserString;
            this.UserObject = refers.UserObject;
        }

        // Token: 0x06000032 RID: 50 RVA: 0x00004B64 File Offset: 0x00002D64
        private void InitToggleGroups()
        {
            this.detailTypeTogGroup.AllowSwitchOff = false;
            this.detailTypeTogGroup.AllowUncheck = false;
            this.detailTypeTogGroup.AddAllChildToggles();
            this.detailTypeTogGroup.InitPreOnToggle();
            this.detailTypeTogGroup.OnActiveToggleChange = delegate (CToggle togNew, CToggle togOld)
            {
                GameLog.LogMessage(string.Format("switch to {0}", togNew.Key));
                this.OnSkillTypeTogChange(togNew, togOld);
            };
            this.detailTypeTogGroup.Set(0, true, true);
            for (int type = 0; type < 16; type++)
            {
                string skillTypeName = Config.LifeSkillType.Instance[type].Name;
                this.detailTypeTogGroup.Get(type).GetComponent<Refers>().CGet<TextMeshProUGUI>("Label").text = skillTypeName;
                this.detailTypeTogGroup.Get(type).GetComponent<Refers>().CGet<TextMeshProUGUI>("BookCount").gameObject.SetActive(false);
            }
        }

        // Token: 0x06000033 RID: 51 RVA: 0x00004C2C File Offset: 0x00002E2C
        private void OnSkillTypeTogChange(CToggle togNew, CToggle togOld)
        {
            sbyte type = (sbyte)togNew.Key;
            short[] skillIdList = Config.LifeSkillType.Instance[type].skillList;
            GameLog.LogMessage(string.Format("skill type: {0}", type));
            sbyte[] readingProgress = new sbyte[5];
            for (int bookGrade = 0; bookGrade < skillIdList.Length; bookGrade++)
            {
                short skillTemplateId = skillIdList[bookGrade];
                Config.LifeSkillItem configData = LifeSkill.Instance[skillTemplateId];
                SkillBookItem bookConfig = SkillBook.Instance[configData.SkillBookId];
                int index = this._dataMonitor.LearnedLifeSkills.FindIndex((GameData.Domains.Character.LifeSkillItem skillItem) => skillItem.SkillTemplateId == skillTemplateId);
                GameData.Domains.Character.LifeSkillItem skillData = (index >= 0) ? this._dataMonitor.LearnedLifeSkills[index] : default(GameData.Domains.Character.LifeSkillItem);
                Refers skillRefers = this.skillHolder.GetChild(bookGrade).GetComponent<Refers>();
                RectTransform pageHolder = skillRefers.CGet<RectTransform>("PageHolder");
                skillRefers.CGet<TextMeshProUGUI>("Name").text = bookConfig.Name;
                for (int i = 0; i < 5; i++)
                {
                    if (this._taiwuLifeSkills.ContainsKey(skillTemplateId))
                    {
                        readingProgress[i] = this._taiwuLifeSkills[skillTemplateId].GetBookPageReadingProgress((byte)i);
                    }
                    else if (this._taiwuNotLearnLifeSkills.ContainsKey(skillTemplateId))
                    {
                        readingProgress[i] = this._taiwuNotLearnLifeSkills[skillTemplateId].GetBookPageReadingProgress((byte)i);
                    }
                    else
                    {
                        readingProgress[i] = ((sbyte)((index >= 0 && skillData.IsPageRead((byte)i)) ? 100 : 0));
                    }
                }
                skillRefers.gameObject.SetActive(true);
                for (int page = 0; page < 5; page++)
                {
                    pageHolder.GetChild(page).GetComponent<Refers>().CGet<GameObject>("NotRead").SetActive(readingProgress[page] <= 0);
                }
            }
        }

        // Token: 0x04000029 RID: 41
        public RectTransform skillHolder;

        // Token: 0x0400002A RID: 42
        public CToggleGroup detailTypeTogGroup;

        // Token: 0x0400002B RID: 43
        private LifeSkillMonitor _dataMonitor;

        // Token: 0x0400002C RID: 44
        private Dictionary<short, TaiwuLifeSkill> _taiwuLifeSkills;

        // Token: 0x0400002D RID: 45
        private Dictionary<short, TaiwuLifeSkill> _taiwuNotLearnLifeSkills;
    }
}
