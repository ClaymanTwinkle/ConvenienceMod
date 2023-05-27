using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.Item;
using GameData.GameDataBridge;
using GameData.Serializer;
using GameData.Utilities;
using HarmonyLib;

namespace ConvenienceBackend.CustomWeapon
{
    internal class CustomWeaponBackendPatch: BaseBackendPatch
    {
        public override void OnModSettingUpdate(string ModIdStr)
        {
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ItemDomain), "CallMethod")]
        public static bool CallMethodItemPatch(Operation operation, RawDataPool argDataPool, RawDataPool returnDataPool, DataContext context, ItemDomain __instance, int __result)
        {
            int num = operation.ArgsOffset;
            ItemDomain item = DomainManager.Item;
            if (operation.MethodId == 160)
            {
                if (operation.ArgsCount == 2)
                {
                    ItemKey itemKey = default(ItemKey);
                    num += Serializer.Deserialize(argDataPool, num, ref itemKey);
                    List<String> tricks = null;
                    num += Serializer.Deserialize(argDataPool, num, ref tricks);

                    var trickIdList = new List<sbyte>();
                    foreach (var trick in tricks) {
                        trickIdList.Add(Config.TrickType.Instance[trick].TemplateId);
                    }
                    
                    GameData.Domains.Item.Weapon element_Weapons = item.GetElement_Weapons(itemKey.Id);
                    element_Weapons.SetTricks(trickIdList, context);
                    __result = Serializer.Serialize(element_Weapons.GetTricks(), returnDataPool);
                    return false;
                }
            }
            return true;
        }
    }
}
