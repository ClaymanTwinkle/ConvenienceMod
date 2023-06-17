using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Config;
using GameData.Domains.Character;
using GameData.Domains.Item;
using GameData.Domains;
using GameData.Utilities;
using HarmonyLib;

namespace ConvenienceBackend.BetterArmor
{
    internal class BetterArmorBackendPatch : BaseBackendPatch
    {
        private static bool _enableMod = false;

        private bool showModification = false;

        public override void Initialize(Harmony harmony, string modIdStr)
        {
            OnModSettingUpdate(modIdStr);
            if (!_enableMod) return;
            base.Initialize(harmony, modIdStr);

            AdaptableLog.Info("更平衡的装备 后端 初始化开始");
            BetterArmor betterArmor = new BetterArmor(this.showModification);
            betterArmor.Modify();
            AdaptableLog.Info("更平衡的装备 后端 初始化结束");
        }

        public override void Dispose()
        {
            AdaptableLog.Info("更平衡的装备 后端 卸载");
        }

        public override void OnModSettingUpdate(string modIdStr)
        {
            DomainManager.Mod.GetSetting(modIdStr, "Toggle_EnableBetterArmor", ref _enableMod);
        }

        /// <summary>
        /// 获取命中因子
        /// </summary>
        /// <param name="__result"></param>
        /// <param name="__instance"></param>
        /// <returns></returns>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameData.Domains.Item.Weapon), "GetHitFactors", new Type[]
        {

        })]
        public static unsafe HitOrAvoidShorts GetHitFactors_Postfix(HitOrAvoidShorts __result, GameData.Domains.Item.Weapon __instance)
        {
            if (!_enableMod) return __result;

            HitOrAvoidShorts baseHitFactors = __instance.GetBaseHitFactors();
            bool flag = ModificationStateHelper.IsActive(__instance.GetModificationState(), 2);
            if (flag)
            {
                RefiningEffects refinedEffects = DomainManager.Item.GetRefinedEffects(__instance.GetItemKey());
                for (int i = 0; i < 4; i++)
                {
                    short ptr = baseHitFactors.Items[i * 2];
                    ptr += (short)((refinedEffects.GetWeaponPropertyBonus(ERefiningEffectWeaponType.HitRateStrength + i) + 9) / 10);
                }
            }
            return baseHitFactors;
        }

        /// <summary>
        /// 获取闪避因子
        /// </summary>
        /// <param name="__result"></param>
        /// <param name="__instance"></param>
        /// <returns></returns>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameData.Domains.Item.Armor), "GetAvoidFactors", new Type[]
        {

        })]
        public static unsafe HitOrAvoidShorts GetAvoidFactors_Postfix(HitOrAvoidShorts __result, GameData.Domains.Item.Armor __instance)
        {
            if (!_enableMod) return __result;

            HitOrAvoidShorts baseAvoidFactors = __instance.GetBaseAvoidFactors();
            bool flag = ModificationStateHelper.IsActive(__instance.GetModificationState(), 2);
            if (flag)
            {
                RefiningEffects refinedEffects = DomainManager.Item.GetRefinedEffects(__instance.GetItemKey());
                for (int i = 0; i < 4; i++)
                {
                    short ptr = baseAvoidFactors.Items[i * 2];
                    ptr += (short)((refinedEffects.GetArmorPropertyBonus(ERefiningEffectArmorType.AvoidRateStrength + i) + 9) / 10);
                }
            }
            return baseAvoidFactors;
        }

        // Token: 0x0600001C RID: 28 RVA: 0x00003F14 File Offset: 0x00002114
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameData.Domains.Item.Armor), "CalcEquipmentAttack")]
        private static short CalcArmorEquipmentAttack_Postfix(short __result, GameData.Domains.Item.Armor __instance)
        {
            if (!_enableMod) return __result;

            int materialResourceBonusValuePercentage = ItemTemplateHelper.GetMaterialResourceBonusValuePercentage(__instance.GetItemType(), __instance.GetTemplateId(), 0, __instance.GetMaterialResources());
            int num = (int)__instance.GetBaseEquipmentAttack() * materialResourceBonusValuePercentage / 100;
            int equipmentEffectId = (int)__instance.GetEquipmentEffectId();
            bool flag = equipmentEffectId >= 0;
            if (flag)
            {
                EquipmentEffectItem equipmentEffectItem = EquipmentEffect.Instance[equipmentEffectId];
                num += num * (int)equipmentEffectItem.EquipmentAttackChange / 100;
            }
            bool flag2 = ModificationStateHelper.IsActive(__instance.GetModificationState(), 2);
            if (flag2)
            {
                int armorPropertyBonus = DomainManager.Item.GetRefinedEffects(__instance.GetItemKey()).GetArmorPropertyBonus(ERefiningEffectArmorType.EquipmentAttack);
                num += armorPropertyBonus * 10;
            }
            return (short)num;
        }

        // Token: 0x0600001D RID: 29 RVA: 0x00003FB8 File Offset: 0x000021B8
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameData.Domains.Item.Armor), "CalcEquipmentDefense")]
        private static short CalcArmorEquipmentDefense_Postfix(short __result, GameData.Domains.Item.Armor __instance)
        {
            if (!_enableMod) return __result;

            int materialResourceBonusValuePercentage = ItemTemplateHelper.GetMaterialResourceBonusValuePercentage(__instance.GetItemType(), __instance.GetTemplateId(), 1, __instance.GetMaterialResources());
            int num = (int)__instance.GetBaseEquipmentDefense() * materialResourceBonusValuePercentage / 100;
            int equipmentEffectId = (int)__instance.GetEquipmentEffectId();
            bool flag = equipmentEffectId >= 0;
            if (flag)
            {
                EquipmentEffectItem equipmentEffectItem = EquipmentEffect.Instance[equipmentEffectId];
                num += num * (int)equipmentEffectItem.EquipmentDefenseChange / 100;
            }
            bool flag2 = ModificationStateHelper.IsActive(__instance.GetModificationState(), 2);
            if (flag2)
            {
                int armorPropertyBonus = DomainManager.Item.GetRefinedEffects(__instance.GetItemKey()).GetArmorPropertyBonus(ERefiningEffectArmorType.EquipmentDefense);
                num += armorPropertyBonus * 10;
            }
            return (short)num;
        }

        /// <summary>
        /// 计算攻击
        /// </summary>
        /// <param name="__result"></param>
        /// <param name="__instance"></param>
        /// <returns></returns>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameData.Domains.Item.Weapon), "CalcEquipmentAttack")]
        private static short CalcWeaponEquipmentAttack_Postfix(short __result, GameData.Domains.Item.Weapon __instance)
        {
            if (!_enableMod) return __result;

            int materialResourceBonusValuePercentage = ItemTemplateHelper.GetMaterialResourceBonusValuePercentage(__instance.GetItemType(), __instance.GetTemplateId(), 0, __instance.GetMaterialResources());
            int num = (int)__instance.GetBaseEquipmentAttack() * materialResourceBonusValuePercentage / 100;
            int equipmentEffectId = (int)__instance.GetEquipmentEffectId();
            bool flag = equipmentEffectId >= 0;
            if (flag)
            {
                EquipmentEffectItem equipmentEffectItem = EquipmentEffect.Instance[equipmentEffectId];
                num += num * (int)equipmentEffectItem.EquipmentAttackChange / 100;
            }
            bool flag2 = ModificationStateHelper.IsActive(__instance.GetModificationState(), 2);
            if (flag2)
            {
                int weaponPropertyBonus = DomainManager.Item.GetRefinedEffects(__instance.GetItemKey()).GetWeaponPropertyBonus(ERefiningEffectWeaponType.EquipmentAttack);
                num += weaponPropertyBonus * 10;
            }
            return (short)num;
        }

        /// <summary>
        /// 计算防御
        /// </summary>
        /// <param name="__result"></param>
        /// <param name="__instance"></param>
        /// <returns></returns>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameData.Domains.Item.Weapon), "CalcEquipmentDefense")]
        private static short CalcWeaponEquipmentDefense_Postfix(short __result, GameData.Domains.Item.Weapon __instance)
        {
            if (!_enableMod) return __result;

            int materialResourceBonusValuePercentage = ItemTemplateHelper.GetMaterialResourceBonusValuePercentage(__instance.GetItemType(), __instance.GetTemplateId(), 1, __instance.GetMaterialResources());
            int num = (int)__instance.GetBaseEquipmentDefense() * materialResourceBonusValuePercentage / 100;
            int equipmentEffectId = (int)__instance.GetEquipmentEffectId();
            if (equipmentEffectId >= 0)
            {
                EquipmentEffectItem equipmentEffectItem = EquipmentEffect.Instance[equipmentEffectId];
                num += num * (int)equipmentEffectItem.EquipmentDefenseChange / 100;
            }
            if (ModificationStateHelper.IsActive(__instance.GetModificationState(), 2))
            {
                int weaponPropertyBonus = DomainManager.Item.GetRefinedEffects(__instance.GetItemKey()).GetWeaponPropertyBonus(ERefiningEffectWeaponType.EquipmentDefense);
                num += weaponPropertyBonus * 10;
            }
            return (short)num;
        }
    }
}
