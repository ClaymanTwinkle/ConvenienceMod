using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData.Domains.Character;
using GameData.Domains.Item;
using GameData.Domains;

namespace ConvenienceBackend.QuicklyCreateCharacter
{
    internal class TempIteamData
    {
        // Token: 0x06000013 RID: 19 RVA: 0x00003070 File Offset: 0x00001270
        public TempIteamData(Inventory inventory)
        {
            if (inventory == null) return;

            foreach (ItemKey itemKey in inventory.Items.Keys)
            {
                ItemBase baseItem = DomainManager.Item.GetBaseItem(itemKey);
                if (baseItem.GetItemSubType() == 1000)
                {
                    this.lifeSkillBook = (SkillBook)baseItem;
                }
                if (baseItem.GetItemSubType() == 1001)
                {
                    this.combatSkillBook = (SkillBook)baseItem;
                }
                this.itemList.Add(baseItem);
            }
            this.Count = this.itemList.Count;
            if (this.combatSkillBook != null)
            {
                this.combatSkillBookPageTypes = new List<int>();
                byte pageTypes = this.combatSkillBook.GetPageTypes();
                BitArray bitArray = new BitArray((int)pageTypes);
                this.combatSkillBookPageTypes.Add(((pageTypes & 1) == 1) ? 1 : 0);
                this.combatSkillBookPageTypes.Add(((pageTypes & 2) == 2) ? 1 : 0);
                this.combatSkillBookPageTypes.Add(((pageTypes & 4) == 4) ? 1 : 0);
                this.combatSkillBookPageTypes.Add(((pageTypes & 8) == 8) ? 1 : 0);
                this.combatSkillBookPageTypes.Add(((pageTypes & 16) == 16) ? 1 : 0);
                this.combatSkillBookPageTypes.Add(((pageTypes & 32) == 32) ? 1 : 0);
                this.combatSkillBookPageTypes.Add(((pageTypes & 64) == 64) ? 1 : 0);
                this.combatSkillBookPageTypes.Add(((pageTypes & 128) == 128) ? 1 : 0);
            }
        }

        // Token: 0x04000017 RID: 23
        public List<ItemBase> itemList = new List<ItemBase>();

        // Token: 0x04000018 RID: 24
        public SkillBook lifeSkillBook;

        // Token: 0x04000019 RID: 25
        public SkillBook combatSkillBook;

        // Token: 0x0400001A RID: 26
        public List<int> combatSkillBookPageTypes;

        // Token: 0x0400001B RID: 27
        public int Count;
    }
}
