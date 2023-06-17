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
using static UnityEngine.GUI;

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
        public void DoRollCharacterData(bool clearCache = true)
        {
            UpdateTheCreationInfo();
            bool bool_IsGetCharacterData = this._bool_IsGetCharacterData;
            if (bool_IsGetCharacterData)
            {
                this.SendTheCreationInfo(clearCache);
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
        private void SendTheCreationInfo(bool clearCache = true)
        {
            if (_listenerId == -1)
            {
                _listenerId = GameDataBridge.RegisterListener(OnNotifyGameData);
            }
            if (clearCache)
            {
                RollingCount = 0;
            }
            this._bool_IsGetCharacterData = false;
            GameDataBridge.AddMethodCall<short, KeyValuePair<bool, ProtagonistCreationInfo>>(_listenerId, 4, GameDataBridgeConst.MethodId, GameDataBridgeConst.Flag.Flag_RollCharacter, new KeyValuePair<bool, ProtagonistCreationInfo>(clearCache, this.protagonistCreationInfo));
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
                        System.Object keyValuePair = default;
                        Serializer.DeserializeDefault(notification2.DataPool, notification.ValueOffset, ref keyValuePair);
                        if (keyValuePair is KeyValuePair<int, KeyValuePair<bool, string>>)
                        {

                            KeyValuePair<bool, string> jsonKeyValuePair = ((KeyValuePair<int, KeyValuePair<bool, string>>)keyValuePair).Value;

                            _bool_IsGetCharacterData = true;

                            if (jsonKeyValuePair.Key)
                            {
                                IsRolling = false;
                            }
                            RefreshUIData(jsonKeyValuePair.Value);
                            if (IsRolling)
                            {
                                DoRollCharacterData(false);
                            }
                        }
                        else if (keyValuePair is KeyValuePair<int, bool>)
                        {
                        }
                    }
                }
            }
        }

        private void RefreshUIData(string json)
        {
            RollingCount++;
            characterDataList = JsonConvert.DeserializeObject<List<string>>(json);
            characterDataDict = CharacterDataTool.CharacterDataListToDataDict(characterDataList);
            characterDataColorDict = CharacterDataTool.CharacterDataDictToColorDict(characterDataDict);
            characterDataNameDict = CharacterDataTool.CharacterDataDictToNameDict(characterDataDict);
            characterDataShortDict = CharacterDataTool.CharacterDataDictToShortDataDict(characterDataDict);
            DoUpdate();
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

        public int RollingCount = 0;
    }
}
