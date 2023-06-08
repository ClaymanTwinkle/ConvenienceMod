using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Config;
using GameData.Domains.Character.AvatarSystem;
using GameData.Domains.Character.Creation;
using GameData.Domains.Item;
using GameData.GameDataBridge;
using HarmonyLib;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

namespace ConvenienceFrontend.QuicklyCreateCharacter
{
    public class CharacterDataController : MonoBehaviour
    {

        public event Action updateDataEvent;

        // Token: 0x06000014 RID: 20 RVA: 0x00002E10 File Offset: 0x00001010
        public void DoRollCharacterData()
        {
            this.UpdateTheCreationInfo();
            bool bool_IsGetCharacterData = this._bool_IsGetCharacterData;
            if (bool_IsGetCharacterData)
            {
                this.SendTheCreationInfo();
                base.Invoke("GetCharacterData", 0.5f);
            }
        }

        // Token: 0x06000015 RID: 21 RVA: 0x00002E4C File Offset: 0x0000104C
        public void UpdateTheCreationInfo()
        {
            Refers refers = this.UI_NewGame_Member.CGet<Refers>("SettingView");
            Refers refers2 = this.UI_NewGame_Member.CGet<Refers>("NameView");
            Refers refers3 = this.UI_NewGame_Member.CGet<Refers>("FaceView");
            Refers refers4 = this.UI_NewGame_Member.CGet<Refers>("HomeView");
            string text = this.UI_NewGame_Member.CGet<Refers>("NameView").CGet<TextMeshProUGUI>("Surname").text;
            string text2 = this.UI_NewGame_Member.CGet<Refers>("NameView").CGet<TextMeshProUGUI>("Name").text;
            ProtagonistCreationInfo protagonistCreationInfo = new ProtagonistCreationInfo();
            protagonistCreationInfo.Surname = text;
            protagonistCreationInfo.GivenName = text2;
            protagonistCreationInfo.Morality = (short)refers2.CGet<CSlider>("GoodnessSlider").value;
            protagonistCreationInfo.Gender = (sbyte)Traverse.Create(this.UI_NewGame_Member).Field("_gender").GetValue();
            protagonistCreationInfo.Age = (short)refers3.CGet<CSlider>("AgeSlider").value;
            protagonistCreationInfo.BirthMonth = (sbyte)refers3.CGet<CSlider>("BirthdaySlider").value;
            protagonistCreationInfo.Avatar = (AvatarData)Traverse.Create(this.UI_NewGame_Member).Field("_avatarData").GetValue();
            protagonistCreationInfo.Avatar.FormatDisabledElements();
            List<ProtagonistFeatureItem> list = (List<ProtagonistFeatureItem>)Traverse.Create(this.UI_NewGame_Member).Field("_selectedAbilities").GetValue();
            protagonistCreationInfo.ProtagonistFeatureIds = list.ConvertAll<short>((ProtagonistFeatureItem e) => e.TemplateId);
            protagonistCreationInfo.TaiwuVillageStateTemplateId = (sbyte)refers4.CGet<CToggleGroup>("MapCells").GetActive().Key;
            protagonistCreationInfo.InscribedChar = null;
            protagonistCreationInfo.ClothingTemplateId = ItemTemplateHelper.GetClothingTemplateIdByDisplayId((byte)protagonistCreationInfo.Avatar.ClothDisplayId);
            this.protagonistCreationInfo = protagonistCreationInfo;
        }

        // Token: 0x06000016 RID: 22 RVA: 0x0000302E File Offset: 0x0000122E
        public void SendTheCreationInfo()
        {
            GameDataBridge.AddMethodCall<ProtagonistCreationInfo>(-1, 4, 0, this.protagonistCreationInfo);
            this._bool_IsSendCreationInfo = true;
            this._bool_IsGetCharacterData = false;
        }

        // Token: 0x06000017 RID: 23 RVA: 0x00003050 File Offset: 0x00001250
        public void GetCharacterData()
        {
            string @string;
            using (MemoryMappedFile memoryMappedFile = MemoryMappedFile.OpenExisting("QuicklyCreateCharacterData"))
            {
                using (MemoryMappedViewAccessor memoryMappedViewAccessor = memoryMappedFile.CreateViewAccessor())
                {
                    byte[] array = new byte[memoryMappedViewAccessor.Capacity];
                    memoryMappedViewAccessor.ReadArray<byte>(0L, array, 0, array.Length);
                    @string = Encoding.Unicode.GetString(array);
                }
            }
            this.characterDataList = JsonConvert.DeserializeObject<List<string>>(@string);
            this.characterDataDict = CharacterDataTool.CharacterDataListToDataDict(this.characterDataList);
            this.characterDataColorDict = CharacterDataTool.CharacterDataDictToColorDict(this.characterDataDict);
            this.characterDataNameDict = CharacterDataTool.CharacterDataDictToNameDict(this.characterDataDict);
            this.characterDataShortDict = CharacterDataTool.CharacterDataDictToShortDataDict(this.characterDataDict);
            this._bool_IsGetCharacterData = true;
            this.updateDataEvent();
        }

        // Token: 0x06000018 RID: 24 RVA: 0x00003134 File Offset: 0x00001334
        public void DoUpdate()
        {
            bool flag = this.updateDataEvent != null;
            if (flag)
            {
                this.updateDataEvent();
            }
        }

        // Token: 0x0400000E RID: 14
        public UI_NewGame UI_NewGame_Member;

        // Token: 0x0400000F RID: 15
        public ProtagonistCreationInfo protagonistCreationInfo = new ProtagonistCreationInfo();

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

        // Token: 0x04000015 RID: 21
        private bool _bool_IsSendCreationInfo = false;

        // Token: 0x04000016 RID: 22
        private bool _bool_IsGetCharacterData = true;
    }
}
