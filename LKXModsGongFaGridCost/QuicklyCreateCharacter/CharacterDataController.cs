using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Config;
using GameData.Domains.Character.AvatarSystem;
using GameData.Domains.Character.Creation;
using GameData.Domains.Item;
using GameData.GameDataBridge;
using GameData.Serializer;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;

namespace ConvenienceFrontend.QuicklyCreateCharacter
{
    public class CharacterDataController : MonoBehaviour
    {

        public event Action updateDataEvent;

        public void StartRollCharacterData()
        { 
            IsRolling = true;
            DoRollCharacterData();
        }

        public void StopRollCharacterData()
        {
            IsRolling = false;
        }

        // Token: 0x06000014 RID: 20 RVA: 0x00002E10 File Offset: 0x00001010
        public void DoRollCharacterData()
        {
            UpdateTheCreationInfo();
            bool bool_IsGetCharacterData = this._bool_IsGetCharacterData;
            if (bool_IsGetCharacterData)
            {
                this.SendTheCreationInfo();
            }
        }

        // Token: 0x06000015 RID: 21 RVA: 0x00002E4C File Offset: 0x0000104C
        public void UpdateTheCreationInfo()
        {
            Refers refers2 = this.UI_NewGame_Member.CGet<Refers>("NameView");
            Refers refers3 = this.UI_NewGame_Member.CGet<Refers>("FaceView");
            Refers refers4 = this.UI_NewGame_Member.CGet<Refers>("HomeView");

            string text = refers2.CGet<TextMeshProUGUI>("Surname")?.text ?? "";
            string text2 = refers2.CGet<TextMeshProUGUI>("Name")?.text ?? "";

            ProtagonistCreationInfo protagonistCreationInfo = new ProtagonistCreationInfo();
            protagonistCreationInfo.Surname = text;
            protagonistCreationInfo.GivenName = text2;
            protagonistCreationInfo.Morality = (short)(refers2.CGet<CSlider>("GoodnessSlider")?.value ?? 0);
            protagonistCreationInfo.Gender = (sbyte)Traverse.Create(this.UI_NewGame_Member).Field("_gender").GetValue();
            protagonistCreationInfo.Age = (short)(refers3.CGet<CSlider>("AgeSlider")?.value ?? 0);
            protagonistCreationInfo.BirthMonth = (sbyte)(refers3.CGet<CSlider>("BirthdaySlider")?.value ?? 0);
            protagonistCreationInfo.Avatar = (AvatarData)Traverse.Create(this.UI_NewGame_Member).Field("_avatarData").GetValue();
            if (protagonistCreationInfo.Avatar != null)
            {
                protagonistCreationInfo.Avatar.FormatDisabledElements();
            }
            List<ProtagonistFeatureItem> list = (List<ProtagonistFeatureItem>)Traverse.Create(this.UI_NewGame_Member).Field("_selectedAbilities").GetValue();
            
            protagonistCreationInfo.ProtagonistFeatureIds = list.ConvertAll<short>((ProtagonistFeatureItem e) => e.TemplateId);
            protagonistCreationInfo.TaiwuVillageStateTemplateId = (sbyte)(refers4.CGet<CToggleGroup>("MapCells")?.GetActive()?.Key ?? 0);
            
            protagonistCreationInfo.InscribedChar = null;
            protagonistCreationInfo.ClothingTemplateId = ItemTemplateHelper.GetClothingTemplateIdByDisplayId((byte)protagonistCreationInfo.Avatar.ClothDisplayId);
            this.protagonistCreationInfo = protagonistCreationInfo;
        }

        // Token: 0x06000016 RID: 22 RVA: 0x0000302E File Offset: 0x0000122E
        private void SendTheCreationInfo()
        {
            if (_listenerId == -1)
            {
                _listenerId = GameDataBridge.RegisterListener(OnNotifyGameData);
            }
            GameDataBridge.AddMethodCall<short, ProtagonistCreationInfo>(_listenerId, 4, GameDataBridgeConst.MethodId, GameDataBridgeConst.Flag.Flag_RollCharacter,  this.protagonistCreationInfo);
            this._bool_IsGetCharacterData = false;
        }

        private void OnNotifyGameData(List<NotificationWrapper> notifications)
        {
            foreach (NotificationWrapper notification2 in notifications)
            {
                Notification notification = notification2.Notification;
                if (notification.Type == 1)
                {
                    if (notification.DomainId == 4 && notification.MethodId == GameDataBridgeConst.MethodId)
                    {
                        string json = "{}";
                        Serializer.Deserialize(notification2.DataPool, notification.ValueOffset, ref json);
                        GetCharacterData(json);
                    }
                }
            }
        }

        // Token: 0x06000017 RID: 23 RVA: 0x00003050 File Offset: 0x00001250
        private void GetCharacterData(string json)
        {
            this.characterDataList = JsonConvert.DeserializeObject<List<string>>(json);
            this.characterDataDict = CharacterDataTool.CharacterDataListToDataDict(this.characterDataList);
            this.characterDataColorDict = CharacterDataTool.CharacterDataDictToColorDict(this.characterDataDict);
            this.characterDataNameDict = CharacterDataTool.CharacterDataDictToNameDict(this.characterDataDict);
            this.characterDataShortDict = CharacterDataTool.CharacterDataDictToShortDataDict(this.characterDataDict);
            this._bool_IsGetCharacterData = true;
            CheckContinueRollData();
            this.updateDataEvent();

            if (IsRolling)
            {
                DoRollCharacterData();
            }
        }

        public void CheckContinueRollData()
        {
            // 全蓝特性
            var enableAllBlue = ConvenienceFrontend.Config.GetTypedValue<bool>("Toggle_FilterEnableAllBlueFeature");
            if (enableAllBlue)
            {
                var featureIds = characterDataDict[CharacterDataType.FeatureIds];
                foreach (var featureId in featureIds)
                {
                    int id = int.Parse(featureId);
                    CharacterFeatureItem characterFeatureItem = CharacterFeature.Instance[id];
                    if (characterFeatureItem.CandidateGroupId == 1)
                    {
                        return;
                    }
                }
            }
            // 特性
            string[] filterFeatureList = (ConvenienceFrontend.Config.GetTypedValue<string>("InputField_FilterAllFeatures") ?? "").Split(' ');
            if (filterFeatureList.Length > 0)
            {
                var featureNames = characterDataDict[CharacterDataType.FeatureIds].ConvertAll<string>(x => CharacterFeature.Instance[int.Parse(x)].Name);
                foreach (var filterFeature in filterFeatureList)
                {
                    if (!filterFeature.Trim().IsNullOrEmpty() && !featureNames.Contains(filterFeature))
                    {
                        return;
                    }
                }
            }

            // 技艺成长
            JArray lifeSkillGrowthTypeJArray = (JArray)ConvenienceFrontend.Config.GetTypedValue<JArray>("ToggleGroup_FilterLifeSkillQualificationGrowthType") ?? new JArray();
            var lifeSkillGrowthType = int.Parse(characterDataDict[CharacterDataType.LifeSkillGrowthType][0]);
            if (lifeSkillGrowthTypeJArray.Count > lifeSkillGrowthType)
            {
                if (!(bool)lifeSkillGrowthTypeJArray[lifeSkillGrowthType])
                {
                    foreach (bool item in lifeSkillGrowthTypeJArray)
                    {
                        if (item)
                        {
                            return;
                        }
                    }
                }
            }
            // 功法成长
            JArray combatSkillGrowthTypeJArray = (JArray)ConvenienceFrontend.Config.GetTypedValue<JArray>("ToggleGroup_FilterCombatSkillQualificationGrowthType") ?? new JArray();
            var combatSkillGrowthType = int.Parse(characterDataDict[CharacterDataType.CombatSkillGrowthType][0]);
            if (combatSkillGrowthTypeJArray.Count > combatSkillGrowthType)
            {
                if (!(bool)combatSkillGrowthTypeJArray[combatSkillGrowthType])
                {
                    foreach (bool item in combatSkillGrowthTypeJArray)
                    {
                        if (item)
                        {
                            return;
                        }
                    }
                }
            }

            // 技艺资质
            if (this.characterDataShortDict.ContainsKey(CharacterDataType.LifeSkillQualification))
            {
                JArray lifeSkillQualificationsTypesJArray = (JArray)ConvenienceFrontend.Config.GetTypedValue<JArray>("ToggleGroup_FilterLifeSkillQualificationsTypes") ?? new JArray();
                if (lifeSkillQualificationsTypesJArray.Count > 0)
                {
                    var lifeSkillQualificationsValue = (float)ConvenienceFrontend.Config.GetTypedValue<Double>("SliderBar_LifeSkillQualificationsValue");
                    var lifeSkillQualificationData = characterDataShortDict[CharacterDataType.LifeSkillQualification];
                    for (int i = 0; i < lifeSkillQualificationData.Count; i++)
                    {
                        if ((bool)lifeSkillQualificationsTypesJArray[i])
                        {
                            if (lifeSkillQualificationData[i] < lifeSkillQualificationsValue) return;
                        }
                    }
                }
            }

            //功法资质
            if (this.characterDataShortDict.ContainsKey(CharacterDataType.CombatSkillQualification))
            {
                JArray combatSkillQualificationsTypesJArray = (JArray)ConvenienceFrontend.Config.GetTypedValue<JArray>("ToggleGroup_FilterCombatSkillQualificationsTypes") ?? new JArray();
                if (combatSkillQualificationsTypesJArray.Count > 0)
                {
                    var combatSkillQualificationsValue = (float)ConvenienceFrontend.Config.GetTypedValue<Double>("SliderBar_FilterCombatSkillQualificationsValue");
                    var combatSkillQualificationData = characterDataShortDict[CharacterDataType.CombatSkillQualification];
                    for (int i = 0; i < combatSkillQualificationData.Count; i++)
                    {
                        if ((bool)combatSkillQualificationsTypesJArray[i])
                        {
                            if (combatSkillQualificationData[i] < combatSkillQualificationsValue) return;
                        }
                    }
                }
            }

            // 主要属性
            if (this.characterDataShortDict.ContainsKey(CharacterDataType.MainAttribute))
            {
                JArray mainAttributeTypesJArray = (JArray)ConvenienceFrontend.Config.GetTypedValue<JArray>("ToggleGroup_FilterMainAttributeTypes") ?? new JArray();
                if (mainAttributeTypesJArray.Count > 0)
                {
                    var minMainAttributeValue = (float)ConvenienceFrontend.Config.GetTypedValue<Double>("SliderBar_FilterMainAttributeValue");
                    var mainAttributeData = this.characterDataShortDict[CharacterDataType.MainAttribute];
                    for (int i = 0; i < mainAttributeData.Count; i++)
                    {
                        if ((bool)mainAttributeTypesJArray[i])
                        {
                            if (mainAttributeData[i] < minMainAttributeValue) return;
                        }
                    }
                }
            }

            // 古冢遗刻-技艺书
            if (characterDataNameDict.ContainsKey(CharacterDataType.LifeSkillBookType))
            {
                JArray lifeSkillBookTypeJArray = (JArray)ConvenienceFrontend.Config.GetTypedValue<JArray>("ToggleGroup_FilterLifeSkillBookName") ?? new JArray();
                if (lifeSkillBookTypeJArray.Count > 0)
                {
                    var index = CharacterDataTool.LifeSkillNameArray.IndexOf(this.characterDataNameDict[CharacterDataType.LifeSkillBookType][0]);
                    if (index > -1)
                    {
                        if (!(bool)lifeSkillBookTypeJArray[index])
                        {
                            foreach (bool item in lifeSkillBookTypeJArray)
                            {
                                if (item)
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
            }


            // 古冢遗刻-功法书
            if (characterDataNameDict.ContainsKey(CharacterDataType.CombatSkillBookName))
            {
                // 书名
                var filterCombatSkillBookName = ConvenienceFrontend.Config.GetTypedValue<string>("InputField_FilterCombatSkillBookName");
                if (!filterCombatSkillBookName.IsNullOrEmpty())
                {
                    if (!filterCombatSkillBookName.Trim().IsNullOrEmpty())
                    {
                        var combatSkillBookName = characterDataNameDict[CharacterDataType.CombatSkillBookName][0];
                        if (!combatSkillBookName.IsNullOrEmpty())
                        {
                            if (!combatSkillBookName.Contains(filterCombatSkillBookName))
                            {
                                return;
                            }
                        }
                    }
                }
                // 正逆练
                var filterDirectAndReverse = ConvenienceFrontend.Config.GetTypedValue<string>("InputField_FilterDirectAndReverse");
                if (filterDirectAndReverse != null && filterDirectAndReverse.Length == 5)
                {
                    if (!string.Join("", this.characterDataNameDict[CharacterDataType.CombatSkillBookPageType]).Contains(filterDirectAndReverse))
                    {
                        return;
                    }
                }
                // 总纲
                JArray generalPrinciplesJArray = (JArray)ConvenienceFrontend.Config.GetTypedValue<JArray>("ToggleGroup_FilterGeneralPrinciples") ?? new JArray();
                if (generalPrinciplesJArray.Count > 0)
                {
                    var index = CharacterDataTool.GeneralPrinciplesNameArray.IndexOf(this.characterDataNameDict[CharacterDataType.CombatSkillBookPageType][0]);
                    if (index > -1)
                    {
                        if (!(bool)generalPrinciplesJArray[index])
                        {
                            foreach (bool item in generalPrinciplesJArray)
                            {
                                if (item)
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
            } 

            IsRolling = false;
        }

        // Token: 0x06000018 RID: 24 RVA: 0x00003134 File Offset: 0x00001334
        public void DoUpdate()
        {
            if (this.updateDataEvent != null)
            {
                this.updateDataEvent();
            }
        }

        // Token: 0x0400000E RID: 14
        public UI_NewGame UI_NewGame_Member;

        // Token: 0x0400000F RID: 15
        public ProtagonistCreationInfo protagonistCreationInfo;

        // Token: 0x04000010 RID: 16
        public List<string> characterDataList = new List<string>();

        // Token: 0x04000011 RID: 17
        public Dictionary<CharacterDataType, List<string>> characterDataDict = new Dictionary<CharacterDataType, List<string>>();

        // Token: 0x04000012 RID: 18
        public Dictionary<CharacterDataType, List<short>> characterDataShortDict = new Dictionary<CharacterDataType, List<short>>();

        // Token: 0x04000013 RID: 19
        public Dictionary<CharacterDataType, List<string>> characterDataNameDict = new Dictionary<CharacterDataType, List<string>>();

        // Token: 0x04000014 RID: 20
        public Dictionary<CharacterDataType, List<string>> characterDataColorDict = new Dictionary<CharacterDataType, List<string>>();

        // Token: 0x04000016 RID: 22
        private bool _bool_IsGetCharacterData = true;

        private int _listenerId = -1;
    
        public bool IsRolling = false;
    }
}
